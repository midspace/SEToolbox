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
        Thin,
        ThinSmoothed,
        Thick,
        ThickSmoothed,
        //SurfaceCalculated
    };

    internal class AppConstants
    {
        public const string HomepageUrl = "http://forums.keenswh.com/post/custom-importereditor-tool-setoolbox-6638984";
        public const string UpdatesUrl = "http://setoolbox.codeplex.com/releases/";
    }
}
