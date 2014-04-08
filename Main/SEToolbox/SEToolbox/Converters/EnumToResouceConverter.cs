namespace SEToolbox.Converters
{
    using System;
    using System.Windows.Data;

    public class EnumToResouceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (value is Enum)
                {
                    string header = ((Enum)value).GetType().Name;
                    string resource = string.Format("{0}_{1}", header, value);
                    return GetResource(resource, value);
                }
                else if (value is bool)
                {
                    return GetResource(string.Format("{0}_{1}", value.GetType().Name, value), value);
                }

                return value;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        private object GetResource(string resource, object value)
        {
            try
            {
                string ret = Properties.Resources.ResourceManager.GetString(resource);

                if (ret == null)
                {
                    return value;
                }

                return ret;
            }
            catch
            {
                return value;
            }
        }

    }
}