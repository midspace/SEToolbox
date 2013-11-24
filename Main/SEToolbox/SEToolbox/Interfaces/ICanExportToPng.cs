namespace SEToolbox.Interfaces
{
    using System.IO;

    public interface ICanExportToPng
    {
        void ExportToPng(Stream stream);
    }
}