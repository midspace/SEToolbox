namespace SEToolbox.Support
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Reflection;
    using System.Windows.Media.Imaging;
    using System.Xml;
    using SEToolbox.ImageLibrary;

    public static class ToolboxExtensions
    {
        #region GetOptimizerPalatte

        /// <summary>
        /// This palatte is used for reducing the colors in an image down to specific colors, by using key colors for reduction.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<Color, Color> GetOptimizerPalatte()
        {
            Dictionary<Color, Color> palette = new Dictionary<Color, Color>()
                {
                    {Color.FromArgb(255, 0, 0, 0), Color.Black},
                    {Color.FromArgb(255, 20, 20, 20), Color.Black},
                    {Color.FromArgb(255, 40, 40, 40), Color.Black},

                    {Color.FromArgb(255, 128, 128, 128), Color.Gray},
                    {Color.FromArgb(255, 92, 92, 92), Color.Gray},

                    {Color.FromArgb(255, 255, 255, 255), Color.White},
                    {Color.FromArgb(255, 192, 192, 192), Color.White},

                    {Color.FromArgb(255, 255, 0, 0), Color.Red},
                    {Color.FromArgb(255, 192, 110, 110), Color.Red},
                    {Color.FromArgb(255, 160, 110, 110), Color.Red},
                    {Color.FromArgb(255, 120, 80, 80), Color.Red},
                    {Color.FromArgb(255, 148, 40, 40), Color.Red},
                    {Color.FromArgb(255, 148, 0, 0), Color.Red},
                    {Color.FromArgb(255, 128, 0, 0), Color.Red},
                    {Color.FromArgb(255, 92, 20, 20), Color.Red},
                    {Color.FromArgb(255, 64, 0, 0), Color.Red},
                    {Color.FromArgb(255, 64, 32, 32), Color.Red},

                    {Color.FromArgb(255, 0, 255, 0), Color.Green},
                    {Color.FromArgb(255, 110, 192, 110), Color.Green},
                    {Color.FromArgb(255, 110, 160, 110), Color.Green},
                    {Color.FromArgb(255, 80, 120, 80), Color.Green},
                    {Color.FromArgb(255, 40, 148, 40), Color.Green},
                    {Color.FromArgb(255, 0, 148, 0), Color.Green},
                    {Color.FromArgb(255, 0, 128, 0), Color.Green},
                    {Color.FromArgb(255, 0, 64, 0), Color.Green},
                    {Color.FromArgb(255, 32, 64, 32), Color.Green},

                    {Color.FromArgb(255, 0, 0, 255), Color.Blue},
                    {Color.FromArgb(255, 110, 110, 192), Color.Blue},
                    {Color.FromArgb(255, 110, 110, 160), Color.Blue},
                    {Color.FromArgb(255, 80, 90, 120), Color.Blue},
                    {Color.FromArgb(255, 40, 40, 148), Color.Blue},
                    {Color.FromArgb(255, 0, 0, 148), Color.Blue},
                    {Color.FromArgb(255, 0, 0, 128), Color.Blue},
                    {Color.FromArgb(255, 0, 0, 64), Color.Blue},
                    {Color.FromArgb(255, 32, 32, 64), Color.Blue},

                    {Color.FromArgb(255, 128, 128, 0), Color.Yellow},
                    {Color.FromArgb(255, 215, 128, 0), Color.Yellow},
                    {Color.FromArgb(255, 192, 128, 0), Color.Yellow},
                    {Color.FromArgb(255, 215, 64, 0), Color.Yellow},
                    {Color.FromArgb(255, 192, 64, 0), Color.Yellow},
                    {Color.FromArgb(255, 215, 192, 128), Color.Yellow},
                    {Color.FromArgb(255, 215, 192, 96), Color.Yellow},
                    {Color.FromArgb(255, 215, 192, 64), Color.Yellow},
                    {Color.FromArgb(255, 192, 172, 96), Color.Yellow},
                    {Color.FromArgb(255, 192, 160, 96), Color.Yellow},
                    {Color.FromArgb(255, 192, 160, 64), Color.Yellow},
                    {Color.FromArgb(255, 192, 160, 32), Color.Yellow},
                    {Color.FromArgb(255, 128, 96, 48), Color.Yellow},
                    {Color.FromArgb(255, 128, 96, 32), Color.Yellow},
                    {Color.FromArgb(255, 128, 96, 16), Color.Yellow},
                    {Color.FromArgb(255, 92, 64, 16), Color.Yellow},

                    // Make 'Transparent' last, to give preference to 'White' above, otherwise white can be mistakenly made transparent.
                    {Color.Transparent, Color.Transparent},
                };

            return palette;
        }

        /// <summary>
        /// Maps the colors used in optimzation to color names used in Space Engineers Armor Cube names.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<Color, string> GetPalatteNames()
        {
            Dictionary<Color, string> palette = new Dictionary<Color, string>()
                {
                    {Color.Black, "Black"},
                    {Color.Gray, ""},
                    {Color.White, "White"},
                    {Color.Red, "Red"},
                    {Color.Green, "Green"},
                    {Color.Blue, "Blue"},
                    {Color.Yellow, "Yellow"},
                };

            return palette;
        }

        #endregion

        #region OptimizeImagePalette

        public static Bitmap OptimizeImagePalette(string imageFilename)
        {
            imageFilename = Path.GetFullPath(imageFilename);
            Bitmap bmp = new Bitmap(imageFilename);
            return OptimizeImagePalette(bmp);
        }

        public static Bitmap OptimizeImagePalette(Bitmap bmp)
        {
            Bitmap palatteImage;

            using (Bitmap image = new Bitmap(bmp))
            {
                var palette = GetOptimizerPalatte();
                PaletteReplacerQuantizer paletteQuantizer = new PaletteReplacerQuantizer(palette);

                palatteImage = paletteQuantizer.Quantize(image);
            }

            bmp.Dispose();

            return palatteImage;
        }

        #endregion

        #region CompactXmlFile

        /// <summary>
        /// Compacts the XML source, removing comments, whitespace, and line wraps.
        /// </summary>
        /// <param name="fileSource">source file name</param>
        /// <param name="fileDestination">destination file name</param>
        /// <returns></returns>
        public static bool CompactXmlFile(string fileSource, string fileDestination)
        {
            if (!File.Exists(fileSource))
            {
                return false;
            }

            if (File.Exists(fileDestination))
            {
                File.Delete(fileDestination);
            }

            var settingsSource = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            var settingsDestination = new XmlWriterSettings()
            {
                Indent = false,
                NewLineHandling = System.Xml.NewLineHandling.None,
                NewLineOnAttributes = false
            };

            using (var xmlWriter = XmlWriter.Create(fileDestination, settingsDestination))
            {

                using (XmlReader xmlReader = XmlReader.Create(fileSource, settingsSource))
                {
                    xmlWriter.WriteNode(xmlReader, true);
                }

                //xmlWriter.Close();
            }

            return true;
        }

        #endregion

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
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, Size size)
        {
            if (size.Width < 1 || size.Height < 1)
            {
                return null;
            }

            // a holder for the result
            Bitmap result = new Bitmap(size.Width, size.Height);
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
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            BitmapImage bitmapImage = new BitmapImage();
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

        #region ConvertPolyToVox

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polyFilename"></param>
        /// <param name="fixScale">Specify voxel size of longest dimension. 1-1024, &lt;=256 for KVX</param>
        /// <param name="gaplessModel">Enable an experimental xor-style converter. It's useful for gap-less models but has buggy color conversion</param>
        /// <returns></returns>
        public static string ConvertPolyToVox(string polyFilename, int fixScale, bool gaplessModel)
        {
            string voxFilename = null;

            if (fixScale > 1024)
                fixScale = 1024;

            string tempfilename = TempfileUtil.NewFilename(".vox");

            Process p = new Process();
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            p.StartInfo.FileName = Path.Combine(directory, "poly2vox.exe");
            p.StartInfo.WorkingDirectory = directory;
            string arguments = string.Format("\"{0}\" \"{1}\"", polyFilename, tempfilename);

            if (fixScale > 1)
            {
                arguments += string.Format(" /v{0}", fixScale);
            }

            if (gaplessModel)
            {
                arguments += string.Format(" /x");
            }

            p.StartInfo.Arguments = arguments;

            p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            var ret = p.Start();
            p.WaitForExit();

            if (ret && File.Exists(tempfilename))
            {
                voxFilename = tempfilename;
            }

            return voxFilename;
        }

        #endregion

        #region Shuffle

        /// <summary>
        ///  Randomly re-orders a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = RandomUtil.GetInt(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion
    }
}
