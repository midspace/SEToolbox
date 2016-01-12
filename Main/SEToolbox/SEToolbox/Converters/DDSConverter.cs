namespace SEToolbox.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using SEToolbox.ImageLibrary;

    public class DDSConverter : IValueConverter
    {
        private static readonly Dictionary<string, ImageSource> Cache = new Dictionary<string, ImageSource>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string)
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

                var filename = (string)value;
                var extension = Path.GetExtension(filename).ToLower();
                var name = filename;

                if (width != -1 && height != -1)
                {
                    name += string.Format(",{0},{1}", width, height);
                }

                if (Cache.ContainsKey(name))
                {
                    return Cache[name];
                }

                if (extension == ".png")
                {
                    try
                    {
                        // TODO: rescale the bitmap to specified width/height.
                        var bitmapImage = new BitmapImage();
                        var bitmap = (Bitmap)Image.FromFile(filename, true);
                        using (var ms = new MemoryStream())
                        {
                            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = ms;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                        }
                        Cache.Add(name, bitmapImage);
                        return bitmapImage;
                    }
                    catch { return null; }
                }

                if (extension == ".dds")
                {
                    var image = ImageTextureUtil.CreateImage(filename, 0, width, height);
                    Cache.Add(name, image);
                    return image;
                }

                return null;
            }

            throw new NotSupportedException(string.Format("{0} cannot convert from {1}.", GetType().FullName, value.GetType().FullName));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(string.Format("{0} does not support converting back.", GetType().FullName));
        }

        public static void ClearCache()
        {
            Cache.Clear();
        }
    }
}