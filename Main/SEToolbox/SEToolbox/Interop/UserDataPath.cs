namespace SEToolbox.Interop
{
    using System.IO;

    public class UserDataPath
    {
        public UserDataPath(string basePath, string savesPathPart, string modsPathPart)
        {
            this.SavesPath = Path.Combine(basePath, savesPathPart);
            this.ModsPath = Path.Combine(basePath, modsPathPart);
        }

        public UserDataPath(string savesPath, string modsPath)
        {
            this.SavesPath = savesPath;
            this.ModsPath = modsPath;
        }

        public string SavesPath { get; set; }
        public string ModsPath { get; set; }
    }
}