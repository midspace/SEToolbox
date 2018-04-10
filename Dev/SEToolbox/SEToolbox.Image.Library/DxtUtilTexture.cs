namespace SEToolbox.ImageLibrary
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Media;

    using SEToolbox.ImageLibrary.Effects;

    public static class DxtUtilTexture
    {
        internal static Bitmap DecompressTextureToBitmap(byte[] imageData, int width, int height, bool ignoreAlpha, uint fourCC, ImageTextureUtil.DXGI_FORMAT dxgiFormat)
        {
            using (var imageStream = new MemoryStream(imageData))
            {
                byte[] pixelColors;

                if (fourCC == (uint)ImageTextureUtil.DDS_FOURCC.DXT1)
                    pixelColors = DxtUtil.DecompressDxt1(imageStream, width, height);
                else if (fourCC == (uint)ImageTextureUtil.DDS_FOURCC.DXT3)
                    pixelColors = DxtUtil.DecompressDxt3(imageStream, width, height);
                else
                    pixelColors = DxtUtil.DecompressDxt5(imageStream, width, height, dxgiFormat);

                // Copy the pixel colors into a byte array
                const int bytesPerPixel = 4;
                var pixelRgba = new byte[pixelColors.Length];
                for (var i = 0; i < pixelColors.Length; i += bytesPerPixel)
                {
                    pixelRgba[i + 0] = pixelColors[i + 2];
                    pixelRgba[i + 1] = pixelColors[i + 1];
                    pixelRgba[i + 2] = pixelColors[i + 0];
                    pixelRgba[i + 3] = ignoreAlpha ? (byte)0xff : pixelColors[i + 3];
                }

                // Here create the Bitmap to the know height, width and format
                var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // Create a BitmapData and Lock all pixels to be written 
                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);

                //Copy the data from the byte array into BitmapData.Scan0
                Marshal.Copy(pixelRgba, 0, bmpData.Scan0, pixelRgba.Length);

                // Unlock the pixels
                bmp.UnlockBits(bmpData);

                return bmp;
            }
        }

        internal static ImageSource DecompressTextureToImageSource(byte[] imageData, int width, int height, bool ignoreAlpha, IPixelEffect effect, uint fourCC, ImageTextureUtil.DXGI_FORMAT dxgiFormat)
        {
            using (var imageStream = new MemoryStream(imageData))
            {
                byte[] pixelColors;

                if (fourCC == (uint)ImageTextureUtil.DDS_FOURCC.DXT1)
                    pixelColors = DxtUtil.DecompressDxt1(imageStream, width, height);
                else if (fourCC == (uint)ImageTextureUtil.DDS_FOURCC.DXT3)
                    pixelColors = DxtUtil.DecompressDxt3(imageStream, width, height);
                else
                    pixelColors = DxtUtil.DecompressDxt5(imageStream, width, height, dxgiFormat);

                // Copy the pixel colors into a byte array
                const int bytesPerPixel = 4;
                var stride = width * bytesPerPixel;
                var pixelRgba = new byte[pixelColors.Length];
                for (var i = 0; i < pixelColors.Length; i += bytesPerPixel)
                {
                    pixelRgba[i + 0] = pixelColors[i + 2];
                    pixelRgba[i + 1] = pixelColors[i + 1];
                    pixelRgba[i + 2] = pixelColors[i + 0];
                    pixelRgba[i + 3] = ignoreAlpha ? (byte)0xff : pixelColors[i + 3];
                }

                if (effect == null)
                    return System.Windows.Media.Imaging.BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixelRgba, stride);

                var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bmp.PixelFormat);

                //Copy the data from the byte array into BitmapData.Scan0
                Marshal.Copy(pixelRgba, 0, bmpData.Scan0, pixelRgba.Length);

                // Unlock the pixels
                bmp.UnlockBits(bmpData);

                bmp = effect.Quantize(bmp);

                return ImageHelper.ConvertBitmapToBitmapImage(bmp);
            }
        }
    }
}
