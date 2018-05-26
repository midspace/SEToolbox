namespace SEToolbox.Models.Asteroids
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;

    public class AsteroidSeedFiller : IMyVoxelFiller
    {
        public IMyVoxelFillProperties CreateRandom(int index, MaterialSelectionModel defaultMaterial, IEnumerable<MaterialSelectionModel> materialsCollection, IEnumerable<GenerateVoxelDetailModel> voxelCollection)
        {
            int idx;

            var randomModel = new AsteroidSeedFillProperties
            {
                Index = index,
                MainMaterial = defaultMaterial,
                FirstMaterial = defaultMaterial,
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

            int chunks, chunkSize;
            double multiplier = 1.0;
            var rare = materialsCollection.Where(m => m.IsRare && m.MinedRatio >= 2).ToList();
            var superRare = materialsCollection.Where(m => m.IsRare && m.MinedRatio < 2).ToList();

            if (islarge)
            {
                // Random 1-4 are rare.
                chunks = 20;
                chunkSize = 5;

                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    randomModel.FirstMaterial = rare[idx];
                    randomModel.FirstVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.FirstRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    rare.RemoveAt(idx);
                }
                multiplier *= 0.85;

                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    randomModel.SecondMaterial = rare[idx];
                    randomModel.SecondVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.SecondRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    rare.RemoveAt(idx);
                }
                multiplier *= 0.85;

                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    randomModel.ThirdMaterial = rare[idx];
                    randomModel.ThirdVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.ThirdRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    rare.RemoveAt(idx);
                }
                multiplier *= 0.85;

                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    randomModel.FourthMaterial = rare[idx];
                    randomModel.FourthVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.FourthRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    rare.RemoveAt(idx);
                }

                // Random 5-7 are super-rare

                multiplier = 1.0;
                chunks = 50;
                chunkSize = 2;

                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    randomModel.FifthMaterial = superRare[idx];
                    randomModel.FifthVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.FifthRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    superRare.RemoveAt(idx);
                }
                multiplier *= 0.5;

                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    randomModel.SixthMaterial = superRare[idx];
                    randomModel.SixthVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.SixthRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    superRare.RemoveAt(idx);
                }
                multiplier *= 0.5;

                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    randomModel.SeventhMaterial = superRare[idx];
                    randomModel.SeventhVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.SeventhRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    superRare.RemoveAt(idx);
                }
                multiplier *= 0.5;

            }
            else // Small Asteroid.
            {

                // Random 1-3 are rare.
                chunks = 10;
                chunkSize = 2;

                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    randomModel.FirstMaterial = rare[idx];
                    randomModel.FirstVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.FirstRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    rare.RemoveAt(idx);
                }
                multiplier *= 0.75;

                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    randomModel.SecondMaterial = rare[idx];
                    randomModel.SecondVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.SecondRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    rare.RemoveAt(idx);
                }
                multiplier *= 0.75;

                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    randomModel.ThirdMaterial = rare[idx];
                    randomModel.ThirdVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.ThirdRadius = RandomUtil.GetInt((int)(chunkSize * multiplier), (int)(chunkSize * 1.5 * multiplier));
                    rare.RemoveAt(idx);
                }


                // Random 4-5 is super-rare

                multiplier = 1.0;
                chunks = 15;

                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    randomModel.FourthMaterial = superRare[idx];
                    randomModel.FourthVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.FourthRadius = 0;
                    superRare.RemoveAt(idx);
                }
                multiplier *= 0.5;

                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    randomModel.FifthMaterial = superRare[idx];
                    randomModel.FifthVeins = RandomUtil.GetInt((int)(chunks * multiplier), (int)(chunks * 1.5 * multiplier));
                    randomModel.FifthRadius = 0;
                    superRare.RemoveAt(idx);
                }
            }

            return randomModel;
        }

        public void FillAsteroid(MyVoxelMap asteroid, IMyVoxelFillProperties fillProperties)
        {
            var properties = (AsteroidSeedFillProperties)fillProperties;

            /* The full history behind this hack/crutch eludes me.
                 * There are roids that won't change their materials unless their face materials forced to something other than current value.
                 * So we have to do that manually by setting to a usually unused ore (uranium) and then reverting to the one we chose (=old one in case of a flaky roid)
                 */
            //byte oldMaterial = asteroid.VoxelMaterial;

            // ForceVoxelFaceMaterial should no longer be required.
            //asteroid.ForceVoxelFaceMaterial("Uraninite_01");
            //asteroid.ForceVoxelFaceMaterial(properties.MainMaterial.Value);

            // Cycle through veins info and add 'spherical' depisits to the voxel cell grid (not voxels themselves)
            int i;

            if (properties.FirstVeins > 0)
                for (i = 0; i < properties.FirstVeins; i++)
                    asteroid.SeedMaterialSphere(properties.FirstMaterial.MaterialIndex.Value, (byte)properties.FirstRadius);

            if (properties.SecondVeins > 0)
                for (i = 0; i < properties.SecondVeins; i++)
                    asteroid.SeedMaterialSphere(properties.SecondMaterial.MaterialIndex.Value, (byte)properties.SecondRadius);

            if (properties.ThirdVeins > 0)
                for (i = 0; i < properties.ThirdVeins; i++)
                    asteroid.SeedMaterialSphere(properties.ThirdMaterial.MaterialIndex.Value, (byte)properties.ThirdRadius);

            if (properties.FourthVeins > 0)
                for (i = 0; i < properties.FourthVeins; i++)
                    asteroid.SeedMaterialSphere(properties.FourthMaterial.MaterialIndex.Value, (byte)properties.FourthRadius);

            if (properties.FifthVeins > 0)
                for (i = 0; i < properties.FifthVeins; i++)
                    asteroid.SeedMaterialSphere(properties.FifthMaterial.MaterialIndex.Value, (byte)properties.FifthRadius);

            if (properties.SixthVeins > 0)
                for (i = 0; i < properties.SixthVeins; i++)
                    asteroid.SeedMaterialSphere(properties.SixthMaterial.MaterialIndex.Value, (byte)properties.SixthRadius);

            if (properties.SeventhVeins > 0)
                for (i = 0; i < properties.SeventhVeins; i++)
                    asteroid.SeedMaterialSphere(properties.SeventhMaterial.MaterialIndex.Value, (byte)properties.SeventhRadius);

            // Hide the surface materials up to depth of 2 cells.
            asteroid.ForceShellMaterial(properties.MainMaterial.Value, 2);

            // This recovers material assigning ability for most roids (could be something specific to indestructibleContent property?)
            // And not for all, apparently :(
            //asteroid.ForceVoxelFaceMaterial(_dataModel.BaseMaterial.DisplayName); // don't change mattype

            // doesn't help
            //asteroid.ForceIndestructibleContent(0xff);

            // Alt ends     
        }
    }
}
