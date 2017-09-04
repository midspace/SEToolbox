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
    [DeploymentItem("_placeholder.txt", "TestOutput")]
    [DeploymentItem("TestOutput\\_placeholder.txt", "TestOutput")]
    public class TextureTests
    {
        private string _path;

        [TestInitialize]
        public void InitTest()
        {
            SpaceEngineersCore.LoadDefinitions();
            string path = Path.GetFullPath(".\\TestOutput");
            //_path = @"D:\Development\GitHub\SEToolbox development\Main\SEToolbox\ToolboxTest\bin\x64\Debug\TestOutput";

            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        [TestMethod]
        public void LoadComponentTextures()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var magnesiumOre = MyDefinitionManager.Static.GetDefinition(SpaceEngineersTypes.Ore, "Magnesium");
            var magnesiumOrePath = Path.Combine(contentPath, magnesiumOre.Icons.First());
            Assert.IsTrue(File.Exists(magnesiumOrePath), "Filepath should exist on developer machine");
            Assert.IsTrue(magnesiumOre is MyPhysicalItemDefinition, "Type should match");
            var magnesiumOreBmp = TexUtil.CreateBitmap(magnesiumOrePath);

            var str = Path.GetFullPath( Path.Combine(_path, "Magnesium_Ore.dds"));
            File.Copy(magnesiumOrePath, Path.Combine(_path, "Magnesium_Ore.dds"), true);
            ImageTextureUtil.WriteImage(magnesiumOreBmp, Path.Combine(_path, @"Magnesium_Ore.png"));

            return;

            var goldIngot = MyDefinitionManager.Static.GetDefinition(SpaceEngineersTypes.Ingot, "Gold");
            var goldIngotPath = Path.Combine(contentPath, goldIngot.Icons.First());
            Assert.IsTrue(File.Exists(goldIngotPath), "Filepath should exist on developer machine");
            Assert.IsTrue(goldIngot is MyPhysicalItemDefinition, "Type should match");
            var goldIngotBmp = TexUtil.CreateBitmap(goldIngotPath);
            ImageTextureUtil.WriteImage(goldIngotBmp, @".\TestOutput\Gold_Ingot.png");

            var ammoMagazine = MyDefinitionManager.Static.GetDefinition(SpaceEngineersTypes.AmmoMagazine, "NATO_5p56x45mm");
            var ammoMagazinePath = Path.Combine(contentPath, ammoMagazine.Icons.First());
            Assert.IsTrue(File.Exists(ammoMagazinePath), "Filepath should exist on developer machine");
            Assert.IsTrue(ammoMagazine is MyAmmoMagazineDefinition, "Type should match");
            var ammoMagazineBmp = TexUtil.CreateBitmap(ammoMagazinePath);
            ImageTextureUtil.WriteImage(ammoMagazineBmp, @".\TestOutput\NATO_5p56x45mm.png");

            var steelPlate = MyDefinitionManager.Static.GetDefinition(SpaceEngineersTypes.Component, "SteelPlate");
            var steelPlatePath = Path.Combine(contentPath, steelPlate.Icons.First());
            Assert.IsTrue(File.Exists(steelPlatePath), "Filepath should exist on developer machine");
            Assert.IsTrue(steelPlate is MyComponentDefinition, "Type should match");
            var steelPlateBmp = TexUtil.CreateBitmap(steelPlatePath);
            ImageTextureUtil.WriteImage(steelPlateBmp, @".\TestOutput\SteelPlate.png");

            var smallBlockLandingGear = MyDefinitionManager.Static.GetDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_LandingGear)), "SmallBlockLandingGear");
            var smallBlockLandingGearPath = Path.Combine(contentPath, smallBlockLandingGear.Icons.First());
            Assert.IsTrue(File.Exists(smallBlockLandingGearPath), "Filepath should exist on developer machine");
            Assert.IsTrue(smallBlockLandingGear is MyCubeBlockDefinition, "Type should match");
            var smallBlockLandingGearBmp = TexUtil.CreateBitmap(smallBlockLandingGearPath);
            ImageTextureUtil.WriteImage(smallBlockLandingGearBmp, @".\TestOutput\SmallBlockLandingGear.png");

            var gridItemPath = Path.Combine(contentPath, @"Textures\GUI\Controls\grid_item.dds");
            Assert.IsTrue(File.Exists(gridItemPath), "Filepath should exist on developer machine");
            var gridBmp = TexUtil.CreateBitmap(gridItemPath);
            ImageTextureUtil.WriteImage(gridBmp, @".\TestOutput\grid_item.png");

            var sunPath = Path.Combine(contentPath, @"Textures\BackgroundCube\Prerender\Sun.dds");
            Assert.IsTrue(File.Exists(sunPath), "Filepath should exist on developer machine");
            var sunBmp = TexUtil.CreateBitmap(sunPath);
            ImageTextureUtil.WriteImage(sunBmp, @".\TestOutput\sun.png");

            var goldPath = Path.Combine(contentPath, @"Textures\Voxels\Gold_01_ForAxisXZ_cm.dds");
            Assert.IsTrue(File.Exists(goldPath), "Filepath should exist on developer machine");
            var goldBmp = TexUtil.CreateBitmap(goldPath);
            ImageTextureUtil.WriteImage(goldBmp, @".\TestOutput\gold.png");

            var siliconPath = Path.Combine(contentPath, @"Textures\Voxels\Silicon_01_ForAxisXZ_cm.dds");
            Assert.IsTrue(File.Exists(siliconPath), "Filepath should exist on developer machine");
            var siliconBmp = TexUtil.CreateBitmap(siliconPath);
            ImageTextureUtil.WriteImage(siliconBmp, @".\TestOutput\silicon.png");

            var platinumPath = Path.Combine(contentPath, @"Textures\Voxels\Platinum_01_ForAxisXZ_cm.dds");
            Assert.IsTrue(File.Exists(platinumPath), "Filepath should exist on developer machine");
            var platinumBmp = TexUtil.CreateBitmap(platinumPath);
            ImageTextureUtil.WriteImage(platinumBmp, @".\TestOutput\platinum.png");

            var medicalMetallicPath = Path.Combine(contentPath, @"Textures\Models\Cubes\large_medical_room_cm.dds");
            Assert.IsTrue(File.Exists(medicalMetallicPath), "Filepath should exist on developer machine");
            var medicalMetallicBmp = TexUtil.CreateBitmap(medicalMetallicPath);
            ImageTextureUtil.WriteImage(medicalMetallicBmp, @".\TestOutput\large_medical_room_cm.png");

            medicalMetallicBmp = TexUtil.CreateBitmap(medicalMetallicPath, true);
            ImageTextureUtil.WriteImage(medicalMetallicBmp, @".\TestOutput\large_medical_room.png");

            var astronautMetallicPath = Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_cm.dds");
            Assert.IsTrue(File.Exists(astronautMetallicPath), "Filepath should exist on developer machine");
            var astronautMetallicBmp = TexUtil.CreateBitmap(astronautMetallicPath);
            ImageTextureUtil.WriteImage(astronautMetallicBmp, @".\TestOutput\Astronaut_cm.png");

            astronautMetallicBmp = TexUtil.CreateBitmap(astronautMetallicPath, true);
            ImageTextureUtil.WriteImage(astronautMetallicBmp, @".\TestOutput\Astronaut_me2.png");

            var astronautNormalGlossPath = Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_ng.dds");
            Assert.IsTrue(File.Exists(astronautNormalGlossPath), "Filepath should exist on developer machine");
            var astronautNormalGlossBmp = TexUtil.CreateBitmap(astronautNormalGlossPath);
            ImageTextureUtil.WriteImage(astronautNormalGlossBmp, @".\TestOutput\Astronaut_ng.png");

            astronautNormalGlossBmp = TexUtil.CreateBitmap(astronautNormalGlossPath, true);
            ImageTextureUtil.WriteImage(astronautNormalGlossBmp, @".\TestOutput\Astronaut_ng2.png");
        }

        [TestMethod]
        public void PixelEffectTextures()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            // ----

            var medicalMetallicPath = Path.Combine(contentPath, @"Textures\Models\Cubes\large_medical_room_cm.dds"); // "32bpp RGBA"
            Assert.IsTrue(File.Exists(medicalMetallicPath), "Filepath should exist on developer machine");
            // TODO: load the "32bpp RGBA" Dx10 texture.
            var medicalMetallicBmp = TexUtil.CreateBitmap(medicalMetallicPath);
            ImageTextureUtil.WriteImage(medicalMetallicBmp, @".\TestOutput\large_medical_room_cm.png");

            var medicalMetallicBmp2 = TexUtil.CreateBitmap(medicalMetallicPath, true);
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
            var largeThrustMetallicBmp = TexUtil.CreateBitmap(largeThrustMetallicPath);
            ImageTextureUtil.WriteImage(largeThrustMetallicBmp, @".\TestOutput\large_thrust_large_me.png");

            var largeThrustMetallicBmp2 = TexUtil.CreateBitmap(largeThrustMetallicPath, true);
            ImageTextureUtil.WriteImage(largeThrustMetallicBmp2, @".\TestOutput\large_thrust_large_me_full.png");

            effect = new AlphaPixelEffect();
            var largeThrustMetallicAlphaBmp = effect.Quantize(largeThrustMetallicBmp);
            ImageTextureUtil.WriteImage(largeThrustMetallicAlphaBmp, @".\TestOutput\large_thrust_large_me_alpha.png");

            effect = new EmissivePixelEffect(0);
            var largeThrustNormalSpecularEmissiveBmp = effect.Quantize(largeThrustMetallicBmp);
            ImageTextureUtil.WriteImage(largeThrustNormalSpecularEmissiveBmp, @".\TestOutput\large_thrust_large_me_emissive.png");

            // ----

            var astronautMaskEmissivePath = Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_me.dds");
            Assert.IsTrue(File.Exists(astronautMaskEmissivePath), "Filepath should exist on developer machine");
            var astronautMaskEmissiveBmp = TexUtil.CreateBitmap(astronautMaskEmissivePath);
            ImageTextureUtil.WriteImage(astronautMaskEmissiveBmp, @".\TestOutput\Astronaut_me.png");

            effect = new AlphaPixelEffect();
            var astronautMaskEmissiveAlphaBmp = effect.Quantize(astronautMaskEmissiveBmp);
            ImageTextureUtil.WriteImage(astronautMaskEmissiveAlphaBmp, @".\TestOutput\Astronaut_me_alpha.png");

            var astronautMaskEmissiveBmp2 = TexUtil.CreateBitmap(astronautMaskEmissivePath, true);
            ImageTextureUtil.WriteImage(astronautMaskEmissiveBmp2, @".\TestOutput\Astronaut_me_full.png");

            effect = new EmissivePixelEffect(255);
            var astronautNormalSpecularEmissiveBmp = effect.Quantize(astronautMaskEmissiveBmp);
            ImageTextureUtil.WriteImage(astronautNormalSpecularEmissiveBmp, @".\TestOutput\Astronaut_me_emissive.png");

            // ----

            var astronautNormalSpecularPath = Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_ns.dds");
            Assert.IsTrue(File.Exists(astronautNormalSpecularPath), "Filepath should exist on developer machine");
            var astronautNormalSpecularBmp = TexUtil.CreateBitmap(astronautNormalSpecularPath);
            ImageTextureUtil.WriteImage(astronautNormalSpecularBmp, @".\TestOutput\Astronaut_ns.png");
        }

        [TestMethod]
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
            var smallBlockLandingGearBmp = TexUtil.CreateBitmap(smallBlockLandingGearPath);

            var gridItemPath = Path.Combine(contentPath, @"Textures\GUI\Controls\grid_item.dds");
            Assert.IsTrue(File.Exists(gridItemPath), "Filepath should exist on developer machine");
            var gridBmp = TexUtil.CreateBitmap(gridItemPath);

            var bmp = ImageTextureUtil.MergeImages(gridBmp, smallBlockLandingGearBmp, Brushes.Black);
            ImageTextureUtil.WriteImage(bmp, @".\TestOutput\Menu_SmallBlockLandingGear.png");
        }

        [TestMethod]
        public void ReadBackgroundTextures()
        {
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var backgroundPath = Path.Combine(contentPath, @"Textures\BackgroundCube\Final\BackgroundCube.dds");
            Assert.IsTrue(File.Exists(backgroundPath), "Filepath should exist on developer machine");

            var backgroundBmp = TexUtil.CreateBitmap(backgroundPath, 0, -1, -1 );
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube0_Full.png");

            backgroundBmp = TexUtil.CreateBitmap(backgroundPath, 1, 1024, 1024);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube1_1024.png");

            backgroundBmp = TexUtil.CreateBitmap(backgroundPath, 2, 512, 512);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube2_512.png");

            backgroundBmp = TexUtil.CreateBitmap(backgroundPath, 3, 128, 128);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube3_128.png");

            backgroundBmp = TexUtil.CreateBitmap(backgroundPath, 4, 64, 64);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube4_64.png");

            backgroundBmp = TexUtil.CreateBitmap(backgroundPath, 5, 32, 32);
            ImageTextureUtil.WriteImage(backgroundBmp, @".\TestOutput\BackgroundCube5_32.png");
        }

        [TestMethod]
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

                graphics.DrawImage(TexUtil.CreateBitmap(backgroundPath, 0, size, size, ignoreAlpha), size * 2, size * 1, size, size);
                graphics.DrawImage(TexUtil.CreateBitmap(backgroundPath, 1, size, size, ignoreAlpha), size * 0, size * 1, size, size);
                graphics.DrawImage(TexUtil.CreateBitmap(backgroundPath, 2, size, size, ignoreAlpha), size * 1, size * 0, size, size);
                graphics.DrawImage(TexUtil.CreateBitmap(backgroundPath, 3, size, size, ignoreAlpha), size * 1, size * 2, size, size);
                graphics.DrawImage(TexUtil.CreateBitmap(backgroundPath, 4, size, size, ignoreAlpha), size * 1, size * 1, size, size);
                graphics.DrawImage(TexUtil.CreateBitmap(backgroundPath, 5, size, size, ignoreAlpha), size * 3, size * 1, size, size);

                // Approximate position of local Sun and light source.
                graphics.FillEllipse(Brushes.White, size * 1 + (int)(size * 0.7), size * 2 + (int)(size * 0.93), (int)(size * 0.06), (int)(size * 0.06));
            }

            ImageTextureUtil.WriteImage(result, string.Format(@".\TestOutput\BackgroundCube_{0}.png", size));
        }

        //[TestMethod]
        //public void LoadAllCubeTextures()
        //{
        //    var files = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationContentPath(), @"Textures\Models\Cubes"), "*.dds");

        //    foreach (var filename in files)
        //    {
        //        var outputFilename = Path.Combine(@".\TestOutput", Path.GetFileNameWithoutExtension( filename) + ".png");
        //        var imageBitmap = TexUtil.CreateBitmap(filename);
        //        ImageTextureUtil.WriteImage(imageBitmap, outputFilename);
        //    }
        //}
    }
}
