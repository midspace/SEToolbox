namespace SEToolbox.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Res = SEToolbox.Properties.Resources;

    public class DistanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var distance = (double)value;

            if (distance > 1000)
                return string.Format("{0:#,###0.0.0} {1}", distance / 1000, Res.GlobalSIDistanceKilometre);

            return string.Format("{0:#,###0.0} {1}", distance, Res.GlobalSIDistanceMetre);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
