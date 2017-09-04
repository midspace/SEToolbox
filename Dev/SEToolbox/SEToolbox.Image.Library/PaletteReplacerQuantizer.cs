namespace SEToolbox.ImageLibrary
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    /// <summary>
    /// Summary description for SEPaletteQuantizer.
    /// </summary>
    public unsafe class PaletteReplacerQuantizer : Quantizer
    {
        /// <summary>
        /// Lookup table for colors
        /// </summary>
        private readonly Hashtable _colorMap;

        /// <summary>
        /// List of all colors. Keys are Quantizer targets. Values are what they will be replaced with.
        /// </summary>
        private readonly Dictionary<Color, Color> _colorMatcher;

        /// <summary>
        /// List of final colors to be used in the palette.
        /// </summary>
        private readonly List<Color> _finalColors;

        /// <summary>
        /// Construct the palette quantizer
        /// </summary>
        /// <param name="paletteReplacement">The color palette to quantize to</param>
        /// <remarks>
        /// Palette quantization only requires a single quantization step
        /// </remarks>
        public PaletteReplacerQuantizer(Dictionary<Color, Color> paletteReplacement)
            : base(true)
        {
            _colorMatcher = paletteReplacement;

            _colorMap = new Hashtable();
            _finalColors = new List<Color>(_colorMatcher.Values.Distinct());
        }

        /// <summary>
        /// Override this to process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <returns>The quantized value</returns>
        protected override byte QuantizePixel(Color32* pixel)
        {
            byte colorIndex = 0;
            int colorHash = pixel->ARGB;

            // Check if the color is in the lookup table
            if (_colorMap.ContainsKey(colorHash))
            {
                colorIndex = (byte)_colorMap[colorHash];
            }
            else
            {
                // Not found - loop through the palette and find the nearest match.
                // Firstly check the alpha value - if 0, lookup the transparent color
                if (0 == pixel->Alpha)
                {
                    // Transparent. Lookup the first color with an alpha value of 0
                    for (int index = 0; index < _finalColors.Count; index++)
                    {
                        if (0 == _finalColors[index].A)
                        {
                            colorIndex = (byte)index;
                            break;
                        }
                    }
                }
                else
                {
                    // Not transparent...
                    int leastDistance = int.MaxValue;
                    int red = pixel->Red;
                    int green = pixel->Green;
                    int blue = pixel->Blue;

                    // Loop through the entire palette, looking for the closest color match

                    foreach (KeyValuePair<Color, Color> kvp in _colorMatcher)
                    {
                        Color paletteColor = kvp.Key;

                        int redDistance = paletteColor.R - red;
                        int greenDistance = paletteColor.G - green;
                        int blueDistance = paletteColor.B - blue;

                        int distance = (redDistance * redDistance) +
                                           (greenDistance * greenDistance) +
                                           (blueDistance * blueDistance);

                        if (distance < leastDistance)
                        {
                            colorIndex = (byte)_finalColors.IndexOf(kvp.Value);

                            leastDistance = distance;

                            // And if it's an exact match, exit the loop
                            if (0 == distance)
                                break;
                        }
                    }
                }

                // Now I have the color, pop it into the hashtable for next time
                _colorMap.Add(colorHash, colorIndex);
            }

            return colorIndex;
        }

        /// <summary>
        /// Retrieve the palette for the quantized image
        /// </summary>
        /// <param name="palette">Any old palette, this is overrwritten</param>
        /// <returns>The new color palette</returns>
        protected override ColorPalette GetPalette(ColorPalette palette)
        {
            // blanking the original palette is not necessary, unless we want a clean palette.
            //for (int index = 0; index < palette.Entries.Length; index++)
            //    palette.Entries[index] = Color.FromArgb(0, 0, 0, 0);

            for (int index = 0; index < _finalColors.Count; index++)
                palette.Entries[index] = _finalColors[index];

            return palette;
        }
    }
}
