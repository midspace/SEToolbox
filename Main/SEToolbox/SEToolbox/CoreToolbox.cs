﻿namespace SEToolbox
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows;
    
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Support;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;
    using Res = SEToolbox.Properties.Resources;

    public class CoreToolbox
    {
        private string _tempBinPath;

        #region methods

        public bool Init(string[] args)
        {
            // Detection and correction of local settings of SE install location.
            var filePath = ToolboxUpdater.GetApplicationFilePath();

            if (GlobalSettings.Default.PromptUser || !ToolboxUpdater.ValidateSpaceEngineersInstall(filePath))
            {
                if (Directory.Exists(filePath))
                {
                    var testPath = Path.Combine(filePath, "SpaceEngineers.exe");
                    if (File.Exists(testPath))
                    {
                        filePath = testPath;
                    }
                    else
                    {
                        testPath = Path.Combine(filePath, "SpaceEngineersDedicated.exe");
                        if (File.Exists(testPath))
                        {
                            filePath = testPath;
                        }
                    }
                }

                var faModel = new FindApplicationModel()
                {
                    GameApplicationPath = filePath
                };
                var faViewModel = new FindApplicationViewModel(faModel);
                var faWindow = new WindowFindApplication(faViewModel);

                if (faWindow.ShowDialog() == true)
                {
                    filePath = faModel.GameBinPath;
                }
                else
                {
                    return false;
                }
            }

            // Update and save user path.
            GlobalSettings.Default.SEBinPath = filePath;
            GlobalSettings.Default.Save();

            var ignoreUpdates = args.Any(a => a.ToUpper() == "/X" || a.ToUpper() == "-X");
            var oldDlls = args.Any(a => a.ToUpper() == "/OLDDLL" || a.ToUpper() == "-OLDDLL");
            var altDlls = !oldDlls;

            // Go looking for any changes in the Dependant Space Engineers assemblies and immediately attempt to update.
            if (!ignoreUpdates && !altDlls && ToolboxUpdater.IsBaseAssembliesChanged() && !Debugger.IsAttached)
            {
                ToolboxUpdater.RunElevated(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SEToolboxUpdate"), "/B " + String.Join(" ", args), false, false);
                return false;
            }

            var proc = Process.GetCurrentProcess();
            if (Process.GetProcessesByName(proc.ProcessName).Length == 1)
            {
                // Clean up Temp files if this is the only instance running.
                TempfileUtil.DestroyTempFiles();
            }

            // Dot not load any of the Space Engineers assemblies or dependant classes before this point.
            // ============================================

            // Alternate experimental method for loading the Space Engineers API assemblies.
            // Copy them to temporary path, then load with reflection on demand through the AppDomain.
            if (altDlls)
            {
                _tempBinPath = ToolboxUpdater.GetBinCachePath();
                var searchPath = GlobalSettings.Default.SEBinPath;

                DirectoryInfo checkDir = null;
                var counter = 0;

                while (checkDir == null && counter < 10)
                {
                    if (Directory.Exists(_tempBinPath))
                        break;

                    checkDir = Directory.CreateDirectory(_tempBinPath);

                    if (checkDir == null)
                    {
                        // wait a while, as the drive may be processing Windows Explorer which had a lock on the Directory after prior cleanup.
                        System.Threading.Thread.Sleep(100);
                    }

                    counter++;
                }

                foreach (var file in ToolboxUpdater.CoreSpaceEngineersFiles)
                {
                    var filename = Path.Combine(searchPath, file);
                    var destFilename = Path.Combine(_tempBinPath, file);

                    if (ToolboxUpdater.DoFilesDiffer(searchPath, _tempBinPath, file))
                    {
                        try
                        {
                            File.Copy(filename, destFilename, true);
                        }
                        catch { }
                    }
                }

                // Copy directories which contain Space Engineers language resources.
                var dirs = Directory.GetDirectories(searchPath);

                foreach (var sourceDir in dirs)
                {
                    var dirName = Path.GetFileName(sourceDir);
                    var destDir = Path.Combine(_tempBinPath, dirName);
                    Directory.CreateDirectory(destDir);

                    foreach (string oldFile in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Copy(oldFile, Path.Combine(destDir, Path.GetFileName(oldFile)), true);
                        }
                        catch { }
                    }
                }

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += currentDomain_AssemblyResolve;
            }

#if DEBUG
            // This will make it hairy for testing the AppDomain stuff.
            //#warning Force the local debugger to load the Types allowing inspection.
            //var settings0 = new Sandbox.Common.ObjectBuilders.MyObjectBuilder_SessionSettings();
#endif

            return true;
        }

        public bool Load(string[] args)
        {
            // Fetch the game version and store, so it can be retrieved during crash if the toolbox makes it this far.
            GlobalSettings.Default.SEVersion = SpaceEngineersConsts.GetSEVersion();

            // Test the Space Engineers version to make sure users are using an version that is new enough for SEToolbox to run with!
            // This is usually because a user has not updated a manual install of a Dedicated Server, or their Steam did not update properly.
            if (GlobalSettings.Default.SEVersion < GlobalSettings.GetAppVersion(true))
            {
                MessageBox.Show(string.Format(Res.DialogOldSEVersionMessage, SpaceEngineersConsts.GetSEVersion(), GlobalSettings.Default.SEBinPath, GlobalSettings.GetAppVersion()), Res.DialogOldSEVersionTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Application.Current.Shutdown();
                return false;
            }

            // Force pre-loading of any Space Engineers resources.
            SpaceEngineersCore.LoadDefinitions();

            // Load the Space Engineers assemblies, or dependant classes after this point.
            var explorerModel = new ExplorerModel();

            if (args.Any(a => a.ToUpper() == "/WR" || a.ToUpper() == "-WR"))
            {
                ResourceReportModel.GenerateOfflineReport(explorerModel, args);
                Application.Current.Shutdown();
                return false;
            }

            var eViewModel = new ExplorerViewModel(explorerModel);
            var eWindow = new WindowExplorer(eViewModel);
            //if (allowClose)
            //{
            eViewModel.CloseRequested += (sender, e) =>
            {
                SaveSettings(eWindow);
                Application.Current.Shutdown();
            };
            //}
            eWindow.Loaded += (sender, e) =>
            {
                Splasher.CloseSplash();
                if (GlobalSettings.Default.WindowLeft.HasValue) eWindow.Left = GlobalSettings.Default.WindowLeft.Value;
                if (GlobalSettings.Default.WindowTop.HasValue) eWindow.Top = GlobalSettings.Default.WindowTop.Value;
                if (GlobalSettings.Default.WindowWidth.HasValue) eWindow.Width = GlobalSettings.Default.WindowWidth.Value;
                if (GlobalSettings.Default.WindowHeight.HasValue) eWindow.Height = GlobalSettings.Default.WindowHeight.Value;
                if (GlobalSettings.Default.WindowState.HasValue) eWindow.WindowState = GlobalSettings.Default.WindowState.Value;
            };
            eWindow.ShowDialog();

            return true;
        }

        public void Exit()
        {
            TempfileUtil.Dispose();
        }

        private static void SaveSettings(WindowExplorer eWindow)
        {
            GlobalSettings.Default.WindowState = eWindow.WindowState;
            eWindow.WindowState = WindowState.Normal; // Reset the State before getting the window size.
            GlobalSettings.Default.WindowHeight = eWindow.Height;
            GlobalSettings.Default.WindowWidth = eWindow.Width;
            GlobalSettings.Default.WindowTop = eWindow.Top;
            GlobalSettings.Default.WindowLeft = eWindow.Left;
            GlobalSettings.Default.Save();
        }

        Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Retrieve the list of referenced assemblies in an array of AssemblyName.
            var filename = args.Name.Substring(0, args.Name.IndexOf(",", StringComparison.InvariantCulture)) + ".dll";

            const string filter = @"^(?<assembly>(?:\w+(?:\.?\w+)+))\s*(?:,\s?Version=(?<version>\d+\.\d+\.\d+\.\d+))?(?:,\s?Culture=(?<culture>[\w-]+))?(?:,\s?PublicKeyToken=(?<token>\w+))?$";
            var match = Regex.Match(args.Name, filter);
            if (match.Success)
            {
                filename = match.Groups["assembly"].Value + ".dll";
            }

            if (ToolboxUpdater.CoreSpaceEngineersFiles.Any(f => string.Equals(f, filename, StringComparison.InvariantCultureIgnoreCase)))
            {
                var assemblyPath = Path.Combine(_tempBinPath, filename);

                // Load the assembly from the specified path.
                // Return the loaded assembly.
                return Assembly.LoadFrom(assemblyPath);
            }

            return null;
        }

        #endregion
    }
}
