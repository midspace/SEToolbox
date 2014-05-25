namespace SEToolbox
{
    using SEToolbox.Models;
    using SEToolbox.Support;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;

    public class CoreToolbox
    {
        private string _tempBinPath;

        #region methods

        public bool Init(string[] args)
        {
            // Detection and correction of local settings of SE install location.
            var filePath = ToolboxUpdater.GetApplicationFilePath();

            if (!ToolboxUpdater.ValidateSpaceEngineersInstall(filePath))
            {
                var faModel = new FindApplicationModel();
                var faViewModel = new FindApplicationViewModel(faModel);
                var faWindow = new WindowFindApplication(faViewModel);
                var ret = faWindow.ShowDialog();
                if (ret == true)
                {
                    filePath = faModel.GameRootPath;
                }
                else
                {
                    Application.Current.Shutdown();
                    return false;
                }
            }

            // Update and save user path.
            GlobalSettings.Default.SEInstallLocation = filePath;
            GlobalSettings.Default.Save();

            var ignoreUpdates = args.Any(a => a.ToUpper() == "/X");
            var oldDlls = args.Any(a => a.ToUpper() == "/OLDDLL");
            var altDlls = !oldDlls;

            // Go looking for any changes in the Dependant Space Engineers assemblies and immediately attempt to update.
            if (!ignoreUpdates && ToolboxUpdater.IsBaseAssembliesChanged() && !altDlls && !Debugger.IsAttached)
            {
                ToolboxUpdater.RunElevated(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "SEToolboxUpdate"), "/B " + String.Join(" ", args), false, false);
                Application.Current.Shutdown();
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
                _tempBinPath = Path.Combine(TempfileUtil.TempPath, "Bin");

                if (!Directory.Exists(_tempBinPath))
                    Directory.CreateDirectory(_tempBinPath);

                foreach (var file in ToolboxUpdater.CoreSpaceEngineersFiles)
                {
                    var filename = Path.Combine(Path.Combine(GlobalSettings.Default.SEInstallLocation, "Bin64"), file);
                    var destFilename = Path.Combine(_tempBinPath, file);

                    try
                    {
                        File.Copy(filename, destFilename, true);
                    }
                    catch { }
                }

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += currentDomain_AssemblyResolve;
            }

            return true;
        }

        public void Load(string[] args)
        {
            // Force pre-loading of any Space Engineers resources.
            SEToolbox.Interop.SpaceEngineersAPI.Init();

            // Load the SpaceEngineers assemblies, or dependant classes after this point.
            var explorerModel = new ExplorerModel();
            explorerModel.Load();

            if (args.Any(a => a.ToUpper() == "/WR"))
            {
                ResourceReportModel.GenerateOfflineReport(explorerModel, args);
                Application.Current.Shutdown();
                return;
            }

            var eViewModel = new ExplorerViewModel(explorerModel);
            var eWindow = new WindowExplorer(eViewModel);
            //if (allowClose)
            //{
            eViewModel.CloseRequested += (object sender, EventArgs e) =>
            {
                SaveSettings(eWindow);
                Application.Current.Shutdown();
            };
            //}
            eWindow.Loaded += (object sender, RoutedEventArgs e) =>
            {
                Splasher.CloseSplash();
                if (GlobalSettings.Default.WindowLeft.HasValue) eWindow.Left = GlobalSettings.Default.WindowLeft.Value;
                if (GlobalSettings.Default.WindowTop.HasValue) eWindow.Top = GlobalSettings.Default.WindowTop.Value;
                if (GlobalSettings.Default.WindowWidth.HasValue) eWindow.Width = GlobalSettings.Default.WindowWidth.Value;
                if (GlobalSettings.Default.WindowHeight.HasValue) eWindow.Height = GlobalSettings.Default.WindowHeight.Value;
                if (GlobalSettings.Default.WindowState.HasValue) eWindow.WindowState = GlobalSettings.Default.WindowState.Value;
            };
            eWindow.ShowDialog();
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
            var filename = args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";

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
