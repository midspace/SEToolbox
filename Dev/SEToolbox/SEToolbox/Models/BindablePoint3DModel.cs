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
            _point = new Point3D();
        }

        public BindablePoint3DModel(double x, double y, double z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public BindablePoint3DModel(Point3D point)
            : this()
        {
            X = point.X;
            Y = point.Y;
            Z = point.Z;
        }

        public BindablePoint3DModel(float x, float y, float z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public BindablePoint3DModel(VRageMath.Vector3 vector)
            : this()
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        public BindablePoint3DModel(VRageMath.Vector3D vector)
            : this()
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        #endregion

        #region Properties

        public double X
        {
            get
            {
                return _point.X;
            }

            set
            {
                if (value != _point.X)
                {
                    _point.X = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public double Y
        {
            get
            {
                return _point.Y;
            }

            set
            {
                if (value != _point.Y)
                {
                    _point.Y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public double Z
        {
            get
            {
                return _point.Z;
            }

            set
            {
                if (value != _point.Z)
                {
                    _point.Z = value;
                    OnPropertyChanged(nameof(Z));
                }
            }
        }

        public Point3D Point3D
        {
            get
            {
                return _point;
            }

            set
            {
                if (value != _point)
                {
                    _point = value;
                    OnPropertyChanged(nameof(Point3D), nameof(X), nameof(Y), nameof(Z));
                }
            }
        }

        #endregion

        #region methods

        public VRageMath.Vector3 ToVector3()
        {
            return new VRageMath.Vector3(ToFloat(X), ToFloat(Y), ToFloat(Z));
        }

        public VRageMath.Vector3D ToVector3D()
        {
            return new VRageMath.Vector3D(X, Y, Z);
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
            var v = new Point3D(Math.Round(_point.X / roundTo, 0, MidpointRounding.ToEven) * roundTo, Math.Round(_point.Y / roundTo, 0, MidpointRounding.ToEven) * roundTo, Math.Round(_point.Z / roundTo, 0, MidpointRounding.ToEven) * roundTo);
            return new BindablePoint3DModel(v);
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", X, Y, Z);
        }

        #endregion
    }
}
