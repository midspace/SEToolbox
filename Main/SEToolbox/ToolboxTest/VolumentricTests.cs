namespace ToolboxTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SEToolbox.Interop;
    using SEToolbox.ViewModels;

    [TestClass]
    public class VolumentricTests
    {
        [TestMethod]
        public void GenerateModelComplexVolumentric()
        {
            var modelFile = @".\TestAssets\algos.obj";

            var cubic = Import3dModelViewModel.ReadModelVolmetic(modelFile, 1, null, false);

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
        }

        [TestMethod]
        public void GenerateModelComplexVolumentricHalfScale()
        {
            var modelFile = @".\TestAssets\algos.obj";

            var cubic = Import3dModelViewModel.ReadModelVolmetic(modelFile, 0.5, null, false);

            Assert.AreEqual(168480, cubic.Length, "Array length size must match.");

            Assert.AreEqual(54, cubic.GetLength(0), "Array size must match.");
            Assert.AreEqual(26, cubic.GetLength(1), "Array size must match.");
            Assert.AreEqual(120, cubic.GetLength(2), "Array size must match.");
        }

        [TestMethod]
        public void GenerateModelSimpleVolumentric()
        {
            var modelFile = @".\TestAssets\t25.obj";

            var cubic = Import3dModelViewModel.ReadModelVolmetic(modelFile, 0, null, false);

            Assert.AreEqual(72, cubic.Length, "Array length size must match.");

            Assert.AreEqual(4, cubic.GetLength(0), "Array size must match.");
            Assert.AreEqual(6, cubic.GetLength(1), "Array size must match.");
            Assert.AreEqual(3, cubic.GetLength(2), "Array size must match.");
        }

        [TestMethod]
        public void LoadBrokenModel()
        {
            // TODO: finish testing the model.
            //var modelFile = @".\TestAssets\LibertyStatue.obj";

            //var cubic = Import3dModelViewModel.ReadModelVolmetic(modelFile, 0);

            //Assert.AreEqual(72, cubic.Length, "Array length size must match.");

            //Assert.AreEqual(4, cubic.GetLength(0), "Array size must match.");
            //Assert.AreEqual(6, cubic.GetLength(1), "Array size must match.");
            //Assert.AreEqual(3, cubic.GetLength(2), "Array size must match.");
        }

        [TestMethod]
        public void GenerateModelSimpleVolumentricFill()
        {
            var modelFile = @".\TestAssets\t25.obj";

            var cubic = Import3dModelViewModel.ReadModelVolmetic(modelFile, 2, null, false);

            Import3dModelViewModel.CalculateInverseCorners(cubic);
            Import3dModelViewModel.CalculateSlopes(cubic);
            Import3dModelViewModel.CalculateCorners(cubic);

            Assert.AreEqual(480, cubic.Length, "Array length size must match.");

            Assert.AreEqual(8, cubic.GetLength(0), "Array size must match.");
            Assert.AreEqual(12, cubic.GetLength(1), "Array size must match.");
            Assert.AreEqual(5, cubic.GetLength(2), "Array size must match.");
        }

        [TestMethod]
        public void GenerateModelSimpleVolumentricAltFill()
        {
            var modelFile = @".\TestAssets\t25.obj";

            var cubic = Import3dModelViewModel.ReadModelVolmeticAlt(modelFile, 1);

            //Assert.AreEqual(480, cubic.Length, "Array length size must match.");

            //Assert.AreEqual(8, cubic.GetLength(0), "Array size must match.");
            //Assert.AreEqual(12, cubic.GetLength(1), "Array size must match.");
            //Assert.AreEqual(5, cubic.GetLength(2), "Array size must match.");
        }
    }
}
