namespace SEToolbox.Interop.Asteroids
{
    using VRage.Voxels;
    using VRageMath;

    class MyVoxelTaskWorker
    {
        public Vector3I BaseCoords { get; set; }
        public MyStorageData VoxelCache { get; set; }

        public MyVoxelTaskWorker(Vector3I baseCoords, MyStorageData voxelCache)
        {
            BaseCoords = baseCoords;
            VoxelCache = voxelCache;
        }
    }
}
