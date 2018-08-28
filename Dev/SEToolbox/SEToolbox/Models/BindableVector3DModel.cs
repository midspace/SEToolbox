namespace SEToolbox.Models
{
    using System;
    using System.Windows.Media.Media3D;

    public class BindableVector3DModel : BaseModel
    {
        #region fields

        private Vector3D _vector;

        #endregion

        #region ctor

        public BindableVector3DModel()
        {
            _vector = new Vector3D();
        }

        public BindableVector3DModel(double x, double y, double z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public BindableVector3DModel(Vector3D vector)
            : this()
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }

        public BindableVector3DModel(float x, float y, float z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public BindableVector3DModel(VRageMath.Vector3 vector)
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
                return _vector.X;
            }

            set
            {
                if (value != _vector.X)
                {
                    _vector.X = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public double Y
        {
            get
            {
                return _vector.Y;
            }

            set
            {
                if (value != _vector.Y)
                {
                    _vector.Y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public double Z
        {
            get
            {
                return _vector.Z;
            }

            set
            {
                if (value != _vector.Z)
                {
                    _vector.Z = value;
                    OnPropertyChanged(nameof(Z));
                }
            }
        }

        public Vector3D Vector3D
        {
            get
            {
                return _vector;
            }

            set
            {
                if (value != _vector)
                {
                    _vector = value;
                    OnPropertyChanged(nameof(Vector3D), nameof(X), nameof(Y), nameof(Z));
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
            float result = (float)value;
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

        public override string ToString()
        {
            return _vector.ToString();
        }

        public BindableVector3DModel Negate()
        {
            Vector3D v = _vector;
            v.Negate();
            return new BindableVector3DModel(v);
        }

        public BindableVector3DModel RoundToAxis()
        {
            Vector3D v = new Vector3D();

            if (Math.Abs(_vector.X) > Math.Abs(_vector.Y) && Math.Abs(_vector.X) > Math.Abs(_vector.Z))
            {
                v = new Vector3D(Math.Sign(_vector.X), 0, 0);
            }
            else if (Math.Abs(_vector.Y) > Math.Abs(_vector.X) && Math.Abs(_vector.Y) > Math.Abs(_vector.Z))
            {
                v = new Vector3D(0, Math.Sign(_vector.Y), 0);
            }
            else if (Math.Abs(_vector.Z) > Math.Abs(_vector.X) && Math.Abs(_vector.Z) > Math.Abs(_vector.Y))
            {
                v = new Vector3D(0, 0, Math.Sign(_vector.Z));
            }

            return new BindableVector3DModel(v);
        }

        #endregion
    }
}
