namespace SEToolbox.Models.Asteroids
{
    using SEToolbox.Interop.Asteroids;
    using System.Collections.Generic;

    public interface IMyVoxelFiller
    {
        void FillAsteroid(MyVoxelMap asteroid, IMyVoxelFillProperties fillProperties);

        IMyVoxelFillProperties CreateRandom(int index, MaterialSelectionModel defaultMaterial, IEnumerable<MaterialSelectionModel> materialsCollection, IEnumerable<GenerateVoxelDetailModel> voxelCollection);
    }
}
