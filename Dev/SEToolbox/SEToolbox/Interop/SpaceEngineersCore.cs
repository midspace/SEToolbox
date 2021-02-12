namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Threading;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.GameSystems;
    using SEToolbox.Models;
    using SpaceEngineers.Game;
    using Support;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Steam;
    using VRage.Utils;
    using VRageRender;

    /// <summary>
    /// core interop for loading up Space Engineers content.
    /// </summary>
    public class SpaceEngineersCore
    {
        class DerivedGame : MySandboxGame
        {
            public DerivedGame(string[] commandlineArgs, IntPtr windowHandle = default)
                : base(commandlineArgs, windowHandle) { }

            protected override void InitializeRender(IntPtr windowHandle) { }
        }

        protected static readonly uint AppId = 244850; // Game
        //protected static readonly uint AppId = 298740; // Dedicated Server

        public static SpaceEngineersCore Default;
        private WorldResource _worldResource;
        private readonly SpaceEngineersResources _stockDefinitions;
        private readonly List<string> _manageDeleteVoxelList;
        protected MyCommonProgramStartup _startup;
        private IMyGameService _steamService;

        public SpaceEngineersCore()
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            string userDataPath = SpaceEngineersConsts.BaseLocalPath.DataPath;

            MyFileSystem.ExePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(FastResourceLock)).Location);

            MyLog.Default = MySandboxGame.Log;
            SpaceEngineersGame.SetupBasicGameInfo();
            _startup = new MyCommonProgramStartup(new string[] { });

            //var appDataPath = _startup.GetAppDataPath();
            //MyInitializer.InvokeBeforeRun(AppId, MyPerGameSettings.BasicGameInfo.ApplicationName + "SEToolbox", appDataPath);
            //MyInitializer.InitCheckSum();

            MyFileSystem.Reset();
            MyFileSystem.Init(contentPath, userDataPath);

            // This will start the Steam Service, and Steam will think SE is running.
            // TODO: we don't want to be doing this all the while SEToolbox is running,
            // perhaps a once off during load to fetch of mods then disconnect/Dispose.
            _steamService = MySteamGameService.Create(MySandboxGame.IsDedicated, AppId);
            MyServiceManager.Instance.AddService(_steamService);

            IMyUGCService ugc = MySteamUgcService.Create(AppId, _steamService);
            //MyServiceManager.Instance.AddService(ugc);
            MyGameService.WorkshopService.AddAggregate(ugc);

            MyFileSystem.InitUserSpecific(_steamService.UserId.ToString()); // This sets the save file/path to load games from.
            //MyFileSystem.InitUserSpecific(null);
            //SpaceEngineersWorkshop.MySteam.Dispose();

            MySandboxGame.Config = new MyConfig("SpaceEngineers.cfg"); // TODO: Is specific to SE, not configurable to ME.
            MySandboxGame.Config.Load();

            SpaceEngineersGame.SetupPerGameSettings();

            VRage.MyVRage.Init(new ToolboxPlatform());
            VRage.MyVRage.Platform.Init();

            MySandboxGame.InitMultithreading();

            VRageRender.MyRenderProxy.Initialize(new MyNullRender());

            // We create a whole instance of MySandboxGame!
            // If this is causing an exception, then there is a missing dependency.
            MySandboxGame gameTemp = new DerivedGame(new string[] { "-skipintro" });

            // creating MySandboxGame will reset the CurrentUICulture, so I have to reapply it.
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);
            SpaceEngineersApi.LoadLocalization();
            MyStorageBase.UseStorageCache = false;

            // Create an empty instance of MySession for use by low level code.
            var mySession = (Sandbox.Game.World.MySession)FormatterServices.GetUninitializedObject(typeof(Sandbox.Game.World.MySession));

            // Required as the above code doesn't populate it during ctor of MySession.
            ReflectionUtil.ConstructField(mySession, "CreativeTools");
            ReflectionUtil.ConstructField(mySession, "m_sessionComponents");
            ReflectionUtil.ConstructField(mySession, "m_sessionComponentsForUpdate");

            mySession.Settings = new MyObjectBuilder_SessionSettings { EnableVoxelDestruction = true };

            // change for the Clone() method to use XML cloning instead of Protobuf because of issues with MyObjectBuilder_CubeGrid.Clone()
            ReflectionUtil.SetFieldValue(typeof(VRage.ObjectBuilders.MyObjectBuilderSerializer), "ENABLE_PROTOBUFFERS_CLONING", false);

            // Assign the instance back to the static.
            Sandbox.Game.World.MySession.Static = mySession;

            var heightMapLoadingSystem = new MyHeightMapLoadingSystem();
            mySession.RegisterComponent(heightMapLoadingSystem, heightMapLoadingSystem.UpdateOrder, heightMapLoadingSystem.Priority);
            heightMapLoadingSystem.LoadData();

            var planets = new MyPlanets();
            mySession.RegisterComponent(planets, heightMapLoadingSystem.UpdateOrder, heightMapLoadingSystem.Priority);
            planets.LoadData();

            _stockDefinitions = new SpaceEngineersResources();
            _stockDefinitions.LoadDefinitions();
            _manageDeleteVoxelList = new List<string>();
        }

        /// <summary>
        /// Forces static ctor to load stock defintiions.
        /// </summary>
        public static void LoadDefinitions()
        {
            typeof(MyTexts).TypeInitializer.Invoke(null, null); // For tests

            Default = new SpaceEngineersCore();
        }

        public static SpaceEngineersResources Resources
        {
            get
            {
                if (Default._worldResource != null)
                    return Default._worldResource.Resources;

                return Default._stockDefinitions;
            }
        }

        public static string GetDataPathOrDefault(string key, string defaultValue)
        {
            if (Default._worldResource != null)
                return Default._worldResource.Resources.GetDataPathOrDefault(key, defaultValue);

            return Default._stockDefinitions.GetDataPathOrDefault(key, defaultValue);
        }

        public static Dictionary<string, byte> MaterialIndex
        {
            get
            {
                if (Default._worldResource != null)
                    return Default._worldResource.Resources.MaterialIndex;

                return Default._stockDefinitions.MaterialIndex;
            }
        }

        public static WorldResource WorldResource
        {
            get { return Default._worldResource; }
            set { Default._worldResource = value; }
        }

        public static List<string> ManageDeleteVoxelList
        {
            get { return Default._manageDeleteVoxelList; }
        }
    }
}
