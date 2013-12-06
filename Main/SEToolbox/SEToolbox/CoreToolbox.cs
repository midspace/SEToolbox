namespace SEToolbox
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Support;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;

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
            RssFeedItem update = ToolboxExtensions.CheckForUpdates();
            if (update != null)
            {
                var dialogResult = MessageBox.Show(string.Format("A new version of SEToolbox ({0}) is available.\r\nWould you like to download it now?", update.Version), "New version available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    Process.Start(update.Link);
                    Application.Current.Shutdown();
                }
            }

            ExplorerModel explorerModel = new ExplorerModel();
            SpaceEngineersAPI.InstallState state = SpaceEngineersAPI.IsSpaceEngineersInstalled();
            if (state == SpaceEngineersAPI.InstallState.OK)
            {
                explorerModel.Load();
                ExplorerViewModel viewModel = new ExplorerViewModel(explorerModel);
                //if (allowClose)
                //{
                viewModel.CloseRequested += (object sender, EventArgs e) => { Application.Current.Shutdown(); };
                //}
                var window = new WindowExplorer(viewModel);
                window.ShowDialog();
            }
            else
            {
                MessageBox.Show(string.Format("The Space Engineers Game was not detected.\r\nTo use the SEToolbox, you must have SpaceEngineers installed on your computer.\r\n\r\nPlease visit www.SpaceEngineersGame.com to find out more about this exciting game.\r\n\r\nCode [{0}]", state), "SpaceEngineers not found", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                Application.Current.Shutdown();
            }
        }

        public void Exit()
        {

        }

        #endregion
    }
}
