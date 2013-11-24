namespace SEToolbox.Interfaces
{
    using System.IO;

    public interface ICanExportToExcel
    {
        void ExportToExcel(Stream stream);
    }
}