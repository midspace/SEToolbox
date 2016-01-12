namespace SEToolbox.Interop.Asteroids
{
    using VRageMath;

    class MyVoxelTaskWorker
    {
        #region ctor

        public MyVoxelTaskWorker(Vector3I baseCoords)
        {
            BaseCoords = baseCoords;
        }

        #endregion

        #region properties

        public Vector3I BaseCoords { get; set; }

        #endregion
    }
}
