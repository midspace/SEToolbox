namespace SEToolbox.Interfaces
{
    /// <summary>
    /// Interface describing the SaveFileDialog.
    /// </summary>
    public interface ISaveFileDialog : IFileDialog
    {
        /// <summary>
        /// Gets or sets a value indicating whether the dialog box will display the overwrite prompt if the specified file already exists.
        /// </summary>
        bool OverwritePrompt { get; set; }
    }
}
