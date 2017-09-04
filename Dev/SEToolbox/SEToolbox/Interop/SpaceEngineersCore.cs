namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using SEToolbox.Models;
    using SpaceEngineers.Game;
    using Support;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Utils;
    using VRageRender;

    /// <summary>
    /// core interop for loading up Space Engineers content.
    /// </summary>
    public class SpaceEngineersCore
    {
        #region fields

        public static SpaceEngineersCore Default = new SpaceEngineersCore();
        private WorldResource _worldResource;
        private readonly SpaceEngineersResources _stockDefinitions;
        private readonly List<string> _manageDeleteVoxelList;

        #endregion

        #region ctor

        public SpaceEngineersCore()
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            string userDataPath = SpaceEngineersConsts.BaseLocalPath.DataPath;

            MyFileSystem.Reset();
            MyFileSystem.Init(contentPath, userDataPath);

            MyLog.Default = MySandboxGame.Log;
            MySandboxGame.Config = new MyConfig("SpaceEngineers.cfg"); // TODO: Is specific to SE, not configurable to ME.
            MySandboxGame.Config.Load();

            MyFileSystem.InitUserSpecific(null);

            MyFakes.ENABLE_INFINARIO = false;

            SpaceEngineersGame.SetupPerGameSettings();

            VRageRender.MyRenderProxy.Initialize(new MyNullRender());
            // We create a whole instance of MySandboxGame!
            MySandboxGame gameTemp = new MySandboxGame(null);

            // creating MySandboxGame will reset the CurrentUICulture, so I have to reapply it.
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfoByIetfLanguageTag(GlobalSettings.Default.LanguageCode);
            SpaceEngineersApi.LoadLocalization();
            MyStorageBase.UseStorageCache = false;

            try
            {
                // Replace the private constructor on MySession, so we can create it without getting involed with Havok and other depdancies.
                var keenStart = typeof(Sandbox.Game.World.MySession).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(MySyncLayer), typeof(bool) }, null);
                var ourStart = typeof(SEToolbox.Interop.MySession).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(MySyncLayer), typeof(bool) }, null);
                ReflectionUtil.ReplaceMethod(ourStart, keenStart);


                // Create an empty instance of MySession for use by low level code.
                ConstructorInfo constructorInfo = typeof(Sandbox.Game.World.MySession).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
                object mySession = constructorInfo.Invoke(new object[0]);

                // Assign the instance back to the static.
                Sandbox.Game.World.MySession.Static = (Sandbox.Game.World.MySession)mySession;
                Sandbox.Game.World.MySession.Static.Settings = new MyObjectBuilder_SessionSettings { EnableVoxelDestruction = true };
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

            try
            {
                Sandbox.Game.GameSystems.MyHeightMapLoadingSystem.Static = new MyHeightMapLoadingSystem();
                Sandbox.Game.GameSystems.MyHeightMapLoadingSystem.Static.LoadData();
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }

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
