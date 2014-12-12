namespace SEToolbox.Interop.Asteroids
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Media.Media3D;

    using SEToolbox.Support;
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
            voxelMap.RemoveContent(stripMaterial, replaceFillMaterial);
            voxelMap.Save(saveFile);
        }

        #region BuildAsteroid standard tools

        public static MyVoxelMap BuildAsteroidCube(bool multiThread, int width, int height, int depth,
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

            return BuildAsteroid(multiThread, buildSize, material, faceMaterial, action);
        }

        public static MyVoxelMap BuildAsteroidCube(bool multiThread, Vector3I min, Vector3I max, string material, string faceMaterial)
        {
            // correct for allowing sizing.
            var buildSize = new Vector3I(max.X.RoundUpToNearest(64), max.Y.RoundUpToNearest(64), max.Z.RoundUpToNearest(64));

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

            return BuildAsteroid(multiThread, buildSize, material, faceMaterial, action);
        }

        public static MyVoxelMap BuildAsteroidSphere(bool multiThread, double radius, string material, string faceMaterial,
            bool hollow = false, int shellWidth = 0)
        {
            var length = (int)((radius * 2) + 2).RoundUpToNearest(64);
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

            return BuildAsteroid(multiThread, size, material, faceMaterial, action);
        }


        public static MyVoxelMap BuildAsteroidFromModel(bool multiThread, string sourceVolumetricFile, string material, string faceMaterial, bool fillObject, string interiorMaterial, ModelTraceVoxel traceType, double scale, Transform3D transform)
        {
            return BuildAsteroidFromModel(multiThread, sourceVolumetricFile, material, faceMaterial, fillObject, interiorMaterial, traceType, scale, transform, null, null);
        }

        public static MyVoxelMap BuildAsteroidFromModel(bool multiThread, string sourceVolumetricFile, string material, string faceMaterial, bool fillObject, string interiorMaterial, ModelTraceVoxel traceType, double scale, Transform3D transform, Action<double, double> resetProgress, Action incrementProgress)
        {
            var volmeticMap = Modelling.ReadModelVolmetic(sourceVolumetricFile, scale, transform, traceType, resetProgress, incrementProgress);
            var size = new Vector3I(volmeticMap.Length + 12, volmeticMap[0].Length + 12, volmeticMap[0][0].Length + 12);

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                if (e.CoordinatePoint.X > 5 && e.CoordinatePoint.Y > 5 && e.CoordinatePoint.Z > 5 &&
                    (e.CoordinatePoint.X <= volmeticMap.Length + 5) && (e.CoordinatePoint.Y <= volmeticMap[0].Length + 5) && (e.CoordinatePoint.Z <= volmeticMap[0][0].Length + 5))
                {
                    var cube = volmeticMap[e.CoordinatePoint.X - 6][e.CoordinatePoint.Y - 6][e.CoordinatePoint.Z - 6];
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

            return BuildAsteroid(multiThread, size, material, faceMaterial, action);
        }

        #endregion

        #region BuildAsteroid

        /// <summary>
        /// Builds an asteroid Voxel. Voxel detail will be completed by function callbacks.
        /// This allows for muti-threading, and generating content via algorithims.
        /// </summary>
        /// <param name="multiThread"></param>
        /// <param name="size"></param>
        /// <param name="material"></param>
        /// <param name="faceMaterial"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static MyVoxelMap BuildAsteroid(bool multiThread, Vector3I size, string material, string faceMaterial, Action<MyVoxelBuilderArgs> func)
        {
            var voxelMap = new MyVoxelMap();
            var actualSize = new Vector3I(size.X.RoundUpToNearest(64), size.Y.RoundUpToNearest(64), size.Z.RoundUpToNearest(64));
            voxelMap.Init(VRageMath.Vector3D.Zero, actualSize, material);

            ProcessAsteroid(voxelMap, multiThread, material, func, false);

            if (faceMaterial != null)
            {
                voxelMap.ForceVoxelFaceMaterial(faceMaterial);
            }

            return voxelMap;
        }

        #endregion

        #region ProcessAsteroid

        /// <summary>
        /// Processes an asteroid Voxel using function callbacks.
        /// This allows for muti-threading, and generating content via algorithims.
        /// </summary>
        /// <param name="voxelMap"></param>
        /// <param name="multiThread"></param>
        /// <param name="material"></param>
        /// <param name="func"></param>
        /// <param name="readWrite"></param>
        /// <returns></returns>
        public static void ProcessAsteroid(MyVoxelMap voxelMap, bool multiThread, string material, Action<MyVoxelBuilderArgs> func, bool readWrite = true)
        {
            var counterTotal = voxelMap.Size.X * voxelMap.Size.Y * voxelMap.Size.Z;
            var counter = 0;
            decimal progress = 0;
            var timer = new Stopwatch();
            Debug.Write(string.Format("Building Asteroid : {0:000},", progress));
            Exception threadException = null;

            timer.Start();

            if (!multiThread)
            {
                #region single thread processing

                for (var x = 0; x < voxelMap.Size.X; x++)
                {
                    for (var y = 0; y < voxelMap.Size.Y; y++)
                    {
                        for (var z = 0; z < voxelMap.Size.Z; z++)
                        {
                            var coords = new Vector3I(x, y, z);

                            byte volume = 0xff;
                            var cellMaterial = material;

                            if (readWrite)
                                voxelMap.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                            var args = new MyVoxelBuilderArgs(voxelMap.Size, coords, cellMaterial, volume, 0xff);

                            try
                            {
                                func(args);
                            }
                            catch (Exception ex)
                            {
                                threadException = ex;
                                break;
                            }

                            if (args.Volume != MyVoxelConstants.VOXEL_CONTENT_FULL)
                            {
                                voxelMap.SetVoxelContent(args.Volume, ref coords);
                            }

                            if (material != args.Material)
                            {
                                voxelMap.SetVoxelMaterialAndIndestructibleContent(args.Material, args.Indestructible, ref coords);
                            }

                            counter++;
                            var prog = Math.Floor(counter / (decimal)counterTotal * 100);
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

                var threadCounter = counterTotal / MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE / MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE / MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE;

                var baseCoords = new Vector3I(0, 0, 0);
                for (baseCoords.X = 0; baseCoords.X < voxelMap.Size.X; baseCoords.X += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE)
                {
                    for (baseCoords.Y = 0; baseCoords.Y < voxelMap.Size.Y; baseCoords.Y += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE)
                    {
                        for (baseCoords.Z = 0; baseCoords.Z < voxelMap.Size.Z; baseCoords.Z += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE)
                        {
                            var task = new Task(obj =>
                            {
                                var bgw = (MyVoxelTaskWorker)obj;

                                var coords = new Vector3I(0, 0, 0);
                                for (coords.X = bgw.BaseCoords.X; coords.X < bgw.BaseCoords.X + MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE; coords.X++)
                                {
                                    for (coords.Y = bgw.BaseCoords.Y; coords.Y < bgw.BaseCoords.Y + MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE; coords.Y++)
                                    {
                                        for (coords.Z = bgw.BaseCoords.Z; coords.Z < bgw.BaseCoords.Z + MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE; coords.Z++)
                                        {
                                            byte volume = 0xff;
                                            var cellMaterial = material;

                                            if (readWrite)
                                                voxelMap.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                                            var args = new MyVoxelBuilderArgs(voxelMap.Size, coords, cellMaterial, volume, 0xff);
                                            try
                                            {
                                                func(args);
                                            }
                                            catch (Exception ex)
                                            {
                                                threadException = ex;
                                                threadCounter = 0;
                                                break;
                                            }

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
                                    var prog = Math.Floor(counter / (decimal)counterTotal * 100);
                                    if (prog != progress)
                                    {
                                        progress = prog;
                                        Debug.Write(string.Format("{0:000},", progress));
                                    }
                                    threadCounter--;
                                }

                            }, new MyVoxelTaskWorker(baseCoords));

                            task.Start();
                        }
                    }
                }

                while (threadCounter > 0)
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                System.Threading.Thread.Sleep(100);
                System.Windows.Forms.Application.DoEvents();

                #endregion
            }

            timer.Stop();

            if (threadException != null)
                throw threadException;

            voxelMap.UpdateContentBounds();

            var count = voxelMap.SumVoxelCells();

            Debug.WriteLine(" Done. | {0}  | VoxCells {1:#,##0}", timer.Elapsed, count);
        }

        #endregion

        #endregion
    }
}
