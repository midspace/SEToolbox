namespace SEToolbox.Interop
{
    using System;
    using Sandbox.CommonLib.ObjectBuilders.VRageData;
    using SEToolbox.Support;
    using VRageMath;

    /// <summary>
    /// Contains Extension methods specifically for Keen classes and structures.
    /// </summary>
    public static class InteropHelper
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

        internal static double LinearVector(this Vector3 vector)
        {
            return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2) + Math.Pow(vector.Z, 2));
        }

        public static Vector3I ToVector3I(this SerializableVector3I vector)
        {
            return new Vector3I(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 ToVector3(this SerializableVector3I vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 ToVector3(this SerializableVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 Transform(this Vector3 vector, CubeType axisCubeType)
        {
            var orientation = SpaceEngineersAPI.CubeOrientations[axisCubeType];
            var matrix = Matrix.CreateFromDir(Base6Directions.GetVector(orientation.Forward), Base6Directions.GetVector(orientation.Up));
            return Vector3.Transform(vector, matrix);
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
            else if (Math.Abs(vector.Y) > Math.Abs(vector.X) && Math.Abs(vector.Y) > Math.Abs(vector.Z))
            {
                return new SerializableVector3(0, Math.Sign(vector.Y), 0);
            }
            else if (Math.Abs(vector.Z) > Math.Abs(vector.X) && Math.Abs(vector.Z) > Math.Abs(vector.Y))
            {
                return new SerializableVector3(0, 0, Math.Sign(vector.Z));
            }

            return new SerializableVector3();
        }
    }
}
