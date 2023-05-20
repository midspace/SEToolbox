namespace SEToolboxUpdate
{
    using SEToolbox.Support;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using Res = SEToolboxUpdate.Properties.Resources;

    class Program
    {
        #region consts

        private const int NoError = 0;
        private const int UpdateBinariesFailed = 1;
        private const int UacDenied = 2;

        #endregion

        #region Main

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);

            // Install.
            if (args.Any(a => a.Equals("/I", StringComparison.OrdinalIgnoreCase) || a.Equals("-I", StringComparison.OrdinalIgnoreCase)))
            {
                InstallConfigurationSettings();
                return;
            }

            // Uninstall.
            if (args.Any(a => a.Equals("/U", StringComparison.OrdinalIgnoreCase) || a.Equals("-U", StringComparison.OrdinalIgnoreCase)))
            {
                UninstallConfigurationSettings();
                return;
            }

            // Binaries.
            if (args.Any(a => a.Equals("/B", StringComparison.OrdinalIgnoreCase) || a.Equals("-B", StringComparison.OrdinalIgnoreCase)))
            {
                UpdateBaseLibrariesFromSpaceEngineers(args);
                return;
            }

            var appFile = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            MessageBox.Show(string.Format(Res.AppParameterHelpMessage, appFile), Res.AppParameterHelpTitle, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK);
        }

        #endregion

        #region InstallConfigurationSettings

        private static void InstallConfigurationSettings()
        {
            DiagnosticsLogging.CreateLog();
            CleanBinCache();
        }

        #endregion

        #region UninstallConfigurationSettings

        private static void UninstallConfigurationSettings()
        {
            DiagnosticsLogging.RemoveLog();
            CleanBinCache();
        }

        #endregion

        #region UpdateBaseLibrariesFromSpaceEngineers

        private static void UpdateBaseLibrariesFromSpaceEngineers(string[] args)
        {
            var attemptedAlready = args.Any(a => a.ToUpper() == "/A");

            var appFile = Assembly.GetExecutingAssembly().Location;
            var appFilePath = Path.GetDirectoryName(appFile);

            if (ToolboxUpdater.IsRuningElevated())
            {
                if (!attemptedAlready)
                {
                    MessageBox.Show(Res.UpdateRequiredMessage, Res.UpdateRequiredTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // is running elevated permission, update the files.
                var updateRet = UpdateBaseFiles(appFilePath);

                if (!attemptedAlready)
                {
                    if (updateRet)
                    {
                        // B = Binaries were updated.
                        ToolboxUpdater.RunElevated(Path.Combine(appFilePath, "SEToolbox.exe"), "/B " + String.Join(" ", args), false, false);
                        Environment.Exit(NoError);
                    }
                    else
                    {
                        // Update failed? Files are readonly. Files are locked. Source Files missing (or renamed).
                        var dialogResult = MessageBox.Show(Res.UpdateErrorMessage, Res.UpdateErrorTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (dialogResult == MessageBoxResult.Yes)
                        {
                            // X = Ignore updates.
                            ToolboxUpdater.RunElevated(Path.Combine(appFilePath, "SEToolbox.exe"), "/X " + String.Join(" ", args), false, false);
                        }
                        Environment.Exit(UpdateBinariesFailed);
                    }
                }

                if (updateRet)
                    Environment.Exit(NoError);
                else
                    Environment.Exit(UpdateBinariesFailed);
            }
            else
            {
                // Does not have elevated permission to run.
                if (!attemptedAlready)
                {
                    MessageBox.Show(Res.UpdateRequiredUACMessage, Res.UpdateRequiredTitle, MessageBoxButton.OK, MessageBoxImage.Information);

                    var ret = ToolboxUpdater.RunElevated(appFile, string.Join(" ", args) + " /A", true, true);
                    if (ret.HasValue)
                    {
                        if (ret.Value == 0)
                        {
                            // B = Binaries were updated.
                            ToolboxUpdater.RunElevated(Path.Combine(appFilePath, "SEToolbox.exe"), "/B " + String.Join(" ", args), false, false);
                            Environment.Exit(NoError);
                        }
                        else
                        {
                            // Update failed? Files are readonly. Files are locked. Source Files missing (or renamed).
                            var dialogResult = MessageBox.Show(Res.UpdateErrorMessage, Res.UpdateErrorTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (dialogResult == MessageBoxResult.Yes)
                            {
                                // X = Ignore updates.
                                ToolboxUpdater.RunElevated(Path.Combine(appFilePath, "SEToolbox.exe"), "/X " + String.Join(" ", args), false, false);
                            }
                            Environment.Exit(ret.Value);
                        }
                    }
                    else
                    {
                        var dialogResult = MessageBox.Show(Res.CancelUACMessage, Res.CancelUACTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (dialogResult == MessageBoxResult.Yes)
                        {
                            // X = Ignore updates.
                            ToolboxUpdater.RunElevated(Path.Combine(appFilePath, "SEToolbox.exe"), "/X " + String.Join(" ", args), false, false);
                        }
                        Environment.Exit(UacDenied);
                    }
                }
            }
        }

        #endregion

        #region UpdateBaseFiles

        /// <summary>
        /// Updates the base library files from the Space Engineers application path.
        /// </summary>
        /// <param name="appFilePath"></param>
        /// <returns>True if it succeeded, False if there was an issue that blocked it.</returns>
        private static bool UpdateBaseFiles(string appFilePath)
        {
            var counter = 0;
            // Wait until SEToolbox is shut down.
            while (Process.GetProcessesByName("SEToolbox").Length > 0)
            {
                System.Threading.Thread.Sleep(100);

                counter++;
                if (counter > 100)
                {
                    // 10 seconds is too long. Abort.
                    return false;
                }
            }

            var baseFilePath = ToolboxUpdater.GetApplicationFilePath();

            foreach (var filename in ToolboxUpdater.CoreSpaceEngineersFiles)
            {
                var sourceFile = Path.Combine(baseFilePath, filename);

                try
                {
                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, Path.Combine(appFilePath, filename), true);
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            foreach (var filename in ToolboxUpdater.OptionalSpaceEngineersFiles)
            {
                var sourceFile = Path.Combine(baseFilePath, filename);

                try
                {
                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, Path.Combine(appFilePath, filename), true);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region CleanBinCache

        /// <summary>
        /// Clear app bin cache.
        /// </summary>
        private static void CleanBinCache()
        {

            var binCache = ToolboxUpdater.GetBinCachePath();
            if (Directory.Exists(binCache))
            {
                try
                {
                    Directory.Delete(binCache, true);
                }
                catch { }
            }
        }

        #endregion
    }
}
