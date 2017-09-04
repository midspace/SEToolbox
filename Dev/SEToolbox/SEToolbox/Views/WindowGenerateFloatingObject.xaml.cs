namespace SEToolbox.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowGenerateFloatingObject.xaml
    /// </summary>
    public partial class WindowGenerateFloatingObject : Window
    {
        public WindowGenerateFloatingObject()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }
    }
}
