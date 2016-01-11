namespace SEToolbox.Support
{
    using System.Windows;

    /// <summary>
    /// Helper to show or close given splash window
    /// </summary>
    public static class Splasher
    {
        /// <summary>
        /// 
        /// </summary>
        private static Window _splash;

        /// <summary>
        /// Get or set the splash screen window
        /// </summary>
        public static Window Splash
        {
            get
            {
                return _splash;
            }
            set
            {
                _splash = value;
            }
        }
        /// <summary>
        /// Show splash screen
        /// </summary>
        public static void ShowSplash()
        {
            if (_splash != null)
            {
                _splash.Show();
                System.Windows.Forms.Application.DoEvents();
            }
        }
        /// <summary>
        /// Close splash screen
        /// </summary>
        public static void CloseSplash()
        {
            if (_splash != null)
            {
                _splash.Close();
            }
        }
    }
}
