namespace SEToolbox.Services
{
    using SEToolbox.Interfaces;

    /// <summary>
    /// ViewModel of the FolderBrowserDialog.
    /// </summary>
    public class FolderBrowserDialogViewModel : IFolderBrowserDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderBrowserDialogViewModel"/> class.
        /// </summary>
        public FolderBrowserDialogViewModel()
        {
            // Set default values
            Description = string.Empty;
            SelectedPath = string.Empty;
            ShowNewFolderButton = true;
        }

        public string Description { get; set; }

        public string SelectedPath { get; set; }

        public bool ShowNewFolderButton { get; set; }
    }
}
