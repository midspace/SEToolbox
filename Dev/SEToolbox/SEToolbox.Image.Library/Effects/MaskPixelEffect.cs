namespace SEToolbox.ImageLibrary.Effects
{
    /// <summary>
    /// Summary description for MaskEffect.
    /// </summary>
    public unsafe class MaskPixelEffect : PixelEffect
    {
        /// <summary>
        /// Construct the Mask pixel effect
        /// </summary>
        /// <remarks>
        /// Mask pixel effect only requires a single effect step
        /// </remarks>
        public MaskPixelEffect()
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
            if (pixel->Alpha > 127)
            {
                destinationPixel->Red = pixel->Red;
                destinationPixel->Green = pixel->Green;
                destinationPixel->Blue = pixel->Blue;
                destinationPixel->Alpha = 255;
            }
            else
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

                //destinationPixel->Alpha = 0;
                destinationPixel->Alpha = pixel->Alpha;
            }
        }
    }
}
