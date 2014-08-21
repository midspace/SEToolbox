﻿namespace ToolboxTest
{
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRageMath;

    [TestClass]
    public class InteropTests
    {
        [TestMethod]
        public void ApiInterop()
        {
            SpaceEngineersCore.LoadDefinitions();

            var d1 = SpaceEngineersApi.GetCubeDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_GravityGenerator)), MyCubeSize.Large, "");
            Assert.AreEqual("DisplayName_Block_GravityGenerator", d1.DisplayName, "Must match");
            Assert.AreEqual(MyCubeSize.Large, d1.CubeSize, "Must match");

            var d2 = SpaceEngineersApi.GetCubeDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_GravityGenerator)), MyCubeSize.Small, "");
            Assert.IsNull(d2, "Must be null");

            var d3 = SpaceEngineersApi.GetCubeDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_Gyro)), MyCubeSize.Small, "SmallBlockGyro");
            Assert.AreEqual("DisplayName_Block_Gyroscope", d3.DisplayName, "Must match");
            Assert.AreEqual(MyCubeSize.Small, d3.CubeSize, "Must match");

            var d4 = SpaceEngineersApi.GetCubeDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_Gyro)), MyCubeSize.Small, "Fake");
            Assert.IsNull(d4, "Must be null");
        }

        [TestMethod]
        public void LoadSandbox_Oldmethod_Fighter()
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var fighterPath = Path.Combine(contentPath, @"Data\Prefabs\Fighter.sbc");
            Assert.IsTrue(File.Exists(fighterPath), "Sandbox content file should exist");

            MyObjectBuilder_Definitions prefabDefinitions;
            var ret = SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Definitions, MyObjectBuilder_DefinitionsSerializer>(fighterPath, out prefabDefinitions);

            Assert.IsNotNull(prefabDefinitions, "Sandbox content should not be null");
            Assert.IsTrue(ret, "Sandbox content should have been detected");
            Assert.IsTrue(prefabDefinitions.Prefabs[0].CubeGrid.CubeBlocks.Count > 10, "Sandbox content should have cube blocks");
        }

        [TestMethod]
        public void LoadSandbox_Fighter()
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var fighterPath = Path.Combine(contentPath, @"Data\Prefabs\Fighter.sbc");
            Assert.IsTrue(File.Exists(fighterPath), "Sandbox content file should exist");

            MyObjectBuilder_Definitions prefabDefinitions;
            bool isCompressed;
            var ret = SpaceEngineersApi.TryReadSpaceEngineersFile(fighterPath, out prefabDefinitions, out isCompressed);

            Assert.IsNotNull(prefabDefinitions, "Sandbox content should not be null");
            Assert.IsTrue(ret, "Sandbox content should have been detected");
            Assert.IsFalse(isCompressed, "Sandbox content should not be compressed");
            Assert.IsTrue(prefabDefinitions.Prefabs[0].CubeGrid.CubeBlocks.Count > 10, "Sandbox content should have cube blocks");
        }

        [TestMethod]
        public void LoadSandbox_Compressed_BaseEasyStart()
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var baseEasyStart1Path = Path.Combine(contentPath, @"Data\Prefabs\BaseEasyStart1.sbc");
            Assert.IsTrue(File.Exists(baseEasyStart1Path), "Sandbox content file should exist");

            MyObjectBuilder_Definitions prefabDefinitions;
            bool isCompressed;
            var ret = SpaceEngineersApi.TryReadSpaceEngineersFile(baseEasyStart1Path, out prefabDefinitions, out isCompressed);

            Assert.IsNotNull(prefabDefinitions, "Sandbox content should not be null");
            Assert.IsTrue(ret, "Sandbox content should have been detected");
            Assert.IsTrue(isCompressed, "Sandbox content should be compressed");
            Assert.IsTrue(prefabDefinitions.Prefabs[0].CubeGrid.CubeBlocks.Count > 10, "Sandbox content should have cube blocks");
        }

        [TestMethod]
        public void RotateComponent()
        {
            var d1 = SpaceEngineersApi.GetCubeDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_Thrust)), MyCubeSize.Large, "LargeBlockLargeThrust");
            Assert.AreEqual("DisplayName_Block_LargeThrust", d1.DisplayName, "Must match");
            Assert.AreEqual(MyCubeSize.Large, d1.CubeSize, "Must match");

            Assert.AreEqual(3, d1.Size.X, "Must match");
            Assert.AreEqual(2, d1.Size.Y, "Must match");
            Assert.AreEqual(4, d1.Size.Z, "Must match");

            //======//

            var orient = new SerializableBlockOrientation(Base6Directions.Direction.Forward, Base6Directions.Direction.Up);

            var f = Base6Directions.GetVector(orient.Forward);
            var u = Base6Directions.GetVector(orient.Up);

            var m = Matrix.CreateFromDir(f, u);
            var q = Quaternion.CreateFromRotationMatrix(m);

            var nf = Base6Directions.GetForward(q);
            var nu = Base6Directions.GetUp(q);

            // Test that Space Engineers orientation methods are working as expected. Forward is still Forward, Up is still Up.
            Assert.AreEqual(nf, orient.Forward, "Initial Orientation Forward must match.");
            Assert.AreEqual(nu, orient.Up, "Initial Orientation Forward must match.");

            //======//

            var v = d1.Size.ToVector3();
            var fV1 = Vector3.Transform(v, m);

            // Orientation of Forward/Up should provide the exact same dimentions as the original Component above.
            Assert.AreEqual(3, fV1.X, "Must match");
            Assert.AreEqual(2, fV1.Y, "Must match");
            Assert.AreEqual(4, fV1.Z, "Must match");

            //======//

            var newOrient = new SerializableBlockOrientation(Base6Directions.Direction.Down, Base6Directions.Direction.Right);

            var newM = Matrix.CreateFromDir(Base6Directions.GetVector(newOrient.Forward), Base6Directions.GetVector(newOrient.Up));

            var fV2 = Vector3.Transform(v, newM);

            // The reoriented Component size should now have changed.
            Assert.AreEqual(2, fV2.X, "Must match");
            Assert.AreEqual(4, fV2.Y, "Must match");
            Assert.AreEqual(3, fV2.Z, "Must match");

            //======//

            // Reducing complexity of code with Extension.
            var direction = new SerializableBlockOrientation(Base6Directions.Direction.Down, Base6Directions.Direction.Right);
            var fV3 = d1.Size.Transform(direction);

            // The reoriented Component size should now have changed.
            Assert.AreEqual(2, fV3.X, "Must match");
            Assert.AreEqual(4, fV3.Y, "Must match");
            Assert.AreEqual(3, fV3.Z, "Must match");
        }
    }
}
