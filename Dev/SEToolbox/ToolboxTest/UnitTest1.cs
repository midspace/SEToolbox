namespace ToolboxTest
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;
    using Color = System.Drawing.Color;

    [TestClass]
    // TODO: should use DeploymentItem attributes, but this will screw up all file dependant tests.
    //       It's an all or nothing, approach to get the correct binaries into the tests.
    //       Also, need some way of getting the Test Settings to sticking to X64 architecture.
    //[DeploymentItem(@"TestAssets\SANDBOX_0_0_0_.XML.sbs", "TestAssets")]
    public class UnitTest1
    {
        [TestInitialize]
        public void InitTest()
        {
            try
            {
                SpaceEngineersCore.LoadDefinitions();
            }
            // For debugging tests.
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void GenerateTempFiles()
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
            var filenameDestination = Path.GetFullPath(@".\TestOutput\test_out.xml");
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
        public void ExtractZipFileToFolder()
        {
            const string filename = @".\TestAssets\Sample World.sbw";
            const string folder = @".\TestOutput\Sample World";

            ZipTools.MakeClearDirectory(folder);
            ZipTools.ExtractZipFileToDirectory(filename, null, folder);
        }

        [TestMethod]
        public void ExtractSandboxFromZip()
        {
            const string filename = @".\TestAssets\Sample World.sbw";

            MyObjectBuilder_Checkpoint checkpoint;
            using (var stream = ZipTools.ExtractZipFileToSteam(filename, null, SpaceEngineersConsts.SandBoxCheckpointFilename))
            {
                checkpoint = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Checkpoint>(stream);
            }

            Assert.AreEqual("Quad Scissor Doors", checkpoint.SessionName, "Checkpoint SessionName must match!");
        }

        [TestMethod]
        public void ExtractContentFromCompressedSandbox()
        {
            const string filename = @".\TestAssets\SANDBOX_0_0_0_.sbs";

            const string xmlfilename = @".\TestOutput\SANDBOX_0_0_0_.xml";

            ZipTools.GZipUncompress(filename, xmlfilename);
        }

        [TestMethod]
        public void ExtractContentFromXmlSandbox()
        {
            const string filename = @".\TestAssets\SANDBOX_0_0_0_.XML.sbs";

            MyObjectBuilder_Sector sectorData;
            bool isCompressed;
            SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Sector>(filename, out sectorData, out isCompressed);

            Assert.IsFalse(isCompressed, "file should not be compressed");
            Assert.IsNotNull(sectorData, "sectorData != null");
            Assert.IsTrue(sectorData.SectorObjects.Count > 0, "sectorData should be more than 0");
        }


        [TestMethod]
        public void ExtractContentFromProtoBufSandbox()
        {
            // filename will automatically be concatenated with "PB"
            const string filename = @".\TestAssets\SANDBOX_0_0_0_.Proto.sbs";

            MyObjectBuilder_Sector sectorData;
            bool isCompressed;
            SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Sector>(filename, out sectorData, out isCompressed);

            Assert.IsFalse(isCompressed, "file should not be compressed");
            Assert.IsNotNull(sectorData, "sectorData != null");
            Assert.IsTrue(sectorData.SectorObjects.Count > 0, "sectorData should be more than 0");
        }

        [TestMethod]
        public void ExtractZipAndRepack()
        {
            const string filename = @".\TestAssets\Sample World.sbw";
            const string folder = @".\TestOutput\Sample World";

            ZipTools.MakeClearDirectory(folder);
            ZipTools.ExtractZipFileToDirectory(filename, null, folder);

            const string newFilename = @".\TestOutput\New World.sbw";
            ZipTools.ZipFolder(folder, null, newFilename);
        }

        [TestMethod]
        public void SandboxColorTest()
        {
            var colors = new Color[]
            {
                Color.FromArgb(255, 255, 255),
                Color.FromArgb(0, 0, 0),
                Color.FromArgb(0, 1, 1),   // PhotoShop = H:180, S:100%, B:1%
                Color.FromArgb(255, 0, 0),
                Color.FromArgb(0, 255, 0),
                Color.FromArgb(0, 0, 255),
            };

            var hsvList = new List<SerializableVector3>();
            foreach (var color in colors)
            {
                hsvList.Add(color.ToSandboxHsvColor());
            }

            var rgbList = new List<Color>();
            foreach (var hsv in hsvList)
            {
                rgbList.Add(hsv.ToSandboxDrawingColor());
            }

            var rgbArray = rgbList.ToArray();

            for (var i = 0; i < colors.Length; i++)
            {
                Assert.AreEqual(rgbArray[i].R, colors[i].R, "Red Should Equal");
                Assert.AreEqual(rgbArray[i].B, colors[i].B, "Blue Should Equal");
                Assert.AreEqual(rgbArray[i].G, colors[i].G, "Green Should Equal");
            }
        }

        [TestMethod]
        public void VRageColorTest()
        {
            var c1 = Color.FromArgb(255, 255, 255);
            var c2 = Color.FromArgb(0, 0, 0);
            var c3 = Color.FromArgb(0, 1, 1);   //PS = H:180, S:100%, B:1%

            var vColor1 = VRageMath.ColorExtensions.ColorToHSV(new VRageMath.Color(c1.R, c1.B, c1.G));
            var vColor2 = VRageMath.ColorExtensions.ColorToHSV(new VRageMath.Color(c2.R, c2.B, c2.G));
            var vColor3 = VRageMath.ColorExtensions.ColorToHSV(new VRageMath.Color(c3.R, c3.B, c3.G));

            var rgb1 = VRageMath.ColorExtensions.HSVtoColor(vColor1);
            var rgb2 = VRageMath.ColorExtensions.HSVtoColor(vColor2);
            var rgb3 = VRageMath.ColorExtensions.HSVtoColor(vColor3);

            Assert.AreEqual(rgb1.R, c1.R, "Red Should Equal");
            Assert.AreEqual(rgb1.B, c1.B, "Blue Should Equal");
            Assert.AreEqual(rgb1.G, c1.G, "Green Should Equal");

            Assert.AreEqual(rgb2.R, c2.R, "Red Should Equal");
            Assert.AreEqual(rgb2.B, c2.B, "Blue Should Equal");
            Assert.AreEqual(rgb2.G, c2.G, "Green Should Equal");

            Assert.AreEqual(rgb3.R, c3.R, "Red Should Equal");
            Assert.AreEqual(rgb3.B, c3.B, "Blue Should Equal");
            Assert.AreEqual(rgb3.G, c3.G, "Green Should Equal");
        }

        [TestMethod]
        public void SingleConversionTest()
        {
            Single f1 = -17.6093254f;
            Single f2 = 72.215f;
            Single f3 = -218.569977f;

            Double d1 = Convert.ToDouble(f1);
            Single g1 = Convert.ToSingle(d1);

            Double d2 = Convert.ToDouble(f2);
            Single g2 = Convert.ToSingle(d2);

            Double d3 = Convert.ToDouble(f3);
            Single g3 = Convert.ToSingle(d3);

            Assert.AreEqual(f1, g1, "Should Equal");
            Assert.AreEqual(f2, g2, "Should Equal");
            Assert.AreEqual(f3, g3, "Should Equal");
        }

        [TestMethod]
        public void BoundingBoxIntersectKeen()
        {
            var point = new VRageMath.Vector3D(5d, 3.5d, 4d);
            var vector = new VRageMath.Vector3D(-0.03598167d, 0.0110336d, 0.9992915d);
            var box = new VRageMath.BoundingBoxD(new VRageMath.Vector3D(3d, 3d, 2d), new VRageMath.Vector3D(7d, 4d, 6d));
            var ray = new VRageMath.RayD(point, vector);

            double? f = box.Intersects(ray);

            Assert.AreEqual(0, f, "Should Equal");
        }

        [TestMethod]
        public void BoundingBoxIntersectCustom()
        {
            var point = new VRageMath.Vector3D(5d, 3.5d, 4d);
            var vector = new VRageMath.Vector3D(-0.03598167d, 0.0110336d, 0.9992915d);
            var box = new VRageMath.BoundingBoxD(new VRageMath.Vector3D(3d, 3d, 2d), new VRageMath.Vector3D(7d, 4d, 6d));

            VRageMath.Vector3D? p = box.IntersectsRayAt(point, vector * 1000);

            Assert.AreEqual(new VRageMath.Vector3D(4.9176489098920966d, 3.5151384795308953d, 6.00000000000319d), p.Value, "Should Equal");
        }

        [TestMethod]
        public void CubeRotate()
        {
            var positionOrientation = new MyPositionAndOrientation(new Vector3D(10, 10, 10), Vector3.Backward, Vector3.Up);
            var gridSizeEnum = MyCubeSize.Large;

            var cube = (MyObjectBuilder_CubeBlock)MyObjectBuilderSerializer.CreateNewObject(typeof(MyObjectBuilder_CubeBlock), "LargeBlockArmorBlock");
            //var cube = (MyObjectBuilder_CubeBlock)MyObjectBuilderSerializer.CreateNewObject(typeof(MyObjectBuilder_Thrust), "LargeBlockLargeThrust");
            cube.Min = new SerializableVector3I(10, 10, 10);
            cube.BlockOrientation = new SerializableBlockOrientation(Base6Directions.Direction.Forward, Base6Directions.Direction.Up);
            cube.BuildPercent = 1;

            var quaternion = positionOrientation.ToQuaternionD();
            var definition = SpaceEngineersApi.GetCubeDefinition(cube.TypeId, gridSizeEnum, cube.SubtypeName);


            var orientSize = definition.Size.Transform(cube.BlockOrientation).Abs();
            var min = cube.Min.ToVector3D() * gridSizeEnum.ToLength();
            var max = (cube.Min + orientSize).ToVector3D() * gridSizeEnum.ToLength();
            var p1 = min.Transform(quaternion) + positionOrientation.Position;
            var p2 = max.Transform(quaternion) + positionOrientation.Position;
            var nb = new BoundingBoxD(p1, p2);
        }

        /// <summary>
        /// This test is critical for rotation a station. For some reason, 
        /// if the rotation is not exactly 1, or 0, then there is an issue placing cubes on the station.
        /// </summary>
        [TestMethod]
        public void Rotation()
        {
            var positionAndOrientation = new MyPositionAndOrientation(
                    new SerializableVector3D(10.0d, -10.0d, -2.5d),
                    new SerializableVector3(0.0f, 0.0f, -1.0f),
                    new SerializableVector3(0.0f, 1.0f, 0.0f));

            // -90 around Z
            var quaternion = Quaternion.CreateFromYawPitchRoll(0, 0, -VRageMath.MathHelper.PiOver2);
            var o = positionAndOrientation.ToQuaternion() * quaternion;
            var on = Quaternion.Normalize(o);
            var p = new MyPositionAndOrientation(on.ToMatrix());

            var quaternion2 = QuaternionD.CreateFromYawPitchRoll(0, 0, -Math.PI / 2);
            var o2 = positionAndOrientation.ToQuaternionD() * quaternion2;
            var on2 = QuaternionD.Normalize(o2);
            var p2 = new MyPositionAndOrientation(on2.ToMatrixD());

            var quaternion3 = new System.Windows.Media.Media3D.Quaternion(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), -90d);
            var x3 = positionAndOrientation.ToQuaternionD();
            var o3 = new System.Windows.Media.Media3D.Quaternion(x3.X, x3.Y, x3.Z, x3.W)*quaternion3;
            var on3 = o3;
            on3.Normalize();


            double num = on3.X * on3.X;
            double num3 = on3.Z * on3.Z;
            double num4 = on3.X * on3.Y;
            double num5 = on3.Z * on3.W;
            double num8 = on3.Y * on3.Z;
            double num9 = on3.X * on3.W;
            var M21 = (2.0d * (num4 - num5));
            var M22 = (1.0d - 2.0d * (num3 + num));
            var M23 = (2.0d * (num8 + num9));

            var up3 = new Vector3D(M21, M22, M23);

            


            var fwd = new SerializableVector3(0.0f, 0.0f, -1.0f);
            var up = new SerializableVector3(1.0f, 0.0f, 0.0f);

            Assert.AreEqual(fwd.X, p.Forward.X, "Forward.X Should Equal");
            Assert.AreEqual(fwd.Y, p.Forward.Y, "Forward.Y Should Equal");
            Assert.AreEqual(fwd.Z, p.Forward.Z, "Forward.Z Should Equal");
            Assert.AreEqual(up.X, p.Up.X, "Up.X Should Equal");
            Assert.AreEqual(up.Y, p.Up.Y, "Up.Y Should Equal");
            Assert.AreEqual(up.Z, p.Up.Z, "Up.Z Should Equal");
        }
    }
}
