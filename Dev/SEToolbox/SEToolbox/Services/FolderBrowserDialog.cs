namespace SEToolbox.Services
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Windows.Forms;
    using SEToolbox.Interfaces;
    using WinFormsFolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

    /// <summary>
    /// Class wrapping System.Windows.Forms.FolderBrowserDialog, making it accept a ViewModel.
    /// </summary>
    public class FolderBrowserDialog : IDisposable
    {
        private readonly IFolderBrowserDialog _folderBrowserDialog;
        private WinFormsFolderBrowserDialog _concreteFolderBrowserDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderBrowserDialog"/> class.
        /// </summary>
        /// <param name="folderBrowserDialog">The interface of a folder browser dialog.</param>
        public FolderBrowserDialog(IFolderBrowserDialog folderBrowserDialog)
        {
            Contract.Requires(folderBrowserDialog != null);

            this._folderBrowserDialog = folderBrowserDialog;

            // Create concrete FolderBrowserDialog
            _concreteFolderBrowserDialog = new WinFormsFolderBrowserDialog
            {
                Description = folderBrowserDialog.Description,
                SelectedPath = folderBrowserDialog.SelectedPath,
                ShowNewFolderButton = folderBrowserDialog.ShowNewFolderButton
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

            var result = _concreteFolderBrowserDialog.ShowDialog(owner);

            // Update ViewModel
            _folderBrowserDialog.SelectedPath = _concreteFolderBrowserDialog.SelectedPath;

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

        ~FolderBrowserDialog()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_concreteFolderBrowserDialog != null)
                {
                    _concreteFolderBrowserDialog.Dispose();
                    _concreteFolderBrowserDialog = null;
                }
            }
        }

        #endregion
    }
}