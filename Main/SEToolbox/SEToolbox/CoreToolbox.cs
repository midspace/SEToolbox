namespace SEToolbox
{
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;
    using System;
    using System.Windows;

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
            else
            {
                MessageBox.Show("The Space Engineers Game was not detected.\r\nTo use the SEToolbox, you must have SpaceEngineers installed on your computer.\r\n\r\nPlease visit www.SpaceEngineersGame.com to find out more about this exciting game.", "SpaceEngineers not found", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
                Application.Current.Shutdown();
            }
        }

        public void Exit()
        {

        }

        #endregion
    }
}
