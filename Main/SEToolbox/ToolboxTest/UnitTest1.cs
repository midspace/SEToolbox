namespace ToolboxTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox;
    using SEToolbox.Support;
    using System.Drawing.Imaging;
    using System.IO;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestImageOptimizer1()
        {
            var filename = Path.GetFullPath(@"..\..\..\..\building 3D\images\7242630_orig.jpg");
            var bmp = ToolboxExtensions.OptimizeImagePalette(filename);
            var outputFileTest = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + "_optimized" + ".png");
            bmp.Save(outputFileTest, ImageFormat.Png);
        }

        [TestMethod]
        public void TestImageOptimizer2()
        {
            var filename = Path.GetFullPath(@"..\..\..\..\building 3D\images\7242630_scale432.png");
            var bmp = ToolboxExtensions.OptimizeImagePalette(filename);
            var outputFileTest = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + "_optimized" + ".png");
            bmp.Save(outputFileTest, ImageFormat.Png);
        }

        [TestMethod]
        public void TestXMLCompacter1()
        {
            var filenameSource = Path.GetFullPath(@".\test.xml");
            var filenameDestination = Path.GetFullPath(@".\test_out.xml");
            ToolboxExtensions.CompactXmlFile(filenameSource, filenameDestination);

            var oldFileSize = new FileInfo(filenameSource).Length;
            var newFileSize = new FileInfo(filenameDestination).Length;

            Assert.IsTrue(newFileSize < oldFileSize, "new file size must be smaller");
            Assert.AreEqual(2225, oldFileSize, "original file size");
            Assert.AreEqual(1510, newFileSize, "new file size");
        }

        //[TestMethod]
        //public void TestXMLCompacter2()
        //{
        //    var filenameSource = Path.GetFullPath(@".\test.xml");
        //    var filenameDestination = Path.GetFullPath(@".\test_out.xml");
        //    Toolbox.CompactXmlFile2(filenameSource, filenameDestination);

        //    var oldFileSize = new FileInfo(filenameSource).Length;
        //    var newFileSize = new FileInfo(filenameDestination).Length;

        //    Assert.IsTrue(newFileSize < oldFileSize, "new file size must be smaller");
        //    Assert.AreEqual(2225, oldFileSize, "original file size");
        //    Assert.AreEqual(1510, newFileSize, "new file size");
        //}

        //[TestMethod]
        //public void TestXMLCompacter222()
        //{
        //    var filenameSource = Path.GetFullPath(@"D:\UserData\Christopher\AppData\Roaming\SpaceEngineers\Saves\76561197961224864\Space Engineers Alpha Image\SANDBOX_0_0_0_.sbs");
        //    var filenameDestination = Path.GetFullPath(@"D:\UserData\Christopher\AppData\Roaming\SpaceEngineers\Saves\76561197961224864\Space Engineers Alpha Image\SANDBOX_0_0_0_.sbs_out");
        //    Toolbox.CompactXmlFile(filenameSource, filenameDestination);
        //}
        
    }
}
