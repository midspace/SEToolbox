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
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;

    public static class DxtUtil
    {
        #region Internal Static Methods

        internal static byte[] DecompressDxt1(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
            {
                return DecompressDxt1(imageStream, width, height);
            }
        }

        internal static byte[] DecompressDxt1(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt1Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        internal static byte[] DecompressDxt3(byte[] imageData, int width, int height)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
            {
                return DecompressDxt3(imageStream, width, height);
            }
        }

        internal static byte[] DecompressDxt3(Stream imageStream, int width, int height)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        DecompressDxt3Block(imageReader, x, y, blockCountX, width, height, imageData);
                    }
                }
            }

            return imageData;
        }

        internal static byte[] DecompressDxt5(byte[] imageData, int width, int height, ImageTextureUtil.DXGI_FORMAT dxgiFormat)
        {
            using (MemoryStream imageStream = new MemoryStream(imageData))
            {
                return DecompressDxt5(imageStream, width, height, dxgiFormat);
            }
        }

        internal static byte[] DecompressDxt5(Stream imageStream, int width, int height, ImageTextureUtil.DXGI_FORMAT dxgiFormat)
        {
            byte[] imageData = new byte[width * height * 4];

            using (BinaryReader imageReader = new BinaryReader(imageStream))
            {
                int blockCountX = (width + 3) / 4;
                int blockCountY = (height + 3) / 4;

                for (int y = 0; y < blockCountY; y++)
                {
                    for (int x = 0; x < blockCountX; x++)
                    {
                        switch (dxgiFormat)
                        {
                            case ImageTextureUtil.DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                            case ImageTextureUtil.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                            case ImageTextureUtil.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                                DecompressBc7Block(imageReader, x, y, blockCountX, width, height, imageData, dxgiFormat);
                                break;
                            default:
                                DecompressDxt5Block(imageReader, x, y, blockCountX, width, height, imageData);
                                break;
                        }
                    }
                }
            }

            return imageData;
        }

        #endregion

        #region Private Static Methods

        private static void DecompressDxt1Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

            byte r0, g0, b0;
            byte r1, g1, b1;
            ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
            ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            uint lookupTable = imageReader.ReadUInt32();

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 255;
                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                    if (c0 > c1)
                    {
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
                    }
                    else
                    {
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
                                r = (byte)((r0 + r1) / 2);
                                g = (byte)((g0 + g1) / 2);
                                b = (byte)((b0 + b1) / 2);
                                break;
                            case 3:
                                r = 0;
                                g = 0;
                                b = 0;
                                a = 0;
                                break;
                        }
                    }

                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        imageData[offset] = r;
                        imageData[offset + 1] = g;
                        imageData[offset + 2] = b;
                        imageData[offset + 3] = a;
                    }
                }
            }
        }

        private static void DecompressDxt3Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            byte a0 = imageReader.ReadByte();
            byte a1 = imageReader.ReadByte();
            byte a2 = imageReader.ReadByte();
            byte a3 = imageReader.ReadByte();
            byte a4 = imageReader.ReadByte();
            byte a5 = imageReader.ReadByte();
            byte a6 = imageReader.ReadByte();
            byte a7 = imageReader.ReadByte();

            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

            byte r0, g0, b0;
            byte r1, g1, b1;
            ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
            ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            uint lookupTable = imageReader.ReadUInt32();

            int alphaIndex = 0;
            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 0;

                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                    switch (alphaIndex)
                    {
                        case 0:
                            a = (byte)((a0 & 0x0F) | ((a0 & 0x0F) << 4));
                            break;
                        case 1:
                            a = (byte)((a0 & 0xF0) | ((a0 & 0xF0) >> 4));
                            break;
                        case 2:
                            a = (byte)((a1 & 0x0F) | ((a1 & 0x0F) << 4));
                            break;
                        case 3:
                            a = (byte)((a1 & 0xF0) | ((a1 & 0xF0) >> 4));
                            break;
                        case 4:
                            a = (byte)((a2 & 0x0F) | ((a2 & 0x0F) << 4));
                            break;
                        case 5:
                            a = (byte)((a2 & 0xF0) | ((a2 & 0xF0) >> 4));
                            break;
                        case 6:
                            a = (byte)((a3 & 0x0F) | ((a3 & 0x0F) << 4));
                            break;
                        case 7:
                            a = (byte)((a3 & 0xF0) | ((a3 & 0xF0) >> 4));
                            break;
                        case 8:
                            a = (byte)((a4 & 0x0F) | ((a4 & 0x0F) << 4));
                            break;
                        case 9:
                            a = (byte)((a4 & 0xF0) | ((a4 & 0xF0) >> 4));
                            break;
                        case 10:
                            a = (byte)((a5 & 0x0F) | ((a5 & 0x0F) << 4));
                            break;
                        case 11:
                            a = (byte)((a5 & 0xF0) | ((a5 & 0xF0) >> 4));
                            break;
                        case 12:
                            a = (byte)((a6 & 0x0F) | ((a6 & 0x0F) << 4));
                            break;
                        case 13:
                            a = (byte)((a6 & 0xF0) | ((a6 & 0xF0) >> 4));
                            break;
                        case 14:
                            a = (byte)((a7 & 0x0F) | ((a7 & 0x0F) << 4));
                            break;
                        case 15:
                            a = (byte)((a7 & 0xF0) | ((a7 & 0xF0) >> 4));
                            break;
                    }
                    ++alphaIndex;

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

                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
                        imageData[offset] = r;
                        imageData[offset + 1] = g;
                        imageData[offset + 2] = b;
                        imageData[offset + 3] = a;
                    }
                }
            }
        }

        private static void DecompressDxt5Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
        {
            byte alpha0 = imageReader.ReadByte();
            byte alpha1 = imageReader.ReadByte();

            ulong alphaMask = imageReader.ReadByte();
            alphaMask += (ulong)imageReader.ReadByte() << 8;
            alphaMask += (ulong)imageReader.ReadByte() << 16;
            alphaMask += (ulong)imageReader.ReadByte() << 24;
            alphaMask += (ulong)imageReader.ReadByte() << 32;
            alphaMask += (ulong)imageReader.ReadByte() << 40;

            ushort c0 = imageReader.ReadUInt16();
            ushort c1 = imageReader.ReadUInt16();

            byte r0, g0, b0;
            byte r1, g1, b1;

            ConvertRgb565ToRgb888(c0, out r0, out g0, out b0);
            ConvertRgb565ToRgb888(c1, out r1, out g1, out b1);

            uint lookupTable = imageReader.ReadUInt32();

            for (int blockY = 0; blockY < 4; blockY++)
            {
                for (int blockX = 0; blockX < 4; blockX++)
                {
                    byte r = 0, g = 0, b = 0, a = 255;
                    uint index = (lookupTable >> 2 * (4 * blockY + blockX)) & 0x03;

                    uint alphaIndex = (uint)((alphaMask >> 3 * (4 * blockY + blockX)) & 0x07);
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

                    int px = (x << 2) + blockX;
                    int py = (y << 2) + blockY;
                    if ((px < width) && (py < height))
                    {
                        int offset = ((py * width) + px) << 2;
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
            int temp = (color >> 11) * 255 + 16;
            r = (byte)((temp / 32 + temp) / 32);
            temp = ((color & 0x07E0) >> 5) * 255 + 32;
            g = (byte)((temp / 64 + temp) / 64);
            temp = (color & 0x001F) * 255 + 16;
            b = (byte)((temp / 32 + temp) / 32);
        }

        #region BC7 helpers

        // Converted from the DirectXTex code.
        // Licensed under the MIT License.
        // http://go.microsoft.com/fwlink/?LinkId=248926

        // TODO: there are still some issues with the brightness of the returned image.
        // It is not caused by the premultiplied Alpha, as it persists with linear Alpha.
        // At this stage it looks like the SRGB, but the code I have doesn't produce the exact result, but it is very close.

        private static void DecompressBc7Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData, ImageTextureUtil.DXGI_FORMAT dxgiFormat)
        {
            assert(imageReader != null);

            byte[] buffer = imageReader.ReadBytes(16);
            m_uBits = new BitArray(buffer);

            assert(buffer.Length > 0);

            uint uFirst = 0;
            while (uFirst < 128 && !GetBit(ref uFirst)) { }
            byte uMode = (byte)(uFirst - 1);

            if (uMode < 8)
            {
                byte uPartitions = ms_aInfo[uMode].uPartitions;
                assert(uPartitions < BC7_MAX_REGIONS);

                byte uNumEndPts = (byte)((uPartitions + 1) << 1);
                byte uIndexPrec = ms_aInfo[uMode].uIndexPrec;
                byte uIndexPrec2 = ms_aInfo[uMode].uIndexPrec2;

                int uStartBit = uMode + 1;
                byte[] P = new byte[6];
                byte uShape = GetBits<byte>(ref uStartBit, ms_aInfo[uMode].uPartitionBits);
                assert(uShape < BC7_MAX_SHAPES);

                byte uRotation = GetBits<byte>(ref uStartBit, ms_aInfo[uMode].uRotationBits);
                assert(uRotation < 4);

                byte uIndexMode = GetBits<byte>(ref uStartBit, ms_aInfo[uMode].uIndexModeBits);
                assert(uIndexMode < 2);

                LDRColorA[] c = new LDRColorA[BC7_MAX_REGIONS << 1];
                for (int i = 0; i < BC7_MAX_REGIONS << 1; i++)
                    c[i] = new LDRColorA();
                LDRColorA RGBAPrec = ms_aInfo[uMode].RGBAPrec;
                LDRColorA RGBAPrecWithP = ms_aInfo[uMode].RGBAPrecWithP;

                assert(uNumEndPts <= (BC7_MAX_REGIONS << 1));

                // Red channel
                for (int i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.r > 128)
                    {
                        throw new Exception("BC7: Invalid block encountered during decoding");
                    }
                    c[i].r = GetBits<byte>(ref uStartBit, RGBAPrec.r);
                }

                // Green channel
                for (int i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.g > 128)
                    {
                        throw new Exception("BC7: Invalid block encountered during decoding");
                    }
                    c[i].g = GetBits<byte>(ref uStartBit, RGBAPrec.g);
                }

                // Blue channel
                for (int i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.b > 128)
                    {
                        throw new Exception("BC7: Invalid block encountered during decoding");
                    }

                    c[i].b = GetBits<byte>(ref uStartBit, RGBAPrec.b);
                }

                // Alpha channel
                for (int i = 0; i < uNumEndPts; i++)
                {
                    if (uStartBit + RGBAPrec.a > 128)
                    {
                        throw new Exception("BC7: Invalid block encountered during decoding");
                    }

                    c[i].a = RGBAPrec.a != 0 ? GetBits<byte>(ref uStartBit, RGBAPrec.a) : (byte)255;
                }

                // P-bits
                assert(ms_aInfo[uMode].uPBits <= 6);
                for (int i = 0; i < ms_aInfo[uMode].uPBits; i++)
                {
                    if (uStartBit > 127)
                    {
                        throw new Exception("BC7: Invalid block encountered during decoding");
                    }

                    //Debugger.Break(); // I'm not sure about this C++ conversion.
                    P[i] = GetBit(ref uStartBit) ? (byte)0 : (byte)1;
                }

                if (ms_aInfo[uMode].uPBits != 0)
                {
                    for (int i = 0; i < uNumEndPts; i++)
                    {
                        int pi = (i * ms_aInfo[uMode].uPBits) / uNumEndPts;
                        for (uint ch = 0; ch < BC7_NUM_CHANNELS; ch++)
                        {
                            if (RGBAPrec[ch] != RGBAPrecWithP[ch])
                            {
                                c[i][ch] = (byte)((c[i][ch] << 1) | P[pi]);
                            }
                        }
                    }
                }

                for (int i = 0; i < uNumEndPts; i++)
                {
                    c[i] = Unquantize(c[i], RGBAPrecWithP);
                }

                byte[] w1 = new byte[NUM_PIXELS_PER_BLOCK];
                byte[] w2 = new byte[NUM_PIXELS_PER_BLOCK];

                // read color indices
                for (int i = 0; i < NUM_PIXELS_PER_BLOCK; i++)
                {
                    uint uNumBits = (uint)(IsFixUpOffset(ms_aInfo[uMode].uPartitions, uShape, i) ? uIndexPrec - 1 : uIndexPrec);
                    if (uStartBit + uNumBits > 128)
                    {
                        throw new Exception("BC7: Invalid block encountered during decoding");
                    }
                    w1[i] = GetBits<byte>(ref uStartBit, (int)uNumBits);
                }

                // read alpha indices
                if (uIndexPrec2 > 0)
                {
                    for (int i = 0; i < NUM_PIXELS_PER_BLOCK; i++)
                    {
                        uint uNumBits = (uint)(i > 0 ? uIndexPrec2 : uIndexPrec2 - 1);
                        if (uStartBit + uNumBits > 128)
                        {
                            throw new Exception("BC7: Invalid block encountered during decoding");
                        }
                        w2[i] = GetBits<byte>(ref uStartBit, (int)uNumBits);
                    }
                }

                int blockSize = (int)Math.Sqrt(NUM_PIXELS_PER_BLOCK);

                for (int i = 0; i < NUM_PIXELS_PER_BLOCK; ++i)
                {
                    uint uRegion = g_aPartitionTable[uPartitions][uShape][i];
                    LDRColorA outPixel;
                    if (uIndexPrec2 == 0)
                    {
                        LDRColorA.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w1[i], w1[i], uIndexPrec, uIndexPrec, out outPixel);
                    }
                    else
                    {
                        if (uIndexMode == 0)
                        {
                            LDRColorA.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w1[i], w2[i], uIndexPrec, uIndexPrec2, out outPixel);
                        }
                        else
                        {
                            LDRColorA.Interpolate(c[uRegion << 1], c[(uRegion << 1) + 1], w2[i], w1[i], uIndexPrec2, uIndexPrec, out outPixel);
                        }
                    }

                    switch (uRotation)
                    {
                        case 1: { byte t = outPixel.r; outPixel.r = outPixel.a; outPixel.a = t; } break;
                        case 2: { byte t = outPixel.g; outPixel.g = outPixel.a; outPixel.a = t; } break;
                        case 3: { byte t = outPixel.b; outPixel.b = outPixel.a; outPixel.a = t; } break;
                    }

                    int dataX = (x * blockSize) + (i % blockSize);
                    int dataY = (y * blockSize) + (i / blockSize);
                    int dataOffset = (dataY * blockCountX * 4 + dataX) * 4;

                    if (outPixel.a != 0)
                    {
                        // final texture uses premultiplied alpha ????
                        //outPixel.r = (byte)((float)outPixel.r / outPixel.a * 255f);
                        //outPixel.g = (byte)((float)outPixel.g / outPixel.a * 255f);
                        //outPixel.b = (byte)((float)outPixel.b / outPixel.a * 255f);

                        // linear to pm
                        //outPixel.r = (byte)((float)outPixel.r * outPixel.a / 255f);
                        //outPixel.g = (byte)((float)outPixel.g * outPixel.a / 255f);
                        //outPixel.b = (byte)((float)outPixel.b * outPixel.a / 255f);
                    }

                    // TODO: SRGB check probably should be somewhere else.
                    if (dxgiFormat == ImageTextureUtil.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB)
                    {
                        float r = outPixel.r * (1f / 255f);
                        float g = outPixel.g * (1f / 255f);
                        float b = outPixel.b * (1f / 255f);

                        outPixel.r = (byte)Math.Round(D3DX_SRGB_to_FLOAT_inexact(r) * 255f, 0);
                        outPixel.g = (byte)Math.Round(D3DX_SRGB_to_FLOAT_inexact(g) * 255f, 0);
                        outPixel.b = (byte)Math.Round(D3DX_SRGB_to_FLOAT_inexact(b) * 255f, 0);

                        //outPixel.r = (byte)Math.Round(255f * D3DX_SRGB_to_FLOAT(outPixel.r), 0);
                        //outPixel.g = (byte)Math.Round(255f * D3DX_SRGB_to_FLOAT(outPixel.g), 0);
                        //outPixel.b = (byte)Math.Round(255f * D3DX_SRGB_to_FLOAT(outPixel.b), 0);
                    }

                    imageData[dataOffset] = outPixel.r;
                    imageData[dataOffset + 1] = outPixel.g;
                    imageData[dataOffset + 2] = outPixel.b;
                    imageData[dataOffset + 3] = outPixel.a;

                    //pOut[i] = HDRColorA(outPixel);
                }
            }
            else
            {
                throw new Exception("BC7: Reserved mode 8 encountered during decoding");
            }
        }

        private static BitArray m_uBits;

        const uint BC7_MAX_REGIONS = 3;
        const uint BC7_MAX_INDICES = 16;
        const int BC7_NUM_CHANNELS = 4;
        const int BC7_MAX_SHAPES = 64;
        const int BC67_WEIGHT_MAX = 64;
        const int BC67_WEIGHT_SHIFT = 6;
        const int BC67_WEIGHT_ROUND = 32;

        // Because these are used in SAL annotations, they need to remain macros rather than const values
        const int NUM_PIXELS_PER_BLOCK = 16;

        private static readonly ModeInfo[] ms_aInfo = new ModeInfo[]
        {
            /// Mode 0: Color only, 3 Subsets, RGBP 4441 (unique P-bit), 3-bit indecies, 16 partitions
            new ModeInfo (2, 4, 6, 0, 0, 3, 0, new LDRColorA(4,4,4,0), new LDRColorA(5,5,5,0)),
            
            /// Mode 1: Color only, 2 Subsets, RGBP 6661 (shared P-bit), 3-bit indecies, 64 partitions
            new ModeInfo (1, 6, 2, 0, 0, 3, 0, new LDRColorA(6,6,6,0), new LDRColorA(7,7,7,0)),

            /// Mode 2: Color only, 3 Subsets, RGB 555, 2-bit indecies, 64 partitions
            new ModeInfo(2, 6, 0, 0, 0, 2, 0, new LDRColorA(5,5,5,0), new LDRColorA(5,5,5,0)),

            /// Mode 3: Color only, 2 Subsets, RGBP 7771 (unique P-bit), 2-bits indecies, 64 partitions
            new ModeInfo(1, 6, 4, 0, 0, 2, 0, new LDRColorA(7,7,7,0), new LDRColorA(8,8,8,0)),

            /// Mode 4: Color w/ Separate Alpha, 1 Subset, RGB 555, A6, 16x2/16x3-bit indices, 2-bit rotation, 1-bit index selector
            new ModeInfo(0, 0, 0, 2, 1, 2, 3, new LDRColorA(5,5,5,6), new LDRColorA(5,5,5,6)),

            /// Mode 5: Color w/ Separate Alpha, 1 Subset, RGB 777, A8, 16x2/16x2-bit indices, 2-bit rotation
            new ModeInfo(0, 0, 0, 2, 0, 2, 2, new LDRColorA(7,7,7,8), new LDRColorA(7,7,7,8)),

            /// Mode 6: Color+Alpha, 1 Subset, RGBAP 77771 (unique P-bit), 16x4-bit indecies
            new ModeInfo(0, 0, 2, 0, 0, 4, 0, new LDRColorA(7,7,7,7), new LDRColorA(8,8,8,8)),

            /// Mode 7: Color+Alpha, 2 Subsets, RGBAP 55551 (unique P-bit), 2-bit indices, 64 partitions
            new ModeInfo(1, 6, 4, 0, 0, 2, 0, new LDRColorA(5,5,5,5), new LDRColorA(6,6,6,6))
        };

        // Partition, Shape, Fixup
        static readonly uint[][][] g_aFixUp = new uint[][][]
        {
            new uint[][]{   // No fix-ups for 1st subset for BC6H or BC7
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },
                new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 },new uint[]{ 0, 0, 0 }
            },

            new uint[][]{   // BC6H/BC7 Partition Set Fixups for 2 Subsets
                new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },
                new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },
                new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },
                new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },
                new uint[]{ 0,15, 0 },new uint[]{ 0, 2, 0 },new uint[]{ 0, 8, 0 },new uint[]{ 0, 2, 0 },
                new uint[]{ 0, 2, 0 },new uint[]{ 0, 8, 0 },new uint[]{ 0, 8, 0 },new uint[]{ 0,15, 0 },
                new uint[]{ 0, 2, 0 },new uint[]{ 0, 8, 0 },new uint[]{ 0, 2, 0 },new uint[]{ 0, 2, 0 },
                new uint[]{ 0, 8, 0 },new uint[]{ 0, 8, 0 },new uint[]{ 0, 2, 0 },new uint[]{ 0, 2, 0 },

                // BC7 Partition Set Fixups for 2 Subsets (second-half)
                new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0, 6, 0 },new uint[]{ 0, 8, 0 },
                new uint[]{ 0, 2, 0 },new uint[]{ 0, 8, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },
                new uint[]{ 0, 2, 0 },new uint[]{ 0, 8, 0 },new uint[]{ 0, 2, 0 },new uint[]{ 0, 2, 0 },
                new uint[]{ 0, 2, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0, 6, 0 },
                new uint[]{ 0, 6, 0 },new uint[]{ 0, 2, 0 },new uint[]{ 0, 6, 0 },new uint[]{ 0, 8, 0 },
                new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0, 2, 0 },new uint[]{ 0, 2, 0 },
                new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },new uint[]{ 0,15, 0 },
                new uint[]{ 0,15, 0 },new uint[]{ 0, 2, 0 },new uint[]{ 0, 2, 0 },new uint[]{ 0,15, 0 }
            },

            new uint[][]{   // BC7 Partition Set Fixups for 3 Subsets
                new uint[]{ 0, 3,15 },new uint[]{ 0, 3, 8 },new uint[]{ 0,15, 8 },new uint[]{ 0,15, 3 },
                new uint[]{ 0, 8,15 },new uint[]{ 0, 3,15 },new uint[]{ 0,15, 3 },new uint[]{ 0,15, 8 },
                new uint[]{ 0, 8,15 },new uint[]{ 0, 8,15 },new uint[]{ 0, 6,15 },new uint[]{ 0, 6,15 },
                new uint[]{ 0, 6,15 },new uint[]{ 0, 5,15 },new uint[]{ 0, 3,15 },new uint[]{ 0, 3, 8 },
                new uint[]{ 0, 3,15 },new uint[]{ 0, 3, 8 },new uint[]{ 0, 8,15 },new uint[]{ 0,15, 3 },
                new uint[]{ 0, 3,15 },new uint[]{ 0, 3, 8 },new uint[]{ 0, 6,15 },new uint[]{ 0,10, 8 },
                new uint[]{ 0, 5, 3 },new uint[]{ 0, 8,15 },new uint[]{ 0, 8, 6 },new uint[]{ 0, 6,10 },
                new uint[]{ 0, 8,15 },new uint[]{ 0, 5,15 },new uint[]{ 0,15,10 },new uint[]{ 0,15, 8 },
                new uint[]{ 0, 8,15 },new uint[]{ 0,15, 3 },new uint[]{ 0, 3,15 },new uint[]{ 0, 5,10 },
                new uint[]{ 0, 6,10 },new uint[]{ 0,10, 8 },new uint[]{ 0, 8, 9 },new uint[]{ 0,15,10 },
                new uint[]{ 0,15, 6 },new uint[]{ 0, 3,15 },new uint[]{ 0,15, 8 },new uint[]{ 0, 5,15 },
                new uint[]{ 0,15, 3 },new uint[]{ 0,15, 6 },new uint[]{ 0,15, 6 },new uint[]{ 0,15, 8 },
                new uint[]{ 0, 3,15 },new uint[]{ 0,15, 3 },new uint[]{ 0, 5,15 },new uint[]{ 0, 5,15 },
                new uint[]{ 0, 5,15 },new uint[]{ 0, 8,15 },new uint[]{ 0, 5,15 },new uint[]{ 0,10,15 },
                new uint[]{ 0, 5,15 },new uint[]{ 0,10,15 },new uint[]{ 0, 8,15 },new uint[]{ 0,13,15 },
                new uint[]{ 0,15, 3 },new uint[]{ 0,12,15 },new uint[]{ 0, 3,15 },new uint[]{ 0, 3, 8 }
            }
        };

        // Partition, Shape, Pixel (index into 4x4 block)
        static readonly uint[][][] g_aPartitionTable = new uint[][][]
        {
            new uint[][]{   // 1 Region case has no subsets (all 0)
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },

            new uint[][]{   // BC6H/BC7 Partition Set for 2 Subsets
                new uint[]{ 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1 }, // Shape 0
                new uint[]{ 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 }, // Shape 1
                new uint[]{ 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1 }, // Shape 2
                new uint[]{ 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1 }, // Shape 3
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1 }, // Shape 4
                new uint[]{ 0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, // Shape 5
                new uint[]{ 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, // Shape 6
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1 }, // Shape 7
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1 }, // Shape 8
                new uint[]{ 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 9
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1 }, // Shape 10
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1 }, // Shape 11
                new uint[]{ 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 12
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 13
                new uint[]{ 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 14
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1 }, // Shape 15
                new uint[]{ 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1 }, // Shape 16
                new uint[]{ 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 }, // Shape 17
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0 }, // Shape 18
                new uint[]{ 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0 }, // Shape 19
                new uint[]{ 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 }, // Shape 20
                new uint[]{ 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0 }, // Shape 21
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0 }, // Shape 22
                new uint[]{ 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1 }, // Shape 23
                new uint[]{ 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0 }, // Shape 24
                new uint[]{ 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0 }, // Shape 25
                new uint[]{ 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0 }, // Shape 26
                new uint[]{ 0, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0 }, // Shape 27
                new uint[]{ 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0 }, // Shape 28
                new uint[]{ 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 }, // Shape 29
                new uint[]{ 0, 1, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0 }, // Shape 30
                new uint[]{ 0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0 }, // Shape 31

                            // BC7 Partition Set for 2 Subsets (second-half)
                new uint[]{ 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 }, // Shape 32
                new uint[]{ 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1 }, // Shape 33
                new uint[]{ 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0 }, // Shape 34
                new uint[]{ 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0 }, // Shape 35
                new uint[]{ 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0 }, // Shape 36
                new uint[]{ 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0 }, // Shape 37
                new uint[]{ 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1 }, // Shape 38
                new uint[]{ 0, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1 }, // Shape 39
                new uint[]{ 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0 }, // Shape 40
                new uint[]{ 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0 }, // Shape 41
                new uint[]{ 0, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0 }, // Shape 42
                new uint[]{ 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0 }, // Shape 43
                new uint[]{ 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0 }, // Shape 44
                new uint[]{ 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1 }, // Shape 45
                new uint[]{ 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1 }, // Shape 46
                new uint[]{ 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0 }, // Shape 47
                new uint[]{ 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0 }, // Shape 48
                new uint[]{ 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0 }, // Shape 49
                new uint[]{ 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0 }, // Shape 50
                new uint[]{ 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0 }, // Shape 51
                new uint[]{ 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1 }, // Shape 52
                new uint[]{ 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1 }, // Shape 53
                new uint[]{ 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0 }, // Shape 54
                new uint[]{ 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 0 }, // Shape 55
                new uint[]{ 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 1 }, // Shape 56
                new uint[]{ 0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1 }, // Shape 57
                new uint[]{ 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1 }, // Shape 58
                new uint[]{ 0, 0, 0, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1 }, // Shape 59
                new uint[]{ 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1 }, // Shape 60
                new uint[]{ 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 }, // Shape 61
                new uint[]{ 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0 }, // Shape 62
                new uint[]{ 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1 }  // Shape 63
            },

            new uint[][]{   // BC7 Partition Set for 3 Subsets
                new uint[]{ 0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 1, 2, 2, 2, 2 }, // Shape 0
                new uint[]{ 0, 0, 0, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 2, 1 }, // Shape 1
                new uint[]{ 0, 0, 0, 0, 2, 0, 0, 1, 2, 2, 1, 1, 2, 2, 1, 1 }, // Shape 2
                new uint[]{ 0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 1, 0, 1, 1, 1 }, // Shape 3
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2 }, // Shape 4
                new uint[]{ 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 2, 2 }, // Shape 5
                new uint[]{ 0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1 }, // Shape 6
                new uint[]{ 0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1 }, // Shape 7
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2 }, // Shape 8
                new uint[]{ 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2 }, // Shape 9
                new uint[]{ 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 10
                new uint[]{ 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2 }, // Shape 11
                new uint[]{ 0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2, 0, 1, 1, 2 }, // Shape 12
                new uint[]{ 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 2 }, // Shape 13
                new uint[]{ 0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2, 1, 2, 2, 2 }, // Shape 14
                new uint[]{ 0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0, 2, 2, 2, 0 }, // Shape 15
                new uint[]{ 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1, 2, 1, 1, 2, 2 }, // Shape 16
                new uint[]{ 0, 1, 1, 1, 0, 0, 1, 1, 2, 0, 0, 1, 2, 2, 0, 0 }, // Shape 17
                new uint[]{ 0, 0, 0, 0, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2 }, // Shape 18
                new uint[]{ 0, 0, 2, 2, 0, 0, 2, 2, 0, 0, 2, 2, 1, 1, 1, 1 }, // Shape 19
                new uint[]{ 0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2 }, // Shape 20
                new uint[]{ 0, 0, 0, 1, 0, 0, 0, 1, 2, 2, 2, 1, 2, 2, 2, 1 }, // Shape 21
                new uint[]{ 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2 }, // Shape 22
                new uint[]{ 0, 0, 0, 0, 1, 1, 0, 0, 2, 2, 1, 0, 2, 2, 1, 0 }, // Shape 23
                new uint[]{ 0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1, 0, 0, 0, 0 }, // Shape 24
                new uint[]{ 0, 0, 1, 2, 0, 0, 1, 2, 1, 1, 2, 2, 2, 2, 2, 2 }, // Shape 25
                new uint[]{ 0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1, 0, 1, 1, 0 }, // Shape 26
                new uint[]{ 0, 0, 0, 0, 0, 1, 1, 0, 1, 2, 2, 1, 1, 2, 2, 1 }, // Shape 27
                new uint[]{ 0, 0, 2, 2, 1, 1, 0, 2, 1, 1, 0, 2, 0, 0, 2, 2 }, // Shape 28
                new uint[]{ 0, 1, 1, 0, 0, 1, 1, 0, 2, 0, 0, 2, 2, 2, 2, 2 }, // Shape 29
                new uint[]{ 0, 0, 1, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 0, 1, 1 }, // Shape 30
                new uint[]{ 0, 0, 0, 0, 2, 0, 0, 0, 2, 2, 1, 1, 2, 2, 2, 1 }, // Shape 31
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 2, 2, 2 }, // Shape 32
                new uint[]{ 0, 2, 2, 2, 0, 0, 2, 2, 0, 0, 1, 2, 0, 0, 1, 1 }, // Shape 33
                new uint[]{ 0, 0, 1, 1, 0, 0, 1, 2, 0, 0, 2, 2, 0, 2, 2, 2 }, // Shape 34
                new uint[]{ 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 0 }, // Shape 35
                new uint[]{ 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0 }, // Shape 36
                new uint[]{ 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0 }, // Shape 37
                new uint[]{ 0, 1, 2, 0, 2, 0, 1, 2, 1, 2, 0, 1, 0, 1, 2, 0 }, // Shape 38
                new uint[]{ 0, 0, 1, 1, 2, 2, 0, 0, 1, 1, 2, 2, 0, 0, 1, 1 }, // Shape 39
                new uint[]{ 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 0, 0, 0, 0, 1, 1 }, // Shape 40
                new uint[]{ 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 41
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1 }, // Shape 42
                new uint[]{ 0, 0, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2, 1, 1, 2, 2 }, // Shape 43
                new uint[]{ 0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 2, 2, 0, 0, 1, 1 }, // Shape 44
                new uint[]{ 0, 2, 2, 0, 1, 2, 2, 1, 0, 2, 2, 0, 1, 2, 2, 1 }, // Shape 45
                new uint[]{ 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 0, 1 }, // Shape 46
                new uint[]{ 0, 0, 0, 0, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1 }, // Shape 47
                new uint[]{ 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2 }, // Shape 48
                new uint[]{ 0, 2, 2, 2, 0, 1, 1, 1, 0, 2, 2, 2, 0, 1, 1, 1 }, // Shape 49
                new uint[]{ 0, 0, 0, 2, 1, 1, 1, 2, 0, 0, 0, 2, 1, 1, 1, 2 }, // Shape 50
                new uint[]{ 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2, 2, 1, 1, 2 }, // Shape 51
                new uint[]{ 0, 2, 2, 2, 0, 1, 1, 1, 0, 1, 1, 1, 0, 2, 2, 2 }, // Shape 52
                new uint[]{ 0, 0, 0, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2 }, // Shape 53
                new uint[]{ 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2 }, // Shape 54
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2, 2, 1, 1, 2 }, // Shape 55
                new uint[]{ 0, 1, 1, 0, 0, 1, 1, 0, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 56
                new uint[]{ 0, 0, 2, 2, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 2, 2 }, // Shape 57
                new uint[]{ 0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 2, 2, 0, 0, 2, 2 }, // Shape 58
                new uint[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1, 1, 2 }, // Shape 59
                new uint[]{ 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 1 }, // Shape 60
                new uint[]{ 0, 2, 2, 2, 1, 2, 2, 2, 0, 2, 2, 2, 1, 2, 2, 2 }, // Shape 61
                new uint[]{ 0, 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 }, // Shape 62
                new uint[]{ 0, 1, 1, 1, 2, 0, 1, 1, 2, 2, 0, 1, 2, 2, 2, 0 }  // Shape 63
            }
        };

        static readonly int[] g_aWeights2 = { 0, 21, 43, 64 };
        static readonly int[] g_aWeights3 = { 0, 9, 18, 27, 37, 46, 55, 64 };
        static readonly int[] g_aWeights4 = { 0, 4, 9, 13, 17, 21, 26, 30, 34, 38, 43, 47, 51, 55, 60, 64 };

        private static void assert(bool condition)
        {
            if (!condition)
            {
                Debugger.Break();
            }
        }


        private static bool GetBit(ref uint uStartBit)
        {
            bool ret = m_uBits[(int)uStartBit];
            uStartBit++;
            return ret;
        }

        private static bool GetBit(ref int uStartBit)
        {
            bool ret = m_uBits[uStartBit];
            uStartBit++;
            return ret;
        }

        private static T GetBits<T>(ref int uStartBit, int uNumBits)
        {
            if (uNumBits == 0)
                return default(T);

            BitArray newBits = new BitArray(uNumBits);

            for (int i = 0; i < uNumBits; i++)
            {
                newBits[i] = m_uBits[uStartBit];
                uStartBit++;
            }

            if (uNumBits > 32)
            {
                Debugger.Break();
                // TODO: this exceeds the size of the uint
            }

            int[] theIntArray = new int[(newBits.Length + 31) / 32];
            new BitArray(newBits).CopyTo(theIntArray, 0);

            if (theIntArray[0] > 255)
            {
                Debugger.Break();
                // TODO: might be a converstion issue with int>uint
            }

            return (T)Convert.ChangeType(theIntArray[0], typeof(T));
        }

        class LDRColorA
        {
            public byte r, g, b, a;
            public LDRColorA() { }
            public LDRColorA(byte rColor, byte gColor, byte bColor, byte aColor)
            {
                r = rColor;
                g = gColor;
                b = bColor;
                a = aColor;
            }

            public byte this[uint uElement]
            {
                get
                {
                    switch (uElement)
                    {
                        case 0: return r;
                        case 1: return g;
                        case 2: return b;
                        case 3: return a;
                        default: assert(false); return r;
                    }
                }
                set
                {
                    switch (uElement)
                    {
                        case 0: r = value; break;
                        case 1: g = value; break;
                        case 2: b = value; break;
                        case 3: a = value; break;
                        default: Debugger.Break(); break;
                    }
                }
            }

            //static void InterpolateRGB(_In_ const LDRColorA& c0, _In_ const LDRColorA& c1, _In_ size_t wc, _In_ _In_range_(2, 4) size_t wcprec, _Out_ LDRColorA& out)
            static void InterpolateRGB(LDRColorA c0, LDRColorA c1, int wc, int wcprec, ref LDRColorA outVal)
            {
                int[] aWeights;
                switch (wcprec)
                {
                    case 2: aWeights = g_aWeights2; assert(wc < 4); break;
                    case 3: aWeights = g_aWeights3; assert(wc < 8); break;
                    case 4: aWeights = g_aWeights4; assert(wc < 16); break;
                    default: assert(false); outVal.r = outVal.g = outVal.b = 0; return;
                }

                if (c1.r > 60)
                {
                }

                outVal.r = (byte)((c0.r * (BC67_WEIGHT_MAX - aWeights[wc]) + (c1.r) * (aWeights[wc]) + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
                outVal.g = (byte)((c0.g * (BC67_WEIGHT_MAX - aWeights[wc]) + (c1.g) * (aWeights[wc]) + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
                outVal.b = (byte)((c0.b * (BC67_WEIGHT_MAX - aWeights[wc]) + (c1.b) * (aWeights[wc]) + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
            }

            // static void InterpolateA(_In_ const LDRColorA& c0, _In_ const LDRColorA& c1, _In_ size_t wa, _In_range_(2, 4) _In_ size_t waprec, _Out_ LDRColorA& out)
            static void InterpolateA(LDRColorA c0, LDRColorA c1, int wa, int waprec, ref LDRColorA outVal)
            {
                int[] aWeights;
                switch (waprec)
                {
                    case 2: aWeights = g_aWeights2; assert(wa < 4); break;
                    case 3: aWeights = g_aWeights3; assert(wa < 8); break;
                    case 4: aWeights = g_aWeights4; assert(wa < 16); break;
                    default: assert(false); outVal.a = 0; return;
                }
                outVal.a = (byte)((c0.a * (BC67_WEIGHT_MAX - aWeights[wa]) + (c1.a) * (aWeights[wa]) + BC67_WEIGHT_ROUND) >> BC67_WEIGHT_SHIFT);
            }


            // static void Interpolate(_In_ const LDRColorA& c0, _In_ const LDRColorA& c1, _In_ size_t wc, _In_ size_t wa, _In_ _In_range_(2, 4) size_t wcprec, _In_ _In_range_(2, 4) size_t waprec, _Out_ LDRColorA& out)
            public static void Interpolate(LDRColorA c0, LDRColorA c1, int wc, int wa, int wcprec, int waprec, out LDRColorA outVal)
            {
                outVal = new LDRColorA();
                InterpolateRGB(c0, c1, wc, wcprec, ref outVal);
                InterpolateA(c0, c1, wa, waprec, ref outVal);
            }
        }

        class ModeInfo
        {
            public byte uPartitions;
            public byte uPartitionBits;
            public byte uPBits;
            public byte uRotationBits;
            public byte uIndexModeBits;
            public byte uIndexPrec;
            public byte uIndexPrec2;
            public LDRColorA RGBAPrec;
            public LDRColorA RGBAPrecWithP;

            public ModeInfo(byte partitions, byte partitionBits, byte pBits, byte rotationBits, byte indexModeBits, byte indexPrec, byte indexPrec2, LDRColorA rgbaPrec, LDRColorA rgbaPrecWithP)
            {
                uPartitions = partitions;
                uPartitionBits = partitionBits;
                uPBits = pBits;
                uRotationBits = rotationBits;
                uIndexModeBits = indexModeBits;
                uIndexPrec = indexPrec;
                uIndexPrec2 = indexPrec2;
                RGBAPrec = rgbaPrec;
                RGBAPrecWithP = rgbaPrecWithP;
            }
        };


        static byte Unquantize(byte comp, int uPrec)
        {
            assert(0 < uPrec && uPrec <= 8);
            comp = (byte)(comp << (8 - uPrec));
            return (byte)(comp | (comp >> uPrec));
        }

        static LDRColorA Unquantize(LDRColorA c, LDRColorA RGBAPrec)
        {
            LDRColorA q = new LDRColorA();
            q.r = Unquantize(c.r, RGBAPrec.r);
            q.g = Unquantize(c.g, RGBAPrec.g);
            q.b = Unquantize(c.b, RGBAPrec.b);
            q.a = RGBAPrec.a > 0 ? Unquantize(c.a, RGBAPrec.a) : (byte)255;
            return q;
        }

        // inline bool IsFixUpOffset(_In_range_(0, 2) size_t uPartitions, _In_range_(0, 63) size_t uShape, _In_range_(0, 15) size_t uOffset)
        static bool IsFixUpOffset(uint uPartitions, uint uShape, int uOffset)
        {
            assert(uPartitions < 3 && uShape < 64 && uOffset < 16);
            for (int p = 0; p <= uPartitions; p++)
            {
                if (uOffset == g_aFixUp[uPartitions][uShape][p])
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ff728749(v=vs.85).aspx
        // D3DX_DXGIFormatConvert.inl
        private static float D3DX_SRGB_to_FLOAT_inexact(float srgb)
        {
            float linear;
            if (srgb <= 0.04045f)
                linear = srgb / 12.92f;
            else
                linear = (float)Math.Pow((srgb + 0.055f) / 1.055f, 2.4f);
            return linear;
        }

        // https://gamedevdaily.io/the-srgb-learning-curve-773b7f68cf7a
        private static float D3DX_FLOAT_to_SRGB(float linear)
        {
            float srgb;
            if (linear < 0.0031308f)
                srgb = linear * 12.92f;
            else
                srgb = 1.055f * (float)Math.Pow(linear, 1.0f / 2.4f) - 0.055f;
            return srgb;
        }

        static readonly uint[] D3DX_SRGBTable =
        {
            0x00000000,0x399f22b4,0x3a1f22b4,0x3a6eb40e,0x3a9f22b4,0x3ac6eb61,0x3aeeb40e,0x3b0b3e5d,
            0x3b1f22b4,0x3b33070b,0x3b46eb61,0x3b5b518d,0x3b70f18d,0x3b83e1c6,0x3b8fe616,0x3b9c87fd,
            0x3ba9c9b7,0x3bb7ad6f,0x3bc63549,0x3bd56361,0x3be539c1,0x3bf5ba70,0x3c0373b5,0x3c0c6152,
            0x3c15a703,0x3c1f45be,0x3c293e6b,0x3c3391f7,0x3c3e4149,0x3c494d43,0x3c54b6c7,0x3c607eb1,
            0x3c6ca5df,0x3c792d22,0x3c830aa8,0x3c89af9f,0x3c9085db,0x3c978dc5,0x3c9ec7c2,0x3ca63433,
            0x3cadd37d,0x3cb5a601,0x3cbdac20,0x3cc5e639,0x3cce54ab,0x3cd6f7d5,0x3cdfd010,0x3ce8ddb9,
            0x3cf2212c,0x3cfb9ac1,0x3d02a569,0x3d0798dc,0x3d0ca7e6,0x3d11d2af,0x3d171963,0x3d1c7c2e,
            0x3d21fb3c,0x3d2796b2,0x3d2d4ebb,0x3d332380,0x3d39152b,0x3d3f23e3,0x3d454fd1,0x3d4b991c,
            0x3d51ffef,0x3d58846a,0x3d5f26b7,0x3d65e6fe,0x3d6cc564,0x3d73c20f,0x3d7add29,0x3d810b67,
            0x3d84b795,0x3d887330,0x3d8c3e4a,0x3d9018f6,0x3d940345,0x3d97fd4a,0x3d9c0716,0x3da020bb,
            0x3da44a4b,0x3da883d7,0x3daccd70,0x3db12728,0x3db59112,0x3dba0b3b,0x3dbe95b5,0x3dc33092,
            0x3dc7dbe2,0x3dcc97b6,0x3dd1641f,0x3dd6412c,0x3ddb2eef,0x3de02d77,0x3de53cd5,0x3dea5d19,
            0x3def8e52,0x3df4d091,0x3dfa23e8,0x3dff8861,0x3e027f07,0x3e054280,0x3e080ea3,0x3e0ae378,
            0x3e0dc105,0x3e10a754,0x3e13966b,0x3e168e52,0x3e198f10,0x3e1c98ad,0x3e1fab30,0x3e22c6a3,
            0x3e25eb09,0x3e29186c,0x3e2c4ed0,0x3e2f8e41,0x3e32d6c4,0x3e362861,0x3e39831e,0x3e3ce703,
            0x3e405416,0x3e43ca5f,0x3e4749e4,0x3e4ad2ae,0x3e4e64c2,0x3e520027,0x3e55a4e6,0x3e595303,
            0x3e5d0a8b,0x3e60cb7c,0x3e6495e0,0x3e6869bf,0x3e6c4720,0x3e702e0c,0x3e741e84,0x3e781890,
            0x3e7c1c38,0x3e8014c2,0x3e82203c,0x3e84308d,0x3e8645ba,0x3e885fc5,0x3e8a7eb2,0x3e8ca283,
            0x3e8ecb3d,0x3e90f8e1,0x3e932b74,0x3e9562f8,0x3e979f71,0x3e99e0e2,0x3e9c274e,0x3e9e72b7,
            0x3ea0c322,0x3ea31892,0x3ea57308,0x3ea7d289,0x3eaa3718,0x3eaca0b7,0x3eaf0f69,0x3eb18333,
            0x3eb3fc18,0x3eb67a18,0x3eb8fd37,0x3ebb8579,0x3ebe12e1,0x3ec0a571,0x3ec33d2d,0x3ec5da17,
            0x3ec87c33,0x3ecb2383,0x3ecdd00b,0x3ed081cd,0x3ed338cc,0x3ed5f50b,0x3ed8b68d,0x3edb7d54,
            0x3ede4965,0x3ee11ac1,0x3ee3f16b,0x3ee6cd67,0x3ee9aeb6,0x3eec955d,0x3eef815d,0x3ef272ba,
            0x3ef56976,0x3ef86594,0x3efb6717,0x3efe6e02,0x3f00bd2d,0x3f02460e,0x3f03d1a7,0x3f055ff9,
            0x3f06f106,0x3f0884cf,0x3f0a1b56,0x3f0bb49b,0x3f0d50a0,0x3f0eef67,0x3f1090f1,0x3f12353e,
            0x3f13dc51,0x3f15862b,0x3f1732cd,0x3f18e239,0x3f1a946f,0x3f1c4971,0x3f1e0141,0x3f1fbbdf,
            0x3f21794e,0x3f23398e,0x3f24fca0,0x3f26c286,0x3f288b41,0x3f2a56d3,0x3f2c253d,0x3f2df680,
            0x3f2fca9e,0x3f31a197,0x3f337b6c,0x3f355820,0x3f3737b3,0x3f391a26,0x3f3aff7c,0x3f3ce7b5,
            0x3f3ed2d2,0x3f40c0d4,0x3f42b1be,0x3f44a590,0x3f469c4b,0x3f4895f1,0x3f4a9282,0x3f4c9201,
            0x3f4e946e,0x3f5099cb,0x3f52a218,0x3f54ad57,0x3f56bb8a,0x3f58ccb0,0x3f5ae0cd,0x3f5cf7e0,
            0x3f5f11ec,0x3f612eee,0x3f634eef,0x3f6571e9,0x3f6797e3,0x3f69c0d6,0x3f6beccd,0x3f6e1bbf,
            0x3f704db8,0x3f7282af,0x3f74baae,0x3f76f5ae,0x3f7933b9,0x3f7b74c6,0x3f7db8e0,0x3f800000
        };

        static float D3DX_SRGB_to_FLOAT(uint val)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(D3DX_SRGBTable[val]), 0);
            //return (float)D3DX_SRGBTable[val];
        }

        #endregion
    }
}
