namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool finalValue;

            if (value == null)
                finalValue = false;
            else if (value is bool)
                finalValue = (bool)value;
            else if (value is string)
                finalValue = !string.IsNullOrEmpty((string)value);
            else
                finalValue = true;

            finalValue = !finalValue;

            if (targetType == typeof(Visibility))
                return finalValue ? Visibility.Visible : Visibility.Collapsed;

            return finalValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool finalValue;

            if (value == null)
                finalValue = false;
            else if (value is Visibility)
                finalValue = (Visibility)value == Visibility.Visible;
            else if (value is bool)
                finalValue = (bool)value;
            else
                finalValue = true;

            return !finalValue;
        }
    }
}
