namespace SEToolbox.Interop.Asteroids
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media.Media3D;

    using SEToolbox.Support;
    using VRageMath;

    public static class MyVoxelRayTracer
    {
        private static readonly object Locker = new object();
        private enum MeshFace : byte { Undefined, Nearside, Farside };

        #region ReadModelAsteroidVolmetic

        // For a 1024 cubed asteroid, it takes approximately 6.5Gb of system memory.

        public static MyVoxelMap ReadModelAsteroidVolmetic(Model3DGroup model, IList<MyMeshModel> mappedMesh, ScaleTransform3D scale, Transform3D rotateTransform, TraceType traceType, TraceCount traceCount, TraceDirection traceDirection,
            Action<double, double> resetProgress, Action incrementProgress, Func<bool> checkCancel, Action complete)
        {
            var traceDirectionCount = 0;
            var materials = new List<byte>();
            var faceMaterials = new List<byte>();
            foreach (var mesh in mappedMesh)
            {
                materials.Add(mesh.MaterialIndex ?? SpaceEngineersConsts.EmptyVoxelMaterial);
                faceMaterials.Add(mesh.FaceMaterialIndex ?? SpaceEngineersConsts.EmptyVoxelMaterial);
            }

            // How far to check in from the proposed Volumetric edge.
            // This number is just made up, but small enough that it still represents the corner edge of the Volumetric space.
            // But still large enough that it isn't the exact corner.
            const double offset = 0.0000045f;

            if (scale.ScaleX > 0 && scale.ScaleY > 0 && scale.ScaleZ > 0 && scale.ScaleX != 1.0f && scale.ScaleY != 1.0f && scale.ScaleZ != 1.0f)
            {
                model.TransformScale(scale.ScaleX, scale.ScaleY, scale.ScaleZ);
            }

            // Attempt to offset the model, so it's only caulated from zero (0) and up, instead of using zero (0) as origin.
            //model.Transform = new TranslateTransform3D(-model.Bounds.X, -model.Bounds.Y, -model.Bounds.Z);

            var tbounds = model.Bounds;
            Matrix3D? rotate = null;
            if (rotateTransform != null)
            {
                rotate = rotateTransform.Value;
                tbounds = rotateTransform.TransformBounds(tbounds);
            }

            //model.Transform = new TranslateTransform3D(-tbounds.X, -tbounds.Y, -tbounds.Z);

            // Add 2 to either side, to allow for material padding to expose internal materials.
            const int bufferSize = 2;
            var xMin = (int)Math.Floor(tbounds.X) - bufferSize;
            var yMin = (int)Math.Floor(tbounds.Y) - bufferSize;
            var zMin = (int)Math.Floor(tbounds.Z) - bufferSize;

            var xMax = (int)Math.Ceiling(tbounds.X + tbounds.SizeX) + bufferSize;
            var yMax = (int)Math.Ceiling(tbounds.Y + tbounds.SizeY) + bufferSize;
            var zMax = (int)Math.Ceiling(tbounds.Z + tbounds.SizeZ) + bufferSize;

            // Do not rounnd up the array size, as this really isn't required, and it increases the calculation time.
            var xCount = (xMax - xMin);
            var yCount = (yMax - yMin);
            var zCount = (zMax - zMin);

            Debug.WriteLine("Approximate Size: {0}x{1}x{2}", Math.Ceiling(tbounds.X + tbounds.SizeX) - Math.Floor(tbounds.X), Math.Ceiling(tbounds.Y + tbounds.SizeY) - Math.Floor(tbounds.Y), Math.Ceiling(tbounds.Z + tbounds.SizeZ) - Math.Floor(tbounds.Z));
            Debug.WriteLine("Bounds Size: {0}x{1}x{2}", xCount, yCount, zCount);

            var finalCubic = ArrayHelper.Create<byte>(xCount, yCount, zCount);
            var finalMater = ArrayHelper.Create<byte>(xCount, yCount, zCount);

            if (resetProgress != null)
            {
                long triangles = (from GeometryModel3D gm in model.Children select gm.Geometry as MeshGeometry3D).Aggregate<MeshGeometry3D, long>(0, (current, g) => current + (g.TriangleIndices.Count / 3));
                long rays = 0;

                if ((traceDirection & TraceDirection.X) == TraceDirection.X)
                    rays += ((yMax - yMin) * (zMax - zMin));
                if ((traceDirection & TraceDirection.Y) == TraceDirection.Y)
                    rays += ((xMax - xMin) * (zMax - zMin));
                if ((traceDirection & TraceDirection.Z) == TraceDirection.Z)
                    rays += ((xMax - xMin) * (yMax - yMin));

                resetProgress.Invoke(0, rays * triangles);
            }

            if (checkCancel != null && checkCancel.Invoke())
            {
                complete?.Invoke();
                return null;
            }

            #region basic ray trace of every individual triangle.

            // Start from the last mesh, which represents the bottom of the UI stack, and overlay each other mesh on top of it.
            for (var modelIdx = mappedMesh.Count - 1; modelIdx >= 0; modelIdx--)
            {
                Debug.WriteLine("Model {0}", modelIdx);

                var modelCubic = new byte[xCount][][];
                var modelMater = new byte[xCount][][];

                for (var x = 0; x < xCount; x++)
                {
                    modelCubic[x] = new byte[yCount][];
                    modelMater[x] = new byte[yCount][];
                    for (var y = 0; y < yCount; y++)
                    {
                        modelCubic[x][y] = new byte[zCount];
                        modelMater[x][y] = new byte[zCount];
                    }
                }

                var meshes = mappedMesh[modelIdx];
                var threadCounter = 0;

                var geometries = new GeometeryDetail[meshes.Geometery.Length];
                for (var i = 0; i < meshes.Geometery.Length; i++)
                    geometries[i] = new GeometeryDetail(meshes.Geometery[i]);

                var startOffset = 0.5f;
                var endOffset = 0.5f;
                var volumeOffset = 0.5f;
                Func<double, int> roundFunc = null;
                if (traceType == TraceType.Odd)
                {
                    startOffset = 0.5f;
                    endOffset = 0.5f;
                    volumeOffset = 0.5f;
                    roundFunc = delegate (double d) { return (int)Math.Round(d, 0); };
                }
                else if (traceType == TraceType.Even)
                {
                    startOffset = 0.0f;
                    endOffset = 1.0f;
                    volumeOffset = 1.0f;
                    roundFunc = delegate (double d) { return (int)Math.Floor(d); };
                }

                #region X ray trace

                if ((traceDirection & TraceDirection.X) == TraceDirection.X)
                {
                    Debug.WriteLine("X Rays");
                    traceDirectionCount++;
                    threadCounter = (yMax - yMin) * (zMax - zMin);

                    for (var y = yMin; y < yMax; y++)
                    {
                        for (var z = zMin; z < zMax; z++)
                        {
                            if (checkCancel != null && checkCancel.Invoke())
                            {
                                complete?.Invoke();
                                return null;
                            }

                            List<Point3D[]> testRays = null;
                            if (traceType == TraceType.Odd)
                                testRays = new List<Point3D[]>
                            {
                                new [] {new Point3D(xMin, y + offset, z + offset), new Point3D(xMax, y + offset, z + offset)},
                                new [] {new Point3D(xMin, y -0.5f + offset, z -0.5f + offset), new Point3D(xMax, y -0.5f + offset, z -0.5f + offset)},
                                new [] {new Point3D(xMin, y + 0.5f - offset, z -0.5f + offset), new Point3D(xMax, y + 0.5f - offset, z -0.5f + offset)},
                                new [] {new Point3D(xMin, y -0.5f + offset, z + 0.5f - offset), new Point3D(xMax, y -0.5f + offset, z + 0.5f - offset)},
                                new [] {new Point3D(xMin, y + 0.5f - offset, z + 0.5f - offset), new Point3D(xMax, y + 0.5f - offset, z + 0.5f - offset)}
                            };
                            else if (traceType == TraceType.Even)
                                testRays = new List<Point3D[]>
                            {
                                new [] {new Point3D(xMin, y + 0.5f - offset, z + 0.5f - offset), new Point3D(xMax, y + 0.5f - offset, z + 0.5f - offset)},
                                new [] {new Point3D(xMin, y + offset, z + offset), new Point3D(xMax, y + offset, z + offset)},
                                new [] {new Point3D(xMin, y + 1.0f - offset, z + offset), new Point3D(xMax, y + 1.0f - offset, z + offset)},
                                new [] {new Point3D(xMin, y + offset, z + 1.0f - offset), new Point3D(xMax, y + offset, z + 1.0f - offset)},
                                new [] {new Point3D(xMin, y + 1.0f - offset, z + 1.0f - offset), new Point3D(xMax, y + 1.0f - offset, z + 1.0f - offset)}
                            };

                            var task = new Task(obj =>
                            {
                                var bgw = (RayTracerTaskWorker)obj;
                                var tracers = new List<Trace>();

                                foreach (var geometery in geometries)
                                {
                                    for (var t = 0; t < geometery.Triangles.Length; t += 3)
                                    {
                                        if (checkCancel != null && checkCancel.Invoke())
                                            return;

                                        if (incrementProgress != null)
                                        {
                                            lock (Locker)
                                            {
                                                incrementProgress.Invoke();
                                            }
                                        }

                                        var p1 = geometery.Positions[geometery.Triangles[t]];
                                        var p2 = geometery.Positions[geometery.Triangles[t + 1]];
                                        var p3 = geometery.Positions[geometery.Triangles[t + 2]];

                                        if (rotate.HasValue)
                                        {
                                            p1 = rotate.Value.Transform(p1);
                                            p2 = rotate.Value.Transform(p2);
                                            p3 = rotate.Value.Transform(p3);
                                        }

                                        foreach (var ray in testRays)
                                        {
                                            if ((p1.Y < ray[0].Y && p2.Y < ray[0].Y && p3.Y < ray[0].Y) ||
                                                (p1.Y > ray[0].Y && p2.Y > ray[0].Y && p3.Y > ray[0].Y) ||
                                                (p1.Z < ray[0].Z && p2.Z < ray[0].Z && p3.Z < ray[0].Z) ||
                                                (p1.Z > ray[0].Z && p2.Z > ray[0].Z && p3.Z > ray[0].Z))
                                                continue;

                                            Point3D intersect;
                                            int normal;
                                            if (MeshHelper.RayIntersetTriangleRound(p1, p2, p3, ray[0], ray[1], out intersect, out normal))
                                            {
                                                tracers.Add(new Trace(intersect, normal));
                                            }
                                        }
                                    }
                                }

                                if (tracers.Count > 1)
                                {
                                    var order = tracers.GroupBy(t => new { t.Point, t.Face }).Select(g => g.First()).OrderBy(k => k.Point.X).ToArray();
                                    var startCoord = roundFunc(order[0].Point.X);
                                    var endCoord = roundFunc(order[order.Length - 1].Point.X);
                                    var surfaces = 0;

                                    for (var x = startCoord; x <= endCoord; x++)
                                    {
                                        var points = order.Where(p => p.Point.X > x - startOffset && p.Point.X < x + endOffset).ToArray();
                                        var volume = (byte)(0xff / testRays.Count * surfaces);

                                        foreach (var point in points)
                                        {
                                            if (point.Face == MeshFace.Farside)
                                            {
                                                volume += (byte)(Math.Round(Math.Abs(x + volumeOffset - point.Point.X) * 255 / testRays.Count, 0));
                                                surfaces++;
                                            }
                                            else if (point.Face == MeshFace.Nearside)
                                            {
                                                volume -= (byte)(Math.Round(Math.Abs(x + volumeOffset - point.Point.X) * 255 / testRays.Count, 0));
                                                surfaces--;
                                            }
                                        }

                                        // TODO: retest detailed model export.
                                        //volume = volume.RoundUpToNearest(8);

                                        modelCubic[x - xMin][bgw.Y - yMin][bgw.Z - zMin] = volume;
                                        modelMater[x - xMin][bgw.Y - yMin][bgw.Z - zMin] = materials[bgw.ModelIdx];
                                    }

                                    if (faceMaterials[bgw.ModelIdx] != 0xff)
                                    {
                                        for (var i = 1; i < 6; i++)
                                        {
                                            if (xMin < startCoord - i && modelCubic[startCoord - i - xMin][bgw.Y - yMin][bgw.Z - zMin] == 0)
                                            {
                                                modelMater[startCoord - i - xMin][bgw.Y - yMin][bgw.Z - zMin] = faceMaterials[bgw.ModelIdx];
                                            }
                                            if (endCoord + i < xMax && modelCubic[endCoord + i - xMin][bgw.Y - yMin][bgw.Z - zMin] == 0)
                                            {
                                                modelMater[endCoord + i - xMin][bgw.Y - yMin][bgw.Z - zMin] = faceMaterials[bgw.ModelIdx];
                                            }
                                        }
                                    }
                                }

                                lock (Locker)
                                {
                                    threadCounter--;
                                }
                            }, new RayTracerTaskWorker(modelIdx, 0, y, z));

                            task.Start();
                        }
                    }

                    // Wait for Multithread parts to finish.
                    while (threadCounter > 0)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        if (checkCancel != null && checkCancel.Invoke())
                            break;
                    }

                    GC.Collect();
                }

                #endregion

                #region Y rays trace

                if ((traceDirection & TraceDirection.Y) == TraceDirection.Y)
                {
                    Debug.WriteLine("Y Rays");
                    traceDirectionCount++;
                    threadCounter = (xMax - xMin) * (zMax - zMin);

                    for (var x = xMin; x < xMax; x++)
                    {
                        for (var z = zMin; z < zMax; z++)
                        {
                            if (checkCancel != null && checkCancel.Invoke())
                            {
                                if (complete != null)
                                    complete.Invoke();
                                return null;
                            }

                            List<Point3D[]> testRays = null;
                            if (traceType == TraceType.Odd)
                                testRays = new List<Point3D[]>
                            {
                                new [] {new Point3D(x + offset, yMin, z + offset), new Point3D(x + offset, yMax, z + offset)},
                                new [] {new Point3D(x -0.5f + offset, yMin, z -0.5f + offset), new Point3D(x -0.5f + offset, yMax, z -0.5f + offset)},
                                new [] {new Point3D(x + 0.5f - offset, yMin, z -0.5f + offset), new Point3D(x + 0.5f - offset, yMax, z -0.5f + offset)},
                                new [] {new Point3D(x -0.5f + offset, yMin, z + 0.5f - offset), new Point3D(x -0.5f + offset, yMax, z + 0.5f - offset)},
                                new [] {new Point3D(x + 0.5f - offset, yMin, z + 0.5f - offset), new Point3D(x + 0.5f - offset, yMax, z + 0.5f - offset)}
                            };
                            else if (traceType == TraceType.Even)
                                testRays = new List<Point3D[]>
                            {
                                new [] {new Point3D(x + 0.5f - offset, yMin, z + 0.5f - offset), new Point3D(x + 0.5f - offset, yMax, z + 0.5f - offset)},
                                new [] {new Point3D(x + offset, yMin, z + offset), new Point3D(x + offset, yMax, z + offset)},
                                new [] {new Point3D(x + 1.0f - offset, yMin, z + offset), new Point3D(x + 1.0f - offset, yMax, z + offset)},
                                new [] {new Point3D(x + offset, yMin, z + 1.0f - offset), new Point3D(x + offset, yMax, z + 1.0f - offset)},
                                new [] {new Point3D(x + 1.0f - offset, yMin, z + 1.0f - offset), new Point3D(x + 1.0f - offset, yMax, z + 1.0f - offset)}
                            };

                            var task = new Task(obj =>
                            {
                                var bgw = (RayTracerTaskWorker)obj;
                                var tracers = new List<Trace>();

                                foreach (var geometery in geometries)
                                {
                                    for (var t = 0; t < geometery.Triangles.Length; t += 3)
                                    {
                                        if (checkCancel != null && checkCancel.Invoke())
                                            return;

                                        if (incrementProgress != null)
                                        {
                                            lock (Locker)
                                            {
                                                incrementProgress.Invoke();
                                            }
                                        }

                                        var p1 = geometery.Positions[geometery.Triangles[t]];
                                        var p2 = geometery.Positions[geometery.Triangles[t + 1]];
                                        var p3 = geometery.Positions[geometery.Triangles[t + 2]];

                                        if (rotate.HasValue)
                                        {
                                            p1 = rotate.Value.Transform(p1);
                                            p2 = rotate.Value.Transform(p2);
                                            p3 = rotate.Value.Transform(p3);
                                        }

                                        foreach (var ray in testRays)
                                        {
                                            if ((p1.X < ray[0].X && p2.X < ray[0].X && p3.X < ray[0].X) ||
                                                (p1.X > ray[0].X && p2.X > ray[0].X && p3.X > ray[0].X) ||
                                                (p1.Z < ray[0].Z && p2.Z < ray[0].Z && p3.Z < ray[0].Z) ||
                                                (p1.Z > ray[0].Z && p2.Z > ray[0].Z && p3.Z > ray[0].Z))
                                                continue;

                                            Point3D intersect;
                                            int normal;
                                            if (MeshHelper.RayIntersetTriangleRound(p1, p2, p3, ray[0], ray[1], out intersect, out normal))
                                            {
                                                tracers.Add(new Trace(intersect, normal));
                                            }
                                        }
                                    }
                                }

                                if (tracers.Count > 1)
                                {
                                    var order = tracers.GroupBy(t => new { t.Point, t.Face }).Select(g => g.First()).OrderBy(k => k.Point.Y).ToArray();
                                    var startCoord = roundFunc(order[0].Point.Y);
                                    var endCoord = roundFunc(order[order.Length - 1].Point.Y);
                                    var surfaces = 0;

                                    for (var y = startCoord; y <= endCoord; y++)
                                    {
                                        var points = order.Where(p => p.Point.Y > y - startOffset && p.Point.Y < y + endOffset).ToArray();
                                        var volume = (byte)(0xff / testRays.Count * surfaces);

                                        foreach (var point in points)
                                        {
                                            if (point.Face == MeshFace.Farside)
                                            {
                                                volume += (byte)(Math.Round(Math.Abs(y + volumeOffset - point.Point.Y) * 255 / testRays.Count, 0));
                                                surfaces++;
                                            }
                                            else if (point.Face == MeshFace.Nearside)
                                            {
                                                volume -= (byte)(Math.Round(Math.Abs(y + volumeOffset - point.Point.Y) * 255 / testRays.Count, 0));
                                                surfaces--;
                                            }
                                        }

                                        if (traceDirectionCount > 1)
                                        {
                                            var prevolumme = modelCubic[bgw.X - xMin][y - yMin][bgw.Z - zMin];
                                            if (prevolumme != 0)
                                            {
                                                // average with the pre-existing X volume.
                                                volume = (byte)Math.Round(((float)prevolumme + (float)volume) / (float)traceDirectionCount, 0);
                                            }
                                        }

                                        //volume = volume.RoundUpToNearest(8);

                                        modelCubic[bgw.X - xMin][y - yMin][bgw.Z - zMin] = volume;
                                        modelMater[bgw.X - xMin][y - yMin][bgw.Z - zMin] = materials[bgw.ModelIdx];
                                    }

                                    if (faceMaterials[bgw.ModelIdx] != 0xff)
                                    {
                                        for (var i = 1; i < 6; i++)
                                        {
                                            if (yMin < startCoord - i && modelCubic[bgw.X - xMin][startCoord - i - yMin][bgw.Z - zMin] == 0)
                                            {
                                                modelMater[bgw.X - xMin][startCoord - i - yMin][bgw.Z - zMin] = faceMaterials[bgw.ModelIdx];
                                            }
                                            if (endCoord + i < yMax && modelCubic[bgw.X - xMin][endCoord + i - yMin][bgw.Z - zMin] == 0)
                                            {
                                                modelMater[bgw.X - xMin][endCoord + i - yMin][bgw.Z - zMin] = faceMaterials[bgw.ModelIdx];
                                            }
                                        }
                                    }
                                }

                                lock (Locker)
                                {
                                    threadCounter--;
                                }
                            }, new RayTracerTaskWorker(modelIdx, x, 0, z));

                            task.Start();
                        }
                    }

                    // Wait for Multithread parts to finish.
                    while (threadCounter > 0)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        if (checkCancel != null && checkCancel.Invoke())
                            break;
                    }

                    GC.Collect();
                }

                #endregion

                #region Z ray trace

                if ((traceDirection & TraceDirection.Z) == TraceDirection.Z)
                {
                    Debug.WriteLine("Z Rays");
                    traceDirectionCount++;
                    threadCounter = (xMax - xMin) * (yMax - yMin);

                    for (var x = xMin; x < xMax; x++)
                    {
                        for (var y = yMin; y < yMax; y++)
                        {
                            if (checkCancel != null && checkCancel.Invoke())
                            {
                                if (complete != null)
                                    complete.Invoke();
                                return null;
                            }

                            List<Point3D[]> testRays = null;
                            if (traceType == TraceType.Odd)
                                testRays = new List<Point3D[]>
                            {
                                new [] {new Point3D(x + offset, y + offset, zMin), new Point3D(x + offset, y + offset, zMax)},
                                new [] {new Point3D(x -0.5f + offset, y -0.5f + offset, zMin), new Point3D(x -0.5f + offset, y -0.5f + offset, zMax)},
                                new [] {new Point3D(x + 0.5f - offset, y -0.5f + offset, zMin), new Point3D(x + 0.5f - offset, y -0.5f + offset, zMax)},
                                new [] {new Point3D(x -0.5f + offset, y + 0.5f - offset, zMin), new Point3D(x -0.5f + offset, y + 0.5f - offset, zMax)},
                                new [] {new Point3D(x + 0.5f - offset, y + 0.5f - offset, zMin), new Point3D(x + 0.5f - offset, y + 0.5f - offset, zMax)}
                            };
                            else if (traceType == TraceType.Even)
                                testRays = new List<Point3D[]>
                            {
                                new [] {new Point3D(x + 0.5f - offset, y + 0.5f - offset, zMin), new Point3D(x + 0.5f - offset, y + 0.5f - offset, zMax)},
                                new [] {new Point3D(x + offset, y + offset, zMin), new Point3D(x + offset, y + offset, zMax)},
                                new [] {new Point3D(x + 1.0f - offset, y + offset, zMin), new Point3D(x + 1.0f - offset, y + offset, zMax)},
                                new [] {new Point3D(x + offset, y + 1.0f - offset, zMin), new Point3D(x + offset, y + 1.0f - offset, zMax)},
                                new [] {new Point3D(x + 1.0f - offset, y + 1.0f - offset, zMin), new Point3D(x + 1.0f - offset, y + 1.0f - offset, zMax)}
                            };

                            var task = new Task(obj =>
                            {
                                var bgw = (RayTracerTaskWorker)obj;
                                var tracers = new List<Trace>();

                                foreach (var geometery in geometries)
                                {
                                    for (var t = 0; t < geometery.Triangles.Length; t += 3)
                                    {
                                        if (checkCancel != null && checkCancel.Invoke())
                                            return;

                                        if (incrementProgress != null)
                                        {
                                            lock (Locker)
                                            {
                                                incrementProgress.Invoke();
                                            }
                                        }

                                        var p1 = geometery.Positions[geometery.Triangles[t]];
                                        var p2 = geometery.Positions[geometery.Triangles[t + 1]];
                                        var p3 = geometery.Positions[geometery.Triangles[t + 2]];

                                        if (rotate.HasValue)
                                        {
                                            p1 = rotate.Value.Transform(p1);
                                            p2 = rotate.Value.Transform(p2);
                                            p3 = rotate.Value.Transform(p3);
                                        }

                                        foreach (var ray in testRays)
                                        {
                                            if ((p1.X < ray[0].X && p2.X < ray[0].X && p3.X < ray[0].X) ||
                                                (p1.X > ray[0].X && p2.X > ray[0].X && p3.X > ray[0].X) ||
                                                (p1.Y < ray[0].Y && p2.Y < ray[0].Y && p3.Y < ray[0].Y) ||
                                                (p1.Y > ray[0].Y && p2.Y > ray[0].Y && p3.Y > ray[0].Y))
                                                continue;

                                            Point3D intersect;
                                            int normal;
                                            if (MeshHelper.RayIntersetTriangleRound(p1, p2, p3, ray[0], ray[1], out intersect, out normal))
                                            {
                                                tracers.Add(new Trace(intersect, normal));
                                            }
                                        }
                                    }
                                }

                                if (tracers.Count > 1)
                                {
                                    var order = tracers.GroupBy(t => new { t.Point, t.Face }).Select(g => g.First()).OrderBy(k => k.Point.Z).ToArray();
                                    var startCoord = roundFunc(order[0].Point.Z);
                                    var endCoord = roundFunc(order[order.Length - 1].Point.Z);
                                    var surfaces = 0;

                                    for (var z = startCoord; z <= endCoord; z++)
                                    {
                                        var points = order.Where(p => p.Point.Z > z - startOffset && p.Point.Z < z + endOffset).ToArray();
                                        var volume = (byte)(0xff / testRays.Count * surfaces);

                                        foreach (var point in points)
                                        {
                                            if (point.Face == MeshFace.Farside)
                                            {
                                                volume += (byte)(Math.Round(Math.Abs(z + volumeOffset - point.Point.Z) * 255 / testRays.Count, 0));
                                                surfaces++;
                                            }
                                            else if (point.Face == MeshFace.Nearside)
                                            {
                                                volume -= (byte)(Math.Round(Math.Abs(z + volumeOffset - point.Point.Z) * 255 / testRays.Count, 0));
                                                surfaces--;
                                            }
                                        }

                                        if (traceDirectionCount > 1)
                                        {
                                            var prevolumme = modelCubic[bgw.X - xMin][bgw.Y - yMin][z - zMin];
                                            if (prevolumme != 0)
                                            {
                                                // average with the pre-existing X and Y volumes.
                                                volume = (byte)Math.Round((((float)prevolumme * (traceDirectionCount - 1)) + (float)volume) / (float)traceDirectionCount, 0);
                                            }
                                        }

                                        //volume = volume.RoundUpToNearest(8);

                                        modelCubic[bgw.X - xMin][bgw.Y - yMin][z - zMin] = volume;
                                        modelMater[bgw.X - xMin][bgw.Y - yMin][z - zMin] = materials[bgw.ModelIdx];
                                    }

                                    if (faceMaterials[bgw.ModelIdx] != 0xff)
                                    {
                                        for (var i = 1; i < 6; i++)
                                        {
                                            if (zMin < startCoord - i && modelCubic[bgw.X - xMin][bgw.Y - yMin][startCoord - i - zMin] == 0)
                                            {
                                                modelMater[bgw.X - xMin][bgw.Y - yMin][startCoord - i - zMin] = faceMaterials[bgw.ModelIdx];
                                            }
                                            if (endCoord + i < zMax && modelCubic[bgw.X - xMin][bgw.Y - yMin][endCoord + i - zMin] == 0)
                                            {
                                                modelMater[bgw.X - xMin][bgw.Y - yMin][endCoord + i - zMin] = faceMaterials[bgw.ModelIdx];
                                            }
                                        }
                                    }
                                }

                                lock (Locker)
                                {
                                    threadCounter--;
                                }
                            }, new RayTracerTaskWorker(modelIdx, x, y, 0));

                            task.Start();
                        }
                    }

                    // Wait for Multithread parts to finish.
                    while (threadCounter > 0)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        if (checkCancel != null && checkCancel.Invoke())
                            break;
                    }

                    GC.Collect();
                }

                #endregion

                if (checkCancel != null && checkCancel.Invoke())
                {
                    complete?.Invoke();
                    return null;
                }

                #region merge individual model results into final

                for (var x = 0; x < xCount; x++)
                {
                    for (var y = 0; y < yCount; y++)
                    {
                        for (var z = 0; z < zCount; z++)
                        {
                            if (modelMater[x][y][z] == 0xff && modelCubic[x][y][z] != 0)
                            {
                                finalCubic[x][y][z] = (byte)Math.Max(finalCubic[x][y][z] - modelCubic[x][y][z], 0);
                            }
                            else if (modelCubic[x][y][z] != 0)
                            {
                                finalCubic[x][y][z] = Math.Max(finalCubic[x][y][z], modelCubic[x][y][z]);
                                finalMater[x][y][z] = modelMater[x][y][z];
                            }
                            else if (finalCubic[x][y][z] == 0 && finalMater[x][y][z] == 0 && modelMater[x][y][z] != 0xff)
                            {
                                finalMater[x][y][z] = modelMater[x][y][z];
                            }
                        }
                    }
                }

                #endregion

            } // end models

            #endregion

            if (checkCancel != null && checkCancel.Invoke())
            {
                complete?.Invoke();
                return null;
            }

            var size = new Vector3I(xCount, yCount, zCount);
            // TODO: at the moment the Mesh list is not complete, so the faceMaterial setting is kind of vague.
            var defaultMaterial = mappedMesh[0].MaterialIndex; // Use the FaceMaterial from the first Mesh in the object list.
            var faceMaterial = mappedMesh[0].FaceMaterialIndex; // Use the FaceMaterial from the first Mesh in the object list.

            var action = (Action<MyVoxelBuilderArgs>)delegate (MyVoxelBuilderArgs e)
            {
                // center the finalCubic structure within the voxel Volume.
                Vector3I pointOffset = (e.Size - size) / 2;

                // the model is only shaped according to its volume, not the voxel which is cubic.
                if (e.CoordinatePoint.X >= pointOffset.X && e.CoordinatePoint.X < pointOffset.X + xCount &&
                    e.CoordinatePoint.Y >= pointOffset.Y && e.CoordinatePoint.Y < pointOffset.Y + yCount &&
                    e.CoordinatePoint.Z >= pointOffset.Z && e.CoordinatePoint.Z < pointOffset.Z + zCount)
                {
                    e.Volume = finalCubic[e.CoordinatePoint.X - pointOffset.X][e.CoordinatePoint.Y - pointOffset.Y][e.CoordinatePoint.Z - pointOffset.Z];
                    e.MaterialIndex = finalMater[e.CoordinatePoint.X - pointOffset.X][e.CoordinatePoint.Y - pointOffset.Y][e.CoordinatePoint.Z - pointOffset.Z];
                }
            };

            var voxelMap = MyVoxelBuilder.BuildAsteroid(true, size, defaultMaterial.Value, faceMaterial, action);

            complete?.Invoke();

            return voxelMap;
        }

        #endregion

        #region support classes

        public class MyMeshModel
        {
            public MyMeshModel(MeshGeometry3D[] geometery, byte? materialIndex, byte? faceMaterialIndex)
            {
                Geometery = geometery;
                MaterialIndex = materialIndex;
                FaceMaterialIndex = faceMaterialIndex;
            }

            public MeshGeometry3D[] Geometery { get; set; }
            public byte? MaterialIndex { get; set; }
            public byte? FaceMaterialIndex { get; set; }
        }

        public class GeometeryDetail
        {
            public GeometeryDetail(MeshGeometry3D meshGeometry) :
                this(meshGeometry.TriangleIndices.ToArray(), meshGeometry.Positions.ToArray())
            {
            }

            public GeometeryDetail(int[] triangles, Point3D[] positions)
            {
                Triangles = triangles;
                Positions = positions;
            }

            public int[] Triangles { get; set; }
            public Point3D[] Positions { get; set; }
        }

        private class Trace
        {
            public Trace(Point3D point, int normal)
            {
                Point = point;
                Face = MeshFace.Undefined;
                if (normal == 1)
                    Face = MeshFace.Nearside;
                else if (normal == -1)
                    Face = MeshFace.Farside;
            }

            public Point3D Point { get; }
            public MeshFace Face { get; }
        }

        private class RayTracerTaskWorker
        {
            #region ctor

            public RayTracerTaskWorker(int modelIdx, int x, int y, int z)
            {
                ModelIdx = modelIdx;
                X = x;
                Y = y;
                Z = z;
            }

            #endregion

            #region properties

            public int ModelIdx { get; }
            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            #endregion
        }

        #endregion
    }
}
