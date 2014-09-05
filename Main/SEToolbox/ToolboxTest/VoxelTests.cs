namespace ToolboxTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Media3D;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using VRageMath;
    using SEToolbox.Models.Asteroids;

    [TestClass]
    public class VoxelTests
    {
        [TestMethod]
        public void VoxelCompression()
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
            var materials = SpaceEngineersApi.GetMaterialList();

            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");
        }

        [TestMethod]
        public void VoxelLoadSave()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
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
        public void VoxelDetails()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileOriginal = @".\TestAssets\DeformedSphereWithHoles_64x128x64.vox";

            var voxelMap = new MyVoxelMap();

            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);
            var voxCells = voxelMap.SumVoxelCells();

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
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            for (byte i = 0; i < materials.Count; i++)
            {
                Assert.AreEqual(i, SpaceEngineersApi.GetMaterialIndex(materials[i].Id.SubtypeId), "Material index should equal original.");
            }

            Assert.AreEqual(0xFF, SpaceEngineersApi.GetMaterialIndex("blaggg"), "Material index should not exist.");
        }

        [TestMethod]
        public void VoxelMaterialChanges()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileOriginal = @".\TestAssets\asteroid0moon4.vox";
            const string fileNew = @".\TestOutput\asteroid0moon4_gold.vox";

            MyVoxelBuilder.ConvertAsteroid(fileOriginal, fileNew, stoneMaterial.Id.SubtypeId, goldMaterial.Id.SubtypeId);

            var lengthOriginal = new FileInfo(fileOriginal).Length;
            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(9428, lengthOriginal, "Original file size must match.");
            Assert.AreNotEqual(9428, lengthNew, "New file size must match.");
        }

        [TestMethod]
        public void VoxelMaterialAssets_MixedGeneratedAsset()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileOriginal = @".\TestAssets\asteroid0moon4.vox";

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
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileOriginal = @".\TestAssets\test_cube2x2x2.vox";

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
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileOriginal = @".\TestAssets\test_cube_mixed_2x2x2.vox";

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
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_05"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");
            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");
            var uraniumMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Uraninite_01"));
            Assert.IsNotNull(uraniumMaterial, "Uranium material should exist.");

            const string fileOriginal = @".\TestAssets\Arabian_Border_7.vox";
            const string fileNewVoxel = @".\TestOutput\Arabian_Border_7_mixed.vox";

            var voxelMap = new MyVoxelMap();
            voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);
            IList<byte> materialAssets;
            Dictionary<byte, long> materialVoxelCells;
            voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);

            Assert.AreEqual(35465, materialAssets.Count, "Asset count should be equal.");

            var distribution = new [] { Double.NaN, .5, .25 };
            var materialSelection = new [] { SpaceEngineersApi.GetMaterialIndex(stoneMaterial.Id.SubtypeId), SpaceEngineersApi.GetMaterialIndex(goldMaterial.Id.SubtypeId), SpaceEngineersApi.GetMaterialIndex(uraniumMaterial.Id.SubtypeId) };

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
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var files = new [] { @".\TestAssets\Arabian_Border_7.vox", @".\TestAssets\cube_52x52x52.vox" };

            foreach (var fileOriginal in files)
            {
                foreach (var material in materials)
                {
                    var fileNewVoxel =
                        Path.Combine(Path.GetDirectoryName(Path.GetFullPath(fileOriginal)),
                            Path.GetFileNameWithoutExtension(fileOriginal) + "_" + material.Id.SubtypeId + ".vox").ToLower();

                    var voxelMap = new MyVoxelMap();
                    voxelMap.Load(fileOriginal, materials[0].Id.SubtypeId);

                    IList<byte> materialAssets;
                    Dictionary<byte, long> materialVoxelCells;
                    voxelMap.CalculateMaterialCellAssets(out materialAssets, out materialVoxelCells);

                    var distribution = new [] { Double.NaN, .99, };
                    var materialSelection = new byte [] { 0, SpaceEngineersApi.GetMaterialIndex(material.Id.SubtypeId) };

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
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_cube_solid_8x8x8_gold.vox";

            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(false, 8, 8, 8, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(7360, lengthNew, "New file size must match.");

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
        public void VoxelGenerateBoxSmallMultiThread()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_cube_solid_8x8x8_gold.vox";

            var voxelMap = MyVoxelBuilder.BuildAsteroidCube(true, 8, 8, 8, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(7360, lengthNew, "New file size must match.");

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
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_sphere_solid_7_gold.vox";

            var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(false, 4, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(7559, lengthNew, "New file size must match.");

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

        [TestMethod]
        public void VoxelGenerateSphereLarge()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string fileNew = @".\TestOutput\test_sphere_solid_499_gold.vox";

            var voxelMap = MyVoxelBuilder.BuildAsteroidSphere(true, 250, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, false, 0);
            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            var voxCells = voxelMap.SumVoxelCells();
            Assert.AreEqual(16589770849, voxCells, "Voxel cells must match.");

            Assert.AreEqual(1213009, lengthNew, "New file size must match.");

            Assert.AreEqual(512, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(512, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(512, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(499, voxelMap.ContentSize.X, "Voxel Content size must match.");
            Assert.AreEqual(499, voxelMap.ContentSize.Y, "Voxel Content size must match.");
            Assert.AreEqual(499, voxelMap.ContentSize.Z, "Voxel Content size must match.");

            // Centered in the middle of the 512x512x512 cell.
            Assert.AreEqual(256, voxelMap.ContentCenter.X, "Voxel Center must match.");
            Assert.AreEqual(256, voxelMap.ContentCenter.Y, "Voxel Center must match.");
            Assert.AreEqual(256, voxelMap.ContentCenter.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateSpikeWall()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            const string fileNew = @".\TestOutput\test_spike_wall.vox";

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

            Assert.AreEqual(52171, lengthNew, "New file size must match.");

            Assert.AreEqual(1024, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(1024, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(64, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(1022, voxelMap.ContentSize.X, "Voxel Content size must match.");
            Assert.AreEqual(1022, voxelMap.ContentSize.Y, "Voxel Content size must match.");
            Assert.AreEqual(2, voxelMap.ContentSize.Z, "Voxel Content size must match.");

            // Centered in the middle of the 512x512x512 cell.
            Assert.AreEqual(511.5, voxelMap.ContentCenter.X, "Voxel Center must match.");
            Assert.AreEqual(511.5, voxelMap.ContentCenter.Y, "Voxel Center must match.");
            Assert.AreEqual(5.5, voxelMap.ContentCenter.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void VoxelGenerateSpikeCube()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var fileNew = @".\TestOutput\test_spike_cube256.vox";

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

            Assert.AreEqual(23465, lengthNew, "New file size must match.");

            Assert.AreEqual(256, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(256, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(256, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(249, voxelMap.ContentSize.X, "Voxel Content size must match.");
            Assert.AreEqual(249, voxelMap.ContentSize.Y, "Voxel Content size must match.");
            Assert.AreEqual(249, voxelMap.ContentSize.Z, "Voxel Content size must match.");

            // Centered in the middle of the 256x256x256 cell.
            Assert.AreEqual(128, voxelMap.ContentCenter.X, "Voxel Center must match.");
            Assert.AreEqual(128, voxelMap.ContentCenter.Y, "Voxel Center must match.");
            Assert.AreEqual(128, voxelMap.ContentCenter.Z, "Voxel Center must match.");
        }

        [TestMethod]
        public void Voxel3DImportStl()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();
            Assert.IsTrue(materials.Count > 0, "Materials should exist. Has the developer got Space Engineers installed?");

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone_02"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            const string modelFile = @".\TestAssets\buddha-fixed-bottom.stl";
            const string voxelFile = @".\TestOutput\buddha-fixed-bottom.vox";

            var transform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), 180, 0, 0);

            var voxelMap = MyVoxelBuilder.BuildAsteroidFromModel(true, modelFile, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, true, stoneMaterial.Id.SubtypeId, ModelTraceVoxel.ThinSmoothed, 0.766, transform);
            voxelMap.Save(voxelFile);

            Assert.AreEqual(50, voxelMap.ContentSize.X, "Voxel Content size must match.");
            Assert.AreEqual(46, voxelMap.ContentSize.Y, "Voxel Content size must match.");
            Assert.AreEqual(70, voxelMap.ContentSize.Z, "Voxel Content size must match.");

            var voxCells = voxelMap.SumVoxelCells();

            Assert.AreEqual(18666335, voxCells, "Voxel cells must match.");
        }

        [TestMethod]
        public void LoadAllVoxelFiles()
        {
            SpaceEngineersCore.LoadDefinitions();

            var files = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationContentPath(), "VoxelMaps"), "*.vox");

            foreach (var filename in files)
            {
                var voxelMap = new MyVoxelMap();
                voxelMap.Load(filename, SpaceEngineersApi.GetMaterialName(0), true);

                Assert.IsTrue(voxelMap.Size.X > 0, "Voxel Size must be greater than zero.");
                Assert.IsTrue(voxelMap.Size.Y > 0, "Voxel Size must be greater than zero.");
                Assert.IsTrue(voxelMap.Size.Z > 0, "Voxel Size must be greater than zero.");

                Debug.WriteLine(string.Format("Filename:\t{0}.vox", Path.GetFileName(filename)));
                Debug.WriteLine(string.Format("Bounding Size:\t{0} × {1} × {2} blocks", voxelMap.Size.X, voxelMap.Size.Y, voxelMap.Size.Z));
                Debug.WriteLine(string.Format("Size:\t{0} m × {1} m × {2} m", voxelMap.ContentSize.X, voxelMap.ContentSize.Y, voxelMap.ContentSize.Z));
                Debug.WriteLine(string.Format("Volume:\t{0:##,###.00} m³", (double)voxelMap.SumVoxelCells() / 255));
                Debug.WriteLine("");
            }
        }

        [TestMethod]
        public void FillVoxelFile()
        {
            //const string fileNew = @".\TestOutput\randomSeedMaterialCube.vox";
            const string fileNew = @"C:\Users\Christopher\AppData\Roaming\SpaceEngineers\Saves\76561197961224864\Easy 01.045.013\hopebase5120.vox";

            SpaceEngineersCore.LoadDefinitions();

            var materials = SpaceEngineersApi.GetMaterialList();
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

            // Strip the original material.
            voxelMap.RemoveMaterial(stoneMaterial.Id.SubtypeId, null);

            voxelMap.Save(fileNew);

            var lengthNew = new FileInfo(fileNew).Length;

            Assert.AreEqual(128, voxelMap.Size.X, "Voxel Bounding size must match.");
            Assert.AreEqual(128, voxelMap.Size.Y, "Voxel Bounding size must match.");
            Assert.AreEqual(128, voxelMap.Size.Z, "Voxel Bounding size must match.");

            Assert.AreEqual(64, voxelMap.ContentSize.X, "Voxel Content size must match.");
            Assert.AreEqual(64, voxelMap.ContentSize.Y, "Voxel Content size must match.");
            Assert.AreEqual(64, voxelMap.ContentSize.Z, "Voxel Content size must match.");
        }
    }
}
