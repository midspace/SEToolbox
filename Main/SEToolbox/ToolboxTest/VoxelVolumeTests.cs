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
    using System.Timers;

    [TestClass]
    public class VoxelVolumeTests
    {
        [TestMethod]
        public void VoxelConvertToVolmeticOdd()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            var scale = new ScaleTransform3D(5, 5, 5);
            var rotateTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), 0, 0, 0);
            var traceType = SEToolbox.Interop.Asteroids.TraceType.Odd;
            var traceCount = SEToolbox.Interop.Asteroids.TraceCount.Trace5;
            var traceDirection = SEToolbox.Interop.Asteroids.TraceDirection.XYZ;

            var asteroidFile = @".\TestOutput\test_sphere_odd.vox";

            var model = MeshHelper.Load(modelFile, ignoreErrors: true);
            var meshes = new List<SEToolbox.Interop.Asteroids.MyVoxelRayTracer.MyMeshModel>();
            foreach (var model3D in model.Children)
            {
                var gm = (GeometryModel3D)model3D;
                var geometry = gm.Geometry as MeshGeometry3D;

                if (geometry != null)
                    meshes.Add(new MyVoxelRayTracer.MyMeshModel(new[] { geometry }, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId));
            }

            var voxelMap = MyVoxelRayTracer.ReadModelAsteroidVolmetic(model, meshes, scale, rotateTransform, traceType, traceCount, traceDirection,
                ResetProgress, IncrementProgress, null, CompleteProgress);
            voxelMap.Save(asteroidFile);

            Assert.IsTrue(File.Exists(asteroidFile), "Generated file must exist");

            var voxelFileLength = new FileInfo(asteroidFile).Length;
            //Assert.AreEqual(2021, voxelFileLength, "File size must match.");
            Assert.AreEqual(new Vector3I(64, 64, 64), voxelMap.Size, "Voxel Bounding size must match.");
            Assert.AreEqual(new Vector3I(25, 25, 25), voxelMap.ContentSize, "Voxel Content size must match.");

            var voxCells = voxelMap.SumVoxelCells();
            Assert.AreEqual(2031782, voxCells, "Voxel cells must match.");
        }

        [TestMethod]
        public void VoxelConvertToVolmeticEven()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            var scale = new ScaleTransform3D(5, 5, 5);
            var rotateTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), 0, 0, 0);
            var traceType = SEToolbox.Interop.Asteroids.TraceType.Even;
            var traceCount = SEToolbox.Interop.Asteroids.TraceCount.Trace5;
            var traceDirection = SEToolbox.Interop.Asteroids.TraceDirection.XYZ;
            var asteroidFile = @".\TestOutput\test_sphere_even.vox";

            var model = MeshHelper.Load(modelFile, ignoreErrors: true);
            var meshes = new List<SEToolbox.Interop.Asteroids.MyVoxelRayTracer.MyMeshModel>();
            foreach (var model3D in model.Children)
            {
                var gm = (GeometryModel3D)model3D;
                var geometry = gm.Geometry as MeshGeometry3D;

                if (geometry != null)
                    meshes.Add(new MyVoxelRayTracer.MyMeshModel(new[] { geometry }, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId));
            }

            var voxelMap = MyVoxelRayTracer.ReadModelAsteroidVolmetic(model, meshes, scale, rotateTransform, traceType, traceCount, traceDirection,
                ResetProgress, IncrementProgress, CheckCancel, CompleteProgress);
            voxelMap.Save(asteroidFile);

            Assert.IsTrue(File.Exists(asteroidFile), "Generated file must exist");

            var voxelFileLength = new FileInfo(asteroidFile).Length;
            Assert.AreEqual(1908, voxelFileLength, "File size must match.");
            Assert.AreEqual(new Vector3I(64, 64, 64), voxelMap.Size, "Voxel Bounding size must match.");
            Assert.AreEqual(new Vector3I(26, 26, 26), voxelMap.ContentSize, "Voxel Content size must match.");

            var voxCells = voxelMap.SumVoxelCells();
            Assert.AreEqual(2043244, voxCells, "Voxel cells must match.");
        }

        [TestMethod]
        public void VoxelConvertToVolmeticCancel()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            var scale = new ScaleTransform3D(50, 50, 50);
            var rotateTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), 0, 0, 0);
            var traceType = SEToolbox.Interop.Asteroids.TraceType.Odd;
            var traceCount = SEToolbox.Interop.Asteroids.TraceCount.Trace5;
            var traceDirection = SEToolbox.Interop.Asteroids.TraceDirection.XYZ;

            var model = MeshHelper.Load(modelFile, ignoreErrors: true);
            var meshes = new List<SEToolbox.Interop.Asteroids.MyVoxelRayTracer.MyMeshModel>();
            foreach (var model3D in model.Children)
            {
                var gm = (GeometryModel3D)model3D;
                var geometry = gm.Geometry as MeshGeometry3D;

                if (geometry != null)
                    meshes.Add(new MyVoxelRayTracer.MyMeshModel(new[] { geometry }, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId));
            }

            bool doCancel = false;

            // cancel the convertion after 5 seconds.
            var timer = new Timer(5000);
            timer.Elapsed += delegate
            {
                Debug.WriteLine("Cancelling!!!");
                doCancel = true;
                timer.Stop();
            };
            timer.Start();

            var cancelFunc = (Func<bool>)delegate
            {
                return doCancel;
            };

            var voxelMap = MyVoxelRayTracer.ReadModelAsteroidVolmetic(model, meshes, scale, rotateTransform, traceType, traceCount, traceDirection,
                ResetProgress, IncrementProgress, cancelFunc, CompleteProgress);
            
            Assert.IsNull(voxelMap, "Asteroid must not exist.");
        }

        //[TestMethod]
        public void VoxelConvertToVolmeticMisc()
        {
            SpaceEngineersCore.LoadDefinitions();
            var materials = SpaceEngineersApi.GetMaterialList();

            var stoneMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Stone"));
            Assert.IsNotNull(stoneMaterial, "Stone material should exist.");

            var goldMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Gold"));
            Assert.IsNotNull(goldMaterial, "Gold material should exist.");

            var silverMaterial = materials.FirstOrDefault(m => m.Id.SubtypeId.Contains("Silver"));
            Assert.IsNotNull(silverMaterial, "Silver material should exist.");

            // Basic test...
            var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            var scale = new ScaleTransform3D(5, 5, 5);
            var rotateTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), 0, 0, 0);
            var traceType = SEToolbox.Interop.Asteroids.TraceType.Odd;
            var traceCount = SEToolbox.Interop.Asteroids.TraceCount.Trace5;
            var traceDirection = SEToolbox.Interop.Asteroids.TraceDirection.XYZ;

            // Basic model test...
            //var modelFile = @".\TestAssets\TwoSpheres.3ds";
            //var scale = new ScaleTransform3D(5, 5, 5);

            // Scale test...
            //var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            //var scale = new ScaleTransform3D(20, 20, 20);
            //Transform3D rotateTransform = null;

            // Max Scale test...  will cause an OutOfMemory exception at this scale because MSTest runs in x86.
            //var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            //var scale = new ScaleTransform3D(120, 120, 120);
            //Transform3D rotateTransform = null;

            // Memory test (probably won't load in Space Engineers) ...  will cause an OutOfMemory exception at this scale because MSTest runs in x86.
            //var modelFile = @".\TestAssets\Sphere_Gold.3ds";
            //var scale = new ScaleTransform3D(200, 200, 200);

            // Complexity test...
            //var modelFile = @".\TestAssets\buddha-fixed-bottom.stl";
            //var scale = new ScaleTransform3D(0.78, 0.78, 0.78);
            //var rotateTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), 180, 0, 0);

            var modelMaterials = new string[] { stoneMaterial.Id.SubtypeId, goldMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId, stoneMaterial.Id.SubtypeId };
            var fillerMaterial = silverMaterial.Id.SubtypeId;
            var asteroidFile = @".\TestOutput\test_sphere.vox";

            var model = MeshHelper.Load(modelFile, ignoreErrors: true);
            var meshes = new List<SEToolbox.Interop.Asteroids.MyVoxelRayTracer.MyMeshModel>();
            foreach (var model3D in model.Children)
            {
                var gm = (GeometryModel3D)model3D;
                var geometry = gm.Geometry as MeshGeometry3D;

                if (geometry != null)
                    meshes.Add(new MyVoxelRayTracer.MyMeshModel(new[] { geometry }, "Stone_01", "Stone_01"));
            }

            var voxelMap = MyVoxelRayTracer.ReadModelAsteroidVolmetic(model, meshes, scale, rotateTransform, traceType, traceCount, traceDirection,
                ResetProgress, IncrementProgress, CheckCancel, CompleteProgress);
            voxelMap.Save(asteroidFile);

            Assert.IsTrue(File.Exists(asteroidFile), "Generated file must exist");

            var voxelFileLength = new FileInfo(asteroidFile).Length;

            Assert.IsTrue(voxelFileLength > 0, "File must not be empty.");

            Assert.IsTrue(voxelMap.Size.X > 0, "Voxel Size must be greater than zero.");
            Assert.IsTrue(voxelMap.Size.Y > 0, "Voxel Size must be greater than zero.");
            Assert.IsTrue(voxelMap.Size.Z > 0, "Voxel Size must be greater than zero.");

            Assert.IsTrue(voxelMap.ContentSize.X > 0, "Voxel ContentSize must be greater than zero.");
            Assert.IsTrue(voxelMap.ContentSize.Y > 0, "Voxel ContentSize must be greater than zero.");
            Assert.IsTrue(voxelMap.ContentSize.Z > 0, "Voxel ContentSize must be greater than zero.");

            var voxCells = voxelMap.SumVoxelCells();
            Assert.IsTrue(voxCells > 0, "voxCells must be greater than zero.");
        }

        #region helpers

        private static double _counter;
        private static double _maximumProgress;
        private static int _percent;
        private static System.Diagnostics.Stopwatch _timer;
        public static void ResetProgress(double initial, double maximumProgress)
        {
            _percent = 0;
            _counter = initial;
            _maximumProgress = maximumProgress;
            _timer = new System.Diagnostics.Stopwatch();
            _timer.Start();

            Debug.WriteLine("{0}%  {1:#,##0}/{2:#,##0}  {3}/Estimating", _percent, _counter, _maximumProgress, _timer.Elapsed);
        }

        public static void IncrementProgress()
        {
            _counter++;

            var p = (int)((double)_counter / _maximumProgress * 100);

            if (_percent < p)
            {
                var elapsed = _timer.Elapsed;
                var estimate = new TimeSpan(p == 0 ? 0 : (long)((double)elapsed.Ticks / ((double)p / 100f)));
                _percent = p;
                Debug.WriteLine("{0}%  {1:#,##0}/{2:#,##0}  {3}/{4}", _percent, _counter, _maximumProgress, elapsed, estimate);
            }
        }

        public static bool CheckCancel()
        {
            return false;
        }

        public static void CompleteProgress()
        {
            Debug.WriteLine("Complete Step finished.");
        }

        #endregion
    }
}
