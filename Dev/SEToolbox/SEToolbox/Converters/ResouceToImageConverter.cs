namespace SEToolbox.Converters
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    public class ResouceToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var imageParameter = value as string;
            if (imageParameter == null)
                imageParameter = parameter as string;

            if (!string.IsNullOrEmpty(imageParameter as string))
            {
                System.Drawing.Bitmap bitmap = null;
                var bitmapImage = new BitmapImage();

                // Application Resource - File Build Action is marked as None, but stored in Resources.resx
                // parameter= myresourceimagename
                try
                {
                    bitmap = Properties.Resources.ResourceManager.GetObject(imageParameter) as System.Drawing.Bitmap;
                }
                catch { }

                if (bitmap != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                    }
                    return bitmapImage;
                }

                // Embedded Resource - File Build Action is marked as Embedded Resource
                // parameter= MyWpfApplication.EmbeddedResource.myotherimage.png
                var asm = Assembly.GetExecutingAssembly();
                var stream = asm.GetManifestResourceStream(imageParameter);
                if (stream != null)
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }

                // This is the standard way of using Image.SourceDependancyProperty.  You shouldn't need to use a converter to to this.
                //// Resource - File Build Action is marked as Resource
                //// parameter= pack://application:,,,/MyWpfApplication;component/Images/myfunkyimage.png
                //Uri imageUriSource = null;
                //if (Uri.TryCreate(imageParameter, UriKind.RelativeOrAbsolute, out imageUriSource))
                //{
                //    bitmapImage.BeginInit();
                //    bitmapImage.UriSource = imageUriSource;
                //    bitmapImage.EndInit();
                //    return bitmapImage;
                //}
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}