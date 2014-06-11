namespace SEToolbox
{
    using System.Globalization;
    using System.Threading;
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
    using WPFLocalizeExtension.Engine;
    using Res = SEToolbox.Properties.Resources;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private CoreToolbox _toolboxApplication;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region events

        private void OnStartup(Object sender, StartupEventArgs e)
        {
            if ((Native.GetKeyState(System.Windows.Forms.Keys.ShiftKey) & KeyStates.Down) == KeyStates.Down)
            {
                // Reset User Settings when Shift is held down during start up.
                GlobalSettings.Default.Reset();
            }

            LocalizeDictionary.Instance.SetCurrentThreadCulture = false;
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);
            Thread.CurrentThread.CurrentUICulture = LocalizeDictionary.Instance.Culture;

            Splasher.Splash = new WindowSplashScreen();
            Splasher.ShowSplash();

            log4net.Config.XmlConfigurator.Configure();

            var update = CodeplexReleases.CheckForUpdates();
            if (update != null)
            {
                var dialogResult = MessageBox.Show(string.Format(Res.DialogNewVersionMessage, update.Version), Res.DialogNewVersionTitle, MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    Process.Start(update.Link);
                    GlobalSettings.Default.Save();
                    Application.Current.Shutdown();
                    return;
                }
            }

            // Configure service locator.
            ServiceLocator.RegisterSingleton<IDialogService, DialogService>();
            ServiceLocator.Register<IOpenFileDialog, OpenFileDialogViewModel>();
            ServiceLocator.Register<ISaveFileDialog, SaveFileDialogViewModel>();
            ServiceLocator.Register<IColorDialog, ColorDialogViewModel>();
            ServiceLocator.Register<IFolderBrowserDialog, FolderBrowserDialogViewModel>();

            this._toolboxApplication = new CoreToolbox();
            if (this._toolboxApplication.Init(e.Args))
                this._toolboxApplication.Load(e.Args);
            else
                Application.Current.Shutdown();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            if (this._toolboxApplication != null)
                this._toolboxApplication.Exit();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var comException = e.Exception as System.Runtime.InteropServices.COMException;

            if (comException != null && comException.ErrorCode == -2147221040)
            {
                // To fix 'OpenClipboard Failed (Exception from HRESULT: 0x800401D0 (CLIPBRD_E_CANT_OPEN)'
                // http://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid

                e.Handled = true;
                return;
            }

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
                    message = string.Format(Res.DialogUnhandledExceptionEventMessage, e.Exception.Message);
                else
                    message = string.Format(Res.DialogUnhandledExceptionMessage, e.Exception.Message);
            }

            MessageBox.Show(message, Res.DialogUnhandledExceptionTitle, MessageBoxButton.OK, MessageBoxImage.Error);

            TempfileUtil.Dispose();

            e.Handled = true;

            if (Application.Current != null)
            {
                Application.Current.Shutdown();
            }
        }

        #endregion
    }
}
