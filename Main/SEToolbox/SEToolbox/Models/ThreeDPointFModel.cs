//namespace SEToolbox.Models
//{
//    using System.Windows.Media.Media3D;

//    public class ThreeDPointFModel : BaseModel
//    {
//        private float x;
//        private float y;
//        private float z;

//        public ThreeDPointFModel()
//        {
//            this.X = 0;
//            this.Y = 0;
//            this.Z = 0;
//        }

//        public ThreeDPointFModel(float x, float y, float z)
//            : this()
//        {
//            this.X = x;
//            this.Y = y;
//            this.Z = z;
//        }

//        public ThreeDPointFModel(VRageMath.Vector3 vector)
//            : this()
//        {
//            this.X = vector.X;
//            this.Y = vector.Y;
//            this.Z = vector.Z;
//        }

//        #region Properties

//        public float X
//        {
//            get
//            {
//                return this.x;
//            }

//            set
//            {
//                if (value != this.x)
//                {
//                    this.x = value;
//                    this.RaisePropertyChanged(() => X);
//                }
//            }
//        }

//        public float Y
//        {
//            get
//            {
//                return this.y;
//            }

//            set
//            {
//                if (value != this.y)
//                {
//                    this.y = value;
//                    this.RaisePropertyChanged(() => Y);
//                }
//            }
//        }

//        public float Z
//        {
//            get
//            {
//                return this.z;
//            }

//            set
//            {
//                if (value != this.z)
//                {
//                    this.z = value;
//                    this.RaisePropertyChanged(() => Z);
//                }
//            }
//        }

//        #endregion

//        public VRageMath.Vector3 ToVector3()
//        {
//            return new VRageMath.Vector3(this.X, this.Y, this.Z);
//        }
//    }
//}
