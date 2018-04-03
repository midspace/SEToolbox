namespace SEToolbox.Interop.Asteroids
{
    using VRage.Voxels;
    using VRageMath;

    class MyVoxelTaskWorker
    {
        #region ctor

        public MyVoxelTaskWorker(Vector3I baseCoords, MyStorageData voxelCache)
        {
            BaseCoords = baseCoords;
            VoxelCache = voxelCache;
        }

        #endregion

        #region properties

        public Vector3I BaseCoords { get; set; }

        public MyStorageData VoxelCache { get; set; }

        #endregion
    }
}
