namespace SEToolbox.Support
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public static class TempfileUtil
    {
        private static readonly List<string> Tempfiles;
        public static readonly string TempPath;

        static TempfileUtil()
        {
            Tempfiles = new List<string>();
            TempPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location));
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

        /// <summary>
        /// Generates a temporary filename in the 'c:\users\%username%\AppData\Local\Temp\%ApplicationName%' path.
        /// </summary>
        /// <returns></returns>
        public static string NewFilename()
        {
            return NewFilename(null);
        }

        /// <summary>
        /// Generates a temporary filename in the 'c:\users\%username%\AppData\Local\Temp\%ApplicationName%' path.
        /// </summary>
        /// <param name="fileExtension">optional file extension in the form '.ext'</param>
        /// <returns></returns>
        public static string NewFilename(string fileExtension)
        {
            string filename;

            if (string.IsNullOrEmpty(fileExtension))
            {
                filename = Path.Combine(TempPath, Guid.NewGuid() + ".tmp");
            }
            else
            {
                filename = Path.Combine(TempPath, Guid.NewGuid() + fileExtension);
            }

            Tempfiles.Add(filename);

            return filename;
        }

        /// <summary>
        /// Cleanup, and remove all Temporary files.
        /// </summary>
        public static void Dispose()
        {
            foreach (var filename in Tempfiles)
            {
                if (File.Exists(filename))
                {
                    try
                    {
                        File.Delete(filename);
                    }
                    catch
                    {
                        // Unable to delete any locked files.
                    }
                }
            }

            Tempfiles.Clear();
        }

        public static void DestroyTempFiles()
        {
            var basePath = new DirectoryInfo(TempPath);

            foreach (FileInfo file in basePath.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch { }
            }

            foreach (DirectoryInfo dir in basePath.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                }
                catch { }
            }
        }
    }
}
