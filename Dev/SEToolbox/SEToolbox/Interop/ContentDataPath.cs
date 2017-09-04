namespace SEToolbox.Interop
{
    public enum ContentPathType
    {
        Texture,
        Model,
        SandboxContent,
        SandboxSector,
    };

    public class ContentDataPath
    {
        public ContentDataPath(ContentPathType contentType, string referencePath, string absolutePath, string zipFilePath)
        {
            ContentType = contentType;
            ReferencePath = referencePath;
            AbsolutePath = absolutePath;
            ZipFilePath = zipFilePath;
        }

        public ContentPathType ContentType { get; set; }
        public string ReferencePath { get; set; }
        public string AbsolutePath { get; set; }
        public string ZipFilePath { get; set; }
    }
}