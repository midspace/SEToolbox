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
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Log

        public static void LogInfo(string message)
        {
            log.Info(message);
        }

        public static void LogException(Exception exception)
        {
            var diagReport = new StringBuilder();
            diagReport.AppendLine("Unhandled Exception");

            var appFile = Path.GetFullPath(Assembly.GetEntryAssembly().Location);
            var appFilePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            diagReport.AppendFormat("Application: {0}\r\n", appFile);
            diagReport.AppendFormat("Files:\r\n");

            var files = Directory.GetFiles(appFilePath);
            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var fileInfo = new FileInfo(file);
                var fileVer = FileVersionInfo.GetVersionInfo(file);
                diagReport.AppendFormat("{0:O}\t{1:#,###0}\t{2}\t{3}\r\n", fileInfo.LastWriteTime, fileInfo.Length, fileVer.FileVersion, filename);
            }

            log.Fatal(diagReport.ToString(), exception);
        }

        #endregion
    }
}
