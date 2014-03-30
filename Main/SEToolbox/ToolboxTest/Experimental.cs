namespace ToolboxTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Media3D;
    using VRageMath;

    /// <summary>
    /// For testing various ideas.
    /// </summary>
    [TestClass]
    public class Experimental
    {
        //[TestMethod]
        public void MemoryTest_NoOfMaterials()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\sphere_mix_large_365radi.vox";
            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Name);

            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);

            var assetNameCount = voxelMap.CountAssets(materialAssets);
        }

        //[TestMethod]
        public void VoxelGenerateSphereLargeHollow()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_hollow_350_10_radi.vox", 350, materials[0].Name, true, 10);    // 00:01:32.6580269 | VoxCells 3,801,278,432. | Is culled.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_hollow_355_10_radi.vox", 355, materials[0].Name, true, 10);    // 00:01:57.0029873 | VoxCells 3,912,545,848. | Is culled.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_hollow_356_10_radi.vox", 356, materials[0].Name, true, 10);    // 00:01:56.1288918 | VoxCells 3,935,432,712. |
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_hollow_357_10_radi.vox", 357, materials[0].Name, true, 10);    // 00:01:58.6494562 | VoxCells 3,957,704,936. | no reponse.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_hollow_358_10_radi.vox", 358, materials[0].Name, true, 10);    // 00:02:01.1396811 | VoxCells 3,980,026,368. | Crash
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_hollow_360_10_radi.vox", 360, materials[0].Name, true, 10);    // 00:02:00.2852452 | VoxCells 4,025,973,228. | >2mins, no response.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_hollow_370_10_radi.vox", 370, materials[0].Name, true, 10);    // 00:02:02.8729741 | VoxCells 4,255,404,456. |
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_hollow_382_10_radi.vox", 382, materials[0].Name, true, 10);    // 00:02:01.4779420 | VoxCells 4,540,873,728. | Won't load

            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_380_radi.vox", 380, materials[0].Name, false, 0);  // 00:01:37.7515161 | VoxCells 58,379,415,373. | culling. VoxCells 58,387,542,901
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_390_radi.vox", 390, materials[0].Name, false, 0);  // 00:02:07.8909152 | VoxCells 63,116,794,109. |
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_400_radi.vox", 400, materials[0].Name, false, 0);  // 00:02:05.2804642 | VoxCells 68,104,580,085. | culling. Alt Tab part works, then crash. VoxCells 68,104,580,085
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_450_radi.vox", 450, materials[0].Name, false, 0);  // works.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_460_radi.vox", 460, materials[0].Name, false, 0);  // works.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_465_radi.vox", 465, materials[0].Name, false, 0);  // 
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_470_radi.vox", 470, materials[0].Name, false, 0);  // works.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_471_radi.vox", 471, materials[0].Name, false, 0); // 00:03:41.2510726  | VoxCells 111,251,164,251 | Works.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_472_radi.vox", 472, materials[0].Name, false, 0); // 00:03:37.6897276  | VoxCells 111,961,704,705 | Works.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_473_radi.vox", 473, materials[0].Name, false, 0); // 00:03:40.2173012  | VoxCells 112,675,647,555 | Crash.
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_474_radi.vox", 474, materials[0].Name, false, 0); // 00:03:46.7690005  | VoxCells 113,392,787,817 | 
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_475_radi.vox", 475, materials[0].Name, false, 0);  // no response?
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_480_radi.vox", 480, materials[0].Name, false, 0);  // 
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_485_radi.vox", 485, materials[0].Name, false, 0);  // 
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_490_radi.vox", 490, materials[0].Name, false, 0);  // 
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_495_radi.vox", 495, materials[0].Name, false, 0);  // 
            //MyVoxelBuilder.BuildAsteroidSphere(true, @".\TestAssets\test_sphere_solid_500_radi.vox", 500, materials[0].Name, false, 0); // 00:04:43.9200838  | VoxCells 133,115,971,161. | Crash.
        }

        //[TestMethod]
        public void VoxelGenerateSphereAssets()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\sphere_hollow_316radi.vox";

            foreach (var material in materials)
            {
                var fileNewVoxel =
                    Path.Combine(Path.GetDirectoryName(Path.GetFullPath(fileOriginal)),
                        Path.GetFileNameWithoutExtension(fileOriginal) + "_" + material.Name + ".vox").ToLower();

                var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(true, fileNewVoxel, 316, material.Name, true, 10);
            }
        }

        //[TestMethod]
        public void VoxelPlanetSurfaceMapper()
        {
            // As of update 01.021.024, Helium_01, Helium_02, Ice_01 no longer exist.

            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Name.Contains("Stone_05"));
            var goldMaterial = materials.FirstOrDefault(m => m.Name.Contains("Gold"));
            var ironMaterial = materials.FirstOrDefault(m => m.Name.Contains("Iron"));
            var heliumMaterial = materials.FirstOrDefault(m => m.Name.Contains("Helium"));
            Assert.IsNotNull(heliumMaterial, "Helium material should exist.");
            var iceMaterial = materials.FirstOrDefault(m => m.Name.Contains("Ice"));
            Assert.IsNotNull(iceMaterial, "Ice material should exist.");

            var imageFile = @".\TestAssets\Earth.png";
            //var fileNew = @".\TestAssets\Earth.vox";
            //fileNew = @"C:\Users\Christopher\AppData\Roaming\SpaceEngineers\Saves\76561197961224864\Builder Toolset\Earth0.vox";
            double radius = 300;

            var length = MyVoxelBuilder.ScaleMod((radius * 2) + 2, 64);
            var size = new Vector3I(length, length, length);
            var origin = new Vector3I(size.X / 2, size.Y / 2, size.Z / 2);

            var bmp = ToolboxExtensions.OptimizeImagePalette(imageFile);
            var palatteNames = ToolboxExtensions.GetPalatteNames();

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                double x = e.CoordinatePoint.X - origin.X;
                double y = e.CoordinatePoint.Y - origin.Y;
                double z = e.CoordinatePoint.Z - origin.Z;

                var dist = Math.Sqrt(Math.Abs(x * x) + Math.Abs(y * y) + Math.Abs(z * z));
                if (dist >= radius)
                {
                    e.Volume = 0x00;
                }
                else if (dist > radius - 1)
                {
                    e.Volume = (byte)((radius - dist) * 255);
                    var point = ToolboxExtensions.Cartesian3DToSphericalPlanar(x, y, z, dist, bmp.Width, bmp.Height);
                    if (point.HasValue)
                    {
                        var color = bmp.GetPixel(point.Value.X, point.Value.Y);
                        var materialColor = palatteNames.FirstOrDefault(c => c.Key.A == color.A && c.Key.R == color.R && c.Key.G == color.G && c.Key.B == color.B);
                        if (materialColor.Value == "" || materialColor.Value == "White")
                        {
                            e.Material = iceMaterial.Name;
                        }
                        if (materialColor.Value == "Blue")
                        {
                            e.Material = heliumMaterial.Name;
                        }
                        //else
                        //{
                        //    e.Material = ironMaterial.Name;
                        //}
                    }
                }
                else
                {
                    e.Volume = 0xFF;
                    var point = ToolboxExtensions.Cartesian3DToSphericalPlanar(x, y, z, dist, bmp.Width, bmp.Height);
                    if (point.HasValue)
                    {
                        var color = bmp.GetPixel(point.Value.X, point.Value.Y);
                        var materialColor = palatteNames.FirstOrDefault(c => c.Key.A == color.A && c.Key.R == color.R && c.Key.G == color.G && c.Key.B == color.B);
                        if (materialColor.Value == "" || materialColor.Value == "White")
                        {
                            e.Material = iceMaterial.Name;
                        }
                        if (materialColor.Value == "Blue")
                        {
                            e.Material = heliumMaterial.Name;
                        }
                        //else
                        //{
                        //    e.Material = ironMaterial.Name;
                        //}
                    }
                }
            };

            // TODO: Get the surface calculations working right.
            //var voxelMap = MyVoxelBuilder.BuildAsteroid(true, fileNew, size, stoneMaterial.Name, action);

            //var m1 = ToolboxExtensions.min;
            //var m2 = ToolboxExtensions.max;
        }

        //[TestMethod]
        public void VoxelGenerateMaterialVolume()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Name.Contains("Stone_01"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");
            var goldMaterial = materials.FirstOrDefault(m => m.Name.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");


            var fileNew = string.Format(@".\TestAssets\test_cube_solid_3x3x3_stone_{0}.vox", goldMaterial.Name);
            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(false, fileNew, 3, 3, 3, stoneMaterial.Name, false, 0);

            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);

            //materialAssets[13] = SpaceEngineersAPI.GetMaterialIndex(goldMaterial.Name);

            //voxelMap.SetMaterialAssets(materialAssets);

            fileNew = @"C:\Users\Christopher\AppData\Roaming\SpaceEngineers\Saves\76561197961224864\Builder Toolset\test_cube_solid_3x3x3_stone_gold_010.vox";
            voxelMap.Save(fileNew);

            // Stone_05:	27 m³ =  558.078 L  =  1,508.32 Kg  == 55.9 Kg per cubic metre.
            // Stone_01:	 8 m³ =  224.078 L  =    605.62 Kg  == 75.7 Kg per cubic metre.
            // Stone_01:	27 m³ =  538.235 L  =  1,454.69 Kg  == 53.9 Kg per cubic metre.


            var lengthNew = new FileInfo(fileNew).Length;

            //Assert.AreEqual(146, lengthNew, "New file size must match.");

            //Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            //Assert.AreEqual(64, voxelMap.Size.Y, "Voxel Bounding size must match.");
            //Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            //Assert.AreEqual(8, voxelMap.ContentSize.X, "Voxel Content size must match.");
            //Assert.AreEqual(8, voxelMap.ContentSize.Y, "Voxel Content size must match.");
            //Assert.AreEqual(8, voxelMap.ContentSize.Z, "Voxel Content size must match.");

            //// Centered in the middle of 1 and 8.   1234-(4.5)-5678
            //Assert.AreEqual(4.5, voxelMap.ContentCenter.X, "Voxel Center must match.");
            //Assert.AreEqual(4.5, voxelMap.ContentCenter.Y, "Voxel Center must match.");
            //Assert.AreEqual(4.5, voxelMap.ContentCenter.Z, "Voxel Center must match.");
        }

        //[TestMethod]
        public void BuildAsteroidSphereTestLayer()
        {
            const bool multiThread = true;
            const string filename = "sphere_ore_layer_365radi.vox";
            const double radius = 365;

            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var materialStone_01 = materials.FirstOrDefault(m => m.Name.Contains("Stone_01"));
            Assert.IsNotNull(materialStone_01, "Stone_01 material should exist.");

            var materialStone_02 = materials.FirstOrDefault(m => m.Name.Contains("Stone_02"));
            Assert.IsNotNull(materialStone_02, "Stone_02 material should exist.");

            var materialStone_03 = materials.FirstOrDefault(m => m.Name.Contains("Stone_03"));
            Assert.IsNotNull(materialStone_03, "Stone_03 material should exist.");

            var materialStone_04 = materials.FirstOrDefault(m => m.Name.Contains("Stone_04"));
            Assert.IsNotNull(materialStone_04, "Stone_04 material should exist.");

            var materialStone_05 = materials.FirstOrDefault(m => m.Name.Contains("Stone_05"));
            Assert.IsNotNull(materialStone_05, "Stone_05 material should exist.");

            var materialIron_01 = materials.FirstOrDefault(m => m.Name.Contains("Iron_01"));
            Assert.IsNotNull(materialIron_01, "Iron_01 material should exist.");

            var materialIron_02 = materials.FirstOrDefault(m => m.Name.Contains("Iron_02"));
            Assert.IsNotNull(materialIron_02, "Iron_02 material should exist.");

            var materialNickel_01 = materials.FirstOrDefault(m => m.Name.Contains("Nickel_01"));
            Assert.IsNotNull(materialNickel_01, "Nickel_01 material should exist.");

            var materialCobalt_01 = materials.FirstOrDefault(m => m.Name.Contains("Cobalt_01"));
            Assert.IsNotNull(materialCobalt_01, "Cobalt_01 material should exist.");

            var materialMagnesium_01 = materials.FirstOrDefault(m => m.Name.Contains("Magnesium_01"));
            Assert.IsNotNull(materialMagnesium_01, "Magnesium_01 material should exist.");

            var materialSilicon_01 = materials.FirstOrDefault(m => m.Name.Contains("Silicon_01"));
            Assert.IsNotNull(materialSilicon_01, "Silicon_01 material should exist.");

            var materialSilver_01 = materials.FirstOrDefault(m => m.Name.Contains("Silver_01"));
            Assert.IsNotNull(materialSilver_01, "Silver_01 material should exist.");

            var materialGold_01 = materials.FirstOrDefault(m => m.Name.Contains("Gold_01"));
            Assert.IsNotNull(materialGold_01, "Gold_01 material should exist.");

            var materialPlatinum_01 = materials.FirstOrDefault(m => m.Name.Contains("Platinum_01"));
            Assert.IsNotNull(materialPlatinum_01, "Platinum_01 material should exist.");

            var materialUraninite_01 = materials.FirstOrDefault(m => m.Name.Contains("Uraninite_01"));
            Assert.IsNotNull(materialUraninite_01, "Uraninite_01 material should exist.");

            var length = MyVoxelBuilder.ScaleMod((radius * 2) + 2, 64);
            var size = new Vector3I(length, length, length);
            var origin = new Vector3I(size.X / 2, size.Y / 2, size.Z / 2);
            const int layerDepth = 22;

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                if (Math.Abs(e.CoordinatePoint.X - origin.X) < 2
                    && Math.Abs(e.CoordinatePoint.Y - origin.Y) < 2
                    && e.CoordinatePoint.Z <= origin.Z)
                {
                    e.Volume = 0x00;
                    return;
                }

                var dist =
                    Math.Sqrt(Math.Abs(Math.Pow(e.CoordinatePoint.X - origin.X, 2)) +
                              Math.Abs(Math.Pow(e.CoordinatePoint.Y - origin.Y, 2)) +
                              Math.Abs(Math.Pow(e.CoordinatePoint.Z - origin.Z, 2)));

                if (dist < 5)
                {
                    e.Volume = 0x00;
                }
                else if (dist >= radius)
                {
                    e.Volume = 0x00;
                }
                else if (dist > radius - 1)
                {
                    e.Volume = (byte)((radius - dist) * 255);
                }
                else if ((radius - (1 * layerDepth)) < dist)
                {
                    // surface.. default  material.
                }
                else if ((radius - (2 * layerDepth)) < dist)
                {
                    e.Material = materialStone_02.Name;
                }
                else if ((radius - (3 * layerDepth)) < dist)
                {
                    e.Material = materialStone_03.Name;
                }
                else if ((radius - (4 * layerDepth)) < dist)
                {
                    e.Material = materialStone_04.Name;
                }
                else if ((radius - (5 * layerDepth)) < dist)
                {
                    e.Material = materialStone_05.Name;
                }
                else if ((radius - (6 * layerDepth)) < dist)
                {
                    e.Material = materialIron_01.Name;
                }
                else if ((radius - (7 * layerDepth)) < dist)
                {
                    e.Material = materialIron_02.Name;
                }
                else if ((radius - (8 * layerDepth)) < dist)
                {
                    e.Material = materialNickel_01.Name;
                }
                else if ((radius - (9 * layerDepth)) < dist)
                {
                    e.Material = materialCobalt_01.Name;
                }
                else if ((radius - (10 * layerDepth)) < dist)
                {
                    e.Material = materialMagnesium_01.Name;
                }
                else if ((radius - (11 * layerDepth)) < dist)
                {
                    e.Material = materialSilicon_01.Name;
                }
                else if ((radius - (12 * layerDepth)) < dist)
                {
                    e.Material = materialSilver_01.Name;
                }
                else if ((radius - (13 * layerDepth)) < dist)
                {
                    e.Material = materialGold_01.Name;
                }
                else if ((radius - (14 * layerDepth)) < dist)
                {
                    e.Material = materialPlatinum_01.Name;
                }
                else if ((radius - (15 * layerDepth)) < dist)
                {
                    e.Material = materialUraninite_01.Name;
                }
                else if ((radius - (16 * layerDepth)) < dist)
                {
                    e.Material = materialStone_01.Name;
                }
            };

            var voxelMap = MyVoxelBuilder.BuildAsteroid(multiThread, filename, size, materialStone_01.Name, action);
        }

    }
}
