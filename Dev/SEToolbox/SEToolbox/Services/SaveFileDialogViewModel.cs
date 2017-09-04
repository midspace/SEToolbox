namespace SEToolbox.Services
{
    using SEToolbox.Interfaces;

    /// <summary>
    /// ViewModel of the SaveFileDialog.
    /// </summary>
    public class SaveFileDialogViewModel : FileDialogViewModel, ISaveFileDialog
    {
        public SaveFileDialogViewModel()
            : base()
        {
            // Set default values
            CheckFileExists = false;
            OverwritePrompt = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box will display the overwrite prompt if the specified file already exists.
        /// </summary>
        public bool OverwritePrompt { get; set; }
    }
}
