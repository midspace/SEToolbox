namespace SEToolbox.Support
{
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using log4net;
    using System;
    using System.Globalization;
    using Res = SEToolbox.Properties.Resources;

    public static partial class DiagnosticsLogging
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            diagReport.AppendLine(Res.ClsErrorUnhandled);

            var appFile = Path.GetFullPath(Assembly.GetEntryAssembly().Location);
            var appFilePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorApplication, ObsufacatePathNames(appFile));
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorCommandLine, ObsufacatePathNames(Environment.CommandLine));
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorCurrentDirectory, ObsufacatePathNames(Environment.CurrentDirectory));
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorSEBinPath, GlobalSettings.Default.SEBinPath);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorSEBinVersion, GlobalSettings.Default.SEVersion);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorProcessorCount, Environment.ProcessorCount);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorOSVersion, Environment.OSVersion);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorVersion, Environment.Version);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorIs64BitOperatingSystem, Environment.Is64BitOperatingSystem);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorIntPtrSize, IntPtr.Size);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorIsAdmin, ToolboxUpdater.IsRuningElevated());
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorCurrentUICulture, CultureInfo.CurrentUICulture.IetfLanguageTag);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorCurrentCulture, CultureInfo.CurrentCulture.IetfLanguageTag);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorTimesStartedTotal, GlobalSettings.Default.TimesStartedTotal);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorTimesStartedLastReset, GlobalSettings.Default.TimesStartedLastReset);
            diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorTimesStartedLastGameUpdate, GlobalSettings.Default.TimesStartedLastGameUpdate);
            diagReport.AppendFormat("\r\n");

            diagReport.AppendFormat("{0}\r\n", Res.ClsErrorFiles);

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
                diagReport.AppendFormat("{0} {1}\r\n", Res.ClsErrorBinCachePath, ObsufacatePathNames(binCache));

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
