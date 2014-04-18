namespace SEToolbox
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using SEToolbox.Models;
    using SEToolbox.Support;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;

    public class CoreToolbox
    {
        #region methods

        public void Startup(string[] args)
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
                    return;
                }
            }

            // Update and save user path.
            GlobalSettings.Default.SEInstallLocation = filePath;
            GlobalSettings.Default.Save();


            // Load the SpaceEngineers assemblies, or dependant classes after this point.

            var ignoreUpdates = args.Any(a => a.ToUpper() == "/X");
            var updateAborted = args.Any(a => a.ToUpper() == "/A");

            if (updateAborted)
            {
                MessageBox.Show("SEToolbox could not invoke UAC to update the old base files. Please run SEToolbox from an account which has Administrator access to update the base files before continuing.", "Update aborted", MessageBoxButton.OK, MessageBoxImage.Stop);
                Application.Current.Shutdown();
                return;
            }

            // Dot not load any of the SpaceEngineers assemblies, or dependant classes before this point.
            if (!ignoreUpdates && ToolboxUpdater.IsBaseAssembliesChanged()) // && !Debugger.IsAttached)
            {
                // Already running as administrator. Run the updater and shut down.
                if (ToolboxUpdater.CheckIsRuningElevated())
                {
                    ToolboxUpdater.RunElevated(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "SEToolboxUpdate"), null, true);
                    Application.Current.Shutdown();
                    return;
                }

                MessageBox.Show("The base version of Space Engineers has changed.\r\nSEToolbox needs to update the base files from Space Engineers before starting.\r\n\r\nPlease press Yes at the UAC prompt to continue.", "Space Engineers update detected", MessageBoxButton.OK, MessageBoxImage.Information);
                GC.Collect();
                if (ToolboxUpdater.RunElevated(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "SEToolboxUpdate"), null, true))
                {
                    Application.Current.Shutdown();
                    return;
                }
                else
                {
                    var dialogResult = MessageBox.Show("By cancelling the UAC request you are running with old base files, and risk corrupting any Space Engineers world when you save.\r\n\r\nIf you accept this risk, then press Yes to continue.", "Update cancelled", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (dialogResult == MessageBoxResult.No)
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                }
            }

            // ============================================

            // Load the SpaceEngineers assemblies, or dependant classes after this point.
            var explorerModel = new ExplorerModel();

            // Force pre-loading of any Space Engineers resources.
            SEToolbox.Interop.SpaceEngineersAPI.Init();

            explorerModel.Load();
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

        #endregion
    }
}
