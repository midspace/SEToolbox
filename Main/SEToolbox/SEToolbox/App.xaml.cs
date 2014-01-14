namespace SEToolbox
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Threading;
    using SEToolbox.Interfaces;
    using SEToolbox.Services;
    using SEToolbox.Support;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private CoreToolbox _toolboxApplication;

        #endregion

        private void OnStartup(Object sender, StartupEventArgs e)
        {
            var update = ToolboxUpdater.CheckForUpdates();
            if (update != null)
            {
                var dialogResult = MessageBox.Show(string.Format("A new version of SEToolbox ({0}) is available.\r\nWould you like to download it now?", update.Version), "New version available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    Process.Start(update.Link);
                    Application.Current.Shutdown();
                }
            }

            // Configure service locator.
            ServiceLocator.RegisterSingleton<IDialogService, DialogService>();
            ServiceLocator.Register<IOpenFileDialog, OpenFileDialogViewModel>();
            ServiceLocator.Register<ISaveFileDialog, SaveFileDialogViewModel>();

            this._toolboxApplication = new CoreToolbox();
            this._toolboxApplication.Startup(e.Args);
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            this._toolboxApplication.Exit();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO: Log details to Events.

            string message = null;

            if (e.Exception is ToolboxException)
            {
                message = e.Exception.Message;
            }
            else
            {
                // Unhandled Exception.
                message = string.Format("An error has been detected in the application that has caused the application to shutdown:\n\n{0}\n\nApologies for any inconvenience.", e.Exception.Message);
            }

            MessageBox.Show(message, "SE Toolbox Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (!Debugger.IsAttached)
            {
                e.Handled = true;

                if (Application.Current != null)
                {
                    Application.Current.Shutdown();
                }
            }

            TempfileUtil.Dispose();
        }
    }
}
