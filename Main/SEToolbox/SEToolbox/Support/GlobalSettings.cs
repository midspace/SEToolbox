namespace SEToolbox.Support
{
    using Microsoft.Win32;

    public class GlobalSettings
    {
        #region fields

        public static GlobalSettings Default = new GlobalSettings();

        private bool _isLoaded;

        private const string _baseKey = @"SOFTWARE\MidSpace\SEToolbox";

        #endregion

        #region ctor

        public GlobalSettings()
        {
            if (!_isLoaded)
            {
                this.Load();
            }
        }

        #endregion

        #region properties

        public string SEInstallLocation { get; set; }

        #endregion

        #region methods

        public void Save()
        {
            var key = Registry.CurrentUser.OpenSubKey(_baseKey, true);
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(_baseKey);
            }

            key.SetValue("SEInstallLocation", this.SEInstallLocation);
        }

        public void Load()
        {
            this._isLoaded = true;
            var key = Registry.CurrentUser.OpenSubKey(_baseKey, false);
            if (key == null)
                return;

            this.SEInstallLocation = key.GetValue("SEInstallLocation") as string;
        }

        #endregion
    }
}
