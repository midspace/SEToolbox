namespace SEToolbox
{
    using SEToolbox.Models;
    using SEToolbox.Support;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;

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

            var ignoreUpdates = args.Any(a => a.ToUpper() == "/X");
            
            // Go looking for any changes in the Dependant Space Engineers assemblies and immediately attempt to update.
            if (!ignoreUpdates && ToolboxUpdater.IsBaseAssembliesChanged() && !Debugger.IsAttached)
            {
                ToolboxUpdater.RunElevated(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "SEToolboxUpdate"), "/B", false, false);
                Application.Current.Shutdown();
                return;
            }

            // Dot not load any of the Space Engineers assemblies or dependant classes before this point.
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
