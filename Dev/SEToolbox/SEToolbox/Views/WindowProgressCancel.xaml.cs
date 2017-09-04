namespace SEToolbox.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowProgressCancel.xaml
    /// </summary>
    public partial class WindowProgressCancel : Window
    {
        public WindowProgressCancel()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }

        public WindowProgressCancel(object viewModel)
            : this()
        {
            this.DataContext = viewModel;
        }
    }
}
