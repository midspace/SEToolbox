namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using SEToolbox.Models;
    using SpaceEngineers.Game;
    using Support;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Utils;
    using VRageRender;
    using MySteamServiceBase = VRage.Steam.MySteamService;

    /// <summary>
    /// core interop for loading up Space Engineers content.
    /// </summary>
    public class SpaceEngineersCore
    {
        protected static readonly uint AppId = 244850; // Game
        //protected static readonly uint AppId = 298740; // Dedicated Server

        #region fields

        public static SpaceEngineersCore Default = new SpaceEngineersCore();
        private WorldResource _worldResource;
        private readonly SpaceEngineersResources _stockDefinitions;
        private readonly List<string> _manageDeleteVoxelList;
        protected MyCommonProgramStartup _startup;
        private MySteamServiceBase _steamService;

        #endregion

        #region ctor

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
            _steamService = new MySteamService(MySandboxGame.IsDedicated, AppId);
            MyServiceManager.Instance.AddService<IMyGameService>(_steamService);
            
            MyFileSystem.InitUserSpecific(SpaceEngineersWorkshop.MySteam.UserId.ToString()); // This sets the save file/path to load games from.
            //MyFileSystem.InitUserSpecific(null);
            //SpaceEngineersWorkshop.MySteam.Dispose();

            MySandboxGame.Config = new MyConfig("SpaceEngineers.cfg"); // TODO: Is specific to SE, not configurable to ME.
            MySandboxGame.Config.Load();

            SpaceEngineersGame.SetupPerGameSettings();

            VRageRender.MyRenderProxy.Initialize(new MyNullRender());
            // We create a whole instance of MySandboxGame!
            // If this is causing an exception, then there is a missing dependency.
            MySandboxGame gameTemp = new MySandboxGame(new string[] { "-skipintro" });

            // creating MySandboxGame will reset the CurrentUICulture, so I have to reapply it.
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);
            SpaceEngineersApi.LoadLocalization();
            MyStorageBase.UseStorageCache = false;

            #region MySession creation

            // Replace the private constructor on MySession, so we can create it without getting involed with Havok and other depdancies.
            var keenStart = typeof(Sandbox.Game.World.MySession).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(MySyncLayer), typeof(bool) }, null);
            var ourStart = typeof(SEToolbox.Interop.MySession).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(MySyncLayer), typeof(bool) }, null);
            ReflectionUtil.ReplaceMethod(ourStart, keenStart);

            // Create an empty instance of MySession for use by low level code.
            Sandbox.Game.World.MySession mySession = ReflectionUtil.ConstructPrivateClass<Sandbox.Game.World.MySession>(new Type[0], new object[0]);
            ReflectionUtil.ConstructField(mySession, "m_sessionComponents"); // Required as the above code doesn't populate it during ctor of MySession.
            mySession.Settings = new MyObjectBuilder_SessionSettings { EnableVoxelDestruction = true };

            VRage.MyVRage.Init(new ToolboxPlatform());

            // change for the Clone() method to use XML cloning instead of Protobuf because of issues with MyObjectBuilder_CubeGrid.Clone()
            ReflectionUtil.SetFieldValue(typeof(VRage.ObjectBuilders.MyObjectBuilderSerializer), "ENABLE_PROTOBUFFERS_CLONING", false);

            // Assign the instance back to the static.
            Sandbox.Game.World.MySession.Static = mySession;

            Sandbox.Game.GameSystems.MyHeightMapLoadingSystem.Static = new MyHeightMapLoadingSystem();
            Sandbox.Game.GameSystems.MyHeightMapLoadingSystem.Static.LoadData();

            #endregion

            _stockDefinitions = new SpaceEngineersResources();
            _stockDefinitions.LoadDefinitions();
            _manageDeleteVoxelList = new List<string>();
        }

        #endregion

        #region LoadDefinitions

        /// <summary>
        /// Forces static ctor to load stock defintiions.
        /// </summary>
        public static void LoadDefinitions()
        {
        }

        #endregion

        #region properties

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

        #endregion
    }
}
