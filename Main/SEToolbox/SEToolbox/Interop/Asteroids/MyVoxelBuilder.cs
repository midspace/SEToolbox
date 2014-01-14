namespace SEToolbox.Interop.Asteroids
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using VRageMath;

    public static class MyVoxelBuilder
    {
        private static readonly object Locker = new object();

        #region methods

        public static void ConvertAsteroid(string loadFile, string saveFile, string material)
        {
            var voxelMap = new MyVoxelMap();
            voxelMap.Load(loadFile, material);
            voxelMap.ForceBaseMaterial(material);
            voxelMap.Save(saveFile);
        }

        public static MyVoxelMap BuildAsteroidCube(bool multiThread, string filename, int width, int height, int depth,
            string material, bool hollow = false, int shellWidth = 0, float safeSize = 0f)
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

            return MyVoxelBuilder.BuildAsteroid(multiThread, filename, buildSize, material, action);
        }

        public static MyVoxelMap BuildAsteroidSphere(bool multiThread, string filename, double radius, string material,
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

            return MyVoxelBuilder.BuildAsteroid(multiThread, filename, size, material, action);
        }

        /// <summary>
        /// Builds and asteroid Voxel and saves it to the specified file. Voxel detail will be completed by function callbacks.
        /// This allows for muti-threading, and generating content via algorithims.
        /// </summary>
        /// <param name="multiThread"></param>
        /// <param name="filename"></param>
        /// <param name="size"></param>
        /// <param name="material"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static MyVoxelMap BuildAsteroid(bool multiThread, string filename, Vector3I size, string material, Action<MyVoxelBuilderArgs> func)
        {
            var displayname = Path.GetFileNameWithoutExtension(filename);
            var voxelMap = new MyVoxelMap();
            var actualSize = new Vector3I(ScaleMod(size.X, 64), ScaleMod(size.Y, 64), ScaleMod(size.Z, 64));
            voxelMap.Init(displayname, Vector3.Zero, actualSize, material);

            var counterTotal = actualSize.X * actualSize.Y * actualSize.Z;
            var counter = 0;
            decimal progress = 0;
            var timer = new Stopwatch();
            Debug.Write(string.Format("{0} : {1:000}", displayname, progress));

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
                                Debug.Write(string.Format("{0:000}", progress));
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

                var workers = new List<BackgroundWorker>();
                var workerCounter = 0;

                var baseCoords = new Vector3I(0, 0, 0);
                for (baseCoords.X = 0; baseCoords.X < actualSize.X; baseCoords.X += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE)
                {
                    for (baseCoords.Y = 0; baseCoords.Y < actualSize.Y; baseCoords.Y += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE)
                    {
                        for (baseCoords.Z = 0; baseCoords.Z < actualSize.Z; baseCoords.Z += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE)
                        {
                            var worker = new BackgroundWorker();

                            // Threaded work. Do each Data Cell as a single worker process, to prevent them from stepping all over each other at the byte level.
                            worker.DoWork += delegate(object s, DoWorkEventArgs workArgs)
                            {
                                var bgw = (MyVoxelBackgroundWorker)workArgs.Argument;

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

                            };

                            // Brute force threading. not 100% reliable. Occasionally, some threads will not complete.
                            worker.RunWorkerAsync(new MyVoxelBackgroundWorker(baseCoords));
                            worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
                            {
                                lock (Locker)
                                {
                                    System.Windows.Forms.Application.DoEvents();
                                    workerCounter++;

                                    counter += MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE *
                                               MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE *
                                               MyVoxelConstants.VOXEL_DATA_CELLS_IN_RENDER_CELL_SIZE;
                                    var prog = Math.Floor((decimal)counter / (decimal)counterTotal * 100);
                                    if (prog != progress)
                                    {
                                        progress = prog;
                                        Debug.Write(string.Format("{0:000}", progress));
                                    }
                                    System.Windows.Forms.Application.DoEvents();
                                }
                            };
                            workers.Add(worker);
                        }
                    }
                }

                while (workerCounter < workers.Count)
                {
                    System.Windows.Forms.Application.DoEvents();
                }

                System.Threading.Thread.Sleep(100);
                System.Windows.Forms.Application.DoEvents();

                #endregion
            }

            timer.Stop();

            voxelMap.UpdateContentBounds();
            var count = voxelMap.SumVoxelCells();

            Debug.WriteLine(" Done. | {0}  | VoxCells {1:#,##0}", timer.Elapsed, count);
            voxelMap.Save(filename);

            return voxelMap;
        }

        #endregion

        #region helpers

        private static int ScaleMod(double value, int scale)
        {
            return (int)Math.Ceiling(value / scale) * scale;
        }

        #endregion
    }
}
