//namespace VRageMath
//{
//    using System;
//    using System.Globalization;
//    using ProtoBuf;

//    [ProtoContract]
//    [Serializable]
//    public struct QuaternionD : IEquatable<QuaternionD>
//    {
//        #region fields

//        public static QuaternionD Identity;

//        [ProtoMember(1)]
//        public double X;

//        [ProtoMember(2)]
//        public double Y;

//        [ProtoMember(3)]
//        public double Z;

//        [ProtoMember(4)]
//        public double W;

//        #endregion

//        #region ctor

//        static QuaternionD()
//        {
//            Identity = new QuaternionD(0d, 0d, 0d, 1d);
//        }

//        public QuaternionD(double x, double y, double z, double w)
//        {
//            X = x;
//            Y = y;
//            Z = z;
//            W = w;
//        }

//        public QuaternionD(Vector3 vectorPart, double scalarPart)
//        {
//            X = vectorPart.X;
//            Y = vectorPart.Y;
//            Z = vectorPart.Z;
//            W = scalarPart;
//        }

//        public static QuaternionD operator -(QuaternionD quaternion)
//        {
//            QuaternionD result;
//            result.X = -quaternion.X;
//            result.Y = -quaternion.Y;
//            result.Z = -quaternion.Z;
//            result.W = -quaternion.W;
//            return result;
//        }

//        #endregion

//        #region operators

//        public static bool operator ==(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            return quaternion1.X == quaternion2.X && quaternion1.Y == quaternion2.Y && quaternion1.Z == quaternion2.Z && quaternion1.W == quaternion2.W;
//        }

//        public static bool operator !=(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            return quaternion1.X != quaternion2.X || quaternion1.Y != quaternion2.Y || quaternion1.Z != quaternion2.Z || quaternion1.W != quaternion2.W;
//        }

//        public static QuaternionD operator +(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            QuaternionD result;
//            result.X = quaternion1.X + quaternion2.X;
//            result.Y = quaternion1.Y + quaternion2.Y;
//            result.Z = quaternion1.Z + quaternion2.Z;
//            result.W = quaternion1.W + quaternion2.W;
//            return result;
//        }

//        public static QuaternionD operator -(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            QuaternionD result;
//            result.X = quaternion1.X - quaternion2.X;
//            result.Y = quaternion1.Y - quaternion2.Y;
//            result.Z = quaternion1.Z - quaternion2.Z;
//            result.W = quaternion1.W - quaternion2.W;
//            return result;
//        }

//        public static QuaternionD operator *(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            double x = quaternion1.X;
//            double y = quaternion1.Y;
//            double z = quaternion1.Z;
//            double w = quaternion1.W;
//            double x2 = quaternion2.X;
//            double y2 = quaternion2.Y;
//            double z2 = quaternion2.Z;
//            double w2 = quaternion2.W;
//            double num = (y * z2 - z * y2);
//            double num2 = (z * x2 - x * z2);
//            double num3 = (x * y2 - y * x2);
//            double num4 = (x * x2 + y * y2 + z * z2);
//            QuaternionD result;
//            result.X = (x * w2 + x2 * w) + num;
//            result.Y = (y * w2 + y2 * w) + num2;
//            result.Z = (z * w2 + z2 * w) + num3;
//            result.W = w * w2 - num4;
//            return result;
//        }
//        public static QuaternionD operator *(QuaternionD quaternion1, double scaleFactor)
//        {
//            QuaternionD result;
//            result.X = quaternion1.X * scaleFactor;
//            result.Y = quaternion1.Y * scaleFactor;
//            result.Z = quaternion1.Z * scaleFactor;
//            result.W = quaternion1.W * scaleFactor;
//            return result;
//        }

//        public static QuaternionD operator /(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            double x = quaternion1.X;
//            double y = quaternion1.Y;
//            double z = quaternion1.Z;
//            double w = quaternion1.W;
//            double num = 1d / (quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W);
//            double num2 = -quaternion2.X * num;
//            double num3 = -quaternion2.Y * num;
//            double num4 = -quaternion2.Z * num;
//            double num5 = quaternion2.W * num;
//            double num6 = (y * num4 - z * num3);
//            double num7 = (z * num2 - x * num4);
//            double num8 = (x * num3 - y * num2);
//            double num9 = (x * num2 + y * num3 + z * num4);
//            QuaternionD result;
//            result.X = (x * num5 + num2 * w) + num6;
//            result.Y = (y * num5 + num3 * w) + num7;
//            result.Z = (z * num5 + num4 * w) + num8;
//            result.W = w * num5 - num9;
//            return result;
//        }

//        #endregion

//        #region overrides

//        public override string ToString()
//        {
//            CultureInfo currentCulture = CultureInfo.CurrentCulture;
//            return string.Format(currentCulture, "{{X:{0} Y:{1} Z:{2} W:{3}}}", new object[]
//            {
//                X.ToString(currentCulture),
//                Y.ToString(currentCulture),
//                Z.ToString(currentCulture),
//                W.ToString(currentCulture)
//            });
//        }

//        public bool Equals(QuaternionD other)
//        {
//            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
//        }

//        public override bool Equals(object obj)
//        {
//            bool result = false;
//            if (obj is QuaternionD)
//            {
//                result = Equals((QuaternionD)obj);
//            }
//            return result;
//        }

//        public override int GetHashCode()
//        {
//            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
//        }

//        #endregion

//        #region methods

//        public double LengthSquared()
//        {
//            return (X * X + Y * Y + Z * Z + W * W);
//        }

//        public double Length()
//        {
//            return Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
//        }

//        public void Normalize()
//        {
//            double num = 1d / Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
//            X *= num;
//            Y *= num;
//            Z *= num;
//            W *= num;
//        }

//        public void GetAxisAngle(out Vector3D axis, out double angle)
//        {
//            axis.X = X;
//            axis.Y = Y;
//            axis.Z = Z;
//            double num = axis.Length();
//            double w = W;
//            if (num != 0d)
//            {
//                axis.X /= num;
//                axis.Y /= num;
//                axis.Z /= num;
//            }
//            angle = Math.Atan2(num, w) * 2d;
//        }

//        public void Conjugate()
//        {
//            X = -X;
//            Y = -Y;
//            Z = -Z;
//        }

//        public MatrixD ToMatrixD()
//        {
//            return ToMatrixD(this);
//        }

//        #endregion

//        #region static methods

//        public static QuaternionD Normalize(QuaternionD quaternion)
//        {
//            double num = 1d / Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);
//            QuaternionD result;
//            result.X = quaternion.X * num;
//            result.Y = quaternion.Y * num;
//            result.Z = quaternion.Z * num;
//            result.W = quaternion.W * num;
//            return result;
//        }

//        public static void Normalize(ref QuaternionD quaternion, out QuaternionD result)
//        {
//            double num = 1d / Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);
//            result.X = quaternion.X * num;
//            result.Y = quaternion.Y * num;
//            result.Z = quaternion.Z * num;
//            result.W = quaternion.W * num;
//        }

//        public static QuaternionD Conjugate(QuaternionD value)
//        {
//            QuaternionD result;
//            result.X = -value.X;
//            result.Y = -value.Y;
//            result.Z = -value.Z;
//            result.W = value.W;
//            return result;
//        }

//        public static void Conjugate(ref QuaternionD value, out QuaternionD result)
//        {
//            result.X = -value.X;
//            result.Y = -value.Y;
//            result.Z = -value.Z;
//            result.W = value.W;
//        }

//        public static QuaternionD Inverse(QuaternionD quaternion)
//        {
//            double num = 1d / (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);
//            QuaternionD result;
//            result.X = -quaternion.X * num;
//            result.Y = -quaternion.Y * num;
//            result.Z = -quaternion.Z * num;
//            result.W = quaternion.W * num;
//            return result;
//        }

//        public static void Inverse(ref QuaternionD quaternion, out QuaternionD result)
//        {
//            double num = 1d / (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);
//            result.X = -quaternion.X * num;
//            result.Y = -quaternion.Y * num;
//            result.Z = -quaternion.Z * num;
//            result.W = quaternion.W * num;
//        }

//        public static QuaternionD CreateFromAxisAngle(Vector3 axis, double angle)
//        {
//            double num = angle * 0.5d;
//            double num2 = Math.Sin(num);
//            double w = Math.Cos(num);
//            QuaternionD result;
//            result.X = axis.X * num2;
//            result.Y = axis.Y * num2;
//            result.Z = axis.Z * num2;
//            result.W = w;
//            return result;
//        }

//        public static void CreateFromAxisAngle(ref Vector3 axis, double angle, out QuaternionD result)
//        {
//            double num = angle * 0.5d;
//            double num2 = Math.Sin(num);
//            double w = Math.Cos(num);
//            result.X = axis.X * num2;
//            result.Y = axis.Y * num2;
//            result.Z = axis.Z * num2;
//            result.W = w;
//        }

//        public static QuaternionD CreateFromYawPitchRoll(double yaw, double pitch, double roll)
//        {
//            double num = roll * 0.5d;
//            double num2 = Math.Sin(num);
//            double num3 = Math.Cos(num);
//            double num4 = pitch * 0.5d;
//            double num5 = Math.Sin(num4);
//            double num6 = Math.Cos(num4);
//            double num7 = yaw * 0.5d;
//            double num8 = Math.Sin(num7);
//            double num9 = Math.Cos(num7);
//            QuaternionD result;
//            result.X = (num9 * num5 * num3 + num8 * num6 * num2);
//            result.Y = (num8 * num6 * num3 - num9 * num5 * num2);
//            result.Z = (num9 * num6 * num2 - num8 * num5 * num3);
//            result.W = (num9 * num6 * num3 + num8 * num5 * num2);
//            return result;
//        }

//        public static void CreateFromYawPitchRoll(double yaw, double pitch, double roll, out QuaternionD result)
//        {
//            double num = roll * 0.5d;
//            double num2 = Math.Sin(num);
//            double num3 = Math.Cos(num);
//            double num4 = pitch * 0.5d;
//            double num5 = Math.Sin(num4);
//            double num6 = Math.Cos(num4);
//            double num7 = yaw * 0.5d;
//            double num8 = Math.Sin(num7);
//            double num9 = Math.Cos(num7);
//            result.X = (num9 * num5 * num3 + num8 * num6 * num2);
//            result.Y = (num8 * num6 * num3 - num9 * num5 * num2);
//            result.Z = (num9 * num6 * num2 - num8 * num5 * num3);
//            result.W = (num9 * num6 * num3 + num8 * num5 * num2);
//        }

//        public static QuaternionD CreateFromForwardUp(Vector3 forward, Vector3 up)
//        {
//            Vector3 vector = -forward;
//            Vector3 vector2 = Vector3.Cross(up, vector);
//            Vector3 vector3 = Vector3.Cross(vector, vector2);
//            double x = vector2.X;
//            double y = vector2.Y;
//            double z = vector2.Z;
//            double x2 = vector3.X;
//            double y2 = vector3.Y;
//            double z2 = vector3.Z;
//            double x3 = vector.X;
//            double y3 = vector.Y;
//            double z3 = vector.Z;
//            double num = x + y2 + z3;
//            QuaternionD result = default(QuaternionD);
//            if (num > 0d)
//            {
//                double num2 = Math.Sqrt(num + 1d);
//                result.W = num2 * 0.5d;
//                num2 = 0.5d / num2;
//                result.X = (z2 - y3) * num2;
//                result.Y = (x3 - z) * num2;
//                result.Z = (y - x2) * num2;
//                return result;
//            }
//            if (x >= y2 && x >= z3)
//            {
//                double num3 = Math.Sqrt(1d + x - y2 - z3);
//                double num4 = 0.5d / num3;
//                result.X = 0.5d * num3;
//                result.Y = (y + x2) * num4;
//                result.Z = (z + x3) * num4;
//                result.W = (z2 - y3) * num4;
//                return result;
//            }
//            if (y2 > z3)
//            {
//                double num5 = Math.Sqrt(1d + y2 - x - z3);
//                double num6 = 0.5d / num5;
//                result.X = (x2 + y) * num6;
//                result.Y = 0.5d * num5;
//                result.Z = (y3 + z2) * num6;
//                result.W = (x3 - z) * num6;
//                return result;
//            }
//            double num7 = Math.Sqrt(1d + z3 - x - y2);
//            double num8 = 0.5d / num7;
//            result.X = (x3 + z) * num8;
//            result.Y = (y3 + z2) * num8;
//            result.Z = 0.5d * num7;
//            result.W = (y - x2) * num8;
//            return result;
//        }

//        //public static QuaternionD CreateFromRotationMatrix(Matrix matrix)
//        //{
//        //    return CreateFromRotationMatrix(matrix);
//        //}

//        public static QuaternionD CreateFromRotationMatrix(MatrixD matrix)
//        {
//            double num = matrix.M11 + matrix.M22 + matrix.M33;
//            QuaternionD result = default(QuaternionD);
//            if (num > 0.0)
//            {
//                double num2 = Math.Sqrt(num + 1.0);
//                result.W = num2 * 0.5d;
//                double num3 = 0.5d / num2;
//                result.X = (matrix.M23 - matrix.M32) * num3;
//                result.Y = (matrix.M31 - matrix.M13) * num3;
//                result.Z = (matrix.M12 - matrix.M21) * num3;
//            }
//            else
//            {
//                if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
//                {
//                    double num4 = Math.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
//                    double num5 = 0.5d / num4;
//                    result.X = 0.5d * num4;
//                    result.Y = (matrix.M12 + matrix.M21) * num5;
//                    result.Z = (matrix.M13 + matrix.M31) * num5;
//                    result.W = (matrix.M23 - matrix.M32) * num5;
//                }
//                else
//                {
//                    if (matrix.M22 > matrix.M33)
//                    {
//                        double num6 = Math.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
//                        double num7 = 0.5d / num6;
//                        result.X = (matrix.M21 + matrix.M12) * num7;
//                        result.Y = 0.5d * num6;
//                        result.Z = (matrix.M32 + matrix.M23) * num7;
//                        result.W = (matrix.M31 - matrix.M13) * num7;
//                    }
//                    else
//                    {
//                        double num8 = Math.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
//                        double num9 = 0.5d / num8;
//                        result.X = (matrix.M31 + matrix.M13) * num9;
//                        result.Y = (matrix.M32 + matrix.M23) * num9;
//                        result.Z = 0.5d * num8;
//                        result.W = (matrix.M12 - matrix.M21) * num9;
//                    }
//                }
//            }
//            return result;
//        }

//        //public static void CreateFromRotationMatrix(ref Matrix matrix, out QuaternionD result)
//        //{
//        //    MatrixD matrix2 = matrix;
//        //    CreateFromRotationMatrix(ref matrix2, out result);
//        //}

//        public static void CreateFromRotationMatrix(ref MatrixD matrix, out QuaternionD result)
//        {
//            double num = matrix.M11 + matrix.M22 + matrix.M33;
//            if (num > 0.0)
//            {
//                double num2 = Math.Sqrt(num + 1.0);
//                result.W = num2 * 0.5d;
//                double num3 = 0.5d / num2;
//                result.X = (matrix.M23 - matrix.M32) * num3;
//                result.Y = (matrix.M31 - matrix.M13) * num3;
//                result.Z = (matrix.M12 - matrix.M21) * num3;
//                return;
//            }
//            if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
//            {
//                double num4 = Math.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
//                double num5 = 0.5d / num4;
//                result.X = 0.5d * num4;
//                result.Y = (matrix.M12 + matrix.M21) * num5;
//                result.Z = (matrix.M13 + matrix.M31) * num5;
//                result.W = (matrix.M23 - matrix.M32) * num5;
//                return;
//            }
//            if (matrix.M22 > matrix.M33)
//            {
//                double num6 = Math.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
//                double num7 = 0.5d / num6;
//                result.X = (matrix.M21 + matrix.M12) * num7;
//                result.Y = 0.5d * num6;
//                result.Z = (matrix.M32 + matrix.M23) * num7;
//                result.W = (matrix.M31 - matrix.M13) * num7;
//                return;
//            }
//            double num8 = Math.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
//            double num9 = 0.5d / num8;
//            result.X = (matrix.M31 + matrix.M13) * num9;
//            result.Y = (matrix.M32 + matrix.M23) * num9;
//            result.Z = 0.5d * num8;
//            result.W = (matrix.M12 - matrix.M21) * num9;
//        }

//        public static double Dot(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            return (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W);
//        }

//        public static void Dot(ref QuaternionD quaternion1, ref QuaternionD quaternion2, out double result)
//        {
//            result = (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W);
//        }

//        public static QuaternionD Slerp(QuaternionD quaternion1, QuaternionD quaternion2, double amount)
//        {
//            double num = (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W);
//            bool flag = false;
//            if (num < 0.0)
//            {
//                flag = true;
//                num = -num;
//            }
//            double num2;
//            double num3;
//            if (num > 0.999998986721039)
//            {
//                num2 = 1d - amount;
//                num3 = (flag ? (-amount) : amount);
//            }
//            else
//            {
//                double num4 = Math.Acos(num);
//                double num5 = (1.0 / Math.Sin(num4));
//                num2 = Math.Sin((1.0 - amount) * num4) * num5;
//                num3 = (flag ? ((-Math.Sin(amount * num4)) * num5) : (Math.Sin(amount * num4) * num5));
//            }
//            QuaternionD result;
//            result.X = (num2 * quaternion1.X + num3 * quaternion2.X);
//            result.Y = (num2 * quaternion1.Y + num3 * quaternion2.Y);
//            result.Z = (num2 * quaternion1.Z + num3 * quaternion2.Z);
//            result.W = (num2 * quaternion1.W + num3 * quaternion2.W);
//            return result;
//        }

//        public static void Slerp(ref QuaternionD quaternion1, ref QuaternionD quaternion2, double amount, out QuaternionD result)
//        {
//            double num = (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W);
//            bool flag = false;
//            if (num < 0.0)
//            {
//                flag = true;
//                num = -num;
//            }
//            double num2;
//            double num3;
//            if (num > 0.999998986721039)
//            {
//                num2 = 1d - amount;
//                num3 = (flag ? (-amount) : amount);
//            }
//            else
//            {
//                double num4 = Math.Acos(num);
//                double num5 = (1.0 / Math.Sin(num4));
//                num2 = Math.Sin((1.0 - amount) * num4) * num5;
//                num3 = (flag ? ((-Math.Sin(amount * num4)) * num5) : (Math.Sin(amount * num4) * num5));
//            }
//            result.X = (num2 * quaternion1.X + num3 * quaternion2.X);
//            result.Y = (num2 * quaternion1.Y + num3 * quaternion2.Y);
//            result.Z = (num2 * quaternion1.Z + num3 * quaternion2.Z);
//            result.W = (num2 * quaternion1.W + num3 * quaternion2.W);
//        }

//        public static QuaternionD Lerp(QuaternionD quaternion1, QuaternionD quaternion2, double amount)
//        {
//            double num = 1d - amount;
//            QuaternionD result = default(QuaternionD);
//            if (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W >= 0.0)
//            {
//                result.X = (num * quaternion1.X + amount * quaternion2.X);
//                result.Y = (num * quaternion1.Y + amount * quaternion2.Y);
//                result.Z = (num * quaternion1.Z + amount * quaternion2.Z);
//                result.W = (num * quaternion1.W + amount * quaternion2.W);
//            }
//            else
//            {
//                result.X = (num * quaternion1.X - amount * quaternion2.X);
//                result.Y = (num * quaternion1.Y - amount * quaternion2.Y);
//                result.Z = (num * quaternion1.Z - amount * quaternion2.Z);
//                result.W = (num * quaternion1.W - amount * quaternion2.W);
//            }
//            double num2 = 1d / Math.Sqrt(result.X * result.X + result.Y * result.Y + result.Z * result.Z + result.W * result.W);
//            result.X *= num2;
//            result.Y *= num2;
//            result.Z *= num2;
//            result.W *= num2;
//            return result;
//        }
//        public static void Lerp(ref QuaternionD quaternion1, ref QuaternionD quaternion2, double amount, out QuaternionD result)
//        {
//            double num = 1d - amount;
//            if (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W >= 0.0)
//            {
//                result.X = (num * quaternion1.X + amount * quaternion2.X);
//                result.Y = (num * quaternion1.Y + amount * quaternion2.Y);
//                result.Z = (num * quaternion1.Z + amount * quaternion2.Z);
//                result.W = (num * quaternion1.W + amount * quaternion2.W);
//            }
//            else
//            {
//                result.X = (num * quaternion1.X - amount * quaternion2.X);
//                result.Y = (num * quaternion1.Y - amount * quaternion2.Y);
//                result.Z = (num * quaternion1.Z - amount * quaternion2.Z);
//                result.W = (num * quaternion1.W - amount * quaternion2.W);
//            }
//            double num2 = 1d / Math.Sqrt(result.X * result.X + result.Y * result.Y + result.Z * result.Z + result.W * result.W);
//            result.X *= num2;
//            result.Y *= num2;
//            result.Z *= num2;
//            result.W *= num2;
//        }
//        public static QuaternionD Concatenate(QuaternionD value1, QuaternionD value2)
//        {
//            double x = value2.X;
//            double y = value2.Y;
//            double z = value2.Z;
//            double w = value2.W;
//            double x2 = value1.X;
//            double y2 = value1.Y;
//            double z2 = value1.Z;
//            double w2 = value1.W;
//            double num = (y * z2 - z * y2);
//            double num2 = (z * x2 - x * z2);
//            double num3 = (x * y2 - y * x2);
//            double num4 = (x * x2 + y * y2 + z * z2);
//            QuaternionD result;
//            result.X = (x * w2 + x2 * w) + num;
//            result.Y = (y * w2 + y2 * w) + num2;
//            result.Z = (z * w2 + z2 * w) + num3;
//            result.W = w * w2 - num4;
//            return result;
//        }
//        public static void Concatenate(ref QuaternionD value1, ref QuaternionD value2, out QuaternionD result)
//        {
//            double x = value2.X;
//            double y = value2.Y;
//            double z = value2.Z;
//            double w = value2.W;
//            double x2 = value1.X;
//            double y2 = value1.Y;
//            double z2 = value1.Z;
//            double w2 = value1.W;
//            double num = (y * z2 - z * y2);
//            double num2 = (z * x2 - x * z2);
//            double num3 = (x * y2 - y * x2);
//            double num4 = (x * x2 + y * y2 + z * z2);
//            result.X = (x * w2 + x2 * w) + num;
//            result.Y = (y * w2 + y2 * w) + num2;
//            result.Z = (z * w2 + z2 * w) + num3;
//            result.W = w * w2 - num4;
//        }
//        public static QuaternionD Negate(QuaternionD quaternion)
//        {
//            QuaternionD result;
//            result.X = -quaternion.X;
//            result.Y = -quaternion.Y;
//            result.Z = -quaternion.Z;
//            result.W = -quaternion.W;
//            return result;
//        }
//        public static void Negate(ref QuaternionD quaternion, out QuaternionD result)
//        {
//            result.X = -quaternion.X;
//            result.Y = -quaternion.Y;
//            result.Z = -quaternion.Z;
//            result.W = -quaternion.W;
//        }
//        public static QuaternionD Add(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            QuaternionD result;
//            result.X = quaternion1.X + quaternion2.X;
//            result.Y = quaternion1.Y + quaternion2.Y;
//            result.Z = quaternion1.Z + quaternion2.Z;
//            result.W = quaternion1.W + quaternion2.W;
//            return result;
//        }
//        public static void Add(ref QuaternionD quaternion1, ref QuaternionD quaternion2, out QuaternionD result)
//        {
//            result.X = quaternion1.X + quaternion2.X;
//            result.Y = quaternion1.Y + quaternion2.Y;
//            result.Z = quaternion1.Z + quaternion2.Z;
//            result.W = quaternion1.W + quaternion2.W;
//        }
//        public static QuaternionD Subtract(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            QuaternionD result;
//            result.X = quaternion1.X - quaternion2.X;
//            result.Y = quaternion1.Y - quaternion2.Y;
//            result.Z = quaternion1.Z - quaternion2.Z;
//            result.W = quaternion1.W - quaternion2.W;
//            return result;
//        }
//        public static void Subtract(ref QuaternionD quaternion1, ref QuaternionD quaternion2, out QuaternionD result)
//        {
//            result.X = quaternion1.X - quaternion2.X;
//            result.Y = quaternion1.Y - quaternion2.Y;
//            result.Z = quaternion1.Z - quaternion2.Z;
//            result.W = quaternion1.W - quaternion2.W;
//        }
//        public static QuaternionD Multiply(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            double x = quaternion1.X;
//            double y = quaternion1.Y;
//            double z = quaternion1.Z;
//            double w = quaternion1.W;
//            double x2 = quaternion2.X;
//            double y2 = quaternion2.Y;
//            double z2 = quaternion2.Z;
//            double w2 = quaternion2.W;
//            double num = (y * z2 - z * y2);
//            double num2 = (z * x2 - x * z2);
//            double num3 = (x * y2 - y * x2);
//            double num4 = (x * x2 + y * y2 + z * z2);
//            QuaternionD result;
//            result.X = (x * w2 + x2 * w) + num;
//            result.Y = (y * w2 + y2 * w) + num2;
//            result.Z = (z * w2 + z2 * w) + num3;
//            result.W = w * w2 - num4;
//            return result;
//        }
//        public static void Multiply(ref QuaternionD quaternion1, ref QuaternionD quaternion2, out QuaternionD result)
//        {
//            double x = quaternion1.X;
//            double y = quaternion1.Y;
//            double z = quaternion1.Z;
//            double w = quaternion1.W;
//            double x2 = quaternion2.X;
//            double y2 = quaternion2.Y;
//            double z2 = quaternion2.Z;
//            double w2 = quaternion2.W;
//            double num = (y * z2 - z * y2);
//            double num2 = (z * x2 - x * z2);
//            double num3 = (x * y2 - y * x2);
//            double num4 = (x * x2 + y * y2 + z * z2);
//            result.X = (x * w2 + x2 * w) + num;
//            result.Y = (y * w2 + y2 * w) + num2;
//            result.Z = (z * w2 + z2 * w) + num3;
//            result.W = w * w2 - num4;
//        }
//        public static QuaternionD Multiply(QuaternionD quaternion1, double scaleFactor)
//        {
//            QuaternionD result;
//            result.X = quaternion1.X * scaleFactor;
//            result.Y = quaternion1.Y * scaleFactor;
//            result.Z = quaternion1.Z * scaleFactor;
//            result.W = quaternion1.W * scaleFactor;
//            return result;
//        }
//        public static void Multiply(ref QuaternionD quaternion1, double scaleFactor, out QuaternionD result)
//        {
//            result.X = quaternion1.X * scaleFactor;
//            result.Y = quaternion1.Y * scaleFactor;
//            result.Z = quaternion1.Z * scaleFactor;
//            result.W = quaternion1.W * scaleFactor;
//        }
//        public static QuaternionD Divide(QuaternionD quaternion1, QuaternionD quaternion2)
//        {
//            double x = quaternion1.X;
//            double y = quaternion1.Y;
//            double z = quaternion1.Z;
//            double w = quaternion1.W;
//            double num = 1d / (quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W);
//            double num2 = -quaternion2.X * num;
//            double num3 = -quaternion2.Y * num;
//            double num4 = -quaternion2.Z * num;
//            double num5 = quaternion2.W * num;
//            double num6 = (y * num4 - z * num3);
//            double num7 = (z * num2 - x * num4);
//            double num8 = (x * num3 - y * num2);
//            double num9 = (x * num2 + y * num3 + z * num4);
//            QuaternionD result;
//            result.X = (x * num5 + num2 * w) + num6;
//            result.Y = (y * num5 + num3 * w) + num7;
//            result.Z = (z * num5 + num4 * w) + num8;
//            result.W = w * num5 - num9;
//            return result;
//        }
//        public static void Divide(ref QuaternionD quaternion1, ref QuaternionD quaternion2, out QuaternionD result)
//        {
//            double x = quaternion1.X;
//            double y = quaternion1.Y;
//            double z = quaternion1.Z;
//            double w = quaternion1.W;
//            double num = 1d / (quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W);
//            double num2 = -quaternion2.X * num;
//            double num3 = -quaternion2.Y * num;
//            double num4 = -quaternion2.Z * num;
//            double num5 = quaternion2.W * num;
//            double num6 = (y * num4 - z * num3);
//            double num7 = (z * num2 - x * num4);
//            double num8 = (x * num3 - y * num2);
//            double num9 = (x * num2 + y * num3 + z * num4);
//            result.X = (x * num5 + num2 * w) + num6;
//            result.Y = (y * num5 + num3 * w) + num7;
//            result.Z = (z * num5 + num4 * w) + num8;
//            result.W = w * num5 - num9;
//        }
//        public static QuaternionD FromVector4(Vector4 v)
//        {
//            return new QuaternionD(v.X, v.Y, v.Z, v.W);
//        }
//        //public Vector4D ToVector4()
//        //{
//        //    return new Vector4D(this.X, this.Y, this.Z, this.W);
//        //}
//        public static bool IsZero(QuaternionD value)
//        {
//            return IsZero(value, 0.0001d);
//        }
//        public static bool IsZero(QuaternionD value, double epsilon)
//        {
//            return Math.Abs(value.X) < epsilon && Math.Abs(value.Y) < epsilon && Math.Abs(value.Z) < epsilon && Math.Abs(value.W) < epsilon;
//        }

//        public static MatrixD ToMatrixD(QuaternionD value)
//        {
//            double num = value.X * value.X;
//            double num2 = value.Y * value.Y;
//            double num3 = value.Z * value.Z;
//            double num4 = value.X * value.Y;
//            double num5 = value.Z * value.W;
//            double num6 = value.Z * value.X;
//            double num7 = value.Y * value.W;
//            double num8 = value.Y * value.Z;
//            double num9 = value.X * value.W;
//            MatrixD result;
//            result.M11 = (1.0d - 2.0d * (num2 + num3));
//            result.M12 = (2.0d * (num4 + num5));
//            result.M13 = (2.0d * (num6 - num7));
//            result.M14 = 0d;
//            result.M21 = (2.0d * (num4 - num5));
//            result.M22 = (1.0d - 2.0d * (num3 + num));
//            result.M23 = (2.0d * (num8 + num9));
//            result.M24 = 0d;
//            result.M31 = (2.0d * (num6 + num7));
//            result.M32 = (2.0d * (num8 - num9));
//            result.M33 = (1.0d - 2.0d * (num2 + num));
//            result.M34 = 0d;
//            result.M41 = 0d;
//            result.M42 = 0d;
//            result.M43 = 0d;
//            result.M44 = 1d;
//            return result;
//        }
//        #endregion
//    }
//}

