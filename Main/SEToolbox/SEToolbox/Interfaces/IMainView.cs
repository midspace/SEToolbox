namespace SEToolbox.Interfaces
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Models;

    public interface IMainView
    {
        bool IsModified { get; set; }

        bool IsBusy { get; set; }

        void CalcDistances();

        void OptimizeModel(params IStructureViewBase[] viewModels);

        string CreateUniqueVoxelFilename(string originalFile, MyObjectBuilder_EntityBase[] additionalList);

        string CreateUniqueVoxelFilename(string originalFile);

        StructureCharacterModel ThePlayerCharacter { get; }

        double Progress { get; set; }

        void ResetProgress(double initial, double maximumProgress);

        void ClearProgress();
    }
}
