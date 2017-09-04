namespace SEToolbox.ImageLibrary.Effects
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public unsafe abstract class PixelEffect : IPixelEffect
    {
        /// <summary>
        /// Flag used to indicate whether a single pass or two passes are needed for quantization.
        /// </summary>
        private readonly bool _singlePass;

        /// <summary>
        /// Construct the effect
        /// </summary>
        /// <param name="singlePass">If true, the quantization only needs to loop through the source pixels once</param>
        /// <remarks>
        /// If you construct this class with a true value for singlePass, then the code will, when quantizing your image,
        /// only call the 'QuantizeImage' function. If two passes are required, the code will call 'InitialQuantizeImage'
        /// and then 'QuantizeImage'.
        /// </remarks>
        protected PixelEffect(bool singlePass)
        {
            _singlePass = singlePass;
        }

        /// <summary>
        /// Quantize an image and return the resulting output bitmap
        /// </summary>
        /// <param name="source">The image to quantize</param>
        /// <returns>A quantized version of the image</returns>
        public Bitmap Quantize(Bitmap source)
        {
            // Get the size of the source image
            var height = source.Height;
            var width = source.Width;

            // And construct a rectangle from these dimensions
            var bounds = new Rectangle(0, 0, width, height);

            // First off take a 32bpp copy of the image
            var copy = (Bitmap)source.Clone();

            // And construct an 8bpp version
            var output = new Bitmap(width, height, source.PixelFormat);

            // Define a pointer to the bitmap data
            BitmapData sourceData = null;


            //var myTestTransparentColor = Color.FromArgb(0, 255, 128, 64);
            //var image = new Bitmap(135, 135, PixelFormat.Format32bppArgb);

            //using (var g = Graphics.FromImage(image))
            //{
            //    g.Clear(myTestTransparentColor);
            //}

            //var color = image.GetPixel(0, 0);

            //Debug.Assert(color == myTestTransparentColor, "channels must match original");



            try
            {
                // Get the source image bits and lock into memory
                sourceData = copy.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                // Call the FirstPass function if not a single pass algorithm.
                // For something like an octree quantizer, this will run through
                // all image pixels, build a data structure, and create a palette.
                if (!_singlePass)
                    FirstPass(sourceData, width, height);

                // Then call the second pass which actually does the conversion
                SecondPass(sourceData, output, width, height, bounds);
            }
            finally
            {
                // Ensure that the bits are unlocked
                copy.UnlockBits(sourceData);
            }

            // Last but not least, return the output bitmap
            return output;
        }

        /// <summary>
        /// Execute the first pass through the pixels in the image
        /// </summary>
        /// <param name="sourceData">The source data</param>
        /// <param name="width">The width in pixels of the image</param>
        /// <param name="height">The height in pixels of the image</param>
        protected virtual void FirstPass(BitmapData sourceData, int width, int height)
        {
            // Define the source data pointers. The source row is a byte to
            // keep addition of the stride value easier (as this is in bytes)
            var pSourceRow = (byte*)sourceData.Scan0.ToPointer();

            // Loop through each row
            for (var row = 0; row < height; row++)
            {
                // Set the source pixel to the first pixel in this row
                Int32* pSourcePixel = (Int32*)pSourceRow;

                // And loop through each column
                for (var col = 0; col < width; col++, pSourcePixel++)
                    // Now I have the pixel, call the FirstPassQuantize function...
                    InitialQuantizePixel((Color32*)pSourcePixel);

                // Add the stride to the source row
                pSourceRow += sourceData.Stride;
            }
        }

        /// <summary>
        /// Execute a second pass through the bitmap
        /// </summary>
        /// <param name="sourceData">The source bitmap, locked into memory</param>
        /// <param name="output">The output bitmap</param>
        /// <param name="width">The width in pixels of the image</param>
        /// <param name="height">The height in pixels of the image</param>
        /// <param name="bounds">The bounding rectangle</param>
        protected virtual void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds)
        {
            BitmapData outputData = null;

            try
            {
                // Lock the output bitmap into memory
                outputData = output.LockBits(bounds, ImageLockMode.WriteOnly, sourceData.PixelFormat);

                // Define the source data pointers. The source row is a byte to
                // keep addition of the stride value easier (as this is in bytes)
                byte* pSourceRow = (byte*)sourceData.Scan0.ToPointer();
                Int32* pSourcePixel = (Int32*)pSourceRow;
                Int32* pPreviousPixel = pSourcePixel;

                // Now define the destination data pointers
                byte* pDestinationRow = (byte*)outputData.Scan0.ToPointer();
                Int32* pDestinationPixel = (Int32*)pDestinationRow;

                // And convert assign the value of the , so that I have values going into the loop
                QuantizePixel((Color32*)pSourcePixel, (Color32*)pDestinationPixel);

                // Loop through each row
                for (var row = 0; row < height; row++)
                {
                    // Set the source pixel to the first pixel in this row
                    pSourcePixel = (Int32*)pSourceRow;

                    // And set the destination pixel pointer to the first pixel in the row
                    pDestinationPixel = (Int32*)pDestinationRow;

                    // Loop through each pixel on this scan line
                    for (var col = 0; col < width; col++, pSourcePixel++, pDestinationPixel++)
                    {
                        QuantizePixel((Color32*)pSourcePixel, (Color32*)pDestinationPixel);
                    }

                    // Add the stride to the source row
                    pSourceRow += sourceData.Stride;

                    // And to the destination row
                    pDestinationRow += outputData.Stride;
                }
            }
            finally
            {
                // Ensure that I unlock the output bits
                output.UnlockBits(outputData);
            }
        }

        /// <summary>
        /// Override this to process the pixel in the first pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <remarks>
        /// This function need only be overridden if your quantize algorithm needs two passes,
        /// such as an Octree quantizer.
        /// </remarks>
        protected virtual void InitialQuantizePixel(Color32* pixel)
        {
        }

        /// <summary>
        /// Override this to process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <param name="destinationPixel"></param>
        /// <returns>The quantized value</returns>
        protected abstract void QuantizePixel(Color32* pixel, Color32* destinationPixel);

        /// <summary>
        /// Struct that defines a 32 bpp colour
        /// </summary>
        /// <remarks>
        /// This struct is used to read data from a 32 bits per pixel image
        /// in memory, and is ordered in this manner as this is the way that
        /// the data is layed out in memory
        /// </remarks>
        [StructLayout(LayoutKind.Explicit)]
        public struct Color32
        {
            /// <summary>
            /// Holds the blue component of the colour
            /// </summary>
            [FieldOffset(0)]
            public byte Blue;
            /// <summary>
            /// Holds the green component of the colour
            /// </summary>
            [FieldOffset(1)]
            public byte Green;
            /// <summary>
            /// Holds the red component of the colour
            /// </summary>
            [FieldOffset(2)]
            public byte Red;
            /// <summary>
            /// Holds the alpha component of the colour
            /// </summary>
            [FieldOffset(3)]
            public byte Alpha;

            /// <summary>
            /// Permits the color32 to be treated as an int32
            /// </summary>
            [FieldOffset(0)]
            public int ARGB;

            /// <summary>
            /// Return the color for this Color32 object
            /// </summary>
            public Color Color
            {
                get { return Color.FromArgb(Alpha, Red, Green, Blue); }
            }
        }
    }
}
