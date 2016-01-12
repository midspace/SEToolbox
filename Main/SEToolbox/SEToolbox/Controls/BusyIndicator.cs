namespace SEToolbox.Controls
{
    using System.Windows;
    using System.Windows.Controls;

    public class BusyIndicator : Control
    {
        #region construction

        static BusyIndicator()
        {
            // This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            // This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BusyIndicator), new FrameworkPropertyMetadata(typeof(BusyIndicator)));
        }

        #endregion
    }
}
