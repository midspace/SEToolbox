namespace SEToolbox.Services
{
    using SEToolbox.Interfaces;

    /// <summary>
    /// ViewModel of the abstract ColorDialog.
    /// </summary>
    public class ColorDialogViewModel : IColorDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDialogViewModel"/> class.
        /// </summary>
        public ColorDialogViewModel()
        {
            // Set default values
            AllowFullOpen = true;
            AnyColor = true;
            CustomColors = new int[0];
            FullOpen = false;
            ShowHelp = false;
            SolidColorOnly = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user can use the dialog box to define custom colors.
        /// </summary>
        public bool AllowFullOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box displays all available colors in the set of basic colors.
        /// </summary>
        public bool AnyColor { get; set; }

        /// <summary>
        /// Gets or sets the color selected by the user.
        /// </summary>
        public System.Drawing.Color? DrawingColor { get; set; }

        public System.Windows.Media.Color? MediaColor { get; set; }

        public System.Windows.Media.SolidColorBrush BrushColor { get; set; }

        /// <summary>
        /// Gets or sets the set of custom colors shown in the dialog box.
        /// </summary>
        public int[] CustomColors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the controls used to create custom colors are visible when the dialog box is opened
        /// </summary>
        public bool FullOpen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a Help button appears in the color dialog box.
        /// </summary>
        public bool ShowHelp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box will restrict users to selecting solid colors only.
        /// </summary>
        public bool SolidColorOnly { get; set; }
    }
}