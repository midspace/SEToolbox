namespace SEToolbox.Interop
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Support;
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

        #endregion

        #region ctor

        public SpaceEngineersCore()
        {
            Sandbox.Common.Localization.MyTextsWrapper.Init();
        }

        #endregion

        /// <summary>
        /// Loads Stock definitions from default path, useful for tests.
        /// </summary>
        public static void LoadDefinitions()
        {
            Default.ReadCubeBlockDefinitions(ToolboxUpdater.GetApplicationContentPath(), null);
        }

        /// <summary>
        /// Loads Stock definitions from specified path, useful for tests.
        /// </summary>
        /// <param name="contentPath"></param>
        public static void LoadDefinitions(string contentPath)
        {
            Default.ReadCubeBlockDefinitions(contentPath, null);
        }

        public static void LoadDefinitionsAndMods(params string[] modPaths)
        {
            Default.ReadCubeBlockDefinitions(ToolboxUpdater.GetApplicationContentPath(), modPaths);
        }

        public static void LoadDefinitionsAndMods(string userModspath, MyObjectBuilder_Checkpoint.ModItem[] mods)
        {
            LoadDefinitionsAndMods(ToolboxUpdater.GetApplicationContentPath(), userModspath, mods);
        }

        public static void LoadDefinitionsAndMods(string applicationContentPath, string userModspath, MyObjectBuilder_Checkpoint.ModItem[] mods)
        {
            var modPaths = new List<string>();

            if (mods != null && !string.IsNullOrEmpty(userModspath) && Directory.Exists(userModspath))
            {
                // Read through the mod paths manually.
                // Using the MyObjectBuilder_Base.DeserializeXML() with MyFSLocationEnum.ContentWithMods does not work in the expected manner.

                foreach (MyObjectBuilder_Checkpoint.ModItem mod in mods)
                {
                    if (mod.PublishedFileId != 0)
                    {
                        modPaths.Add(Path.Combine(userModspath, string.Format("{0}.sbm", mod.PublishedFileId)));
                    }
                    else if (mod.PublishedFileId == 0 && !string.IsNullOrEmpty(mod.Name))
                    {
                        modPaths.Add(Path.Combine(userModspath, mod.Name));
                    }
                }
            }

            Default.ReadCubeBlockDefinitions(applicationContentPath, modPaths.ToArray());
        }

        #region ReadCubeBlockDefinitions

        private void ReadCubeBlockDefinitions(string contentPath, string[] modPaths)
        {
            // Dynamically read all definitions as soon as the SpaceEngineersAPI class is first invoked.
            FindDefinitions(contentPath, modPaths, out _definitions, out _contentDataPaths);
            _materialIndex = new Dictionary<string, byte>();
        }

        /// <summary>
        /// Load all the Space Engineers data definitions.
        /// </summary>
        /// <param name="contentPath">allows redirection of path when switching between single player or server.</param>
        /// <param name="modPaths"></param>
        /// <param name="definitions"></param>
        /// <param name="contentData"></param>
        /// <returns></returns>
        private void FindDefinitions(string contentPath, IEnumerable<string> modPaths, out MyObjectBuilder_Definitions definitions, out Dictionary<string, ContentDataPath> contentData)
        {
            // Maintain a list of referenced files, prefabs, models, textures, etc., as they must also be found and mapped.
            // ie.,  "Models\Characters\Custom\Awesome_astronaut_model" might live under "Mods\Awesome_astronaut\Models\Characters\Custom\Awesome_astronaut_model".
            contentData = new Dictionary<string, ContentDataPath>();

            definitions = LoadAllDefinitions(contentPath, contentPath, ref contentData);

            if (modPaths != null)
            {
                foreach (var modPath in modPaths)
                {
                    var filePath = Environment.ExpandEnvironmentVariables(modPath);

                    if (File.Exists(filePath))
                    {
                        var modDefinitions = LoadAllDefinitionsZip(contentPath, filePath, ref contentData);
                        MergeDefinitions(ref definitions, modDefinitions);
                    }
                    else if (Directory.Exists(filePath))
                    {
                        var modDefinitions = LoadAllDefinitions(contentPath, filePath, ref contentData);
                        MergeDefinitions(ref definitions, modDefinitions);
                    }
                }
            }

            // Verify that content data was available, just in case the .sbc files didn't exist.
            if (definitions.AmmoMagazines == null) throw new ToolboxException(ExceptionState.MissingContentFile, "AmmoMagazines.sbc");
            if (definitions.Animations == null) throw new ToolboxException(ExceptionState.MissingContentFile, "Animations.sbc");
            if (definitions.Blueprints == null) throw new ToolboxException(ExceptionState.MissingContentFile, "Blueprints.sbc");
            if (definitions.Characters == null) throw new ToolboxException(ExceptionState.MissingContentFile, "Characters.sbc");
            if (definitions.Components == null) throw new ToolboxException(ExceptionState.MissingContentFile, "Components.sbc");
            if (definitions.ContainerTypes == null) throw new ToolboxException(ExceptionState.MissingContentFile, "ContainerTypes.sbc");
            if (definitions.CubeBlocks == null) throw new ToolboxException(ExceptionState.MissingContentFile, "CubeBlocks.sbc");
            if (definitions.GlobalEvents == null) throw new ToolboxException(ExceptionState.MissingContentFile, "GlobalEvents.sbc");
            if (definitions.HandItems == null) throw new ToolboxException(ExceptionState.MissingContentFile, "HandItems.sbc");
            if (definitions.PhysicalItems == null) throw new ToolboxException(ExceptionState.MissingContentFile, "PhysicalItems.sbc");
            if (definitions.SpawnGroups == null) throw new ToolboxException(ExceptionState.MissingContentFile, "SpawnGroups.sbc");
            if (definitions.TransparentMaterials == null) throw new ToolboxException(ExceptionState.MissingContentFile, "TransparentMaterials.sbc");
            if (definitions.VoxelMaterials == null) throw new ToolboxException(ExceptionState.MissingContentFile, "VoxelMaterials.sbc");

            // Deal with duplicate entries. Must use the last one found and overwrite all others.
            definitions.AmmoMagazines = definitions.AmmoMagazines.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.Animations = definitions.Animations.Where(c => !c.Id.TypeId.IsNull).GroupBy(a => a.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.Blueprints = definitions.Blueprints.Where(c => !c.Result.Id.TypeId.IsNull).GroupBy(c => c.Result.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.Characters = definitions.Characters.GroupBy(c => c.Name).Select(c => c.Last()).ToArray();
            definitions.Components = definitions.Components.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            // definitions.Configuration is not an array.
            definitions.ContainerTypes = definitions.ContainerTypes.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.CubeBlocks = definitions.CubeBlocks.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            // definitions.Environment is not an array.
            definitions.GlobalEvents = definitions.GlobalEvents.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.HandItems = definitions.HandItems.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.PhysicalItems = definitions.PhysicalItems.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.SpawnGroups = definitions.SpawnGroups.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.TransparentMaterials = definitions.TransparentMaterials.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
            definitions.VoxelMaterials = definitions.VoxelMaterials.Where(c => !c.Id.TypeId.IsNull).GroupBy(c => c.Id.ToString()).Select(c => c.Last()).ToArray();
        }

        private MyObjectBuilder_Definitions LoadAllDefinitions(string stockContentPath, string modContentPath, ref Dictionary<string, ContentDataPath> contentData)
        {
            var searchPath = Path.Combine(modContentPath, "Data");
            var definitions = new MyObjectBuilder_Definitions();

            if (!Directory.Exists(searchPath))
                return definitions;

            var files = Directory.GetFiles(searchPath, "*.sbc");

            foreach (var filePath in files)
            {
                MyObjectBuilder_Definitions stockTemp = null;
                try
                {
                    bool isCompressed;
                    stockTemp = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Definitions>(filePath, out isCompressed);
                }
                catch (Exception ex)
                {
                    // ignore errors, keep on trucking just like SE.
                    // write event log warning of any files not loaded.
                    DiagnosticsLogging.LogWarning(string.Format(Res.ExceptionState_CorruptContentFile, filePath), ex);
                }

                if (stockTemp != null)
                    MergeDefinitions(ref definitions, stockTemp);
            }

            LoadContent(stockContentPath, modContentPath, null, null, definitions, ref contentData);

            return definitions;
        }

        private MyObjectBuilder_Definitions LoadAllDefinitionsZip(string stockContentPath, string zipModFile, ref Dictionary<string, ContentDataPath> contentData)
        {
            var zipFiles = ZipTools.ExtractZipContentList(zipModFile, null);
            var definitions = new MyObjectBuilder_Definitions();

            if (!zipFiles.Any(f => Path.GetDirectoryName(f).Equals("Data", StringComparison.InvariantCultureIgnoreCase)))
            {
                return definitions;
            }

            var files = zipFiles.Where(f => Path.GetDirectoryName(f).Equals("Data", StringComparison.InvariantCultureIgnoreCase) && Path.GetExtension(f).Equals(".sbc", StringComparison.InvariantCultureIgnoreCase)).ToArray();

            foreach (var filePath in files)
            {
                MyObjectBuilder_Definitions stockTemp = null;
                try
                {
                    using (var stream = ZipTools.ExtractZipFileToSteam(zipModFile, null, filePath))
                    {
                        bool isCompressed;
                        stockTemp = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Definitions>(stream, out isCompressed);
                    }
                }
                catch (Exception ex)
                {
                    // ignore errors, keep on trucking just like SE.
                    // write event log warning of any files not loaded.
                    DiagnosticsLogging.LogWarning(string.Format(Res.ExceptionState_CorruptContentFile, filePath), ex);
                }

                if (stockTemp != null)
                    MergeDefinitions(ref definitions, stockTemp);
            }

            LoadContent(stockContentPath, null, zipModFile, zipFiles, definitions, ref contentData);

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

        private void LoadContent(string stockContentPath, string modContentPath, string zipModFile, string[] zipFiles, MyObjectBuilder_Definitions definitions, ref Dictionary<string, ContentDataPath> contentData)
        {
            // rechecks pre-existing contentData
            foreach (var kvp in contentData)
            {
                if (modContentPath != null)
                {
                    var contentFile = Path.Combine(modContentPath, kvp.Value.ReferencePath);
                    if (File.Exists(contentFile))
                    {
                        kvp.Value.AbsolutePath = contentFile;
                        kvp.Value.ZipFilePath = null;
                    }
                }
                else if (zipModFile != null)
                {
                    var contentFile = zipFiles.FirstOrDefault(f => f.Equals(kvp.Value.ReferencePath, StringComparison.InvariantCultureIgnoreCase));
                    if (contentFile != null)
                    {
                        kvp.Value.ZipFilePath = zipModFile;
                        kvp.Value.AbsolutePath = null;
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

            if (definitions.Environment != null)
            {
                icons.Add(definitions.Environment.EnvironmentTexture);
            }

            if (definitions.VoxelMaterials != null)
            {
                icons.AddRange(definitions.VoxelMaterials.Where(vm => !string.IsNullOrEmpty(vm.DiffuseXZ)).Select(vm => vm.DiffuseXZ));
                icons.AddRange(definitions.VoxelMaterials.Where(vm => !string.IsNullOrEmpty(vm.DiffuseY)).Select(vm => vm.DiffuseY));
                icons.AddRange(definitions.VoxelMaterials.Where(vm => !string.IsNullOrEmpty(vm.NormalXZ)).Select(vm => vm.NormalXZ));
                icons.AddRange(definitions.VoxelMaterials.Where(vm => !string.IsNullOrEmpty(vm.NormalY)).Select(vm => vm.NormalY));
            }

            if (modContentPath != null)
            {
                var voxelsPath = Path.Combine(modContentPath, @"Textures\Voxels");
                if (Directory.Exists(voxelsPath))
                {
                    foreach (var filePath in Directory.GetFiles(voxelsPath, "*.dds"))
                    {
                        // TODO: check if it exists in the mod or not.
                        var refPath = Path.Combine(@"Textures\Voxels", Path.GetFileName(filePath));
                        contentData.Update(refPath.ToLower(), new ContentDataPath(ContentPathType.Texture, refPath, filePath, zipModFile));
                    }
                }
            }
            else if (zipModFile != null)
            {
                var files = zipFiles.Where(f => Path.GetDirectoryName(f).Equals(@"Textures\Voxels", StringComparison.InvariantCultureIgnoreCase) && Path.GetExtension(f).Equals(".dds", StringComparison.InvariantCultureIgnoreCase)).ToArray();

                foreach (var refPath in files)
                {
                    // TODO: check if it exists in the mod or not.
                    contentData.Update(refPath.ToLower(), new ContentDataPath(ContentPathType.Texture, refPath, null, zipModFile));
                }
            }

            foreach (var icon in icons.Where(s => !string.IsNullOrEmpty(s)).Select(s => s).Distinct())
            {
                if (modContentPath != null)
                {
                    var contentFile = Path.Combine(modContentPath, icon);

                    // if the content exists, add/update it.
                    if (File.Exists(contentFile))
                    {
                        contentData.Update(icon.ToLower(), new ContentDataPath(ContentPathType.Texture, icon, contentFile, null));
                    }
                    else if (!contentData.ContainsKey(icon.ToLower()))
                    {
                        // doesn't exist in this mod, assume it's stock icon and add/update it anyhow.
                        contentFile = Path.Combine(stockContentPath, icon);
                        contentData.Update(icon.ToLower(), new ContentDataPath(ContentPathType.Texture, icon, contentFile, null));
                    }
                }
                else if (zipModFile != null)
                {
                    var contentFile = zipFiles.FirstOrDefault(f => f.Equals(icon, StringComparison.InvariantCultureIgnoreCase));

                    // if the content exists, add/update it.
                    if (contentFile != null)
                    {
                        contentData.Update(icon.ToLower(), new ContentDataPath(ContentPathType.Texture, icon, null, zipModFile));
                    }
                    else if (!contentData.ContainsKey(icon.ToLower()))
                    {
                        // doesn't exist in this mod, assume it's stock icon and add/update it anyhow.
                        contentFile = Path.Combine(stockContentPath, icon);
                        contentData.Update(icon.ToLower(), new ContentDataPath(ContentPathType.Texture, icon, contentFile, null));
                    }
                }
            }

            foreach (var model in models.Where(m => !string.IsNullOrEmpty(m)).Select(m => m).Distinct())
            {
                if (modContentPath != null)
                {
                    // TODO: check if it exists in the mod or not.
                    contentData.Update(model.ToLower(), new ContentDataPath(ContentPathType.Model, model, Path.Combine(modContentPath, model), zipModFile));
                }
                else if (zipModFile != null)
                {
                    // TODO: check if it exists in the mod or not.
                    contentData.Update(model.ToLower(), new ContentDataPath(ContentPathType.Model, model, null, zipModFile));
                }
            }

            if (modContentPath != null)
            {
                var prefabPath = Path.Combine(modContentPath, @"Data\Prefabs");
                if (Directory.Exists(prefabPath))
                {
                    foreach (var filePath in Directory.GetFiles(prefabPath, "*.sbc"))
                    {
                        var refPath = Path.Combine(@"Data\Prefabs", Path.GetFileName(filePath));
                        contentData.Update(refPath.ToLower(), new ContentDataPath(ContentPathType.SandboxContent, refPath, filePath, zipModFile));
                    }

                    foreach (var filePath in Directory.GetFiles(prefabPath, "*.sbs"))
                    {
                        var refPath = Path.Combine(@"Data\Prefabs", Path.GetFileName(filePath));
                        contentData.Update(refPath.ToLower(), new ContentDataPath(ContentPathType.SandboxSector, refPath, filePath, zipModFile));
                    }
                }
            }
            else if (zipModFile != null)
            {
                var files = zipFiles.Where(f => Path.GetDirectoryName(f).Equals(@"Data\Prefabs", StringComparison.InvariantCultureIgnoreCase) && Path.GetExtension(f).Equals(".sbc", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                foreach (var refPath in files)
                {
                    contentData.Update(refPath.ToLower(), new ContentDataPath(ContentPathType.SandboxContent, refPath, null, zipModFile));
                }

                files = zipFiles.Where(f => Path.GetDirectoryName(f).Equals(@"Data\Prefabs", StringComparison.InvariantCultureIgnoreCase) && Path.GetExtension(f).Equals(".sbs", StringComparison.InvariantCultureIgnoreCase)).ToArray();
                foreach (var refPath in files)
                {
                    contentData.Update(refPath.ToLower(), new ContentDataPath(ContentPathType.SandboxContent, refPath, null, zipModFile));
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
            {
                if (Default._contentDataPaths[key.ToLower()].AbsolutePath != null)
                    return Default._contentDataPaths[key.ToLower()].AbsolutePath;
                
                if (Default._contentDataPaths[key.ToLower()].ZipFilePath != null)
                {
                    var tempContentFile = TempfileUtil.NewFilename(Path.GetExtension(defaultValue));
                    ZipTools.ExtractZipFileToFile(Default._contentDataPaths[key.ToLower()].ZipFilePath, null, Default._contentDataPaths[key.ToLower()].ReferencePath, tempContentFile);
                    return tempContentFile;
                }
            }
            
            return defaultValue;
        }

        public static Dictionary<string, byte> MaterialIndex
        {
            get { return Default._materialIndex; }
        }

        #endregion
    }
}
