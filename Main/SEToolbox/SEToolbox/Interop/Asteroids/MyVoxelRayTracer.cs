namespace SEToolbox.Interop.Asteroids
{
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Media.Media3D;
    using VRageMath;

    public static class MyVoxelRayTracer
    {
        private enum MeshFace : byte { Undefined, Nearside, Farside };

        private class Trace
        {
            public Trace(Point3D point, MeshFace face)
            {
                this.Point = point;
                this.Face = face;
            }

            public Point3D Point { get; set; }
            public MeshFace Face { get; set; }
        }

        #region ReadModelAsteroidVolmetic

        public static MyVoxelMap ReadModelAsteroidVolmetic(bool multiThread, string modelFile, string asteroidFile, double scaleMultiplyierX, double scaleMultiplyierY, double scaleMultiplyierZ, Transform3D rotateTransform, string[] mappedMaterials, string fillerMaterial,
            Action<long, long> resetProgress, Action incrementProgress)
        {
            var materials = new List<byte>();
            foreach (var materialName in mappedMaterials)
            {
                materials.Add(SpaceEngineersAPI.GetMaterialIndex(materialName));
            }

            var model = MeshHelper.Load(modelFile, ignoreErrors: true);

            var dispatcher = model.Dispatcher;

            // How far to check in from the proposed Volumetric edge.
            // This number is just made up, but small enough that it still represents the corner edge of the Volumetric space.
            // But still large enough that it isn't the exact corner.
            const double offset = 0.0000045f;

            if (scaleMultiplyierX > 0 && scaleMultiplyierY > 0 && scaleMultiplyierZ > 0 && scaleMultiplyierX != 1.0f && scaleMultiplyierY != 1.0f && scaleMultiplyierZ != 1.0f)
            {
                model.TransformScale(scaleMultiplyierX, scaleMultiplyierY, scaleMultiplyierZ);
            }

            // Attempt to offset the model, so it's only caulated from zero (0) and up, instead of using zero (0) as origin.
            //model.Transform = new TranslateTransform3D(-model.Bounds.X, -model.Bounds.Y, -model.Bounds.Z);

            var tbounds = model.Bounds;
            if (rotateTransform != null)
                tbounds = rotateTransform.TransformBounds(tbounds);

            //model.Transform = new TranslateTransform3D(-tbounds.X, -tbounds.Y, -tbounds.Z);

            // Add 2 to either side, to allow for material padding to expose internal materials.
            var xMin = (int)Math.Floor(tbounds.X) - 2;
            var yMin = (int)Math.Floor(tbounds.Y) - 2;
            var zMin = (int)Math.Floor(tbounds.Z) - 2;

            var xMax = (int)Math.Ceiling(tbounds.X + tbounds.SizeX) + 2;
            var yMax = (int)Math.Ceiling(tbounds.Y + tbounds.SizeY) + 2;
            var zMax = (int)Math.Ceiling(tbounds.Z + tbounds.SizeZ) + 2;

            var xCount = MyVoxelBuilder.ScaleMod(xMax - xMin, 64);
            var yCount = MyVoxelBuilder.ScaleMod(yMax - yMin, 64);
            var zCount = MyVoxelBuilder.ScaleMod(zMax - zMin, 64);

            Debug.WriteLine("Size: {0}x{1}x{2}", xCount, yCount, zCount);

            var finalCubic = new byte[xCount][][];
            var finalMater = new byte[xCount][][];

            for (var x = 0; x < xCount; x++)
            {
                finalCubic[x] = new byte[yCount][];
                finalMater[x] = new byte[yCount][];
                for (var y = 0; y < yCount; y++)
                {
                    finalCubic[x][y] = new byte[zCount];
                    finalMater[x][y] = new byte[zCount];
                }
            }

            var modelIdx = 0;

            if (resetProgress != null)
            {
                long triangles = (from GeometryModel3D gm in model.Children select gm.Geometry as MeshGeometry3D).Aggregate<MeshGeometry3D, long>(0, (current, g) => current + (g.TriangleIndices.Count / 3));
                long rays = ((yMax - yMin) * (zMax - zMin)) + ((xMax - xMin) * (zMax - zMin)) + ((xMax - xMin) * (yMax - yMin));
                resetProgress.Invoke(0, rays * triangles);
            }

            #region basic ray trace of every individual triangle.

            foreach (var model3D in model.Children)
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

                var gm = (GeometryModel3D)model3D;
                var geometry = gm.Geometry as MeshGeometry3D;

                #region X ray trace

                Debug.WriteLine("X Rays");

                for (var y = yMin; y < yMax; y++)
                {
                    for (var z = zMin; z < zMax; z++)
                    {
                        var testRays = new List<Point3D[]>()
                        {
                            new [] {new Point3D(xMin, y + offset, z + offset), new Point3D(xMax, y + offset, z + offset)},
                            new [] {new Point3D(xMin, y -0.5f + offset, z -0.5f + offset), new Point3D(xMax, y -0.5f + offset, z -0.5f + offset)},
                            new [] {new Point3D(xMin, y + 0.5f - offset, z -0.5f + offset), new Point3D(xMax, y + 0.5f - offset, z -0.5f + offset)},
                            new [] {new Point3D(xMin, y -0.5f + offset, z + 0.5f - offset), new Point3D(xMax, y -0.5f + offset, z + 0.5f - offset)},
                            new [] {new Point3D(xMin, y + 0.5f - offset, z + 0.5f - offset), new Point3D(xMax, y + 0.5f - offset, z + 0.5f - offset)}
                        };

                        var triangles = geometry.TriangleIndices;
                        var positions = geometry.Positions;

                        // TODO: Multithreading
                        {
                            var tracers = new List<Trace>();

                            for (var t = 0; t < triangles.Count; t += 3)
                            {
                                if (incrementProgress != null)
                                {
                                    incrementProgress.Invoke();
                                }

                                var p1 = positions[triangles[t]];
                                var p2 = positions[triangles[t + 1]];
                                var p3 = positions[triangles[t + 2]];

                                if (rotateTransform != null)
                                {
                                    p1 = rotateTransform.Transform(p1);
                                    p2 = rotateTransform.Transform(p2);
                                    p3 = rotateTransform.Transform(p3);
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
                                        tracers.Add(new Trace(intersect, normal == 1 ? MeshFace.Nearside : MeshFace.Farside));
                                    }
                                }
                            }

                            if (tracers.Count > 1)
                            {
                                var order = tracers.GroupBy(t => new { t.Point, t.Face }).Select(g => g.First()).OrderBy(k => k.Point.X).ToArray();
                                var startCoord = (int)Math.Round(order[0].Point.X, 0);
                                var endCoord = (int)Math.Round(order[order.Length - 1].Point.X, 0);
                                int surfaces = 0;

                                for (var x = startCoord; x <= endCoord; x++)
                                {
                                    var points = order.Where(p => p.Point.X > x - 0.5 && p.Point.X < x + 0.5).ToArray();

                                    byte volume = (byte)(0xff / testRays.Count * surfaces);

                                    foreach (var point in points)
                                    {
                                        if (point.Face == MeshFace.Farside)
                                        {
                                            volume += (byte)(Math.Round(Math.Abs(x + 0.5 - point.Point.X) * 255 / testRays.Count, 0));
                                            surfaces++;
                                        }
                                        else if (point.Face == MeshFace.Nearside)
                                        {
                                            volume -= (byte)(Math.Round(Math.Abs(x + 0.5 - point.Point.X) * 255 / testRays.Count, 0));
                                            surfaces--;
                                        }
                                    }

                                    modelCubic[x - xMin][y - yMin][z - zMin] = volume;
                                    modelMater[x - xMin][y - yMin][z - zMin] = materials[modelIdx];
                                }
                            }
                        } // Multithread
                    }
                }

                // TODO: Wait for Multithread parts to finish.

                #endregion

                #region Y rays trace

                Debug.WriteLine("Y Rays");

                for (var x = xMin; x < xMax; x++)
                {
                    for (var z = zMin; z < zMax; z++)
                    {
                        var testRays = new List<Point3D[]>()
                        {
                            new [] {new Point3D(x + offset, yMin, z + offset), new Point3D(x + offset, yMax, z + offset)},
                            new [] {new Point3D(x -0.5f + offset, yMin, z -0.5f + offset), new Point3D(x -0.5f + offset, yMax, z -0.5f + offset)},
                            new [] {new Point3D(x + 0.5f - offset, yMin, z -0.5f + offset), new Point3D(x + 0.5f - offset, yMax, z -0.5f + offset)},
                            new [] {new Point3D(x -0.5f + offset, yMin, z + 0.5f - offset), new Point3D(x -0.5f + offset, yMax, z + 0.5f - offset)},
                            new [] {new Point3D(x + 0.5f - offset, yMin, z + 0.5f - offset), new Point3D(x + 0.5f - offset, yMax, z + 0.5f - offset)}
                        };

                        var triangles = geometry.TriangleIndices;
                        var positions = geometry.Positions;

                        // TODO: Multithreading
                        {
                            var tracers = new List<Trace>();

                            for (var t = 0; t < triangles.Count; t += 3)
                            {
                                if (incrementProgress != null)
                                {
                                    incrementProgress.Invoke();
                                }

                                var p1 = positions[triangles[t]];
                                var p2 = positions[triangles[t + 1]];
                                var p3 = positions[triangles[t + 2]];

                                if (rotateTransform != null)
                                {
                                    p1 = rotateTransform.Transform(p1);
                                    p2 = rotateTransform.Transform(p2);
                                    p3 = rotateTransform.Transform(p3);
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
                                        tracers.Add(new Trace(intersect, normal == 1 ? MeshFace.Nearside : MeshFace.Farside));
                                    }
                                }
                            }

                            if (tracers.Count > 1)
                            {
                                var order = tracers.GroupBy(t => new { t.Point, t.Face }).Select(g => g.First()).OrderBy(k => k.Point.Y).ToArray();
                                var startCoord = (int)Math.Round(order[0].Point.Y, 0);
                                var endCoord = (int)Math.Round(order[order.Length - 1].Point.Y, 0);
                                int surfaces = 0;

                                for (var y = startCoord; y <= endCoord; y++)
                                {
                                    var points = order.Where(p => p.Point.Y > y - 0.5 && p.Point.Y < y + 0.5).ToArray();

                                    byte volume = (byte)(0xff / testRays.Count * surfaces);

                                    foreach (var point in points)
                                    {
                                        if (point.Face == MeshFace.Farside)
                                        {
                                            volume += (byte)(Math.Round(Math.Abs(y + 0.5 - point.Point.Y) * 255 / testRays.Count, 0));
                                            surfaces++;
                                        }
                                        else if (point.Face == MeshFace.Nearside)
                                        {
                                            volume -= (byte)(Math.Round(Math.Abs(y + 0.5 - point.Point.Y) * 255 / testRays.Count, 0));
                                            surfaces--;
                                        }
                                    }

                                    var prevolumme = modelCubic[x - xMin][y - yMin][z - zMin];
                                    if (prevolumme != 0)
                                    {
                                        // average with the pre-existing X volume.
                                        volume = (byte)Math.Round(((float)prevolumme + (float)volume) / 2f, 0);
                                        //volume = Math.Max(prevolumme, volume);
                                    }

                                    modelCubic[x - xMin][y - yMin][z - zMin] = volume;
                                    modelMater[x - xMin][y - yMin][z - zMin] = materials[modelIdx];
                                }
                            }
                        } // Multithread
                    }
                }

                // TODO: Wait for Multithread parts to finish.

                #endregion

                #region Z ray trace

                Debug.WriteLine("Z Rays");

                for (var x = xMin; x < xMax; x++)
                {
                    for (var y = yMin; y < yMax; y++)
                    {
                        var testRays = new List<Point3D[]>()
                        {
                            new [] {new Point3D(x + offset, y + offset, zMin), new Point3D(x + offset, y + offset, zMax)},
                            new [] {new Point3D(x -0.5f + offset, y -0.5f + offset, zMin), new Point3D(x -0.5f + offset, y -0.5f + offset, zMax)},
                            new [] {new Point3D(x + 0.5f - offset, y -0.5f + offset, zMin), new Point3D(x + 0.5f - offset, y -0.5f + offset, zMax)},
                            new [] {new Point3D(x -0.5f + offset, y + 0.5f - offset, zMin), new Point3D(x -0.5f + offset, y + 0.5f - offset, zMax)},
                            new [] {new Point3D(x + 0.5f - offset, y + 0.5f - offset, zMin), new Point3D(x + 0.5f - offset, y + 0.5f - offset, zMax)}
                        };

                        var triangles = geometry.TriangleIndices;
                        var positions = geometry.Positions;

                        // TODO: Multithreading
                        {
                            var tracers = new List<Trace>();

                            for (var t = 0; t < triangles.Count; t += 3)
                            {
                                if (incrementProgress != null)
                                {
                                    incrementProgress.Invoke();
                                }

                                var p1 = positions[triangles[t]];
                                var p2 = positions[triangles[t + 1]];
                                var p3 = positions[triangles[t + 2]];

                                if (rotateTransform != null)
                                {
                                    p1 = rotateTransform.Transform(p1);
                                    p2 = rotateTransform.Transform(p2);
                                    p3 = rotateTransform.Transform(p3);
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
                                        tracers.Add(new Trace(intersect, normal == 1 ? MeshFace.Nearside : MeshFace.Farside));
                                    }
                                }
                            }

                            if (tracers.Count > 1)
                            {
                                var order = tracers.GroupBy(t => new { t.Point, t.Face }).Select(g => g.First()).OrderBy(k => k.Point.Z).ToArray();
                                var startCoord = (int)Math.Round(order[0].Point.Z, 0);
                                var endCoord = (int)Math.Round(order[order.Length - 1].Point.Z, 0);
                                int surfaces = 0;

                                for (var z = startCoord; z <= endCoord; z++)
                                {
                                    var points = order.Where(p => p.Point.Z > z - 0.5 && p.Point.Z < z + 0.5).ToArray();

                                    byte volume = (byte)(0xff / testRays.Count * surfaces);

                                    foreach (var point in points)
                                    {
                                        if (point.Face == MeshFace.Farside)
                                        {
                                            volume += (byte)(Math.Round(Math.Abs(z + 0.5 - point.Point.Z) * 255 / testRays.Count, 0));
                                            surfaces++;
                                        }
                                        else if (point.Face == MeshFace.Nearside)
                                        {
                                            volume -= (byte)(Math.Round(Math.Abs(z + 0.5 - point.Point.Z) * 255 / testRays.Count, 0));
                                            surfaces--;
                                        }
                                    }

                                    var prevolumme = modelCubic[x - xMin][y - yMin][z - zMin];
                                    if (prevolumme != 0)
                                    {
                                        // average with the pre-existing X and Y volumes.
                                        volume = (byte)Math.Round((((float)prevolumme * 2) + (float)volume) / 3f, 0);
                                        //volume = Math.Max(prevolumme, volume);
                                    }

                                    modelCubic[x - xMin][y - yMin][z - zMin] = volume;
                                    modelMater[x - xMin][y - yMin][z - zMin] = materials[modelIdx];
                                }
                            }
                        } // Multithread
                    }
                }

                // TODO: Wait for Multithread parts to finish.

                #endregion

                #region merge individual model results into final

                for (var x = 0; x < xCount; x++)
                {
                    for (var y = 0; y < yCount; y++)
                    {
                        for (var z = 0; z < zCount; z++)
                        {
                            if (modelCubic[x][y][z] != 0)
                            {
                                finalCubic[x][y][z] = Math.Max(finalCubic[x][y][z], modelCubic[x][y][z]);
                                finalMater[x][y][z] = modelMater[x][y][z];
                            }
                        }
                    }
                }

                #endregion

                modelIdx++;
            } // end models

            #endregion

            var size = new Vector3I(xCount, yCount, zCount);
            var faceMaterial = SpaceEngineersAPI.GetMaterialList().FirstOrDefault(m => m.IsRare == false).Name;  // Default to first non-rare material.

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                e.Volume = finalCubic[e.CoordinatePoint.X][e.CoordinatePoint.Y][e.CoordinatePoint.Z];
                if (e.Volume != 0)
                {
                    e.Material = SpaceEngineersAPI.GetMaterialName(finalMater[e.CoordinatePoint.X][e.CoordinatePoint.Y][e.CoordinatePoint.Z]);
                }
            };

            return MyVoxelBuilder.BuildAsteroid(multiThread, asteroidFile, size, fillerMaterial, faceMaterial, action);
        }

        #endregion
    }
}
