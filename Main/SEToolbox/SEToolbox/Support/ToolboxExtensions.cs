namespace SEToolbox.Support
{
    using SEToolbox.ImageLibrary;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Imaging;
    using System.Xml;

    public class ToolboxExtensions
    {
        #region GetOptimizerPalatte

        //internal static Dictionary<Color, string> GetOptimizerPalatte()
        //{
        //    Dictionary<Color, string> palette = new Dictionary<Color, string>()
        //        {
        //            {Color.FromArgb(255, 0, 0, 0), "Black"},
        //            {Color.FromArgb(255, 20, 20, 20), "Black"},
        //            {Color.FromArgb(255, 40, 40, 40), "Black"},

        //            {Color.FromArgb(255, 128, 128, 128), ""}, // grey
        //            {Color.FromArgb(255, 92, 92, 92), ""}, // grey

        //            {Color.FromArgb(255, 255, 255, 255), "White"},
        //            {Color.FromArgb(255, 192, 192, 192), "White"},

        //            {Color.FromArgb(255, 255, 0, 0), "Red"},
        //            {Color.FromArgb(255, 192, 110, 110), "Red"},
        //            {Color.FromArgb(255, 160, 110, 110), "Red"},
        //            {Color.FromArgb(255, 120, 80, 80), "Red"},
        //            {Color.FromArgb(255, 148, 40, 40), "Red"},
        //            {Color.FromArgb(255, 148, 0, 0), "Red"},
        //            {Color.FromArgb(255, 128, 0, 0), "Red"},
        //            {Color.FromArgb(255, 92, 20, 20), "Red"},
        //            {Color.FromArgb(255, 64, 0, 0), "Red"},
        //            {Color.FromArgb(255, 64, 32, 32), "Red"},

        //            {Color.FromArgb(255, 0, 255, 0), "Green"},
        //            {Color.FromArgb(255, 110, 192, 110), "Green"},
        //            {Color.FromArgb(255, 110, 160, 110), "Green"},
        //            {Color.FromArgb(255, 80, 120, 80), "Green"},
        //            {Color.FromArgb(255, 40, 148, 40), "Green"},
        //            {Color.FromArgb(255, 0, 148, 0), "Green"},
        //            {Color.FromArgb(255, 0, 128, 0), "Green"},
        //            {Color.FromArgb(255, 0, 64, 0), "Green"},
        //            {Color.FromArgb(255, 32, 64, 32), "Green"},

        //            {Color.FromArgb(255, 0, 0, 255), "Blue"},
        //            {Color.FromArgb(255, 110, 110, 192), "Blue"},
        //            {Color.FromArgb(255, 110, 110, 160), "Blue"},
        //            {Color.FromArgb(255, 80, 90, 120), "Blue"},
        //            {Color.FromArgb(255, 40, 40, 148), "Blue"},
        //            {Color.FromArgb(255, 0, 0, 148), "Blue"},
        //            {Color.FromArgb(255, 0, 0, 128), "Blue"},
        //            {Color.FromArgb(255, 0, 0, 64), "Blue"},
        //            {Color.FromArgb(255, 32, 32, 64), "Blue"},

        //            {Color.FromArgb(255, 128, 128, 0), "Yellow"},
        //            {Color.FromArgb(255, 215, 128, 0), "Yellow"},
        //            {Color.FromArgb(255, 192, 128, 0), "Yellow"},
        //            {Color.FromArgb(255, 215, 64, 0), "Yellow"},
        //            {Color.FromArgb(255, 192, 64, 0), "Yellow"},
        //            {Color.FromArgb(255, 215, 192, 128), "Yellow"},
        //            {Color.FromArgb(255, 215, 192, 96), "Yellow"},
        //            {Color.FromArgb(255, 215, 192, 64), "Yellow"},
        //            {Color.FromArgb(255, 192, 172, 96), "Yellow"},
        //            {Color.FromArgb(255, 192, 160, 96), "Yellow"},
        //            {Color.FromArgb(255, 192, 160, 64), "Yellow"},
        //            {Color.FromArgb(255, 192, 160, 32), "Yellow"},
        //            {Color.FromArgb(255, 128, 96, 48), "Yellow"},
        //            {Color.FromArgb(255, 128, 96, 32), "Yellow"},
        //            {Color.FromArgb(255, 128, 96, 16), "Yellow"},
        //            {Color.FromArgb(255, 92, 64, 16), "Yellow"},
        //        };

        //    return palette;
        //}

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
                };

            return palette;
        }

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
            //Bitmap palatteImage;

            //using (Bitmap image = new Bitmap(bmp))
            //{
            //    var palette = GetOptimizerPalatte();

            //    //OctreeQuantizer octreeQuantizer = new OctreeQuantizer(255, 8);

            //    //using (Bitmap octreeImage = octreeQuantizer.Quantize(image))
            //    //{
            //    ArrayList myPalette = new ArrayList(palette.Keys.ToArray());
            //    PaletteQuantizer paletteQuantizer = new PaletteQuantizer(myPalette);

            //    palatteImage = paletteQuantizer.Quantize(image);
            //}

            //bmp.Dispose();

            //return palatteImage;
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
    }
}
