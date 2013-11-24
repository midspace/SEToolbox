namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int childrenCount = (int)value;
            if (childrenCount > 0)
                return System.Windows.Visibility.Visible;

            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
