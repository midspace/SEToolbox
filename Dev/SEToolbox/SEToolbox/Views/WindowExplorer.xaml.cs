namespace SEToolbox.Views
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Shell;

    /// <summary>
    /// Interaction logic for WindowExplorer.xaml
    /// </summary>
    public partial class WindowExplorer : Window
    {
        public WindowExplorer()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();

            try
            {
                object taskbarList = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("56FDF344-FD6D-11d0-958A-006097C9A090")));
                taskbarList = null;

                TaskbarItemInfo taskbar = new TaskbarItemInfo();
                BindingOperations.SetBinding(taskbar, TaskbarItemInfo.ProgressStateProperty, new Binding("ProgressState"));
                BindingOperations.SetBinding(taskbar, TaskbarItemInfo.ProgressValueProperty, new Binding("ProgressValue"));
                this.TaskbarItemInfo = taskbar;
            }
            catch
            {
                // This is to replace the Xaml code below that implments TaskbarInfoItem, and instead do it through code, so that it can catch a little known error.
                //    System.OutOfMemoryException: Retrieving the COM class factory for component with CLSID {56FDF344-FD6D-11D0-958A-006097C9A090} failed 
                //    due to the following error: 8007000e Not enough storage is available to complete this operation. (Exception from HRESULT: 0x8007000E (E_OUTOFMEMORY)).
                // The cause of this error is unknown, but there have been 4 reported cases of this issue during the life of SEToolbox so far.
                // In this case, the progress bar has been coded to gracefully degrade if this issue occurs.

                //<Window.TaskbarItemInfo>
                //<TaskbarItemInfo ProgressState="{Binding ProgressState}" ProgressValue="{Binding ProgressValue}" />
                //</Window.TaskbarItemInfo>
            }

        }

        public WindowExplorer(object viewModel)
            : this()
        {
            this.DataContext = viewModel;
        }
    }
}
