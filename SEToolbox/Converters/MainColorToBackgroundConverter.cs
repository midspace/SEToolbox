namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    public class MainColorToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mainColor = (string)value;

            if (mainColor == null)
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            if (mainColor.StartsWith("#"))
            {
                mainColor = mainColor.Substring(1);
            }

            if (mainColor.Length == 8)
            {
                return new SolidColorBrush(Color.FromArgb(
                    System.Convert.ToByte(mainColor.Substring(0, 2), 16),
                    System.Convert.ToByte(mainColor.Substring(2, 2), 16),
                    System.Convert.ToByte(mainColor.Substring(4, 2), 16),
                    System.Convert.ToByte(mainColor.Substring(6, 2), 16)
                ));
            }

            if (mainColor.Length == 6)
            {
                return new SolidColorBrush(Color.FromRgb(
                    System.Convert.ToByte(mainColor.Substring(0, 2), 16),
                    System.Convert.ToByte(mainColor.Substring(2, 2), 16),
                    System.Convert.ToByte(mainColor.Substring(4, 2), 16)
                ));
            }

            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
