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
        public static SpaceEngineersResources Resources
        {
            get => singleton._worldResource?.Resources ?? singleton._stockDefinitions;
        }

        public static Dictionary<string, byte> MaterialIndex
        {
            get => Resources.MaterialIndex;
        }

        public static WorldResource WorldResource
        {
            get => singleton._worldResource;
            set => singleton._worldResource = value;
        }

        public static List<string> ManageDeleteVoxelList
        {
            get => singleton._manageDeleteVoxelList;
        }

        public static string GetDataPathOrDefault(string key, string defaultValue)
        {
            return Resources.GetDataPathOrDefault(key, defaultValue);
        }

        /// <summary>
        /// Forces static ctor to load stock defintiions.
        /// </summary>
        public static void LoadDefinitions()
        {
            typeof(MyTexts).TypeInitializer.Invoke(null, null); // For tests

            singleton = new SpaceEngineersCore();
        }

        static SpaceEngineersCore singleton;

        WorldResource _worldResource;
        readonly SpaceEngineersResources _stockDefinitions;
        readonly List<string> _manageDeleteVoxelList;
        MyCommonProgramStartup _startup;
        IMyGameService _steamService;

        const uint AppId = 244850; // Game
        //const uint AppId = 298740; // Dedicated Server

        public SpaceEngineersCore()
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var userDataPath = SpaceEngineersConsts.BaseLocalPath.DataPath;

            MyFileSystem.ExePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(FastResourceLock)).Location);

            MyLog.Default = MySandboxGame.Log;
            SpaceEngineersGame.SetupBasicGameInfo();

            _startup = new MyCommonProgramStartup(Array.Empty<string>());

            //var appDataPath = _startup.GetAppDataPath();
            //MyInitializer.InvokeBeforeRun(AppId, MyPerGameSettings.BasicGameInfo.ApplicationName + "SEToolbox", appDataPath);
            //MyInitializer.InitCheckSum();

            MyFileSystem.Reset();
            MyFileSystem.Init(contentPath, userDataPath);

            // This will start the Steam Service, and Steam will think SE is running.
            // TODO: we don't want to be doing this all the while SEToolbox is running,
            // perhaps a once off during load to fetch of mods then disconnect/Dispose.
            _steamService = MySteamGameService.Create(Sandbox.Engine.Platform.Game.IsDedicated, AppId);
            MyServiceManager.Instance.AddService(_steamService);

            MyVRage.Init(new ToolboxPlatform());
            MyVRage.Platform.Init();

            IMyUGCService ugc = MySteamUgcService.Create(AppId, _steamService);
            //MyServiceManager.Instance.AddService(ugc);
            MyGameService.WorkshopService.AddAggregate(ugc);

            MyFileSystem.InitUserSpecific(_steamService.UserId.ToString()); // This sets the save file/path to load games from.
            //MyFileSystem.InitUserSpecific(null);
            //SpaceEngineersWorkshop.MySteam.Dispose();

            MySandboxGame.Config = new MyConfig("SpaceEngineers.cfg"); // TODO: Is specific to SE, not configurable to ME.
            MySandboxGame.Config.Load();

            SpaceEngineersGame.SetupPerGameSettings();
            MySandboxGame.InitMultithreading();
            MyRenderProxy.Initialize(new MyNullRender());

            // If this is causing an exception then there is a missing dependency.
            // gameTemp instance gets captured in MySandboxGame.Static
            MySandboxGame gameTemp = new DerivedGame(new string[] { "-skipintro" });

            // Creating MySandboxGame will reset the CurrentUICulture, so I have to reapply it.
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);
            SpaceEngineersApi.LoadLocalization();

            // Create an empty instance of MySession for use by low level code.
            var session = (Sandbox.Game.World.MySession)FormatterServices.GetUninitializedObject(typeof(Sandbox.Game.World.MySession));

            // Required as the above code doesn't populate it during ctor of MySession.
            ReflectionUtil.ConstructField(session, "CreativeTools");
            ReflectionUtil.ConstructField(session, "m_sessionComponents");
            ReflectionUtil.ConstructField(session, "m_sessionComponentsForUpdate");

            session.Settings = new MyObjectBuilder_SessionSettings { EnableVoxelDestruction = true };

            // Change for the Clone() method to use XML cloning instead of Protobuf because of issues with MyObjectBuilder_CubeGrid.Clone()
            ReflectionUtil.SetFieldValue(typeof(VRage.ObjectBuilders.MyObjectBuilderSerializer), "ENABLE_PROTOBUFFERS_CLONING", false);

            // Assign the instance back to the static.
            Sandbox.Game.World.MySession.Static = session;

            var heightmapSystem = new MyHeightMapLoadingSystem();
            session.RegisterComponent(heightmapSystem, heightmapSystem.UpdateOrder, heightmapSystem.Priority);
            heightmapSystem.LoadData();

            var planets = new MyPlanets();
            session.RegisterComponent(planets, heightmapSystem.UpdateOrder, heightmapSystem.Priority);
            planets.LoadData();

            _stockDefinitions = new SpaceEngineersResources();
            _stockDefinitions.LoadDefinitions();
            _manageDeleteVoxelList = new List<string>();
        }

        class DerivedGame : MySandboxGame
        {
            public DerivedGame(string[] commandlineArgs, IntPtr windowHandle = default)
                : base(commandlineArgs, windowHandle) { }

            protected override void InitializeRender(IntPtr windowHandle) { }
        }
    }
}
