namespace SEToolbox.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowRegeneratePlanet.xaml
    /// </summary>
    public partial class WindowRegeneratePlanet : Window
    {
        public WindowRegeneratePlanet()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }
    }
}
