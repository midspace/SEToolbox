namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;

    public class SelectWorldModel : BaseModel
    {
        #region Fields

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        private string baseSavePath;

        private bool isValidSaveDirectory;

        private SaveResource selectedWorld;

        private ObservableCollection<SaveResource> worlds;

        private bool isBusy;

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
                return this.baseSavePath;
            }

            set
            {
                if (value != this.baseSavePath)
                {
                    this.baseSavePath = value;
                    this.RaisePropertyChanged(() => BaseSavePath);
                }
            }
        }

        public bool IsValidSaveDirectory
        {
            get
            {
                return this.isValidSaveDirectory;
            }

            set
            {
                if (value != this.isValidSaveDirectory)
                {
                    this.isValidSaveDirectory = value;
                    this.RaisePropertyChanged(() => IsValidSaveDirectory);
                }
            }
        }

        public SaveResource SelectedWorld
        {
            get
            {
                return this.selectedWorld;
            }

            set
            {
                if (value != this.selectedWorld)
                {
                    this.selectedWorld = value;
                    this.RaisePropertyChanged(() => SelectedWorld);
                }
            }
        }

        public ObservableCollection<SaveResource> Worlds
        {
            get
            {
                return this.worlds;
            }

            set
            {
                if (value != this.worlds)
                {
                    this.worlds = value;
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
                return this.isBusy;
            }

            set
            {
                if (value != this.isBusy)
                {
                    this.isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    if (this.isBusy)
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
            StringBuilder str = new StringBuilder();
            bool statusNormal = true;
            bool missingFiles = false;

            ExplorerModel model = new ExplorerModel();
            model.ActiveWorld = this.SelectedWorld;
            model.ActiveWorld.LoadCheckpoint();
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
                        model.ActiveWorld.Content.CameraController = Sandbox.CommonLib.ObjectBuilders.MyCameraControllerEnum.Entity;
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
                            model.ActiveWorld.Content.CameraController = Sandbox.CommonLib.ObjectBuilders.MyCameraControllerEnum.ThirdPersonSpectator;
                            model.ActiveWorld.Content.CameraEntity = 0;
                            str.AppendLine("* Found and Set new active Player.");
                            model.SaveCheckPointAndSandBox();
                            str.AppendLine("* Saved changes.");
                        }
                        else
                        {
                            str.AppendLine("! Could not find any Player Characters.");
                            character = new Sandbox.CommonLib.ObjectBuilders.MyObjectBuilder_Character();
                            character.EntityId = SpaceEngineersAPI.GenerateEntityId();
                            character.PersistentFlags = Sandbox.CommonLib.ObjectBuilders.MyPersistentEntityFlags2.CastShadows | Sandbox.CommonLib.ObjectBuilders.MyPersistentEntityFlags2.InScene;
                            character.PositionAndOrientation = new Sandbox.CommonLib.ObjectBuilders.MyPositionAndOrientation(new VRageMath.Vector3(0, 0, 0), new VRageMath.Vector3(0, 0, 1), new VRageMath.Vector3(0, 1, 0));
                            character.CharacterModel = Sandbox.CommonLib.ObjectBuilders.MyCharacterModelEnum.Astronaut_White;
                            character.Battery = new Sandbox.CommonLib.ObjectBuilders.MyObjectBuilder_Battery() { CurrentCapacity = 0.5f };
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
                            model.ActiveWorld.Content.CameraController = Sandbox.CommonLib.ObjectBuilders.MyCameraControllerEnum.Entity;
                            model.ActiveWorld.Content.CameraEntity = character.EntityId;

                            model.SectorData.SectorObjects.Add(character);

                            str.AppendLine("* Created new active Player.");
                            model.SaveCheckPointAndSandBox();
                            str.AppendLine("* Saved changes.");
                        }
                    }
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

                foreach (var userPath in userPaths)
                {
                    var savePaths = Directory.GetDirectories(userPath);

                    foreach (var savePath in savePaths)
                    {
                        SaveResource saveResource;
                        this.Worlds.Add(saveResource = new SaveResource()
                        {
                            Savename = Path.GetFileName(savePath),
                            Username = Path.GetFileName(userPath),
                            Savepath = savePath
                        });

                        saveResource.LoadCheckpoint();
                    }
                }

                this.IsValidSaveDirectory = true;
            }
            else
            {
                this.IsValidSaveDirectory = false;
            }
        }

        #endregion
    }
}
