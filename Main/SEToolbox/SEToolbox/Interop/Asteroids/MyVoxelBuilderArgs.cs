namespace SEToolbox.Interop.Asteroids
{
    using System;
    using VRageMath;

    public class MyVoxelBuilderArgs : EventArgs
    {
        public MyVoxelBuilderArgs(Vector3I size, Vector3I coordinatePoint, byte materialIndex, byte volume)
        {
            Size = size;
            CoordinatePoint = coordinatePoint;
            MaterialIndex = materialIndex;
            Volume = volume;
        }

        /// <summary>
        /// The size of the Voxel Storage.
        /// </summary>
        public Vector3I Size { get; }

        /// <summary>
        /// The currently selected Voxel Coordinate in local space.
        /// </summary>
        public Vector3I CoordinatePoint { get; }

        /// <summary>
        /// The Material to be applied. It may already be set with the existing material.
        /// </summary>
        public byte MaterialIndex { get; set; }

        /// <summary>
        /// The Volume to be applied. It may already be set with the existing Volume.
        /// </summary>
        public byte Volume { get; set; }
    }
}
