namespace SEToolbox.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowImportVoxel.xaml
    /// </summary>
    public partial class WindowImportVoxel : Window
    {
        public WindowImportVoxel()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }
    }
}
