namespace SEToolbox.Models
{
    using System;
    using System.Windows.Media.Media3D;

    public class BindablePoint3DModel : BaseModel
    {
        #region fields

        private Point3D _point;

        #endregion

        #region ctor

        public BindablePoint3DModel()
        {
            this._point = new Point3D();
        }

        public BindablePoint3DModel(double x, double y, double z)
            : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public BindablePoint3DModel(Point3D point)
            : this()
        {
            this.X = point.X;
            this.Y = point.Y;
            this.Z = point.Z;
        }

        public BindablePoint3DModel(float x, float y, float z)
            : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public BindablePoint3DModel(VRageMath.Vector3 vector)
            : this()
        {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }

        #endregion

        #region Properties

        public double X
        {
            get
            {
                return this._point.X;
            }

            set
            {
                if (value != this._point.X)
                {
                    this._point.X = value;
                    this.RaisePropertyChanged(() => X);
                }
            }
        }

        public double Y
        {
            get
            {
                return this._point.Y;
            }

            set
            {
                if (value != this._point.Y)
                {
                    this._point.Y = value;
                    this.RaisePropertyChanged(() => Y);
                }
            }
        }

        public double Z
        {
            get
            {
                return this._point.Z;
            }

            set
            {
                if (value != this._point.Z)
                {
                    this._point.Z = value;
                    this.RaisePropertyChanged(() => Z);
                }
            }
        }

        public Point3D Point3D
        {
            get
            {
                return this._point;
            }

            set
            {
                if (value != this._point)
                {
                    this._point = value;
                    this.RaisePropertyChanged(() => Point3D, () => X, () => Y, () => Z);
                }
            }
        }

        #endregion

        #region methods

        public VRageMath.Vector3 ToVector3()
        {
            return new VRageMath.Vector3(ToFloat(this.X), ToFloat(this.Y), ToFloat(this.Z));
        }

        private float ToFloat(double value)
        {
            var result = (float)value;
            if (float.IsPositiveInfinity(result))
            {
                result = float.MaxValue;
            }
            else if (float.IsNegativeInfinity(result))
            {
                result = float.MinValue;
            }
            return result;
        }

        public BindablePoint3DModel RoundOff(double roundTo)
        {
            var v = new Point3D(Math.Round(this._point.X / roundTo, 0, MidpointRounding.ToEven) * roundTo, Math.Round(this._point.Y / roundTo, 0, MidpointRounding.ToEven) * roundTo, Math.Round(this._point.Z / roundTo, 0, MidpointRounding.ToEven) * roundTo);
            return new BindablePoint3DModel(v);
        }

        #endregion
    }
}
