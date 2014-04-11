namespace SEToolbox
{
    using System.Windows.Input;
    using SEToolbox.Interop;
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
        #region Fields

        #endregion

        #region methods

        public void Startup(string[] args)
        {
            string filePath = null;
            if ((Native.GetKeyState(System.Windows.Forms.Keys.ShiftKey) & KeyStates.Down) != KeyStates.Down)
            {
                // TODO: load User Settings when Shift is not held down.
                filePath = Properties.Settings.Default.SEInstallLocation; 
            }

            //// Detection and correction of local settings of SE install location.
            //if (!ToolboxUpdater.ValidateSpaceEngineersInstall(filePath))
            //{
            //    filePath = ToolboxUpdater.GetGameRegistryFilePath();
            //    if (!ToolboxUpdater.ValidateSpaceEngineersInstall(filePath))
            //    {
            //        var faModel = new FindApplicationModel();

            //        faModel.Load();
            //        var faViewModel = new FindApplicationViewModel(faModel);
            //        //if (allowClose)
            //        //{
            //        faViewModel.CloseRequested += (object sender, EventArgs e) => { Application.Current.Shutdown(); };
            //        //}
            //        var faWindow = new WindowFindApplication(faViewModel);
            //        //faWindow.Loaded += (object sender, RoutedEventArgs e) => { Splasher.CloseSplash(); };
            //        faWindow.ShowDialog();
            //        // TODO:
            //    }
            //}

            //Properties.Settings.Default.SEInstallLocation = filePath;
            //Properties.Settings.Default.Save();

            //// Load the SpaceEngineers assemblies, or dependant classes after this point.

            if (!ToolboxUpdater.FindSpaceEngineersLocation())
            {
                MessageBox.Show("SEToolbox could not detect the installation of Space Engineers.\r\nPlease make sure you have purchased a legal copy of the game, and installed it correctly.", "Space Engineers Toolbox", MessageBoxButton.OK, MessageBoxImage.Stop);
                Application.Current.Shutdown();
                return;
            }

            var ignoreUpdates = args.Any(a => a.ToUpper() == "/X");
            var updateAborted = args.Any(a => a.ToUpper() == "/A");

            if (updateAborted)
            {
                MessageBox.Show("SEToolbox could not invoke UAC to update the old base files. Please run SEToolbox from an account which has Administrator access to update the base files before continuing.", "Update aborted", MessageBoxButton.OK, MessageBoxImage.Stop);
                Application.Current.Shutdown();
                return;
            }

            // Dot not load any of the SpaceEngineers assemblies, or dependant classes before this point.
            if (!ignoreUpdates && ToolboxUpdater.IsBaseAssembliesChanged() && !Debugger.IsAttached)
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

            SEToolbox.Interop.SpaceEngineersAPI.Init();
            SEToolbox.ImageLibrary.ImageTextureUtil.Init();

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
            eWindow.Loaded += (object sender, RoutedEventArgs e) => { Splasher.CloseSplash(); };
            eWindow.ShowDialog();
        }

        public void Exit()
        {
            TempfileUtil.Dispose();
        }

        private void SaveSettings(WindowExplorer eWindow)
        {
            // TODO:
            //eWindow.WindowState
            //eWindow.Height;

        }

        #endregion
    }
}
