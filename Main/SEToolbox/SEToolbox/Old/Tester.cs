namespace SEToolbox.Enums
{
    using System;
    using System.IO;
    using System.Xml;

    public static class Tester
    {
        public static void Test()
        {
            var filename = @"D:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\Content\Data\CubeBlocks.sbc";
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(filename);

            var nav = xDoc.CreateNavigator();

            var definitions = nav.Select("MyObjectBuilder_CubeBlockDefinitions/Definitions/Definition");
            while (definitions.MoveNext())
            {
                var name = definitions.Current.SelectSingleNode("Id/SubtypeId").Value;

                if (definitions.Current.SelectSingleNode("Variants") != null)
                {
                    var variants = definitions.Current.Select("Variants/Variant");
                    while (variants.MoveNext())
                    {
                        name += variants.Current.SelectSingleNode("@Color").Value;
                    }
                }
                else
                {
                }
            }


            //var settings = new XmlReaderSettings()
            //{
            //    IgnoreComments = true,
            //    IgnoreWhitespace = true,
            //};


            //using (XmlReader myReader = XmlReader.Create(filename, settings))
            //{
            //    while (myReader.Read())
            //    {
            //        // Process each node (myReader.Value) here
            //        // ...
            //    }
            //}
        }
    }

    //// TODO: use Text Templating to generate content from D:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\Content\Data\CubeBlocks.sbc
    //public enum Block
    //{
    //    None,

    //    // Large
    //    LargeBlockArmorBlock,
    //    LargeBlockArmorBlockRed,
    //    LargeBlockArmorBlockYellow,
    //    LargeBlockArmorBlockBlue,
    //    LargeBlockArmorBlockGreen,
    //    LargeBlockArmorBlockBlack,
    //    LargeBlockArmorBlockWhite,

    //    LargeBlockArmorSlope,
    //    LargeBlockArmorSlopeRed,
    //    LargeBlockArmorSlopeYellow,
    //    LargeBlockArmorSlopeBlue,
    //    LargeBlockArmorSlopeGreen,
    //    LargeBlockArmorSlopeBlack,
    //    LargeBlockArmorSlopeWhite,

    //    LargeBlockArmorCorner,
    //    LargeBlockArmorCornerRed,
    //    LargeBlockArmorCornerYellow,
    //    LargeBlockArmorCornerBlue,
    //    LargeBlockArmorCornerGreen,
    //    LargeBlockArmorCornerBlack,
    //    LargeBlockArmorCornerWhite,

    //    LargeBlockArmorCornerInv,
    //    LargeBlockArmorCornerInvRed,
    //    LargeBlockArmorCornerInvYellow,
    //    LargeBlockArmorCornerInvBlue,
    //    LargeBlockArmorCornerInvGreen,
    //    LargeBlockArmorCornerInvBlack,
    //    LargeBlockArmorCornerInvWhite,

    //    // Heavy
    //    LargeHeavyBlockArmorBlock,
    //    LargeHeavyBlockArmorBlockRed,
    //    LargeHeavyBlockArmorBlockYellow,
    //    LargeHeavyBlockArmorBlockBlue,
    //    LargeHeavyBlockArmorBlockGreen,
    //    LargeHeavyBlockArmorBlockBlack,
    //    LargeHeavyBlockArmorBlockWhite,

    //    LargeHeavyBlockArmorSlope,
    //    LargeHeavyBlockArmorSlopeRed,
    //    LargeHeavyBlockArmorSlopeYellow,
    //    LargeHeavyBlockArmorSlopeBlue,
    //    LargeHeavyBlockArmorSlopeGreen,
    //    LargeHeavyBlockArmorSlopeBlack,
    //    LargeHeavyBlockArmorSlopeWhite,

    //    LargeHeavyBlockArmorCorner,
    //    LargeHeavyBlockArmorCornerRed,
    //    LargeHeavyBlockArmorCornerYellow,
    //    LargeHeavyBlockArmorCornerBlue,
    //    LargeHeavyBlockArmorCornerGreen,
    //    LargeHeavyBlockArmorCornerBlack,
    //    LargeHeavyBlockArmorCornerWhite,

    //    LargeHeavyBlockArmorCornerInv,
    //    LargeHeavyBlockArmorCornerInvRed,
    //    LargeHeavyBlockArmorCornerInvYellow,
    //    LargeHeavyBlockArmorCornerInvBlue,
    //    LargeHeavyBlockArmorCornerInvGreen,
    //    LargeHeavyBlockArmorCornerInvBlack,
    //    LargeHeavyBlockArmorCornerInvWhite,


    //    // Small
    //    SmallBlockArmorBlock,
    //    SmallBlockArmorBlockRed,
    //    SmallBlockArmorBlockYellow,
    //    SmallBlockArmorBlockBlue,
    //    SmallBlockArmorBlockGreen,
    //    SmallBlockArmorBlockBlack,
    //    SmallBlockArmorBlockWhite,

    //    SmallBlockArmorSlope,
    //    SmallBlockArmorSlopeRed,
    //    SmallBlockArmorSlopeYellow,
    //    SmallBlockArmorSlopeBlue,
    //    SmallBlockArmorSlopeGreen,
    //    SmallBlockArmorSlopeBlack,
    //    SmallBlockArmorSlopeWhite,

    //    SmallBlockArmorCorner,
    //    SmallBlockArmorCornerRed,
    //    SmallBlockArmorCornerYellow,
    //    SmallBlockArmorCornerBlue,
    //    SmallBlockArmorCornerGreen,
    //    SmallBlockArmorCornerBlack,
    //    SmallBlockArmorCornerWhite,

    //    SmallBlockArmorCornerInv,
    //    SmallBlockArmorCornerInvRed,
    //    SmallBlockArmorCornerInvYellow,
    //    SmallBlockArmorCornerInvBlue,
    //    SmallBlockArmorCornerInvGreen,
    //    SmallBlockArmorCornerInvBlack,
    //    SmallBlockArmorCornerInvWhite,

    //    // Heavy
    //    SmallHeavyBlockArmorBlock,
    //    SmallHeavyBlockArmorBlockRed,
    //    SmallHeavyBlockArmorBlockYellow,
    //    SmallHeavyBlockArmorBlockBlue,
    //    SmallHeavyBlockArmorBlockGreen,
    //    SmallHeavyBlockArmorBlockBlack,
    //    SmallHeavyBlockArmorBlockWhite,

    //    SmallHeavyBlockArmorSlope,
    //    SmallHeavyBlockArmorSlopeRed,
    //    SmallHeavyBlockArmorSlopeYellow,
    //    SmallHeavyBlockArmorSlopeBlue,
    //    SmallHeavyBlockArmorSlopeGreen,
    //    SmallHeavyBlockArmorSlopeBlack,
    //    SmallHeavyBlockArmorSlopeWhite,

    //    SmallHeavyBlockArmorCorner,
    //    SmallHeavyBlockArmorCornerRed,
    //    SmallHeavyBlockArmorCornerYellow,
    //    SmallHeavyBlockArmorCornerBlue,
    //    SmallHeavyBlockArmorCornerGreen,
    //    SmallHeavyBlockArmorCornerBlack,
    //    SmallHeavyBlockArmorCornerWhite,

    //    SmallHeavyBlockArmorCornerInv,
    //    SmallHeavyBlockArmorCornerInvRed,
    //    SmallHeavyBlockArmorCornerInvYellow,
    //    SmallHeavyBlockArmorCornerInvBlue,
    //    SmallHeavyBlockArmorCornerInvGreen,
    //    SmallHeavyBlockArmorCornerInvBlack,
    //    SmallHeavyBlockArmorCornerInvWhite,

    //    // Other
    //    LargeBlockCockpit,
    //    LargeInteriorTurret,
    //    LargeBlockRadioAntenna,
    //    LargeBlockFrontLight,
    //    SmallLight,
    //    LargeWindowSquare,
    //    LargeWindowEdge,
    //    LargeStairs,
    //    LargeRamp,
    //    LargeSteelCatwalk,
    //    LargeCoverWall,
    //    LargeCoverWallHalf,
    //    LargeExplosionCube,
    //    LargeBlockInteriorWall,
    //    LargeInteriorPillar,
    //    LargeBlockLandingGear,
    //    LargeRefinery,
    //    LargeAssembler,
    //    LargeOreDetector,
    //    LargeMedicalRoom,
    //    SmallBlockCockpit,
    //    SmallBlockLandingGear,
    //    SmallBlockFrontLight,
    //    SmallBlockDrill,
    //    SmallBlockOreDetector,
    //    SmallBlockRadioAntenna,
    //    SmallBlockSmallContainer,
    //    SmallBlockMediumContainer,
    //    SmallBlockLargeContainer,
    //    LargeBlockSmallContainer,
    //    LargeBlockLargeContainer,
    //    SmallBlockSmallThrust,
    //    SmallBlockLargeThrust,
    //    LargeBlockSmallThrust,
    //    LargeBlockLargeThrust,
    //    LargeBlockGyro,
    //    SmallBlockGyro,
    //    SmallBlockSmallGenerator,
    //    SmallBlockLargeGenerator,
    //    LargeBlockSmallGenerator,
    //    LargeBlockLargeGenerator,
    //    SmallBlockConveyor,
    //    LargeBlockConveyor,
    //};
}
