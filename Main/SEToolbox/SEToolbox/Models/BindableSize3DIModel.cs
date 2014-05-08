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
            this.Width = 0;
            this.Height = 0;
            this.Depth = 0;
        }

        public BindableSize3DIModel(int width, int height, int depth)
            : this()
        {
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
        }

        public BindableSize3DIModel(Vector3I size)
        {
            this.Width = size.X;
            this.Height = size.Y;
            this.Depth = size.Z;
        }

        public BindableSize3DIModel(Size size)
            : this()
        {
            this.Width = size.Width;
            this.Height = size.Height;
        }

        #region Properties

        public int Width
        {
            get
            {
                return this._width;
            }

            set
            {
                if (value != this._width)
                {
                    this._width = value;
                    this.RaisePropertyChanged(() => Width);
                }
            }
        }

        public int Height
        {
            get
            {
                return this._height;
            }

            set
            {
                if (value != this._height)
                {
                    this._height = value;
                    this.RaisePropertyChanged(() => Height);
                }
            }
        }

        public int Depth
        {
            get
            {
                return this._depth;
            }

            set
            {
                if (value != this._depth)
                {
                    this._depth = value;
                    this.RaisePropertyChanged(() => Depth);
                }
            }
        }

        public Size3D ToSize3D
        {
            get
            {
                return new Size3D(this.Width, this.Height, this.Depth);
            }
        }

        #endregion

        public Vector3I ToVector3I()
        {
            return new Vector3I(this.Width, this.Height, this.Depth);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", this.Width, this.Height, this.Depth);
        }
    }
}
