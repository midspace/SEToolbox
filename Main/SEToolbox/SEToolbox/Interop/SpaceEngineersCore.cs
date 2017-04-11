namespace SEToolbox.Interop
{
    using System.Collections.Generic;
    using Sandbox;
    using Sandbox.Engine.Utils;
    using SEToolbox.Models;
    using SpaceEngineers.Game;
    using Support;
    using VRage.FileSystem;
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
            MySandboxGame gameTemp = new MySandboxGame(null, null);

            SpaceEngineersApi.LoadLocalization();
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
