namespace ToolboxTest
{
    using System.Windows.Media.Media3D;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using SEToolbox.ViewModels;

    [TestClass]
    public class VolumentricTests
    {
        [TestMethod]
        public void GenerateModelComplexVolumentric()
        {
            const string modelFile = @".\TestAssets\algos.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 1, null, ModelTraceVoxel.Thin);

            var cubicCount = Modelling.CountCubic(cubic);

            Assert.AreEqual(1290600, cubic.Length, "Array length size must match.");

            Assert.AreEqual(108, cubic.GetLength(0), "Array size must match.");
            Assert.AreEqual(50, cubic.GetLength(1), "Array size must match.");
            Assert.AreEqual(239, cubic.GetLength(2), "Array size must match.");

            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 7]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 17]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 18]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 19]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 20]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 23]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 24]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 25]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 26]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 35]);
            Assert.AreEqual(CubeType.Cube, cubic[54, 39, 36]);

            Assert.AreEqual(51921, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(188293, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod]
        public void GenerateModelComplexVolumentricHalfScale()
        {
            const string modelFile = @".\TestAssets\algos.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0.5, null, ModelTraceVoxel.Thin);

            var cubicCount = Modelling.CountCubic(cubic);

            Assert.AreEqual(168480, cubic.Length, "Array length size must match.");

            Assert.AreEqual(54, cubic.GetLength(0), "Array size must match.");
            Assert.AreEqual(26, cubic.GetLength(1), "Array size must match.");
            Assert.AreEqual(120, cubic.GetLength(2), "Array size must match.");

            Assert.AreEqual(12540, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(20651, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod]
        public void GenerateModelSimpleThinVolumentric()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0, null, ModelTraceVoxel.Thin);

            var cubicCount = Modelling.CountCubic(cubic);

            Assert.AreEqual(72, cubic.Length, "Array length size must match.");

            Assert.AreEqual(4, cubic.GetLength(0), "Array size must match.");
            Assert.AreEqual(6, cubic.GetLength(1), "Array size must match.");
            Assert.AreEqual(3, cubic.GetLength(2), "Array size must match.");

            Assert.AreEqual(36, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(4, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod]
        public void GenerateModelSimpleThinSmoothedVolumentric()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0, null, ModelTraceVoxel.ThinSmoothed);

            var cubicCount = Modelling.CountCubic(cubic);

            Assert.AreEqual(72, cubic.Length, "Array length size must match.");

            Assert.AreEqual(4, cubic.GetLength(0), "Array size must match.");
            Assert.AreEqual(6, cubic.GetLength(1), "Array size must match.");
            Assert.AreEqual(3, cubic.GetLength(2), "Array size must match.");

            Assert.AreEqual(36, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(4, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod]
        public void GenerateModelSimpleThickVolumentric()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0, null, ModelTraceVoxel.Thick);

            var cubicCount = Modelling.CountCubic(cubic);

            Assert.AreEqual(72, cubic.Length, "Array length size must match.");

            Assert.AreEqual(4, cubic.GetLength(0), "Array size must match.");
            Assert.AreEqual(6, cubic.GetLength(1), "Array size must match.");
            Assert.AreEqual(3, cubic.GetLength(2), "Array size must match.");

            Assert.AreEqual(58, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(2, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod]
        public void LoadBrokenModel()
        {
            // TODO: finish testing the model.
            const string modelFile = @".\TestAssets\LibertyStatue.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0, null, ModelTraceVoxel.Thin);

            //Assert.AreEqual(72, cubic.Length, "Array length size must match.");

            //Assert.AreEqual(4, cubic.GetLength(0), "Array size must match.");
            //Assert.AreEqual(6, cubic.GetLength(1), "Array size must match.");
            //Assert.AreEqual(3, cubic.GetLength(2), "Array size must match.");
        }

        [TestMethod]
        public void GenerateModelSimpleVolumentricFill()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 2, null, ModelTraceVoxel.Thin);

            var cubicCount = Modelling.CountCubic(cubic);

            Assert.AreEqual(480, cubic.Length, "Array length size must match.");

            Assert.AreEqual(8, cubic.GetLength(0), "Array size must match.");
            Assert.AreEqual(12, cubic.GetLength(1), "Array size must match.");
            Assert.AreEqual(5, cubic.GetLength(2), "Array size must match.");

            Assert.AreEqual(168, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(48, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod]
        public void GenerateModelSimpleVolumentricAltFill()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmeticAlt(modelFile, 1);

            //Assert.AreEqual(480, cubic.Length, "Array length size must match.");

            //Assert.AreEqual(8, cubic.GetLength(0), "Array size must match.");
            //Assert.AreEqual(12, cubic.GetLength(1), "Array size must match.");
            //Assert.AreEqual(5, cubic.GetLength(2), "Array size must match.");
        }

        [TestMethod]
        public void GenerateModelWithMaterial()
        {
            const string modelFile = @".\TestAssets\test.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 1, null, ModelTraceVoxel.Thin);

            //Assert.AreEqual(480, cubic.Length, "Array length size must match.");

            //Assert.AreEqual(8, cubic.GetLength(0), "Array size must match.");
            //Assert.AreEqual(12, cubic.GetLength(1), "Array size must match.");
            //Assert.AreEqual(5, cubic.GetLength(2), "Array size must match.");
        }

        [TestMethod]
        public void IntersectionTestPoint0()
        {
            Point3D intersect;

            var p1 = new Point3D(0, 0, 0);
            var p2 = new Point3D(10.999, 0, 0);
            var p3 = new Point3D(0, 10.999, -15.999);

            var r1 = new Point3D(0, 0, -10);
            var r2 = new Point3D(0, 0, +10);

            var ret = MeshHelper.RayIntersetTriangle(p1, p2, p3, r1, r2, out intersect);

            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(intersect, new Point3D(0, 0, 0), "Point must be match.");
        }

        [TestMethod]
        public void IntersectionTestPoint1()
        {
            Point3D intersect;

            var p1 = new Point3D(1, 1, 0);
            var p2 = new Point3D(10, 1, 0);
            var p3 = new Point3D(1, 10, 0);

            var rays = new Point3D[]
                {
                    new Point3D(1, 1, -10), new Point3D(1, 1, +10)
                };

            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, rays, out intersect);

            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(intersect, new Point3D(1,1,0), "Point must be match.");
        }
    }
}
