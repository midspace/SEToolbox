namespace SEToolbox.Support
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Principal;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public static class ToolboxUpdater
    {
        private static readonly string[] CoreSpaceEngineersFiles = {
            "Sandbox.Common.dll",
            "Sandbox.Common.XmlSerializers.dll",
            "VRage.Common.dll",
            "VRage.Library.dll",
            "VRage.Math.dll",};

        #region GetApplicationFilePath

        public static string GetApplicationFilePath()
        {
            var gamePath = Properties.Settings.Default.SEInstallLocation;

            if (string.IsNullOrEmpty(gamePath))
                gamePath = GetGameRegistryFilePath();

            return gamePath;
        }

        /// <summary>
        /// Looks for the Space Engineers install location in the Registry, which should return the form:
        /// "C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers"
        /// </summary>
        /// <returns></returns>
        public static string GetGameRegistryFilePath()
        {
            RegistryKey key;
            if (Environment.Is64BitProcess)
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244850", false);
            else
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244850", false);

            if (key != null)
            {
                return key.GetValue("InstallLocation") as string;
            }

            // Backup check, but no choice if the above goes to pot.
            // Using the [Software\Valve\Steam\SteamPath] as a base for "\steamapps\common\SpaceEngineers", is unreliable, as the Steam Library is customizable and could be on another drive and directory.
            var steamPath = GetSteamFilePath();
            if (!string.IsNullOrEmpty(steamPath))
            {
                return Path.Combine(steamPath, @"SteamApps\common\SpaceEngineers");
            }

            return null;
        }
        
        #endregion

        #region GetSteamFilePath

        /// <summary>
        /// Looks for the Steam install location in the Registry, which should return the form:
        /// "C:\Program Files (x86)\Steam"
        /// </summary>
        /// <returns></returns>
        public static string GetSteamFilePath()
        {
            RegistryKey key;

            if (Environment.Is64BitProcess)
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam", false);
            else
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam", false);

            if (key != null)
            {
                return (string)key.GetValue("InstallPath");
            }

            return null;
        }

        #endregion

        #region IsSpaceEngineersInstalled

        /// <summary>
        /// Checks for key directory names in the root game folder.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool ValidateSpaceEngineersInstall(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;
            if (!Directory.Exists(filePath))
                return false;
            if (!Directory.Exists(Path.Combine(filePath, "Bin64")))
                return false;
            if (!Directory.Exists(Path.Combine(filePath, "Content")))
                return false;

            // Skip checking for the .exe. Not required for the Toolbox currently.
            return true;
        }

        #endregion

        #region CheckForUpdates

        public static RssFeedItem CheckForUpdates()
        {
#if DEBUG
            // Skip the load check, as it make take a few seconds.
            if (Debugger.IsAttached)
                return null;
#endif

            var assemblyVersion = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)
                    .OfType<AssemblyFileVersionAttribute>()
                    .FirstOrDefault();
            var currentVersion = new Version(assemblyVersion.Version);

            // Create the WebClient with Proxy Credentials, as stupidly this works for some reason before calling XDocument.Load.
            var webclient = new WebClient();
            webclient.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials; // For Proxy servers on Corporate networks.
            XDocument rssFeed = null;

            try
            {
                rssFeed = XDocument.Load("http://setoolbox.codeplex.com/project/feeds/rss?ProjectRSSFeed=codeplex%3a%2f%2frelease%2fsetoolbox");
            }
            catch
            {
                // Ignore any errors.
                // If it cannot connect, then there may be an intermittant connection issue, either with the internet, or codeplex (which has happened before).
            }

            if (rssFeed == null)
                return null;

            var items = (from item in rssFeed.Descendants("item")
                         select new RssFeedItem { Title = item.Element("title").Value, Link = item.Element("link").Value }).ToList();

            var newItem = items.FirstOrDefault(i => i.GetVersion() > currentVersion);
            return newItem;
        }

        #endregion

        #region IsBaseAssembliesChanged

        public static bool IsBaseAssembliesChanged()
        {
            // We use the Bin64 Path, as these assemblies are marked "AllCPU", and will work regardless of processor architecture.
            var baseFilePath = Path.Combine(GetApplicationFilePath(), "Bin64");
            var appFilePath = Path.GetDirectoryName(Application.ExecutablePath);

            foreach (var filename in CoreSpaceEngineersFiles)
            {
                if (DoFilesDiffer(baseFilePath, appFilePath, filename))
                    return true;
            }

            return false;
        }

        public static bool UpdateBaseFiles()
        {
            // We use the Bin64 Path, as these assemblies are marked "AllCPU", and will work regardless of processor architecture.
            var baseFilePath = Path.Combine(GetApplicationFilePath(), "Bin64");
            var appFilePath = Path.GetDirectoryName(Application.ExecutablePath);

            foreach (var filename in CoreSpaceEngineersFiles)
            {
                var sourceFile = Path.Combine(baseFilePath, filename);

                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, Path.Combine(appFilePath, filename), true);
                }
            }

            return true;
        }

        #endregion

        #region DoFilesDiffer

        public static bool DoFilesDiffer(string directory1, string directory2, string filename)
        {
            return DoFilesDiffer(Path.Combine(directory1, filename), Path.Combine(directory2, filename));
        }

        public static bool DoFilesDiffer(string file1, string file2)
        {
            if (File.Exists(file1) != File.Exists(file2))
                return false;

            var buffer1 = File.ReadAllBytes(file1);
            var buffer2 = File.ReadAllBytes(file2);

            if (buffer1.Length != buffer2.Length)
                return true;

            var ass1 = Assembly.Load(buffer1);
            var guid1 = ass1.ManifestModule.ModuleVersionId;

            var ass2 = Assembly.Load(buffer2);
            var guid2 = ass2.ManifestModule.ModuleVersionId;

            return guid1 != guid2;
        }

        #endregion

        #region CheckIsRuningElevated

        internal static bool CheckIsRuningElevated()
        {
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return pricipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #endregion

        #region RunElevated

        internal static bool RunElevated(string fileName, string arguments, bool elevate)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments
            };

            if (elevate)
            {
                processInfo.Verb = "runas";
            }

            try
            {
                Process.Start(processInfo);
                return true;
            }
            catch (Win32Exception)
            {
                //Do nothing. Probably the user canceled the UAC window
            }
            return false;
        }

        #endregion
    }
}
