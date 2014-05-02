namespace SEToolbox.Support
{
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

    internal class AppConstants
    {
        public const string DocumentationUrl = "https://setoolbox.codeplex.com/documentation";
        public const string SupportUrl = "http://forums.keenswh.com/post/custom-importereditor-tool-setoolbox-6638984";
        public const string UpdatesUrl = "http://setoolbox.codeplex.com/releases/";
    }
}
