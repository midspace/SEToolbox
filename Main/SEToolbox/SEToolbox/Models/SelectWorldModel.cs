namespace SEToolbox.Models
{
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.XPath;

    public class SelectWorldModel : BaseModel
    {
        #region Fields

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        private string _baseSavePath;

        private bool _isValidSaveDirectory;

        private SaveResource _selectedWorld;

        private ObservableCollection<SaveResource> _worlds;

        private bool _isBusy;

        #endregion

        #region ctor

        public SelectWorldModel()
        {
            this.SelectedWorld = null;
            this.Worlds = new ObservableCollection<SaveResource>();
        }

        #endregion

        #region Properties

        public string BaseSavePath
        {
            get
            {
                return this._baseSavePath;
            }

            set
            {
                if (value != this._baseSavePath)
                {
                    this._baseSavePath = value;
                    this.RaisePropertyChanged(() => BaseSavePath);
                }
            }
        }

        public bool IsValidSaveDirectory
        {
            get
            {
                return this._isValidSaveDirectory;
            }

            set
            {
                if (value != this._isValidSaveDirectory)
                {
                    this._isValidSaveDirectory = value;
                    this.RaisePropertyChanged(() => IsValidSaveDirectory);
                }
            }
        }

        public SaveResource SelectedWorld
        {
            get
            {
                return this._selectedWorld;
            }

            set
            {
                if (value != this._selectedWorld)
                {
                    this._selectedWorld = value;
                    this.RaisePropertyChanged(() => SelectedWorld);
                }
            }
        }

        public ObservableCollection<SaveResource> Worlds
        {
            get
            {
                return this._worlds;
            }

            set
            {
                if (value != this._worlds)
                {
                    this._worlds = value;
                    this.RaisePropertyChanged(() => Worlds);
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
                return this._isBusy;
            }

            set
            {
                if (value != this._isBusy)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    if (this._isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        #endregion

        #region methods

        public void Load(string baseSavePath)
        {
            this.BaseSavePath = baseSavePath;
            this.LoadSaveList();
        }

        public string RepairSandBox()
        {
            var str = new StringBuilder();
            var statusNormal = true;
            var missingFiles = false;
            var saveAfterScan = false;

            var model = new ExplorerModel
            {
                ActiveWorld = this.SelectedWorld
            };

            model.ActiveWorld.LoadCheckpoint();

            var xDoc = model.RepairerLoadSandBoxXml();
            if (xDoc == null)
            {
                str.AppendLine("! Checkpoint file is missing or broken.");
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

            model.LoadSandBox();

            if (model.ActiveWorld.Content == null)
            {
                statusNormal = false;
                str.AppendLine("! Checkpoint file is missing or broken.");
                missingFiles = true;
            }
            
            if (model.SectorData == null)
            {
                statusNormal = false;
                str.AppendLine("! Sector file is missing or broken.");
                missingFiles = true;
            }

            if (!missingFiles)
            {
                if (model.ThePlayerCharacter == null)
                {
                    statusNormal = false;
                    str.AppendLine("! No active Player in Save content.");

                    var character = model.FindAstronautCharacter();
                    if (character != null)
                    {
                        model.ActiveWorld.Content.ControlledObject = character.EntityId;
                        model.ActiveWorld.Content.CameraController = Sandbox.Common.ObjectBuilders.MyCameraControllerEnum.Entity;
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
                            model.ActiveWorld.Content.CameraController = Sandbox.Common.ObjectBuilders.MyCameraControllerEnum.ThirdPersonSpectator;
                            model.ActiveWorld.Content.CameraEntity = 0;
                            str.AppendLine("* Found and Set new active Player.");
                            model.SaveCheckPointAndSandBox();
                            str.AppendLine("* Saved changes.");
                        }
                        else
                        {
                            str.AppendLine("! Could not find any Player Characters.");
                            character = new Sandbox.Common.ObjectBuilders.MyObjectBuilder_Character();
                            character.EntityId = SpaceEngineersAPI.GenerateEntityId();
                            character.PersistentFlags = Sandbox.Common.ObjectBuilders.MyPersistentEntityFlags2.CastShadows | Sandbox.Common.ObjectBuilders.MyPersistentEntityFlags2.InScene;
                            character.PositionAndOrientation = new Sandbox.Common.ObjectBuilders.MyPositionAndOrientation(VRageMath.Vector3.Zero, VRageMath.Vector3.Forward, VRageMath.Vector3.Up);
                            character.CharacterModel = Sandbox.Common.ObjectBuilders.MyCharacterModelEnum.Astronaut_White;
                            character.Battery = new Sandbox.Common.ObjectBuilders.MyObjectBuilder_Battery() { CurrentCapacity = 0.5f };
                            character.LightEnabled = false;
                            character.HeadAngle = new VRageMath.Vector2();
                            character.LinearVelocity = new VRageMath.Vector3();
                            character.AutoenableJetpackDelay = -1;
                            character.JetpackEnabled = true;
                            character.Inventory = (MyObjectBuilder_Inventory)MyObjectBuilder_Base.CreateNewObject(MyObjectBuilderTypeEnum.Inventory);

                            // Add default items to Inventory.
                            MyObjectBuilder_InventoryItem item;

                            character.Inventory.Items.Add(item = (MyObjectBuilder_InventoryItem)MyObjectBuilder_Base.CreateNewObject(MyObjectBuilderTypeEnum.InventoryItem));
                            item.AmountDecimal = 1;
                            item.Content = new MyObjectBuilder_Welder() { EntityId = SpaceEngineersAPI.GenerateEntityId(), PersistentFlags = MyPersistentEntityFlags2.None };

                            character.Inventory.Items.Add(item = (MyObjectBuilder_InventoryItem)MyObjectBuilder_Base.CreateNewObject(MyObjectBuilderTypeEnum.InventoryItem));
                            item.AmountDecimal = 1;
                            item.Content = new MyObjectBuilder_AngleGrinder() { EntityId = SpaceEngineersAPI.GenerateEntityId(), PersistentFlags = MyPersistentEntityFlags2.None };

                            character.Inventory.Items.Add(item = (MyObjectBuilder_InventoryItem)MyObjectBuilder_Base.CreateNewObject(MyObjectBuilderTypeEnum.InventoryItem));
                            item.AmountDecimal = 1;
                            item.Content = new MyObjectBuilder_HandDrill() { EntityId = SpaceEngineersAPI.GenerateEntityId(), PersistentFlags = MyPersistentEntityFlags2.None };

                            character.Inventory.Items.Add(item = (MyObjectBuilder_InventoryItem)MyObjectBuilder_Base.CreateNewObject(MyObjectBuilderTypeEnum.InventoryItem));
                            item.AmountDecimal = 1;
                            item.Content = new MyObjectBuilder_AutomaticRifle() { EntityId = SpaceEngineersAPI.GenerateEntityId(), PersistentFlags = MyPersistentEntityFlags2.None, CurrentAmmo = 0 };

                            model.ActiveWorld.Content.ControlledObject = character.EntityId;
                            model.ActiveWorld.Content.CameraController = Sandbox.Common.ObjectBuilders.MyCameraControllerEnum.Entity;
                            model.ActiveWorld.Content.CameraEntity = character.EntityId;

                            model.SectorData.SectorObjects.Add(character);

                            str.AppendLine("* Created new active Player.");
                            model.SaveCheckPointAndSandBox();
                            str.AppendLine("* Saved changes.");
                        }
                    }
                }

                saveAfterScan = false;

                // Scan through all items.
                foreach (var entity in model.SectorData.SectorObjects)
                {
                    if (entity is MyObjectBuilder_CubeGrid)
                    {
                        var cubeGrid = (MyObjectBuilder_CubeGrid)entity;

                        var list = cubeGrid.CubeBlocks.Where(c => c is MyObjectBuilder_Ladder).ToArray();

                        for (var i = 0; i < list.Length; i++)
                        {
                            var c = new MyObjectBuilder_Passage()
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
                            str.AppendLine( "! 'Ladder' no longer supported in game.");
                            str.AppendLine(string.Format("! {0} 'Ladder' converted to 'Passage'.", list.Length));
                            saveAfterScan = true;
                        }

                        // TODO: search for cubeblocks that don't exist in the definitions.
                        //var definition = SpaceEngineersAPI.GetCubeDefinition(block.GetType(), this.CubeGrid.GridSizeEnum, block.SubtypeName);
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
            this.Worlds.Clear();

            if (Directory.Exists(this.BaseSavePath))
            {
                var userPaths = Directory.GetDirectories(this.BaseSavePath);
                var list = new List<SaveResource>();

                foreach (var userPath in userPaths)
                {
                    var lastLoadedFile = Path.Combine(userPath, SpaceEngineersConsts.LoadLoadedFilename);

                    // Ignore any other base Save paths without the LastLoaded file.
                    if (File.Exists(lastLoadedFile))
                    {
                        var lastLoaded = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_LastLoadedTimes, MyObjectBuilder_LastLoadedTimesSerializer>(lastLoadedFile);
                        var savePaths = Directory.GetDirectories(userPath);

                        // Still check every potential game world path.
                        foreach (var savePath in savePaths)
                        {
                            SaveResource saveResource;
                            list.Add(saveResource = new SaveResource()
                            {
                                Savename = Path.GetFileName(savePath),
                                Username = Path.GetFileName(userPath),
                                Savepath = savePath
                            });

                            var last = lastLoaded.LastLoaded.Dictionary.FirstOrDefault(d => d.Key.Equals(savePath, StringComparison.OrdinalIgnoreCase));
                            if (last.Key != null)
                            {
                                saveResource.LastLoadTime = last.Value;
                            }
                            
                            // This should still allow Games to be copied into the Save path manually.

                            saveResource.LoadCheckpoint();
                        }
                    }
                }

                this.Worlds = new ObservableCollection<SaveResource>(list.OrderByDescending(w => w.LastLoadTime));

                this.IsValidSaveDirectory = true;
            }
            else
            {
                this.IsValidSaveDirectory = false;
            }
        }

        internal static SaveResource FindSaveSession(string baseSavePath, string findSessionName)
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
                            var saveResource = new SaveResource()
                            {
                                Savename = Path.GetFileName(savePath),
                                Username = Path.GetFileName(userPath),
                                Savepath = savePath
                            };

                            saveResource.LoadCheckpoint();

                            if (saveResource.Savename.ToUpper() == findSessionName || saveResource.SessionName.ToUpper() == findSessionName)
                            {
                                return saveResource;
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
