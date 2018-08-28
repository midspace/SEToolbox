namespace SEToolbox.Models
{
    public class BindablePoint3DIModel : BaseModel
    {
        #region fields

        private int _x;
        private int _y;
        private int _z;

        #endregion

        #region ctor

        public BindablePoint3DIModel()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public BindablePoint3DIModel(int x, int y, int z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public BindablePoint3DIModel(VRageMath.Vector3I vector)
            : this()
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        #endregion

        #region Properties

        public int X
        {
            get
            {
                return _x;
            }

            set
            {
                if (value != _x)
                {
                    _x = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public int Y
        {
            get
            {
                return _y;
            }

            set
            {
                if (value != _y)
                {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public int Z
        {
            get
            {
                return _z;
            }

            set
            {
                if (value != _z)
                {
                    _z = value;
                    OnPropertyChanged(nameof(Z));
                }
            }
        }

        #endregion

        #region methods

        public VRageMath.Vector3I ToVector3I()
        {
            return new VRageMath.Vector3I(X, Y, Z);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", X, Y, Z);
        }

        #endregion
    }
}
