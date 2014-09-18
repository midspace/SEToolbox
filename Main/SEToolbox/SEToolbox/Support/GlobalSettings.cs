﻿namespace SEToolbox.Support
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using Microsoft.Win32;

    public class GlobalSettings
    {
        #region fields

        public static GlobalSettings Default = new GlobalSettings();

        /// <summary>
        /// Temporary store for Game Version.
        /// </summary>
        public Version SEVersion;

        /// <summary>
        /// Temporary property to reprompt user to game installation path.
        /// </summary>
        public bool PromptUser;

        private bool _isLoaded;

        private const string BaseKey = @"SOFTWARE\MidSpace\SEToolbox";

        #endregion

        #region ctor

        public GlobalSettings()
        {
            if (!_isLoaded)
            {
                Load();
            }
        }

        #endregion

        #region properties
        
        /// <summary>
        /// Application binary path.
        /// </summary>
        public string SEBinPath { get; set; }

        /// <summary>
        /// Display language for localized text.
        /// <remarks>This is not for number or date formats, as this is taken directly from the User profile via CurrentCulture.</remarks>
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Delimited ';' list of UNC paths to search for Save World data, with the 'LastLoaded.sbl' at its root.
        /// </summary>
        public string CustomUserSavePaths { get; set; }

        public WindowState? WindowState { get; set; }
        public double? WindowTop { get; set; }
        public double? WindowLeft { get; set; }
        public double? WindowWidth { get; set; }
        public double? WindowHeight { get; set; }

        /// <summary>
        /// Indicates if Toolbox Version check should be ignored.
        /// </summary>
        public bool? AlwaysCheckForUpdates { get; set; }
        
        /// <summary>
        /// Ignore this specific version during Toolbox version check.
        /// </summary>
        public string IgnoreUpdateVersion { get; set; }

        #endregion

        #region methods

        public void Save()
        {
            var key = Registry.CurrentUser.OpenSubKey(BaseKey, true) ?? Registry.CurrentUser.CreateSubKey(BaseKey);

            UpdateValue(key, "SEBinPath", SEBinPath);
            UpdateValue(key, "CustomUserSavePaths", CustomUserSavePaths);
            UpdateValue(key, "LanguageCode", LanguageCode);
            UpdateValue(key, "WindowState", WindowState);
            UpdateValue(key, "WindowTop", WindowTop);
            UpdateValue(key, "WindowLeft", WindowLeft);
            UpdateValue(key, "WindowWidth", WindowWidth);
            UpdateValue(key, "WindowHeight", WindowHeight);
            UpdateValue(key, "AlwaysCheckForUpdates", AlwaysCheckForUpdates);
            UpdateValue(key, "IgnoreUpdateVersion", IgnoreUpdateVersion);
        }

        public void Load()
        {
            _isLoaded = true;
            var key = Registry.CurrentUser.OpenSubKey(BaseKey, false);
            if (key == null)
            {
                Reset();
                return;
            }

            SEBinPath = ReadValue<string>(key, "SEBinPath", null);
            LanguageCode = ReadValue<string>(key, "LanguageCode", CultureInfo.CurrentUICulture.IetfLanguageTag);
            CustomUserSavePaths = ReadValue<string>(key, "CustomUserSavePaths", null);
            WindowState = ReadValue<WindowState?>(key, "WindowState", null);
            WindowTop = ReadValue<double?>(key, "WindowTop", null);
            WindowLeft = ReadValue<double?>(key, "WindowLeft", null);
            WindowWidth = ReadValue<double?>(key, "WindowWidth", null);
            WindowHeight = ReadValue<double?>(key, "WindowHeight", null);
            AlwaysCheckForUpdates = ReadValue<bool?>(key, "AlwaysCheckForUpdates", null);
            IgnoreUpdateVersion = ReadValue<string>(key, "IgnoreUpdateVersion", null);
        }

        /// <summary>
        /// set all properties to their default value. Used for new application installs.
        /// </summary>
        public void Reset()
        {
            SEBinPath = null;
            LanguageCode = CultureInfo.CurrentUICulture.IetfLanguageTag;  // Display language (only applied on multi lingual deployment of Windows OS).
            CustomUserSavePaths = null;
            WindowState = null;
            WindowTop = null;
            WindowLeft = null;
            WindowHeight = null;
            WindowWidth = null;
            AlwaysCheckForUpdates = null;
            IgnoreUpdateVersion = null;
        }

        public static Version GetAppVersion(bool ignoreRevision = false)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly()
              .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)
              .OfType<AssemblyFileVersionAttribute>()
              .FirstOrDefault();

            var version = assemblyVersion == null ? new Version() : new Version(assemblyVersion.Version);

            if (ignoreRevision)
                return new Version(version.Major, version.Minor, version.Build);
            else
                return version;
        }

        #endregion

        #region helpers

        private static void UpdateValue(RegistryKey key, string subkey, object value)
        {
            if (value == null)
            {
                key.DeleteValue(subkey, false);
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
                
                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var baseType = typeof(T).GetGenericArguments()[0];

                    if (baseType.BaseType == typeof(Enum))
                    {
                        return (T)Enum.Parse(baseType, (string)item);
                    }

                    return (T)Convert.ChangeType(item, baseType);
                }

                if (typeof(T) == typeof(Version))
                {
                    item = new Version((string)item);
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
