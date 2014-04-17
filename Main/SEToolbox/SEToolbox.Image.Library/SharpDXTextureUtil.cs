namespace SEToolbox.ImageLibrary
{
    using System.Drawing;
    using System.Windows.Media;

    public static class SharpDXTextureUtil
    {
        static readonly SharpDX.Toolkit.Graphics.GraphicsDevice GraphicsDevice;

        static SharpDXTextureUtil()
        {
            // Create the graphics device. The first time it is called, it may take up to a couple of seconds.
            GraphicsDevice = SharpDX.Toolkit.Graphics.GraphicsDevice.New(SharpDX.Toolkit.Graphics.GraphicsAdapter.Default);
        }

        public static void Init()
        {
            // Placeholder to make sure ctor is called.
        }

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
    }
}
