namespace SEToolbox.Models
{
    using System.Windows.Media.Media3D;

    public class BindableSize3DModel : BaseModel
    {
        #region fields

        private Size3D _size;

        #endregion

        public BindableSize3DModel()
        {
            _size = new Size3D(0, 0, 0);
        }

        public BindableSize3DModel(int width, int height, int depth)
        {
            _size = new Size3D(width, height, depth);
        }

        public BindableSize3DModel(Rect3D size)
        {
            _size = size.IsEmpty ? new Size3D() : new Size3D(size.SizeX, size.SizeY, size.SizeZ);
        }

        public BindableSize3DModel(Size3D size)
        {
            _size = new Size3D(size.X, size.Y, size.Z);
        }

        #region Properties

        public double Width
        {
            get
            {
                return _size.X;
            }

            set
            {
                if (value != _size.X)
                {
                    _size.X = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public double Height
        {
            get
            {
                return _size.Y;
            }

            set
            {
                if (value != _size.Y)
                {
                    _size.Y = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public double Depth
        {
            get
            {
                return _size.Z;
            }

            set
            {
                if (value != _size.Z)
                {
                    _size.Z = value;
                    OnPropertyChanged(nameof(Depth));
                }
            }
        }

        public Size3D ToSize3D
        {
            get
            {
                return _size;
            }
        }

        #endregion
    }
}
