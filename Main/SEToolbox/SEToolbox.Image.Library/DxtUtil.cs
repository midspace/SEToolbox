// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
//
// https://github.com/mono/MonoGame/blob/develop/MonoGame.Framework/Graphics/DxtUtil.cs
// 
namespace SEToolbox.ImageLibrary
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Media;

    using SEToolbox.ImageLibrary.Effects;

    public static class DxtUtil
    {
        internal static Bitmap DecompressDxt5TextureToBitmap(byte[] imageData, int width, int height, bool ignoreAlpha)
        {
            using (var imageStream = new MemoryStream(imageData))
            {
                var pixelColors = DecompressDxt5(imageStream, width, height);

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

        internal static ImageSource DecompressDxt5TextureToImageSource(byte[] imageData, int width, int height, bool ignoreAlpha, IPixelEffect effect)
        {
            using (var imageStream = new MemoryStream(imageData))
            {
                var pixelColors = DecompressDxt5(imageStream, width, height);

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

        internal static byte[] DecompressDxt5(byte[] imageData, int width, int height)
        {
            using (var imageStream = new MemoryStream(imageData))
                return DecompressDxt5(imageStream, width, height);
        }

        internal static byte[] DecompressDxt5(Stream imageStream, int width, int height)
        {
            var imageData = new byte[width * height * 4];

            using (var imageReader = new BinaryReader(imageStream))
            {
                var blockCountX = (width + 3) / 4;
                var blockCountY = (height + 3) / 4;

                for (var y = 0; y < blockCountY; y++)
                {
                    for (var x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt5Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        private static void DecompressDxt5Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            var alpha0 = imageReader.ReadByte();
            var alpha1 = imageReader.ReadByte();

            var alphaMask = (ulong)imageReader.ReadByte();
            alphaMask += (ulong)imageReader.ReadByte() << 8;
            alphaMask += (ulong)imageReader.ReadByte() << 16;
            alphaMask += (ulong)imageReader.ReadByte() << 24;
            alphaMask += (ulong)imageReader.ReadByte() << 32;
            alphaMask += (ulong)imageReader.ReadByte() << 40;

            var c0 = imageReader.ReadUInt16();
            var c1 = imageReader.ReadUInt16();

            byte r0, g0, b0;
            byte r1, g1, b1;
            ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
            ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            var lookupTable = imageReader.ReadUInt32();

            for (var blockY = 0; blockY < 4; blockY++)
            {
                for (var blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 255;
                    var index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                    var alphaIndex = (uint)((alphaMask >> 3 * (4 * blockY + blockX)) & 0x07);
                    if (alphaIndex == 0)
                    {
                        a = alpha0;
                    }
                    else if (alphaIndex == 1)
                    {
                        a = alpha1;
                    }
                    else if (alpha0 > alpha1)
                    {
                        a = (byte)(((8 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 7);
                    }
                    else if (alphaIndex == 6)
                    {
                        a = 0;
                    }
                    else if (alphaIndex == 7)
                    {
                        a = 0xff;
                    }
                    else
                    {
                        a = (byte)(((6 - alphaIndex) * alpha0 + (alphaIndex - 1) * alpha1) / 5);
                    }

                    switch (index)
                    {
                        case 0:
                            r = r0;
                            g = g0;
                            b = b0;
                            break;
                        case 1:
                            r = r1;
                            g = g1;
                            b = b1;
                            break;
                        case 2:
                            r = (byte)((2 * r0 + r1) / 3);
                            g = (byte)((2 * g0 + g1) / 3);
                            b = (byte)((2 * b0 + b1) / 3);
                            break;
                        case 3:
                            r = (byte)((r0 + 2 * r1) / 3);
                            g = (byte)((g0 + 2 * g1) / 3);
                            b = (byte)((b0 + 2 * b1) / 3);
                            break;
                    }

                    var px = (x << 2) + blockX;
                    var py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        var offset = ((py * width) + px) << 2;
                        imageData[offset] = r;
                        imageData[offset + 1] = g;
                        imageData[offset + 2] = b;
                        imageData[offset + 3] = a;
                    }
                }
            }
        }

        private static void ConvertRgb565ToRgb888(ushort color, out byte r, out byte g, out byte b)
        {
            var temp = (color >> 11) * 255 + 16;
            r = (byte)((temp / 32 + temp) / 32);
            temp = ((color & 0x07E0) >> 5) * 255 + 32;
            g = (byte)((temp / 64 + temp) / 64);
            temp = (color & 0x001F) * 255 + 16;
            b = (byte)((temp / 32 + temp) / 32);
        }
    }
}
