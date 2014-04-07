namespace SEToolbox.Converters
{
    using SEToolbox.ImageLibrary;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    public class DDSConverter : IValueConverter
    {
        private static readonly Dictionary<string, ImageSource> Cache = new Dictionary<string, ImageSource>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is string)
            {
                var sizeParameter = parameter as string;
                var sizeArray = sizeParameter.Split(',');
                int width = -1;
                int height = -1;
                if (sizeArray.Length == 2)
                {
                    Int32.TryParse(sizeArray[0], out width);
                    Int32.TryParse(sizeArray[1], out height);
                }

                var name = (string)value;

                if (width != -1 && height != -1)
                {
                    name += string.Format(",{0},{1}", width, height);
                }

                if (Cache.ContainsKey(name))
                {
                    return Cache[name];
                }

                var image = ImageTextureUtil.CreateImage((string)value, 0, width, height);
                Cache.Add(name, image);
                return image;
            }
            else
            {
                throw new NotSupportedException(string.Format("{0} cannot convert from {1}.", this.GetType().FullName, value.GetType().FullName));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(string.Format("{0} does not support converting back.", this.GetType().FullName));
        }
    }
}