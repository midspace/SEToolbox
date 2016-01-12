namespace ToolboxTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models.Asteroids;
    using SEToolbox.Support;
    using VRageMath;

    [TestClass]
    public class VoxelTests
    {
        [TestMethod]
        public void VoxelCompressionV1()
        {
            const string fileOriginal = @".\TestAssets\asteroid0moon4.vox";
            const string fileExtracted = @".\TestOutput\asteroid0moon4.vox.bin";
            const string fileNew = @".\TestOutput\asteroid0moon4_test.vox";
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
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();

            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");
        }

        [TestMethod]
        public void VoxelLoadSaveVox()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\asteroid0moon4.vox";
            const string fileNew = @".\TestOutput\asteroid0moon4_save.vox";

            var voxelMap = new MyVoxelMap();

            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);
            voxelMap.Save(fileNew);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(9428, lengthOriginal, "File size must match.");
            Assert.AreEqual(9428, lengthNew, "File size must match.");
        }

        [TestMethod]
        public void VoxelLoadSaveVx2V1()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\AsteroidV1Format.vx2";
            const string fileNew = @".\TestOutput\AsteroidV1Format_save.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);
        
            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);
            Assert.AreEqual(594485, materialAssets.Count, "Asset count should be equal.");

            var asset0 = materialAssets.Where(c => c == 0).ToList();
            Assert.AreEqual(0, asset0.Count, "asset0 count should be equal.");

            var asset1 = materialAssets.Where(c => c == 1).ToList();
            Assert.AreEqual(0, asset1.Count, "asset1 Asset count should be equal.");

            var asset2 = materialAssets.Where(c => c == 2).ToList();
            Assert.AreEqual(0, asset2.Count, "asset2 Asset count should be equal.");

            var asset3 = materialAssets.Where(c => c == 3).ToList();
            Assert.AreEqual(251145, asset3.Count, "asset3 Asset count should be equal.");

            var asset4 = materialAssets.Where(c => c == 4).ToList();
            Assert.AreEqual(0, asset4.Count, "asset4 Asset count should be equal.");

            var asset5 = materialAssets.Where(c => c == 5).ToList();
            Assert.AreEqual(0, asset5.Count, "asset5 Asset count should be equal.");

            var asset6 = materialAssets.Where(c => c == 6).ToList();
            Assert.AreEqual(217283, asset6.Count, "asset6 Asset count should be equal.");

            var asset7 = materialAssets.Where(c => c == 7).ToList();
            Assert.AreEqual(237, asset7.Count, "asset7 Asset count should be equal.");

            var asset8 = materialAssets.Where(c => c == 8).ToList();
            Assert.AreEqual(9608, asset8.Count, "asset8 Asset count should be equal.");

            var asset9 = materialAssets.Where(c => c == 9).ToList();
            Assert.AreEqual(40801, asset9.Count, "asset9 Asset count should be equal.");

            var asset10 = materialAssets.Where(c => c == 10).ToList();
            Assert.AreEqual(152, asset10.Count, "asset10 Asset count should be equal.");

            var assetNameCount = voxelMap.CountAssets(materialAssets);
            Assert.IsTrue(assetNameCount.Count > 0, "Contains assets.");

            voxelMap.Save(fileNew);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(88299, lengthOriginal, "File size must match.");
            Assert.AreEqual(72296, lengthNew, "File size must match.");
        }

        [TestMethod]
        public void VoxelLoadSaveVx2V2()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\AsteroidV2Format.vx2";
            const string fileNew = @".\TestOutput\AsteroidV2Format_save.vx2";

            var voxelMap = new MyVoxelMap();

            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);

            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);
            Assert.AreEqual(594485, materialAssets.Count, "Asset count should be equal.");

            var asset0 = materialAssets.Where(c => c == 0).ToList();
            Assert.AreEqual(0, asset0.Count, "asset0 count should be equal.");

            var asset1 = materialAssets.Where(c => c == 1).ToList();
            Assert.AreEqual(0, asset1.Count, "asset1 Asset count should be equal.");

            var asset2 = materialAssets.Where(c => c == 2).ToList();
            Assert.AreEqual(0, asset2.Count, "asset2 Asset count should be equal.");

            var asset3 = materialAssets.Where(c => c == 3).ToList();
            Assert.AreEqual(251145, asset3.Count, "asset3 Asset count should be equal.");

            var asset4 = materialAssets.Where(c => c == 4).ToList();
            Assert.AreEqual(0, asset4.Count, "asset4 Asset count should be equal.");

            var asset5 = materialAssets.Where(c => c == 5).ToList();
            Assert.AreEqual(0, asset5.Count, "asset5 Asset count should be equal.");

            var asset6 = materialAssets.Where(c => c == 6).ToList();
            Assert.AreEqual(217283, asset6.Count, "asset6 Asset count should be equal.");

            var asset7 = materialAssets.Where(c => c == 7).ToList();
            Assert.AreEqual(237, asset7.Count, "asset7 Asset count should be equal.");

            var asset8 = materialAssets.Where(c => c == 8).ToList();
            Assert.AreEqual(9608, asset8.Count, "asset8 Asset count should be equal.");

            var asset9 = materialAssets.Where(c => c == 9).ToList();
            Assert.AreEqual(40801, asset9.Count, "asset9 Asset count should be equal.");

            var asset10 = materialAssets.Where(c => c == 10).ToList();
            Assert.AreEqual(152, asset10.Count, "asset10 Asset count should be equal.");

            var assetNameCount = voxelMap.CountAssets(materialAssets);
            Assert.IsTrue(assetNameCount.Count > 0, "Contains assets.");

            voxelMap.Save(fileNew);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(72296, lengthOriginal, "File size must match.");
            Assert.AreEqual(72296, lengthNew, "File size must match.");
        }


        [TestMethod]
        public void VoxelLoadStock()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var redShipCrashedAsteroidPath = Path.Combine(contentPath, "VoxelMaps", "RedShipCrashedAsteroid.vx2");

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(redShipCrashedAsteroidPath, materials[0].Id.SubtypeId);

            Assert.AreEqual(1, voxelMap.FileVersion, "FileVersion should be equal.");

            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);
            Assert.AreEqual(563742, materialAssets.Count, "Asset count should be equal.");

            var asset0 = materialAssets.Where(c => c == 0).ToList();
            Assert.AreEqual(563742, asset0.Count, "asset0 count should be equal.");

            var assetNameCount = voxelMap.CountAssets(materialAssets);
            Assert.IsTrue(assetNameCount.Count > 0, "Contains assets.");

            var lengthOriginal = new FileInfo(redShipCrashedAsteroidPath).Length;
            Assert.AreEqual(51819, lengthOriginal, "File size must match.");
        }

        [TestMethod]
        public void VoxelDetails()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\DeformedSphereWithHoles_64x128x64.vx2";

            var voxelMap = new MyVoxelMap();

            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);
            var voxCells = voxelMap.SumVoxelCells();

            Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(128, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(48, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(112, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(48, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            Assert.AreEqual(30909925, voxCells, "Voxel cells must match.");
        }

        [TestMethod]
        public void VoxelMaterialIndexes()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            for (byte i = 0; i < materials.Count; i++)
            {
                Assert.AreEqual(i, SpaceEngineersCore.Resources.GetMaterialIndex(materials[i].Id.SubtypeId), "Material index should equal original.");
            }

            Assert.AreEqual(0xFF, SpaceEngineersCore.Resources.GetMaterialIndex("blaggg"), "Material index should not exist.");
        }

        [TestMethod]
        public void VoxelMaterialChanges()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileOriginal = @".\TestAssets\asteroid0moon4.vx2";
            const string fileNew = @".\TestOutput\asteroid0moon4_gold.vx2";

            MyVoxelBuilder.ConvertAsteroid(fileOriginal, fileNew, stoneMaterial.Id.SubtypeId, goldMaterial.Id.SubtypeId);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(9431, lengthOriginal, "Original file size must match.");
            Assert.AreNotEqual(9431, lengthNew, "New file size must match.");
        }

        [TestMethod]
        public void VoxelMaterialAssets_MixedGeneratedAsset()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileOriginal = @".\TestAssets\asteroid0moon4.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);

            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);

            Assert.AreEqual(44135, materialAssets.Count, "Asset count should be equal.");

            var otherAssets = materialAssets.Where(c => c != 4).ToList();

            Assert.AreEqual(0, otherAssets.Count, "Other Asset count should be equal.");

            var assetNameCount = voxelMap.CountAssets(materialAssets);
            Assert.IsTrue(assetNameCount.Count > 0, "Contains assets.");
        }

        [TestMethod]
        public void VoxelMaterialAssets_FixedSize()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileOriginal = @".\TestAssets\test_cube2x2x2.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);

            var cellCount = voxelMap.SumVoxelCells();
            Assert.AreEqual(8 * 255, cellCount, "Cell count should be equal.");

            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);
            Assert.AreEqual(8, materialAssets.Count, "Asset count should be equal.");

            var stoneAssets = materialAssets.Where(c => c == 1).ToList();
            Assert.AreEqual(8, stoneAssets.Count, "Stone Asset count should be equal.");

            var assetNameCount = voxelMap.CountAssets(materialAssets);
            Assert.IsTrue(assetNameCount.Count > 0, "Contains assets.");
        }

        [TestMethod]
        public void VoxelMaterialAssets_FixedSize_MixedContent()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileOriginal = @".\TestAssets\test_cube_mixed_2x2x2.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);
            var cellCount = voxelMap.SumVoxelCells();
            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);
            var assetNameCount = voxelMap.CountAssets(materialAssets);

            Assert.AreEqual(8 * 255, cellCount, "Cell count should be equal.");
            Assert.AreEqual(8, materialAssets.Count, "Asset count should be equal.");
            Assert.AreEqual(8, assetNameCount.Count, "Asset Mertials count should be equal.");
        }

        [TestMethod]
        public void VoxelMaterialAssetsRandom()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_05"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");
            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");
            var uraniumMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Uraninite_01"));
            Assert.IsNotNull(uraniumMaterial, "Uranium material should exist.");

            const string fileOriginal = @".\TestAssets\Arabian_Border_7.vx2";
            const string fileNewVoxel = @".\TestOutput\Arabian_Border_7_mixed.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);
            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);

            Assert.AreEqual(35465, materialAssets.Count, "Asset count should be equal.");

            var distribution = new[] { Double.NaN, .5, .25 };
            var materialSelection = new[] { SpaceEngineersCore.Resources.GetMaterialIndex(stoneMaterial.Id.SubtypeId), SpaceEngineersCore.Resources.GetMaterialIndex(goldMaterial.Id.SubtypeId), SpaceEngineersCore.Resources.GetMaterialIndex(uraniumMaterial.Id.SubtypeId) };

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

            var assetNameCount = voxelMap.CountAssets(newDistributiuon);

            Assert.AreEqual(3, assetNameCount.Count, "Asset Mertials count should be equal.");
            Assert.AreEqual(8867, assetNameCount[stoneMaterial.Id.SubtypeId], "Asset Mertials count should be equal.");
            Assert.AreEqual(17732, assetNameCount[goldMaterial.Id.SubtypeId], "Asset Mertials count should be equal.");
            Assert.AreEqual(8866, assetNameCount[uraniumMaterial.Id.SubtypeId], "Asset Mertials count should be equal.");

            voxelMap.SetMaterialAssets(newDistributiuon);

            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);
            var cellCount = voxelMap.SumVoxelCells();

            voxelMap.Save(fileNewVoxel);
        }

        [TestMethod]
        public void VoxelMaterialAssetsGenerateFixed()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var files = new[] { @".\TestAssets\Arabian_Border_7.vx2", @".\TestAssets\cube_52x52x52.vx2" };

            foreach (var fileOriginal in files)
            {
                foreach (var material in materials)
                {
                    var fileNewVoxel =
                        Path.Combine(Path.GetDirectoryName(Path.GetFullPath(fileOriginal)),
                            Path.GetFileNameWithoutExtension(fileOriginal) + "_" + material.Id.SubtypeId + ".vx2").ToLower();

                    var voxelMap = new MyVoxelMap();
                    voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);

                    IList<byte> materialAssets;
                    Dictionary<byte, long> materialVoxelCells;
                    voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);

                    var distribution = new[] { Double.NaN, .99, };
                    var materialSelection = new byte[] { 0, SpaceEngineersCore.Resources.GetMaterialIndex(material.Id.SubtypeId) };

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

                    voxelMap.SetMaterialAssets(newDistributiuon);
                    voxelMap.Save(fileNewVoxel);
                }
            }
        }

        [TestMethod]
        public void VoxelGenerateBoxSmall()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_cube_solid_8x8x8_gold.vx2";

            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(false, 8, 8, 8, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(2085, lengthNew, "New file size must match.");

            Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(8, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(8, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(8, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            // Centered in the middle of 1 and 8.   1234-(4.5)-5678
            Assert.AreEqual(4.5, voxelMap.BoundingContent.Center.X, "Voxel Center must match.");
            Assert.AreEqual(4.5, voxelMap.BoundingContent.Center.Y, "Voxel Center must match.");
            Assert.AreEqual(4.5, voxelMap.BoundingContent.Center.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateBoxSmallMultiThread()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_cube_solid_8x8x8_gold.vx2";

            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(true, 8, 8, 8, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(2085, lengthNew, "New file size must match.");

            Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(8, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(8, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(8, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            // Centered in the middle of 1 and 8.   1234-(4.5)-5678
            Assert.AreEqual(4.5, voxelMap.BoundingContent.Center.X, "Voxel Center must match.");
            Assert.AreEqual(4.5, voxelMap.BoundingContent.Center.Y, "Voxel Center must match.");
            Assert.AreEqual(4.5, voxelMap.BoundingContent.Center.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateSphereSmall()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_sphere_solid_7_gold.vx2";

            var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(false, 4, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(2278, lengthNew, "New file size must match.");

            Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(7, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(7, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(7, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            // Centered in the middle of the 64x64x64 cell.
            Assert.AreEqual(32, voxelMap.BoundingContent.Center.X, "Voxel Center must match.");
            Assert.AreEqual(32, voxelMap.BoundingContent.Center.Y, "Voxel Center must match.");
            Assert.AreEqual(32, voxelMap.BoundingContent.Center.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateSphereLarge()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_sphere_solid_499_gold.vx2";

            var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(true, 250, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            var voxCells = voxelMap.SumVoxelCells();
            Assert.AreEqual(16589770849, voxCells, "Voxel cells must match.");

            Assert.AreEqual(785439, lengthNew, "New file size must match.");

            Assert.AreEqual(512, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(512, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(512, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(499, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(499, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(499, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            // Centered in the middle of the 512x512x512 cell.
            Assert.AreEqual(256, voxelMap.BoundingContent.Center.X, "Voxel Center must match.");
            Assert.AreEqual(256, voxelMap.BoundingContent.Center.Y, "Voxel Center must match.");
            Assert.AreEqual(256, voxelMap.BoundingContent.Center.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateSpikeWall()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileNew = @".\TestOutput\test_spike_wall.vx2";

            var size = new Vector3I(1024, 1024, 64);

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                e.Volume = 0x00;

                if (e.CoordinatePoint.X > 0 && e.CoordinatePoint.Y > 0 && e.CoordinatePoint.Z > 0
                    && e.CoordinatePoint.X < size.X - 1 && e.CoordinatePoint.Y < size.Y - 1 && e.CoordinatePoint.Z < size.Z - 1)
                {
                    if (e.CoordinatePoint.Z == 5 && (e.CoordinatePoint.X % 2 == 0) && (e.CoordinatePoint.Y % 2 == 0))
                    {
                        e.Volume = 0x92;
                    }
                    if (e.CoordinatePoint.Z == 6 && ((e.CoordinatePoint.X + 1) % 2 == 0) && ((e.CoordinatePoint.Y + 1) % 2 == 0))
                    {
                        e.Volume = 0x92;
                    }
                }
            };

            var voxelMap = MyVoxelBuilder.BuildAsteroid(true, size, materials[0].Id.SubtypeId, null, action);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(49809, lengthNew, "New file size must match.");

            Assert.AreEqual(1024, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(1024, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(1022, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(1022, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(2, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            // Centered in the middle of the 512x512x512 cell.
            Assert.AreEqual(511.5, voxelMap.BoundingContent.Center.X, "Voxel Center must match.");
            Assert.AreEqual(511.5, voxelMap.BoundingContent.Center.Y, "Voxel Center must match.");
            Assert.AreEqual(5.5, voxelMap.BoundingContent.Center.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateSpikeCube()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var fileNew = @".\TestOutput\test_spike_cube256.vx2";

            var length = 256;
            var min = 4;
            var max = length - 4;

            var size = new Vector3I(length, length, length);

            var buildparams = new int[][]
            {
                new[] { min, 0 }, 
                new[] { min + 1, 1 }, 
                new[] { max, 0 }, 
                new[] { max - 1, -1 }
            };

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                e.Volume = 0x00;

                if (e.CoordinatePoint.X > 0 && e.CoordinatePoint.Y > 0 && e.CoordinatePoint.Z > 0
                  && e.CoordinatePoint.X < size.X - 1 && e.CoordinatePoint.Y < size.Y - 1 && e.CoordinatePoint.Z < size.Z - 1
                && e.CoordinatePoint.X >= min && e.CoordinatePoint.Y >= min && e.CoordinatePoint.Z >= min
                    && e.CoordinatePoint.X <= max && e.CoordinatePoint.Y <= max && e.CoordinatePoint.Z <= max)
                {
                    foreach (int[] t in buildparams)
                    {
                        if (e.CoordinatePoint.X == t[0] && ((e.CoordinatePoint.Z + t[1]) % 2 == 0) && ((e.CoordinatePoint.Y + t[1]) % 2 == 0))
                        {
                            e.Volume = 0x92;
                        }
                        if (e.CoordinatePoint.Y == t[0] && ((e.CoordinatePoint.X + t[1]) % 2 == 0) && ((e.CoordinatePoint.Z + t[1]) % 2 == 0))
                        {
                            e.Volume = 0x92;
                        }
                        if (e.CoordinatePoint.Z == t[0] && ((e.CoordinatePoint.X + t[1]) % 2 == 0) && ((e.CoordinatePoint.Y + t[1]) % 2 == 0))
                        {
                            e.Volume = 0x92;
                        }
                    }
                }
            };

            var voxelMap = MyVoxelBuilder.BuildAsteroid(true, size, materials[0].Id.SubtypeId, null, action);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(22542, lengthNew, "New file size must match.");

            Assert.AreEqual(256, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(256, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(256, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(249, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(249, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(249, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            // Centered in the middle of the 256x256x256 cell.
            Assert.AreEqual(128, voxelMap.BoundingContent.Center.X, "Voxel Center must match.");
            Assert.AreEqual(128, voxelMap.BoundingContent.Center.Y, "Voxel Center must match.");
            Assert.AreEqual(128, voxelMap.BoundingContent.Center.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void Voxel3DImportStl()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string modelFile = @".\TestAssets\buddha-fixed-bottom.stl";
            const string voxelFile = @".\TestOutput\buddha-fixed-bottom.vx2";

            var transform = MeshHelper.TransformVector(new System.Windows.Media.Media3D.Vector3D(0, 0, 0), 0, 0, 180);

            var voxelMap = MyVoxelBuilder.BuildAsteroidFromModel(true, modelFile, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, true, stoneMaterial.Id.SubtypeId, ModelTraceVoxel.ThinSmoothed, 0.766, transform);
            voxelMap.Save(voxelFile);

            Assert.AreEqual(50, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(46, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(70, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            var voxCells = voxelMap.SumVoxelCells();

            Assert.AreEqual(18666335, voxCells, "Voxel cells must match.");
        }

        [TestMethod]
        public void LoadAllVoxelFiles()
        {
            SpaceEngineersCore.LoadDefinitions();

            var files = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationContentPath(), "VoxelMaps"), "*.vx2");

            foreach (var filename in files)
            {
                var voxelMap = new MyVoxelMap();
                voxelMap.Load(filename, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

                Assert.IsTrue(voxelMap.Size.X > 0, "Voxel Size must be greater than zero.");
                Assert.IsTrue(voxelMap.Size.Y > 0, "Voxel Size must be greater than zero.");
                Assert.IsTrue(voxelMap.Size.Z > 0, "Voxel Size must be greater than zero.");

                Debug.WriteLine(string.Format("Filename:\t{0}.vx2", Path.GetFileName(filename)));
                Debug.WriteLine(string.Format("Bounding Size:\t{0} × {1} × {2} blocks", voxelMap.Size.X, voxelMap.Size.Y, voxelMap.Size.Z));
                Debug.WriteLine(string.Format("Size:\t{0} m × {1} m × {2} m", voxelMap.BoundingContent.SizeInt().X + 1, voxelMap.BoundingContent.SizeInt().Y + 1, voxelMap.BoundingContent.SizeInt().Z + 1));
                Debug.WriteLine(string.Format("Volume:\t{0:##,###.00} m³", (double)voxelMap.SumVoxelCells() / 255));
                Debug.WriteLine("");
            }
        }

        [TestMethod]
        public void SeedFillVoxelFile()
        {
            SpaceEngineersCore.LoadDefinitions();

            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_01"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var ironMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Iron_02"));
            Assert.IsNotNull(ironMaterial, "Iron material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(false, 64, 64, 64, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);
            //var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(true, 64, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);

            var filler = new AsteroidSeedFiller();
            var fillProperties = new AsteroidSeedFillProperties
            {
                MainMaterial = new SEToolbox.Models.MaterialSelectionModel { Value = stoneMaterial.Id.SubtypeId },
                FirstMaterial = new SEToolbox.Models.MaterialSelectionModel { Value = ironMaterial.Id.SubtypeId },
                FirstRadius = 3,
                FirstVeins = 2,
                SecondMaterial = new SEToolbox.Models.MaterialSelectionModel { Value = goldMaterial.Id.SubtypeId },
                SecondRadius = 1,
                SecondVeins = 1,
            };

            filler.FillAsteroid(voxelMap, fillProperties);

            Assert.AreEqual(128, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(128, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(128, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(64, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(64, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(64, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            var voxCells = voxelMap.SumVoxelCells();
            Assert.AreEqual(66846720, voxCells, "Voxel cells must match.");

            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);

            var stoneAssets = materialAssets.Where(c => c == SpaceEngineersCore.Resources.GetMaterialIndex(stoneMaterial.Id.SubtypeId)).ToList();
            var ironAssets = materialAssets.Where(c => c == SpaceEngineersCore.Resources.GetMaterialIndex(ironMaterial.Id.SubtypeId)).ToList();
            var goldAssets = materialAssets.Where(c => c == SpaceEngineersCore.Resources.GetMaterialIndex(goldMaterial.Id.SubtypeId)).ToList();

            var sumAssets = (stoneAssets.Count + ironAssets.Count + goldAssets.Count) * 255;  // A cube should produce full voxcells, so all of them are 255.
            Assert.AreEqual(voxCells, sumAssets, "Assets sum should equal cells");

            // Seeder is too random to provide stable values.
            //Assert.AreEqual(236032, stoneAssets.Count, "Stone assets should equal.");
            //Assert.AreEqual(23040,  ironAssets.Count , "Iron assets should equal.");
            //Assert.AreEqual(3072,  goldAssets.Count, "Gold assets should equal.");

            // Strip the original material.
            //voxelMap.RemoveMaterial(stoneMaterial.Id.SubtypeId, null);
            //const string fileNew = @".\TestOutput\randomSeedMaterialCube.vx2";
            //voxelMap.Save(fileNew);
            //var lengthNew = new FileInfo(fileNew).Length;
        }

        [TestMethod]
        public void FetchVoxelV2DetailPreview()
        {
            const string fileOriginal = @".\TestAssets\DeformedSphereWithHoles_64x128x64.vx2";

            var size = MyVoxelMap.LoadVoxelSize(fileOriginal);

            Assert.AreEqual(64, size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(128, size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, size.Z, "Voxel Bounding size must match.");
        }

        [TestMethod]
        public void FetchVoxelV1DetailPreview()
        {
            const string fileOriginal = @".\TestAssets\asteroid0moon4.vox";
            
            var size = MyVoxelMap.LoadVoxelSize(fileOriginal);

            Assert.AreEqual(64, size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(64, size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, size.Z, "Voxel Bounding size must match.");
        }

        [TestMethod]
        public void VoxelMaterialAssets_FilledVolume()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileNew = @".\TestOutput\test_filledvolume.vx2";
            const int length = 64;
            var size = new Vector3I(length, length, length);
        
            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                e.Volume = 0xFF;
            };

            var voxelMap = MyVoxelBuilder.BuildAsteroid(true, size, materials[06].Id.SubtypeId, null, action);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(163, lengthNew, "New file size must match.");

            Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");
        }

        [TestMethod]
        public void VoxelRebuild1()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersCore.Resources.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var fileNew = @".\TestOutput\test_spike_cube1024.vx2";

            var length = 1024;
            var min = 4;
            var max = length - 4;

            var size = new Vector3I(length, length, length);

            var buildparams = new int[][]
            {
                new[] { min, 0 }, 
                new[] { min + 1, 1 }, 
                new[] { max, 0 }, 
                new[] { max - 1, -1 }
            };

            var action = (Action<MyVoxelBuilderArgs>)delegate(MyVoxelBuilderArgs e)
            {
                e.Volume = 0x00;

                if (e.CoordinatePoint.X > 0 && e.CoordinatePoint.Y > 0 && e.CoordinatePoint.Z > 0
                  && e.CoordinatePoint.X < size.X - 1 && e.CoordinatePoint.Y < size.Y - 1 && e.CoordinatePoint.Z < size.Z - 1
                && e.CoordinatePoint.X >= min && e.CoordinatePoint.Y >= min && e.CoordinatePoint.Z >= min
                    && e.CoordinatePoint.X <= max && e.CoordinatePoint.Y <= max && e.CoordinatePoint.Z <= max)
                {
                    foreach (int[] t in buildparams)
                    {
                        if (e.CoordinatePoint.X == t[0] && ((e.CoordinatePoint.Z + t[1]) % 2 == 0) && ((e.CoordinatePoint.Y + t[1]) % 2 == 0))
                        {
                            e.Volume = 0x92;
                        }
                        if (e.CoordinatePoint.Y == t[0] && ((e.CoordinatePoint.X + t[1]) % 2 == 0) && ((e.CoordinatePoint.Z + t[1]) % 2 == 0))
                        {
                            e.Volume = 0x92;
                        }
                        if (e.CoordinatePoint.Z == t[0] && ((e.CoordinatePoint.X + t[1]) % 2 == 0) && ((e.CoordinatePoint.Y + t[1]) % 2 == 0))
                        {
                            e.Volume = 0x92;
                        }
                    }
                }
            };

            var voxelMap = MyVoxelBuilder.BuildAsteroid(true, size, materials[0].Id.SubtypeId, null, action);
            voxelMap.Save(fileNew);
        }


        [TestMethod]
        public void LoadLoader()
        {
            VoxelMapLoader.Load(@"D:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\Content\VoxelMaps\Arabian_Border_7.vx2");
        }

    }
}
