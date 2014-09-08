namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.XPath;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Converters;
    using SEToolbox.Interop;
    using SEToolbox.Support;

    public class SelectWorldModel : BaseModel
    {
        #region Fields

        private SaveResource _selectedWorld;

        private ObservableCollection<SaveResource> _worlds;

        private bool _isBusy;

        #endregion

        #region ctor

        public SelectWorldModel()
        {
            SelectedWorld = null;
            Worlds = new ObservableCollection<SaveResource>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        public UserDataPath BaseLocalPath { get; set; }

        public UserDataPath BaseDedicatedServerHostPath { get; set; }

        public UserDataPath BaseDedicatedServerServicePath { get; set; }

        public bool ReloadModsRequired { get; set; }

        public SaveResource SelectedWorld
        {
            get
            {
                return _selectedWorld;
            }

            set
            {
                if (value != _selectedWorld)
                {
                    _selectedWorld = value;
                    RaisePropertyChanged(() => SelectedWorld);
                }
            }
        }

        public ObservableCollection<SaveResource> Worlds
        {
            get
            {
                return _worlds;
            }

            set
            {
                if (value != _worlds)
                {
                    _worlds = value;
                    RaisePropertyChanged(() => Worlds);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    RaisePropertyChanged(() => IsBusy);
                    if (_isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        #endregion

        #region methods

        public void Load(UserDataPath baseLocalPath, UserDataPath baseDedicatedServerHostPath, UserDataPath baseDedicatedServerServicePath)
        {
            BaseLocalPath = baseLocalPath;
            BaseDedicatedServerHostPath = baseDedicatedServerHostPath;
            BaseDedicatedServerServicePath = baseDedicatedServerServicePath;
            LoadSaveList();
        }

        public void Refresh()
        {
            LoadSaveList();
        }

        public string RepairSandBox()
        {
            var str = new StringBuilder();
            var statusNormal = true;
            var missingFiles = false;
            var saveAfterScan = false;

            var model = new ExplorerModel
            {
                ActiveWorld = SelectedWorld
            };

            model.ActiveWorld.LoadCheckpoint();

            var xDoc = model.RepairerLoadSandBoxXml();
            if (xDoc == null)
            {
                str.AppendLine("! Checkpoint file is missing or broken.");
                str.AppendLine("! Unable to repair.");
                missingFiles = true;
            }
            else
            {
                var nsManager = xDoc.BuildXmlNamespaceManager();
                var nav = xDoc.CreateNavigator();

                var shipNodes = nav.Select("MyObjectBuilder_Sector/SectorObjects/MyObjectBuilder_EntityBase[@xsi:type='MyObjectBuilder_CubeGrid']", nsManager);
                while (shipNodes.MoveNext())
                {
                    var groupBlocksNode = shipNodes.Current.SelectSingleNode("BlockGroups/MyObjectBuilder_BlockGroup/Blocks", nsManager);
                    if (groupBlocksNode != null)
                    {
                        var entityIdNodes = groupBlocksNode.Select("long", nsManager);
                        var removeNodes = new List<XPathNavigator>();
                        while (entityIdNodes.MoveNext())
                        {
                            var entityId = Convert.ToInt64(entityIdNodes.Current.Value);
                            var node = shipNodes.Current.SelectSingleNode(string.Format("CubeBlocks/*[./EntityId='{0}']", entityId), nsManager);
                            if (node != null)
                            {
                                var x = node.ToValue<string>("Min/@x");
                                var y = node.ToValue<string>("Min/@y");
                                var z = node.ToValue<string>("Min/@z");

                                entityIdNodes.Current.InsertBefore(string.Format("<Vector3I><X>{0}</X><Y>{1}</Y><Z>{2}</Z></Vector3I>", x, y, z));
                                removeNodes.Add(entityIdNodes.Current.Clone());
                                str.AppendLine("* Replaced BlockGroup item.");
                                saveAfterScan = true;
                                statusNormal = false;
                            }
                        }

                        foreach (var node in removeNodes)
                        {
                            node.DeleteSelf();
                        }
                    }
                }

                //<BlockGroups>
                //<MyObjectBuilder_BlockGroup>
                //    <Name>Open</Name>
                //    <Blocks>
                //    <long>-2287829012813351669</long>
                //    <long>-1828477283611406765</long>
                //    <long>73405095007807299</long>
                //    <long>-8785290580748247313</long>
                //    </Blocks>
                //</MyObjectBuilder_BlockGroup>
                //</BlockGroups>

                //<BlockGroups>
                //<MyObjectBuilder_BlockGroup>
                //    <Name>Open</Name>
                //    <Blocks>
                //    <Vector3I>
                //        <X>-1</X>
                //        <Y>2</Y>
                //        <Z>-4</Z>
                //    </Vector3I>
                //    <Vector3I>
                //        <X>-1</X>
                //        <Y>7</Y>
                //        <Z>2</Z>
                //    </Vector3I>
                //    <Vector3I>
                //        <X>-1</X>
                //        <Y>8</Y>
                //        <Z>-9</Z>
                //    </Vector3I>
                //    <Vector3I>
                //        <X>-1</X>
                //        <Y>13</Y>
                //        <Z>-3</Z>
                //    </Vector3I>
                //    </Blocks>
                //</MyObjectBuilder_BlockGroup>
                //</BlockGroups>

                if (saveAfterScan)
                {
                    model.RepairerSaveSandBoxXml(xDoc);
                    str.AppendLine("* Saved changes.");
                }
            }

            SpaceEngineersCore.LoadDefinitionsAndMods(model.ActiveWorld.DataPath.ModsPath, model.ActiveWorld.Content.Mods.ToArray());
            ReloadModsRequired = true;
            model.LoadSandBox();

            if (model.ActiveWorld.Content == null)
            {
                statusNormal = false;
                str.AppendLine("! Checkpoint file is missing or broken.");
                str.AppendLine("! Unable to repair.");
                missingFiles = true;
            }

            if (model.SectorData == null)
            {
                statusNormal = false;
                str.AppendLine("! Sector file is missing or broken.");
                str.AppendLine("! Unable to repair.");
                missingFiles = true;
            }

            if (!missingFiles)
            {
                if (SelectedWorld.SaveType == SaveWorldType.Local && model.ThePlayerCharacter == null)
                {
                    statusNormal = false;
                    str.AppendLine("! No active Player in Save content.");

                    var character = model.FindAstronautCharacter();
                    if (character != null)
                    {
                        model.ActiveWorld.Content.ControlledObject = character.EntityId;
                        model.ActiveWorld.Content.CameraController = MyCameraControllerEnum.Entity;
                        model.ActiveWorld.Content.CameraEntity = character.EntityId;
                        str.AppendLine("* Found and Set new active Player.");
                        model.SaveCheckPointAndSandBox();
                        str.AppendLine("* Saved changes.");
                    }
                    else
                    {
                        var cockpit = model.FindPilotCharacter();
                        if (cockpit != null)
                        {
                            model.ActiveWorld.Content.ControlledObject = cockpit.EntityId;
                            model.ActiveWorld.Content.CameraController = MyCameraControllerEnum.ThirdPersonSpectator;
                            model.ActiveWorld.Content.CameraEntity = 0;
                            str.AppendLine("* Found and Set new active Player.");
                            model.SaveCheckPointAndSandBox();
                            str.AppendLine("* Saved changes.");
                        }
                        else
                        {
                            str.AppendLine("! Could not find any Player Characters.");
                            character = new MyObjectBuilder_Character();
                            character.EntityId = SpaceEngineersApi.GenerateEntityId();
                            character.PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene;
                            character.PositionAndOrientation = new MyPositionAndOrientation(VRageMath.Vector3.Zero, VRageMath.Vector3.Forward, VRageMath.Vector3.Up);
                            character.CharacterModel = SpaceEngineersCore.Definitions.Characters[0].Name;
                            character.ColorMaskHSV = new Sandbox.Common.ObjectBuilders.VRageData.SerializableVector3(0, -1, 1); // White
                            character.Battery = new MyObjectBuilder_Battery { CurrentCapacity = 0.5f };
                            character.LightEnabled = false;
                            character.HeadAngle = new VRageMath.Vector2();
                            character.LinearVelocity = new VRageMath.Vector3();
                            character.AutoenableJetpackDelay = -1;
                            character.JetpackEnabled = true;
                            character.Inventory = (MyObjectBuilder_Inventory)MyObjectBuilder_Base.CreateNewObject(typeof(MyObjectBuilder_Inventory));

                            // Add default items to Inventory.
                            MyObjectBuilder_InventoryItem item;
                            MyObjectBuilder_EntityBase gunEntity;

                            character.Inventory.Items.Add(item = (MyObjectBuilder_InventoryItem)MyObjectBuilder_Base.CreateNewObject(typeof(MyObjectBuilder_InventoryItem)));
                            item.Amount = 1;
                            item.ItemId = 0;
                            item.Content = new MyObjectBuilder_Welder();
                            gunEntity = (MyObjectBuilder_EntityBase)MyObjectBuilder_Base.CreateNewObject(typeof(MyObjectBuilder_Welder));
                            gunEntity.EntityId = SpaceEngineersApi.GenerateEntityId();
                            gunEntity.PersistentFlags = MyPersistentEntityFlags2.None;
                            ((MyObjectBuilder_PhysicalGunObject)item.PhysicalContent).GunEntity = gunEntity;

                            character.Inventory.Items.Add(item = (MyObjectBuilder_InventoryItem)MyObjectBuilder_Base.CreateNewObject(typeof(MyObjectBuilder_InventoryItem)));
                            item.Amount = 1;
                            item.ItemId = 1;
                            item.Content = new MyObjectBuilder_AngleGrinder();
                            gunEntity = (MyObjectBuilder_EntityBase)MyObjectBuilder_Base.CreateNewObject(typeof(MyObjectBuilder_AngleGrinder));
                            gunEntity.EntityId = SpaceEngineersApi.GenerateEntityId();
                            gunEntity.PersistentFlags = MyPersistentEntityFlags2.None;
                            ((MyObjectBuilder_PhysicalGunObject)item.PhysicalContent).GunEntity = gunEntity;

                            character.Inventory.Items.Add(item = (MyObjectBuilder_InventoryItem)MyObjectBuilder_Base.CreateNewObject(typeof(MyObjectBuilder_InventoryItem)));
                            item.Amount = 1;
                            item.ItemId = 2;
                            item.Content = new MyObjectBuilder_HandDrill();
                            gunEntity = (MyObjectBuilder_EntityBase)MyObjectBuilder_Base.CreateNewObject(typeof(MyObjectBuilder_HandDrill));
                            gunEntity.EntityId = SpaceEngineersApi.GenerateEntityId();
                            gunEntity.PersistentFlags = MyPersistentEntityFlags2.None;
                            ((MyObjectBuilder_PhysicalGunObject)item.PhysicalContent).GunEntity = gunEntity;

                            character.Inventory.Items.Add(item = (MyObjectBuilder_InventoryItem)MyObjectBuilder_Base.CreateNewObject(typeof(MyObjectBuilder_InventoryItem)));
                            item.Amount = 1;
                            item.ItemId = 3;
                            item.Content = new MyObjectBuilder_AutomaticRifle();
                            gunEntity = (MyObjectBuilder_EntityBase)MyObjectBuilder_Base.CreateNewObject(typeof(MyObjectBuilder_AutomaticRifle));
                            gunEntity.EntityId = SpaceEngineersApi.GenerateEntityId();
                            gunEntity.PersistentFlags = MyPersistentEntityFlags2.None;
                            ((MyObjectBuilder_PhysicalGunObject)item.PhysicalContent).GunEntity = gunEntity;

                            model.ActiveWorld.Content.ControlledObject = character.EntityId;
                            model.ActiveWorld.Content.CameraController = MyCameraControllerEnum.Entity;
                            model.ActiveWorld.Content.CameraEntity = character.EntityId;

                            model.SectorData.SectorObjects.Add(character);

                            str.AppendLine("* Created new active Player.");
                            model.SaveCheckPointAndSandBox();
                            str.AppendLine("* Saved changes.");
                        }
                    }
                }

                saveAfterScan = false;

                // Make sure the character in a locally saved world has all tools.
                if (SelectedWorld.SaveType == SaveWorldType.Local)
                {
                    // SubtypeNames for required tools.
                    var requiredItems = new[] { 
                        "WelderItem", "AngleGrinderItem", "HandDrillItem" };

                    var character = model.FindAstronautCharacter()
                        ?? model.FindPilotCharacter().Pilot;

                    MyObjectBuilder_Inventory inventory = character.Inventory;
                    requiredItems.ForEach(
                        delegate(string subtypeName)
                        {
                            if (!inventory.Items.Any(i =>
                                i.PhysicalContent != null &&
                                i.PhysicalContent.SubtypeName == subtypeName))
                            {
                                statusNormal = false;
                                str.AppendLine("! Replaced astronaut's missing " + subtypeName + ".");
                                saveAfterScan = true;
                                inventory.Items.Add(new MyObjectBuilder_InventoryItem
                                {
                                    Amount = 1,
                                    Content = new MyObjectBuilder_PhysicalGunObject { SubtypeName = subtypeName },
                                    ItemId = inventory.nextItemId
                                });
                                inventory.nextItemId++;
                            }
                        });
                }

                // Scan through all items.
                foreach (var entity in model.SectorData.SectorObjects)
                {
                    if (entity is MyObjectBuilder_CubeGrid)
                    {
                        var cubeGrid = (MyObjectBuilder_CubeGrid)entity;

                        var list = cubeGrid.CubeBlocks.Where(c => c is MyObjectBuilder_Ladder).ToArray();

                        for (var i = 0; i < list.Length; i++)
                        {
                            var c = new MyObjectBuilder_Passage
                            {
                                EntityId = list[i].EntityId,
                                BlockOrientation = list[i].BlockOrientation,
                                BuildPercent = list[i].BuildPercent,
                                ColorMaskHSV = list[i].ColorMaskHSV,
                                IntegrityPercent = list[i].IntegrityPercent,
                                Min = list[i].Min,
                                SubtypeName = list[i].SubtypeName
                            };
                            cubeGrid.CubeBlocks.Remove(list[i]);
                            cubeGrid.CubeBlocks.Add(c);
                        }

                        if (list.Length > 0)
                        {
                            // MyObjectBuilder_Ladder/MyObjectBuilder_Passage convert.
                            statusNormal = false;
                            str.AppendLine("! 'Ladder' no longer supported in game.");
                            str.AppendLine(string.Format("! {0} 'Ladder' converted to 'Passage'.", list.Length));
                            saveAfterScan = true;
                        }

                        list = cubeGrid.CubeBlocks.OfType<MyObjectBuilder_Cockpit>().ToArray();
                        for (var i = 0; i < list.Length; i++)
                        {
                            if (((MyObjectBuilder_Cockpit)list[i]).Pilot != null)
                            {
                                var character = ((MyObjectBuilder_Cockpit)list[i]).Pilot;

                                if (!SpaceEngineersCore.Definitions.Characters.Any(c => c.Name == character.CharacterModel))
                                {
                                    character.CharacterModel = SpaceEngineersCore.Definitions.Characters[0].Name;
                                    statusNormal = false;
                                    str.AppendLine("! Fixed astronaut's CharacterModel.");
                                    saveAfterScan = true;
                                }
                            }
                        }

                        // TODO: search for cubeblocks that don't exist in the definitions.
                        //var definition = SpaceEngineersAPI.GetCubeDefinition(block.GetType(), CubeGrid.GridSizeEnum, block.SubtypeName);
                    }

                    if (entity is MyObjectBuilder_Character)
                    {
                        var character = (MyObjectBuilder_Character)entity;
                        if (!SpaceEngineersCore.Definitions.Characters.Any(c => c.Name == character.CharacterModel))
                        {
                            character.CharacterModel = SpaceEngineersCore.Definitions.Characters[0].Name;
                            statusNormal = false;
                            str.AppendLine("! Fixed astronaut's CharacterModel.");
                            saveAfterScan = true;
                        }
                    }
                }

                foreach (var item in SelectedWorld.Content.Players.Dictionary)
                {
                    if (!SpaceEngineersCore.Definitions.Characters.Any(c => c.Name == item.Value.PlayerModel))
                    {
                        item.Value.PlayerModel = SpaceEngineersCore.Definitions.Characters[0].Name;
                        statusNormal = false;
                        str.AppendLine("! Fixed astronaut's CharacterModel.");
                        saveAfterScan = true;
                    }

                    if (item.Value.PlayerId == 0)
                    {
                        item.Value.PlayerId = SpaceEngineersApi.GenerateEntityId();
                        SelectedWorld.Content.AllPlayers.Add(new MyObjectBuilder_Checkpoint.PlayerItem(item.Value.PlayerId, "Repair", false, item.Value.SteamID, null));
                        statusNormal = false;
                        str.AppendLine("! Fixed corrupt or missing Player defitinion.");
                        saveAfterScan = true;
                    }
                }

                if (saveAfterScan)
                {
                    model.SaveCheckPointAndSandBox();
                    str.AppendLine("* Saved changes.");
                }
            }

            if (statusNormal)
            {
                str.AppendLine("Detected no issues.");
            }

            return str.ToString();
        }

        #endregion

        #region helpers

        private void LoadSaveList()
        {
            Worlds.Clear();
            var list = new List<SaveResource>();

            #region local saves

            if (Directory.Exists(BaseLocalPath.SavesPath))
            {
                var userPaths = Directory.GetDirectories(BaseLocalPath.SavesPath);

                foreach (var userPath in userPaths)
                {
                    var userName = Path.GetFileName(userPath);
                    list.AddRange(FindSaveFiles(userPath, userName, SaveWorldType.Local, BaseLocalPath));
                }
            }

            #endregion

            #region Host Server

            if (Directory.Exists(BaseDedicatedServerHostPath.SavesPath))
            {
                list.AddRange(FindSaveFiles(BaseDedicatedServerHostPath.SavesPath, "Local / Console", SaveWorldType.DedicatedServerHost, BaseDedicatedServerHostPath));
            }

            #endregion

            #region Service Server

            if (Directory.Exists(BaseDedicatedServerServicePath.SavesPath))
            {
                var instancePaths = Directory.GetDirectories(BaseDedicatedServerServicePath.SavesPath);

                foreach (var instancePath in instancePaths)
                {
                    var lastLoadedPath = Path.Combine(instancePath, "Saves");

                    if (Directory.Exists(lastLoadedPath))
                    {
                        var instanceName = Path.GetFileName(instancePath);
                        var dataPath = new UserDataPath(lastLoadedPath, Path.Combine(instancePath, "Mods"));
                        list.AddRange(FindSaveFiles(lastLoadedPath, instanceName, SaveWorldType.DedicatedServerService, dataPath));
                    }
                }
            }

            #endregion

            foreach (var item in list.OrderByDescending(w => w.LastLoadTime))
                Worlds.Add(item);
        }

        private IEnumerable<SaveResource> FindSaveFiles(string lastLoadedPath, string userName, SaveWorldType saveType, UserDataPath dataPath)
        {
            var lastLoadedFile = Path.Combine(lastLoadedPath, SpaceEngineersConsts.LoadLoadedFilename);
            var list = new List<SaveResource>();

            // Ignore any other base Save paths without the LastLoaded file.
            if (File.Exists(lastLoadedFile))
            {
                MyObjectBuilder_LastLoadedTimes lastLoaded = null;
                try
                {
                    lastLoaded = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_LastLoadedTimes>(lastLoadedFile);
                }
                catch { }
                var savePaths = Directory.GetDirectories(lastLoadedPath);

                // Still check every potential game world path.
                foreach (var savePath in savePaths)
                {
                    var saveResource = LoadSaveFromPath(savePath, userName, saveType, dataPath);
                    if (lastLoaded != null)
                    {
                        var last = lastLoaded.LastLoaded.Dictionary.FirstOrDefault(d => d.Key.Equals(savePath, StringComparison.OrdinalIgnoreCase));
                        if (last.Key != null)
                        {
                            saveResource.LastLoadTime = last.Value;
                        }
                    }

                    // This should still allow Games to be copied into the Save path manually.

                    saveResource.LoadCheckpoint();
                    list.Add(saveResource);
                }
            }

            return list;
        }

        internal SaveResource LoadSaveFromPath(string savePath, string userName, SaveWorldType saveType, UserDataPath dataPath)
        {
            var saveResource = new SaveResource
            {
                GroupDescription = string.Format("{0}: {1}", new EnumToResouceConverter().Convert(saveType, typeof(string), null, CultureInfo.CurrentUICulture), userName),
                SaveType = saveType,
                Savename = Path.GetFileName(savePath),
                UserName = userName,
                Savepath = savePath,
                DataPath = dataPath,
            };

            return saveResource;
        }

        internal static SaveResource FindSaveSession(string baseSavePath, string findSession)
        {
            if (Directory.Exists(baseSavePath))
            {
                var userPaths = Directory.GetDirectories(baseSavePath);

                foreach (var userPath in userPaths)
                {
                    var lastLoadedFile = Path.Combine(userPath, SpaceEngineersConsts.LoadLoadedFilename);

                    // Ignore any other base Save paths without the LastLoaded file.
                    if (File.Exists(lastLoadedFile))
                    {
                        var savePaths = Directory.GetDirectories(userPath);

                        // Still check every potential game world path.
                        foreach (var savePath in savePaths)
                        {
                            var saveResource = new SaveResource
                            {
                                Savename = Path.GetFileName(savePath),
                                UserName = Path.GetFileName(userPath),
                                Savepath = savePath
                            };

                            saveResource.LoadCheckpoint();

                            if (saveResource.Savename.ToUpper() == findSession || saveResource.SessionName.ToUpper() == findSession)
                            {
                                return saveResource;
                            }
                        }
                    }
                }
            }

            return null;
        }

        internal static SaveResource LoadSession(string savePath)
        {
            if (Directory.Exists(savePath))
            {
                var userPath = Path.GetDirectoryName(savePath);

                var saveResource = new SaveResource
                {
                    Savename = Path.GetFileName(savePath),
                    UserName = Path.GetFileName(userPath),
                    Savepath = savePath
                };

                saveResource.LoadCheckpoint();

                return saveResource;
            }

            return null;
        }

        #endregion
    }
}
