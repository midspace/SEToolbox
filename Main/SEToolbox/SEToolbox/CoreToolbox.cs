namespace SEToolbox
{
    using SEToolbox.Models;
    using SEToolbox.Support;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;
    using System;
    using System.Diagnostics;
    using System.Windows;
    // Make sure none of the SpaceEngineers are referenced here, to prevent preloading of the assemblies.
    // Otherwise the assemblies updater will not work.

    public class CoreToolbox
    {
        #region Fields

        #endregion

        #region methods

        public CoreToolbox()
        {
        }

        public void Startup(string[] args)
        {
            RssFeedItem update = ToolboxUpdater.CheckForUpdates();
            if (update != null)
            {
                var dialogResult = MessageBox.Show(string.Format("A new version of SEToolbox ({0}) is available.\r\nWould you like to download it now?", update.Version), "New version available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    Process.Start(update.Link);
                    Application.Current.Shutdown();
                }
            }

            if (ToolboxUpdater.IsSpaceEngineersInstalled())
            {
                // TODO: split the base application and updater.

                //// Dot not load any of the SpaceEngineers assemblies, or dependant classes before this point.
                //if (ToolboxUpdater.IsBaseAssembliesChanged())
                //{
                //    var dialogResult = MessageBox.Show("The base version of Space Engineers has changed.  If you have not updated SEToolbox, you can update your base files from Space Engineers.\r\nWould you like to do that now?", "Space Engineers update detected", MessageBoxButton.YesNo, MessageBoxImage.Information);
                //    if (dialogResult == MessageBoxResult.Yes)
                //    {
                //        ToolboxUpdater.UpdateBaseFiles();
                //    }
                //}
                // GC.Collect();

                // ============================================

                // Load the SpaceEngineers assemblies, or dependant classes after this point.
                var explorerModel = new ExplorerModel();

                explorerModel.Load();
                var viewModel = new ExplorerViewModel(explorerModel);
                //if (allowClose)
                //{
                viewModel.CloseRequested += (object sender, EventArgs e) => { Application.Current.Shutdown(); };
                //}
                var window = new WindowExplorer(viewModel);
                window.ShowDialog();
            }
        }

        public void Exit()
        {

        }

        #endregion
    }
}
