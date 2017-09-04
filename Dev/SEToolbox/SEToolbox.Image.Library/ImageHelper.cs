namespace SEToolbox.ImageLibrary
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Media.Imaging;

    public static class ImageHelper
    {
        #region ResizeImage

        //    1) Prevent anti-aliasing.
        //...
        //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //// add below line
        //g.CompositingMode = CompositingMode.SourceCopy;
        //...
        //http://stackoverflow.com/questions/4772273/interpolationmode-highqualitybicubic-introducing-artefacts-on-edge-of-resized-im


        ////resize the image to the specified height and width
        //using (var resized = ImageUtilities.ResizeImage(image, 50, 100))
        //{
        //    //save the resized image as a jpeg with a quality of 90
        //    ImageUtilities.SaveJpeg(@"C:\myimage.jpeg", resized, 90);
        //}

        // Sourced from:
        // http://stackoverflow.com/questions/249587/high-quality-image-scaling-c-sharp

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="size">The width and height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, Size size)
        {
            if (size.Width < 1 || size.Height < 1)
            {
                return null;
            }

            // a holder for the result
            var result = new Bitmap(size.Width, size.Height);
            //set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //use a graphics object to draw the resized image into the bitmap
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }

        #endregion

        #region ConvertBitmapToBitmapImage

        public static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(memoryStream.ToArray());
            bitmapImage.EndInit();
            return bitmapImage;
        }

        #endregion

        #region SavePng

        public static void SavePng(string path, Image image)
        {
            image.Save(path, ImageFormat.Png);
        }

        #endregion
    }
}
