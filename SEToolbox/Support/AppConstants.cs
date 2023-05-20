namespace SEToolbox.Support
{
    using SEToolbox.Interop;
    using System;
    using System.Collections.Generic;
    using Res = SEToolbox.Properties.Resources;

    internal class AppConstants
    {
        internal static Dictionary<string, string> SupportedLanguages = new Dictionary<string, string>()
        {
            // The following local code must match those from the game, or they will be considered new.
            {"en", "flag_great_britain"},
            {"en-US", "flag_usa"},
            {"ca-ES", "flag_catalonia"},
            {"cs-CZ", "flag_czech_republic"},
            {"da", "flag_denmark"},
            {"de", "flag_germany"},
            {"es", "flag_spain"},
            {"es-MX", "flag_mexico"},
            {"et-EE", "flag_estonia"},
            {"fi", "flag_finland"},
            {"fr", "flag_france"},
            {"hr-HR", "flag_croatia"},
            {"hu-HU", "flag_hungary"},
            {"is-IS", "flag_iceland"},
            {"it", "flag_italy"},
            {"lv", "flag_lithuania"},
            {"nl", "flag_netherlands"},
            {"no", "flag_norway"},
            {"pl-PL", "flag_poland"},
            {"pt-BR", "flag_brazil"},
            {"ro", "flag_romania"},
            {"ru", "flag_russia"},
            {"sk-SK", "flag_slovakia"},
            {"sv", "flag_sweden"},
            {"tr-TR", "flag_turkey"},
            {"uk", "flag_ukraine"},
            {"zh-CN", "flag_china"},
        };

        public static string HtmlFilter => $"{Res.DialogHtmlFiles} (*.htm;*.htm)|*.htm;*.html";
        public static string ImageFilter => $"{Res.DialogImageFiles} (*.bmp;*.jpg;*.gif;*.png)|*.bmp;*.jpg;*.gif;*.png|" +
                                            $"{Res.DialogAllFiles} (*.*)|*.*";
        public static string ModelFilter => $"{Res.DialogAllSupportedFiles}|*.3ds;*.lwo;*.obj;*.objx;*.stl;*.off|" +
                                            $"{Res.Dialog3DStudioFiles} (*.3ds)|*.3ds|" +
                                            $"{Res.DialogLightwaveFiles} (*.lwo)|*.lwo|" +
                                            $"{Res.DialogWavefrontFiles} (*.obj)|*.obj;*.objx|" +
                                            $"{Res.DialogStereoLithographyFiles} (*.stl)|*.stl|" +
                                            $"{Res.DialogOFFFiles} (*.off)|*.off";
        public static string PrefabObjectFilter => $"{Res.DialogSandboxPrefabXmlFiles} (*.sbc)|*.sbc|" +
                                                   $"{Res.DialogSandboxPrefabBinaryFiles} (*.sbc{SpaceEngineersConsts.ProtobuffersExtension})|*.sbc{SpaceEngineersConsts.ProtobuffersExtension}";
        public static string SandboxFilter => $"{Res.DialogSandboxFiles} |Sandbox.sbc";
        public static string SandboxObjectImportFilter => $"{Res.DialogSandboxContentFiles} (*.sbc;*.sbc{SpaceEngineersConsts.ProtobuffersExtension})|*.sbc;*.sbc{SpaceEngineersConsts.ProtobuffersExtension}|" +
                                                          $"{Res.DialogXmlFiles} (*.xml)|*.xml|{Res.DialogAllFiles} (*.*)|*.*";
        public static string SandboxObjectExportFilter => $"{Res.DialogSandboxXmlContentFiles} (*.sbc)|*.sbc|" +
                                                          $"{Res.DialogSandboxBinaryContentFiles} (*.sbc{SpaceEngineersConsts.ProtobuffersExtension})|*.sbc{SpaceEngineersConsts.ProtobuffersExtension}|" +
                                                          $"{Res.DialogXmlFiles} (*.xml)|*.xml|{Res.DialogAllFiles} (*.*)|*.*";
        public static string SpaceEngineersApplicationFilter => $"{Res.DialogSpaceEngineersApplicationFiles}|SpaceEngineers*.exe";
        public static string TextFileFilter => $"{Res.DialogTextFiles} (*.txt)|*.txt";
        public static string VoxelAnyFilter => $"{Res.DialogAsteroidFiles} (*.vox;*.vx2)|*.vox;*.vx2|{Res.DialogAllFiles} (*.*)|*.*";
        public static string VoxelFilter => $"{Res.DialogAsteroidFiles} (*.vx2)|*.vx2";
        public static string XmlFileFilter => $"{Res.DialogXmlFiles} (*.xml)|*.xml";
    }

    public enum ExceptionState
    {
        OK,
        NoRegistry,
        NoDirectory,
        NoApplication,
        MissingContentFile,
        CorruptContentFile,
        EmptyContentFile,
    };

    public enum Mirror
    {
        None,
        EvenUp,
        EvenDown,
        Odd
    };

    public enum ModelTraceVoxel
    {
        /// <summary>
        /// Thin Shell calculation.
        /// </summary>
        Thin,

        /// <summary>
        /// Thin Shell calculation + slope additions.
        /// </summary>
        ThinSmoothed,

        /// <summary>
        /// Thick Shell calculation.
        /// </summary>
        Thick,

        /// <summary>
        /// Thick Shell calculation, with additive slope additions.
        /// </summary>
        ThickSmoothedUp,

        ///// <summary>
        ///// Thick Shell calculation, subtractive slope additions.
        ///// </summary>
        //ThickSmoothedDown,

        //SurfaceCalculated
    };

    public enum SaveWorldType
    {
        // The normal method.
        Local,

        /// <summary>
        /// Console Host.
        /// </summary>
        DedicatedServerHost,

        /// <summary>
        /// Service. Requires Admin access to save.
        /// </summary>
        DedicatedServerService,

        /// <summary>
        /// User custom specified.
        /// </summary>
        Custom,

        /// <summary>
        /// User custom but requires Admin access to save.
        /// </summary>
        CustomAdminRequired
    };

    public enum VoxelMergeType
    {
        UnionVolumeLeftToRight,
        UnionVolumeRightToLeft,

        UnionMaterialLeftToRight,
        UnionMaterialRightToLeft,

        SubtractVolumeLeftFromRight,
        SubtractVolumeRightFromLeft,
    }

    /// <summary>
    /// These represent generic orientations for any cube.
    /// </summary>
    public enum OrientType : byte
    {
        Axis24_Backward_Down,
        Axis24_Backward_Left,
        Axis24_Backward_Right,
        Axis24_Backward_Up,
        Axis24_Down_Backward,
        Axis24_Down_Forward,
        Axis24_Down_Left,
        Axis24_Down_Right,
        Axis24_Forward_Down,
        Axis24_Forward_Left,
        Axis24_Forward_Right,
        Axis24_Forward_Up,
        Axis24_Left_Backward,
        Axis24_Left_Down,
        Axis24_Left_Forward,
        Axis24_Left_Up,
        Axis24_Right_Backward,
        Axis24_Right_Down,
        Axis24_Right_Forward,
        Axis24_Right_Up,
        Axis24_Up_Backward,
        Axis24_Up_Forward,
        Axis24_Up_Left,
        Axis24_Up_Right,
    };


    // TODO: Temporary enum, before creating a proper registrable fill types.
    public enum AsteroidFillType
    {
        None,
        ByteFiller
    }

    public enum ReportType
    {
        Unknown,
        Text,
        Html,
        Xml
    }

    [Flags]
    public enum GridConnectionType
    {
        None = 0x00,

        /// <summary>
        /// Landing Gear.
        /// </summary>
        GearLock = 0x01,

        /// <summary>
        /// Connector.
        /// </summary>
        ConnectorLock = 0x02,

        /// <summary>
        /// Piston, Rotor, Suspension/Wheel.
        /// </summary>
        Mechanical = 0x04,

        ConnectorLock_Mechanical = ConnectorLock + Mechanical,
    }
}
