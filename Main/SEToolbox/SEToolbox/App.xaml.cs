namespace SEToolbox
{
    using log4net;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using SEToolbox.Views;
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Res = SEToolbox.Properties.Resources;

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
            Splasher.Splash = new WindowSplashScreen();
            Splasher.ShowSplash();

            if ((Native.GetKeyState(System.Windows.Forms.Keys.ShiftKey) & KeyStates.Down) == KeyStates.Down)
            {
                // Reset User Settings when Shift is held down during start up.
                GlobalSettings.Default.Reset();
            }

            log4net.Config.XmlConfigurator.Configure(); 

            var update = CodeplexReleases.CheckForUpdates();
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
            if (this._toolboxApplication != null)
                this._toolboxApplication.Exit();
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Log details to Application Event Log.
            DiagnosticsLogging.LogException(e.Exception);

            string message = null;

            if (e.Exception is ToolboxException)
            {
                message = e.Exception.Message;
            }
            else
            {
                // Unhandled Exception.
                if (DiagnosticsLogging.LoggingSourceExists())
                    message = string.Format(Res.UnhandledExceptionEventMessage, e.Exception.Message);
                else
                    message = string.Format(Res.UnhandledExceptionMessage, e.Exception.Message);
            }

            MessageBox.Show(message, Res.UnhandledExceptionTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            TempfileUtil.Dispose();

            e.Handled = true;

            if (Application.Current != null)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
