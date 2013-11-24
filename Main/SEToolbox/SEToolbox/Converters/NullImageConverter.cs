namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Data;

    public class NullImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            if (!File.Exists(value as string))
                return DependencyProperty.UnsetValue;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
