namespace SEToolbox.Models
{
    using System;
    using System.Windows.Media.Media3D;

    public class BindableVector3DModel : BaseModel
    {
        #region fields

        private Vector3D vector;

        #endregion

        #region ctor

        public BindableVector3DModel()
        {
            this.vector = new Vector3D();
        }

        public BindableVector3DModel(double x, double y, double z)
            : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public BindableVector3DModel(Vector3D vector)
            : this()
        {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }

        public BindableVector3DModel(float x, float y, float z)
            : this()
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public BindableVector3DModel(VRageMath.Vector3 vector)
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
                return this.vector.X;
            }

            set
            {
                if (value != this.vector.X)
                {
                    this.vector.X = value;
                    this.RaisePropertyChanged(() => X);
                }
            }
        }

        public double Y
        {
            get
            {
                return this.vector.Y;
            }

            set
            {
                if (value != this.vector.Y)
                {
                    this.vector.Y = value;
                    this.RaisePropertyChanged(() => Y);
                }
            }
        }

        public double Z
        {
            get
            {
                return this.vector.Z;
            }

            set
            {
                if (value != this.vector.Z)
                {
                    this.vector.Z = value;
                    this.RaisePropertyChanged(() => Z);
                }
            }
        }

        public Vector3D Vector3D
        {
            get
            {
                return this.vector;
            }

            set
            {
                if (value != this.vector)
                {
                    this.vector = value;
                    this.RaisePropertyChanged(() => Vector3D, () => X, () => Y, () => Z);
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
            return this.vector.ToString();
        }

        public BindableVector3DModel Negate()
        {
            Vector3D v = this.vector;
            v.Negate();
            return new BindableVector3DModel(v);
        }

        public BindableVector3DModel RoundToAxis()
        {
            Vector3D v = new Vector3D();

            if (Math.Abs(this.vector.X) > Math.Abs(this.vector.Y) && Math.Abs(this.vector.X) > Math.Abs(this.vector.Z))
            {
                v = new Vector3D(Math.Sign(this.vector.X), 0, 0);
            }
            else if (Math.Abs(this.vector.Y) > Math.Abs(this.vector.X) && Math.Abs(this.vector.Y) > Math.Abs(this.vector.Z))
            {
                v = new Vector3D(0, Math.Sign(this.vector.Y), 0);
            }
            else if (Math.Abs(this.vector.Z) > Math.Abs(this.vector.X) && Math.Abs(this.vector.Z) > Math.Abs(this.vector.Y))
            {
                v = new Vector3D(0, 0, Math.Sign(this.vector.Z));
            }

            return new BindableVector3DModel(v);
        }

        #endregion
    }
}
