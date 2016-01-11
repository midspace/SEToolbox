namespace SEToolbox.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowExplorer.xaml
    /// </summary>
    public partial class WindowExplorer : Window
    {
        public WindowExplorer()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }

        public WindowExplorer(object viewModel)
            : this()
        {
            this.DataContext = viewModel;
        }
    }
}
