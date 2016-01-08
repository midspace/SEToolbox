﻿namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Shell;
    using System.Windows.Threading;

    using Microsoft.VisualBasic.FileIO;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using VRageMath;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;
    using VRage.ObjectBuilders;
    using VRage;

    public class ExplorerModel : BaseModel
    {
        #region Fields

        private bool _isActive;

        private bool _isBusy;

        private bool _isModified;

        private bool _isBaseSaveChanged;

        private StructureCharacterModel _thePlayerCharacter;

        ///// <summary>
        ///// Collection of <see cref="IStructureBase"/> objects that represent the builds currently configured.
        ///// </summary>
        private ObservableCollection<IStructureBase> _structures;

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
            Structures = new ObservableCollection<IStructureBase>();
            _timer = new Stopwatch();
            SetActiveStatus();
        }

        #endregion

        #region Properties

        public ObservableCollection<IStructureBase> Structures
        {
            get
            {
                return _structures;
            }

            set
            {
                if (value != _structures)
                {
                    _structures = value;
                    RaisePropertyChanged(() => Structures);
                }
            }
        }

        public StructureCharacterModel ThePlayerCharacter
        {
            get
            {
                return _thePlayerCharacter;
            }

            set
            {
                if (value != _thePlayerCharacter)
                {
                    _thePlayerCharacter = value;
                    RaisePropertyChanged(() => ThePlayerCharacter);
                }
            }
        }

        public WorldResource ActiveWorld
        {
            get { return SpaceEngineersCore.WorldResource; }

            set
            {
                if (value != SpaceEngineersCore.WorldResource)
                {
                    SpaceEngineersCore.WorldResource = value;
                    RaisePropertyChanged(() => ActiveWorld);
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
                return _isActive;
            }

            set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    RaisePropertyChanged(() => IsActive);
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
                    SetActiveStatus();
                    if (_isBusy)
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
                return _isModified;
            }

            set
            {
                if (value != _isModified)
                {
                    _isModified = value;
                    RaisePropertyChanged(() => IsModified);
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
                return _isBaseSaveChanged;
            }

            set
            {
                if (value != _isBaseSaveChanged)
                {
                    _isBaseSaveChanged = value;
                    RaisePropertyChanged(() => IsBaseSaveChanged);
                }
            }
        }

        public bool ShowProgress
        {
            get
            {
                return _showProgress;
            }

            set
            {
                if (value != _showProgress)
                {
                    _showProgress = value;
                    RaisePropertyChanged(() => ShowProgress);
                }
            }
        }

        public double Progress
        {
            get
            {
                return _progress;
            }

            set
            {
                if (value != _progress)
                {
                    _progress = value;
                    _progressValue = _progress / _maximumProgress;

                    if (!_timer.IsRunning || _timer.ElapsedMilliseconds > 200)
                    {
                        RaisePropertyChanged(() => Progress);
                        RaisePropertyChanged(() => ProgressValue);
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
                return _progressState;
            }

            set
            {
                if (value != _progressState)
                {
                    _progressState = value;
                    RaisePropertyChanged(() => ProgressState);
                }
            }
        }

        public double ProgressValue
        {
            get
            {
                return _progressValue;
            }

            set
            {
                if (value != _progressValue)
                {
                    _progressValue = value;
                    RaisePropertyChanged(() => ProgressValue);
                }
            }
        }


        public double MaximumProgress
        {
            get
            {
                return _maximumProgress;
            }

            set
            {
                if (value != _maximumProgress)
                {
                    _maximumProgress = value;
                    RaisePropertyChanged(() => MaximumProgress);
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
                    foreach (Vector3 hsv in ActiveWorld.Checkpoint.CharacterToolbar.ColorMaskHSVList)
                    {
                        var rgb = ((SerializableVector3)hsv).ToSandboxDrawingColor();
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
            IsActive = !IsBusy;
        }

        public void BeginLoad()
        {
            IsBusy = true;
        }

        public void EndLoad()
        {
            IsModified = false;
            IsBusy = false;
        }

        public void ParseSandBox()
        {
            // make sure the LoadSector is called on the right thread for binding of data.
            Dispatcher.CurrentDispatcher.Invoke(
                DispatcherPriority.DataBind,
                new Action(LoadSectorDetail));
        }

        public void SaveCheckPointAndSandBox()
        {
            IsBusy = true;
            ActiveWorld.SaveCheckPointAndSector(true);

            // Manages the adding of new voxel files.
            foreach (var entity in Structures)
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

                        if (Path.GetExtension(voxel.SourceVoxelFilepath).Equals(MyVoxelMap.V1FileExtension, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Convert between formats.
                            var voxelmap = new MyVoxelMap();
                            voxelmap.Load(voxel.SourceVoxelFilepath, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);
                            voxelmap.Save(voxel.VoxelFilepath);
                        }
                        else
                        {
                            File.Copy(voxel.SourceVoxelFilepath, voxel.VoxelFilepath);
                        }
                        voxel.SourceVoxelFilepath = null;
                    }
                }
            }

            // Manages the removal old voxels files.
            foreach (var file in SpaceEngineersCore.ManageDeleteVoxelList)
            {
                var filename = Path.Combine(ActiveWorld.Savepath, file);
                if (File.Exists(filename))
                {
                    FileSystem.DeleteFile(filename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            }
            SpaceEngineersCore.ManageDeleteVoxelList.Clear();

            IsModified = false;
            IsBusy = false;
        }

        public string SaveTemporarySandbox()
        {
            IsBusy = true;

            var tempFilename = TempfileUtil.NewFilename(".xml");
            SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Sector>(ActiveWorld.SectorData, tempFilename);

            IsBusy = false;
            return tempFilename;
        }

        /// <summary>
        /// Loads the content from the directory and SE objects, creating object models.
        /// </summary>
        private void LoadSectorDetail()
        {
            Structures.Clear();
            SpaceEngineersCore.ManageDeleteVoxelList.Clear();
            ThePlayerCharacter = null;
            _customColors = null;

            if (ActiveWorld.SectorData != null && ActiveWorld.Checkpoint != null)
            {
                foreach (var entityBase in ActiveWorld.SectorData.SectorObjects)
                {
                    var structure = StructureBaseModel.Create(entityBase, ActiveWorld.Savepath);

                    if (structure is StructureCharacterModel)
                    {
                        var character = structure as StructureCharacterModel;

                        if (ActiveWorld.Checkpoint != null && character.EntityId == ActiveWorld.Checkpoint.ControlledObject)
                        {
                            character.IsPlayer = true;
                            ThePlayerCharacter = character;
                        }
                    }
                    else if (structure is StructureCubeGridModel)
                    {
                        var cubeGrid = structure as StructureCubeGridModel;

                        var list = cubeGrid.GetActiveCockpits();
                        foreach (var cockpit in list)
                        {
                            cubeGrid.Pilots++;
                            var character = (StructureCharacterModel)StructureBaseModel.Create(cockpit.Pilot, null);
                            character.IsPilot = true;

                            if (ActiveWorld.Checkpoint != null && cockpit.EntityId == ActiveWorld.Checkpoint.ControlledObject)
                            {
                                ThePlayerCharacter = character;
                                ThePlayerCharacter.IsPlayer = true;
                            }

                            Structures.Add(character);
                        }
                    }

                    Structures.Add(structure);
                }

                CalcDistances();
            }

            RaisePropertyChanged(() => Structures);
        }

        public void CalcDistances()
        {
            if (ActiveWorld.SectorData != null)
            {
                var position = ThePlayerCharacter != null && ThePlayerCharacter.PositionAndOrientation.HasValue ? (Vector3D)ThePlayerCharacter.PositionAndOrientation.Value.Position : Vector3D.Zero;
                foreach (var structure in Structures)
                {
                    structure.RecalcPosition(position);
                }
            }
        }

        public void SaveEntity(MyObjectBuilder_EntityBase entity, string filename)
        {
            if (entity is MyObjectBuilder_CubeGrid)
            {
                SpaceEngineersApi.WriteSpaceEngineersFile((MyObjectBuilder_CubeGrid)entity, filename);
            }
            else if (entity is MyObjectBuilder_Character)
            {
                SpaceEngineersApi.WriteSpaceEngineersFile((MyObjectBuilder_Character)entity, filename);
            }
            else if (entity is MyObjectBuilder_FloatingObject)
            {
                SpaceEngineersApi.WriteSpaceEngineersFile((MyObjectBuilder_FloatingObject)entity, filename);
            }
            else if (entity is MyObjectBuilder_Meteor)
            {
                SpaceEngineersApi.WriteSpaceEngineersFile((MyObjectBuilder_Meteor)entity, filename);
            }
        }

        public List<string> LoadEntities(string[] filenames)
        {
            IsBusy = true;
            var idReplacementTable = new Dictionary<long, long>();
            var badfiles = new List<string>();

            foreach (var filename in filenames)
            {
                bool isCompressed;
                MyObjectBuilder_CubeGrid cubeEntity;
                MyObjectBuilder_FloatingObject floatingEntity;
                MyObjectBuilder_Meteor meteorEntity;
                MyObjectBuilder_Character characterEntity;
                MyObjectBuilder_Definitions prefabDefinitions;

                if (SpaceEngineersApi.TryReadSpaceEngineersFile(filename, out cubeEntity, out isCompressed))
                {
                    MergeData(cubeEntity, ref idReplacementTable);
                }
                else if (SpaceEngineersApi.TryReadSpaceEngineersFile(filename, out prefabDefinitions, out isCompressed))
                {
                    if (prefabDefinitions.Prefabs != null)
                    {
                        foreach (var prefab in prefabDefinitions.Prefabs)
                        {
                            if (prefab.CubeGrid != null)
                                MergeData(prefab.CubeGrid, ref idReplacementTable);
                            if (prefab.CubeGrids != null)
                            {
                                foreach (var cubeGrid in prefab.CubeGrids)
                                {
                                    MergeData(cubeGrid, ref idReplacementTable);
                                }
                            }
                        }
                    }
                }
                else if (SpaceEngineersApi.TryReadSpaceEngineersFile(filename, out floatingEntity, out isCompressed))
                {
                    var newEntity = AddEntity(floatingEntity);
                    newEntity.EntityId = MergeId(floatingEntity.EntityId, ref idReplacementTable);
                }
                else if (SpaceEngineersApi.TryReadSpaceEngineersFile(filename, out meteorEntity, out isCompressed))
                {
                    var newEntity = AddEntity(meteorEntity);
                    newEntity.EntityId = MergeId(meteorEntity.EntityId, ref idReplacementTable);
                }
                else if (SpaceEngineersApi.TryReadSpaceEngineersFile(filename, out characterEntity, out isCompressed))
                {
                    var newEntity = AddEntity(characterEntity);
                    newEntity.EntityId = MergeId(characterEntity.EntityId, ref idReplacementTable);
                }
                else
                {
                    badfiles.Add(filename);
                }
            }

            IsBusy = false;
            return badfiles;
        }

        // TODO: Bounding box collision detection.
        public void CollisionCorrectEntity(MyObjectBuilder_EntityBase entity)
        {
            //var cubeGrid = entity as MyObjectBuilder_CubeGrid;
            //if (cubeGrid != null)
            //{
            //    BoundingBoxD bb = SpaceEngineersAPI.GetBoundingBox(cubeGrid);
            //    foreach (var sectorObject in SectorData.SectorObjects)
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
                ActiveWorld.SectorData.SectorObjects.Add(entity);
                var structure = StructureBaseModel.Create(entity, ActiveWorld.Savepath);
                var position = ThePlayerCharacter != null ? (Vector3D)ThePlayerCharacter.PositionAndOrientation.Value.Position : Vector3D.Zero;
                structure.PlayerDistance = (position - structure.PositionAndOrientation.Value.Position).Length();
                Structures.Add(structure);
                IsModified = true;
                return structure;
            }
            return null;
        }

        public bool RemoveEntity(MyObjectBuilder_EntityBase entity)
        {
            if (entity != null)
            {
                if (ActiveWorld.SectorData.SectorObjects.Contains(entity))
                {
                    if (entity is MyObjectBuilder_VoxelMap)
                    {
                        SpaceEngineersCore.ManageDeleteVoxelList.Add(((MyObjectBuilder_VoxelMap)entity).StorageName + MyVoxelMap.V2FileExtension);
                    }

                    ActiveWorld.SectorData.SectorObjects.Remove(entity);
                    IsModified = true;
                    return true;
                }

                // TODO: write as linq;
                //var x = SectorData.SectorObjects.Where(s => s is MyObjectBuilder_CubeGrid).Cast<MyObjectBuilder_CubeGrid>().
                //    Where(s => s.CubeBlocks.Any(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Pilot == entity)).Select(e => e).ToArray();

                foreach (var sectorObject in ActiveWorld.SectorData.SectorObjects)
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
                                    var structure = Structures.FirstOrDefault(s => s.EntityBase == sectorObject) as StructureCubeGridModel;
                                    structure.Pilots--;
                                    return true;
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
            bool contains = Structures.Any(s => s is StructureVoxelModel && Path.GetFileNameWithoutExtension(((StructureVoxelModel)s).Name).ToUpper() == Path.GetFileNameWithoutExtension(filename).ToUpper()) || SpaceEngineersCore.ManageDeleteVoxelList.Any(f => Path.GetFileNameWithoutExtension(f).ToUpper() == Path.GetFileNameWithoutExtension(filename).ToUpper());

            if (contains || additionalList == null)
            {
                return contains;
            }

            contains |= additionalList.Any(s => s is MyObjectBuilder_VoxelMap && Path.GetFileNameWithoutExtension(((MyObjectBuilder_VoxelMap)s).StorageName).ToUpper() == Path.GetFileNameWithoutExtension(filename).ToUpper());

            return contains;
        }

        /// <summary>
        /// automatically number all voxel files, and check for duplicate filenames.
        /// </summary>
        /// <param name="originalFile"></param>
        /// <param name="additionalList"></param>
        /// <returns></returns>
        public string CreateUniqueVoxelStorageName(string originalFile, MyObjectBuilder_EntityBase[] additionalList)
        {
            var filepartname = Path.GetFileNameWithoutExtension(originalFile).ToLower();
            var extension = Path.GetExtension(originalFile).ToLower();
            var index = 0;

            if (!ContainsVoxelFilename(originalFile, additionalList))
                return originalFile;

            var filename = string.Format("{0}{1}{2}", filepartname, index, extension);

            while (ContainsVoxelFilename(filename, additionalList))
            {
                index++;
                filename = string.Format("{0}{1}{2}", filepartname, index, extension);
            }

            return filename;
        }

        public void MergeData(IList<IStructureBase> data)
        {
            var idReplacementTable = new Dictionary<long, long>();

            foreach (var item in data)
            {
                if (item is StructureCubeGridModel)
                {
                    var ship = item as StructureCubeGridModel;
                    MergeData(ship.CubeGrid, ref idReplacementTable);
                }
                else if (item is StructureVoxelModel)
                {
                    var asteroid = item as StructureVoxelModel;

                    if (ContainsVoxelFilename(asteroid.Name, null))
                    {
                        asteroid.Name = CreateUniqueVoxelStorageName(asteroid.Name, null);
                    }

                    var entity = (StructureVoxelModel)AddEntity(asteroid.VoxelMap);
                    entity.EntityId = MergeId(asteroid.EntityId, ref idReplacementTable);

                    if (asteroid.SourceVoxelFilepath != null)
                        entity.SourceVoxelFilepath = asteroid.SourceVoxelFilepath;  // Source Voxel file is temporary. Hasn't been saved yet.
                    else
                        entity.SourceVoxelFilepath = asteroid.VoxelFilepath;  // Source Voxel file exists.
                }
                else if (item is StructureFloatingObjectModel)
                {
                    var floatObject = item as StructureFloatingObjectModel;
                    var entity = AddEntity(floatObject.FloatingObject);
                    entity.EntityId = MergeId(floatObject.EntityId, ref idReplacementTable);
                }
                else if (item is StructureMeteorModel)
                {
                    var meteor = item as StructureMeteorModel;
                    var entity = AddEntity(meteor.Meteor);
                    entity.EntityId = MergeId(meteor.EntityId, ref idReplacementTable);
                }
                else if (item is StructureUnknownModel)
                {
                    var unknown = item as StructureUnknownModel;
                    var entity = AddEntity(unknown.EntityBase);
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
                //cubeGrid.MultiBlockId // ???

                var cockpit = cubeGrid as MyObjectBuilder_Cockpit;
                if (cockpit != null)
                {
                    cockpit.Pilot = null;  // remove any pilots.
                }

                var motorBase = cubeGrid as MyObjectBuilder_MotorBase;
                if (motorBase != null && motorBase.RotorEntityId.HasValue)
                {
                    // reattach motor/rotor to correct entity.
                    motorBase.RotorEntityId = MergeId(motorBase.RotorEntityId.Value, ref idReplacementTable);
                }

                var pistonBase = cubeGrid as MyObjectBuilder_PistonBase;
                if (pistonBase != null)
                {
                    // reattach PistonBase (and ExtendedPistonBase) to correct entity.
                    pistonBase.TopBlockId = MergeId(pistonBase.TopBlockId, ref idReplacementTable);
                }

                var shipConnector = cubeGrid as MyObjectBuilder_ShipConnector;
                if (shipConnector != null)
                {
                    // reattach ShipConnector to correct entity.
                    shipConnector.ConnectedEntityId = MergeId(shipConnector.ConnectedEntityId, ref idReplacementTable);
                }

                var buttonPanel = cubeGrid as MyObjectBuilder_ButtonPanel;
                if (buttonPanel != null)
                {
                    // reattach Button Panels to correct entity.
                    RenumberToolbar(buttonPanel.Toolbar, ref idReplacementTable);
                }

                var timerBlock = cubeGrid as MyObjectBuilder_TimerBlock;
                if (timerBlock != null)
                {
                    // reattach Timer actions to correct entity.
                    RenumberToolbar(timerBlock.Toolbar, ref idReplacementTable);
                }

                var sensorBlock = cubeGrid as MyObjectBuilder_SensorBlock;
                if (sensorBlock != null)
                {
                    // reattach Sensor actions to correct entity.
                    RenumberToolbar(sensorBlock.Toolbar, ref idReplacementTable);
                }

                var shipController = cubeGrid as MyObjectBuilder_ShipController;
                if (shipController != null)
                {
                    // reattach Ship Controller actions to correct entity.
                    RenumberToolbar(shipController.Toolbar, ref idReplacementTable);
                }
            }

            AddEntity(cubeGridObject);
        }

        private void RenumberToolbar(MyObjectBuilder_Toolbar toolbar, ref Dictionary<Int64, Int64> idReplacementTable)
        {
            if (toolbar == null)
                return;
            foreach (var item in toolbar.Slots)
            {
                var terminalGroup = item.Data as MyObjectBuilder_ToolbarItemTerminalGroup;
                if (terminalGroup != null)
                {
                    // GridEntityId does not require remapping. accoring to IL on ToolbarItemTerminalGroup.
                    //terminalGroup.GridEntityId = MergeId(terminalGroup.GridEntityId, ref idReplacementTable);
                    terminalGroup.BlockEntityId = MergeId(terminalGroup.BlockEntityId, ref idReplacementTable);
                }
                var terminalBlock = item.Data as MyObjectBuilder_ToolbarItemTerminalBlock;
                if (terminalBlock != null)
                {
                    terminalBlock.BlockEntityId = MergeId(terminalBlock.BlockEntityId, ref idReplacementTable);
                }
            }
        }

        private static Int64 MergeId(long currentId, ref Dictionary<Int64, Int64> idReplacementTable)
        {
            if (currentId == 0)
                return 0;

            if (idReplacementTable.ContainsKey(currentId))
                return idReplacementTable[currentId];

            idReplacementTable[currentId] = SpaceEngineersApi.GenerateEntityId(IDType.ENTITY);
            return idReplacementTable[currentId];
        }

        public void OptimizeModel(StructureCubeGridModel viewModel)
        {
            if (viewModel == null)
                return;

            // Optimise ordering of CubeBlocks within structure, so that loops can load quickly based on {X+, Y+, Z+}.
            var neworder = viewModel.CubeGrid.CubeBlocks.OrderBy(c => c.Min.Z).ThenBy(c => c.Min.Y).ThenBy(c => c.Min.X).ToList();
            viewModel.CubeGrid.CubeBlocks = neworder;
            IsModified = true;
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
            //IsModified = true;

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
                //            Orientation = Quaternion.CreateFromRotationMatrix(MatrixD.CreateLookAt(Vector3D.Zero, Vector3.Forward, Vector3.Up))
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
                //        Orientation = Quaternion.CreateFromRotationMatrix(MatrixD.CreateLookAt(Vector3D.Zero, Vector3.Forward, Vector3.Up))
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
                //            Position = new Vector3D(),
                //            //Position = new Vector3D(-7.5f, -10, 27.5f),
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
                var existingGroup = model1.CubeGrid.BlockGroups.FirstOrDefault(bg => bg.Name == group.Name);
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
            var pos1 = (Vector3D)model1.PositionAndOrientation.Value.Position;
            var pos2 = (Vector3D)model2.PositionAndOrientation.Value.Position;
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
                    var newcube = (MyObjectBuilder_CubeBlock)cube2.Clone();
                    newcube.Min = cube2.Min + offset;
                    model1.CubeGrid.CubeBlocks.Add(newcube);
                }

                // Merge Groupings in.
                foreach (var group in model2.CubeGrid.BlockGroups)
                {
                    var existingGroup = model1.CubeGrid.BlockGroups.FirstOrDefault(bg => bg.Name == group.Name);
                    if (existingGroup == null)
                    {
                        existingGroup = new MyObjectBuilder_BlockGroup { Name = group.Name };
                        model1.CubeGrid.BlockGroups.Add(existingGroup);
                    }

                    foreach (var block in group.Blocks)
                    {
                        existingGroup.Blocks.Add(block + offset);
                    }
                }

                // TODO: Merge Bones.
                //if (model2.CubeGrid.Skeleton != null)
                //{
                //    if (model1.CubeGrid.Skeleton == null)
                //        model1.CubeGrid.Skeleton = new List<BoneInfo>();

                //        for (var i = model2.CubeGrid.Skeleton.Count - 1; i >= 0; i--)
                //        //foreach (var bone in model2.CubeGrid.Skeleton)
                //        {
                //            var bone = model2.CubeGrid.Skeleton[i];
                //            model1.CubeGrid.Skeleton.Insert(0, new BoneInfo()
                //            {
                //                BonePosition = bone.BonePosition + offset,
                //                BoneOffset = bone.BoneOffset
                //            });
                //        }
                //}

                // TODO: Merge ConveyorLines
                // need to fix the rotation of ConveyorLines first.

                return true;
            }

            return false;
        }

        #endregion

        public void ResetProgress(double initial, double maximumProgress)
        {
            MaximumProgress = maximumProgress;
            Progress = initial;
            ShowProgress = true;
            ProgressState = TaskbarItemProgressState.Normal;
            _timer.Restart();
            System.Windows.Forms.Application.DoEvents();
        }

        public void IncrementProgress()
        {
            Progress++;
        }

        public void ClearProgress()
        {
            _timer.Stop();
            ShowProgress = false;
            Progress = 0;
            ProgressState = TaskbarItemProgressState.None;
            ProgressValue = 0;
        }
    }
}
