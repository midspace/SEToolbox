namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class StringFormatMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var copy = (object[])values.Clone();

            // Remove the {DependancyProperty.UnsetValue} from unbound datasources.
            for (int i = 0; i < copy.Length; i++)
            {
                if (copy[i] != null && copy[i] == DependencyProperty.UnsetValue)
                {
                    copy[i] = null;
                }
            }

            return String.Format((string)parameter, copy);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }
    }
}