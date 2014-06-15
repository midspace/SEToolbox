namespace SEToolbox.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for WindowResourceReport.xaml
    /// </summary>
    public partial class WindowResourceReport : Window
    {
        public WindowResourceReport()
        {
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            InitializeComponent();
        }

        // TODO: remove from code behind, into behavior?
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Loads context menu from Button as a Drop down Menu.
            var button = sender as Button;
            button.ContextMenu.IsEnabled = true;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }
    }
}
