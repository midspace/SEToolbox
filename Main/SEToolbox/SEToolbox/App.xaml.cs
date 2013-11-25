namespace SEToolbox
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Threading;
    using SEToolbox.Interfaces;
    using SEToolbox.Services;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private CoreToolbox toolboxApplication;

        #endregion

        private void OnStartup(Object sender, StartupEventArgs e)
        {
            // Configure service locator
            ServiceLocator.RegisterSingleton<IDialogService, DialogService>();
            ServiceLocator.Register<IOpenFileDialog, OpenFileDialogViewModel>();
            ServiceLocator.Register<ISaveFileDialog, SaveFileDialogViewModel>();

            this.toolboxApplication = new CoreToolbox();

            string[] args = e.Args;

            this.toolboxApplication.Startup(args);
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            this.toolboxApplication.Exit();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO: Log details to Events.
            
            MessageBox.Show(
                string.Format(
                "An error has been detected in the application that has caused the application to shutdown:\n\n{0}\n\nApologies for any inconvenience.",
                e.Exception.Message),
                "SE Toolbox",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            if (!Debugger.IsAttached)
            {
                e.Handled = true;

                if (Application.Current != null)
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}
