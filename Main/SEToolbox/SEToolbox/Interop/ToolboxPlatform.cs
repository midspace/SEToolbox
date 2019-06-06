using System;
using System.Collections.Generic;
using VRage;
using VRage.Input;
using VRage.Library.Exceptions;
using VRage.Utils;
using VRageMath;

namespace SEToolbox.Interop
{
    public class ToolboxPlatform : VRage.IVRagePlatform
    {
        public float CPUCounter { get; set; }

        public float RAMCounter { get; set; }

        public string Clipboard { get; set; }

        public bool IsAllocationReady { get; set; }

        public IAnsel Ansel { get; set; }

        public IAfterMath AfterMath { get; set; }

        public IVRageInput Input { get; set; }

        public IVRageWindow Window { get; set; }

        public bool SessionReady { set; get; }

        public IMyAnalytics Analytics { get; set; }

        public IMyImeProcessor ImeProcessor
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void CreateToolWindow(IntPtr windowHandle)
        {

        }

        public IVideoPlayer CreateVideoPlayer()
        {
            return null;
        }

        public void CreateWindow(string gameName, string gameIcon, Type imeCandidateType)
        {

        }

        public IntPtr FindWindowInParent(string parent, string child)
        {
            return IntPtr.Zero;
        }

        public ulong GetGlobalAllocationsStamp()
        {
            return 0;
        }

        public string GetInfoCPU(out uint frequency)
        {
            frequency = 0;
            return null;
        }

        public string GetOsName()
        {
            return null;
        }

        public List<string> GetProcessesLockingFile(string path)
        {
            return new List<string>();
        }

        public ulong GetThreadAllocationStamp()
        {
            return 0;
        }

        public ulong GetTotalPhysicalMemory()
        {
            return 0;
        }

        public List<MyDriverDetails> GetVideoDriverDetails()
        {
            return new List<MyDriverDetails>();
        }

        public void HideSplashScreen()
        {
        }

        public void InitAnalytics(string projectId, string version)
        {
        }

        public void InitGameAnalytics(string projectId, string version)
        {
            throw new NotImplementedException();
        }

        public void InitializeInput(bool dummy, IMyControlNameLookup controlLookup, Dictionary<MyStringId, MyControl> defaultGameControls, bool enableDevKeys)
        {
            throw new NotImplementedException();
        }

        public void LogEnvironmentInformation()
        {
        }

        public MessageBoxResult MessageBox(string text, string caption, MessageBoxOptions options)
        {
            return MessageBoxResult.Cancel;
        }

        public bool MessageBoxCrashForm(ref MyCrashScreenTexts texts, out string message, out string email)
        {
            message = null;
            email = null;
            return false;
        }

        public void MessageBoxModCrashForm(ref MyModCrashScreenTexts texts)
        {
        }

        public void PostMessage(IntPtr handle, uint wm, IntPtr wParam, IntPtr lParam)
        {
        }

        public void ResetColdStartRegister()
        {
        }

        public void ShowSplashScreen(string image, Vector2 scale)
        {
        }

        public bool WriteMiniDump(string dumpPath, MyMiniDump.Options dumpFlags, MyMiniDump.ExceptionInfo info)
        {
            return false;
        }
    }
}
