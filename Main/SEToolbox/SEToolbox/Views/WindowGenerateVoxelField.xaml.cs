namespace SEToolbox.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowGenerateVoxelField.xaml
    /// </summary>
    public partial class WindowGenerateVoxelField : Window
    {
        public WindowGenerateVoxelField()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }
    }
}
