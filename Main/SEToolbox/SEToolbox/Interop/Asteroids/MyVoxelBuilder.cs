namespace SEToolbox.Interop.Asteroids
{
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media.Media3D;
    using VRageMath;

    public static class MyVoxelBuilder
    {
        private static readonly object Locker = new object();

        #region methods

        public static void ConvertAsteroid(string loadFile, string saveFile, string defaultMaterial, string material)
        {
            var voxelMap = new MyVoxelMap();
            voxelMap.Load(loadFile, material);
            voxelMap.ForceBaseMaterial(defaultMaterial, material);
            voxelMap.Save(saveFile);
        }

        public static void StripMaterial(string loadFile, string saveFile, string defaultMaterial, string stripMaterial, string replaceFillMaterial)
        {
            var voxelMap = new MyVoxelMap();
            voxelMap.Load(loadFile, defaultMaterial);
            voxelMap.EmptyMaterial(stripMaterial, replaceFillMaterial);
            voxelMap.Save(saveFile);
        }

        #region BuildAsteroid standard tools

        public static MyVoxelMap BuildAsteroidCube(bool multiThread, string filename, int width, int height, int depth,
          string material, string faceMaterial, bool hollow = false, int shellWidth = 0, float safeSize = 0f)
        {
            // offset by 1, to allow for the 3 faces on the origin side.
            var size = new Vector3I(width + 1, height + 1, depth + 1);

            // offset by 1, to allow for the 3 faces on opposite side.
            var buildSize = new Vector3I(size.X + 1, size.Y + 1, size.Z + 1);

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                if (e.CoordinatePoint.X <= safeSize || e.CoordinatePoint.Y <= safeSize ||
                    e.CoordinatePoint.Z <= safeSize
                    || e.CoordinatePoint.X >= size.X - safeSize || e.CoordinatePoint.Y >= size.Y - safeSize ||
                    e.CoordinatePoint.Z >= size.Z - safeSize)
                {
                    e.Volume = 0x00;
                }
                else if (hollow &&
                         (e.CoordinatePoint.X <= safeSize + shellWidth || e.CoordinatePoint.Y <= safeSize + shellWidth ||
                          e.CoordinatePoint.Z <= safeSize + shellWidth
                          || e.CoordinatePoint.X >= size.X - (safeSize + shellWidth) ||
                          e.CoordinatePoint.Y >= size.Y - (safeSize + shellWidth) ||
                          e.CoordinatePoint.Z >= size.Z - (safeSize + shellWidth)))
                {
                    e.Volume = 0xFF;
                }
                else if (hollow)
                {
                    e.Volume = 0x00;
                }
                else //if (!hollow)
                {
                    e.Volume = 0xFF;
                }
            };

            return MyVoxelBuilder.BuildAsteroid(multiThread, filename, buildSize, material, faceMaterial, action);
        }

        public static MyVoxelMap BuildAsteroidCube(bool multiThread, string filename, Vector3I min, Vector3I max, string material, string faceMaterial)
        {
            // correct for allowing sizing.
            var buildSize = new Vector3I(ScaleMod(max.X, 64), ScaleMod(max.Y, 64), ScaleMod(max.Z, 64));

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                if (e.CoordinatePoint.X < min.X || e.CoordinatePoint.Y < min.Y || e.CoordinatePoint.Z < min.Z
                    || e.CoordinatePoint.X > max.X || e.CoordinatePoint.Y > max.Y || e.CoordinatePoint.Z > max.Z)
                {
                    e.Volume = 0x00;
                }
                else //if (!hollow)
                {
                    e.Volume = 0xFF;
                }
            };

            return MyVoxelBuilder.BuildAsteroid(multiThread, filename, buildSize, material, faceMaterial, action);
        }

        public static MyVoxelMap BuildAsteroidSphere(bool multiThread, string filename, double radius, string material, string faceMaterial,
            bool hollow = false, int shellWidth = 0)
        {
            var length = ScaleMod((radius * 2) + 2, 64);
            var size = new Vector3I(length, length, length);
            var origin = new Vector3I(size.X / 2, size.Y / 2, size.Z / 2);

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                var dist =
                    Math.Sqrt(Math.Abs(Math.Pow(e.CoordinatePoint.X - origin.X, 2)) +
                              Math.Abs(Math.Pow(e.CoordinatePoint.Y - origin.Y, 2)) +
                              Math.Abs(Math.Pow(e.CoordinatePoint.Z - origin.Z, 2)));

                if (dist >= radius)
                {
                    e.Volume = 0x00;
                }
                else if (dist > radius - 1)
                {
                    e.Volume = (byte)((radius - dist) * 255);
                }
                else if (hollow && (radius - shellWidth) < dist)
                {
                    e.Volume = 0xFF;
                }
                else if (hollow && (radius - shellWidth - 1) < dist)
                {
                    e.Volume = (byte)((1 - ((radius - shellWidth) - dist)) * 255);
                }
                else if (hollow)
                {
                    e.Volume = 0x00;
                }
                else //if (!hollow)
                {
                    e.Volume = 0xFF;
                }
            };

            return MyVoxelBuilder.BuildAsteroid(multiThread, filename, size, material, faceMaterial, action);
        }


        public static MyVoxelMap BuildAsteroidFromModel(bool multiThread, string sourceVolumetricFile, string filename, string material, string faceMaterial, bool fillObject, string interiorMaterial, ModelTraceVoxel traceType, double scale, Transform3D transform)
        {
            return BuildAsteroidFromModel(multiThread, sourceVolumetricFile, filename, material, faceMaterial, fillObject, interiorMaterial, traceType, scale, transform, null, null);
        }

        public static MyVoxelMap BuildAsteroidFromModel(bool multiThread, string sourceVolumetricFile, string filename, string material, string faceMaterial, bool fillObject, string interiorMaterial, ModelTraceVoxel traceType, double scale, Transform3D transform, Action<double, double> resetProgress, Action incrementProgress)
        {
            var volmeticMap = Modelling.ReadModelVolmetic(sourceVolumetricFile, scale, transform, traceType, resetProgress, incrementProgress);
            var size = new Vector3I(volmeticMap.GetLength(0) + 4, volmeticMap.GetLength(1) + 4, volmeticMap.GetLength(2) + 4);

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                if (e.CoordinatePoint.X > 1 && e.CoordinatePoint.Y > 1 && e.CoordinatePoint.Z > 1 &&
                    //e.CoordinatePoint.X <= volmeticMap.GetLength(0) && e.CoordinatePoint.Y <= volmeticMap.GetLength(1) && e.CoordinatePoint.Z <= volmeticMap.GetLength(2))
                    (e.CoordinatePoint.X <= volmeticMap.GetLength(0) + 1) && (e.CoordinatePoint.Y <= volmeticMap.GetLength(1) + 1) && (e.CoordinatePoint.Z <= volmeticMap.GetLength(2) + 1))
                {
                    var cube = volmeticMap[e.CoordinatePoint.X - 2, e.CoordinatePoint.Y - 2, e.CoordinatePoint.Z - 2];
                    if (cube == CubeType.Interior && fillObject)
                    {
                        e.Volume = 0xff;    // 100%
                        if (interiorMaterial != null)
                        {
                            e.Material = interiorMaterial;
                        }
                    }
                    else if (cube == CubeType.Cube)
                        e.Volume = 0xff;    // 100% "11111111"
                    else if (cube.ToString().StartsWith("InverseCorner"))
                        e.Volume = 0xD4;    // 83%  "11010100"
                    else if (cube.ToString().StartsWith("Slope"))
                        e.Volume = 0x7F;    // 50%  "01111111"
                    else if (cube.ToString().StartsWith("NormalCorner"))
                        e.Volume = 0x2B;    // 16%  "00101011"
                    else
                        e.Volume = 0x00;    // 0%   "00000000"
                }
                else
                {
                    e.Volume = 0x00;
                }
            };

            return MyVoxelBuilder.BuildAsteroid(multiThread, filename, size, material, faceMaterial, action);
        }

        #endregion

        #region BuildAsteroid

        /// <summary>
        /// Builds and asteroid Voxel and saves it to the specified file. Voxel detail will be completed by function callbacks.
        /// This allows for muti-threading, and generating content via algorithims.
        /// </summary>
        /// <param name="multiThread"></param>
        /// <param name="filename"></param>
        /// <param name="size"></param>
        /// <param name="material"></param>
        /// <param name="faceMaterial"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static MyVoxelMap BuildAsteroid(bool multiThread, string filename, Vector3I size, string material, string faceMaterial, Action<MyVoxelBuilderArgs> func)
        {
            var displayname = Path.GetFileNameWithoutExtension(filename);
            var voxelMap = new MyVoxelMap();
            var actualSize = new Vector3I(ScaleMod(size.X, 64), ScaleMod(size.Y, 64), ScaleMod(size.Z, 64));
            voxelMap.Init(displayname, Vector3.Zero, actualSize, material);

            var counterTotal = actualSize.X * actualSize.Y * actualSize.Z;
            var counter = 0;
            decimal progress = 0;
            var timer = new Stopwatch();
            Debug.Write(string.Format("{0} : {1:000},", displayname, progress));

            timer.Start();

            if (!multiThread)
            {
                #region single thread processing

                for (var x = 0; x < actualSize.X; x++)
                {
                    for (var y = 0; y < actualSize.Y; y++)
                    {
                        for (var z = 0; z < actualSize.Z; z++)
                        {
                            var coords = new Vector3I(x, y, z);

                            var args = new MyVoxelBuilderArgs(actualSize, coords, material, 0xff, 0xff);
                            func(args);

                            if (args.Volume != MyVoxelConstants.VOXEL_CONTENT_FULL)
                            {
                                voxelMap.SetVoxelContent(args.Volume, ref coords);
                            }

                            if (material != args.Material)
                            {
                                voxelMap.SetVoxelMaterialAndIndestructibleContent(args.Material, args.Indestructible, ref coords);
                            }

                            counter++;
                            var prog = Math.Floor((decimal)counter / (decimal)counterTotal * 100);
                            if (prog != progress)
                            {
                                progress = prog;
                                Debug.Write(string.Format("{0:000},", progress));
                            }
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region multi thread processing

                // TODO: re-write the multi thread processing to be more stable.
                // But still try and max out the processors.

                var taskArray = new List<Task>();

                var baseCoords = new Vector3I(0, 0, 0);
                for (baseCoords.X = 0; baseCoords.X < actualSize.X; baseCoords.X += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE)
                {
                    for (baseCoords.Y = 0; baseCoords.Y < actualSize.Y; baseCoords.Y += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE)
                    {
                        for (baseCoords.Z = 0; baseCoords.Z < actualSize.Z; baseCoords.Z += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE)
                        {
                            Task task;

                            taskArray.Add(task = new Task((obj) =>
                            {
                                var bgw = (MyVoxelBackgroundWorker)obj;

                                var coords = new Vector3I(0, 0, 0);
                                for (coords.X = bgw.BaseCoords.X; coords.X < bgw.BaseCoords.X + MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE; coords.X++)
                                {
                                    for (coords.Y = bgw.BaseCoords.Y; coords.Y < bgw.BaseCoords.Y + MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE; coords.Y++)
                                    {
                                        for (coords.Z = bgw.BaseCoords.Z; coords.Z < bgw.BaseCoords.Z + MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE; coords.Z++)
                                        {
                                            var args = new MyVoxelBuilderArgs(actualSize, coords, material, 0xff, 0xff);
                                            func(args);

                                            if (args.Volume != MyVoxelConstants.VOXEL_CONTENT_FULL)
                                            {
                                                voxelMap.SetVoxelContent(args.Volume, ref coords);
                                            }

                                            if (material != args.Material)
                                            {
                                                voxelMap.SetVoxelMaterialAndIndestructibleContent(args.Material, args.Indestructible, ref coords);
                                            }
                                        }
                                    }
                                }

                                lock (Locker)
                                {
                                    counter += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE *
                                            MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE *
                                            MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE;
                                    var prog = Math.Floor((decimal)counter / (decimal)counterTotal * 100);
                                    if (prog != progress)
                                    {
                                        progress = prog;
                                        Debug.Write(string.Format("{0:000},", progress));
                                    }
                                }

                            }, new MyVoxelBackgroundWorker(baseCoords)));

                            task.Start();
                        }
                    }
                }

                while (taskArray.Any(t => !t.IsCompleted))
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                System.Threading.Thread.Sleep(100);
                System.Windows.Forms.Application.DoEvents();

                #endregion
            }

            timer.Stop();

            if (faceMaterial != null)
            {
                voxelMap.ForceVoxelFaceMaterial(faceMaterial);
            }

            voxelMap.UpdateContentBounds();

            var count = voxelMap.SumVoxelCells();

            Debug.WriteLine(string.Format(" Done. | {0}  | VoxCells {1:#,##0}", timer.Elapsed, count));
            voxelMap.Save(filename);

            return voxelMap;
        }

        #endregion

        #endregion

        #region helpers

        public static int ScaleMod(double value, int scale)
        {
            return (int)Math.Ceiling(value / scale) * scale;
        }

        #endregion
    }
}
