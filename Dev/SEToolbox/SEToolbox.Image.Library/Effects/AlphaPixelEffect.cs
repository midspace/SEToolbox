namespace SEToolbox.ImageLibrary.Effects
{
    /// <summary>
    /// Summary description for AlphaEffect.
    /// </summary>
    public unsafe class AlphaPixelEffect : PixelEffect
    {
        /// <summary>
        /// Construct the Alpha pixel effect
        /// </summary>
        /// <remarks>
        /// Alpha pixel effect only requires a single effect step
        /// </remarks>
        public AlphaPixelEffect()
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
            destinationPixel->Red = pixel->Alpha;
            destinationPixel->Blue = pixel->Alpha;
            destinationPixel->Green = pixel->Alpha;
            destinationPixel->Alpha = 255;
        }
    }
}
