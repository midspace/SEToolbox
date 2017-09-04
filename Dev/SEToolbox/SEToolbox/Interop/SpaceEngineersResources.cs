namespace SEToolbox.Interop
{
    using System.Collections.Generic;
    using System.Linq;
    using Sandbox.Definitions;
    using SEToolbox.Support;
    using VRage.Game;
    using VRage.ObjectBuilders;

    /// <summary>
    /// Encapsulates the game definitions, either stock or loaded for a specific save game world.
    /// </summary>
    public class SpaceEngineersResources
    {
        #region LoadDefinitions

        /// <summary>
        /// Loads Stock definitions from default path, useful for tests.
        /// </summary>
        public void LoadDefinitions()
        {
            MyDefinitionManager.Static.LoadData(new List<MyObjectBuilder_Checkpoint.ModItem>());
            MaterialIndex = new Dictionary<string, byte>();
        }

        public void LoadDefinitionsAndMods(string userModspath, MyObjectBuilder_Checkpoint.ModItem[] mods)
        {
            LoadDefinitionsAndMods(ToolboxUpdater.GetApplicationContentPath(), userModspath, mods);
        }

        public void LoadDefinitionsAndMods(string applicationContentPath, string userModspath, MyObjectBuilder_Checkpoint.ModItem[] mods)
        {
            MyDefinitionManager.Static.LoadData(mods.ToList());
            MaterialIndex = new Dictionary<string, byte>();
        }

        #endregion

        #region properties

        public Dictionary<string, byte> MaterialIndex { get; private set; }

        #endregion

        #region methods

        public string GetDataPathOrDefault(string key, string defaultValue)
        {
            //if (key != null && _contentDataPaths.ContainsKey(key.ToLower()))
            //{
            //    if (_contentDataPaths[key.ToLower()].AbsolutePath != null)
            //        return _contentDataPaths[key.ToLower()].AbsolutePath;

            //    if (_contentDataPaths[key.ToLower()].ZipFilePath != null)
            //    {
            //        var tempContentFile = TempfileUtil.NewFilename(Path.GetExtension(defaultValue));
            //        try
            //        {
            //            ZipTools.ExtractZipFileToFile(_contentDataPaths[key.ToLower()].ZipFilePath, null, _contentDataPaths[key.ToLower()].ReferencePath, tempContentFile);
            //            return tempContentFile;
            //        }
            //        catch (Exception ex)
            //        {
            //            // ignore errors, keep on trucking just like SE.
            //            // write event log warning of any files not loaded.
            //            DiagnosticsLogging.LogWarning(string.Format(Res.ExceptionState_CorruptModFile, _contentDataPaths[key.ToLower()].ZipFilePath), ex);
            //            return defaultValue;
            //        }
            //    }
            //}

            return defaultValue;
        }

        private static readonly object MatindexLock = new object();

        public byte GetMaterialIndex(string materialName)
        {
            lock (MatindexLock)
            {
                return MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialName).Index;
            }
        }

        public IList<MyBlueprintDefinitionBase> BlueprintDefinitions
        {
            get { return MyDefinitionManager.Static.GetBlueprintDefinitions().ToList(); }
        }

        public IList<MyCubeBlockDefinition> CubeBlockDefinitions
        {
            get { return MyDefinitionManager.Static.GetAllDefinitions().Where(e => e is MyCubeBlockDefinition).Cast<MyCubeBlockDefinition>().ToList(); }
        }

        public IList<MyComponentDefinition> ComponentDefinitions
        {
            get { return MyDefinitionManager.Static.GetPhysicalItemDefinitions().Where(e => e is MyComponentDefinition).Cast<MyComponentDefinition>().ToList(); }
        }

        public IList<MyPhysicalItemDefinition> PhysicalItemDefinitions
        {
            get { return MyDefinitionManager.Static.GetPhysicalItemDefinitions().Where(e => !(e is MyComponentDefinition)).ToList(); }
        }

        public IList<MyAmmoMagazineDefinition> AmmoMagazineDefinitions
        {
            get { return MyDefinitionManager.Static.GetAllDefinitions().Where(e => e is MyAmmoMagazineDefinition).Cast<MyAmmoMagazineDefinition>().ToList(); }
        }

        public IList<MyVoxelMaterialDefinition> VoxelMaterialDefinitions
        {
            get { return MyDefinitionManager.Static.GetVoxelMaterialDefinitions().ToList(); }
        }

        public IList<MyVoxelMapStorageDefinition> VoxelMapStorageDefinitions
        {
            get { return MyDefinitionManager.Static.GetVoxelMapStorageDefinitions().ToList(); }
        }

        public IList<MyCharacterDefinition> CharacterDefinitions
        {
            get { return MyDefinitionManager.Static.Characters.ToList(); }
        }

        public string GetMaterialName(byte materialIndex, byte defaultMaterialIndex)
        {
            if (materialIndex <= MyDefinitionManager.Static.GetVoxelMaterialDefinitions().Count)
                return MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialIndex).Id.SubtypeName;

            return MyDefinitionManager.Static.GetVoxelMaterialDefinition(defaultMaterialIndex).Id.SubtypeName;

            //if (materialIndex <= _definitions.VoxelMaterials.Length)
            //    return _definitions.VoxelMaterials[materialIndex].Id.SubtypeId;

            //return _definitions.VoxelMaterials[defaultMaterialIndex].Id.SubtypeId;
        }

        public string GetMaterialName(byte materialIndex)
        {
            return MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialIndex).Id.SubtypeName;
            //return _definitions.VoxelMaterials[materialIndex].Id.SubtypeId;
        }

        public string GetDefaultMaterialName()
        {
            return MyDefinitionManager.Static.GetDefaultVoxelMaterialDefinition().Id.SubtypeName;
        }

        public T CreateNewObject<T>()
            where T : MyObjectBuilder_Base
        {
            return (T)MyObjectBuilderSerializer.CreateNewObject(typeof(T));
        }

        public T CreateNewObject<T>(MyObjectBuilderType typeId, string subtypeId)
           where T : MyObjectBuilder_Base
        {
            return (T)MyObjectBuilderSerializer.CreateNewObject(typeId, subtypeId);
        }

        #endregion
    }
}
