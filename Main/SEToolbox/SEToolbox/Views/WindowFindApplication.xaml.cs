namespace SEToolbox.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowFindApplication.xaml
    /// </summary>
    public partial class WindowFindApplication : Window
    {
        public WindowFindApplication()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }

        public WindowFindApplication(object viewModel)
            : this()
        {
            this.DataContext = viewModel;
        }
    }
}
