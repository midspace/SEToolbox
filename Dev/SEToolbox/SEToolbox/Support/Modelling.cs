namespace SEToolbox.Support
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using VRage.Game;
    using VRageMath;

    public static class Modelling
    {
        #region PreviewModelVolmetic

        public static Rect3D PreviewModelVolmetic(string modelFile, out Model3D model)
        {
            if (modelFile != null)
            {
                try
                {
                    model = MeshHelper.Load(modelFile, ignoreErrors: true);
                }
                catch
                {
                    model = null;
                    return Rect3D.Empty;
                }

                if (model.Bounds == Rect3D.Empty)
                {
                    model = null;
                    return Rect3D.Empty;
                }

                return model.Bounds;
            }

            model = null;
            return Rect3D.Empty;
        }

        #endregion

        #region ReadModelVolmetic

        public static CubeType[][][] ReadModelVolmetic(string modelFile, double scaleMultiplyier, Transform3D transform, ModelTraceVoxel traceType)
        {
            return ReadModelVolmetic(modelFile, scaleMultiplyier, scaleMultiplyier, scaleMultiplyier, transform, traceType, null, null);
        }

        public static CubeType[][][] ReadModelVolmetic(string modelFile, double scaleMultiplyier, Transform3D transform, ModelTraceVoxel traceType, Action<double, double> resetProgress, Action incrementProgress)
        {
            return ReadModelVolmetic(modelFile, scaleMultiplyier, scaleMultiplyier, scaleMultiplyier, transform, traceType, resetProgress, incrementProgress);
        }

        /// <summary>
        /// Volumes are calculated across axis where they are whole numbers (rounded to 0 decimal places).
        /// </summary>
        /// <param name="modelFile"></param>
        /// <param name="scaleMultiplyierX"></param>
        /// <param name="scaleMultiplyierY"></param>
        /// <param name="scaleMultiplyierZ"></param>
        /// <param name="transform"></param>
        /// <param name="traceType"></param>
        /// <param name="resetProgress"></param>
        /// <param name="incrementProgress"></param>
        /// <returns></returns>
        public static CubeType[][][] ReadModelVolmetic(string modelFile, double scaleMultiplyierX, double scaleMultiplyierY, double scaleMultiplyierZ, Transform3D transform, ModelTraceVoxel traceType, Action<double, double> resetProgress, Action incrementProgress)
        {
            var model = MeshHelper.Load(modelFile, ignoreErrors: true);

            // How far to check in from the proposed Volumetric edge.
            // This number is just made up, but small enough that it still represents the corner edge of the Volumetric space.
            // But still large enough that it isn't the exact corner.
            const double offset = 0.00000456f;

            if (scaleMultiplyierX > 0 && scaleMultiplyierY > 0 && scaleMultiplyierZ > 0 && scaleMultiplyierX != 1.0f && scaleMultiplyierY != 1.0f && scaleMultiplyierZ != 1.0f)
            {
                model.TransformScale(scaleMultiplyierX, scaleMultiplyierY, scaleMultiplyierZ);
            }

            var tbounds = model.Bounds;
            if (transform != null)
                tbounds = transform.TransformBounds(tbounds);

            var xMin = (int)Math.Floor(tbounds.X);
            var yMin = (int)Math.Floor(tbounds.Y);
            var zMin = (int)Math.Floor(tbounds.Z);

            var xMax = (int)Math.Ceiling(tbounds.X + tbounds.SizeX);
            var yMax = (int)Math.Ceiling(tbounds.Y + tbounds.SizeY);
            var zMax = (int)Math.Ceiling(tbounds.Z + tbounds.SizeZ);

            var xCount = xMax - xMin;
            var yCount = yMax - yMin;
            var zCount = zMax - zMin;

            var ccubic = ArrayHelper.Create<CubeType>(xCount, yCount, zCount);

            if (resetProgress != null)
            {
                double count = (from GeometryModel3D gm in model.Children select gm.Geometry as MeshGeometry3D).Aggregate<MeshGeometry3D, double>(0, (current, g) => current + (g.TriangleIndices.Count / 3));
                if (traceType == ModelTraceVoxel.ThinSmoothed || traceType == ModelTraceVoxel.ThickSmoothedUp)
                {
                    count += (xCount * yCount * zCount * 3);
                }

                resetProgress.Invoke(0, count);
            }

            #region basic ray trace of every individual triangle.

            foreach (var model3D in model.Children)
            {
                var gm = (GeometryModel3D)model3D;
                var g = gm.Geometry as MeshGeometry3D;
                var materials = gm.Material as MaterialGroup;
                System.Windows.Media.Color color = Colors.Transparent;

                if (materials != null)
                {
                    var material = materials.Children.OfType<DiffuseMaterial>().FirstOrDefault();

                    if (material != null && material != null && material.Brush is SolidColorBrush)
                    {
                        color = ((SolidColorBrush)material.Brush).Color;
                    }
                }

                for (var t = 0; t < g.TriangleIndices.Count; t += 3)
                {
                    if (incrementProgress != null)
                    {
                        incrementProgress.Invoke();
                    }

                    var p1 = g.Positions[g.TriangleIndices[t]];
                    var p2 = g.Positions[g.TriangleIndices[t + 1]];
                    var p3 = g.Positions[g.TriangleIndices[t + 2]];

                    if (transform != null)
                    {
                        p1 = transform.Transform(p1);
                        p2 = transform.Transform(p2);
                        p3 = transform.Transform(p3);
                    }

                    var minBound = MeshHelper.Min(p1, p2, p3).Floor();
                    var maxBound = MeshHelper.Max(p1, p2, p3).Ceiling();

                    Point3D[] rays;

                    for (var y = minBound.Y; y < maxBound.Y; y++)
                    {
                        for (var z = minBound.Z; z < maxBound.Z; z++)
                        {
                            if (traceType == ModelTraceVoxel.Thin || traceType == ModelTraceVoxel.ThinSmoothed)
                            {
                                rays = new Point3D[] // 1 point ray trace in the center.
                                    {
                                        new Point3D(xMin, y + 0.5 + offset, z + 0.5 + offset), new Point3D(xMax, y + 0.5 + offset, z + 0.5 + offset)
                                    };
                            }
                            else
                            {
                                rays = new Point3D[] // 4 point ray trace within each corner of the expected Volumetric cube.
                                    {
                                        new Point3D(xMin, y + offset, z + offset), new Point3D(xMax, y + offset, z + offset),
                                        new Point3D(xMin, y + 1 - offset, z + offset), new Point3D(xMax, y + 1 - offset, z + offset),
                                        new Point3D(xMin, y + offset, z + 1 - offset), new Point3D(xMax, y + offset, z + 1 - offset),
                                        new Point3D(xMin, y + 1 - offset, z + 1 - offset), new Point3D(xMax, y + 1 - offset, z + 1 - offset)
                                    };
                            }

                            Point3D intersect;
                            int normal;
                            if (MeshHelper.RayIntersetTriangleRound(p1, p2, p3, rays, out intersect, out normal))
                            {
                                ccubic[(int)Math.Floor(intersect.X) - xMin][(int)Math.Floor(intersect.Y) - yMin][(int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                            }
                        }
                    }

                    for (var x = minBound.X; x < maxBound.X; x++)
                    {
                        for (var z = minBound.Z; z < maxBound.Z; z++)
                        {
                            if (traceType == ModelTraceVoxel.Thin || traceType == ModelTraceVoxel.ThinSmoothed)
                            {
                                rays = new Point3D[] // 1 point ray trace in the center.
                                    {
                                        new Point3D(x + 0.5 + offset, yMin, z + 0.5 + offset), new Point3D(x + 0.5 + offset, yMax, z + 0.5 + offset)
                                    };
                            }
                            else
                            {
                                rays = new Point3D[] // 4 point ray trace within each corner of the expected Volumetric cube.
                                    {
                                        new Point3D(x + offset, yMin, z + offset), new Point3D(x + offset, yMax, z + offset),
                                        new Point3D(x + 1 - offset, yMin, z + offset), new Point3D(x + 1 - offset, yMax, z + offset),
                                        new Point3D(x + offset, yMin, z + 1 - offset), new Point3D(x + offset, yMax, z + 1 - offset),
                                        new Point3D(x + 1 - offset, yMin, z + 1 - offset), new Point3D(x + 1 - offset, yMax, z + 1 - offset)
                                    };
                            }

                            Point3D intersect;
                            int normal;
                            if (MeshHelper.RayIntersetTriangleRound(p1, p2, p3, rays, out intersect, out normal))
                            {
                                ccubic[(int)Math.Floor(intersect.X) - xMin][(int)Math.Floor(intersect.Y) - yMin][(int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                            }
                        }
                    }

                    for (var x = minBound.X; x < maxBound.X; x++)
                    {
                        for (var y = minBound.Y; y < maxBound.Y; y++)
                        {
                            if (traceType == ModelTraceVoxel.Thin || traceType == ModelTraceVoxel.ThinSmoothed)
                            {
                                rays = new Point3D[] // 1 point ray trace in the center.
                                    {
                                        new Point3D(x + 0.5 + offset, y + 0.5 + offset, zMin), new Point3D(x + 0.5 + offset, y + 0.5 + offset, zMax),
                                    };
                            }
                            else
                            {
                                rays = new Point3D[] // 4 point ray trace within each corner of the expected Volumetric cube.
                                    {
                                        new Point3D(x + offset, y + offset, zMin), new Point3D(x + offset, y + offset, zMax),
                                        new Point3D(x + 1 - offset, y + offset, zMin), new Point3D(x + 1 - offset, y + offset, zMax),
                                        new Point3D(x + offset, y + 1 - offset, zMin), new Point3D(x + offset, y + 1 - offset, zMax),
                                        new Point3D(x + 1 - offset, y + 1 - offset, zMin), new Point3D(x + 1 - offset, y + 1 - offset, zMax)
                                    };
                            }

                            Point3D intersect;
                            int normal;
                            if (MeshHelper.RayIntersetTriangleRound(p1, p2, p3, rays, out intersect, out normal))
                            {
                                ccubic[(int)Math.Floor(intersect.X) - xMin][(int)Math.Floor(intersect.Y) - yMin][(int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                            }
                        }
                    }
                }
            }

            #endregion

            CrawlExterior(ccubic);

            if (traceType == ModelTraceVoxel.ThinSmoothed || traceType == ModelTraceVoxel.ThickSmoothedUp)
            {
                CalculateAddedInverseCorners(ccubic, incrementProgress);
                CalculateAddedSlopes(ccubic, incrementProgress);
                CalculateAddedCorners(ccubic, incrementProgress);
            }

            //if (traceType == ModelTraceVoxel.ThickSmoothedDown)
            //{
            //    CalculateSubtractedCorners(ccubic);
            //    CalculateSubtractedSlopes(ccubic);
            //    CalculateSubtractedInverseCorners(ccubic);
            //}

            return ccubic;
        }

        #endregion

        #region ReadModelVolmeticAlt

        // WIP.
        public static CubeType[][][] ReadModelVolmeticAlt(string modelFile, double voxelUnitSize)
        {
            var model = MeshHelper.Load(modelFile, ignoreErrors: true);

            var min = model.Bounds;
            var max = new Point3D(model.Bounds.Location.X + model.Bounds.Size.X, model.Bounds.Location.X + model.Bounds.Size.Z, model.Bounds.Location.Z + model.Bounds.Size.Z);

            int xCount = 10; //xMax - xMin;
            int yCount = 10; //yMax - yMin;
            int zCount = 10; //zMax - zMin;

            var ccubic = ArrayHelper.Create<CubeType>(xCount, yCount, zCount);

            var blockDict = new Dictionary<Point3D, byte[]>();

            #region basic ray trace of every individual triangle.

            foreach (GeometryModel3D gm in model.Children)
            {
                var g = gm.Geometry as MeshGeometry3D;

                for (int t = 0; t < g.TriangleIndices.Count; t += 3)
                {
                    var p1 = g.Positions[g.TriangleIndices[t]];
                    var p2 = g.Positions[g.TriangleIndices[t + 1]];
                    var p3 = g.Positions[g.TriangleIndices[t + 2]];

                    var minBound = MeshHelper.Min(p1, p2, p3).Floor();
                    var maxBound = MeshHelper.Max(p1, p2, p3).Ceiling();

                    //for (var y = minBound.Y; y < maxBound.Y; y++)
                    //{
                    //    for (var z = minBound.Z; z < maxBound.Z; z++)
                    //    {
                    //        var r1 = new Point3D(xMin, y, z);
                    //        var r2 = new Point3D(xMax, y, z);

                    //        Point3D intersect;
                    //        if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r1, r2, out intersect)) // Ray
                    //        {
                    //            //var blockPoint = intersect.Round();
                    //            //var cornerHit = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };

                    //            //if (!blockDict.ContainsKey(blockPoint))
                    //            //    blockDict[blockPoint] = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    //            //if (Math.Round(intersect.X) - intersect.X < 0 && Math.Round(intersect.Y) - intersect.Y < 0 && Math.Round(intersect.Z) - intersect.Z < 0)
                    //            //    cornerHit = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X < 0 && Math.Round(intersect.Y) - intersect.Y > 0 && Math.Round(intersect.Z) - intersect.Z < 0)
                    //            //    cornerHit = new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X < 0 && Math.Round(intersect.Y) - intersect.Y < 0 && Math.Round(intersect.Z) - intersect.Z > 0)
                    //            //    cornerHit = new byte[] { 0, 0, 1, 0, 0, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X < 0 && Math.Round(intersect.Y) - intersect.Y > 0 && Math.Round(intersect.Z) - intersect.Z > 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X > 0 && Math.Round(intersect.Y) - intersect.Y < 0 && Math.Round(intersect.Z) - intersect.Z < 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X > 0 && Math.Round(intersect.Y) - intersect.Y > 0 && Math.Round(intersect.Z) - intersect.Z < 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X > 0 && Math.Round(intersect.Y) - intersect.Y < 0 && Math.Round(intersect.Z) - intersect.Z > 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 0, 0, 0, 1, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X > 0 && Math.Round(intersect.Y) - intersect.Y > 0 && Math.Round(intersect.Z) - intersect.Z > 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }; 

                    //            //blockDict[blockPoint]=[int(bool(a+b)) for a,b in zip(blockDict[blockPoint],cornerHit)]


                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //        else if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r2, r1, out intersect)) // Reverse Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //    }
                    //}

                    //for (var x = minBound.X; x < maxBound.X; x++)
                    //{
                    //    for (var z = minBound.Z; z < maxBound.Z; z++)
                    //    {
                    //        var r1 = new Point3D(x, yMin, z);
                    //        var r2 = new Point3D(x, yMax, z);

                    //        Point3D intersect;
                    //        if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r1, r2, out intersect)) // Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //        else if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r2, r1, out intersect)) // Reverse Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //    }
                    //}

                    //for (var x = minBound.X; x < maxBound.X; x++)
                    //{
                    //    for (var y = minBound.Y; y < maxBound.Y; y++)
                    //    {
                    //        var r1 = new Point3D(x, y, zMin);
                    //        var r2 = new Point3D(x, y, zMax);

                    //        Point3D intersect;
                    //        if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r1, r2, out intersect)) // Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //        else if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r2, r1, out intersect)) // Reverse Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //    }
                    //}
                }
            }

            #endregion

            return ccubic;
        }

        #endregion

        #region CrawlExterior

        public static void CrawlExterior(CubeType[][][] cubic)
        {
            // TODO: multi-thread this entire method, as it is the slowest to run for large arrays.

            var xCount = cubic.Length;
            var yCount = cubic[0].Length;
            var zCount = cubic[0][0].Length;

            var list = new Queue<Vector3I>();

            // Add basic check points from the corner blocks.
            if (cubic[0][0][0] == CubeType.None)
                list.Enqueue(new Vector3I(0, 0, 0));
            if (cubic[xCount - 1][0][0] == CubeType.None)
                list.Enqueue(new Vector3I(xCount - 1, 0, 0));
            if (cubic[0][yCount - 1][0] == CubeType.None)
                list.Enqueue(new Vector3I(0, yCount - 1, 0));
            if (cubic[0][0][zCount - 1] == CubeType.None)
                list.Enqueue(new Vector3I(0, 0, zCount - 1));
            if (cubic[xCount - 1][yCount - 1][0] == CubeType.None)
                list.Enqueue(new Vector3I(xCount - 1, yCount - 1, 0));
            if (cubic[0][yCount - 1][zCount - 1] == CubeType.None)
                list.Enqueue(new Vector3I(0, yCount - 1, zCount - 1));
            if (cubic[xCount - 1][0][zCount - 1] == CubeType.None)
                list.Enqueue(new Vector3I(xCount - 1, 0, zCount - 1));
            if (cubic[xCount - 1][yCount - 1][zCount - 1] == CubeType.None)
                list.Enqueue(new Vector3I(xCount - 1, yCount - 1, zCount - 1));

            while (list.Count > 0)
            {
                var item = list.Dequeue();

                if (cubic[item.X][item.Y][item.Z] == CubeType.None)
                {
                    cubic[item.X][item.Y][item.Z] = CubeType.Exterior;

                    if (item.X - 1 >= 0 && item.Y >= 0 && item.Z >= 0 && item.X - 1 < xCount && item.Y < yCount && item.Z < zCount && cubic[item.X - 1][item.Y][item.Z] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X - 1, item.Y, item.Z));
                    }
                    if (item.X >= 0 && item.Y - 1 >= 0 && item.Z >= 0 && item.X < xCount && item.Y - 1 < yCount && item.Z < zCount && cubic[item.X][item.Y - 1][item.Z] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X, item.Y - 1, item.Z));
                    }
                    if (item.X >= 0 && item.Y >= 0 && item.Z - 1 >= 0 && item.X < xCount && item.Y < yCount && item.Z - 1 < zCount && cubic[item.X][item.Y][item.Z - 1] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X, item.Y, item.Z - 1));
                    }
                    if (item.X + 1 >= 0 && item.Y >= 0 && item.Z >= 0 && item.X + 1 < xCount && item.Y < yCount && item.Z < zCount && cubic[item.X + 1][item.Y][item.Z] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X + 1, item.Y, item.Z));
                    }
                    if (item.X >= 0 && item.Y + 1 >= 0 && item.Z >= 0 && item.X < xCount && item.Y + 1 < yCount && item.Z < zCount && cubic[item.X][item.Y + 1][item.Z] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X, item.Y + 1, item.Z));
                    }
                    if (item.X >= 0 && item.Y >= 0 && item.Z + 1 >= 0 && item.X < xCount && item.Y < yCount && item.Z + 1 < zCount && cubic[item.X][item.Y][item.Z + 1] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X, item.Y, item.Z + 1));
                    }
                }
            }

            // switch values around to work with current enum logic.
            for (var x = 0; x < xCount; x++)
            {
                var cx = cubic[x];
                for (var y = 0; y < yCount; y++)
                {
                    var cy = cx[y];
                    for (var z = 0; z < zCount; z++)
                    {
                        if (cy[z] == CubeType.None)
                            cy[z] = CubeType.Interior;
                        else if (cy[z] == CubeType.Exterior)
                            cy[z] = CubeType.None;
                    }
                }
            }
        }

        #endregion

        #region CountCubic

        public static Dictionary<CubeType, int> CountCubic(CubeType[][][] cubic)
        {
            var assetCount = new Dictionary<CubeType, int>();
            var xCount = cubic.Length;
            var yCount = cubic[0].Length;
            var zCount = cubic[0][0].Length;

            for (var x = 0; x < xCount; x++)
            {
                var cx = cubic[x];
                for (var y = 0; y < yCount; y++)
                {
                    var cy = cx[y];
                    for (var z = 0; z < zCount; z++)
                    {
                        if (assetCount.ContainsKey(cy[z]))
                        {
                            assetCount[cy[z]]++;
                        }
                        else
                        {
                            assetCount.Add(cy[z], 1);
                        }
                    }
                }
            }

            return assetCount;
        }

        #endregion

        #region CalculateAddedSlopes

        private static void CalculateAddedSlopes(CubeType[][][] cubic, Action incrementProgress)
        {
            var xCount = cubic.Length;
            var yCount = cubic[0].Length;
            var zCount = cubic[0][0].Length;

            for (var x = 0; x < xCount; x++)
            {
                var cx = cubic[x];
                for (var y = 0; y < yCount; y++)
                {
                    var cy = cx[y];
                    for (var z = 0; z < zCount; z++)
                    {
                        if (incrementProgress != null)
                        {
                            incrementProgress.Invoke();
                        }

                        if (cy[z] == CubeType.None)
                        {
                            if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, 1, 1, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeCenterFrontTop;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, 1, 0, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeLeftFrontCenter;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 1, 1, 0, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeRightFrontCenter;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, 1, -1, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeCenterFrontBottom;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 1, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeLeftCenterTop;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 1, 0, 1, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeRightCenterTop;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, 0, -1, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeLeftCenterBottom;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 1, 0, -1, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeRightCenterBottom;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 1, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeCenterBackTop;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, -1, 0, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeLeftBackCenter;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 1, -1, 0, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeRightBackCenter;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, -1, -1, CubeType.Cube))
                            {
                                cy[z] = CubeType.SlopeCenterBackBottom;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region CalculateAddedCorners

        private static void CalculateAddedCorners(CubeType[][][] cubic, Action incrementProgress)
        {
            var xCount = cubic.Length;
            var yCount = cubic[0].Length;
            var zCount = cubic[0][0].Length;

            for (var x = 0; x < xCount; x++)
            {
                for (var y = 0; y < yCount; y++)
                {
                    for (var z = 0; z < zCount; z++)
                    {
                        if (incrementProgress != null)
                        {
                            incrementProgress.Invoke();
                        }

                        if (cubic[x][y][z] == CubeType.None)
                        {
                            if (CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftFrontCenter, -1, 0, 0, CubeType.SlopeCenterFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftFrontCenter, 0, +1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.SlopeLeftFrontCenter) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.SlopeLeftCenterTop, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.SlopeLeftFrontCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerLeftFrontTop;
                            }
                            else if (CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightFrontCenter, +1, 0, 0, CubeType.SlopeCenterFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightFrontCenter, 0, +1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.SlopeRightFrontCenter) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.SlopeRightCenterTop, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.SlopeRightFrontCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerRightFrontTop;
                            }
                            else if (CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftFrontCenter, -1, 0, 0, CubeType.SlopeCenterFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftFrontCenter, 0, +1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.SlopeLeftFrontCenter) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.SlopeLeftCenterBottom, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.SlopeLeftFrontCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerLeftFrontBottom;
                            }
                            else if (CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightFrontCenter, +1, 0, 0, CubeType.SlopeCenterFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightFrontCenter, 0, +1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.SlopeRightFrontCenter) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.SlopeRightCenterBottom, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.SlopeRightFrontCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerRightFrontBottom;
                            }
                            else if (CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftBackCenter, -1, 0, 0, CubeType.SlopeCenterBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftBackCenter, 0, -1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.SlopeLeftBackCenter) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.SlopeLeftCenterTop, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.SlopeLeftBackCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerLeftBackTop;
                            }
                            else if (CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightBackCenter, +1, 0, 0, CubeType.SlopeCenterBackTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightBackCenter, 0, -1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.SlopeRightBackCenter) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.SlopeRightCenterTop, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.SlopeRightBackCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerRightBackTop;
                            }
                            else if (CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftBackCenter, -1, 0, 0, CubeType.SlopeCenterBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftBackCenter, 0, -1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackBottom, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.SlopeLeftBackCenter) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.SlopeLeftCenterBottom, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.SlopeLeftBackCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerLeftBackBottom;
                            }
                            else if (CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightBackCenter, +1, 0, 0, CubeType.SlopeCenterBackBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightBackCenter, 0, -1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.SlopeRightBackCenter) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.SlopeRightCenterBottom, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(cubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.SlopeRightBackCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerRightBackBottom;
                            }


                            // ########### Triplet checks
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.SlopeRightCenterBottom, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.SlopeRightBackCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerRightBackBottom;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                  CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                  CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.SlopeLeftCenterTop, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                  CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.SlopeLeftFrontCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerLeftFrontTop;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterBottom, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.SlopeLeftBackCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerLeftBackBottom;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.SlopeRightCenterTop, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.SlopeRightFrontCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerRightFrontTop;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.SlopeRightCenterBottom, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.SlopeRightFrontCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerRightFrontBottom;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.SlopeLeftCenterTop, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.SlopeLeftBackCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerLeftBackTop;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.SlopeLeftCenterBottom, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.SlopeLeftFrontCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerLeftFrontBottom;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.SlopeRightCenterTop, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.SlopeRightBackCenter))
                            {
                                cubic[x][y][z] = CubeType.NormalCornerRightBackTop;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region CalculateAddedInverseCorners

        private static void CalculateAddedInverseCorners(CubeType[][][] cubic, Action incrementProgress)
        {
            var xCount = cubic.Length;
            var yCount = cubic[0].Length;
            var zCount = cubic[0][0].Length;

            for (var x = 0; x < xCount; x++)
            {
                for (var y = 0; y < yCount; y++)
                {
                    for (var z = 0; z < zCount; z++)
                    {
                        if (incrementProgress != null)
                        {
                            incrementProgress.Invoke();
                        }

                        if (cubic[x][y][z] == CubeType.None)
                        {
                            if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.Cube, 0, -1, 0, CubeType.Cube, 0, 0, -1, CubeType.Cube))
                            {
                                cubic[x][y][z] = CubeType.InverseCornerLeftFrontTop;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.Cube, 0, +1, 0, CubeType.Cube, 0, 0, -1, CubeType.Cube))
                            {
                                cubic[x][y][z] = CubeType.InverseCornerRightBackTop;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.Cube, 0, +1, 0, CubeType.Cube, 0, 0, -1, CubeType.Cube))
                            {
                                cubic[x][y][z] = CubeType.InverseCornerLeftBackTop;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.Cube, 0, -1, 0, CubeType.Cube, 0, 0, -1, CubeType.Cube))
                            {
                                cubic[x][y][z] = CubeType.InverseCornerRightFrontTop;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.Cube, 0, +1, 0, CubeType.Cube, 0, 0, +1, CubeType.Cube))
                            {
                                cubic[x][y][z] = CubeType.InverseCornerLeftBackBottom;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.Cube, 0, +1, 0, CubeType.Cube, 0, 0, +1, CubeType.Cube))
                            {
                                cubic[x][y][z] = CubeType.InverseCornerRightBackBottom;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.Cube, 0, -1, 0, CubeType.Cube, 0, 0, +1, CubeType.Cube))
                            {
                                cubic[x][y][z] = CubeType.InverseCornerLeftFrontBottom;
                            }
                            else if (CheckAdjacentCubic3(cubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.Cube, 0, -1, 0, CubeType.Cube, 0, 0, +1, CubeType.Cube))
                            {
                                cubic[x][y][z] = CubeType.InverseCornerRightFrontBottom;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region CheckAdjacentCubic

        private static bool IsValidRange(int x, int y, int z, int xCount, int yCount, int zCount, int xDelta, int yDelta, int zDelta)
        {
            if (x + xDelta >= 0 && x + xDelta < xCount
            && y + yDelta >= 0 && y + yDelta < yCount
            && z + zDelta >= 0 && z + zDelta < zCount)
            {
                return true;
            }

            return false;
        }

        private static bool CheckAdjacentCubic(CubeType[][][] ccubic, int x, int y, int z, int xCount, int yCount, int zCount, int xDelta, int yDelta, int zDelta, CubeType cubeType)
        {
            if (IsValidRange(x, y, z, xCount, yCount, zCount, xDelta, yDelta, zDelta))
            {
                if (xDelta != 0 && ccubic[x + xDelta][y][z] == cubeType &&
                    yDelta != 0 && ccubic[x][y + yDelta][z] == cubeType &&
                    zDelta == 0)
                {
                    return true;
                }

                if (xDelta != 0 && ccubic[x + xDelta][y][z] == cubeType &&
                    yDelta == 0 &&
                    zDelta != 0 && ccubic[x][y][z + zDelta] == cubeType)
                {
                    return true;
                }

                if (xDelta == 0 &&
                    yDelta != 0 && ccubic[x][y + yDelta][z] == cubeType &&
                    zDelta != 0 && ccubic[x][y][z + zDelta] == cubeType)
                {
                    return true;
                }

                if (xDelta != 0 && ccubic[x + xDelta][y][z] == cubeType &&
                    yDelta != 0 && ccubic[x][y + yDelta][z] == cubeType &&
                    zDelta != 0 && ccubic[x][y][z + zDelta] == cubeType)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CheckAdjacentCubic1(CubeType[][][] ccubic, int x, int y, int z, int xCount, int yCount, int zCount,
           int xDelta, int yDelta, int zDelta, CubeType cubeType)
        {
            if (IsValidRange(x, y, z, xCount, yCount, zCount, xDelta, yDelta, zDelta))
            {
                return ccubic[x + xDelta][y + yDelta][z + zDelta] == cubeType;
            }

            return false;
        }

        private static bool CheckAdjacentCubic2(CubeType[][][] ccubic, int x, int y, int z, int xCount, int yCount, int zCount,
            int xDelta1, int yDelta1, int zDelta1, CubeType cubeType1,
            int xDelta2, int yDelta2, int zDelta2, CubeType cubeType2)
        {
            if (IsValidRange(x, y, z, xCount, yCount, zCount, xDelta1, yDelta1, zDelta1) && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta2, yDelta2, zDelta2))
            {
                return ccubic[x + xDelta1][y + yDelta1][z + zDelta1] == cubeType1 && ccubic[x + xDelta2][y + yDelta2][z + zDelta2] == cubeType2;
            }

            return false;
        }

        private static bool CheckAdjacentCubic3(CubeType[][][] ccubic, int x, int y, int z, int xCount, int yCount, int zCount,
            int xDelta1, int yDelta1, int zDelta1, CubeType cubeType1,
            int xDelta2, int yDelta2, int zDelta2, CubeType cubeType2,
            int xDelta3, int yDelta3, int zDelta3, CubeType cubeType3)
        {
            if (IsValidRange(x, y, z, xCount, yCount, zCount, xDelta1, yDelta1, zDelta1)
                && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta2, yDelta2, zDelta2)
                && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta3, yDelta3, zDelta3))
            {
                return ccubic[x + xDelta1][y + yDelta1][z + zDelta1] == cubeType1
                    && ccubic[x + xDelta2][y + yDelta2][z + zDelta2] == cubeType2
                    && ccubic[x + xDelta3][y + yDelta3][z + zDelta3] == cubeType3;
            }

            return false;
        }

        #endregion

        #region BuildStructureFromCubic

        internal static void BuildStructureFromCubic(MyObjectBuilder_CubeGrid entity, CubeType[][][] cubic, bool fillObject, SubtypeId blockType, SubtypeId slopeBlockType, SubtypeId cornerBlockType, SubtypeId inverseCornerBlockType)
        {
            var xCount = cubic.Length;
            var yCount = cubic[0].Length;
            var zCount = cubic[0][0].Length;

            for (var x = 0; x < xCount; x++)
            {
                for (var y = 0; y < yCount; y++)
                {
                    for (var z = 0; z < zCount; z++)
                    {
                        if ((cubic[x][y][z] != CubeType.None && cubic[x][y][z] != CubeType.Interior) ||
                            (cubic[x][y][z] == CubeType.Interior && fillObject))
                        {
                            MyObjectBuilder_CubeBlock newCube;
                            entity.CubeBlocks.Add(newCube = new MyObjectBuilder_CubeBlock());

                            if (cubic[x][y][z].ToString().StartsWith("Cube"))
                            {
                                newCube.SubtypeName = blockType.ToString();
                            }
                            else if (cubic[x][y][z].ToString().StartsWith("Slope"))
                            {
                                newCube.SubtypeName = slopeBlockType.ToString();
                            }
                            else if (cubic[x][y][z].ToString().StartsWith("NormalCorner"))
                            {
                                newCube.SubtypeName = cornerBlockType.ToString();
                            }
                            else if (cubic[x][y][z].ToString().StartsWith("InverseCorner"))
                            {
                                newCube.SubtypeName = inverseCornerBlockType.ToString();
                            }
                            else if (cubic[x][y][z] == CubeType.Interior && fillObject)
                            {
                                newCube.SubtypeName = blockType.ToString();
                                cubic[x][y][z] = CubeType.Cube;
                            }

                            newCube.EntityId = 0;
                            newCube.BlockOrientation = GetCubeOrientation(cubic[x][y][z]);
                            newCube.Min = new Vector3I(x, y, z);
                        }
                    }
                }
            }
        }

        #endregion

        #region CalculateSubtractedCorners

        // Experimental code.
        private static void CalculateSubtractedCorners(CubeType[][][] cubic)
        {
            var xCount = cubic.Length;
            var yCount = cubic[0].Length;
            var zCount = cubic[0][0].Length;

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        // TODO:
                    }
                }
            }
        }

        #endregion

        #region CalculateSubtractedSlopes

        // Experimental code.
        private static void CalculateSubtractedSlopes(CubeType[][][] cubic)
        {
            var xCount = cubic.Length;
            var yCount = cubic[0].Length;
            var zCount = cubic[0][0].Length;

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        // TODO:
                        if (cubic[x][y][z] == CubeType.Cube)
                        {
                            // TODO:

                            if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, +1, +1, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, -1, -1, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeCenterFrontTop;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, +1, 0, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, +1, -1, 0, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeLeftFrontCenter;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, +1, +1, 0, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, -1, 0, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeRightFrontCenter;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, +1, -1, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, -1, +1, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeCenterFrontBottom;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, 0, +1, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, +1, 0, -1, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeLeftCenterTop;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, +1, 0, +1, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, 0, -1, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeRightCenterTop;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, 0, -1, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, +1, 0, +1, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeLeftCenterBottom;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, +1, 0, -1, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, 0, +1, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeRightCenterBottom;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, -1, +1, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, +1, -1, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeCenterBackTop;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, -1, 0, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, +1, +1, 0, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeLeftBackCenter;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, +1, -1, 0, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, -1, +1, 0, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeRightBackCenter;
                            }
                            else if (CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, -1, -1, CubeType.Cube) &&
                                CheckAdjacentCubic(cubic, x, y, z, xCount, yCount, zCount, 0, +1, +1, CubeType.None))
                            {
                                cubic[x][y][z] = CubeType.SlopeCenterBackBottom;
                            }

                        }
                    }
                }
            }
        }

        #endregion

        #region CalculateSubtractedInverseCorners

        // Experimental code.
        private static void CalculateSubtractedInverseCorners(CubeType[][][] cubic)
        {
            var xCount = cubic.Length;
            var yCount = cubic[0].Length;
            var zCount = cubic[0][0].Length;

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        // TODO:
                    }
                }
            }
        }

        #endregion

        #region SetCubeOrientation

        internal static readonly Dictionary<CubeType, SerializableBlockOrientation> CubeOrientations = new Dictionary<CubeType, SerializableBlockOrientation>()
        {
            // TODO: Remove the Cube Armor orientation, as these appear to work fine with the Generic.
            {CubeType.Cube, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)},

            // TODO: Remove the Slope Armor orientations, as these appear to work fine with the Generic.
            {CubeType.SlopeCenterBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Forward)}, // -90 around X
            {CubeType.SlopeRightBackCenter, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.SlopeLeftBackCenter, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.SlopeCenterBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)}, // no rotation
            {CubeType.SlopeRightCenterTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.SlopeLeftCenterTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.SlopeRightCenterBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Left)}, // +90 around Z
            {CubeType.SlopeLeftCenterBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Right)}, // -90 around Z
            {CubeType.SlopeCenterFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Down)}, // 180 around X
            {CubeType.SlopeRightFrontCenter, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Left)},
            {CubeType.SlopeLeftFrontCenter, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.SlopeCenterFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Backward)},// +90 around X

             // Probably got the names of these all messed up in relation to their actual orientation.
            {CubeType.NormalCornerLeftFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.NormalCornerRightFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Down)}, // 180 around X
            {CubeType.NormalCornerLeftBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.NormalCornerRightBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Forward)}, // -90 around X
            {CubeType.NormalCornerLeftFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.NormalCornerRightFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Backward)},// +90 around X 
            {CubeType.NormalCornerLeftBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Right)},// -90 around Z
            {CubeType.NormalCornerRightBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)},  // no rotation

            {CubeType.InverseCornerLeftFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.InverseCornerRightFrontTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Backward, VRageMath.Base6Directions.Direction.Down)}, // 180 around X
            {CubeType.InverseCornerLeftBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.InverseCornerRightBackTop, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Down, VRageMath.Base6Directions.Direction.Forward)},  // -90 around X
            {CubeType.InverseCornerLeftFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Right)},
            {CubeType.InverseCornerRightFrontBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Up, VRageMath.Base6Directions.Direction.Backward)}, // +90 around X
            {CubeType.InverseCornerLeftBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Right)}, // -90 around Z
            {CubeType.InverseCornerRightBackBottom, new SerializableBlockOrientation(VRageMath.Base6Directions.Direction.Forward, VRageMath.Base6Directions.Direction.Up)},  // no rotation
        };

        public static SerializableBlockOrientation GetCubeOrientation(CubeType type)
        {
            if (CubeOrientations.ContainsKey(type))
                return CubeOrientations[type];

            throw new NotImplementedException(string.Format("SetCubeOrientation of type [{0}] not yet implemented.", type));
        }

        #endregion

        #region TestObjects

        internal static CubeType[][][] TestCreateSplayedDiagonalPlane()
        {
            // Splayed diagonal plane.
            var max = 40;
            var ccubic = ArrayHelper.Create<CubeType>(max, max, max);

            for (var z = 0; z < max; z++)
            {
                for (var j = 0; j < max; j++)
                {
                    {
                        var x = j + z;
                        var y = j;
                        if (x >= 0 && y >= 0 && z >= 0 && x < max && y < max && z < max) ccubic[x][y][z] = CubeType.Cube;
                    }
                    {
                        var x = j;
                        var y = j + z;
                        if (x >= 0 && y >= 0 && z >= 0 && x < max && y < max && z < max) ccubic[x][y][z] = CubeType.Cube;
                    }
                    {
                        var x = j + z;
                        var y = max - j;
                        if (x >= 0 && y >= 0 && z >= 0 && x < max && y < max && z < max) ccubic[x][y][z] = CubeType.Cube;
                    }
                    {
                        var x = j;
                        var y = max - (j + z);
                        if (x >= 0 && y >= 0 && z >= 0 && x < max && y < max && z < max) ccubic[x][y][z] = CubeType.Cube;
                    }
                }
            }

            return ccubic;
        }

        internal static CubeType[][][] TestCreateSlopedDiagonalPlane()
        {
            // Sloped diagonal plane.
            var max = 20;
            var ccubic = ArrayHelper.Create<CubeType>(max, max, max);
            var dx = 1;
            var dy = 6;
            var dz = 0;

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (dx + j >= 0 && dy + j - i >= 0 && dz + i >= 0 &&
                        dx + j < max && dy + j - i < max && dz + i < max)
                    {
                        ccubic[dx + j][dy + j - i][dz + i] = CubeType.Cube;
                    }
                }
            }
            return ccubic;
        }

        internal static CubeType[][][] TestCreateStaggeredStar()
        {
            // Staggered star.

            var ccubic = ArrayHelper.Create<CubeType>(9, 9, 9);

            for (var i = 2; i < 7; i++)
            {
                for (var j = 2; j < 7; j++)
                {
                    ccubic[i][j][4] = CubeType.Cube;
                    ccubic[i][4][j] = CubeType.Cube;
                    ccubic[4][i][j] = CubeType.Cube;
                }
            }

            for (var i = 0; i < 9; i++)
            {
                ccubic[i][4][4] = CubeType.Cube;
                ccubic[4][i][4] = CubeType.Cube;
                ccubic[4][4][i] = CubeType.Cube;
            }

            return ccubic;
        }

        internal static CubeType[][][] TestCreateTrayShape()
        {
            // Tray shape

            var max = 20;
            var offset = 5;

            var ccubic = ArrayHelper.Create<CubeType>(max, max, max);

            for (var x = 0; x < max; x++)
            {
                for (var y = 0; y < max; y++)
                {
                    ccubic[2][x][y] = CubeType.Cube;
                }
            }

            for (var z = 1; z < 4; z += 2)
            {
                for (int i = 0; i < max; i++)
                {
                    ccubic[z][i][0] = CubeType.Cube;
                    ccubic[z][i][max - 1] = CubeType.Cube;
                    ccubic[z][0][i] = CubeType.Cube;
                    ccubic[z][max - 1][i] = CubeType.Cube;
                }

                for (int i = 0 + offset; i < max - offset; i++)
                {
                    ccubic[z][i][i] = CubeType.Cube;
                    ccubic[z][max - i - 1][i] = CubeType.Cube;
                }
            }

            return ccubic;
        }

        #endregion
    }
}
