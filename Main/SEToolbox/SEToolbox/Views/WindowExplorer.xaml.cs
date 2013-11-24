namespace SEToolbox.Views
{
    using System.Globalization;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Interaction logic for WindowExplorer.xaml
    /// </summary>
    public partial class WindowExplorer : Window
    {
        public WindowExplorer()
        {
            this.InitLanguage(CultureInfo.CurrentCulture);
            InitializeComponent();
        }

        public WindowExplorer(object viewModel)
            : this()
        {
            this.DataContext = viewModel;
        }

        #region SetLanguage

        /// <summary>
        /// Initializes the Window's language property according to the passed in context.
        /// </summary>
        /// <param name="culture">The application culture</param>
        public void InitLanguage(CultureInfo culture)
        {
            // Language can only be set in the Constructor, otherwise it does not work.
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(culture.IetfLanguageTag);

            if (Thread.CurrentThread.CurrentUICulture.IetfLanguageTag.ToUpper() != culture.IetfLanguageTag.ToUpper())
            {
                Thread.CurrentThread.CurrentUICulture = culture;
            }

            if (Thread.CurrentThread.CurrentCulture.IetfLanguageTag.ToUpper() != culture.IetfLanguageTag.ToUpper())
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        #endregion
    }
}
