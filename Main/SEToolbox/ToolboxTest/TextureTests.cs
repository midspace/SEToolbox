namespace ToolboxTest
{
    using System.Drawing;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.ImageLibrary;
    using SEToolbox.ImageLibrary.Effects;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using Brushes = System.Drawing.Brushes;
    using TexUtil = SEToolbox.ImageLibrary.ImageTextureUtil;

    [TestClass]
    public class TextureTests
    {
        [TestMethod]
        public void LoadComponentTextures()
        {
            SpaceEngineersCore.LoadDefinitions();
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var magnesiumOre = SpaceEngineersApi.GetDefinition(SpaceEngineersTypes.Ore, "Magnesium");
            var magnesiumOrePath = Path.Combine(contentPath, magnesiumOre.Icon);
            Assert.IsTrue(File.Exists(magnesiumOrePath), "Filepath should exist on developer machine");
            Assert.IsTrue(magnesiumOre is MyObjectBuilder_PhysicalItemDefinition, "Type should match");
            var magnesiumOreBmp = TexUtil.CreateBitmap(magnesiumOrePath);
            ImageTextureUtil.WriteImage(magnesiumOreBmp, @".\TestOutput\Magnesium_Ore.png");

            var goldIngot = SpaceEngineersApi.GetDefinition(SpaceEngineersTypes.Ingot, "Gold");
            var goldIngotPath = Path.Combine(contentPath, goldIngot.Icon);
            Assert.IsTrue(File.Exists(goldIngotPath), "Filepath should exist on developer machine");
            Assert.IsTrue(goldIngot is MyObjectBuilder_PhysicalItemDefinition, "Type should match");
            var goldIngotBmp = TexUtil.CreateBitmap(goldIngotPath);
            ImageTextureUtil.WriteImage(goldIngotBmp, @".\TestOutput\Gold_Ingot.png");

            var ammoMagazine = SpaceEngineersApi.GetDefinition(SpaceEngineersTypes.AmmoMagazine, "NATO_5p56x45mm");
            var ammoMagazinePath = Path.Combine(contentPath, ammoMagazine.Icon);
            Assert.IsTrue(File.Exists(ammoMagazinePath), "Filepath should exist on developer machine");
            Assert.IsTrue(ammoMagazine is MyObjectBuilder_AmmoMagazineDefinition, "Type should match");
            var ammoMagazineBmp = TexUtil.CreateBitmap(ammoMagazinePath);
            ImageTextureUtil.WriteImage(ammoMagazineBmp, @".\TestOutput\NATO_5p56x45mm.png");

            var steelPlate = SpaceEngineersApi.GetDefinition(SpaceEngineersTypes.Component, "SteelPlate");
            var steelPlatePath = Path.Combine(contentPath, steelPlate.Icon);
            Assert.IsTrue(File.Exists(steelPlatePath), "Filepath should exist on developer machine");
            Assert.IsTrue(steelPlate is MyObjectBuilder_ComponentDefinition, "Type should match");
            var steelPlateBmp = TexUtil.CreateBitmap(steelPlatePath);
            ImageTextureUtil.WriteImage(steelPlateBmp, @".\TestOutput\SteelPlate.png");

            var smallBlockLandingGear = SpaceEngineersApi.GetDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_LandingGear)), "SmallBlockLandingGear");
            var smallBlockLandingGearPath = Path.Combine(contentPath, smallBlockLandingGear.Icon);
            Assert.IsTrue(File.Exists(smallBlockLandingGearPath), "Filepath should exist on developer machine");
            Assert.IsTrue(smallBlockLandingGear is MyObjectBuilder_CubeBlockDefinition, "Type should match");
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

            var goldPath = Path.Combine(contentPath, @"Textures\Voxels\Gold_01_ForAxisXZ_de.dds");
            Assert.IsTrue(File.Exists(goldPath), "Filepath should exist on developer machine");
            var goldBmp = TexUtil.CreateBitmap(goldPath);
            ImageTextureUtil.WriteImage(goldBmp, @".\TestOutput\gold.png");

            var siliconPath = Path.Combine(contentPath, @"Textures\Voxels\Silicon_01_ForAxisXZ_de.dds");
            Assert.IsTrue(File.Exists(siliconPath), "Filepath should exist on developer machine");
            var siliconBmp = TexUtil.CreateBitmap(siliconPath);
            ImageTextureUtil.WriteImage(siliconBmp, @".\TestOutput\silicon.png");

            var platinumPath = Path.Combine(contentPath, @"Textures\Voxels\Platinum_01_ForAxisXZ_de.dds");
            Assert.IsTrue(File.Exists(platinumPath), "Filepath should exist on developer machine");
            var platinumBmp = TexUtil.CreateBitmap(platinumPath);
            ImageTextureUtil.WriteImage(platinumBmp, @".\TestOutput\platinum.png");

            var medicalDiffuseEmissivePath = Path.Combine(contentPath, @"Textures\Models\Cubes\large_medical_room_de.dds");
            Assert.IsTrue(File.Exists(medicalDiffuseEmissivePath), "Filepath should exist on developer machine");
            var medicalDiffuseEmissiveBmp = TexUtil.CreateBitmap(medicalDiffuseEmissivePath);
            ImageTextureUtil.WriteImage(medicalDiffuseEmissiveBmp, @".\TestOutput\large_medical_room_de.png");

            medicalDiffuseEmissiveBmp = TexUtil.CreateBitmap(medicalDiffuseEmissivePath, true);
            ImageTextureUtil.WriteImage(medicalDiffuseEmissiveBmp, @".\TestOutput\large_medical_room.png");

            var astronautMaskEmissivePath = Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_me.dds");
            Assert.IsTrue(File.Exists(astronautMaskEmissivePath), "Filepath should exist on developer machine");
            var astronautMaskEmissiveBmp = TexUtil.CreateBitmap(astronautMaskEmissivePath);
            ImageTextureUtil.WriteImage(astronautMaskEmissiveBmp, @".\TestOutput\Astronaut_me.png");

            astronautMaskEmissiveBmp = TexUtil.CreateBitmap(astronautMaskEmissivePath, true);
            ImageTextureUtil.WriteImage(astronautMaskEmissiveBmp, @".\TestOutput\Astronaut_me2.png");

            var astronautNormalSpecularPath = Path.Combine(contentPath, @"Textures\Models\Characters\Astronaut\Astronaut_ns.dds");
            Assert.IsTrue(File.Exists(astronautNormalSpecularPath), "Filepath should exist on developer machine");
            var astronautNormalSpecularBmp = TexUtil.CreateBitmap(astronautNormalSpecularPath);
            ImageTextureUtil.WriteImage(astronautNormalSpecularBmp, @".\TestOutput\Astronaut_ns.png");

            astronautNormalSpecularBmp = TexUtil.CreateBitmap(astronautNormalSpecularPath, true);
            ImageTextureUtil.WriteImage(astronautNormalSpecularBmp, @".\TestOutput\Astronaut_ns2.png");
        }

        [TestMethod]
        public void PixelEffectTextures()
        {
            SpaceEngineersCore.LoadDefinitions();
            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            // ----

            var medicalDiffuseEmissivePath = Path.Combine(contentPath, @"Textures\Models\Cubes\large_medical_room_de.dds");
            Assert.IsTrue(File.Exists(medicalDiffuseEmissivePath), "Filepath should exist on developer machine");
            var medicalDiffuseEmissiveBmp = TexUtil.CreateBitmap(medicalDiffuseEmissivePath);
            ImageTextureUtil.WriteImage(medicalDiffuseEmissiveBmp, @".\TestOutput\large_medical_room_de.png");

            var medicalDiffuseEmissiveBmp2 = TexUtil.CreateBitmap(medicalDiffuseEmissivePath, true);
            ImageTextureUtil.WriteImage(medicalDiffuseEmissiveBmp2, @".\TestOutput\large_medical_room_de_full.png");
            
            IPixelEffect effect = new AlphaPixelEffect();
            var medicalDiffuseEmissiveAlphaBmp = effect.Quantize(medicalDiffuseEmissiveBmp);
            ImageTextureUtil.WriteImage(medicalDiffuseEmissiveAlphaBmp, @".\TestOutput\large_medical_room_de_alpha.png");

            effect = new EmissivePixelEffect(0);
            var medicalNormalSpecularEmissiveBmp = effect.Quantize(medicalDiffuseEmissiveBmp);
            ImageTextureUtil.WriteImage(medicalNormalSpecularEmissiveBmp, @".\TestOutput\large_medical_room_emissive.png");

            var defaultImage = SEToolbox.ImageLibrary.ImageTextureUtil.CreateImage(medicalDiffuseEmissivePath, false, new EmissivePixelEffect(0));

            // ----

            var largeThrustDiffuseEmissivePath = Path.Combine(contentPath, @"Textures\Models\Cubes\large_thrust_large_me.dds");
            Assert.IsTrue(File.Exists(largeThrustDiffuseEmissivePath), "Filepath should exist on developer machine");
            var largeThrustDiffuseEmissiveBmp = TexUtil.CreateBitmap(largeThrustDiffuseEmissivePath);
            ImageTextureUtil.WriteImage(largeThrustDiffuseEmissiveBmp, @".\TestOutput\large_thrust_large_me.png");

            var largeThrustDiffuseEmissiveBmp2 = TexUtil.CreateBitmap(largeThrustDiffuseEmissivePath, true);
            ImageTextureUtil.WriteImage(largeThrustDiffuseEmissiveBmp2, @".\TestOutput\large_thrust_large_me_full.png");

            effect = new AlphaPixelEffect();
            var largeThrustDiffuseEmissiveAlphaBmp = effect.Quantize(largeThrustDiffuseEmissiveBmp);
            ImageTextureUtil.WriteImage(largeThrustDiffuseEmissiveAlphaBmp, @".\TestOutput\large_thrust_large_me_alpha.png");

            effect = new EmissivePixelEffect(0);
            var largeThrustNormalSpecularEmissiveBmp = effect.Quantize(largeThrustDiffuseEmissiveBmp);
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
            SpaceEngineersCore.LoadDefinitions();

            var location = ToolboxUpdater.GetApplicationFilePath();
            Assert.IsNotNull(location, "Space Engineers should be installed on developer machine");
            Assert.IsTrue(Directory.Exists(location), "Filepath should exist on developer machine");

            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            var smallBlockLandingGear = SpaceEngineersApi.GetDefinition(new MyObjectBuilderType(typeof(MyObjectBuilder_LandingGear)), "SmallBlockLandingGear");
            var smallBlockLandingGearPath = Path.Combine(contentPath, smallBlockLandingGear.Icon);
            Assert.IsTrue(File.Exists(smallBlockLandingGearPath), "Filepath should exist on developer machine");
            Assert.IsTrue(smallBlockLandingGear is MyObjectBuilder_CubeBlockDefinition, "Type should match");
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
        //    SpaceEngineersCore.LoadDefinitions();

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
