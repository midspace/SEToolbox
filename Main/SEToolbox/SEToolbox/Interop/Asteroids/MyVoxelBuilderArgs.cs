namespace SEToolbox.Interop.Asteroids
{
    using System;
    using VRageMath;

    public class MyVoxelBuilderArgs : EventArgs
    {
        private readonly Vector3I _size;
        private readonly Vector3I _coordinatePoint;

        public MyVoxelBuilderArgs(Vector3I size, Vector3I coordinatePoint, string material, byte volume, byte indestructible)
        {
            this._size = size;
            this._coordinatePoint = coordinatePoint;
            this.Material = material;
            this.Volume = volume;
            this.Indestructible = indestructible;
        }

        public Vector3I Size { get { return this._size; } }
        public Vector3I CoordinatePoint { get { return this._coordinatePoint; } }
        public string Material { get; set; }
        public byte Volume { get; set; }
        public byte Indestructible { get; set; }
    }
}
