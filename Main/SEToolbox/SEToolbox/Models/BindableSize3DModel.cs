namespace SEToolbox.Models
{
    using System.Drawing;
    using System.Windows.Media.Media3D;

    public class BindableSize3DModel : BaseModel
    {
        #region fields

        private Size3D size;

        #endregion

        public BindableSize3DModel()
        {
            this.size = new Size3D(0, 0, 0);
        }

        public BindableSize3DModel(int width, int height, int depth)
        {
            this.size = new Size3D(width, height, depth);
        }

        public BindableSize3DModel(Rect3D size)
        {
            this.size = new Size3D(size.SizeX, size.SizeY, size.SizeZ);
        }

        #region Properties

        public double Width
        {
            get
            {
                return this.size.X;
            }

            set
            {
                if (value != this.size.X)
                {
                    this.size.X = value;
                    this.RaisePropertyChanged(() => Width);
                }
            }
        }

        public double Height
        {
            get
            {
                return this.size.Y;
            }

            set
            {
                if (value != this.size.Y)
                {
                    this.size.Y = value;
                    this.RaisePropertyChanged(() => Height);
                }
            }
        }

        public double Depth
        {
            get
            {
                return this.size.Z;
            }

            set
            {
                if (value != this.size.Z)
                {
                    this.size.Z = value;
                    this.RaisePropertyChanged(() => Depth);
                }
            }
        }

        public Size3D ToSize3D
        {
            get
            {
                return size;
            }
        }

        #endregion
    }
}
