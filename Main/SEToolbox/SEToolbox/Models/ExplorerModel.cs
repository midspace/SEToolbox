namespace SEToolbox.Models
{
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using SEToolbox.Interop;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Threading;

    public class ExplorerModel : BaseModel
    {
        #region Fields

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        private string baseSavePath;

        private SaveResource activeWorld;

        private bool isActive;

        private bool isBusy;

        private bool isModified;

        private bool isBaseSaveChanged;

        private MyObjectBuilder_Sector sectorData;

        private StructureCharacterModel thePlayerCharacter;

        ///// <summary>
        ///// Collection of <see cref="IStructureBase"/> objects that represent the builds currently configured.
        ///// </summary>
        private ObservableCollection<IStructureBase> structures;

        /// <summary>
        /// List of new Voxel files to add to the 'world'. [localVoxelFile, SourceFilepathName].
        /// </summary>
        private Dictionary<string, string> manageNewVoxelList;

        private List<string> manageDeleteVoxelList;

        #endregion

        #region Constructors

        public ExplorerModel()
        {
            this.Structures = new ObservableCollection<IStructureBase>();
            this.manageNewVoxelList = new Dictionary<string, string>();
            this.manageDeleteVoxelList = new List<string>();
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

        public ObservableCollection<IStructureBase> Structures
        {
            get
            {
                return this.structures;
            }

            set
            {
                if (value != this.structures)
                {
                    this.structures = value;
                    this.RaisePropertyChanged(() => Structures);
                }
            }
        }

        public StructureCharacterModel ThePlayerCharacter
        {
            get
            {
                return this.thePlayerCharacter;
            }

            set
            {
                if (value != this.thePlayerCharacter)
                {
                    this.thePlayerCharacter = value;
                    this.RaisePropertyChanged(() => ThePlayerCharacter);
                }
            }
        }

        public SaveResource ActiveWorld
        {
            get
            {
                return this.activeWorld;
            }

            set
            {
                if (value != this.activeWorld)
                {
                    this.activeWorld = value;
                    this.RaisePropertyChanged(() => ActiveWorld);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is available.  This is based on the IsInError and IsBusy properties
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this.isActive;
            }

            set
            {
                if (value != this.isActive)
                {
                    this.isActive = value;
                    this.RaisePropertyChanged(() => IsActive);
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
                    this.SetActiveStatus();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View content has been changed.
        /// </summary>
        public bool IsModified
        {
            get
            {
                return this.isModified;
            }

            set
            {
                if (value != this.isModified)
                {
                    this.isModified = value;
                    this.RaisePropertyChanged(() => IsModified);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the base SE save content has changed.
        /// </summary>
        public bool IsBaseSaveChanged
        {
            get
            {
                return this.isBaseSaveChanged;
            }

            set
            {
                if (value != this.isBaseSaveChanged)
                {
                    this.isBaseSaveChanged = value;
                    this.RaisePropertyChanged(() => IsBaseSaveChanged);
                }
            }
        }

        public MyObjectBuilder_Sector SectorData
        {
            get
            {
                return this.sectorData;
            }

            set
            {
                if (value != this.sectorData)
                {
                    this.sectorData = value;
                    this.RaisePropertyChanged(() => SectorData);
                }
            }
        }

        #endregion

        #region Methods

        public void SetActiveStatus()
        {
            this.IsActive = !this.IsBusy;
        }

        public void Load()
        {
            this.BaseSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SpaceEngineers\Saves");
            SpaceEngineersAPI.ReadCubeBlockDefinitions();
            this.SetActiveStatus();
        }

        public void LoadSandBox()
        {
            this.IsBusy = true;

            Dispatcher.CurrentDispatcher.Invoke(
                DispatcherPriority.DataBind,
                new Action(delegate
                {
                    if (this.ActiveWorld == null)
                    {
                        this.SectorData = null;
                    }
                    else
                    {
                        var filename = Path.Combine(this.ActiveWorld.Savepath, SpaceEngineersConsts.SandBoxSectorFilename);
                        this.SectorData = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(filename);
                    }
                    this.LoadSectorDetail();
                    this.IsModified = false;
                    this.IsBusy = false;
                }));

            if (this.ThePlayerCharacter == null)
            {
                // TODO: warn user there is no active "Player" character.
                // Show a dialog, and ask them if they want one created.
            }
        }

        public void SaveSandBox()
        {
            this.IsBusy = true;
            this.ActiveWorld.LastSaveTime = DateTime.Now;

            var checkpointFilename = Path.Combine(this.ActiveWorld.Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);
            SpaceEngineersAPI.WriteSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(this.ActiveWorld.Content, checkpointFilename);

            var sectorFilename = Path.Combine(this.ActiveWorld.Savepath, SpaceEngineersConsts.SandBoxSectorFilename);
            SpaceEngineersAPI.WriteSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(this.SectorData, sectorFilename);

            // manages the adding of new voxel files. [localVoxelFile, SourceFilepathName].
            foreach (KeyValuePair<string, string> kvp in this.manageNewVoxelList)
            {
                var filename = Path.Combine(this.ActiveWorld.Savepath, kvp.Key);
                File.Copy(kvp.Value, filename);
            }
            this.manageNewVoxelList.Clear();

            // Manages the removal old voxels files.
            foreach (var file in this.manageDeleteVoxelList)
            {
                var filename = Path.Combine(this.ActiveWorld.Savepath, file);
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            this.manageDeleteVoxelList.Clear();

            this.IsModified = false;
            this.IsBusy = false;
        }

        /// <summary>
        /// Loads the content from the directory and SE objects, creating object models.
        /// </summary>
        private void LoadSectorDetail()
        {
            this.Structures.Clear();
            this.manageNewVoxelList.Clear();
            this.manageDeleteVoxelList.Clear();
            this.ThePlayerCharacter = null;

            if (this.SectorData != null)
            {
                foreach (var entityBase in this.SectorData.SectorObjects)
                {
                    var structure = StructureBaseModel.Create(entityBase);

                    if (structure is StructureCharacterModel)
                    {
                        var character = structure as StructureCharacterModel;

                        if (character.EntityId == this.ActiveWorld.Content.ControlledObject)
                        {
                            character.IsPlayer = true;
                            this.ThePlayerCharacter = character;
                        }
                    }
                    else if (structure is StructureCubeGridModel)
                    {
                        var cubeGrid = structure as StructureCubeGridModel;

                        var list = cubeGrid.GetActiveCockpits();
                        foreach (var cockpit in list)
                        {
                            cubeGrid.Pilots++;
                            var character = StructureBaseModel.Create(cockpit.Pilot);

                            if (cockpit.EntityId == this.ActiveWorld.Content.ControlledObject)
                            {
                                this.ThePlayerCharacter = character as StructureCharacterModel;
                                this.ThePlayerCharacter.IsPlayer = true;
                            }

                            this.Structures.Add(character);
                        }
                    }

                    this.Structures.Add(structure);
                }

                if (this.ThePlayerCharacter != null)
                {
                    foreach (var structure in this.Structures)
                    {
                        var dist = this.ThePlayerCharacter.PositionAndOrientation.Value.Position - structure.PositionAndOrientation.Value.Position;
                        structure.PlayerDistance = dist.Length();
                    }
                }
            }

            this.RaisePropertyChanged(() => Structures);
        }

        public IStructureBase AddEntity(MyObjectBuilder_EntityBase entity)
        {
            if (entity != null)
            {
                this.SectorData.SectorObjects.Add(entity);
                var structure = StructureBaseModel.Create(entity);
                this.Structures.Add(structure);
                this.IsModified = true;
                return structure;
            }
            return null;
        }

        public bool RemoveEntity(MyObjectBuilder_EntityBase entity)
        {
            if (entity != null)
            {
                if (this.SectorData.SectorObjects.Contains(entity))
                {
                    if (entity is MyObjectBuilder_VoxelMap)
                    {
                        manageDeleteVoxelList.Add(((MyObjectBuilder_VoxelMap)entity).Filename);
                    }

                    this.SectorData.SectorObjects.Remove(entity);
                    this.IsModified = true;
                    return true;
                }
                else
                {
                    // TODO: write as linq;
                    //var x = this.SectorData.SectorObjects.Where(s => s is MyObjectBuilder_CubeGrid).Cast<MyObjectBuilder_CubeGrid>().
                    //    Where(s => s.CubeBlocks.Any(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Pilot == entity)).Select(e => e).ToArray();

                    foreach (var sectorObject in this.SectorData.SectorObjects)
                    {
                        if (sectorObject is MyObjectBuilder_CubeGrid)
                        {
                            foreach (var cubeGrid in ((MyObjectBuilder_CubeGrid)sectorObject).CubeBlocks)
                            {
                                if (cubeGrid is MyObjectBuilder_Cockpit)
                                {
                                    var cockpit = cubeGrid as MyObjectBuilder_Cockpit;
                                    if (cockpit.Pilot == entity)
                                    {
                                        cockpit.Pilot = null;
                                        var structure = this.Structures.FirstOrDefault(s => s.EntityBase == sectorObject) as StructureCubeGridModel;
                                        structure.Pilots--;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void AddVoxelFile(string voxelFilename, string sourceFilename)
        {
            this.manageNewVoxelList.Add(voxelFilename, sourceFilename);
        }
        public bool ContainsVoxelFilename(string filename)
        {
            return this.Structures.Any(s => s is StructureVoxelModel && ((StructureVoxelModel)s).Filename.ToUpper() == filename.ToUpper()) || this.manageDeleteVoxelList.Any(f => f.ToUpper() == filename.ToUpper());
        }


        #endregion
    }
}
