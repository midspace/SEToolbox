namespace SEToolbox.ImageLibrary.Effects
{
    using System.Drawing;

    public interface IPixelEffect
    {
        Bitmap Quantize(Bitmap source);
    }
}
