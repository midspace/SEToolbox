namespace SEToolbox.Services
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Windows.Forms;
    using SEToolbox.Interfaces;
    using WinFormsSaveFileDialog = System.Windows.Forms.SaveFileDialog;

    /// <summary>
    /// Class wrapping System.Windows.Forms.SaveFileDialog, making it accept a ISaveFileDialog.
    /// </summary>
    public class SaveFileDialog : IDisposable
    {
        private readonly ISaveFileDialog _saveFileDialog;
        private WinFormsSaveFileDialog _concreteSaveFileDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveFileDialog"/> class.
        /// </summary>
        /// <param name="saveFileDialog">The interface of a save file dialog.</param>
        public SaveFileDialog(ISaveFileDialog saveFileDialog)
        {
            Contract.Requires(saveFileDialog != null);

            this._saveFileDialog = saveFileDialog;

            // Create concrete SaveFileDialog
            _concreteSaveFileDialog = new WinFormsSaveFileDialog
            {
                AddExtension = saveFileDialog.AddExtension,
                CheckFileExists = saveFileDialog.CheckFileExists,
                CheckPathExists = saveFileDialog.CheckPathExists,
                DefaultExt = saveFileDialog.DefaultExt,
                FileName = saveFileDialog.FileName,
                Filter = saveFileDialog.Filter,
                InitialDirectory = saveFileDialog.InitialDirectory,
                Title = saveFileDialog.Title,
                OverwritePrompt = saveFileDialog.OverwritePrompt
            };
        }

        /// <summary>
        /// Runs a common dialog box with the specified owner.
        /// </summary>
        /// <param name="owner">
        /// Any object that implements System.Windows.Forms.IWin32Window that represents the top-level
        /// window that will own the modal dialog box.
        /// </param>
        /// <returns>
        /// System.Windows.Forms.DialogResult.OK if the user clicks OK in the dialog box; otherwise,
        /// System.Windows.Forms.DialogResult.Cancel.
        /// </returns>
        public DialogResult ShowDialog(IWin32Window owner)
        {
            Contract.Requires(owner != null);

            DialogResult result = _concreteSaveFileDialog.ShowDialog(owner);

            // Update ViewModel
            _saveFileDialog.FileName = _concreteSaveFileDialog.FileName;
            _saveFileDialog.FileNames = _concreteSaveFileDialog.FileNames;

            return result;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        ~SaveFileDialog()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_concreteSaveFileDialog != null)
                {
                    _concreteSaveFileDialog.Dispose();
                    _concreteSaveFileDialog = null;
                }
            }
        }

        #endregion
    }
}
