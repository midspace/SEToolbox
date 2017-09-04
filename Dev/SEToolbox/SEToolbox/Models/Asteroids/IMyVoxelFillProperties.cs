namespace SEToolbox.Models.Asteroids
{
    public interface IMyVoxelFillProperties
    {
        int Index { get; set; }
        GenerateVoxelDetailModel VoxelFile { get; set; }
        IMyVoxelFillProperties Clone();
    }
}
