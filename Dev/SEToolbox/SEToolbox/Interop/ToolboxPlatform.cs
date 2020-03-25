using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using ProtoBuf.Meta;
using VRage;
using VRage.Audio;
using VRage.Http;
using VRage.Input;
using VRage.Serialization;
using VRageRender;

namespace SEToolbox.Interop
{
    public class ToolboxPlatform : IVRagePlatform
    {
        public bool SessionReady { get; set; }

        public IVRageWindows Windows => throw new NotImplementedException();

        public IVRageHttp Http => throw new NotImplementedException();

        public IVRageSystem System { get; } = new VRageSystemImpl();

        public IVRageRender Render { get; } = new VRageRenderImpl();

        public IAnsel Ansel => throw new NotImplementedException();

        public IAfterMath AfterMath => throw new NotImplementedException();

        public IVRageInput Input => throw new NotImplementedException();

        public IVRageInput2 Input2 => throw new NotImplementedException();

        public IMyAudio Audio => throw new NotImplementedException();

        public IMyImeProcessor ImeProcessor => throw new NotImplementedException();

        public IMyCrashReporting CrashReporting => throw new NotImplementedException();

        IProtoTypeModel typeModel;

        public void Init()
        {
            typeModel = new DynamicTypeModel();
        }

        public bool CreateInput2()
        {
            throw new NotImplementedException();
        }

        public IVideoPlayer CreateVideoPlayer()
        {
            throw new NotImplementedException();
        }

        public void Done()
        {
            throw new NotImplementedException();
        }

        public IProtoTypeModel GetTypeModel()
        {
            return typeModel;
        }

        public IMyAnalytics InitAnalytics(string projectId, string version)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }

    class VRageSystemImpl : IVRageSystem
    {
        public float CPUCounter => throw new NotImplementedException();

        public float RAMCounter => throw new NotImplementedException();

        public float GCMemory => throw new NotImplementedException();

        public long RemainingMemoryForGame => throw new NotImplementedException();

        public long ProcessPrivateMemory => throw new NotImplementedException();

        public bool IsScriptCompilationSupported => throw new NotImplementedException();

        public string Clipboard { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsAllocationProfilingReady => throw new NotImplementedException();

        public bool IsSingleInstance => throw new NotImplementedException();

        public bool IsRemoteDebuggingSupported => throw new NotImplementedException();

        public SimulationQuality SimulationQuality => throw new NotImplementedException();

        public bool IsDeprecatedOS => throw new NotImplementedException();

        public event Action<IntPtr> OnSystemProtocolActivated;

        (string Name, uint Frequency)? m_cpuInfo;

        public string GetAppDataPath()
        {
            throw new NotImplementedException();
        }

        public ulong GetGlobalAllocationsStamp()
        {
            throw new NotImplementedException();
        }

        public string GetInfoCPU(out uint frequency)
        {
            if (m_cpuInfo.HasValue)
            {
                string name;
                (name, frequency) = m_cpuInfo.GetValueOrDefault();
                return name;
            }

            frequency = 0u;
            string cpuName = "";

            try
            {
                using (var managementObjectSearcher = new ManagementObjectSearcher("select Name, MaxClockSpeed from Win32_Processor"))
                {
                    foreach (ManagementObject item in managementObjectSearcher.Get())
                    {
                        cpuName = item["Name"].ToString();
                        frequency = (uint)item["MaxClockSpeed"];
                    }
                }
            }
            catch (Exception ex)
            {
                //m_log.WriteLine("Couldn't get cpu info: " + ex);
            }

            m_cpuInfo = (cpuName, frequency);

            return cpuName;
        }

        public string GetOsName()
        {
            throw new NotImplementedException();
        }

        public List<string> GetProcessesLockingFile(string path)
        {
            throw new NotImplementedException();
        }

        public ulong GetThreadAllocationStamp()
        {
            throw new NotImplementedException();
        }

        public ulong GetTotalPhysicalMemory()
        {
            throw new NotImplementedException();
        }

        public void LogEnvironmentInformation()
        {
            throw new NotImplementedException();
        }

        public void LogToExternalDebugger(string message)
        {
            throw new NotImplementedException();
        }

        public bool OpenUrl(string url)
        {
            throw new NotImplementedException();
        }

        public void ResetColdStartRegister()
        {
            throw new NotImplementedException();
        }

        public void WriteLineToConsole(string msg)
        {
            Console.WriteLine(msg);
        }
    }

    class VRageRenderImpl : IVRageRender
    {
        public bool UseParallelRenderInit => throw new NotImplementedException();

        public bool IsRenderOutputDebugSupported => throw new NotImplementedException();

        public event Action OnResuming;
        public event Action OnSuspending;

        public void ApplyRenderSettings(MyRenderDeviceSettings? settings)
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

        public void DisposeRenderDevice()
        {
            throw new NotImplementedException();
        }

        public void FlushIndirectArgsFromComputeShader(object deviceContext)
        {
            throw new NotImplementedException();
        }

        public ulong GetMemoryBudgetForStreamedResources()
        {
            return 0;
        }

        public MyAdapterInfo[] GetRenderAdapterList()
        {
            throw new NotImplementedException();
        }

        public MyRenderPresetEnum GetRenderQualityHint()
        {
            throw new NotImplementedException();
        }

        public void ResumeRenderContext()
        {
            throw new NotImplementedException();
        }

        public void SetMemoryUsedForImprovedGFX(long bytes)
        {
            throw new NotImplementedException();
        }

        public void SuspendRenderContext()
        {
            throw new NotImplementedException();
        }
    }

    // Internal class copied from VRage.Platform.Windows
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
