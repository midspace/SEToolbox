namespace SEToolbox.Interop
{
    using System;
    using System.Globalization;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Support;
    using VRage;
    using VRageMath;

    /// <summary>
    /// Contains Extension methods specifically for Keen classes and structures.
    /// </summary>
    public static class SpaceEngineersExtensions
    {
        internal static SerializableVector3I Mirror(this SerializableVector3I vector, Mirror xMirror, int xAxis, Mirror yMirror, int yAxis, Mirror zMirror, int zAxis)
        {
            var newVector = new Vector3I(vector.X, vector.Y, vector.Z);
            switch (xMirror)
            {
                case Support.Mirror.Odd: newVector.X = xAxis - (vector.X - xAxis); break;
                case Support.Mirror.EvenUp: newVector.X = xAxis - (vector.X - xAxis) + 1; break;
                case Support.Mirror.EvenDown: newVector.X = xAxis - (vector.X - xAxis) - 1; break;
            }
            switch (yMirror)
            {
                case Support.Mirror.Odd: newVector.Y = yAxis - (vector.Y - yAxis); break;
                case Support.Mirror.EvenUp: newVector.Y = yAxis - (vector.Y - yAxis) + 1; break;
                case Support.Mirror.EvenDown: newVector.Y = yAxis - (vector.Y - yAxis) - 1; break;
            }
            switch (zMirror)
            {
                case Support.Mirror.Odd: newVector.Z = zAxis - (vector.Z - zAxis); break;
                case Support.Mirror.EvenUp: newVector.Z = zAxis - (vector.Z - zAxis) + 1; break;
                case Support.Mirror.EvenDown: newVector.Z = zAxis - (vector.Z - zAxis) - 1; break;
            }
            return newVector;
        }

        public static double LinearVector(this Vector3 vector)
        {
            return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2) + Math.Pow(vector.Z, 2));
        }

        public static Vector3I ToVector3I(this SerializableVector3I vector)
        {
            return new Vector3I(vector.X, vector.Y, vector.Z);
        }

        public static Vector3I RoundToVector3I(this Vector3 vector)
        {
            return new Vector3I((int)Math.Round(vector.X, 0, MidpointRounding.ToEven), (int)Math.Round(vector.Y, 0, MidpointRounding.ToEven), (int)Math.Round(vector.Z, 0, MidpointRounding.ToEven));
        }

        public static Vector3 ToVector3(this SerializableVector3I vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 ToVector3(this SerializableVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector3I SizeInt(this BoundingBox box)
        {
            var size = box.Size();
            return new Vector3I((int)size.X, (int)size.Y, (int)size.Z);
        }

        public static System.Windows.Media.Media3D.Vector3D ToVector3D(this SerializableVector3 vector)
        {
            return new System.Windows.Media.Media3D.Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static System.Windows.Media.Media3D.Vector3D ToVector3D(this Vector3 vector)
        {
            return new System.Windows.Media.Media3D.Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static System.Windows.Media.Media3D.Point3D ToPoint3D(this Vector3 vector)
        {
            return new System.Windows.Media.Media3D.Point3D(vector.X, vector.Y, vector.Z);
        }

        public static System.Windows.Media.Media3D.Point3D ToPoint3D(this SerializableVector3 point)
        {
            return new System.Windows.Media.Media3D.Point3D(point.X, point.Y, point.Z);
        }

        public static System.Windows.Point ToPoint(this Vector2 vector)
        {
            return new System.Windows.Point(vector.X, vector.Y);
        }

        public static Vector3 ToVector3(this System.Windows.Media.Media3D.Point3D point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Vector3 ToVector3(this System.Windows.Media.Media3D.Size3D size3D)
        {
            return new Vector3((float)size3D.X, (float)size3D.Y, (float)size3D.Z);
        }

        public static Quaternion ToQuaternion(this SerializableBlockOrientation blockOrientation)
        {
            var matrix = Matrix.CreateFromDir(Base6Directions.GetVector(blockOrientation.Forward), Base6Directions.GetVector(blockOrientation.Up));
            return Quaternion.CreateFromRotationMatrix(matrix);
        }

        public static Quaternion ToQuaternion(this MyPositionAndOrientation positionOrientation)
        {
            return Quaternion.CreateFromForwardUp(positionOrientation.Forward, positionOrientation.Up);
        }

        public static Matrix ToMatrix(this MyPositionAndOrientation positionOrientation)
        {
            return Matrix.CreateFromQuaternion(Quaternion.CreateFromForwardUp(positionOrientation.Forward, positionOrientation.Up));
        }

        public static Matrix ToMatrix(this Quaternion quaternion)
        {
            return Matrix.CreateFromQuaternion(quaternion);
        }

        public static Vector3 Transform(this Vector3 vector, SerializableBlockOrientation orientation)
        {
            var matrix = Matrix.CreateFromDir(Base6Directions.GetVector(orientation.Forward), Base6Directions.GetVector(orientation.Up));
            return Vector3.Transform(vector, matrix);
        }

        public static Vector3I Transform(this SerializableVector3I size, SerializableBlockOrientation orientation)
        {
            var matrix = Matrix.CreateFromDir(Base6Directions.GetVector(orientation.Forward), Base6Directions.GetVector(orientation.Up));
            var rotation = Quaternion.CreateFromRotationMatrix(matrix);
            return Vector3I.Transform(size.ToVector3I(), rotation);
        }

        public static SerializableVector3I Add(this SerializableVector3I size, int value)
        {
            return new SerializableVector3I(size.X + value, size.Y + value, size.Z + value);
        }

        public static Vector3I Abs(this Vector3I size)
        {
            return new Vector3I(Math.Abs(size.X), Math.Abs(size.Y), Math.Abs(size.Z));
        }

        public static SerializableVector3 RoundOff(this SerializableVector3 vector, float roundTo)
        {
            return new SerializableVector3((float)Math.Round(vector.X / roundTo, 0, MidpointRounding.ToEven) * roundTo, (float)Math.Round(vector.Y / roundTo, 0, MidpointRounding.ToEven) * roundTo, (float)Math.Round(vector.Z / roundTo, 0, MidpointRounding.ToEven) * roundTo);
        }

        public static SerializableVector3 RoundToAxis(this SerializableVector3 vector)
        {
            if (Math.Abs(vector.X) > Math.Abs(vector.Y) && Math.Abs(vector.X) > Math.Abs(vector.Z))
            {
                return new SerializableVector3(Math.Sign(vector.X), 0, 0);
            }

            if (Math.Abs(vector.Y) > Math.Abs(vector.X) && Math.Abs(vector.Y) > Math.Abs(vector.Z))
            {
                return new SerializableVector3(0, Math.Sign(vector.Y), 0);
            }

            if (Math.Abs(vector.Z) > Math.Abs(vector.X) && Math.Abs(vector.Z) > Math.Abs(vector.Y))
            {
                return new SerializableVector3(0, 0, Math.Sign(vector.Z));
            }

            return new SerializableVector3();
        }

        // SerializableVector3 stores Color HSV in the range of H=X=0.0 to +1.0, S=Y=-1.0 to +1.0, V=Z=-1.0 to +1.0

        public static System.Drawing.Color ToSandboxDrawingColor(this SerializableVector3 hsv)
        {
            var vColor = ColorExtensions.HSVtoColor(new Vector3(hsv.X, (hsv.Y + 1f) / 2f, (hsv.Z + 1f) / 2f));
            return System.Drawing.Color.FromArgb(vColor.A, vColor.R, vColor.G, vColor.B);
        }

        public static System.Windows.Media.Color ToSandboxMediaColor(this SerializableVector3 hsv)
        {
            var vColor = ColorExtensions.HSVtoColor(new Vector3(hsv.X, (hsv.Y + 1f) / 2f, (hsv.Z + 1f) / 2f));
            return System.Windows.Media.Color.FromArgb(vColor.A, vColor.R, vColor.G, vColor.B);
        }

        public static System.Windows.Media.Color ToSandboxMediaColor(this Vector3 rgb)
        {
            var vColor = new Color(rgb);
            return System.Windows.Media.Color.FromArgb(vColor.A, vColor.R, vColor.G, vColor.B);
        }

        public static SerializableVector3 ToSandboxHsvColor(this System.Drawing.Color color)
        {
            var vColor = ColorExtensions.ColorToHSV(new Color(color.R, color.G, color.B));
            return new SerializableVector3(vColor.X, vColor.Y * 2f - 1f, vColor.Z * 2f - 1f);
        }

        public static SerializableVector3 ToSandboxHsvColor(this System.Windows.Media.Color color)
        {
            var vColor = ColorExtensions.ColorToHSV(new Color(color.R, color.G, color.B));
            return new SerializableVector3(vColor.X, vColor.Y * 2f - 1f, vColor.Z * 2f - 1f);
        }

        /// <summary>
        /// Returns block size.
        /// </summary>
        /// <remarks>see: http://spaceengineerswiki.com/index.php?title=FAQs
        /// Why are the blocks 0.5 and 2.5 meter blocks?
        /// </remarks>
        /// <param name="cubeSize"></param>
        /// <returns></returns>
        public static float ToLength(this MyCubeSize cubeSize)
        {
            switch (cubeSize)
            {
                case MyCubeSize.Large: return SpaceEngineersCore.Resources.Definitions.Configuration.CubeSizes.Large;
                case MyCubeSize.Medium: return SpaceEngineersCore.Resources.Definitions.Configuration.CubeSizes.Medium;
                case MyCubeSize.Small: return SpaceEngineersCore.Resources.Definitions.Configuration.CubeSizes.Small;
            }
            return 0f;
        }

        public static MyFixedPoint ToFixedPoint(this decimal value)
        {
            return MyFixedPoint.DeserializeString(value.ToString(CultureInfo.InvariantCulture));
        }

        public static MyFixedPoint ToFixedPoint(this double value)
        {
            return MyFixedPoint.DeserializeString(value.ToString(CultureInfo.InvariantCulture));
        }

        public static MyFixedPoint ToFixedPoint(this float value)
        {
            return MyFixedPoint.DeserializeString(value.ToString(CultureInfo.InvariantCulture));
        }

        public static MyFixedPoint ToFixedPoint(this int value)
        {
            return MyFixedPoint.DeserializeString(value.ToString(CultureInfo.InvariantCulture));
        }

        public static Vector3? IntersectsRayAt(this BoundingBox boundingBox, Vector3 position, Vector3 rayTo)
        {
            var corners = boundingBox.GetCorners();
            var tariangles = new int[][] {
                new [] {2,1,0},
                new [] {3,2,0},
                new [] {4,5,6},
                new [] {4,6,7},
                new [] {0,1,5},
                new [] {0,5,4},
                new [] {7,6,2},
                new [] {7,2,3},
                new [] {0,4,7},
                new [] {0,7,3},
                new [] {5,1,2},
                new [] {5,2,6}};

            foreach (var triangle in tariangles)
            {
                System.Windows.Media.Media3D.Point3D intersection;
                int norm;

                if (MeshHelper.RayIntersetTriangleRound(corners[triangle[0]].ToPoint3D(), corners[triangle[1]].ToPoint3D(), corners[triangle[2]].ToPoint3D(), position.ToPoint3D(), rayTo.ToPoint3D(), out intersection, out norm))
                {
                    return intersection.ToVector3();
                }
            }

            return null;
        }

        public static SerializableVector3UByte Transform(this SerializableVector3UByte value, Quaternion rotation)
        {
            var vector = Vector3I.Transform(new Vector3I(value.X - 127, value.Y - 127, value.Z - 127), rotation);
            return new SerializableVector3UByte((byte)(vector.X + 127), (byte)(vector.Y + 127), (byte)(vector.Z + 127));
        }
    }
}
