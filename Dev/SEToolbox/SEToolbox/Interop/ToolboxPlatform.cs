using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ProtoBuf.Meta;
using VRage;
using VRage.Audio;
using VRage.Http;
using VRage.Input;
using VRage.Serialization;
using VRageMath;
using VRageRender;

namespace SEToolbox.Interop
{
    public class ToolboxPlatform : IVRagePlatform
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

        public IMyImeProcessor ImeProcessor => throw new NotImplementedException();

        public IVRageHttp Http => throw new NotImplementedException();

        public float GCMemory => throw new NotImplementedException();

        public long RemainingAvailableMemory => throw new NotImplementedException();

        public long ProcessPrivateMemory => throw new NotImplementedException();

        public bool IsScriptCompilationSupported => throw new NotImplementedException();

        public bool IsSingleInstance => throw new NotImplementedException();

        public IVRageInput2 Input2 => throw new NotImplementedException();

        public bool IsRenderOutputDebugSupported => throw new NotImplementedException();

        public IMyAudio Audio => throw new NotImplementedException();

        public bool IsRemoteDebuggingSupported => throw new NotImplementedException();

        public uint[] DeveloperKeys => throw new NotImplementedException();

        public IMyCrashReporting CrashReporting => throw new NotImplementedException();

        public event Action<IntPtr> OnSystemProtocolActivated;

        IProtoTypeModel typeModel = new DynamicTypeModel();

        public void ApplyRenderSettings(MyRenderDeviceSettings? settings)
        {
            throw new NotImplementedException();
        }

        public void CreateInput2()
        {
            throw new NotImplementedException();
        }

        public object CreateRenderAnnotation(object deviceContext)
        {
            throw new NotImplementedException();
        }

        public void CreateRenderDevice(ref MyRenderDeviceSettings? settings, out object deviceInstance, out object swapChain)
        {
            throw new NotImplementedException();
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
            Window = new Window();
        }

        public void DisposeRenderDevice()
        {
            throw new NotImplementedException();
        }

        public void Done()
        {
            throw new NotImplementedException();
        }

        public IntPtr FindWindowInParent(string parent, string child)
        {
            return IntPtr.Zero;
        }

        public string GetAppDataPath()
        {
            throw new NotImplementedException();
        }

        public ulong GetGlobalAllocationsStamp()
        {
            return 0;
        }

        public string GetInfoCPU(out uint frequency)
        {
            frequency = 0;
            return "";
        }

        public string GetOsName()
        {
            return null;
        }

        public List<string> GetProcessesLockingFile(string path)
        {
            return new List<string>();
        }

        public MyAdapterInfo[] GetRenderAdapterList()
        {
            throw new NotImplementedException();
        }

        public ulong GetThreadAllocationStamp()
        {
            return 0;
        }

        public ulong GetTotalPhysicalMemory()
        {
            return 0;
        }

        public IProtoTypeModel GetTypeModel()
        {
            return typeModel;
        }

        public void HideSplashScreen()
        {
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public void InitAnalytics(string projectId, string version)
        {
        }

        public void LogEnvironmentInformation()
        {
        }

        public void LogToExternalDebugger(string message)
        {
            throw new NotImplementedException();
        }

        public MessageBoxResult MessageBox(string text, string caption, MessageBoxOptions options)
        {
            return MessageBoxResult.Cancel;
        }

        public bool OpenUrl(string url)
        {
            throw new NotImplementedException();
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

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void WriteLineToConsole(string msg)
        {
            Console.WriteLine(msg);
        }
    }

    class Window : IVRageWindow
    {
        public bool DrawEnabled => false;

        public bool IsActive => false;

        public Vector2I ClientSize => default;

        public event Action OnExit;

        public void AddMessageHandler(uint wm, ActionRef<MyMessage> action)
        {
            throw new NotImplementedException();
        }

        public void DoEvents()
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public void Hide()
        {
            throw new NotImplementedException();
        }

        public void RemoveMessageHandler(uint wm, ActionRef<MyMessage> action)
        {
            throw new NotImplementedException();
        }

        public void SetCursor(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void ShowAndFocus()
        {
            throw new NotImplementedException();
        }

        public void UpdateMainThread()
        {
            throw new NotImplementedException();
        }

        public bool UpdateRenderThread()
        {
            return false;
        }
    }

    // Internal class copied from SE
    class DynamicTypeModel : IProtoTypeModel
    {
        public TypeModel Model => m_typeModel;

        private RuntimeTypeModel m_typeModel;

        public DynamicTypeModel()
        {
            CreateTypeModel();
        }

        private void CreateTypeModel()
        {
            m_typeModel = RuntimeTypeModel.Create(true);
            m_typeModel.AutoAddMissingTypes = true;
            m_typeModel.UseImplicitZeroDefaults = false;
        }

        private static ushort Get16BitHash(string s)
        {
            using (MD5 mD = MD5.Create())
                return BitConverter.ToUInt16(mD.ComputeHash(Encoding.UTF8.GetBytes(s)), 0);
        }

        public void RegisterTypes(IEnumerable<Type> types)
        {
            var registered = new HashSet<Type>();

            foreach (Type type in types)
                RegisterType(type);

            void RegisterType(Type protoType)
            {
                if (protoType.IsGenericType)
                    return;

                if (protoType.BaseType == typeof(object) || protoType.IsValueType)
                {
                    m_typeModel.Add(protoType, true);
                }
                else
                {
                    RegisterType(protoType.BaseType);

                    if (registered.Add(protoType))
                    {
                        int fieldNumber = Get16BitHash(protoType.Name) + 65535;
                        m_typeModel.Add(protoType, true);
                        m_typeModel[protoType.BaseType].AddSubType(fieldNumber, protoType);
                    }
                }
            }
        }

        public void FlushCaches()
        {
            CreateTypeModel();
        }
    }
}
