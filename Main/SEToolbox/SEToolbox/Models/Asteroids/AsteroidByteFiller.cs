namespace SEToolbox.Models.Asteroids
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Support;

    public class AsteroidByteFiller : IMyVoxelFiller
    {
        public IMyVoxelFillProperties CreateRandom(int index, MaterialSelectionModel defaultMaterial, IEnumerable<MaterialSelectionModel> materialsCollection, IEnumerable<GenerateVoxelDetailModel> voxelCollection)
        {
            int idx;

            var randomModel = new AsteroidByteFillProperties
            {
                Index = index,
                MainMaterial = defaultMaterial,
                SecondMaterial = defaultMaterial,
                ThirdMaterial = defaultMaterial,
                FourthMaterial = defaultMaterial,
                FifthMaterial = defaultMaterial,
                SixthMaterial = defaultMaterial,
                SeventhMaterial = defaultMaterial,
            };

            // Must be by reference, not value.
            var largeVoxelFileList = voxelCollection.Where(v => v.FileSize > 100000).ToList();
            var smallVoxelFileList = voxelCollection.Where(v => v.FileSize > 0 && v.FileSize < 100000).ToList();

            // Random Asteroid.
            var d = RandomUtil.GetDouble(1, 100);
            var islarge = false;

            if (largeVoxelFileList.Count == 0 && smallVoxelFileList.Count == 0)
            {
                // no asteroids?  You are so screwed.
                throw new Exception("No valid asteroids found. Re-validate your game cache.");
            }

            if (largeVoxelFileList.Count == 0) // empty last list? Force to small list.
                d = 1;
            if (smallVoxelFileList.Count == 0) // empty small list? Force to large list.
                d = 100;

            if (d > 70)
            {
                idx = RandomUtil.GetInt(largeVoxelFileList.Count);
                randomModel.VoxelFile = largeVoxelFileList[idx];
                islarge = true;
            }
            else
            {
                idx = RandomUtil.GetInt(smallVoxelFileList.Count);
                randomModel.VoxelFile = smallVoxelFileList[idx];
            }

            // Random Main Material non-Rare.
            var nonRare = materialsCollection.Where(m => m.IsRare == false).ToArray();
            idx = RandomUtil.GetInt(nonRare.Length);
            randomModel.MainMaterial = nonRare[idx];

            int percent;
            var rare = materialsCollection.Where(m => m.IsRare && m.MinedRatio >= 2).ToList();
            var superRare = materialsCollection.Where(m => m.IsRare && m.MinedRatio < 2).ToList();

            if (islarge)
            {
                // Random 1. Rare.
                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    percent = RandomUtil.GetInt(40, 60);
                    randomModel.SecondMaterial = rare[idx];
                    randomModel.SecondPercent = percent;
                    rare.RemoveAt(idx);
                }

                // Random 2. Rare.
                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    percent = RandomUtil.GetInt(6, 12);
                    randomModel.ThirdMaterial = rare[idx];
                    randomModel.ThirdPercent = percent;
                    rare.RemoveAt(idx);
                }

                // Random 3. Rare.
                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    percent = RandomUtil.GetInt(6, 12);
                    randomModel.ThirdMaterial = rare[idx];
                    randomModel.ThirdPercent = percent;
                    rare.RemoveAt(idx);
                }

                // Random 4. Super Rare.
                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    percent = RandomUtil.GetInt(2, 4);
                    randomModel.FourthMaterial = superRare[idx];
                    randomModel.FourthPercent = percent;
                    superRare.RemoveAt(idx);
                }

                // Random 5. Super Rare.
                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    percent = RandomUtil.GetInt(1, 3);
                    randomModel.FifthMaterial = superRare[idx];
                    randomModel.FifthPercent = percent;
                    superRare.RemoveAt(idx);
                }

                // Random 6. Super Rare.
                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    percent = RandomUtil.GetInt(1, 3);
                    randomModel.SixthMaterial = superRare[idx];
                    randomModel.SixthPercent = percent;
                    superRare.RemoveAt(idx);
                }

                // Random 7. Super Rare.
                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    percent = RandomUtil.GetInt(1, 3);
                    randomModel.SeventhMaterial = superRare[idx];
                    randomModel.SeventhPercent = percent;
                    superRare.RemoveAt(idx);
                }
            }
            else // Small Asteroid.
            {
                // Random 1. Rare.
                idx = RandomUtil.GetInt(rare.Count);
                percent = RandomUtil.GetInt(6, 13);
                randomModel.SecondMaterial = rare[idx];
                randomModel.SecondPercent = percent;

                // Random 2. Super Rare.
                idx = RandomUtil.GetInt(superRare.Count);
                percent = RandomUtil.GetInt(2, 4);
                randomModel.ThirdMaterial = superRare[idx];
                randomModel.ThirdPercent = percent;
                superRare.RemoveAt(idx);
            }

            return randomModel;
        }

        public void FillAsteroid(MyVoxelMap asteroid, IMyVoxelFillProperties fillProperties)
        {
            var properties = (AsteroidByteFillProperties)fillProperties;

            IList<byte> baseAssets = asteroid.CalcVoxelMaterialList();

            var distribution = new List<double> { double.NaN };
            var materialSelection = new List<byte> { SpaceEngineersCore.Resources.GetMaterialIndex(properties.MainMaterial.Value) };

            if (properties.SecondPercent > 0)
            {
                distribution.Add((double)properties.SecondPercent / 100);
                materialSelection.Add(SpaceEngineersCore.Resources.GetMaterialIndex(properties.SecondMaterial.Value));
            }
            if (properties.ThirdPercent > 0)
            {
                distribution.Add((double)properties.ThirdPercent / 100);
                materialSelection.Add(SpaceEngineersCore.Resources.GetMaterialIndex(properties.ThirdMaterial.Value));
            }
            if (properties.FourthPercent > 0)
            {
                distribution.Add((double)properties.FourthPercent / 100);
                materialSelection.Add(SpaceEngineersCore.Resources.GetMaterialIndex(properties.FourthMaterial.Value));
            }
            if (properties.FifthPercent > 0)
            {
                distribution.Add((double)properties.FifthPercent / 100);
                materialSelection.Add(SpaceEngineersCore.Resources.GetMaterialIndex(properties.FifthMaterial.Value));
            }
            if (properties.SixthPercent > 0)
            {
                distribution.Add((double)properties.SixthPercent / 100);
                materialSelection.Add(SpaceEngineersCore.Resources.GetMaterialIndex(properties.SixthMaterial.Value));
            }
            if (properties.SeventhPercent > 0)
            {
                distribution.Add((double)properties.SeventhPercent / 100);
                materialSelection.Add(SpaceEngineersCore.Resources.GetMaterialIndex(properties.SeventhMaterial.Value));
            }

            var newDistributiuon = new List<byte>();
            int count;
            for (var i = 1; i < distribution.Count(); i++)
            {
                count = (int)Math.Floor(distribution[i] * baseAssets.Count); // Round down.
                for (var j = 0; j < count; j++)
                {
                    newDistributiuon.Add(materialSelection[i]);
                }
            }
            count = baseAssets.Count - newDistributiuon.Count;
            for (var j = 0; j < count; j++)
            {
                newDistributiuon.Add(materialSelection[0]);
            }

            newDistributiuon.Shuffle();
            asteroid.SetVoxelMaterialList(newDistributiuon);
            //asteroid.ForceVoxelFaceMaterial(_dataModel.BaseMaterial.DisplayName);
        }
    }
}
