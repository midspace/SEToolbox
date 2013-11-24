namespace SEToolbox.Support
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using SEToolbox.ImageLibrary;

    public class ToolboxExtensions
    {
        #region GetOptimizerPalatte

        internal static Dictionary<Color, string> GetOptimizerPalatte()
        {
            Dictionary<Color, string> palette = new Dictionary<Color, string>()
                {
                    {Color.FromArgb(255, 0, 0, 0), "Black"},
                    {Color.FromArgb(255, 20, 20, 20), "Black"},
                    {Color.FromArgb(255, 40, 40, 40), "Black"},

                    {Color.FromArgb(255, 128, 128, 128), ""}, // grey
                    {Color.FromArgb(255, 92, 92, 92), ""}, // grey

                    {Color.FromArgb(255, 255, 255, 255), "White"},
                    {Color.FromArgb(255, 192, 192, 192), "White"},

                    {Color.FromArgb(255, 255, 0, 0), "Red"},
                    {Color.FromArgb(255, 192, 110, 110), "Red"},
                    {Color.FromArgb(255, 160, 110, 110), "Red"},
                    {Color.FromArgb(255, 120, 80, 80), "Red"},
                    {Color.FromArgb(255, 148, 40, 40), "Red"},
                    {Color.FromArgb(255, 148, 0, 0), "Red"},
                    {Color.FromArgb(255, 128, 0, 0), "Red"},
                    {Color.FromArgb(255, 92, 20, 20), "Red"},
                    {Color.FromArgb(255, 64, 0, 0), "Red"},
                    {Color.FromArgb(255, 64, 32, 32), "Red"},

                    {Color.FromArgb(255, 0, 255, 0), "Green"},
                    {Color.FromArgb(255, 110, 192, 110), "Green"},
                    {Color.FromArgb(255, 110, 160, 110), "Green"},
                    {Color.FromArgb(255, 80, 120, 80), "Green"},
                    {Color.FromArgb(255, 40, 148, 40), "Green"},
                    {Color.FromArgb(255, 0, 148, 0), "Green"},
                    {Color.FromArgb(255, 0, 128, 0), "Green"},
                    {Color.FromArgb(255, 0, 64, 0), "Green"},
                    {Color.FromArgb(255, 32, 64, 32), "Green"},

                    {Color.FromArgb(255, 0, 0, 255), "Blue"},
                    {Color.FromArgb(255, 110, 110, 192), "Blue"},
                    {Color.FromArgb(255, 110, 110, 160), "Blue"},
                    {Color.FromArgb(255, 80, 90, 120), "Blue"},
                    {Color.FromArgb(255, 40, 40, 148), "Blue"},
                    {Color.FromArgb(255, 0, 0, 148), "Blue"},
                    {Color.FromArgb(255, 0, 0, 128), "Blue"},
                    {Color.FromArgb(255, 0, 0, 64), "Blue"},
                    {Color.FromArgb(255, 32, 32, 64), "Blue"},

                    {Color.FromArgb(255, 128, 128, 0), "Yellow"},
                    {Color.FromArgb(255, 215, 128, 0), "Yellow"},
                    {Color.FromArgb(255, 192, 128, 0), "Yellow"},
                    {Color.FromArgb(255, 215, 64, 0), "Yellow"},
                    {Color.FromArgb(255, 192, 64, 0), "Yellow"},
                    {Color.FromArgb(255, 215, 192, 128), "Yellow"},
                    {Color.FromArgb(255, 215, 192, 96), "Yellow"},
                    {Color.FromArgb(255, 215, 192, 64), "Yellow"},
                    {Color.FromArgb(255, 192, 172, 96), "Yellow"},
                    {Color.FromArgb(255, 192, 160, 96), "Yellow"},
                    {Color.FromArgb(255, 192, 160, 64), "Yellow"},
                    {Color.FromArgb(255, 192, 160, 32), "Yellow"},
                    {Color.FromArgb(255, 128, 96, 48), "Yellow"},
                    {Color.FromArgb(255, 128, 96, 32), "Yellow"},
                    {Color.FromArgb(255, 128, 96, 16), "Yellow"},
                    {Color.FromArgb(255, 92, 64, 16), "Yellow"},

                };

            return palette;
        }

        #endregion

        #region OptimizeImagePalette

        public static Bitmap OptimizeImagePalette(string imageFilename)
        {
            imageFilename = Path.GetFullPath(imageFilename);
            Bitmap bmp = new Bitmap(imageFilename);
            Bitmap palatteImage;

            using (Bitmap image = new Bitmap(bmp))
            {
                var palette = GetOptimizerPalatte();

                //OctreeQuantizer octreeQuantizer = new OctreeQuantizer(255, 8);

                //using (Bitmap octreeImage = octreeQuantizer.Quantize(image))
                //{
                ArrayList myPalette = new ArrayList(palette.Keys.ToArray());
                PaletteQuantizer paletteQuantizer = new PaletteQuantizer(myPalette);

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
    }
}
