namespace SEToolbox.Support
{
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using log4net;
    using System;

    public static partial class DiagnosticsLogging
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Log

        public static void LogInfo(string message)
        {
            Log.Info(message);
        }

        public static void LogWarning(string message, Exception exception)
        {
            Log.Warn(message, exception);
        }

        public static void LogException(Exception exception)
        {
            var diagReport = new StringBuilder();
            diagReport.AppendLine("Unhandled Exception");

            var appFile = Path.GetFullPath(Assembly.GetEntryAssembly().Location);
            var appFilePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            diagReport.AppendFormat("Application: {0}\r\n", ObsufacatePathNames(appFile));
            diagReport.AppendFormat("CommandLine: {0}\r\n", ObsufacatePathNames(Environment.CommandLine));
            diagReport.AppendFormat("CurrentDirectory: {0}\r\n", ObsufacatePathNames(Environment.CurrentDirectory));
            diagReport.AppendFormat("SEBinPath: {0}\r\n", GlobalSettings.Default.SEBinPath);
            diagReport.AppendFormat("ProcessorCount: {0}\r\n", Environment.ProcessorCount);
            diagReport.AppendFormat("OSVersion: {0}\r\n", Environment.OSVersion);
            diagReport.AppendFormat("Version: {0}\r\n", Environment.Version);
            diagReport.AppendFormat("Is64BitOperatingSystem: {0}\r\n", Environment.Is64BitOperatingSystem);
            diagReport.AppendFormat("IntPtr.Size: {0}\r\n", IntPtr.Size);
            diagReport.AppendFormat("IsAdmin: {0}\r\n", ToolboxUpdater.IsRuningElevated());

            diagReport.AppendFormat("\r\n");

            diagReport.AppendFormat("Files:\r\n");

            if (appFilePath != null)
            {
                var files = Directory.GetFiles(appFilePath);
                foreach (var file in files)
                {
                    var filename = Path.GetFileName(file);
                    var fileInfo = new FileInfo(file);
                    var fileVer = FileVersionInfo.GetVersionInfo(file);
                    diagReport.AppendFormat("{0:O}\t{1:#,###0}\t{2}\t{3}\r\n", fileInfo.LastWriteTime, fileInfo.Length, fileVer.FileVersion, filename);
                }
            }

            var binCache = ToolboxUpdater.GetBinCachePath();
            if (binCache != null && Directory.Exists(binCache))
            {
                diagReport.AppendFormat("\r\n");
                diagReport.AppendFormat("BinCachePath: {0}\r\n", ObsufacatePathNames(binCache));

                var files = Directory.GetFiles(binCache);
                foreach (var file in files)
                {
                    var filename = Path.GetFileName(file);
                    var fileInfo = new FileInfo(file);
                    var fileVer = FileVersionInfo.GetVersionInfo(file);
                    diagReport.AppendFormat("{0:O}\t{1:#,###0}\t{2}\t{3}\r\n", fileInfo.LastWriteTime, fileInfo.Length, fileVer.FileVersion, filename);
                }
            }

            Log.Fatal(diagReport.ToString(), exception);
        }

        private static string ObsufacatePathNames(string path)
        {
            return path.Replace(@"\" + Environment.UserName + @"\", @"\%USERNAME%\");
        }

        #endregion
    }
}
