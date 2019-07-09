namespace SEToolbox.ImageLibrary
{
    using SEToolbox.ImageLibrary.Effects;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Media;

    /// <summary>
    /// A cobbled togeather utility for reading the Space Engineers texture assets which are generally defined as BC3_UNorm textures in .dds files.
    /// </summary>
    public static class ImageTextureUtil
    {
        // http://msdn.microsoft.com/en-us/library/windows/desktop/bb943991(v=vs.85).aspx#File_Layout1
        // http://msdn.microsoft.com/en-us/library/windows/apps/jj651550.aspx

        #region const

        /// <summary>
        /// PixelFormat flags.
        /// </summary>
        [Flags]
        public enum PixelFormatFlags
        {
            FourCC = 0x00000004, // DDPF_FOURCC
            Rgb = 0x00000040, // DDPF_RGB
            Rgba = 0x00000041, // DDPF_RGB | DDPF_ALPHAPIXELS
            Luminance = 0x00020000, // DDPF_LUMINANCE
            LuminanceAlpha = 0x00020001, // DDPF_LUMINANCE | DDPF_ALPHAPIXELS
            Alpha = 0x00000002, // DDPF_ALPHA
            Pal8 = 0x00000020, // DDPF_PALETTEINDEXED8            
        }

        #endregion

        #region enums

        [Flags]
        internal enum DDSCAPS2 : uint
        {
            DDSCAPS2_CUBEMAP = 0x200,
            DDSCAPS2_CUBEMAP_POSITIVEX = 0x400,
            DDSCAPS2_CUBEMAP_NEGATIVEX = 0x800,
            DDSCAPS2_CUBEMAP_POSITIVEY = 0x1000,
            DDSCAPS2_CUBEMAP_NEGATIVEY = 0x2000,
            DDSCAPS2_CUBEMAP_POSITIVEZ = 0x4000,
            DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x8000,
            DDSCAPS2_VOLUME = 0x200000,
        }

        internal enum DDSType
        {
            DDS_A8R8G8B8 = 0,
            DDS_A1R5G5B5 = 1,
            DDS_A4R4G4B4 = 2,
            DDS_R8G8B8 = 3,
            DDS_R5G6B5 = 4,
            DDS_DXT1 = 5,
            DDS_DXT2 = 6,
            DDS_DXT3 = 7,
            DDS_DXT4 = 8,
            DDS_DXT5 = 9,
            DDS_RXGB = 10,
            DDS_ATI2 = 11,
            DDS_UNKNOWN
        }

        internal enum DDS_FOURCC : uint
        {
            DXT1 = 0x31545844,
            DXT3 = 0x33545844,
            DXT5 = 0x35545844,
            BC4U = 0x55344342,
            BC4S = 0x53344342,
            ATI2 = 0x32495441,
            BC5S = 0x53354342,
            RGBG = 0x47424752,
            GRGB = 0x42475247,
            DXT2 = 0x32545844,
            DXT4 = 0x34545844,
            UYVY = 0x59565955,
            YUY2 = 0x32595559,
            DX10 = 0x30315844,
        };

        internal enum DXGI_FORMAT : uint
        {
            DXGI_FORMAT_UNKNOWN = 0,
            DXGI_FORMAT_R32G32B32A32_TYPELESS = 1,
            DXGI_FORMAT_R32G32B32A32_FLOAT = 2,
            DXGI_FORMAT_R32G32B32A32_UINT = 3,
            DXGI_FORMAT_R32G32B32A32_SINT = 4,
            DXGI_FORMAT_R32G32B32_TYPELESS = 5,
            DXGI_FORMAT_R32G32B32_FLOAT = 6,
            DXGI_FORMAT_R32G32B32_UINT = 7,
            DXGI_FORMAT_R32G32B32_SINT = 8,
            DXGI_FORMAT_R16G16B16A16_TYPELESS = 9,
            DXGI_FORMAT_R16G16B16A16_FLOAT = 10,
            DXGI_FORMAT_R16G16B16A16_UNORM = 11,
            DXGI_FORMAT_R16G16B16A16_UINT = 12,
            DXGI_FORMAT_R16G16B16A16_SNORM = 13,
            DXGI_FORMAT_R16G16B16A16_SINT = 14,
            DXGI_FORMAT_R32G32_TYPELESS = 15,
            DXGI_FORMAT_R32G32_FLOAT = 16,
            DXGI_FORMAT_R32G32_UINT = 17,
            DXGI_FORMAT_R32G32_SINT = 18,
            DXGI_FORMAT_R32G8X24_TYPELESS = 19,
            DXGI_FORMAT_D32_FLOAT_S8X24_UINT = 20,
            DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS = 21,
            DXGI_FORMAT_X32_TYPELESS_G8X24_UINT = 22,
            DXGI_FORMAT_R10G10B10A2_TYPELESS = 23,
            DXGI_FORMAT_R10G10B10A2_UNORM = 24,
            DXGI_FORMAT_R10G10B10A2_UINT = 25,
            DXGI_FORMAT_R11G11B10_FLOAT = 26,
            DXGI_FORMAT_R8G8B8A8_TYPELESS = 27,
            DXGI_FORMAT_R8G8B8A8_UNORM = 28,
            DXGI_FORMAT_R8G8B8A8_UNORM_SRGB = 29,
            DXGI_FORMAT_R8G8B8A8_UINT = 30,
            DXGI_FORMAT_R8G8B8A8_SNORM = 31,
            DXGI_FORMAT_R8G8B8A8_SINT = 32,
            DXGI_FORMAT_R16G16_TYPELESS = 33,
            DXGI_FORMAT_R16G16_FLOAT = 34,
            DXGI_FORMAT_R16G16_UNORM = 35,
            DXGI_FORMAT_R16G16_UINT = 36,
            DXGI_FORMAT_R16G16_SNORM = 37,
            DXGI_FORMAT_R16G16_SINT = 38,
            DXGI_FORMAT_R32_TYPELESS = 39,
            DXGI_FORMAT_D32_FLOAT = 40,
            DXGI_FORMAT_R32_FLOAT = 41,
            DXGI_FORMAT_R32_UINT = 42,
            DXGI_FORMAT_R32_SINT = 43,
            DXGI_FORMAT_R24G8_TYPELESS = 44,
            DXGI_FORMAT_D24_UNORM_S8_UINT = 45,
            DXGI_FORMAT_R24_UNORM_X8_TYPELESS = 46,
            DXGI_FORMAT_X24_TYPELESS_G8_UINT = 47,
            DXGI_FORMAT_R8G8_TYPELESS = 48,
            DXGI_FORMAT_R8G8_UNORM = 49,
            DXGI_FORMAT_R8G8_UINT = 50,
            DXGI_FORMAT_R8G8_SNORM = 51,
            DXGI_FORMAT_R8G8_SINT = 52,
            DXGI_FORMAT_R16_TYPELESS = 53,
            DXGI_FORMAT_R16_FLOAT = 54,
            DXGI_FORMAT_D16_UNORM = 55,
            DXGI_FORMAT_R16_UNORM = 56,
            DXGI_FORMAT_R16_UINT = 57,
            DXGI_FORMAT_R16_SNORM = 58,
            DXGI_FORMAT_R16_SINT = 59,
            DXGI_FORMAT_R8_TYPELESS = 60,
            DXGI_FORMAT_R8_UNORM = 61,
            DXGI_FORMAT_R8_UINT = 62,
            DXGI_FORMAT_R8_SNORM = 63,
            DXGI_FORMAT_R8_SINT = 64,
            DXGI_FORMAT_A8_UNORM = 65,
            DXGI_FORMAT_R1_UNORM = 66,
            DXGI_FORMAT_R9G9B9E5_SHAREDEXP = 67,
            DXGI_FORMAT_R8G8_B8G8_UNORM = 68,
            DXGI_FORMAT_G8R8_G8B8_UNORM = 69,
            DXGI_FORMAT_BC1_TYPELESS = 70,
            DXGI_FORMAT_BC1_UNORM = 71,
            DXGI_FORMAT_BC1_UNORM_SRGB = 72,
            DXGI_FORMAT_BC2_TYPELESS = 73,
            DXGI_FORMAT_BC2_UNORM = 74,
            DXGI_FORMAT_BC2_UNORM_SRGB = 75,
            DXGI_FORMAT_BC3_TYPELESS = 76,
            DXGI_FORMAT_BC3_UNORM = 77,
            DXGI_FORMAT_BC3_UNORM_SRGB = 78,
            DXGI_FORMAT_BC4_TYPELESS = 79,
            DXGI_FORMAT_BC4_UNORM = 80,
            DXGI_FORMAT_BC4_SNORM = 81,
            DXGI_FORMAT_BC5_TYPELESS = 82,
            DXGI_FORMAT_BC5_UNORM = 83,
            DXGI_FORMAT_BC5_SNORM = 84,
            DXGI_FORMAT_B5G6R5_UNORM = 85,
            DXGI_FORMAT_B5G5R5A1_UNORM = 86,
            DXGI_FORMAT_B8G8R8A8_UNORM = 87,
            DXGI_FORMAT_B8G8R8X8_UNORM = 88,
            DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM = 89,
            DXGI_FORMAT_B8G8R8A8_TYPELESS = 90,
            DXGI_FORMAT_B8G8R8A8_UNORM_SRGB = 91,
            DXGI_FORMAT_B8G8R8X8_TYPELESS = 92,
            DXGI_FORMAT_B8G8R8X8_UNORM_SRGB = 93,
            DXGI_FORMAT_BC6H_TYPELESS = 94,
            DXGI_FORMAT_BC6H_UF16 = 95,
            DXGI_FORMAT_BC6H_SF16 = 96,
            DXGI_FORMAT_BC7_TYPELESS = 97,
            DXGI_FORMAT_BC7_UNORM = 98,
            DXGI_FORMAT_BC7_UNORM_SRGB = 99,
            DXGI_FORMAT_AYUV = 100,
            DXGI_FORMAT_Y410 = 101,
            DXGI_FORMAT_Y416 = 102,
            DXGI_FORMAT_NV12 = 103,
            DXGI_FORMAT_P010 = 104,
            DXGI_FORMAT_P016 = 105,
            DXGI_FORMAT_420_OPAQUE = 106,
            DXGI_FORMAT_YUY2 = 107,
            DXGI_FORMAT_Y210 = 108,
            DXGI_FORMAT_Y216 = 109,
            DXGI_FORMAT_NV11 = 110,
            DXGI_FORMAT_AI44 = 111,
            DXGI_FORMAT_IA44 = 112,
            DXGI_FORMAT_P8 = 113,
            DXGI_FORMAT_A8P8 = 114,
            DXGI_FORMAT_B4G4R4A4_UNORM = 115,
            //DXGI_FORMAT_FORCE_UINT = 0xffffffffUL
        };

        internal enum D3D10_RESOURCE_DIMENSION : uint
        {
            D3D10_RESOURCE_DIMENSION_UNKNOWN = 0,
            D3D10_RESOURCE_DIMENSION_BUFFER = 1,
            D3D10_RESOURCE_DIMENSION_TEXTURE1D = 2,
            D3D10_RESOURCE_DIMENSION_TEXTURE2D = 3,
            D3D10_RESOURCE_DIMENSION_TEXTURE3D = 4
        };

        #endregion

        #region structs

        [StructLayout(LayoutKind.Sequential)]
        internal struct DDS_PIXELFORMAT
        {
            internal UInt32 dwSize;
            internal UInt32 dwFlags;
            internal UInt32 dwFourCC;
            internal UInt32 dwRGBBitCount;
            internal UInt32 dwRBitMask;
            internal UInt32 dwGBitMask;
            internal UInt32 dwBBitMask;
            internal UInt32 dwABitMask;
        };

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct DDS_HEADER
        {
            internal UInt32 dwSize;
            internal UInt32 dwFlags;
            internal UInt32 dwHeight;
            internal UInt32 dwWidth;
            internal UInt32 dwPitchOrLinearSize;
            internal UInt32 dwDepth;
            internal UInt32 dwMipMapCount;
            internal fixed UInt32 dwReserved1[11];
            internal DDS_PIXELFORMAT ddspf;
            internal UInt32 dwCaps;
            internal UInt32 dwCaps2;
            internal UInt32 dwCaps3;
            internal UInt32 dwCaps4;
            internal UInt32 dwReserved2;

            /// <summary>
            /// The size of the <see cref="DDS_HEADER"/> type, in bytes.
            /// </summary>
            public static readonly int SizeInBytes = Marshal.SizeOf(typeof(DDS_HEADER));
        };

        [StructLayout(LayoutKind.Sequential)]
        internal struct DDS_HEADER_DXT10
        {
            internal DXGI_FORMAT dxgiFormat;
            internal D3D10_RESOURCE_DIMENSION resourceDimension;
            internal uint miscFlag;
            internal uint arraySize;
            internal uint miscFlags2;

            /// <summary>
            /// The size of the <see cref="DDS_HEADER"/> type, in bytes.
            /// </summary>
            public static readonly int SizeInBytes = Marshal.SizeOf(typeof(DDS_HEADER_DXT10));
        };

        #endregion

        #region MarshalTo

        private static T MarshalTo<T>(byte[] buffer)
        {
            IntPtr intPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, intPtr, buffer.Length);
            var record = (T)Marshal.PtrToStructure(intPtr, typeof(T));
            Marshal.FreeHGlobal(intPtr);
            return record;
        }

        #endregion

        #region CreateImage

        // For WPF display.

        public static ImageSource CreateImage(string filename, bool ignoreAlpha = false, IPixelEffect effect = null)
        {
            return CreateImage(filename, 0, -1, -1, ignoreAlpha, effect);
        }

        public static ImageSource CreateImage(string filename, int depthSlice, int width, int height, bool ignoreAlpha = false, IPixelEffect effect = null)
        {
            var extension = Path.GetExtension(filename).ToLower();

            if (extension == ".png")
            {
                // TODO: rescale the bitmap to specified width/height.
                try
                {
                    return ImageHelper.ConvertBitmapToBitmapImage((Bitmap)Image.FromFile(filename, true));
                }
                catch { return null; }
            }
            else if (extension == ".dds")
            {
                using (var stream = File.OpenRead(filename))
                {
                    return CreateImage(stream, depthSlice, width, height, ignoreAlpha, effect);
                }
            }

            return null;
        }

        public static ImageSource CreateImage(Stream stream, int depthSlice, int width, int height, bool ignoreAlpha = false, IPixelEffect effect = null)
        {
            if (stream == null)
                return null;

            uint fourCC = 0;
            DXGI_FORMAT dxgiFormat;
            var pixelChannel = ReadTextureFile(stream, depthSlice, ref width, ref height, out fourCC, out dxgiFormat);
            if (pixelChannel == null)
                return null;
            try
            {
                return DxtUtilTexture.DecompressTextureToImageSource(pixelChannel, width, height, ignoreAlpha, effect, fourCC, dxgiFormat);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region CreateBitmap

        // For export to Report.

        //public static Bitmap CreateBitmap(string filename, bool ignoreAlpha = false)
        //{
        //    using (var stream = File.OpenRead(filename))
        //    {
        //        return CreateBitmap(stream, filename, 0, -1, -1, ignoreAlpha);
        //    }
        //}

        public static Bitmap CreateBitmap(Stream stream, string filename, int depthSlice = 0, int width = -1, int height = -1, bool ignoreAlpha = false)
        {
            if (stream == null)
                return null;

            var extension = Path.GetExtension(filename)?.ToLower();

            if (extension == ".png")
            {
                // TODO: rescale the bitmap to specified width/height.
                return (Bitmap)Image.FromFile(filename, true);
            }
            if (extension == ".dds")
            {
                uint fourCC = 0;
                DXGI_FORMAT dxgiFormat;
                var pixelChannel = ReadTextureFile(stream, depthSlice, ref width, ref height, out fourCC, out dxgiFormat);
                if (pixelChannel == null)
                    return null;
                try
                {
                    return DxtUtilTexture.DecompressTextureToBitmap(pixelChannel, width, height, ignoreAlpha, fourCC, dxgiFormat);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public static string GetTextureToBase64(Stream stream, string filename, int width, int height, bool ignoreAlpha = false)
        {
            using (var bmp = CreateBitmap(stream, filename, 0, width, height, ignoreAlpha))
            {
                var converter = new ImageConverter();
                return Convert.ToBase64String((byte[])converter.ConvertTo(bmp, typeof(byte[])));
            }
        }

        #endregion

        #region ReadTextureFile

        private static byte[] ReadTextureFile(Stream stream, int depthSlice, ref int width, ref int height, out uint fourCC, out DXGI_FORMAT dxgiFormat)
        {
            dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;

            var reader = new BinaryReader(stream);
            try
            {
                var magicNumber = reader.ReadUInt32();
                if (magicNumber != 0x20534444)
                {
                    fourCC = 0;
                    return null;
                }

                var header = MarshalTo<DDS_HEADER>(reader.ReadBytes(DDS_HEADER.SizeInBytes));
                fourCC = header.ddspf.dwFourCC;

                UInt32 bytesPerPixel;
                UInt32 dwPitchOrLinearSize;

                var slices = 1;
                if (((DDSCAPS2)header.dwCaps2 & DDSCAPS2.DDSCAPS2_CUBEMAP) != 0)
                    slices = 6;

                bytesPerPixel = header.dwPitchOrLinearSize / (header.dwWidth * header.dwHeight);

                var c = header.dwCaps2;
                var mipCount = (int)header.dwMipMapCount;
                if (mipCount == 0)
                    mipCount = 1;

                if (header.ddspf.dwFlags == (uint)PixelFormatFlags.FourCC && fourCC == (uint)DDS_FOURCC.DX10)
                {
                    DDS_HEADER_DXT10 dx10Header = MarshalTo<DDS_HEADER_DXT10>(reader.ReadBytes(DDS_HEADER_DXT10.SizeInBytes));
                    dxgiFormat = dx10Header.dxgiFormat;

                    switch (dx10Header.dxgiFormat)
                    {
                        case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                        case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                        case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                        case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                        case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                        case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                        case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                        case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                        case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                        case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                        case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                        case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                        case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                        case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                        case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                        case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                        case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                        case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                        case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                        case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                        case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                            // The block-size is 8 bytes for DXT1, BC1, and BC4 formats, and 16 bytes for other block-compressed formats.
                            UInt32 blockSize = 8;
                            // For block-compressed formats, compute the pitch as:
                            dwPitchOrLinearSize = Math.Max(1, ((header.dwWidth + 3) / 4)) * blockSize;
                            break;

                        case DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM:
                        case DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM:
                            //For R8G8_B8G8, G8R8_G8B8, legacy UYVY-packed, and legacy YUY2-packed formats, compute the pitch as:
                            dwPitchOrLinearSize = ((header.dwWidth + 1) >> 1) * 4;
                            break;

                        default:
                            //case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
                            // For other formats, compute the pitch as:
                            bytesPerPixel = header.ddspf.dwSize;
                            dwPitchOrLinearSize = (header.dwWidth * bytesPerPixel + 7) / 8;
                            break;
                    }
                    //bytesPerPixel = BitsPerPixel(dx10Header.dxgiFormat);
                }

                if (bytesPerPixel == 0)
                {
                    width = (int)header.dwWidth;
                    height = (int)header.dwHeight;
                    int size = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                    return reader.ReadBytes(size);
                }

                for (var slice = 0; slice < slices; slice++)
                {
                    uint w = header.dwWidth == 0 ? 1 : header.dwWidth;
                    uint h = header.dwHeight == 0 ? 1 : header.dwHeight;

                    for (var map = 0; map < mipCount; map++)
                    {
                        var size = (int)(bytesPerPixel * w * h);
                        if (depthSlice == slice && ((width <= 0 && height <= 0) || (w == width && h == height)))
                        {
                            width = (int)w;
                            height = (int)h;
                            return reader.ReadBytes(size);
                        }
                        else
                        {
                            if (stream.CanSeek)
                            {
                                reader.BaseStream.Seek(size, SeekOrigin.Current);
                            }
                            else
                            {
                                // hack for VRage.Compression.MyStreamWrapper
                                for (int i = 0; i < size; i++)
                                    reader.BaseStream.ReadByte();
                            }
                        }

                        w = w >> 1;
                        h = h >> 1;
                        if (w == 0) w = 1;
                        if (h == 0) h = 1;
                    }

                    reader.BaseStream.Seek(27, SeekOrigin.Current);
                }

                return null;
            }
            catch
            {
                fourCC = 0;
                return null;
            }
        }

        #endregion

        private static uint BitsPerPixel(DXGI_FORMAT fmt)
        {
            switch (fmt)
            {
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_SINT:
                    return 128;

                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32B32_SINT:
                    return 96;

                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G32_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32G8X24_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT_S8X24_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_X32_TYPELESS_G8X24_UINT:
                    return 64;

                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10A2_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R11G11B10_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16G16_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R32_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R24G8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R24_UNORM_X8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_X24_TYPELESS_G8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R9G9B9E5_SHAREDEXP:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_B8G8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_G8R8_G8B8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM_SRGB:
                    return 32;

                case DXGI_FORMAT.DXGI_FORMAT_R8G8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R16_FLOAT:
                case DXGI_FORMAT.DXGI_FORMAT_D16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R16_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R16_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_B5G6R5_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM:
                    return 16;

                case DXGI_FORMAT.DXGI_FORMAT_R8_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_R8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8_UINT:
                case DXGI_FORMAT.DXGI_FORMAT_R8_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8_SINT:
                case DXGI_FORMAT.DXGI_FORMAT_A8_UNORM:
                    return 8;

                case DXGI_FORMAT.DXGI_FORMAT_R1_UNORM:
                    return 1;

                case DXGI_FORMAT.DXGI_FORMAT_BC1_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC4_SNORM:
                    return 4;

                case DXGI_FORMAT.DXGI_FORMAT_BC2_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC6H_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16:
                case DXGI_FORMAT.DXGI_FORMAT_BC6H_SF16:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_TYPELESS:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB:
                    return 8;

                default:
                    return 0;
            }
        }

        #region WriteImage

        public static void WriteImage(Bitmap image, string filename)
        {
            if (image == null)
                return;

            string extension = Path.GetExtension(filename)?.ToUpper();

            switch (extension)
            {
                case ".GIF":
                    image.Save(filename, ImageFormat.Gif);
                    break;

                case ".BMP":
                    image.Save(filename, ImageFormat.Bmp);
                    break;

                case ".PNG":
                    image.Save(filename, ImageFormat.Png);
                    break;

                case ".TIF":
                case ".TIFF":
                    image.Save(filename, ImageFormat.Tiff);
                    break;

                case ".JPEG":
                case ".JPG":
                    image.Save(filename, ImageFormat.Jpeg);
                    break;
                default:
                    throw new NotImplementedException("That file extension is not a valid export image format.");
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
