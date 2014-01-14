using System;

namespace ToolboxTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void GenreateTempFiles()
        {
            for (var i = 0; i < 10; i++)
            {
                var file1 = TempfileUtil.NewFilename(null);
                File.WriteAllBytes(file1, new byte[] { 0x00, 0x01, 0x02 });

                var file2 = TempfileUtil.NewFilename(".txt");
                File.WriteAllText(file2, "blah blah");
            }

            TempfileUtil.Dispose();
        }

        [TestMethod]
        public void TestImageOptimizer1()
        {
            var filename = Path.GetFullPath(@".\TestAssets\7242630_orig.jpg");
            var bmp = ToolboxExtensions.OptimizeImagePalette(filename);
            var outputFileTest = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + "_optimized" + ".png");
            bmp.Save(outputFileTest, ImageFormat.Png);
        }

        [TestMethod]
        public void TestImageOptimizer2()
        {
            var filename = Path.GetFullPath(@".\TestAssets\7242630_scale432.png");
            var bmp = ToolboxExtensions.OptimizeImagePalette(filename);
            var outputFileTest = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + "_optimized" + ".png");
            bmp.Save(outputFileTest, ImageFormat.Png);
        }

        [TestMethod]
        public void TestXmlCompacter1()
        {
            var filenameSource = Path.GetFullPath(@".\TestAssets\test.xml");
            var filenameDestination = Path.GetFullPath(@".\TestAssets\test_out.xml");
            ToolboxExtensions.CompactXmlFile(filenameSource, filenameDestination);

            var oldFileSize = new FileInfo(filenameSource).Length;
            var newFileSize = new FileInfo(filenameDestination).Length;

            Assert.IsTrue(newFileSize < oldFileSize, "new file size must be smaller");
            Assert.AreEqual(2225, oldFileSize, "original file size");
            Assert.AreEqual(1510, newFileSize, "new file size");
        }

        [TestMethod]
        public void LocateSpaceEngineersApplication()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "SpaceEgineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");
        }


        [TestMethod]
        public void VoxelCompression()
        {
            var fileOriginal = @".\TestAssets\asteroid0moon4.vox";
            var fileExtracted = @".\TestAssets\asteroid0moon4.vox.bin";
            var fileNew = @".\TestAssets\asteroid0moon4_test.vox";
            MyVoxelMap.Uncompress(fileOriginal, fileExtracted);
            MyVoxelMap.Compress(fileExtracted, fileNew);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthExtracted = new FileInfo(fileExtracted).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(9428, lengthOriginal, "File size must match.");
            Assert.AreEqual(310276, lengthExtracted, "File size must match.");
            Assert.AreEqual(9428, lengthNew, "File size must match.");
        }

        [TestMethod]
        public void VoxelMaterials()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();

            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");
        }

        [TestMethod]
        public void VoxelLoadSave()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var fileOriginal = @".\TestAssets\asteroid0moon4.vox";
            var fileNew = @".\TestAssets\asteroid0moon4_save.vox";

            var voxelMap = new MyVoxelMap();

            voxelMap.Load(fileOriginal, materials[0].Name);
            voxelMap.Save(fileNew);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(9428, lengthOriginal, "File size must match.");
            Assert.AreEqual(9428, lengthNew, "File size must match.");
        }

        [TestMethod]
        public void VoxelDetails()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var fileOriginal = @".\TestAssets\DeformedSphereWithHoles_64x128x64.vox";

            var voxelMap = new MyVoxelMap();

            voxelMap.Load(fileOriginal, materials[0].Name);
            var voxCells = voxelMap.SumVoxelCells();

            Assert.AreEqual("DeformedSphereWithHoles_64x128x64", voxelMap.DisplayName, "Voxel Name must match.");

            Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(128, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(48, voxelMap.ContentSize.X, "Voxel Content size must match.");
            Assert.AreEqual(112, voxelMap.ContentSize.Y, "Voxel Content size must match.");
            Assert.AreEqual(48, voxelMap.ContentSize.Z, "Voxel Content size must match.");

            Assert.AreEqual(30909925, voxCells, "Voxel cells must match.");
        }

        [TestMethod]
        public void VoxelMaterialIndexes()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            for (byte i = 0; i < materials.Count; i++)
            {
                Assert.AreEqual(i, SpaceEngineersAPI.GetMaterialIndex(materials[i].Name), "Material index should equal original.");
            }

            Assert.AreEqual(0xFF, SpaceEngineersAPI.GetMaterialIndex("blaggg"), "Material index should not exist.");
        }

        [TestMethod]
        public void VoxelMaterialChanges()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Name.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var fileOriginal = @".\TestAssets\asteroid0moon4.vox";
            var fileNew = @".\TestAssets\asteroid0moon4_gold.vox";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Name);
            voxelMap.ForceBaseMaterial(goldMaterial.Name);
            voxelMap.Save(fileNew);

            // or call...
            //MyVoxelBuilder.ConvertAsteroid(fileOriginal, fileNew, goldMaterial.Name);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(9428, lengthOriginal, "Original file size must match.");
            Assert.AreEqual(7008, lengthNew, "New file size must match.");
        }

        [TestMethod]
        public void VoxelMaterialAssets_MixedGeneratedAsset()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Name.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var fileOriginal = @".\TestAssets\asteroid0moon4.vox";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Name);
            var materialAssets = voxelMap.CalculateMaterialAssets();

            Assert.AreEqual(44135, materialAssets.Count, "Asset count should be equal.");

            var otherAssets = materialAssets.Where(c => c != 4).ToList();

            Assert.AreEqual(0, otherAssets.Count, "Other Asset count should be equal.");

            var assetNameCount = SpaceEngineersAPI.CountAssets(materialAssets);
        }

        [TestMethod]
        public void VoxelMaterialAssets_FixedSize()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Name.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var fileOriginal = @".\TestAssets\test_cube2x2x2.vox";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Name);
            var materialAssets = voxelMap.CalculateMaterialAssets();

            Assert.AreEqual(8, materialAssets.Count, "Asset count should be equal.");

            var stoneAssets = materialAssets.Where(c => c == 1).ToList();

            Assert.AreEqual(8, stoneAssets.Count, "Stone Asset count should be equal.");

            var assetNameCount = SpaceEngineersAPI.CountAssets(materialAssets);
        }

        [TestMethod]
        public void VoxelMaterialAssets_FixedSize_MixedContent()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Name.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var fileOriginal = @".\TestAssets\test_cube_mixed_2x2x2.vox";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Name);
            var materialAssets = voxelMap.CalculateMaterialAssets();

            Assert.AreEqual(8, materialAssets.Count, "Asset count should be equal.");

            var assetNameCount = SpaceEngineersAPI.CountAssets(materialAssets);

            Assert.AreEqual(8, assetNameCount.Count, "Asset Mertials count should be equal.");
        }

        [TestMethod]
        public void VoxelMaterialAssetsRandom()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            //var goldMaterial = materials.FirstOrDefault(m => m.Name.Contains("Gold"));
            //Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var fileOriginal = @".\TestAssets\Arabian_Border_7.vox";
            var fileNewVoxel = @".\TestAssets\Arabian_Border_7_mixed.vox";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Name);
            var materialAssets = voxelMap.CalculateMaterialAssets();

            Assert.AreEqual(35465, materialAssets.Count, "Asset count should be equal.");

            var distribution = new double[] {Double.NaN, .5, .25};
            var materialSelection = new byte[] {6, 15, 17};  //Helium, Gold, Uranium


            var newDistributiuon = new List<byte>();
            int count;
            for (var i = 1; i < distribution.Count(); i++)
            {
                count = (int)Math.Floor(distribution[i] * materialAssets.Count); // Round down.
                for (var j = 0; j < count; j++)
                {
                    newDistributiuon.Add(materialSelection[i]);
                }
            }
            count = materialAssets.Count - newDistributiuon.Count;
            for (var j = 0; j < count; j++)
            {
                newDistributiuon.Add(materialSelection[0]);
            }

            newDistributiuon.Shuffle();

            var assetNameCount = SpaceEngineersAPI.CountAssets(newDistributiuon);

            Assert.AreEqual(3, assetNameCount.Count, "Asset Mertials count should be equal.");
            Assert.AreEqual(8867, assetNameCount[materials[6].Name], "Asset Mertials count should be equal.");
            Assert.AreEqual(17732, assetNameCount[materials[15].Name], "Asset Mertials count should be equal.");
            Assert.AreEqual(8866, assetNameCount[materials[17].Name], "Asset Mertials count should be equal.");

            voxelMap.SetMaterialAssets(newDistributiuon);
            voxelMap.Save(fileNewVoxel);
        }

        //[TestMethod]
        //public void MemoryTest_NoMaterials()
        //{
        //    var materials = SpaceEngineersAPI.GetMaterialList();
        //    Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

        //    var fileOriginal = @".\TestAssets\sphere_mix_large_365radi.vox";
        //    var voxelMap = new MyVoxelMap();
        //    voxelMap.Load(fileOriginal, materials[0].Name);

        //    var materialAssets = voxelMap.CalculateMaterialAssets();
        //    var assetNameCount = SpaceEngineersAPI.CountAssets(materialAssets);
        //}

        [TestMethod]
        public void VoxelGenerateBoxSmall()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Name.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var fileNew = @".\TestAssets\test_cube_solid_8x8x8_gold.vox";

            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(false, fileNew, 8, 8, 8, goldMaterial.Name, false, 0);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(146, lengthNew, "New file size must match.");

            Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(8, voxelMap.ContentSize.X, "Voxel Content size must match.");
            Assert.AreEqual(8, voxelMap.ContentSize.Y, "Voxel Content size must match.");
            Assert.AreEqual(8, voxelMap.ContentSize.Z, "Voxel Content size must match.");

            // Centered in the middle of 1 and 8.   1234-(4.5)-5678
            Assert.AreEqual(4.5, voxelMap.ContentCenter.X, "Voxel Center must match.");
            Assert.AreEqual(4.5, voxelMap.ContentCenter.Y, "Voxel Center must match.");
            Assert.AreEqual(4.5, voxelMap.ContentCenter.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateSphereSmall()
        {
            var materials = SpaceEngineersAPI.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Name.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var fileNew = @".\TestAssets\test_sphere_solid_7_gold.vox";

            var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(false, fileNew, 4, goldMaterial.Name, false, 0);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(287, lengthNew, "New file size must match.");

            Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(7, voxelMap.ContentSize.X, "Voxel Content size must match.");
            Assert.AreEqual(7, voxelMap.ContentSize.Y, "Voxel Content size must match.");
            Assert.AreEqual(7, voxelMap.ContentSize.Z, "Voxel Content size must match.");

            // Centered in the middle of the 64x64x64 cell.
            Assert.AreEqual(32, voxelMap.ContentCenter.X, "Voxel Center must match.");
            Assert.AreEqual(32, voxelMap.ContentCenter.Y, "Voxel Center must match.");
            Assert.AreEqual(32, voxelMap.ContentCenter.Z, "Voxel Center must match.");
        }
    }
}
