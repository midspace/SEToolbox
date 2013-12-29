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
    // Make sure none of the SpaceEngineers are referenced here, to prevent preloading of the assemblies.
    // Otherwise the assemblies updater will not work.

    public static class ToolboxUpdater
    {
        #region GetApplicationFilePath

        public static string GetApplicationFilePath()
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
            else
            {
                // Backup check, but no choice if the above goes to pot.
                // Using the [Software\Valve\Steam\SteamPath] as a base for "\steamapps\common\SpaceEngineers", is unreliable, as the Steam Library is customizable and could be on another drive and directory.
                if (Environment.Is64BitProcess)
                    key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Valve\Steam", false);
                else
                    key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam", false);

                if (key != null)
                {
                    return (string)key.GetValue("InstallPath") + @"\SteamApps\common\SpaceEngineers";
                }
            }

            return null;
        }

        #endregion

        #region IsSpaceEngineersInstalled

        public static bool IsSpaceEngineersInstalled()
        {
            string filePath = GetApplicationFilePath();
            if (string.IsNullOrEmpty(filePath))
                throw new ToolboxException(ExceptionState.NoRegistry);
            if (!Directory.Exists(filePath))
                throw new ToolboxException(ExceptionState.NoDirectory);

            // Skip checking for the .exe. Not required for the Toolbox currently.
            // The new "bin" and "Bin64" directories in the current release make this pointless.
            //if (!File.Exists(Path.Combine(filePath, "SpaceEngineers.exe")))
            //    throw new ToolboxException(ExceptionState.NoApplication);
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
            }

            if (rssFeed != null)
            {
                var items = (from item in rssFeed.Descendants("item")
                             select new RssFeedItem { Title = item.Element("title").Value, Link = item.Element("link").Value }).ToList();

                var newItem = items.FirstOrDefault(i => i.GetVersion() > currentVersion);
                return newItem;
            }

            return null;
        }

        #endregion

        #region IsBaseAssembliesChanged

        public static bool IsBaseAssembliesChanged()
        {
            // We use the Bin64 Path, as these assemblies are marked "AllCPU", and will work regardless of processor architecture.
            string baseFilePath = Path.Combine(GetApplicationFilePath(), "Bin64");
            string appFilePath = Path.GetDirectoryName(Application.ExecutablePath);

            bool update = false;

            update = DoFilesDiffer(baseFilePath, appFilePath, "Sandbox.CommonLib.dll");
            if (update)
                return update;

            update = DoFilesDiffer(baseFilePath, appFilePath, "Sandbox.CommonLib.XmlSerializers.dll");
            if (update)
                return update;

            update = DoFilesDiffer(baseFilePath, appFilePath, "VRage.Common.dll");
            if (update)
                return update;

            update = DoFilesDiffer(baseFilePath, appFilePath, "VRage.Library.dll");
            if (update)
                return update;

            update = DoFilesDiffer(baseFilePath, appFilePath, "VRage.Math.dll");

            return update;
        }

        public static bool UpdateBaseFiles()
        {
            // We use the Bin64 Path, as these assemblies are marked "AllCPU", and will work regardless of processor architecture.
            string baseFilePath = Path.Combine(GetApplicationFilePath(), "Bin64");
            string appFilePath = Path.GetDirectoryName(Application.ExecutablePath);

            var files = new string[]{
            "Sandbox.CommonLib.dll",
            "Sandbox.CommonLib.XmlSerializers.dll",
            "VRage.Common.dll",
            "VRage.Library.dll",
            "VRage.Math.dll",};

            foreach (var filename in files)
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

        internal static bool RunElevated(string fileName)
        {
            var processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = fileName;

            try
            {
                Process.Start(processInfo);
                return true;
            }
            catch (Win32Exception ex)
            {
                //Do nothing. Probably the user canceled the UAC window
            }
            return false;
        }
    }
}
