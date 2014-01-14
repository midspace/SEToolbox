namespace SEToolbox.Interop.Asteroids
{
    using VRageMath;

    class MyVoxelBackgroundWorker
    {
        #region ctor

        public MyVoxelBackgroundWorker(Vector3I baseCoords)
        {
            this.BaseCoords = baseCoords;
        }

        #endregion

        #region properties

        public Vector3I BaseCoords { get; set; }

        #endregion
    }
}
