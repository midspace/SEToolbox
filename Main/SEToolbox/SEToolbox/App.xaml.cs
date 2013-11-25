namespace SEToolbox
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Threading;
    using SEToolbox.Interfaces;
    using SEToolbox.Services;
    using System.Drawing;
    using SEToolbox.Support;
    using System.Linq;

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

            //testlab();

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


        private void testlab()
        {
            var x = new System.Windows.Media.Media3D.Vector3D(10, 20, 30);

            x.Negate();

            //x.Normalize();

            var y = new VRageMath.Vector3(10, 20, 30);

            var a = y.Cross(new VRageMath.Vector3(-1, -1, -1));

            var b = y.Dot(new VRageMath.Vector3(-1));

            //y.



        }
    }
}
