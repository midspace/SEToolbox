namespace SEToolbox.Support
{
    using System.Collections.Generic;

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

    internal class AppConstants
    {
        public const string DocumentationUrl = "https://setoolbox.codeplex.com/documentation";
        public const string SupportUrl = "http://forums.keenswh.com/post/custom-importereditor-tool-setoolbox-6638984";
        public const string UpdatesUrl = "http://setoolbox.codeplex.com/releases/";

        internal static Dictionary<string, string> SupportedLanguages = new Dictionary<string, string>()
        {
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
            //{"zh", "flag_china"},
        };
    }
}
