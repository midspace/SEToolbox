﻿namespace ToolboxTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Models;
    using SEToolbox.Support;

    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void BaseModel1LoadSave()
        {
            SpaceEngineersCore.LoadDefinitions();
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var largeThruster = (MyObjectBuilder_CubeBlockDefinition)SpaceEngineersApi.GetDefinition(SpaceEngineersConsts.Thrust, "LargeBlockLargeThrust");
            var thrusterModelPath = Path.Combine(contentPath, largeThruster.Model);
            Assert.IsTrue(File.Exists(thrusterModelPath), "Filepath should exist on developer machine");

            var modelData2 = MyModel.LoadModelData(thrusterModelPath);
            var modelData = MyModel.LoadCustomModelData(thrusterModelPath);

            var testFilePath = @".\TestOutput\Thruster.mwm";
            MyModel.SaveModelData(testFilePath, modelData);

            var originalBytes = File.ReadAllBytes(thrusterModelPath);
            var newBytes = File.ReadAllBytes(testFilePath);

            Assert.AreEqual(originalBytes.Length, newBytes.Length, "Bytestream content must equal");
            Assert.IsTrue(originalBytes.SequenceEqual(newBytes), "Bytestream content must equal");
        }

        [TestMethod]
        public void CustomModel1LoadSave()
        {
            SpaceEngineersCore.LoadDefinitions();
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var cockpitModelPath = Path.Combine(contentPath, @"Models\Characters\Animations\cockpit.mwm");
            Assert.IsTrue(File.Exists(cockpitModelPath), "Filepath should exist on developer machine");

            var modelData = MyModel.LoadCustomModelData(cockpitModelPath);

            var testFilePath = @".\TestOutput\cockpit_animation.mwm";
            MyModel.SaveModelData(testFilePath, modelData);

            var originalBytes = File.ReadAllBytes(cockpitModelPath);
            var newBytes = File.ReadAllBytes(testFilePath);

            Assert.AreEqual(originalBytes.Length, newBytes.Length, "Bytestream content must equal");
            Assert.IsTrue(originalBytes.SequenceEqual(newBytes), "Bytestream content must equal");
        }

        //[TestMethod]
        public void LoadModelFailures()
        {
            SpaceEngineersCore.LoadDefinitions();
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var files = Directory.GetFiles(Path.Combine(contentPath, "Models"), "*.mwm", SearchOption.AllDirectories);
            var badList = new List<string>();
            var convertDiffers = new List<string>();

            foreach (var file in files)
            {
                Dictionary<string, object> data = null;
                try
                {
                    data = MyModel.LoadModelData(file);
                    //data = MyModel.LoadCustomModelData(file);
                }
                catch (Exception)
                {
                    badList.Add(file);
                    continue;
                }

                if (data != null)
                {
                    var testFilePath = @".\TestOutput\TempModelTest.mwm";

                    MyModel.SaveModelData(testFilePath, data);

                    var originalBytes = File.ReadAllBytes(file);
                    var newBytes = File.ReadAllBytes(testFilePath);

                    if (!originalBytes.SequenceEqual(newBytes))
                    {
                        convertDiffers.Add(file);
                    }

                    //Assert.AreEqual(originalBytes.Length, newBytes.Length, "File {0} Bytestream content must equal", file);
                    //Assert.IsTrue(originalBytes.SequenceEqual(newBytes), "File {0} Bytestream content must equal", file);
                }
            }

            Assert.IsTrue(convertDiffers.Count > 0, "");
            Assert.IsTrue(badList.Count > 0, "");
        }
    }
}
