namespace SEToolbox.Interfaces
{
    using Sandbox.CommonLib.ObjectBuilders;
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
    }
}
