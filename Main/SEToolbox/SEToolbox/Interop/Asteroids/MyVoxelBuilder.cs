namespace SEToolbox.Interop.Asteroids
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Media.Media3D;

    using SEToolbox.Support;
    using VRage.Voxels;
    using VRageMath;

    public static class MyVoxelBuilder
    {
        private static readonly object Locker = new object();

        #region methods

        public static void ConvertAsteroid(string loadFile, string saveFile, string defaultMaterial, string material)
        {
            var voxelMap = new MyVoxelMap();
            voxelMap.Load(loadFile);
            voxelMap.ForceBaseMaterial(defaultMaterial, material);
            voxelMap.Save(saveFile);
            voxelMap.Dispose();
        }

        public static void StripMaterial(string loadFile, string saveFile, string defaultMaterial, string stripMaterial, string replaceFillMaterial)
        {
            var voxelMap = new MyVoxelMap();
            voxelMap.Load(loadFile);
            voxelMap.RemoveContent(stripMaterial, replaceFillMaterial);
            voxelMap.Save(saveFile);
        }

        #region BuildAsteroid standard tools

        public static MyVoxelMap BuildAsteroidCube(bool multiThread, int width, int height, int depth,
          byte materialIndex, byte faceMaterialIndex, bool hollow = false, int shellWidth = 0, float safeSize = 0f)
        {
            // offset by 1, to allow for the 3 faces on the origin side.
            var size = new Vector3I(width, height, depth) + 1;

            // offset by 1, to allow for the 3 faces on opposite side.
            var buildSize = size + 1;

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

            return BuildAsteroid(multiThread, buildSize, materialIndex, faceMaterialIndex, action);
        }

        public static MyVoxelMap BuildAsteroidCube(bool multiThread, Vector3I min, Vector3I max, byte materialIndex, byte faceMaterialIndex)
        {
            // correct for allowing sizing.
            var buildSize = CalcRequiredSize(max);

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

            return BuildAsteroid(multiThread, buildSize, materialIndex, faceMaterialIndex, action);
        }

        public static MyVoxelMap BuildAsteroidSphere(bool multiThread, double radius, byte materialIndex, byte faceMaterialIndex,
            bool hollow = false, int shellWidth = 0)
        {
            var length = (int)((radius * 2) + 2);
            var buildSize = CalcRequiredSize(length);
            var origin = new Vector3I(buildSize.X / 2, buildSize.Y / 2, buildSize.Z / 2);

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                VRageMath.Vector3D voxelPosition = e.CoordinatePoint;

                int v = GetSphereVolume(ref voxelPosition, radius, origin);
                if (hollow)
                {
                    int h = GetSphereVolume(ref voxelPosition, radius - shellWidth, origin);
                    e.Volume = (byte)(v - h);
                }
                else
                    e.Volume = (byte)v;
            };

            return BuildAsteroid(multiThread, buildSize, materialIndex, faceMaterialIndex, action);
        }

        public static byte GetSphereVolume(ref VRageMath.Vector3D voxelPosition, double radius, VRageMath.Vector3D center)
        {
            double num = (voxelPosition - center).Length();
            double signedDistance = num - radius;

            signedDistance = MathHelper.Clamp(-signedDistance, -1f, 1f) * 0.5f + 0.5f;
            return (byte)(signedDistance * 255);
        }

        public static MyVoxelMap BuildAsteroidFromModel(bool multiThread, string sourceVolumetricFile, byte materialIndex, byte faceMaterialIndex, bool fillObject, byte? interiorMaterialIndex, ModelTraceVoxel traceType, double scale, Transform3D transform)
        {
            return BuildAsteroidFromModel(multiThread, sourceVolumetricFile, materialIndex, faceMaterialIndex, fillObject, interiorMaterialIndex, traceType, scale, transform, null, null);
        }

        public static MyVoxelMap BuildAsteroidFromModel(bool multiThread, string sourceVolumetricFile, byte materialIndex, byte faceMaterialIndex, bool fillObject, byte? interiorMaterialIndex, ModelTraceVoxel traceType, double scale, Transform3D transform, Action<double, double> resetProgress, Action incrementProgress)
        {
            var volmeticMap = Modelling.ReadModelVolmetic(sourceVolumetricFile, scale, transform, traceType, resetProgress, incrementProgress);
            // these large values were to fix issue with large square gaps in voxlized asteroid model.
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
                        if (interiorMaterialIndex != null)
                        {
                            e.MaterialIndex = interiorMaterialIndex.Value;
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

            return BuildAsteroid(multiThread, size, materialIndex, faceMaterialIndex, action);
        }

        #endregion

        #region BuildAsteroid

        /// <summary>
        /// Builds an asteroid Voxel. Voxel detail will be completed by function callbacks.
        /// This allows for muti-threading, and generating content via algorithims.
        /// </summary>
        public static MyVoxelMap BuildAsteroid(bool multiThread, Vector3I size, byte materialIndex, byte? faceMaterialIndex, Action<MyVoxelBuilderArgs> func)
        {
            var voxelMap = new MyVoxelMap();

            var buildSize = CalcRequiredSize(size);
            voxelMap.Create(buildSize, materialIndex);
            ProcessAsteroid(voxelMap, multiThread, materialIndex, func, true);

            // This should no longer be required.
            //if (faceMaterialIndex != null)
            //{
            //    voxelMap.ForceVoxelFaceMaterial(faceMaterialIndex.Value);
            //}

            return voxelMap;
        }

        #endregion

        #region ProcessAsteroid

        /// <summary>
        /// Processes an asteroid Voxel using function callbacks.
        /// This allows for muti-threading, and generating content via algorithims.
        /// </summary>
        public static void ProcessAsteroid(MyVoxelMap voxelMap, bool multiThread, byte materialIndex, Action<MyVoxelBuilderArgs> func, bool readWrite = true)
        {
            long counterTotal = (long)voxelMap.Size.X * voxelMap.Size.Y * voxelMap.Size.Z;
            long counter = 0;
            decimal progress = 0;
            var timer = new Stopwatch();
            Debug.Write($"Building Asteroid : {progress:000},");
            Console.Write($"Building Asteroid : {progress:000},");
            Exception threadException = null;

            timer.Start();

            if (!multiThread)
            {
                #region single thread processing

                Vector3I block;
                const int cellSize = 64;
                var cacheSize = Vector3I.Min(new Vector3I(cellSize), voxelMap.Storage.Size);
                var oldCache = new MyStorageData();

                // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
                for (block.Z = 0; block.Z < voxelMap.Storage.Size.Z; block.Z += cellSize)
                    for (block.Y = 0; block.Y < voxelMap.Storage.Size.Y; block.Y += cellSize)
                        for (block.X = 0; block.X < voxelMap.Storage.Size.X; block.X += cellSize)
                        {
                            oldCache.Resize(cacheSize);
                            // LOD0 is required to read if you intend to write back to the voxel storage.
                            Vector3I maxRange = block + cacheSize - 1;
                            voxelMap.Storage.ReadRange(oldCache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, maxRange);

                            Vector3I p;
                            for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                                for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                    for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                    {
                                        var coords = block + p;

                                        byte volume = 0x0;
                                        byte cellMaterial = materialIndex;

                                        if (readWrite)
                                        {
                                            volume = oldCache.Content(ref p);
                                            cellMaterial = oldCache.Material(ref p);
                                        }

                                        var args = new MyVoxelBuilderArgs(voxelMap.Size, coords, cellMaterial, volume);

                                        try
                                        {
                                            func(args);
                                        }
                                        catch (Exception ex)
                                        {
                                            threadException = ex;
                                            break;
                                        }

                                        if (args.Volume != volume)
                                        {
                                            oldCache.Set(MyStorageDataTypeEnum.Content, ref p, args.Volume);
                                        }

                                        if (args.MaterialIndex != cellMaterial)
                                        {
                                            oldCache.Set(MyStorageDataTypeEnum.Material, ref p, args.MaterialIndex);
                                        }

                                        counter++;
                                        var prog = Math.Floor(counter / (decimal)counterTotal * 100);
                                        if (prog != progress)
                                        {
                                            progress = prog;
                                            Debug.Write($"{progress:000},");
                                        }

                                    }

                            voxelMap.Storage.WriteRange(oldCache, MyStorageDataTypeFlags.ContentAndMaterial, block, maxRange);
                        }

                #endregion
            }
            else
            {
                #region multi thread processing

                // TODO: re-write the multi thread processing to be more stable.
                // But still try and max out the processors.

                Vector3I block;
                const int cellSize = 64;
                var cacheSize = Vector3I.Min(new Vector3I(cellSize), voxelMap.Storage.Size);
                long threadCounter = counterTotal / cellSize / cellSize / cellSize;

                for (block.Z = 0; block.Z < voxelMap.Storage.Size.Z; block.Z += cellSize)
                    for (block.Y = 0; block.Y < voxelMap.Storage.Size.Y; block.Y += cellSize)
                        for (block.X = 0; block.X < voxelMap.Storage.Size.X; block.X += cellSize)
                        {
                            var oldCache = new MyStorageData();
                            oldCache.Resize(cacheSize);
                            // LOD1 is not detailed enough for content information on asteroids.
                            Vector3I maxRange = block + cacheSize - 1;
                            voxelMap.Storage.ReadRange(oldCache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, maxRange);

                            var task = new Task(obj =>
                            {
                                var bgw = (MyVoxelTaskWorker)obj;

                                Vector3I p;
                                for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                                    for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                        for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                        {
                                            var coords = bgw.BaseCoords + p;

                                            byte volume = 0x0;
                                            byte cellMaterial = materialIndex;

                                            if (readWrite)
                                            {
                                                // read the existing material and volume into the arguments before passing the to args for processing.
                                                volume = bgw.VoxelCache.Content(ref p);
                                                cellMaterial = bgw.VoxelCache.Material(ref p);
                                            }

                                            var args = new MyVoxelBuilderArgs(voxelMap.Size, coords, cellMaterial, volume);

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

                                            if (args.Volume != volume)
                                            {
                                                bgw.VoxelCache.Set(MyStorageDataTypeEnum.Content, ref p, args.Volume);
                                            }

                                            if (args.MaterialIndex != cellMaterial)
                                            {
                                                bgw.VoxelCache.Set(MyStorageDataTypeEnum.Material, ref p, args.MaterialIndex);
                                            }

                                            counter++;
                                            var prog = Math.Floor(counter / (decimal)counterTotal * 100);
                                            if (prog != progress)
                                            {
                                                progress = prog;
                                                Debug.Write($"{progress:000},");
                                            }
                                        }

                                lock (Locker)
                                {
                                    var b = bgw.BaseCoords;
                                    Vector3I mr = bgw.BaseCoords + cacheSize - 1;
                                    voxelMap.Storage.WriteRange(bgw.VoxelCache, MyStorageDataTypeFlags.ContentAndMaterial, b, mr);

                                    counter += (long)cellSize * cellSize * cellSize;
                                    var prog = Math.Floor(counter / (decimal)counterTotal * 100);
                                    if (prog != progress)
                                    {
                                        progress = prog;
                                        Debug.Write($"{progress:000},");
                                        Console.Write($"{progress:000},");
                                        GC.Collect();
                                    }
                                    threadCounter--;
                                }

                            }, new MyVoxelTaskWorker(block, oldCache));

                            task.Start();
                        }

              

                GC.Collect();

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

            voxelMap.RefreshAssets();

            //voxelMap.UpdateContentBounds();

            Debug.WriteLine($" Done. | {timer.Elapsed}  | VoxCells {voxelMap.VoxCells:#,##0}");
            Console.WriteLine($" Done. | {timer.Elapsed}  | VoxCells {voxelMap.VoxCells:#,##0}");
        }

        #endregion

        private static Vector3I CalcRequiredSize(int size)
        {
            // the size of 4x4x4 is too small. the game allows it, but the physics is broken.
            // So I'm restricting the smallest to 8x8x8.
            // All voxels are cubic, and powers of 2 in size.
            return new Vector3I(MathHelper.GetNearestBiggerPowerOfTwo(Math.Max(8, size)));
        }

        public static Vector3I CalcRequiredSize(Vector3I size)
        {
            // the size of 4x4x4 is too small. the game allows it, but the physics is broken.
            // So I'm restricting the smallest to 8x8x8.
            // All voxels are cubic, and powers of 2 in size.
            return new Vector3I(MathHelper.GetNearestBiggerPowerOfTwo(SpaceEngineersExtensions.Max(8, size.X, size.Y, size.Z)));
        }

        #endregion
    }
}
