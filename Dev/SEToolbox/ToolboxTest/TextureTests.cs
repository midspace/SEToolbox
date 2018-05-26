namespace ToolboxTest
{
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using SEToolbox.ImageLibrary;
    using SEToolbox.ImageLibrary.Effects;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage.ObjectBuilders;
    using Brushes = System.Drawing.Brushes;
    using TexUtil = SEToolbox.ImageLibrary.ImageTextureUtil;

    [TestClass]
    public class TextureTests
    {
        private string _path;

        [TestInitialize]
        public void InitTest()
        {
            SpaceEngineersCore.LoadDefinitions();
            _path = Path.GetFullPath(".\\TestOutput");

            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        [TestMethod, TestCategory("UnitTest"), TestCategory("DX9")]
        public void LoadComponentTextures()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Models\Cubes\DoorBlock_cm.dds"));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\GUI\Icons\Cubes\ExplosivesComponent.dds"));


            var magnesiumOre = MyDefinitionManager.Static.GetDefinition(SpaceEngineersTypes.Ore, "Magnesium");
            Assert.IsTrue(magnesiumOre is MyPhysicalItemDefinition, "Type should match");
            TestLoadTextureAndExport(Path.Combine(contentPath, magnesiumOre.Icons.First()));


            var goldIngot = MyDefinitionManager.Static.GetDefinition(SpaceEngineersTypes.Ingot, "Gold");
            Assert.IsTrue(goldIngot is MyPhysicalItemDefinition, "Type should match");
            TestLoadTextureAndExport(Path.Combine(contentPath, goldIngot.Icons.First()));


            var ammoMagazine = MyDefinitionManager.Static.GetDefinition(SpaceEngineersTypes.AmmoMagazine, "NATO_5p56x45mm");
            Assert.IsTrue(ammoMagazine is MyAmmoMagazineDefinition, "Type should match");
            TestLoadTextureAndExport(Path.Combine(contentPath, ammoMagazine.Icons.First()));


            var steelPlate = MyDefinitionManager.Static.GetDefinition(SpaceEngineersTypes.Component, "SteelPlate");
            Assert.IsTrue(steelPlate is MyComponentDefinition, "Type should match");
            TestLoadTextureAndExport(Path.Combine(contentPath, steelPlate.Icons.First()));
            

            var smallBlockLandingGear = MyDefinitionManager.Static.GetDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_LandingGear)), "SmallBlockLandingGear");
            Assert.IsTrue(smallBlockLandingGear is MyCubeBlockDefinition, "Type should match");
            TestLoadTextureAndExport(Path.Combine(contentPath, smallBlockLandingGear.Icons.First()));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\GUI\Controls\grid_item.dds"));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\BackgroundCube\Prerender\Sun.dds"));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Voxels\Gold_01_ForAxisXZ_cm.dds"));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Voxels\Silicon_01_ForAxisXZ_cm.dds"));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Voxels\Platinum_01_ForAxisXZ_cm.dds"));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_cm.dds"));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_cm.dds"), true);


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_ng.dds"));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_ng.dds"), true);
        }

        [TestMethod, TestCategory("UnitTest"), TestCategory("DX10")]
        public void LoadComponentTexturesDx10PremultipliedAlpha()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\GUI\Icons\Cubes\AdvancedMotor.dds"));

            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\GUI\Icons\component\ExplosivesComponent.dds"));
        }

        [TestMethod, TestCategory("UnitTest"), TestCategory("DX11")]
        public void LoadComponentTexturesDx11()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Models\Cubes\large_medical_room_cm.dds"));


            TestLoadTextureAndExport(Path.Combine(contentPath, @"Textures\Models\Cubes\large_medical_room_cm.dds"), true);
        }

        private Bitmap TestLoadTexture(string textureFilePath, int depthSlice = 0, int width = -1, int height = -1, bool ignoreAlpha = false)
        {
            Assert.IsTrue(File.Exists(textureFilePath), $"Filepath {textureFilePath} should exist on developer machine.");

            string name = Path.GetFileNameWithoutExtension(textureFilePath) + (ignoreAlpha ? "_alpha" : "");
            Bitmap textureFilePathBmp;
            using (var stream = File.OpenRead(textureFilePath))
            {
                textureFilePathBmp = TexUtil.CreateBitmap(stream, textureFilePath, depthSlice, width, height, ignoreAlpha);
            }
            Assert.IsNotNull(textureFilePathBmp, $"Texture for {name} should not be null.");

            return textureFilePathBmp;
        }


        private void TestLoadTextureAndExport(string textureFilePath, bool ignoreAlpha = false)
        {
            Assert.IsTrue(File.Exists(textureFilePath), $"Filepath {textureFilePath} should exist on developer machine.");

            string name = Path.GetFileNameWithoutExtension(textureFilePath) + (ignoreAlpha ? "_alpha" : "");
            Bitmap textureFilePathBmp;
            using (var stream = File.OpenRead(textureFilePath))
            {
                textureFilePathBmp = TexUtil.CreateBitmap(stream, textureFilePath, ignoreAlpha: ignoreAlpha);
            }
            Assert.IsNotNull(textureFilePathBmp, $"Texture for {name} should not be null.");

            File.Copy(textureFilePath, Path.Combine(_path, name + ".dds"), true);
            ImageTextureUtil.WriteImage(textureFilePathBmp, Path.Combine(_path, name + ".png"));
        }

        [TestMethod, TestCategory("DX10")]
        public void PixelEffectTextures()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            // ----

            var medicalMetallicPath = Path.Combine(contentPath, @"Textures\Models\Cubes\large_medical_room_cm.dds"); // "32bpp RGBA"
            Assert.IsTrue(File.Exists(medicalMetallicPath), "Filepath should exist on developer machine");
            var medicalMetallicBmp = TestLoadTexture(medicalMetallicPath);
            ImageTextureUtil.WriteImage(medicalMetallicBmp, @".\TestOutput\large_medical_room_cm.png");

            var medicalMetallicBmp2 = TestLoadTexture(medicalMetallicPath, ignoreAlpha: true);
            ImageTextureUtil.WriteImage(medicalMetallicBmp2, @".\TestOutput\large_medical_room_cm_full.png");
            
            IPixelEffect effect = new AlphaPixelEffect();
            var medicalMetallicAlphaBmp = effect.Quantize(medicalMetallicBmp);
            ImageTextureUtil.WriteImage(medicalMetallicAlphaBmp, @".\TestOutput\large_medical_room_cm_alpha.png");

            effect = new EmissivePixelEffect(0);
            var medicalNormalSpecularEmissiveBmp = effect.Quantize(medicalMetallicBmp);
            ImageTextureUtil.WriteImage(medicalNormalSpecularEmissiveBmp, @".\TestOutput\large_medical_room_emissive.png");

            var defaultImage = SEToolbox.ImageLibrary.ImageTextureUtil.CreateImage(medicalMetallicPath, false, new EmissivePixelEffect(0));

            // ----

            var largeThrustMetallicPath = Path.Combine(contentPath, @"Textures\Models\Cubes\large_thrust_large_cm.dds"); // "32bpp RGBA"
            Assert.IsTrue(File.Exists(largeThrustMetallicPath), "Filepath should exist on developer machine");
            var largeThrustMetallicBmp = TestLoadTexture(largeThrustMetallicPath);
            ImageTextureUtil.WriteImage(largeThrustMetallicBmp, @".\TestOutput\large_thrust_large_me.png");

            var largeThrustMetallicBmp2 = TestLoadTexture(largeThrustMetallicPath, ignoreAlpha: true);
            ImageTextureUtil.WriteImage(largeThrustMetallicBmp2, @".\TestOutput\large_thrust_large_me_full.png");

            effect = new AlphaPixelEffect();
            var largeThrustMetallicAlphaBmp = effect.Quantize(largeThrustMetallicBmp);
            ImageTextureUtil.WriteImage(largeThrustMetallicAlphaBmp, @".\TestOutput\large_thrust_large_me_alpha.png");

            effect = new EmissivePixelEffect(0);
            var largeThrustNormalSpecularEmissiveBmp = effect.Quantize(largeThrustMetallicBmp);
            ImageTextureUtil.WriteImage(largeThrustNormalSpecularEmissiveBmp, @".\TestOutput\large_thrust_large_me_emissive.png");

            // ----

            var astronautMaskEmissivePath = Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_cm.dds");
            Assert.IsTrue(File.Exists(astronautMaskEmissivePath), "Filepath should exist on developer machine");
            var astronautMaskEmissiveBmp = TestLoadTexture(astronautMaskEmissivePath);
            ImageTextureUtil.WriteImage(astronautMaskEmissiveBmp, @".\TestOutput\Astronaut_cm.png");

            effect = new AlphaPixelEffect();
            var astronautMaskEmissiveAlphaBmp = effect.Quantize(astronautMaskEmissiveBmp);
            ImageTextureUtil.WriteImage(astronautMaskEmissiveAlphaBmp, @".\TestOutput\Astronaut_me_alpha.png");

            var astronautMaskEmissiveBmp2 = TestLoadTexture(astronautMaskEmissivePath, ignoreAlpha: true);
            ImageTextureUtil.WriteImage(astronautMaskEmissiveBmp2, @".\TestOutput\Astronaut_me_full.png");

            effect = new EmissivePixelEffect(255);
            var astronautNormalSpecularEmissiveBmp = effect.Quantize(astronautMaskEmissiveBmp);
            ImageTextureUtil.WriteImage(astronautNormalSpecularEmissiveBmp, @".\TestOutput\Astronaut_me_emissive.png");

            // ----

            var astronautNormalSpecularPath = Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_ng.dds");
            Assert.IsTrue(File.Exists(astronautNormalSpecularPath), "Filepath should exist on developer machine");
            var astronautNormalSpecularBmp = TestLoadTexture(astronautNormalSpecularPath);
            ImageTextureUtil.WriteImage(astronautNormalSpecularBmp, @".\TestOutput\Astronaut_ng.png");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void CreateMenuTextures()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var smallBlockLandingGear = (MyCubeBlockDefinition)MyDefinitionManager.Static.GetDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_LandingGear)), "SmallBlockLandingGear");
            var smallBlockLandingGearPath = Path.Combine(contentPath, smallBlockLandingGear.Icons.First());
            Assert.IsTrue(File.Exists(smallBlockLandingGearPath), "Filepath should exist on developer machine");
            Assert.IsTrue(smallBlockLandingGear is MyCubeBlockDefinition, "Type should match");
            var smallBlockLandingGearBmp = TestLoadTexture(smallBlockLandingGearPath);

            var gridItemPath = Path.Combine(contentPath, @"Textures\GUI\Controls\grid_item.dds");
            Assert.IsTrue(File.Exists(gridItemPath), "Filepath should exist on developer machine");
            var gridBmp = TestLoadTexture(gridItemPath);

            var bmp = ImageTextureUtil.MergeImages(gridBmp, smallBlockLandingGearBmp, Brushes.Black);
            ImageTextureUtil.WriteImage(bmp, @".\TestOutput\Menu_SmallBlockLandingGear.png");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void ReadBackgroundTextures()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var backgroundPath = Path.Combine(contentPath, @"Textures\BackgroundCube\Final\BackgroundCube.dds");
            Assert.IsTrue(File.Exists(backgroundPath), "Filepath should exist on developer machine");

            var backgroundBmp = TestLoadTexture(backgroundPath, 0, -1, -1 );
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube0_Full.png");

            backgroundBmp = TestLoadTexture(backgroundPath, 1, 1024, 1024);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube1_1024.png");

            backgroundBmp = TestLoadTexture(backgroundPath, 2, 512, 512);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube2_512.png");

            backgroundBmp = TestLoadTexture(backgroundPath, 3, 128, 128);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube3_128.png");

            backgroundBmp = TestLoadTexture(backgroundPath, 4, 64, 64);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube4_64.png");

            backgroundBmp = TestLoadTexture(backgroundPath, 5, 32, 32);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube5_32.png");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void CreateBackgroundPreview()
        {
            const int size = 128;
            const bool ignoreAlpha = true;

            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var backgroundPath = Path.Combine(contentPath, @"Textures\BackgroundCube\Final\BackgroundCube.dds");
            Assert.IsTrue(File.Exists(backgroundPath), "Filepath should exist on developer machine");

            var result = new Bitmap(size * 4, size * 3);

            using (var graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                if (ignoreAlpha)
                {
                    graphics.FillRectangle(Brushes.Black, size * 2, size * 1, size, size);
                    graphics.FillRectangle(Brushes.Black, size * 0, size * 1, size, size);
                    graphics.FillRectangle(Brushes.Black, size * 1, size * 0, size, size);
                    graphics.FillRectangle(Brushes.Black, size * 1, size * 2, size, size);
                    graphics.FillRectangle(Brushes.Black, size * 1, size * 1, size, size);
                    graphics.FillRectangle(Brushes.Black, size * 3, size * 1, size, size);
                }

                graphics.DrawImage(TestLoadTexture(backgroundPath, 0, size, size, ignoreAlpha), size * 2, size * 1, size, size);
                graphics.DrawImage(TestLoadTexture(backgroundPath, 1, size, size, ignoreAlpha), size * 0, size * 1, size, size);
                graphics.DrawImage(TestLoadTexture(backgroundPath, 2, size, size, ignoreAlpha), size * 1, size * 0, size, size);
                graphics.DrawImage(TestLoadTexture(backgroundPath, 3, size, size, ignoreAlpha), size * 1, size * 2, size, size);
                graphics.DrawImage(TestLoadTexture(backgroundPath, 4, size, size, ignoreAlpha), size * 1, size * 1, size, size);
                graphics.DrawImage(TestLoadTexture(backgroundPath, 5, size, size, ignoreAlpha), size * 3, size * 1, size, size);

                // Approximate position of local Sun and light source.
                graphics.FillEllipse(Brushes.White, size * 1 + (int)(size * 0.7), size * 2 + (int)(size * 0.93), (int)(size * 0.06), (int)(size * 0.06));
            }

            ImageTextureUtil.WriteImage(result, string.Format(@".\TestOutput\BackgroundCube_{0}.png", size));
        }

        // this is ignored, as it really isn't a unit test. It simply extracts game textures.
        [Ignore]
        [TestMethod]
        public void LoadAllCubeTextures()
        {
            var files = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationContentPath(), @"Textures\Models\Cubes"), "*.dds");

            foreach (var filename in files)
            {
                var outputFilename = Path.Combine(@".\TestOutput", Path.GetFileNameWithoutExtension(filename) + ".png");
                var imageBitmap = TestLoadTexture(filename);
                ImageTextureUtil.WriteImage(imageBitmap, outputFilename);
            }
        }
    }
}
