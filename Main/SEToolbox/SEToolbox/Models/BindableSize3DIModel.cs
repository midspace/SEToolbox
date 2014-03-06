namespace SEToolbox.Models
{
    using System.Drawing;
    using System.Windows.Media.Media3D;
    using VRageMath;

    public class BindableSize3DIModel : BaseModel
    {
        #region fields

        private int width;
        private int height;
        private int depth;

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
                return this.width;
            }

            set
            {
                if (value != this.width)
                {
                    this.width = value;
                    this.RaisePropertyChanged(() => Width);
                }
            }
        }

        public int Height
        {
            get
            {
                return this.height;
            }

            set
            {
                if (value != this.height)
                {
                    this.height = value;
                    this.RaisePropertyChanged(() => Height);
                }
            }
        }

        public int Depth
        {
            get
            {
                return this.depth;
            }

            set
            {
                if (value != this.depth)
                {
                    this.depth = value;
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
