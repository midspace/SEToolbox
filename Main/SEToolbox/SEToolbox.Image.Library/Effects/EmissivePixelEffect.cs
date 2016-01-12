namespace SEToolbox.ImageLibrary.Effects
{
    /// <summary>
    /// Summary description for EmissiveEffect.
    /// </summary>
    public unsafe class EmissivePixelEffect : PixelEffect
    {
        private readonly byte _alphaEmmissiveValue;

        /// <summary>
        /// Construct the Emissive pixel effect
        /// </summary>
        /// <remarks>
        /// Emissive pixel effect only requires a single effect step
        /// </remarks>
        public EmissivePixelEffect(byte alphaEmmissiveValue)
            : base(true)
        {
            _alphaEmmissiveValue = alphaEmmissiveValue;
        }

        /// <summary>
        /// Override this to process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <param name="destinationPixel"></param>
        /// <returns>The quantized value</returns>
        protected override void QuantizePixel(Color32* pixel, Color32* destinationPixel)
        {
            destinationPixel->Red = pixel->Red;
            destinationPixel->Green = pixel->Green;
            destinationPixel->Blue = pixel->Blue;

            if (pixel->Alpha == _alphaEmmissiveValue)
            {
                destinationPixel->Alpha = 255;
            }
            else
            {
                destinationPixel->Alpha = 0;
            }
        }
    }
}
