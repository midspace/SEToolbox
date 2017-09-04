namespace SEToolbox.ImageLibrary.Effects
{
    /// <summary>
    /// Summary description for DesaturatePixelEffect.
    /// </summary>
    public unsafe class DesaturatePixelEffect : PixelEffect
    {
        /// <summary>
        /// Construct the Desaturate PixelEffect
        /// </summary>
        /// <remarks>
        /// Desaturate effect only requires a single effect step
        /// </remarks>
        public DesaturatePixelEffect()
            : base(true)
        {
        }

        /// <summary>
        /// Override this to process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <param name="destinationPixel"></param>
        /// <returns>The quantized value</returns>
        protected override void QuantizePixel(Color32* pixel, Color32* destinationPixel)
        {
            int maxColor = pixel->Red;
            if (maxColor < pixel->Green)
                maxColor = pixel->Green;
            if (maxColor < pixel->Blue)
                maxColor = pixel->Blue;

            int minColor = pixel->Red;
            if (minColor > pixel->Green)
                minColor = pixel->Green;
            if (minColor > pixel->Blue)
                minColor = pixel->Blue;

            var luminance = (byte)((minColor + maxColor) / 2.00f);

            destinationPixel->Red = luminance;
            destinationPixel->Green = luminance;
            destinationPixel->Blue = luminance;
            destinationPixel->Alpha = pixel->Alpha;
        }
    }
}
