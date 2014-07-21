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

        public string[] GetReferenceFiles()
        {
            switch (ContentType)
            {
                case ContentPathType.Texture: return new[] { ReferencePath + ".dds", ReferencePath + ".png" };
                case ContentPathType.Model: return new[] { ReferencePath + ".mwm" };
                case ContentPathType.SandboxContent: return new[] { ReferencePath + ".sbc" };
                case ContentPathType.SandboxSector: return new[] { ReferencePath + ".sbs" };
            }
            return new string[0];
        }
    }
}