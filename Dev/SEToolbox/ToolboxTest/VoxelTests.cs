namespace ToolboxTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using Sandbox.ModAPI;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models.Asteroids;
    using SEToolbox.Support;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRage.Voxels;
    using VRageMath;
    using MyVoxelMap = SEToolbox.Interop.Asteroids.MyVoxelMap;

    [TestClass]
    public class VoxelTests
    {
        [TestInitialize]
        public void InitTest()
        {
            SpaceEngineersCore.LoadDefinitions();
        }

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
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;

            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");
        }

        [TestMethod]
        public void VoxelLoadSaveVox()
        {
            const string fileOriginal = @".\TestAssets\asteroid0moon4.vox";
            const string fileNew = @".\TestOutput\asteroid0moon4_save.vx2";

            MyVoxelMap.UpdateFileFormat(fileOriginal, fileNew);

            var voxelMap = new MyVoxelMap();

            voxelMap.Load(fileNew);
            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(9428, lengthOriginal, "File size must match.");
            Assert.AreEqual(9431, lengthNew, "File size must match.");
        }

        [TestMethod]
        public void VoxelLoadSaveVx2V1()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\AsteroidV1Format.vx2";
            const string fileNew = @".\TestOutput\AsteroidV1Format_save.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(147470221, voxelMap.VoxCells, "VoxCells count should be equal.");

            Assert.AreEqual(10, assetNameCount.Count, "Asset count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Stone_04"), "Stone_04 asset should exist.");
            Assert.AreEqual(59954503, assetNameCount["Stone_04"], "Stone_04 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Iron_02"), "Iron_02 asset should exist.");
            Assert.AreEqual(55380508, assetNameCount["Iron_02"], "Iron_02 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Magnesium_01"), "Magnesium_01 asset should exist.");
            Assert.AreEqual(10404255, assetNameCount["Magnesium_01"], "Magnesium_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Platinum_01"), "Platinum_01 asset should exist.");
            Assert.AreEqual(9756991, assetNameCount["Platinum_01"], "Platinum_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Gold_01"), "Gold_01 asset should exist.");
            Assert.AreEqual(3448256, assetNameCount["Gold_01"], "Gold_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Nickel_01"), "Nickel_01 asset should exist.");
            Assert.AreEqual(60253, assetNameCount["Nickel_01"], "Nickel_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Uraninite_01"), "Uraninite_01 asset should exist.");
            Assert.AreEqual(1377765, assetNameCount["Uraninite_01"], "Uraninite_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Silver_01"), "Silver_01 asset should exist.");
            Assert.AreEqual(4602677, assetNameCount["Silver_01"], "Silver_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Cobalt_01"), "Cobalt_01 asset should exist.");
            Assert.AreEqual(2446253, assetNameCount["Cobalt_01"], "Cobalt_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Silicon_01"), "Silicon_01 asset should exist.");
            Assert.AreEqual(38760, assetNameCount["Silicon_01"], "Silicon_01 count should be equal.");

            voxelMap.Save(fileNew);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(88299, lengthOriginal, "File size must match.");
            Assert.AreEqual(135125, lengthNew, "File size must match.");
        }

        [TestMethod]
        public void VoxelLoadSaveVx2V2()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\AsteroidV2Format.vx2";
            const string fileNew = @".\TestOutput\AsteroidV2Format_save.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(147470221, voxelMap.VoxCells, "VoxCells count should be equal.");

            Assert.AreEqual(10, assetNameCount.Count, "Asset count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Stone_04"), "Stone_04 asset should exist.");
            Assert.AreEqual(59954503, assetNameCount["Stone_04"], "Stone_04 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Iron_02"), "Iron_02 asset should exist.");
            Assert.AreEqual(55380508, assetNameCount["Iron_02"], "Iron_02 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Magnesium_01"), "Magnesium_01 asset should exist.");
            Assert.AreEqual(10404255, assetNameCount["Magnesium_01"], "Magnesium_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Platinum_01"), "Platinum_01 asset should exist.");
            Assert.AreEqual(9756991, assetNameCount["Platinum_01"], "Platinum_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Gold_01"), "Gold_01 asset should exist.");
            Assert.AreEqual(3448256, assetNameCount["Gold_01"], "Gold_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Nickel_01"), "Nickel_01 asset should exist.");
            Assert.AreEqual(60253, assetNameCount["Nickel_01"], "Nickel_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Uraninite_01"), "Uraninite_01 asset should exist.");
            Assert.AreEqual(1377765, assetNameCount["Uraninite_01"], "Uraninite_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Silver_01"), "Silver_01 asset should exist.");
            Assert.AreEqual(4602677, assetNameCount["Silver_01"], "Silver_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Cobalt_01"), "Cobalt_01 asset should exist.");
            Assert.AreEqual(2446253, assetNameCount["Cobalt_01"], "Cobalt_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Silicon_01"), "Silicon_01 asset should exist.");
            Assert.AreEqual(38760, assetNameCount["Silicon_01"], "Silicon_01 count should be equal.");

            voxelMap.Save(fileNew);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(72296, lengthOriginal, "File size must match.");
            Assert.AreEqual(135125, lengthNew, "File size must match.");
        }

        [TestMethod]
        public void VoxelLoadSaveVx2V3()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\AsteroidV3Format.vx2";
            const string fileNew = @".\TestOutput\AsteroidV3Format_save.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(147470221, voxelMap.VoxCells, "VoxCells count should be equal.");

            Assert.AreEqual(10, assetNameCount.Count, "Asset count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Stone_04"), "Stone_04 asset should exist.");
            Assert.AreEqual(59954503, assetNameCount["Stone_04"], "Stone_04 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Iron_02"), "Iron_02 asset should exist.");
            Assert.AreEqual(55380508, assetNameCount["Iron_02"], "Iron_02 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Magnesium_01"), "Magnesium_01 asset should exist.");
            Assert.AreEqual(10404255, assetNameCount["Magnesium_01"], "Magnesium_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Platinum_01"), "Platinum_01 asset should exist.");
            Assert.AreEqual(9756991, assetNameCount["Platinum_01"], "Platinum_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Gold_01"), "Gold_01 asset should exist.");
            Assert.AreEqual(3448256, assetNameCount["Gold_01"], "Gold_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Nickel_01"), "Nickel_01 asset should exist.");
            Assert.AreEqual(60253, assetNameCount["Nickel_01"], "Nickel_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Uraninite_01"), "Uraninite_01 asset should exist.");
            Assert.AreEqual(1377765, assetNameCount["Uraninite_01"], "Uraninite_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Silver_01"), "Silver_01 asset should exist.");
            Assert.AreEqual(4602677, assetNameCount["Silver_01"], "Silver_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Cobalt_01"), "Cobalt_01 asset should exist.");
            Assert.AreEqual(2446253, assetNameCount["Cobalt_01"], "Cobalt_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Silicon_01"), "Silicon_01 asset should exist.");
            Assert.AreEqual(38760, assetNameCount["Silicon_01"], "Silicon_01 count should be equal.");

            voxelMap.Save(fileNew);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(145351, lengthOriginal, "File size must match.");
            Assert.AreEqual(145351, lengthNew, "File size must match.");
        }

        [TestMethod]
        public void VoxelLoadStock()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var redShipCrashedAsteroidPath = Path.Combine(contentPath, "VoxelMaps", "RedShipCrashedAsteroid.vx2");

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(redShipCrashedAsteroidPath);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(139716285, voxelMap.VoxCells, "VoxCells count should be equal.");

            Assert.AreEqual(7, assetNameCount.Count, "Asset count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Stone_04"), "Stone_04 asset should exist.");
            Assert.AreEqual(110319366, assetNameCount["Stone_04"], "Stone_04 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Platinum_01"), "Platinum_01 asset should exist.");
            Assert.AreEqual(17876016, assetNameCount["Platinum_01"], "Platinum_01 count should be equal.");

            var lengthOriginal = new FileInfo(redShipCrashedAsteroidPath).Length;
            Assert.AreEqual(109192, lengthOriginal, "File size must match.");
        }

        [TestMethod]
        public void VoxelDetails()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\DeformedSphereWithHoles_64x128x64.vx2";

            var voxelMap = new MyVoxelMap();

            voxelMap.Load(fileOriginal);
            voxelMap.RefreshAssets();

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            Assert.AreEqual(128, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(128, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(128, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(48, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(112, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(48, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            Assert.AreEqual(30909925, voxelMap.VoxCells, "Voxel cells must match.");
        }

        [TestMethod]
        public void VoxelMaterialIndexes()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            for (byte i = 0; i < materials.Count; i++)
            {
                Assert.AreEqual(i, SpaceEngineersCore.Resources.GetMaterialIndex(materials[i].Id.SubtypeName), "Material index should equal original.");
            }

            // Cannot test for non-existing material.
            //Assert.AreEqual(0xFF, SpaceEngineersCore.Resources.GetMaterialIndex("blaggg"), "Material index should not exist.");
        }

        [TestMethod]
        public void VoxelMaterialChanges()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileOriginal = @".\TestAssets\asteroid0moon4.vx2";
            const string fileNew = @".\TestOutput\asteroid0moon4_gold.vx2";

            MyVoxelBuilder.ConvertAsteroid(fileOriginal, fileNew, stoneMaterial.Id.SubtypeName, goldMaterial.Id.SubtypeName);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(9431, lengthOriginal, "Original file size must match.");
            Assert.AreEqual(14674, lengthNew, "New file size must match.");

            var voxelMapOriginal = new MyVoxelMap();
            voxelMapOriginal.Load(fileOriginal);

            Assert.IsTrue(voxelMapOriginal.IsValid, "Voxel format must be valid.");

            Dictionary<string, long> assetNameCountOriginal = voxelMapOriginal.RefreshAssets();

            Assert.AreEqual(10654637, voxelMapOriginal.VoxCells, "VoxCells count should be equal.");
            Assert.AreEqual(1, assetNameCountOriginal.Count, "Asset count should be equal.");
            Assert.IsTrue(assetNameCountOriginal.ContainsKey("Stone_05"), "Stone_05 asset should exist.");
            Assert.AreEqual(10654637, assetNameCountOriginal["Stone_05"], "Stone_05 count should be equal.");

            var voxelMapNew = new MyVoxelMap();
            voxelMapNew.Load(fileNew);

            Assert.IsTrue(voxelMapNew.IsValid, "Voxel format must be valid.");

            Dictionary<string, long> assetNameCountNew = voxelMapNew.RefreshAssets();

            Assert.AreEqual(10654637, voxelMapNew.VoxCells, "VoxCells count should be equal.");
            Assert.AreEqual(1, assetNameCountNew.Count, "Asset count should be equal.");
            Assert.IsTrue(assetNameCountNew.ContainsKey("Gold_01"), "Gold_01 asset should exist.");
            Assert.AreEqual(10654637, assetNameCountNew["Gold_01"], "Gold_01 count should be equal.");
        }

        [TestMethod]
        public void VoxelMaterialAssets_FixedSize()
        {
            const string fileOriginal = @".\TestAssets\test_cube2x2x2.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(2040, voxelMap.VoxCells, "VoxCells count should be equal.");
            Assert.AreEqual(1, assetNameCount.Count, "Asset count should be equal.");
            Assert.IsTrue(assetNameCount.ContainsKey("Stone_02"), "Stone_02 asset should exist.");
            Assert.AreEqual(2040, assetNameCount["Stone_02"], "Stone_02 count should be equal.");
        }

        [TestMethod]
        public void VoxelMaterialAssets_FixedSize_MixedContent()
        {
            const string fileOriginal = @".\TestAssets\test_cube_mixed_2x2x2.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(2040, voxelMap.VoxCells, "VoxCells count should be equal.");
            Assert.AreEqual(8, assetNameCount.Count, "Asset count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Iron_01"), "Iron_01 asset should exist.");
            Assert.AreEqual(255, assetNameCount["Iron_01"], "Iron_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Platinum_01"), "Platinum_01 asset should exist.");
            Assert.AreEqual(255, assetNameCount["Platinum_01"], "Platinum_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Uraninite_01"), "Uraninite_01 asset should exist.");
            Assert.AreEqual(255, assetNameCount["Uraninite_01"], "Uraninite_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Silver_01"), "Silver_01 asset should exist.");
            Assert.AreEqual(255, assetNameCount["Silver_01"], "Silver_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Gold_01"), "Gold_01 asset should exist.");
            Assert.AreEqual(255, assetNameCount["Gold_01"], "Gold_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Silicon_01"), "Silicon_01 asset should exist.");
            Assert.AreEqual(255, assetNameCount["Silicon_01"], "Silicon_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Cobalt_01"), "Cobalt_01 asset should exist.");
            Assert.AreEqual(255, assetNameCount["Cobalt_01"], "Cobalt_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Magnesium_01"), "Magnesium_01 asset should exist.");
            Assert.AreEqual(255, assetNameCount["Magnesium_01"], "Magnesium_01 count should be equal.");
        }

        [TestMethod]
        public void VoxelMaterialAssetsRandom()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone_05"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");
            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");
            var uraniumMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Uraninite_01"));
            Assert.IsNotNull(uraniumMaterial, "Uranium material should exist.");

            const string fileOriginal = @".\TestAssets\Arabian_Border_7.vx2";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            IList<byte> materialAssets = voxelMap.CalcVoxelMaterialList();
            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(35465, materialAssets.Count, "Asset count should be equal.");

            Assert.AreEqual(8538122, voxelMap.VoxCells, "VoxCells count should be equal.");
            Assert.AreEqual(1, assetNameCount.Count, "Asset count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Stone_01"), "Stone_01 asset should exist.");
            Assert.AreEqual(8538122, assetNameCount["Stone_01"], "Stone_01 count should be equal.");

            // Create matieral distribution of set percentages, with remainder to Stone.

            var distribution = new[] { double.NaN, .5, .25 };
            var materialSelection = new[]
            {
                SpaceEngineersCore.Resources.GetMaterialIndex(stoneMaterial.Id.SubtypeName),
                SpaceEngineersCore.Resources.GetMaterialIndex(goldMaterial.Id.SubtypeName),
                SpaceEngineersCore.Resources.GetMaterialIndex(uraniumMaterial.Id.SubtypeName)
            };

            var newMaterialAssetDistributiuon = new List<byte>();
            int count;
            for (var i = 1; i < distribution.Length; i++)
            {
                count = (int)Math.Floor(distribution[i] * materialAssets.Count); // Round down.
                for (var j = 0; j < count; j++)
                {
                    newMaterialAssetDistributiuon.Add(materialSelection[i]);
                }
            }
            count = materialAssets.Count - newMaterialAssetDistributiuon.Count;
            for (var j = 0; j < count; j++)
                newMaterialAssetDistributiuon.Add(materialSelection[0]);

            // Randomize the distribution.
            newMaterialAssetDistributiuon.Shuffle();

            // Update the materials in the voxel.
            voxelMap.SetVoxelMaterialList(newMaterialAssetDistributiuon);
            assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(8538122, voxelMap.VoxCells, "VoxCells count should be equal.");
            Assert.AreEqual(3, assetNameCount.Count, "Asset count should be equal.");

            // due to randomization distribution acting a little differently depending on how the octree fills,
            // we need test for 1 percent less than target value.

            Assert.IsTrue(assetNameCount.ContainsKey("Stone_05"), "Stone_05 asset should exist.");
            Assert.IsTrue(assetNameCount["Stone_05"] > 0.24 * voxelMap.VoxCells, "Stone_05 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Gold_01"), "Gold_01 asset should exist.");
            Assert.IsTrue(assetNameCount["Gold_01"] > 0.49 * voxelMap.VoxCells, "Gold_01 count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey("Uraninite_01"), "Uraninite_01 asset should exist.");
            Assert.IsTrue(assetNameCount["Uraninite_01"] > 0.24 * voxelMap.VoxCells, "Uraninite_01 count should be equal.");
        }

        [TestMethod]
        public void VoxelMaterialAssetsGenerateFixed()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var files = new[] { @".\TestAssets\Arabian_Border_7.vx2", @".\TestAssets\cube_52x52x52.vx2" };

            foreach (var fileOriginal in files)
            {
                foreach (var material in materials)
                {
                    var fileNewVoxel =
                        Path.Combine(Path.GetDirectoryName(Path.GetFullPath(fileOriginal)),
                            Path.GetFileNameWithoutExtension(fileOriginal) + "_" + material.Id.SubtypeName + ".vx2").ToLower();

                    var voxelMap = new MyVoxelMap();
                    voxelMap.Load(fileOriginal);

                    IList<byte> materialAssets = voxelMap.CalcVoxelMaterialList();

                    var distribution = new[] { double.NaN, .99, };
                    var materialSelection = new byte[] { 0, SpaceEngineersCore.Resources.GetMaterialIndex(material.Id.SubtypeName) };

                    var newDistributiuon = new List<byte>();
                    int count;
                    for (var i = 1; i < distribution.Length; i++)
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

                    voxelMap.SetVoxelMaterialList(newDistributiuon);
                    voxelMap.Save(fileNewVoxel);
                }
            }
        }

        [TestMethod]
        public void VoxelGenerateBoxSmall()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_cube_solid_8x8x8_gold.vx2";

            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(false, 8, 8, 8, goldMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, false, 0);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(917, lengthNew, "New file size must match.");

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

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(130560, voxelMap.VoxCells, "VoxCells count should be equal.");
            Assert.AreEqual(1, assetNameCount.Count, "Asset count should be equal.");
            Assert.IsTrue(assetNameCount.ContainsKey("Gold_01"), "Gold_01 asset should exist.");
            Assert.AreEqual(130560, assetNameCount["Gold_01"], "Gold_01 count should be equal.");
        }

        [TestMethod]
        public void VoxelGenerateBoxSmallMultiThread()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_cube_solid_8x8x8_gold.vx2";

            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(true, 8, 8, 8, goldMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, false, 0);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(917, lengthNew, "New file size must match.");

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

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(130560, voxelMap.VoxCells, "VoxCells count should be equal.");
            Assert.AreEqual(1, assetNameCount.Count, "Asset count should be equal.");
            Assert.IsTrue(assetNameCount.ContainsKey("Gold_01"), "Gold_01 asset should exist.");
            Assert.AreEqual(130560, assetNameCount["Gold_01"], "Gold_01 count should be equal.");
        }

        [TestMethod]
        public void VoxelGenerateSphereSmall()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_sphere_solid_7_gold.vx2";

            var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(false, 4, goldMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, false, 0);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            //Assert.AreEqual(1337, lengthNew, "New file size must match.");

            //Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            //Assert.AreEqual(64, voxelMap.Size.Y, "Voxel Bounding size must match.");
            //Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            //Assert.AreEqual(7, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            //Assert.AreEqual(7, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            //Assert.AreEqual(7, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            //// Centered in the middle of the 64x64x64 cell.
            //Assert.AreEqual(32, voxelMap.BoundingContent.Center.X, "Voxel Center must match.");
            //Assert.AreEqual(32, voxelMap.BoundingContent.Center.Y, "Voxel Center must match.");
            //Assert.AreEqual(32, voxelMap.BoundingContent.Center.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateShape()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            Vector3I size = new Vector3I(128, 128, 128);
            var voxelMap = new MyVoxelMap();
            var actualSize = new Vector3I(size.X.RoundUpToCube(), size.Y.RoundUpToCube(), size.Z.RoundUpToCube());
            voxelMap.Create(actualSize, stoneMaterial.Id.SubtypeName);

            MyShapeSphere sphereShape1 = new MyShapeSphere
            {
                Center = new Vector3D(64, 64, 64),
                Radius = 40
            };

            voxelMap.UpdateVoxelShape(OperationType.Fill, sphereShape1, goldMaterial.Index);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            Assert.AreEqual(81, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(81, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(81, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            // Centered in the middle of the 512x512x512 cell.
            Assert.AreEqual(64, voxelMap.BoundingContent.Center.X, "Voxel Center must match.");
            Assert.AreEqual(64, voxelMap.BoundingContent.Center.Y, "Voxel Center must match.");
            Assert.AreEqual(64, voxelMap.BoundingContent.Center.Z, "Voxel Center must match.");

            Assert.AreEqual(1, assetNameCount.Count, "Asset count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey(goldMaterial.Id.SubtypeName), "Gold asset should exist.");
            Assert.IsTrue(assetNameCount[goldMaterial.Id.SubtypeName] > 10000, "Gold count should be equal.");

            //const string fileNew = @".\TestOutput\test_sphere_solid_7_gold.vx2";
            //voxelMap.Save(fileNew);
            //var lengthNew = new FileInfo(fileNew).Length;


            MyShapeBox sphereBox = new MyShapeBox()
            {
                Boundaries = new BoundingBoxD
                {
                    Min = new Vector3D(0, 0, 0),
                    Max = new Vector3D(127, 127, 127)
                }
            };

            voxelMap.UpdateVoxelShape(OperationType.Cut, sphereBox, 0);
            Dictionary<string, long> assetNameCount2 = voxelMap.RefreshAssets();

            Assert.AreEqual(0, voxelMap.VoxCells, "Voxel cells must match.");
        }

        [TestMethod]
        public void VoxelGenerateSphereLarge()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_sphere_solid_499_gold.vx2";

            var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(true, 250, goldMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, false, 0);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            voxelMap.Save(fileNew);
            voxelMap.RefreshAssets();

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(16689203471, voxelMap.VoxCells, "Voxel cells must match.");

            //Assert.AreEqual(2392621, lengthNew, "New file size must match.");

            Assert.AreEqual(512, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(512, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(512, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(501, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(501, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(501, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            // Centered in the middle of the 512x512x512 cell.
            Assert.AreEqual(256, voxelMap.BoundingContent.Center.X, "Voxel Center must match.");
            Assert.AreEqual(256, voxelMap.BoundingContent.Center.Y, "Voxel Center must match.");
            Assert.AreEqual(256, voxelMap.BoundingContent.Center.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateSpikeWall()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileNew = @".\TestOutput\test_spike_wall.vx2";

            var size = new Vector3I(1024, 1024, 64);

            var action = (Action<MyVoxelBuilderArgs>)delegate (MyVoxelBuilderArgs e)
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

            var voxelMap = MyVoxelBuilder.BuildAsteroid(true, size, materials[0].Id.SubtypeName, null, action);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            // Multi threading does not produce a consistant volume across the cells in the voxel, so the actual file content can vary!!
            Assert.IsTrue(lengthNew > 77000, "New file size must match.");

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
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var fileNew = @".\TestOutput\test_spike_cube256.vx2";

            var length = 256;
            var min = 4;
            var max = length - 4;

            var size = new Vector3I(length, length, length);

            int[][] buildparams =
            {
                new[] { min, 0 },
                new[] { min + 1, 1 },
                new[] { max, 0 },
                new[] { max - 1, -1 }
            };

            var action = (Action<MyVoxelBuilderArgs>)delegate (MyVoxelBuilderArgs e)
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

            var voxelMap = MyVoxelBuilder.BuildAsteroid(true, size, materials[0].Id.SubtypeName, null, action);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            // Multi threading does not produce a consistant volume across the cells in the voxel, so the actual file content can vary!!
            Assert.IsTrue(lengthNew > 44000, "New file size must match.");

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
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string modelFile = @".\TestAssets\buddha-fixed-bottom.stl";
            const string voxelFile = @".\TestOutput\buddha-fixed-bottom.vx2";

            var transform = MeshHelper.TransformVector(new System.Windows.Media.Media3D.Vector3D(0, 0, 0), 0, 0, 180);

            var voxelMap = MyVoxelBuilder.BuildAsteroidFromModel(true, modelFile, goldMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, true, stoneMaterial.Id.SubtypeName, ModelTraceVoxel.ThinSmoothed, 0.766, transform);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            voxelMap.Save(voxelFile);

            Assert.AreEqual(50, voxelMap.BoundingContent.SizeInt().X + 1, "Voxel Content size must match.");
            Assert.AreEqual(46, voxelMap.BoundingContent.SizeInt().Y + 1, "Voxel Content size must match.");
            Assert.AreEqual(70, voxelMap.BoundingContent.SizeInt().Z + 1, "Voxel Content size must match.");

            Assert.AreEqual(18666335, voxelMap.VoxCells, "Voxel cells must match.");
        }

        [TestMethod]
        public void LoadAllVoxelFiles()
        {
            var files = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationContentPath(), "VoxelMaps"), "*.vx2");

            foreach (var filename in files)
            {
                string name = Path.GetFileName(filename);
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var voxelMap = new MyVoxelMap();
                voxelMap.Load(filename);
                watch.Stop();

                Debug.WriteLine(string.Format("Filename:\t{0}.vx2", name));
                Debug.WriteLine("Load Time:\t{0}", watch.Elapsed);
                Debug.WriteLine("Valid:\t{0}", voxelMap.IsValid);
                Debug.WriteLine("Bounding Size:\t{0} × {1} × {2} blocks", voxelMap.Size.X, voxelMap.Size.Y, voxelMap.Size.Z);
                Debug.WriteLine("");
            }
        }

        [TestMethod]
        public void SeedFillVoxelFile()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Stone_01"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var ironMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Iron_02"));
            Assert.IsNotNull(ironMaterial, "Iron material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeName.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(false, 64, 64, 64, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, false, 0);
            //var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(true, 64, stoneMaterial.Id.SubtypeName, stoneMaterial.Id.SubtypeName, false, 0);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            var filler = new AsteroidSeedFiller();
            var fillProperties = new AsteroidSeedFillProperties
            {
                MainMaterial = new SEToolbox.Models.MaterialSelectionModel { Value = stoneMaterial.Id.SubtypeName },
                FirstMaterial = new SEToolbox.Models.MaterialSelectionModel { Value = ironMaterial.Id.SubtypeName },
                FirstRadius = 3,
                FirstVeins = 2,
                SecondMaterial = new SEToolbox.Models.MaterialSelectionModel { Value = goldMaterial.Id.SubtypeName },
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

            Assert.AreEqual(66846720, voxelMap.VoxCells, "Voxel cells must match.");

            Dictionary<string, long> assetNameCount = voxelMap.RefreshAssets();

            // A cube should produce full voxcells, so all of them are 255.
            Assert.AreEqual(2040, voxelMap.VoxCells, "VoxCells count should be equal.");
            Assert.AreEqual(8, assetNameCount.Count, "Asset count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey(stoneMaterial.Id.SubtypeName), "Stone asset should exist.");
            Assert.AreEqual(255, assetNameCount[stoneMaterial.Id.SubtypeName], "Stone count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey(ironMaterial.Id.SubtypeName), "Iron asset should exist.");
            Assert.AreEqual(255, assetNameCount[ironMaterial.Id.SubtypeName], "Iron count should be equal.");

            Assert.IsTrue(assetNameCount.ContainsKey(goldMaterial.Id.SubtypeName), "Gold asset should exist.");
            Assert.AreEqual(255, assetNameCount[goldMaterial.Id.SubtypeName], "Gold count should be equal.");


            // Seeder is too random to provide stable values.
            //Assert.AreEqual(236032, stoneAssets.Count, "Stone assets should equal.");
            //Assert.AreEqual(23040,  ironAssets.Count , "Iron assets should equal.");
            //Assert.AreEqual(3072,  goldAssets.Count, "Gold assets should equal.");

            // Strip the original material.
            //voxelMap.RemoveMaterial(stoneMaterial.Id.SubtypeName, null);
            //const string fileNew = @".\TestOutput\randomSeedMaterialCube.vx2";
            //voxelMap.Save(fileNew);
            //var lengthNew = new FileInfo(fileNew).Length;
        }

        [TestMethod]
        public void FetchVoxelV2DetailPreview()
        {
            const string fileOriginal = @".\TestAssets\DeformedSphereWithHoles_64x128x64.vx2";

            var size = MyVoxelMap.LoadVoxelSize(fileOriginal);

            Assert.AreEqual(128, size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(128, size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(128, size.Z, "Voxel Bounding size must match.");
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
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileNew = @".\TestOutput\test_filledvolume.vx2";
            const int length = 64;
            var size = new Vector3I(length, length, length);

            var action = (Action<MyVoxelBuilderArgs>)delegate (MyVoxelBuilderArgs e)
            {
                e.Volume = 0xFF;
            };

            var voxelMap = MyVoxelBuilder.BuildAsteroid(true, size, materials[06].Id.SubtypeName, null, action);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            voxelMap.Save(fileNew);
            voxelMap.RefreshAssets();

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(376, lengthNew, "New file size must match.");

            Assert.AreEqual(66846720, voxelMap.VoxCells, "Voxel Cells must match."); // 255 * 64 * 64 * 64
            Assert.AreEqual(64, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");
        }

        [TestMethod]
        public void VoxelRebuild1()
        {
            var materials = SpaceEngineersCore.Resources.VoxelMaterialDefinitions;
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var fileNew = @".\TestOutput\test_spike_cube1024.vx2";

            var length = 1024;
            var min = 4;
            var max = length - 4;

            var size = new Vector3I(length, length, length);

            int[][] buildparams =
            {
                new[] { min, 0 },
                new[] { min + 1, 1 },
                new[] { max, 0 },
                new[] { max - 1, -1 }
            };

            var action = (Action<MyVoxelBuilderArgs>)delegate (MyVoxelBuilderArgs e)
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

            var voxelMap = MyVoxelBuilder.BuildAsteroid(true, size, materials[0].Id.SubtypeName, null, action);

            Assert.IsTrue(voxelMap.IsValid, "Voxel format must be valid.");

            voxelMap.Save(fileNew);
        }

        [TestMethod]
        public void LoadLoader()
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var arabianBorder7AsteroidPath = Path.Combine(contentPath, "VoxelMaps", "Arabian_Border_7.vx2");
            VoxelMapLoader.Load(arabianBorder7AsteroidPath);
        }

    }
}
