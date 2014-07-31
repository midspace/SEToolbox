namespace SEToolbox.Interop
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Res = SEToolbox.Properties.Resources;

    /// <summary>
    /// core interop for loading up Space Engineers content.
    /// </summary>
    public class SpaceEngineersCore
    {
        #region fields

        public static SpaceEngineersCore Default = new SpaceEngineersCore();
        private MyObjectBuilder_Definitions _definitions;
        private Dictionary<string, ContentDataPath> _contentDataPaths;
        private Dictionary<string, byte> _materialIndex;
        private string _lastUserModsPath;

        #endregion

        #region ctor

        public SpaceEngineersCore()
        {
            Sandbox.Common.Localization.MyTextsWrapper.Init();
        }

        #endregion

        public static void LoadStockDefinitions()
        {
            // Load Stock paths for tests.
            Default.ReadCubeBlockDefinitions(ToolboxUpdater.GetApplicationContentPath(), null);
        }

        public static void LoadDefinitions()
        {
            // Defined (default) path for initial startup.
            Default.ReadCubeBlockDefinitions(ToolboxUpdater.GetApplicationContentPath(), SpaceEngineersConsts.BaseLocalPath.ModsPath);
        }

        public static void LoadDefinitions(string userModPath)
        {
            Default.ReadCubeBlockDefinitions(ToolboxUpdater.GetApplicationContentPath(), userModPath);
        }

        #region ReadCubeBlockDefinitions

        private void ReadCubeBlockDefinitions(string contentPath, string userModspath)
        {
            // Dynamically read all definitions as soon as the SpaceEngineersAPI class is first invoked.
            if (this._definitions == null || _lastUserModsPath != userModspath)
            {
                FindDefinitions(ToolboxUpdater.GetApplicationContentPath(), userModspath, out _definitions, out _contentDataPaths);
                this._materialIndex = new Dictionary<string, byte>();
                this._lastUserModsPath = userModspath;
            }
        }

        /// <summary>
        /// Load all the Space Engineers data definitions.
        /// </summary>
        /// <param name="contentPath">allows redirection of path when switching between single player or server.</param>
        /// <param name="userModspath"></param>
        /// <param name="definitions"></param>
        /// <param name="contentData"></param>
        /// <returns></returns>
        private void FindDefinitions(string contentPath, string userModspath, out MyObjectBuilder_Definitions definitions, out Dictionary<string, ContentDataPath> contentData)
        {
            // Maintain a list of referenced files, prefabs, models, textures, etc., as they must also be found and mapped.
            // ie.,  "Models\Characters\Custom\Awesome_astronaut_model" might live under "Mods\Awesome_astronaut\Models\Characters\Custom\Awesome_astronaut_model".
            contentData = new Dictionary<string, ContentDataPath>();

            definitions = LoadAllDefinitions(contentPath);
            LoadContent(contentPath, definitions, ref contentData);

            if (!string.IsNullOrEmpty(userModspath) && Directory.Exists(userModspath))
            {
                // Read through the mod paths manually.
                // Using the MyObjectBuilder_Base.DeserializeXML() with MyFSLocationEnum.ContentWithMods does not work in the expected manner.

                var directories = Directory.GetDirectories(userModspath);
                foreach (var modDirectory in directories)
                {
                    var modDefinitions = LoadAllDefinitions(modDirectory);
                    LoadContent(modDirectory, modDefinitions, ref contentData);
                    MergeDefinitions(ref definitions, modDefinitions);
                }
            }

            // Verify that content data was available, just in case the .sbc files didn't exist.
            if (definitions.AmmoMagazines == null) throw new ToolboxException(ExceptionState.MissingContentFile, "AmmoMagazines.sbc");
            if (definitions.Blueprints == null) throw new ToolboxException(ExceptionState.MissingContentFile, "Blueprints.sbc");
            if (definitions.Characters == null) throw new ToolboxException(ExceptionState.MissingContentFile, "Characters.sbc");
            if (definitions.Components == null) throw new ToolboxException(ExceptionState.MissingContentFile, "Components.sbc");
            if (definitions.ContainerTypes == null) throw new ToolboxException(ExceptionState.MissingContentFile, "ContainerTypes.sbc");
            if (definitions.CubeBlocks == null) throw new ToolboxException(ExceptionState.MissingContentFile, "CubeBlocks.sbc");
            if (definitions.GlobalEvents == null) throw new ToolboxException(ExceptionState.MissingContentFile, "GlobalEvents.sbc");
            if (definitions.HandItems == null) throw new ToolboxException(ExceptionState.MissingContentFile, "HandItems.sbc");
            if (definitions.PhysicalItems == null) throw new ToolboxException(ExceptionState.MissingContentFile, "PhysicalItems.sbc");
            if (definitions.Scenarios == null) throw new ToolboxException(ExceptionState.MissingContentFile, "Scenarios.sbc");
            if (definitions.SpawnGroups == null) throw new ToolboxException(ExceptionState.MissingContentFile, "SpawnGroups.sbc");
            if (definitions.TransparentMaterials == null) throw new ToolboxException(ExceptionState.MissingContentFile, "TransparentMaterials.sbc");
            if (definitions.VoxelMaterials == null) throw new ToolboxException(ExceptionState.MissingContentFile, "VoxelMaterials.sbc");

            // Deal with duplicate entries. Must use the last one found and overwrite all others.
            definitions.AmmoMagazines = definitions.AmmoMagazines.GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.Blueprints = definitions.Blueprints.GroupBy(c => c.Result.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.Characters = definitions.Characters.GroupBy(c => c.Name).Select(c => c.Last()).ToArray();
            definitions.Components = definitions.Components.GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            // definitions.Configuration is not an array.
            definitions.ContainerTypes = definitions.ContainerTypes.GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.CubeBlocks = definitions.CubeBlocks.GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            // definitions.Environment is not an array.
            definitions.GlobalEvents = definitions.GlobalEvents.GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.HandItems = definitions.HandItems.GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.PhysicalItems = definitions.PhysicalItems.GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.Scenarios = definitions.Scenarios.GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            // definitions.SpawnGroups don't appear to have a unique idetifier.
            definitions.TransparentMaterials = definitions.TransparentMaterials.GroupBy(c => c.Name).Select(c => c.Last()).ToArray();
            definitions.VoxelMaterials = definitions.VoxelMaterials.GroupBy(c => c.Name).Select(c => c.Last()).ToArray();
        }

        private MyObjectBuilder_Definitions LoadAllDefinitions(string contentPath)
        {
            var searchPath = Path.Combine(contentPath, "Data");
            var definitions = new MyObjectBuilder_Definitions();

            if (!Directory.Exists(searchPath))
                return definitions;

            var files = Directory.GetFiles(searchPath, "*.sbc");

            foreach (var filePath in files)
            {
                MyObjectBuilder_Definitions stockTemp = null;
                bool isCompressed;
                try
                {
                    stockTemp = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Definitions>(filePath, out isCompressed);
                }
                catch (Exception ex)
                {
                    // ignore errors, keep on trucking just like SE.
                    // write event log warning of any files not loaded.
                    DiagnosticsLogging.LogWarning(string.Format(Res.ExceptionState_CorruptContentFile, filePath), ex);
                }

                if (stockTemp == null) continue;

                MergeDefinitions(ref definitions, stockTemp);
            }

            return definitions;
        }

        private void MergeDefinitions(ref MyObjectBuilder_Definitions baseDefinitions, MyObjectBuilder_Definitions newDefinitions)
        {
            var fields = newDefinitions.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var readValues = field.GetValue(newDefinitions);
                if (readValues != null)
                {
                    var currentValues = field.GetValue(baseDefinitions);
                    if (currentValues == null || !field.FieldType.IsArray)
                    {
                        field.SetValue(baseDefinitions, readValues);
                    }
                    else
                    {
                        // Merge together multiple values from seperate files.
                        var newArray = ArrayHelper.MergeGenericArrays(currentValues, readValues);
                        field.SetValue(baseDefinitions, newArray);
                    }
                }
            }
        }

        private void LoadContent(string contentPath, MyObjectBuilder_Definitions definitions, ref Dictionary<string, ContentDataPath> contentData)
        {
            // rechecks pre-existing contentData
            foreach (var kvp in contentData)
            {
                foreach(var referenceFile in kvp.Value.GetReferenceFiles())
                {
                    var contentFile = Path.Combine(contentPath, referenceFile);
                    if (File.Exists(contentFile))
                    {
                        kvp.Value.AbsolutePath = contentFile;
                    }
                }
            }

            var icons = new List<string>(); // .dds .png
            var models = new List<string>(); // .mwm

            if (definitions.AmmoMagazines != null)
            {
                icons.AddRange(definitions.AmmoMagazines.Select(magazine => magazine.Icon));
                models.AddRange(definitions.AmmoMagazines.Select(magazine => magazine.Model));
            }

            if (definitions.Characters != null)
            {
                models.AddRange(definitions.Characters.Select(character => character.Model));
            }

            if (definitions.Components != null)
            {
                icons.AddRange(definitions.Components.Select(component => component.Icon));
                models.AddRange(definitions.Components.Select(component => component.Model));
            }

            if (definitions.CubeBlocks != null)
            {
                icons.AddRange(definitions.CubeBlocks.Select(definition => definition.Icon));
                models.AddRange(definitions.CubeBlocks.Where(d => d.CubeDefinition != null && d.CubeDefinition.Sides != null).SelectMany(d => d.CubeDefinition.Sides).Select(side => side.Model));
                models.AddRange(definitions.CubeBlocks.Where(d => d.BuildProgressModels != null).SelectMany(d => d.BuildProgressModels).Select(model => model.File));
            }

            if (definitions.HandItems != null)
            {
                models.AddRange(definitions.HandItems.Select(handitem => handitem.FingersAnimation));
            }

            if (definitions.PhysicalItems != null)
            {
                icons.AddRange(definitions.PhysicalItems.Select(physicalItem => physicalItem.Icon));
                models.AddRange(definitions.PhysicalItems.Select(physicalItem => physicalItem.Model));
            }

            if (definitions.Scenarios != null)
            {
                icons.AddRange(definitions.Scenarios.Select(scenarioDefinition => scenarioDefinition.Icon));
            }

            if (definitions.Environment != null)
            {
                icons.Add(definitions.Environment.EnvironmentTexture);
            }

            if (definitions.VoxelMaterials != null)
            {
                icons.AddRange(definitions.VoxelMaterials.Select(voxelMaterial => @"Textures\Voxels\" + voxelMaterial.AssetName + "_ForAxisXZ_de"));
                icons.AddRange(definitions.VoxelMaterials.Select(voxelMaterial => @"Textures\Voxels\" + voxelMaterial.AssetName + "_ForAxisXZ_ns"));
            }

            var voxelsPath = Path.Combine(contentPath, @"Textures\Voxels");
            if (Directory.Exists(voxelsPath))
            {
                foreach (var filePath in Directory.GetFiles(voxelsPath, "*.dds"))
                {
                    var refPath = Path.Combine(@"Textures\Voxels", Path.GetFileNameWithoutExtension(filePath));
                    contentData.Update(refPath.ToLower(), new ContentDataPath(ContentPathType.Texture, refPath, filePath));
                }
            }

            foreach (var icon in icons.Where(s => !string.IsNullOrEmpty(s)).Select(s => s).Distinct())
            {
                var contentFile = Path.Combine(contentPath, icon + ".dds");

                if (File.Exists(contentFile) || !contentData.ContainsKey(icon.ToLower()))
                {
                    contentData.Update(icon.ToLower(), new ContentDataPath(ContentPathType.Texture, icon, contentFile));
                }
                else
                {
                    contentFile = Path.Combine(contentPath, icon + ".png");
                    if (File.Exists(contentFile) || !contentData.ContainsKey(icon.ToLower()))
                    {
                        contentData.Update(icon.ToLower(), new ContentDataPath(ContentPathType.Texture, icon, contentFile));
                    }
                }
            }

            foreach (var model in models.Where(m => !string.IsNullOrEmpty(m)).Select(m => m).Distinct())
                contentData.Update(model.ToLower(), new ContentDataPath(ContentPathType.Model, model, Path.Combine(contentPath, model + ".mwm")));

            var prefabPath = Path.Combine(contentPath, @"Data\Prefabs");
            if (Directory.Exists(prefabPath))
            {
                foreach (var filePath in Directory.GetFiles(prefabPath, "*.sbc"))
                {
                    var refPath = Path.Combine(@"Data\Prefabs", Path.GetFileName(filePath));
                    contentData.Update(refPath.ToLower(), new ContentDataPath(ContentPathType.SandboxContent, refPath, filePath));
                }

                foreach (var filePath in Directory.GetFiles(prefabPath, "*.sbs"))
                {
                    var refPath = Path.Combine(@"Data\Prefabs", Path.GetFileName(filePath));
                    contentData.Update(refPath.ToLower(), new ContentDataPath(ContentPathType.SandboxSector, refPath, filePath));
                }
            }

            // prefabs aren't defined with a directory name currently, so searching for them differing paths seems pointless.
            //var prefabContent = new List<string>(); // .sbc
            //prefabContent.AddRange(Directory.GetFiles(Path.Combine(contentPath, @"Data\Prefabs"), "*.sbc").Select(Path.GetFileNameWithoutExtension));
            //prefabContent.AddRange(definitions.Scenarios.Where(s => s.WorldGeneratorOperations != null).SelectMany(s => s.WorldGeneratorOperations).
            //    Where(o => o is MyObjectBuilder_WorldGeneratorOperation_SetupBasePrefab).
            //    Cast<MyObjectBuilder_WorldGeneratorOperation_SetupBasePrefab>().
            //    Select(o => o.PrefabFile));
            //prefabContent.AddRange(definitions.Scenarios.Where(s => s.WorldGeneratorOperations != null).SelectMany(s => s.WorldGeneratorOperations).
            //    Where(o => o is MyObjectBuilder_WorldGeneratorOperation_AddShipPrefab).
            //    Cast<MyObjectBuilder_WorldGeneratorOperation_AddShipPrefab>().
            //    Select(o => o.PrefabFile));
            //prefabContent.AddRange(definitions.SpawnGroups.SelectMany(g => g.Prefabs).Select(p => p.File));
            //prefabContent.Add(definitions.Configuration.BaseBlockPrefabs.SmallDynamic);
            //prefabContent.Add(definitions.Configuration.BaseBlockPrefabs.SmallStatic);
            //prefabContent.Add(definitions.Configuration.BaseBlockPrefabs.MediumDynamic);
            //prefabContent.Add(definitions.Configuration.BaseBlockPrefabs.MediumStatic);
            //prefabContent.Add(definitions.Configuration.BaseBlockPrefabs.LargeDynamic);
            //prefabContent.Add(definitions.Configuration.BaseBlockPrefabs.LargeStatic);


            // Find: "Data\Prefabs\*.sbs"
            //var prefabSectors = new List<string>(); // .sbs
            //prefabSectors = Directory.GetFiles(Path.Combine(contentPath, @"Data\Prefabs"), "*.sbs").Select(Path.GetFileNameWithoutExtension).ToList();
            // Scenarios/ScenarioDefinition/WorldGeneratorOperations/Operation xsi:type="AddObjectsPrefab"/PrefabFile


            // AddAsteroidPrefab.PrefabFile is an Enumeration currently, so it might be useless to consider tracking it for mods.
            //var voxelMaps = new List<string>(); // .vox
            // Find: "VoxelMaps\*.vox"
            //voxelMaps.AddRange(definitions.Scenarios.Where(s => s.WorldGeneratorOperations != null).SelectMany(s => s.WorldGeneratorOperations).
            //    Where(o => o is MyObjectBuilder_WorldGeneratorOperation_AddAsteroidPrefab).
            //    Cast<MyObjectBuilder_WorldGeneratorOperation_AddAsteroidPrefab>().
            //    Select(o => o.PrefabFile));
            //voxelMaps = voxelMaps.Where(s => !string.IsNullOrEmpty(s)).Select(s => s + ".vox").Distinct().ToList();
        }

        #endregion

        #region properties

        public static MyObjectBuilder_Definitions Definitions
        {
            get { return Default._definitions; }
        }

        public static Dictionary<string, ContentDataPath> ContentDataPaths
        {
            get { return Default._contentDataPaths; }
        }

        public static string GetDataPathOrDefault(string key, string defaultValue)
        {
            if (key != null && Default._contentDataPaths.ContainsKey(key.ToLower()))
                return Default._contentDataPaths[key.ToLower()].AbsolutePath;
            else
                return defaultValue;
        }

        public static Dictionary<string, byte> MaterialIndex
        {
            get { return Default._materialIndex; }
        }

        #endregion
    }
}
