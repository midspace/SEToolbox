namespace SEToolbox.Support
{
    using System;
    using System.Globalization;
    using System.Windows;
    using Microsoft.Win32;

    public class GlobalSettings
    {
        #region fields

        public static GlobalSettings Default = new GlobalSettings();

        private bool _isLoaded;

        private const string BaseKey = @"SOFTWARE\MidSpace\SEToolbox";

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

        public string LanguageCode { get; set; }

        public WindowState? WindowState { get; set; }
        public double? WindowTop { get; set; }
        public double? WindowLeft { get; set; }
        public double? WindowWidth { get; set; }
        public double? WindowHeight { get; set; }
        public bool? AlwaysCheckForUpdates { get; set; }

        #endregion

        #region methods

        public void Save()
        {
            var key = Registry.CurrentUser.OpenSubKey(BaseKey, true);
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(BaseKey);
            }

            UpdateValue(key, "SEInstallLocation", this.SEInstallLocation);
            UpdateValue(key, "LanguageCode", this.LanguageCode);
            UpdateValue(key, "WindowState", this.WindowState);
            UpdateValue(key, "WindowTop", this.WindowTop);
            UpdateValue(key, "WindowLeft", this.WindowLeft);
            UpdateValue(key, "WindowWidth", this.WindowWidth);
            UpdateValue(key, "WindowHeight", this.WindowHeight);
            UpdateValue(key, "AlwaysCheckForUpdates", this.AlwaysCheckForUpdates);
        }

        public void Load()
        {
            this._isLoaded = true;
            var key = Registry.CurrentUser.OpenSubKey(BaseKey, false);
            if (key == null)
                return;

            this.SEInstallLocation = ReadValue<string>(key, "SEInstallLocation", null);
            this.LanguageCode = ReadValue<string>(key, "LanguageCode", CultureInfo.CurrentCulture.IetfLanguageTag);
            this.WindowState = ReadValue<WindowState?>(key, "WindowState", null);
            this.WindowTop = ReadValue<double?>(key, "WindowTop", null);
            this.WindowLeft = ReadValue<double?>(key, "WindowLeft", null);
            this.WindowWidth = ReadValue<double?>(key, "WindowWidth", null);
            this.WindowHeight = ReadValue<double?>(key, "WindowHeight", null);
            this.AlwaysCheckForUpdates = ReadValue<bool?>(key, "AlwaysCheckForUpdates", null);
        }

        public void Reset()
        {
            this.SEInstallLocation = null;
            this.LanguageCode = CultureInfo.CurrentCulture.IetfLanguageTag;
            this.WindowState = null;
            this.WindowTop = null;
            this.WindowLeft = null;
            this.WindowHeight = null;
            this.WindowWidth = null;
            this.AlwaysCheckForUpdates = null;
        }

        #endregion

        #region helpers

        private static void UpdateValue(RegistryKey key, string subkey, object value)
        {
            if (value == null)
            {
                key.DeleteSubKey(subkey, false);
            }
            else
            {
                if (value is bool || value is Int32)
                {
                    key.SetValue(subkey, value, RegistryValueKind.DWord);
                }
                else
                {
                    key.SetValue(subkey, value);
                }
            }
        }

        private static T ReadValue<T>(RegistryKey key, string subkey, T defaultValue)
        {
            var item = key.GetValue(subkey, defaultValue);

            try
            {
                if (item == null)
                {
                    return default(T);
                }
                else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var baseType = typeof(T).GetGenericArguments()[0];

                    if (baseType.BaseType == typeof(Enum))
                    {
                        return (T)Enum.Parse(baseType, (string)item);
                    }

                    return (T)Convert.ChangeType(item, baseType);
                }
            }
            catch
            {
                item = defaultValue;
            }

            return (T)item;
        }

        #endregion
    }
}
