namespace SEToolbox.Interop
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using SEToolbox.Models;
    using VRage.ObjectBuilders;
    using VRage.Plugins;

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
            //SpaceEngineersGame.SetupPerGameSettings(); // not required currently.
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //MyPlugins.RegisterGameAssemblyFile(Path.Combine(path, "SpaceEngineers.Game.dll")); // not required currently.
            MyPlugins.RegisterGameObjectBuildersAssemblyFile(Path.Combine(path, "SpaceEngineers.ObjectBuilders.dll"));
            MyPlugins.RegisterSandboxAssemblyFile(Path.Combine(path, "Sandbox.Common.dll"));
            //MyPlugins.RegisterSandboxGameAssemblyFile(Path.Combine(path, "Sandbox.Game.dll")); // not required currently.

            MyObjectBuilderType.RegisterAssemblies();
            MyObjectBuilderSerializer.RegisterAssembliesAndLoadSerializers();

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
