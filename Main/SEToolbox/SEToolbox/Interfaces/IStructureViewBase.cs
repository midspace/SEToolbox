namespace SEToolbox.Interfaces
{
    public interface IStructureViewBase
    {
        bool IsSelected { get; set; }

        IStructureBase DataModel { get; }
    }
}
