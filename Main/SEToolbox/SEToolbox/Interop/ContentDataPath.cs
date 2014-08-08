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
        public ContentDataPath(ContentPathType contentType, string referencePath, string absolutePath)
        {
            this.ContentType = contentType;
            this.ReferencePath = referencePath;
            this.AbsolutePath = absolutePath;
        }

        public ContentPathType ContentType { get; set; }
        public string ReferencePath { get; set; }
        public string AbsolutePath { get; set; }
    }
}