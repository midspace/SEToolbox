namespace SEToolbox.Interfaces
{
    /// <summary>
    /// Interface describing the ColorDialog.
    /// </summary>
    public interface IColorDialog
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user can use the dialog box to define custom colors.
        /// </summary>
        bool AllowFullOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box displays all available colors in the set of basic colors.
        /// </summary>
        bool AnyColor { get; set; }

        /// <summary>
        /// Gets or sets the color selected by the user.
        /// </summary>
        System.Drawing.Color? DrawingColor { get; set; }

        System.Windows.Media.Color? MediaColor { get; set; }

        System.Windows.Media.SolidColorBrush BrushColor { get; set; }

        /// <summary>
        /// Gets or sets the set of custom colors shown in the dialog box.
        /// </summary>
        int[] CustomColors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the controls used to create custom colors are visible when the dialog box is opened
        /// </summary>
        bool FullOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a Help button appears in the color dialog box.
        /// </summary>
        bool ShowHelp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box will restrict users to selecting solid colors only.
        /// </summary>
        bool SolidColorOnly { get; set; }
    }
}