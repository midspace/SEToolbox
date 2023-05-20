namespace SEToolbox.Interop
{
    using System;
    using System.IO;

    public class UserDataPath
    {
        #region ctor

        public UserDataPath(string basePath, string savesPathPart, string modsPathPart, string blueprintsPathPart)
        {
            DataPath = basePath;
            SavesPath = Path.Combine(basePath, savesPathPart);
            ModsPath = Path.Combine(basePath, modsPathPart);
            if (blueprintsPathPart != null)
                BlueprintsPath = Path.Combine(basePath, blueprintsPathPart);
        }

        #endregion

        #region properties

        public string DataPath { get; set; }
        public string SavesPath { get; set; }
        public string ModsPath { get; set; }
        public string BlueprintsPath { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Determine the correct UserDataPath for this save game if at all possible to allow finding the mods folder.
        /// </summary>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public static UserDataPath FindFromSavePath(string savePath)
        {
            var dp = SpaceEngineersConsts.BaseLocalPath;
            var basePath = GetPathBase(savePath, SpaceEngineersConsts.SavesFolder);
            if (basePath != null)
            {
                dp = new UserDataPath(basePath, SpaceEngineersConsts.SavesFolder, SpaceEngineersConsts.ModsFolder, SpaceEngineersConsts.BlueprintsFolder);
            }

            return dp;
        }

        #endregion

        #region helpers

        private static string GetPathBase(string path, string baseName)
        {
            var parentPath = path;
            var currentName = Path.GetFileName(parentPath);
            while (currentName != null && !currentName.Equals(baseName, StringComparison.CurrentCultureIgnoreCase))
            {
                parentPath = Path.GetDirectoryName(parentPath);
                currentName = Path.GetFileName(parentPath);
            }

            if (currentName != null && currentName.Equals(baseName, StringComparison.CurrentCultureIgnoreCase))
            {
                return Path.GetDirectoryName(parentPath);
            }

            return null;
        }

        #endregion
    }
}