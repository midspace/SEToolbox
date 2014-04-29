namespace ToolboxTest
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System.Collections.Generic;

    [TestClass]
    public class UnitTest1
    {
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
        public void ExtractZipFileToFolder()
        {
            const string filename = @".\TestAssets\Sample World.sbw";
            const string folder = @".\TestAssets\Sample World";

            ZipTools.MakeClearDirectory(folder);
            ZipTools.ExtractZipFile(filename, null, folder);
        }

        [TestMethod]
        public void ExtractSandboxFromZip()
        {
            const string filename = @".\TestAssets\Sample World.sbw";

            MyObjectBuilder_Checkpoint checkpoint;
            using (var stream = ZipTools.ReadFile(filename, null, SpaceEngineersConsts.SandBoxCheckpointFilename))
            {
                checkpoint = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(stream);
            }

            Assert.AreEqual("Quad Scissor Doors", checkpoint.SessionName, "Checkpoint SessionName must match!");
        }

        [TestMethod]
        public void ExtractContentFromCompressedSandbox()
        {
            const string filename = @".\TestAssets\SANDBOX_0_0_0_.sbs";

            const string xmlfilename = @".\TestAssets\SANDBOX_0_0_0_.xml";

            ZipTools.GZipUncompress(filename, xmlfilename);
        }

        [TestMethod]
        public void ExtractZipAndRepack()
        {
            const string filename = @".\TestAssets\Sample World.sbw";
            const string folder = @".\TestAssets\Sample World";

            ZipTools.MakeClearDirectory(folder);
            ZipTools.ExtractZipFile(filename, null, folder);

            const string newFilename = @".\TestAssets\New World.sbw";
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
    }
}
