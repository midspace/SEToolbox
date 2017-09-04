namespace SEToolbox.Converters
{
    using System;
    using System.Windows.Data;
    using System.Windows.Input;

    /// <summary>
    /// Sets the cursor state of the mouse.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Cursors))]
    public class BusyMouseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return Cursors.Wait;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Cursors)
            {
                if (value == Cursors.Wait)
                {
                    return true;
                }

                return false;
            }

            return null;
        }
    }
}
