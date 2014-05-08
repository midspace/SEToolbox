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
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        public BindablePoint3DIModel(int x, int y, int z)
            : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public BindablePoint3DIModel(VRageMath.Vector3I vector)
            : this()
        {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }

        #endregion

        #region Properties

        public int X
        {
            get
            {
                return this._x;
            }

            set
            {
                if (value != this._x)
                {
                    this._x = value;
                    this.RaisePropertyChanged(() => X);
                }
            }
        }

        public int Y
        {
            get
            {
                return this._y;
            }

            set
            {
                if (value != this._y)
                {
                    this._y = value;
                    this.RaisePropertyChanged(() => Y);
                }
            }
        }

        public int Z
        {
            get
            {
                return this._z;
            }

            set
            {
                if (value != this._z)
                {
                    this._z = value;
                    this.RaisePropertyChanged(() => Z);
                }
            }
        }

        #endregion

        #region methods

        public VRageMath.Vector3I ToVector3I()
        {
            return new VRageMath.Vector3I(this.X, this.Y, this.Z);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", this.X, this.Y, this.Z);
        }

        #endregion
    }
}
