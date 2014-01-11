namespace SEToolbox
{
    using SEToolbox.Models;
    using SEToolbox.Support;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Linq;
    using System.Globalization;

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
            if (ToolboxUpdater.IsSpaceEngineersInstalled())
            {
                var ignoreUpdates = args.Any(a => a.ToUpper() == "/X");

                // Dot not load any of the SpaceEngineers assemblies, or dependant classes before this point.
                if (!ignoreUpdates && ToolboxUpdater.IsBaseAssembliesChanged())
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
