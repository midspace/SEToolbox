namespace ToolboxTest
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.VRageData;
    using SEToolbox.Interop;
    using SEToolbox.Support;

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
        public void ExtractZipFileToFolder()
        {
            var filename = @".\TestAssets\Sample World.sbw";
            var folder = @".\TestAssets\Sample World";

            ZipTools.MakeClearDirectory(folder);
            ZipTools.ExtractZipFile(filename, null, folder);
        }

        [TestMethod]
        public void ExtractSandboxFromZip()
        {
            var filename = @".\TestAssets\Sample World.sbw";

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
            var filename = @".\TestAssets\SANDBOX_0_0_0_.sbs";

            var xmlfilename = @".\TestAssets\SANDBOX_0_0_0_.xml";

            ZipTools.GZipUncompress(filename, xmlfilename);
        }

        [TestMethod]
        public void ExtractZipAndRepack()
        {
            var filename = @".\TestAssets\Sample World.sbw";
            var folder = @".\TestAssets\Sample World";

            ZipTools.MakeClearDirectory(folder);
            ZipTools.ExtractZipFile(filename, null, folder);

            var newFilename = @".\TestAssets\New World.sbw";
            ZipTools.ZipFolder(folder, null, newFilename);
        }


        [TestMethod]
        public void ColorTest()
        {
            var c1 = Color.FromArgb(255, 255, 255);
            var c2 = Color.FromArgb(0, 0, 0);
            var c3 = Color.FromArgb(0, 1, 1);   //PS = H:180, S:100%, B:1%

            var h3 = c3.GetHue();
            var s3 = c3.GetSaturation();
            var b3 = c3.GetBrightness();

            var v3 = new SerializableVector3(c3.GetHue() / 360f, c3.GetSaturation() * 2 - 1, c3.GetBrightness() * 2 - 1);
        }
    }
}
