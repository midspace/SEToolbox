namespace SEToolbox.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowImportAsteroidModel.xaml
    /// </summary>
    public partial class WindowImportAsteroidModel : Window
    {
        public WindowImportAsteroidModel()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
