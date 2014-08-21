namespace SEToolbox.Interop
{
    using System.IO;

    public class UserDataPath
    {
        public UserDataPath(string basePath, string savesPathPart, string modsPathPart)
        {
            SavesPath = Path.Combine(basePath, savesPathPart);
            ModsPath = Path.Combine(basePath, modsPathPart);
        }

        public UserDataPath(string savesPath, string modsPath)
        {
            SavesPath = savesPath;
            ModsPath = modsPath;
        }

        public string SavesPath { get; set; }
        public string ModsPath { get; set; }
    }
}