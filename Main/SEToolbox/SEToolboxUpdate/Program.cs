namespace SEToolboxUpdate
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using Microsoft.Win32;

    class Program
    {
        static void Main(string[] args)
        {
            var attemptedAlready = args.Any(a => a.ToUpper() == "/A");

            var appFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appFilePath = Path.GetDirectoryName(appFile);

            if (CheckIsRuningElevated())
            {
                UpdateBaseFiles(appFilePath);
                RunElevated(Path.Combine(appFilePath, "SEToolbox.exe"), "/U", false);
            }
            else
            {
                if (attemptedAlready)
                {
                    RunElevated(Path.Combine(appFilePath, "SEToolbox.exe"), "/U /A", false);
                }
                else
                {
                    // TODO: check the return condition.
                    RunElevated(appFile, string.Join(" ", args) + " /A", true);
                }
            }
        }

        private static bool UpdateBaseFiles(string appFilePath)
        {
            // We use the Bin64 Path, as these assemblies are marked "AllCPU", and will work regardless of processor architecture.
            var baseFilePath = Path.Combine(GetApplicationFilePath(), "Bin64");

            var files = new string[]{
            "Sandbox.Common.dll",
            "Sandbox.Common.XmlSerializers.dll",
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

        private static string GetApplicationFilePath()
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

        private static bool CheckIsRuningElevated()
        {
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return pricipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool RunElevated(string fileName, string arguments, bool elevate)
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
    }
}
