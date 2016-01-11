namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class DistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var distance = (double)value;

            if (distance > 1000)
                return String.Format("{0:#,###0.0.0} Km", distance / 1000);
            else
                return String.Format("{0:#,###0.0} m", distance);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
