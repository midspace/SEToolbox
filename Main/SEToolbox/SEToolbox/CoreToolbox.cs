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
            if (SpaceEngineersAPI.IsSpaceEngineersInstalled())
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
        }

        public void Exit()
        {

        }

        #endregion
    }
}
