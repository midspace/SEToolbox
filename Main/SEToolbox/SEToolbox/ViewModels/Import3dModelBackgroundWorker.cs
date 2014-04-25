namespace SEToolbox.ViewModels
{
    using System;
    using System.Windows.Media.Media3D;

    [Obsolete]
    class Import3dModelBackgroundWorker
    {
        #region ctor
        public Import3dModelBackgroundWorker(MeshGeometry3D mesh, int triangleIndex)
        {
            this.P1 = mesh.Positions[mesh.TriangleIndices[triangleIndex]];
            this.P2 = mesh.Positions[mesh.TriangleIndices[triangleIndex + 1]];
            this.P3 = mesh.Positions[mesh.TriangleIndices[triangleIndex + 2]];
        }

        #endregion

        #region properties

        public Point3D P1 { get; set; }
        public Point3D P2 { get; set; }
        public Point3D P3 { get; set; }

        #endregion
    }
}
