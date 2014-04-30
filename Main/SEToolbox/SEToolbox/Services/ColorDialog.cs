namespace SEToolbox.Services
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Windows.Forms;
    using System.Windows.Media;
    using SEToolbox.Interfaces;
    using WinFormsColorDialog = System.Windows.Forms.ColorDialog;

    /// <summary>
    /// Class wrapping System.Windows.Forms.ColorDialog, making it accept a IColorDialog.
    /// </summary>
    public class ColorDialog : IDisposable
    {
        private readonly IColorDialog _colorDialog;
        private WinFormsColorDialog _concreteColorDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDialog"/> class.
        /// </summary>
        /// <param name="colorDialog">The interface of a color dialog.</param>
        public ColorDialog(IColorDialog colorDialog)
        {
            Contract.Requires(colorDialog != null);

            this._colorDialog = colorDialog;

            // Create concrete ColorDialog
            _concreteColorDialog = new WinFormsColorDialog
            {
                AllowFullOpen = colorDialog.AllowFullOpen,
                AnyColor = colorDialog.AnyColor,
                FullOpen = colorDialog.FullOpen,
                CustomColors = colorDialog.CustomColors,
                ShowHelp = colorDialog.ShowHelp,
                SolidColorOnly = colorDialog.SolidColorOnly,
            };

            if (colorDialog.DrawingColor.HasValue)
                _concreteColorDialog.Color = colorDialog.DrawingColor.Value;
            else if (colorDialog.MediaColor.HasValue)
                _concreteColorDialog.Color = System.Drawing.Color.FromArgb(colorDialog.MediaColor.Value.A, colorDialog.MediaColor.Value.R, colorDialog.MediaColor.Value.G, colorDialog.MediaColor.Value.B);
            else if (colorDialog.BrushColor != null)
            {
                var c = ((System.Windows.Media.SolidColorBrush)colorDialog.BrushColor).Color;
                _concreteColorDialog.Color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
            }
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

            var result = _concreteColorDialog.ShowDialog(owner);

            // Update ViewModel
            _colorDialog.DrawingColor = _concreteColorDialog.Color;
            _colorDialog.MediaColor = System.Windows.Media.Color.FromArgb(_concreteColorDialog.Color.A, _concreteColorDialog.Color.R, _concreteColorDialog.Color.G, _concreteColorDialog.Color.B);
            _colorDialog.BrushColor = new SolidColorBrush(_colorDialog.MediaColor.Value);
            _colorDialog.CustomColors = _concreteColorDialog.CustomColors;

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

        ~ColorDialog()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_concreteColorDialog != null)
                {
                    _concreteColorDialog.Dispose();
                    _concreteColorDialog = null;
                }
            }
        }

        #endregion
    }
}
