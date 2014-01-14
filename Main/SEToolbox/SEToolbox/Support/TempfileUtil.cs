namespace SEToolbox.Support
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    public static class TempfileUtil
    {
        private static List<string> _tempfiles;
        private static string _tempPath;

        static TempfileUtil()
        {
            _tempfiles = new List<string>();
            _tempPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location));
            if (!Directory.Exists(_tempPath))
                Directory.CreateDirectory(_tempPath);
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
                filename = Path.Combine(_tempPath, Guid.NewGuid() + ".tmp");
            }
            else
            {
                filename = Path.Combine(_tempPath, Guid.NewGuid() + fileExtension);
            }

            _tempfiles.Add(filename);

            return filename;
        }

        /// <summary>
        /// Cleanup, and remove all Temporary files.
        /// </summary>
        public static void Dispose()
        {
            foreach (var filename in _tempfiles)
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }

            _tempfiles.Clear();
        }
    }
}
