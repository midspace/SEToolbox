namespace SEToolbox.ImageLibrary
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Media;

    public static class ImageTextureUtil
    {
        static readonly SharpDX.Toolkit.Graphics.GraphicsDevice GraphicsDevice;

        static ImageTextureUtil()
        {
            // Create the graphics device. The first time it is called, it may take up to a couple of seconds.
            GraphicsDevice = SharpDX.Toolkit.Graphics.GraphicsDevice.New(SharpDX.Toolkit.Graphics.GraphicsAdapter.Default);
        }

        public static void Init()
        {
            // Placeholder to make sure ctor is called.
        }

        #region WriteImage

        public static void WriteImage(Bitmap image, string filename)
        {
            if (image == null)
                return;

            image.Save(filename, ImageFormat.Png);
        }

        #endregion

        #region CreateImage

        public static ImageSource CreateImage(string filename)
        {
            return CreateImage(filename, 0, -1, -1);
        }

        public static ImageSource CreateImage(string filename, int depthSlice, int width, int height, bool ignoreAlpha = false)
        {
            try
            {
                // Create the graphics device
                using (var graphicsDevice = SharpDX.Toolkit.Graphics.GraphicsDevice.New(SharpDX.Toolkit.Graphics.GraphicsAdapter.Default))
                {
                    // Load the texture
                    using (var texture = SharpDX.Toolkit.Graphics.Texture.Load(graphicsDevice, filename))
                    {
                        var buffer = texture.GetDataAsImage().PixelBuffer;
                        var mipSlice = 0;
                        if (height != -1 && width != -1)
                        {
                            for (var i = 0; i < buffer.Count; i++)
                            {
                                if (buffer[i].Width == width && buffer[i].Height == height)
                                {
                                    mipSlice = i;
                                    break;
                                }
                            }
                        }

                        width = buffer[mipSlice].Width;
                        height = buffer[mipSlice].Height;

                        var pixelChannel = texture.GetData<byte>(depthSlice, mipSlice);
                        return DxtUtil.DecompressDxt5TextureToImageSource(pixelChannel, width, height, ignoreAlpha);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region CreateBitmap

        public static Bitmap CreateBitmap(string filename)
        {
            return CreateBitmap(filename, 0, -1, -1);
        }

        public static Bitmap CreateBitmap(string filename, int depthSlice, int width, int height, bool ignoreAlpha = false)
        {
            try
            {
                // Load the texture
                using (var texture = SharpDX.Toolkit.Graphics.Texture.Load(GraphicsDevice, filename))
                {
                    var buffer = texture.GetDataAsImage().PixelBuffer;
                    var mipSlice = 0;
                    if (height != -1 && width != -1)
                    {
                        for (var i = 0; i < buffer.Count; i++)
                        {
                            if (buffer[i].Width == width && buffer[i].Height == height)
                            {
                                mipSlice = i;
                                break;
                            }
                        }
                    }

                    width = buffer[mipSlice].Width;
                    height = buffer[mipSlice].Height;

                    var pixelChannel = texture.GetData<byte>(depthSlice, mipSlice);
                    return DxtUtil.DecompressDxt5TextureToBitmap(pixelChannel, width, height, ignoreAlpha);
                }
            }
            catch
            {
                return null;                
            }
        }

        #endregion

        #region MergeImages

        /// <summary>
        /// Merges two specifed Bitmaps, and applies a background brush.
        /// </summary>
        /// <param name="image1">Back image</param>
        /// <param name="image2">Front image</param>
        /// <param name="backgroundFill">Specify a background brush, or Brushes.Transparent for none.</param>
        /// <returns></returns>
        public static Bitmap MergeImages(Bitmap image1, Bitmap image2, System.Drawing.Brush backgroundFill)
        {
            var result = new Bitmap(image1.Width, image1.Height);

            using (var graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Apply any fill.
                if (backgroundFill != null)
                {
                    graphics.FillRectangle(backgroundFill, 0, 0, image1.Width, image1.Height);
                }

                //draw the image into the target bitmap
                graphics.DrawImage(image1, 0, 0, result.Width, result.Height);
                graphics.DrawImage(image2, 0, 0, result.Width, result.Height);
            }

            return result;
        }

        #endregion
    }
}
