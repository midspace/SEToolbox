namespace SEToolbox
{
    using System;
    using System.Windows;
    using SEToolbox.Models;
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
            ExplorerModel explorerModel = new ExplorerModel();
            explorerModel.Load();
            ExplorerViewModel viewModel = new ExplorerViewModel(explorerModel);
            //if (allowClose)
            //{
            viewModel.CloseRequested += (object sender, EventArgs e) => { Application.Current.Shutdown(); };
            //}
            var window = new WindowExplorer(viewModel);
            window.ShowDialog();

        }

        public void Exit()
        {

        }

        #endregion
    }
}
