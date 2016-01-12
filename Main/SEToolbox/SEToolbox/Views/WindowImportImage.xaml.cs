namespace SEToolbox.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowImportImage.xaml
    /// </summary>
    public partial class WindowImportImage : Window
    {
        public WindowImportImage()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }
    }
}