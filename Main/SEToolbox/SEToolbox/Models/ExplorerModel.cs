namespace SEToolbox.Models
{
    using Microsoft.VisualBasic.FileIO;
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Shell;
    using System.Windows.Threading;
    using System.Xml;
    using VRageMath;

    public class ExplorerModel : BaseModel
    {
        #region Fields

        private SaveResource _activeWorld;

        private bool _isActive;

        private bool _isBusy;

        private bool _isModified;

        private bool _isBaseSaveChanged;

        private MyObjectBuilder_Sector _sectorData;

        private StructureCharacterModel _thePlayerCharacter;

        ///// <summary>
        ///// Collection of <see cref="IStructureBase"/> objects that represent the builds currently configured.
        ///// </summary>
        private ObservableCollection<IStructureBase> _structures;

        private readonly List<string> _manageDeleteVoxelList;

        private bool _compressedSectorFormat;

        private bool _showProgress;

        private double _progress;

        private TaskbarItemProgressState _progressState;

        private double _progressValue;

        private readonly Stopwatch _timer;

        private double _maximumProgress;

        private List<int> _customColors;

        #endregion

        #region Constructors

        public ExplorerModel()
        {
            this.Structures = new ObservableCollection<IStructureBase>();
            this._manageDeleteVoxelList = new List<string>();
            this._timer = new Stopwatch();
        }

        #endregion

        #region Properties

        public ObservableCollection<IStructureBase> Structures
        {
            get
            {
                return this._structures;
            }

            set
            {
                if (value != this._structures)
                {
                    this._structures = value;
                    this.RaisePropertyChanged(() => Structures);
                }
            }
        }

        public StructureCharacterModel ThePlayerCharacter
        {
            get
            {
                return this._thePlayerCharacter;
            }

            set
            {
                if (value != this._thePlayerCharacter)
                {
                    this._thePlayerCharacter = value;
                    this.RaisePropertyChanged(() => ThePlayerCharacter);
                }
            }
        }

        public SaveResource ActiveWorld
        {
            get
            {
                return this._activeWorld;
            }

            set
            {
                if (value != this._activeWorld)
                {
                    this._activeWorld = value;
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
                return this._isActive;
            }

            set
            {
                if (value != this._isActive)
                {
                    this._isActive = value;
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
                return this._isBusy;
            }

            set
            {
                if (value != this._isBusy)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    this.SetActiveStatus();
                    if (this._isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
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
                return this._isModified;
            }

            set
            {
                if (value != this._isModified)
                {
                    this._isModified = value;
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
                return this._isBaseSaveChanged;
            }

            set
            {
                if (value != this._isBaseSaveChanged)
                {
                    this._isBaseSaveChanged = value;
                    this.RaisePropertyChanged(() => IsBaseSaveChanged);
                }
            }
        }

        public MyObjectBuilder_Sector SectorData
        {
            get
            {
                return this._sectorData;
            }

            set
            {
                if (value != this._sectorData)
                {
                    this._sectorData = value;
                    this.RaisePropertyChanged(() => SectorData);
                }
            }
        }

        public bool ShowProgress
        {
            get
            {
                return this._showProgress;
            }

            set
            {
                if (value != this._showProgress)
                {
                    this._showProgress = value;
                    this.RaisePropertyChanged(() => ShowProgress);
                }
            }
        }

        public double Progress
        {
            get
            {
                return this._progress;
            }

            set
            {
                if (value != this._progress)
                {
                    this._progress = value;
                    this._progressValue = this._progress / this._maximumProgress;

                    if (!_timer.IsRunning || _timer.ElapsedMilliseconds > 200 )
                    {
                        this.RaisePropertyChanged(() => Progress);
                        this.RaisePropertyChanged(() => ProgressValue);
                        System.Windows.Forms.Application.DoEvents();
                        _timer.Restart();
                    }
                }
            }
        }

        public TaskbarItemProgressState ProgressState
        {
            get
            {
                return this._progressState;
            }

            set
            {
                if (value != this._progressState)
                {
                    this._progressState = value;
                    this.RaisePropertyChanged(() => ProgressState);
                }
            }
        }

        public double ProgressValue
        {
            get
            {
                return this._progressValue;
            }

            set
            {
                if (value != this._progressValue)
                {
                    this._progressValue = value;
                    this.RaisePropertyChanged(() => ProgressValue);
                }
            }
        }


        public double MaximumProgress
        {
            get
            {
                return this._maximumProgress;
            }

            set
            {
                if (value != this._maximumProgress)
                {
                    this._maximumProgress = value;
                    this.RaisePropertyChanged(() => MaximumProgress);
                }
            }
        }

        /// <summary>
        /// Read in current 'world' color Palette.
        /// { 8421504, 9342617, 4408198, 4474015, 4677703, 5339473, 8414016, 10056001, 5803425, 5808314, 11447986, 12105932, 3815995, 5329241 }
        /// </summary>
        public int[] CreativeModeColors
        {
            get
            {
                if (_customColors == null)
                {
                    _customColors = new List<int>();
                    foreach (VRageMath.Vector3 hsv in this.ActiveWorld.Content.CharacterToolbar.ColorMaskHSVList)
                    {
                        var rgb = ((Sandbox.Common.ObjectBuilders.VRageData.SerializableVector3)hsv).ToSandboxDrawingColor();
                        _customColors.Add(((rgb.B << 0x10) | (rgb.G << 8) | rgb.R) & 0xffffff);
                    }
                }
                return _customColors.ToArray();
            }

            set
            {
                _customColors = value.ToList();
                //foreach (int val in value)
                //{
                //    var r = (byte)(val & 0xffL);
                //    var g = (byte)((val >> 8) & 0xffL);
                //    var b = (byte)((val >> 0x10) & 0xffL);
                //    var c = Color.FromArgb(r, g, b);
                //    // Add to ColorMaskHSVList =>  c.ToSandboxHsvColor();
                //}
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
            this.SetActiveStatus();
        }

        public void LoadSandBox(bool snapshot = false)
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

                        if (ZipTools.IsGzipedFile(filename))
                        {
                            // New file format is compressed.
                            // These steps could probably be combined, but would have to use a MemoryStream, which has memory limits before it causes performance issues when chunking memory.
                            // Using a temporary file in this situation has less performance issues as it's moved straight to disk.
                            var tempFilename = TempfileUtil.NewFilename();
                            ZipTools.GZipUncompress(filename, tempFilename);
                            this.SectorData = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(tempFilename);
                            _compressedSectorFormat = true;
                        }
                        else
                        {
                            // Old file format is raw XML.

                            // Snapshot used for Report on Dedicated servers.
                            if (snapshot)
                            {
                                var tempFilename = TempfileUtil.NewFilename();
                                File.Copy(filename, tempFilename);
                                this.SectorData = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(tempFilename);
                            }
                            else
                            {
                                this.SectorData = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(filename);
                            }
                            _compressedSectorFormat = false;
                        }
                    }
                    this.LoadSectorDetail();
                    this.IsModified = false;
                    this.IsBusy = false;
                }));
        }

        public XmlDocument RepairerLoadSandBoxXml()
        {
            var xDoc = new XmlDocument();

            if (this.ActiveWorld == null)
            {
                xDoc = null;
            }
            else
            {
                var filename = Path.Combine(this.ActiveWorld.Savepath, SpaceEngineersConsts.SandBoxSectorFilename);

                if (ZipTools.IsGzipedFile(filename))
                {
                    // New file format is compressed.
                    // These steps could probably be combined, but would have to use a MemoryStream, which has memory limits before it causes performance issues when chunking memory.
                    // Using a temporary file in this situation has less performance issues as it's moved straight to disk.
                    var tempFilename = TempfileUtil.NewFilename();
                    ZipTools.GZipUncompress(filename, tempFilename);
                    xDoc.Load(tempFilename);
                    _compressedSectorFormat = true;
                }
                else
                {
                    // Old file format is raw XML.
                    this.SectorData = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(filename);
                    xDoc.Load(filename);
                    _compressedSectorFormat = false;
                }
            }

            return xDoc;
        }

        public void RepairerSaveSandBoxXml(XmlDocument xDoc)
        {
            var sectorFilename = Path.Combine(this.ActiveWorld.Savepath, SpaceEngineersConsts.SandBoxSectorFilename);
            var sectorBackupFilename = sectorFilename + ".bak";

            if (File.Exists(sectorBackupFilename))
            {
                FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }

            File.Move(sectorFilename, sectorBackupFilename);

            if (_compressedSectorFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                xDoc.Save(tempFilename);
                ZipTools.GZipCompress(tempFilename, sectorFilename);
            }
            else
                xDoc.Save(sectorFilename);
        }

        public void SaveCheckPointAndSandBox()
        {
            this.IsBusy = true;
            this.ActiveWorld.LastSaveTime = DateTime.Now;
            var checkpointFilename = Path.Combine(this.ActiveWorld.Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);
            var checkpointBackupFilename = checkpointFilename + ".bak";
            var sectorFilename = Path.Combine(this.ActiveWorld.Savepath, SpaceEngineersConsts.SandBoxSectorFilename);
            var sectorBackupFilename = sectorFilename + ".bak";

            if (File.Exists(checkpointBackupFilename))
            {
                FileSystem.DeleteFile(checkpointBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }

            File.Move(checkpointFilename, checkpointBackupFilename);

            this.ActiveWorld.Content.AppVersion = Sandbox.Common.MyFinalBuildConstants.APP_VERSION;
            this.SectorData.AppVersion = Sandbox.Common.MyFinalBuildConstants.APP_VERSION;

            if (this.ActiveWorld.CompressedCheckpointFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(this.ActiveWorld.Content, tempFilename);
                ZipTools.GZipCompress(tempFilename, checkpointFilename);
            }
            else
            {
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(this.ActiveWorld.Content, checkpointFilename);
            }

            if (File.Exists(sectorBackupFilename))
            {
                FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }

            File.Move(sectorFilename, sectorBackupFilename);

            if (_compressedSectorFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(this.SectorData, tempFilename);
                ZipTools.GZipCompress(tempFilename, sectorFilename);
            }
            else
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(this.SectorData, sectorFilename);

            // Manages the adding of new voxel files.
            foreach (var entity in this.Structures)
            {
                if (entity is StructureVoxelModel)
                {
                    var voxel = (StructureVoxelModel)entity;
                    if (voxel.SourceVoxelFilepath != null && File.Exists(voxel.SourceVoxelFilepath))
                    {
                        // Any asteroid that already exists with same name, must be removed.
                        // TODO: must improve this later when multiple sectors are implemented and the asteroid filename is used in a different sector.
                        if (File.Exists(voxel.VoxelFilepath))
                        {
                            FileSystem.DeleteFile(voxel.VoxelFilepath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                        }

                        File.Copy(voxel.SourceVoxelFilepath, voxel.VoxelFilepath);
                        voxel.SourceVoxelFilepath = null;
                    }
                }
            }

            // Manages the removal old voxels files.
            foreach (var file in this._manageDeleteVoxelList)
            {
                var filename = Path.Combine(this.ActiveWorld.Savepath, file);
                if (File.Exists(filename))
                {
                    FileSystem.DeleteFile(filename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            }
            this._manageDeleteVoxelList.Clear();

            this.IsModified = false;
            this.IsBusy = false;
        }

        public string SaveTemporarySandbox()
        {
            this.IsBusy = true;

            var tempFilename = TempfileUtil.NewFilename() + ".xml";
            SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(this.SectorData, tempFilename);

            this.IsBusy = false;
            return tempFilename;
        }

        /// <summary>
        /// Loads the content from the directory and SE objects, creating object models.
        /// </summary>
        private void LoadSectorDetail()
        {
            this.Structures.Clear();
            this._manageDeleteVoxelList.Clear();
            this.ThePlayerCharacter = null;
            this._customColors = null;

            if (this.SectorData != null && this.ActiveWorld.Content != null)
            {
                foreach (var entityBase in this.SectorData.SectorObjects)
                {
                    var structure = StructureBaseModel.Create(entityBase, this.ActiveWorld.Savepath, this.ActiveWorld.Content.Settings);

                    if (structure is StructureCharacterModel)
                    {
                        var character = structure as StructureCharacterModel;

                        if (this.ActiveWorld.Content != null && character.EntityId == this.ActiveWorld.Content.ControlledObject)
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
                            var character = (StructureCharacterModel)StructureBaseModel.Create(cockpit.Pilot, null, this.ActiveWorld.Content.Settings);
                            character.IsPilot = true;

                            if (this.ActiveWorld.Content != null && cockpit.EntityId == this.ActiveWorld.Content.ControlledObject)
                            {
                                this.ThePlayerCharacter = character;
                                this.ThePlayerCharacter.IsPlayer = true;
                            }

                            this.Structures.Add(character);
                        }
                    }

                    this.Structures.Add(structure);
                }

                this.CalcDistances();
            }

            this.RaisePropertyChanged(() => Structures);
        }

        public void CalcDistances()
        {
            if (this.SectorData != null)
            {
                var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value.Position.ToVector3() : Vector3.Zero;
                foreach (var structure in this.Structures)
                {
                    structure.RecalcPosition(position);
                }
            }
        }

        public void SaveEntity(MyObjectBuilder_EntityBase entity, string filename)
        {
            if (entity is MyObjectBuilder_CubeGrid)
            {
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_CubeGrid, MyObjectBuilder_CubeGridSerializer>((MyObjectBuilder_CubeGrid)entity, filename);
            }
            else if (entity is MyObjectBuilder_Character)
            {
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Character, MyObjectBuilder_CharacterSerializer>((MyObjectBuilder_Character)entity, filename);
            }
            else if (entity is MyObjectBuilder_FloatingObject)
            {
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_FloatingObject, MyObjectBuilder_FloatingObjectSerializer>((MyObjectBuilder_FloatingObject)entity, filename);
            }
            else if (entity is MyObjectBuilder_Meteor)
            {
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Meteor, MyObjectBuilder_MeteorSerializer>((MyObjectBuilder_Meteor)entity, filename);
            }
        }

        public List<string> LoadEntities(string[] filenames)
        {
            this.IsBusy = true;
            var idReplacementTable = new Dictionary<long, long>();
            var badfiles = new List<string>();

            foreach (var filename in filenames)
            {
                bool isCompressed;
                MyObjectBuilder_CubeGrid cubeEntity = null;
                MyObjectBuilder_FloatingObject floatingEntity = null;
                MyObjectBuilder_Meteor meteorEntity = null;
                MyObjectBuilder_Character characterEntity = null;

                if (SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_CubeGrid>(filename, out cubeEntity, out isCompressed))
                {
                    this.MergeData(cubeEntity, ref idReplacementTable);
                }
                else if (SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_FloatingObject>(filename, out floatingEntity, out isCompressed))
                {
                    var newEntity = this.AddEntity(floatingEntity);
                    newEntity.EntityId = MergeId(floatingEntity.EntityId, ref idReplacementTable);
                }
                else if (SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Meteor>(filename, out meteorEntity, out isCompressed))
                {
                    var newEntity = this.AddEntity(meteorEntity);
                    newEntity.EntityId = MergeId(meteorEntity.EntityId, ref idReplacementTable);
                }
                else if (SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Character>(filename, out characterEntity, out isCompressed))
                {
                    var newEntity = this.AddEntity(characterEntity);
                    newEntity.EntityId = MergeId(characterEntity.EntityId, ref idReplacementTable);
                }
                else
                {
                    badfiles.Add(filename);
                }
            }

            this.IsBusy = false;
            return badfiles;
        }

        // TODO: Bounding box collision detection.
        public void CollisionCorrectEntity(MyObjectBuilder_EntityBase entity)
        {
            //var cubeGrid = entity as MyObjectBuilder_CubeGrid;
            //if (cubeGrid != null)
            //{
            //    BoundingBox bb = SpaceEngineersAPI.GetBoundingBox(cubeGrid);
            //    foreach (var sectorObject in this.SectorData.SectorObjects)
            //    {
            //        if (sectorObject is MyObjectBuilder_CubeGrid)
            //        {
            //            var checkbb = SpaceEngineersAPI.GetBoundingBox((MyObjectBuilder_CubeGrid)sectorObject);
            //        }
            //    }

            //    //bb.Intersect
            //}
        }

        public IStructureBase AddEntity(MyObjectBuilder_EntityBase entity)
        {
            if (entity != null)
            {
                this.SectorData.SectorObjects.Add(entity);
                var structure = StructureBaseModel.Create(entity, this.ActiveWorld.Savepath, this.ActiveWorld.Content.Settings);
                var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value.Position.ToVector3() : Vector3.Zero;
                structure.PlayerDistance = (position - structure.PositionAndOrientation.Value.Position.ToVector3()).Length();
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
                        _manageDeleteVoxelList.Add(((MyObjectBuilder_VoxelMap)entity).Filename);
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

        public bool ContainsVoxelFilename(string filename, MyObjectBuilder_EntityBase[] additionalList)
        {
            bool contains = this.Structures.Any(s => s is StructureVoxelModel && ((StructureVoxelModel)s).Filename.ToUpper() == filename.ToUpper()) || this._manageDeleteVoxelList.Any(f => f.ToUpper() == filename.ToUpper());

            if (contains || additionalList == null)
            {
                return contains;
            }

            contains |= additionalList.Any(s => s is MyObjectBuilder_VoxelMap && ((MyObjectBuilder_VoxelMap)s).Filename.ToUpper() == filename.ToUpper());

            return contains;
        }

        public MyObjectBuilder_Character FindAstronautCharacter()
        {
            if (this.SectorData != null)
            {
                foreach (var entityBase in this.SectorData.SectorObjects)
                {
                    if (entityBase is MyObjectBuilder_Character)
                    {
                        return (MyObjectBuilder_Character)entityBase;
                    }
                }
            }
            return null;
        }

        public MyObjectBuilder_Cockpit FindPilotCharacter()
        {
            if (this.SectorData != null)
            {
                foreach (var entityBase in this.SectorData.SectorObjects)
                {
                    if (entityBase is MyObjectBuilder_CubeGrid)
                    {
                        var cubes = ((MyObjectBuilder_CubeGrid)entityBase).CubeBlocks.Where<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Pilot != null).ToList();
                        if (cubes.Count > 0)
                        {
                            return (MyObjectBuilder_Cockpit)cubes[0];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// automatically number all voxel files, and check for duplicate filenames.
        /// </summary>
        /// <param name="originalFile"></param>
        /// <returns></returns>
        public string CreateUniqueVoxelFilename(string originalFile, MyObjectBuilder_EntityBase[] additionalList)
        {
            var filepartname = Path.GetFileNameWithoutExtension(originalFile).ToLower();
            var extension = Path.GetExtension(originalFile).ToLower();
            var index = 0;
            var filename = filepartname + index.ToString() + extension;

            while (this.ContainsVoxelFilename(filename, additionalList))
            {
                index++;
                filename = filepartname + index.ToString() + extension;
            }

            return filename;
        }

        public void MergeData(IList<IStructureBase> data)
        {
            var idReplacementTable = new Dictionary<long, long>();

            foreach (var item in (IList<IStructureBase>)data)
            {
                if (item is StructureCubeGridModel)
                {
                    var ship = item as StructureCubeGridModel;
                    this.MergeData(ship.CubeGrid, ref idReplacementTable);
                }
                else if (item is StructureVoxelModel)
                {
                    var asteroid = item as StructureVoxelModel;

                    if (this.ContainsVoxelFilename(asteroid.Filename, null))
                    {
                        asteroid.Filename = CreateUniqueVoxelFilename(asteroid.Filename, null);
                    }

                    var entity = (StructureVoxelModel)this.AddEntity(asteroid.VoxelMap);
                    entity.EntityId = MergeId(asteroid.EntityId, ref idReplacementTable);

                    if (asteroid.SourceVoxelFilepath != null)
                        entity.SourceVoxelFilepath = asteroid.SourceVoxelFilepath;  // Source Voxel file is temporary. Hasn't been saved yet.
                    else
                        entity.SourceVoxelFilepath = asteroid.VoxelFilepath;  // Source Voxel file exists.
                }
                else if (item is StructureFloatingObjectModel)
                {
                    var floatObject = item as StructureFloatingObjectModel;
                    var entity = this.AddEntity(floatObject.FloatingObject);
                    entity.EntityId = MergeId(floatObject.EntityId, ref idReplacementTable);
                }
                else if (item is StructureMeteorModel)
                {
                    var meteor = item as StructureMeteorModel;
                    var entity = this.AddEntity(meteor.Meteor);
                    entity.EntityId = MergeId(meteor.EntityId, ref idReplacementTable);
                }
                else if (item is StructureUnknownModel)
                {
                    var unknown = item as StructureUnknownModel;
                    var entity = this.AddEntity(unknown.EntityBase);
                    entity.EntityId = MergeId(unknown.EntityId, ref idReplacementTable);
                }

                // ignore the StructureCharacterModel.
            }
        }

        private void MergeData(MyObjectBuilder_CubeGrid cubeGridObject, ref Dictionary<Int64, Int64> idReplacementTable)
        {
            if (cubeGridObject == null)
                return;

            cubeGridObject.EntityId = MergeId(cubeGridObject.EntityId, ref idReplacementTable);

            foreach (var cubeGrid in cubeGridObject.CubeBlocks)
            {
                cubeGrid.EntityId = MergeId(cubeGrid.EntityId, ref idReplacementTable);

                if (cubeGrid is MyObjectBuilder_Cockpit)
                {
                    ((MyObjectBuilder_Cockpit)cubeGrid).Pilot = null;  // remove any pilots.
                }

                if (cubeGrid is MyObjectBuilder_MotorStator)
                {
                    // reattach motor/rotor to correct entity.
                    ((MyObjectBuilder_MotorStator)cubeGrid).RotorEntityId = MergeId(((MyObjectBuilder_MotorStator)cubeGrid).RotorEntityId, ref idReplacementTable);
                }
            }

            this.AddEntity(cubeGridObject);
        }

        private static Int64 MergeId(long currentId, ref Dictionary<Int64, Int64> idReplacementTable)
        {
            if (currentId == 0)
                return 0;
            else if (idReplacementTable.ContainsKey(currentId))
                return idReplacementTable[currentId];
            else
            {
                idReplacementTable[currentId] = SpaceEngineersApi.GenerateEntityId();
                return idReplacementTable[currentId];
            }
        }

        public void OptimizeModel(StructureCubeGridModel viewModel)
        {
            if (viewModel == null)
                return;

            // Optimise ordering of CubeBlocks within structure, so that loops can load quickly based on {X+, Y+, Z+}.
            var neworder = viewModel.CubeGrid.CubeBlocks.OrderBy(c => c.Min.Z).ThenBy(c => c.Min.Y).ThenBy(c => c.Min.X).ToList();
            viewModel.CubeGrid.CubeBlocks = neworder;
            this.IsModified = true;
        }

        public void TestDisplayRotation(StructureCubeGridModel viewModel)
        {
            //var corners = viewModel.CubeGrid.CubeBlocks.Where(b => b.SubtypeName.Contains("ArmorCorner")).ToList();
            //var corners = viewModel.CubeGrid.CubeBlocks.OfType<MyObjectBuilder_CubeBlock>().ToArray();
            var corners = viewModel.CubeGrid.CubeBlocks.Where(b => StructureCubeGridModel.TubeCurvedRotationBlocks.Contains(b.SubtypeName)).ToList();

            foreach (var corner in corners)
            {
                Debug.WriteLine("{0}\t = \tAxis24_{1}_{2}", corner.SubtypeName, corner.BlockOrientation.Forward, corner.BlockOrientation.Up);
            }
        }

        public void TestConvert(StructureCubeGridModel viewModel)
        {
            // Trim Horse image.
            //viewModel.CubeGrid.CubeBlocks.RemoveAll(b => b.SubtypeName.EndsWith("White"));

            //foreach (var block in viewModel.CubeGrid.CubeBlocks)
            //{
            //    if (block.SubtypeName == SubtypeId.SmallBlockArmorBlock.ToString())
            //    {
            //        block.SubtypeName = SubtypeId.SmallBlockArmorBlockRed.ToString();
            //    }
            //}
            //this.IsModified = true;

            //viewModel.CubeGrid.CubeBlocks.RemoveAll(b => b.SubtypeName == SubtypeId.SmallLight.ToString());

            //var newBlocks = new List<MyObjectBuilder_CubeBlock>();

            foreach (var block in viewModel.CubeGrid.CubeBlocks)
            {

                if (block.SubtypeName == SubtypeId.SmallBlockArmorBlock.ToString())
                {
                    //block.SubtypeName = SubtypeId.SmallBlockArmorBlockBlack.ToString();

                    //var light = block as MyObjectBuilder_ReflectorLight;
                    //light.Intensity = 5;
                    //light.Radius = 5;
                }
                //if (block.SubtypeName == SubtypeId.LargeBlockArmorBlockBlack.ToString())
                //{
                //    for (var i = 0; i < 3; i++)
                //    {
                //        var newBlock = new MyObjectBuilder_CubeBlock()
                //        {
                //            SubtypeName = block.SubtypeName, // SubtypeId.LargeBlockArmorBlockWhite.ToString(),
                //            EntityId = block.EntityId == 0 ? 0 : SpaceEngineersAPI.GenerateEntityId(),
                //            PersistentFlags = block.PersistentFlags,
                //            Min = new Vector3I(block.Min.X, block.Min.Y, block.Min.Z + 1 + i),
                //            Max = new Vector3I(block.Max.X, block.Max.Y, block.Max.Z + 1 + i),
                //            Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up))
                //        };

                //        newBlocks.Add(newBlock);
                //    }
                //}

                //if (block.SubtypeName == SubtypeId.LargeBlockArmorBlockWhite.ToString())
                //{
                //    var newBlock = new MyObjectBuilder_CubeBlock()
                //    {
                //        SubtypeName = block.SubtypeName, // SubtypeId.LargeBlockArmorBlockWhite.ToString(),
                //        EntityId = block.EntityId == 0 ? 0 : SpaceEngineersAPI.GenerateEntityId(),
                //        PersistentFlags = block.PersistentFlags,
                //        Min = new Vector3I(block.Min.X, block.Min.Y, block.Min.Z + 3),
                //        Max = new Vector3I(block.Max.X, block.Max.Y, block.Max.Z + 3),
                //        Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up))
                //    };

                //    newBlocks.Add(newBlock);
                //}

                //if (block.Min.Z == 3 && block.Min.X % 2 == 1 && block.Min.Y % 2 == 1)
                //{
                //    var newBlock = new MyObjectBuilder_InteriorLight()
                //    {
                //        SubtypeName = SubtypeId.SmallLight.ToString(),
                //        EntityId = SpaceEngineersAPI.GenerateEntityId(),
                //        PersistentFlags = MyPersistentEntityFlags2.Enabled | MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                //        Min = new Vector3I(block.Min.X, block.Min.Y, 1),
                //        Max = new Vector3I(block.Max.X, block.Max.Y, 1),
                //        Orientation = new Quaternion(1, 0, 0, 0),
                //        Radius = 3.6f,
                //        Falloff = 1.3f,
                //        Intensity = 1.5f,
                //        PositionAndOrientation = new MyPositionAndOrientation()
                //        {
                //            Position = new Vector3(),
                //            //Position = new Vector3(-7.5f, -10, 27.5f),
                //            Forward = new Vector3(0,-1,0),
                //            Up = new Vector3(1,0,0)
                //        }

                //    };

                //    newBlocks.Add(newBlock);
                //}
            }

            //viewModel.CubeGrid.CubeBlocks.AddRange(newBlocks);

            OptimizeModel(viewModel);
        }

        /// <summary>
        /// Copy blocks from ship2 into ship1.
        /// </summary>
        /// <param name="model1"></param>
        /// <param name="model2"></param>
        internal void RejoinBrokenShip(StructureCubeGridModel model1, StructureCubeGridModel model2)
        {
            // Copy blocks from ship2 into ship1.
            model1.CubeGrid.CubeBlocks.AddRange(model2.CubeGrid.CubeBlocks);

            // Merge Groupings
            foreach (var group in model2.CubeGrid.BlockGroups)
            {
                var existingGroup = model1.CubeGrid.BlockGroups.FirstOrDefault(bg => bg.Name == group.Name) as MyObjectBuilder_BlockGroup;
                if (existingGroup == null)
                {
                    model1.CubeGrid.BlockGroups.Add(group);
                }
                else
                {
                    existingGroup.Blocks.AddRange(group.Blocks);
                }
            }

            // Merge ConveyorLines
            model1.CubeGrid.ConveyorLines.AddRange(model2.CubeGrid.ConveyorLines);
        }

        /// <summary>
        /// Merges and copies blocks from ship2 into ship1.
        /// </summary>
        /// <param name="model1"></param>
        /// <param name="model2"></param>
        /// <returns></returns>
        internal bool MergeShipParts(StructureCubeGridModel model1, StructureCubeGridModel model2)
        {
            // find closest major axis for both parts.
            var q1 = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromDir(model1.PositionAndOrientation.Value.Forward.RoundToAxis(), model1.PositionAndOrientation.Value.Up.RoundToAxis()));
            var q2 = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromDir(model2.PositionAndOrientation.Value.Forward.RoundToAxis(), model2.PositionAndOrientation.Value.Up.RoundToAxis()));

            // Calculate the rotation between the two.
            var fixRotate = Quaternion.Inverse(q2) * q1;
            fixRotate.Normalize();

            // Rotate the orientation of model2 to (closely) match model1.
            // It's Inverse, as the ship is actually rotated inverse in response to rotation of the cubes.
            model2.RotateCubes(Quaternion.Inverse(fixRotate));

            // At this point ship2 has been reoriented around to closely match ship1.
            // The cubes in ship2 have be reoriended in reverse, so effectly there is no visual difference in ship2, except now all the cubes are aligned to the same X,Y,Z axis as ship1.

            // find two cubes, one from each ship that are closest to each other to use as the reference.
            var pos1 = model1.PositionAndOrientation.Value.Position.ToVector3();
            var pos2 = model2.PositionAndOrientation.Value.Position.ToVector3();
            var orient1 = model1.PositionAndOrientation.Value.ToQuaternion();
            var orient2 = model2.PositionAndOrientation.Value.ToQuaternion();
            var multi1 = model1.GridSize.ToLength();
            var multi2 = model2.GridSize.ToLength();

            var maxDistance = float.MaxValue;
            MyObjectBuilder_CubeBlock maxCube1 = null;
            MyObjectBuilder_CubeBlock maxCube2 = null;

            foreach (var cube1 in model1.CubeGrid.CubeBlocks)
            {
                var cPos1 = pos1 + Vector3.Transform(cube1.Min.ToVector3() * multi1, orient1);

                foreach (var cube2 in model2.CubeGrid.CubeBlocks)
                {
                    var cPos2 = pos2 + Vector3.Transform(cube2.Min.ToVector3() * multi2, orient2);

                    var d = Vector3.Distance(cPos1, cPos2);
                    if (maxDistance > d)
                    {
                        maxDistance = d;
                        maxCube1 = cube1;
                        maxCube2 = cube2;
                    }
                }
            }

            // Ignore ships that are too far away from one another.
            // A distance of 4 cubes to allow for large cubes, as we are only using the Min as position, not the entire size of a cube.
            if (maxDistance < (model1.GridSize.ToLength() * 5))
            {
                // calculate offset for merging of closest cubes.
                var cPos1 = pos1 + Vector3.Transform(maxCube1.Min.ToVector3() * multi1, orient1);
                var cPos2 = pos2 + Vector3.Transform(maxCube2.Min.ToVector3() * multi2, orient2);
                var adjustedPos = Vector3.Transform(cPos2 - pos1, VRageMath.Quaternion.Inverse(orient1)) / multi1;
                var offset = adjustedPos.RoundToVector3I() - maxCube2.Min.ToVector3I();

                // Merge cubes in.
                foreach (var cube2 in model2.CubeGrid.CubeBlocks)
                {
                    var newcube = cube2.Clone() as MyObjectBuilder_CubeBlock;
                    newcube.Min = cube2.Min + offset;
                    model1.CubeGrid.CubeBlocks.Add(newcube);
                }

                // Merge Groupings in.
                foreach (var group in model2.CubeGrid.BlockGroups)
                {
                    var existingGroup = model1.CubeGrid.BlockGroups.FirstOrDefault(bg => bg.Name == group.Name) as MyObjectBuilder_BlockGroup;
                    if (existingGroup == null)
                    {
                        existingGroup = new MyObjectBuilder_BlockGroup(){Name = group.Name};
                        model1.CubeGrid.BlockGroups.Add(existingGroup);
                    }

                    foreach (var block in group.Blocks)
                    {
                        existingGroup.Blocks.Add(block + offset);
                    }
                }

                // TODO: Merge ConveyorLines
                // need to fix the rotation of ConveyorLines first.

                return true;
            }

            return false;
        }

        #endregion

        public void ResetProgress(double initial, double maximumProgress)
        {
            this.MaximumProgress = maximumProgress;
            this.Progress = initial;
            this.ShowProgress = true;
            this.ProgressState = TaskbarItemProgressState.Normal;
            _timer.Restart();
            System.Windows.Forms.Application.DoEvents();
        }

        public void IncrementProgress()
        {
            this.Progress++;
        }

        public void ClearProgress()
        {
            _timer.Stop();
            this.ShowProgress = false;
            this.Progress = 0;
            this.ProgressState = TaskbarItemProgressState.None;
            this.ProgressValue = 0;
        }
    }
}
