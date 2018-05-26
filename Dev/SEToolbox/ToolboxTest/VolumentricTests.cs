namespace ToolboxTest
{
    using System.Diagnostics;
    using System.Windows.Media.Media3D;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox.Interop;
    using SEToolbox.Support;

    [TestClass]
    public class VolumentricTests
    {
        [TestMethod, TestCategory("UnitTest")]
        public void GenerateModelComplexVolumentric()
        {
            const string modelFile = @".\TestAssets\algos.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 1, null, ModelTraceVoxel.Thin);

            var cubicCount = Modelling.CountCubic(cubic);

            var size = cubic.Length * cubic[0].Length * cubic[0][0].Length;
            Assert.AreEqual(1290600, size, "Array length size must match.");

            Assert.AreEqual(108, cubic.Length, "Array size must match.");
            Assert.AreEqual(50, cubic[0].Length, "Array size must match.");
            Assert.AreEqual(239, cubic[0][0].Length, "Array size must match.");

            Assert.AreEqual(CubeType.Cube, cubic[54][39][7]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][17]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][18]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][19]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][20]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][23]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][24]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][25]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][26]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][35]);
            Assert.AreEqual(CubeType.Cube, cubic[54][39][36]);

            Assert.AreEqual(51921, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(188293, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GenerateModelComplexVolumentricHalfScale()
        {
            const string modelFile = @".\TestAssets\algos.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0.5, null, ModelTraceVoxel.Thin);

            var cubicCount = Modelling.CountCubic(cubic);

            var size = cubic.Length * cubic[0].Length * cubic[0][0].Length;
            Assert.AreEqual(168480, size, "Array length size must match.");

            Assert.AreEqual(54, cubic.Length, "Array size must match.");
            Assert.AreEqual(26, cubic[0].Length, "Array size must match.");
            Assert.AreEqual(120, cubic[0][0].Length, "Array size must match.");

            Assert.AreEqual(12540, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(20651, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GenerateModelSimpleThinVolumentric()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0, null, ModelTraceVoxel.Thin);

            var cubicCount = Modelling.CountCubic(cubic);

            var size = cubic.Length * cubic[0].Length * cubic[0][0].Length;
            Assert.AreEqual(72, size, "Array length size must match.");

            Assert.AreEqual(4, cubic.Length, "Array size must match.");
            Assert.AreEqual(6, cubic[0].Length, "Array size must match.");
            Assert.AreEqual(3, cubic[0][0].Length, "Array size must match.");

            Assert.AreEqual(36, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(4, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GenerateModelSimpleThinSmoothedVolumentric()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0, null, ModelTraceVoxel.ThinSmoothed);

            var cubicCount = Modelling.CountCubic(cubic);

            var size = cubic.Length * cubic[0].Length * cubic[0][0].Length;
            Assert.AreEqual(72, size, "Array length size must match.");

            Assert.AreEqual(4, cubic.Length, "Array size must match.");
            Assert.AreEqual(6, cubic[0].Length, "Array size must match.");
            Assert.AreEqual(3, cubic[0][0].Length, "Array size must match.");

            Assert.AreEqual(36, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(4, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GenerateModelSimpleThickVolumentric()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0, null, ModelTraceVoxel.Thick);

            var cubicCount = Modelling.CountCubic(cubic);

            var size = cubic.Length * cubic[0].Length * cubic[0][0].Length;
            Assert.AreEqual(72, size, "Array length size must match.");

            Assert.AreEqual(4, cubic.Length, "Array size must match.");
            Assert.AreEqual(6, cubic[0].Length, "Array size must match.");
            Assert.AreEqual(3, cubic[0][0].Length, "Array size must match.");

            Assert.AreEqual(58, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(2, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void LoadBrokenModel()
        {
            // TODO: finish testing the model.
            const string modelFile = @".\TestAssets\LibertyStatue.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 0, null, ModelTraceVoxel.Thin);

            //var size = cubic.Length * cubic[0].Length * cubic[0][0].Length;
            //Assert.AreEqual(72, size, "Array length size must match.");

            //Assert.AreEqual(4, cubic.Length, "Array size must match.");
            //Assert.AreEqual(6, cubic[0].Length, "Array size must match.");
            //Assert.AreEqual(3, cubic[0][0].Length, "Array size must match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GenerateModelSimpleVolumentricFill()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 2, null, ModelTraceVoxel.Thin);

            var cubicCount = Modelling.CountCubic(cubic);

            var size = cubic.Length * cubic[0].Length * cubic[0][0].Length;
            Assert.AreEqual(480, size, "Array length size must match.");

            Assert.AreEqual(8, cubic.Length, "Array size must match.");
            Assert.AreEqual(12, cubic[0].Length, "Array size must match.");
            Assert.AreEqual(5, cubic[0][0].Length, "Array size must match.");

            Assert.AreEqual(168, cubicCount[CubeType.Cube], "Cube count must match.");
            Assert.AreEqual(48, cubicCount[CubeType.Interior], "Interior count must match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GenerateModelSimpleVolumentricAltFill()
        {
            const string modelFile = @".\TestAssets\t25.obj";

            var cubic = Modelling.ReadModelVolmeticAlt(modelFile, 1);

            //var size = cubic.Length * cubic[0].Length * cubic[0][0].Length;
            //Assert.AreEqual(480, size, "Array length size must match.");

            //Assert.AreEqual(8, cubic.Length, "Array size must match.");
            //Assert.AreEqual(12, cubic[0].Length, "Array size must match.");
            //Assert.AreEqual(5, cubic[0][0].Length, "Array size must match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void GenerateModelWithMaterial()
        {
            const string modelFile = @".\TestAssets\test.obj";

            var cubic = Modelling.ReadModelVolmetic(modelFile, 1, null, ModelTraceVoxel.Thin);

            //var size = cubic.Length * cubic[0].Length * cubic[0][0].Length;
            //Assert.AreEqual(480, size, "Array length size must match.");

            //Assert.AreEqual(8, cubic.Length, "Array size must match.");
            //Assert.AreEqual(12, cubic[0].Length, "Array size must match.");
            //Assert.AreEqual(5, cubic[0][0].Length, "Array size must match.");
        }

        private static long _counter;
        private static long _maximumProgress;
        private static int _percent;
        public void ResetProgress(long initial, long maximumProgress)
        {
            _percent = 0;
            _counter = initial;
            _maximumProgress = maximumProgress;
        }

        public void IncrementProgress()
        {
            _counter++;

            var p = (int)((double)_counter / _maximumProgress * 100);
            if (_percent < p)
            {
                _percent = p;
                Debug.WriteLine("{0}%", _percent);
            }
        }

        [TestMethod, TestCategory("UnitTest")]
        public void IntersectionTestPoint0()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(0, 0, 0);
            var p2 = new Point3D(10.999, 0, 0);
            var p3 = new Point3D(0, 10.999, -15.999);

            var r1 = new Point3D(0, 0, -10);
            var r2 = new Point3D(0, 0, +10);

            var ret = MeshHelper.RayIntersetTriangle(p1, p2, p3, r1, r2, out intersection, out normal);

            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(new Point3D(0, 0, 0), intersection, "Point must be match.");
            Assert.AreEqual(1, normal, "Normal must be 1.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void IntersectionTestPoint1()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(1, 1, 0);
            var p2 = new Point3D(10, 1, 0);
            var p3 = new Point3D(1, 10, 0);

            var rays = new Point3D[]
                {
                    new Point3D(1, 1, -10), new Point3D(1, 1, +10)
                };

            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, rays, out intersection, out normal);

            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(new Point3D(1, 1, 0), intersection, "Point must be match.");
            Assert.AreEqual(1, normal, "Normal must be 1.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestFace()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(10, 10, 10);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(20, 10, 12);

            var r1 = new Point3D(15, 12, 0);
            var r2 = new Point3D(15, 12, 20);
            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(new Point3D(15, 12, 11), intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestFaceReverse()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(20, 10, 12);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(10, 10, 10);

            var r1 = new Point3D(15, 12, 0);
            var r2 = new Point3D(15, 12, 20);
            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(new Point3D(15, 12, 11), intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestEdge()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(10, 10, 10);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(20, 10, 12);

            var r1 = new Point3D(14, 14, 0);
            var r2 = new Point3D(14, 14, 20);
            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(new Point3D(14, 14, 10.8), intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestEdgeReverse()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(20, 10, 12);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(10, 10, 10);

            var r1 = new Point3D(14, 14, 0);
            var r2 = new Point3D(14, 14, 20);
            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(new Point3D(14, 14, 10.8), intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestVertex1()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(10, 10, 10);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(20, 10, 12);
            var r1 = new Point3D(p1.X, p1.Y, 0);
            var r2 = new Point3D(p1.X, p1.Y, 20);

            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(p1, intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestVertex2()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(10, 10, 10);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(20, 10, 12);
            var r1 = new Point3D(p2.X, p2.Y, 0);
            var r2 = new Point3D(p2.X, p2.Y, 20);

            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(p2, intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestVertex3()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(10, 10, 10);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(20, 10, 12);
            var r1 = new Point3D(p3.X, p3.Y, 0);
            var r2 = new Point3D(p3.X, p3.Y, 20);

            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(p3, intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestVertexReverse1()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(20, 10, 12);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(10, 10, 10);
            var r1 = new Point3D(p1.X, p1.Y, 0);
            var r2 = new Point3D(p1.X, p1.Y, 20);

            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(p1, intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestVertexReverse2()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(20, 10, 12);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(10, 10, 10);
            var r1 = new Point3D(p2.X, p2.Y, 0);
            var r2 = new Point3D(p2.X, p2.Y, 20);

            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(p2, intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestVertexReverse3()
        {
            Point3D intersection;
            int normal;

            var p1 = new Point3D(20, 10, 12);
            var p2 = new Point3D(15, 15, 11);
            var p3 = new Point3D(10, 10, 10);
            var r1 = new Point3D(p3.X, p3.Y, 0);
            var r2 = new Point3D(p3.X, p3.Y, 20);

            var ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(p3, intersection, "intersection must be match.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestNormalCheck()
        {
            Point3D p1, p2, p3, r1, r2, intersection;
            int normal;
            bool ret;

            r1 = new Point3D(15, 12, 2000);
            r2 = new Point3D(15, 12, -2000);

            p1 = new Point3D(10, 10, 10);
            p2 = new Point3D(15, 15, 11);
            p3 = new Point3D(20, 10, 12);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, 1, "Normal must be 1.");

            p1 = new Point3D(10, 10, -990);
            p2 = new Point3D(15, 15, 11);
            p3 = new Point3D(20, 10, 1012);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, 1, "Normal must be 1.");

            p1 = new Point3D(10, 10, 1010);
            p2 = new Point3D(15, 15, 11);
            p3 = new Point3D(20, 10, -990);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, 1, "Normal must be 1.");

            // reverse ray
            r1 = new Point3D(15, 12, -2000);
            r2 = new Point3D(15, 12, 2000);

            // reverse normal
            p3 = new Point3D(10, 10, 10);
            p2 = new Point3D(15, 15, 11);
            p1 = new Point3D(20, 10, 12);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, 1, "Normal must be 1.");

            p3 = new Point3D(10, 10, -990);
            p2 = new Point3D(15, 15, 11);
            p1 = new Point3D(20, 10, 1012);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, 1, "Normal must be 1.");

            p3 = new Point3D(10, 10, 1010);
            p2 = new Point3D(15, 15, 11);
            p1 = new Point3D(20, 10, -990);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, 1, "Normal must be 1.");
        }

        [TestMethod, TestCategory("UnitTest")]
        public void RayTestNormalInverseCheck()
        {
            Point3D p1, p2, p3, r1, r2, intersection;
            int normal;
            bool ret;

            r1 = new Point3D(15, 12, -2000);
            r2 = new Point3D(15, 12, 2000);

            p1 = new Point3D(10, 10, 10);
            p2 = new Point3D(15, 15, 11);
            p3 = new Point3D(20, 10, 12);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, -1, "Normal must be -1.");

            p1 = new Point3D(10, 10, -990);
            p2 = new Point3D(15, 15, 11);
            p3 = new Point3D(20, 10, 1012);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, -1, "Normal must be -1.");

            p1 = new Point3D(10, 10, 1010);
            p2 = new Point3D(15, 15, 11);
            p3 = new Point3D(20, 10, -990);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, -1, "Normal must be -1.");

            // reverse ray
            r1 = new Point3D(15, 12, 2000);
            r2 = new Point3D(15, 12, -2000);

            // reverse normal
            p3 = new Point3D(10, 10, 10);
            p2 = new Point3D(15, 15, 11);
            p1 = new Point3D(20, 10, 12);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, -1, "Normal must be -1.");

            p3 = new Point3D(10, 10, -990);
            p2 = new Point3D(15, 15, 11);
            p1 = new Point3D(20, 10, 1012);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, -1, "Normal must be -1.");

            p3 = new Point3D(10, 10, 1010);
            p2 = new Point3D(15, 15, 11);
            p1 = new Point3D(20, 10, -990);
            ret = MeshHelper.RayIntersetTriangleRound(p1, p2, p3, r1, r2, out intersection, out normal);
            Assert.AreEqual(true, ret, "ret must be true.");
            Assert.AreEqual(normal, -1, "Normal must be -1.");
        }
    }
}
