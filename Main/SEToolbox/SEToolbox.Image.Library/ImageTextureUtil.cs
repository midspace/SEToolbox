namespace SEToolbox.ImageLibrary
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Media;

    public static class ImageTextureUtil
    {
        static SharpDX.Toolkit.Graphics.GraphicsDevice _graphicsDevice;

        static ImageTextureUtil()
        {
            // Create the graphics device
            _graphicsDevice = SharpDX.Toolkit.Graphics.GraphicsDevice.New(SharpDX.Toolkit.Graphics.GraphicsAdapter.Default);
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
            return CreateImage(filename, -1, -1);
        }

        public static ImageSource CreateImage(string filename, int width, int height)
        {
            try
            {
                // Create the graphics device
                using (var graphicsDevice = SharpDX.Toolkit.Graphics.GraphicsDevice.New(SharpDX.Toolkit.Graphics.GraphicsAdapter.Default))
                {
                    // Load the texture
                    using (var texture = SharpDX.Toolkit.Graphics.Texture2D.Load(graphicsDevice, filename))
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

                        var pixelChannel = texture.GetData<byte>(0, mipSlice);
                        return DxtUtil.DecompressDxt5TextureToImageSource(pixelChannel, width, height);
                    }
                }
            }
            catch { }

            return null;
        }

        #endregion

        #region CreateBitmap

        public static Bitmap CreateBitmap(string filename)
        {
            return CreateBitmap(filename, -1, -1);
        }

        public static Bitmap CreateBitmap(string filename, int width, int height)
        {
            try
            {
                // Load the texture
                using (var texture = SharpDX.Toolkit.Graphics.Texture2D.Load(_graphicsDevice, filename))
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

                    var pixelChannel = texture.GetData<byte>(0, mipSlice);
                    return DxtUtil.DecompressDxt5TextureToBitmap(pixelChannel, width, height);
                }
            }
            catch { }

            return null;
        }

        #endregion

        #region MergeImages

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
                graphics.FillRectangle(backgroundFill, 0, 0, image1.Width, image1.Height);

                //draw the image into the target bitmap
                graphics.DrawImage(image1, 0, 0, result.Width, result.Height);
                graphics.DrawImage(image2, 0, 0, result.Width, result.Height);
            }

            return result;
        }

        #endregion
    }
}
