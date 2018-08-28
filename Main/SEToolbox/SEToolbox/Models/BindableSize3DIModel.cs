namespace SEToolbox.Models
{
    using System.Drawing;
    using System.Windows.Media.Media3D;

    using VRageMath;

    public class BindableSize3DIModel : BaseModel
    {
        #region fields

        private int _width;
        private int _height;
        private int _depth;

        #endregion

        public BindableSize3DIModel()
        {
            Width = 0;
            Height = 0;
            Depth = 0;
        }

        public BindableSize3DIModel(int width, int height, int depth)
            : this()
        {
            Width = width;
            Height = height;
            Depth = depth;
        }

        public BindableSize3DIModel(Vector3I size)
        {
            Width = size.X;
            Height = size.Y;
            Depth = size.Z;
        }

        public BindableSize3DIModel(Size size)
            : this()
        {
            Width = size.Width;
            Height = size.Height;
        }

        #region Properties

        public int Width
        {
            get
            {
                return _width;
            }

            set
            {
                if (value != _width)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }

            set
            {
                if (value != _height)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public int Depth
        {
            get
            {
                return _depth;
            }

            set
            {
                if (value != _depth)
                {
                    _depth = value;
                    OnPropertyChanged(nameof(Depth));
                }
            }
        }

        public Size3D ToSize3D
        {
            get
            {
                return new Size3D(Width, Height, Depth);
            }
        }

        #endregion

        public Vector3I ToVector3I()
        {
            return new Vector3I(Width, Height, Depth);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", Width, Height, Depth);
        }
    }
}
