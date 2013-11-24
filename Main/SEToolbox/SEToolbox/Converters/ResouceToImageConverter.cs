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
            if (parameter is string && !string.IsNullOrEmpty(parameter as string))
            {
                string imageParameter = parameter as string;
                System.Drawing.Bitmap bitmap = null;
                BitmapImage bitmapImage = new BitmapImage();

                // Application Resource - File Build Action is marked as None, but stored in Resources.resx
                // parameter= myresourceimagename
                try
                {
                    bitmap = (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject(imageParameter);
                }
                catch
                {
                }

                if (bitmap != null)
                {
                    MemoryStream ms = new MemoryStream();
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }

                // Embedded Resource - File Build Action is marked as Embedded Resource
                // parameter= MyWpfApplication.EmbeddedResource.myotherimage.png
                Assembly asm = Assembly.GetExecutingAssembly();
                Stream stream = asm.GetManifestResourceStream(imageParameter);
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