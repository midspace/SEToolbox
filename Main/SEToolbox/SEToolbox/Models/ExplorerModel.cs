namespace SEToolbox.Models
{
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using Sandbox.CommonLib.ObjectBuilders.VRageData;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Threading;
    using VRageMath;
    using Microsoft.VisualBasic.FileIO;

    public class ExplorerModel : BaseModel
    {
        #region Fields

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        private string _baseSavePath;

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

        private readonly List<string> manageDeleteVoxelList;

        private bool _compressedSectorFormat;

        #endregion

        #region Constructors

        public ExplorerModel()
        {
            this.Structures = new ObservableCollection<IStructureBase>();
            this.manageDeleteVoxelList = new List<string>();
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

        #endregion

        #region Methods

        public void SetActiveStatus()
        {
            this.IsActive = !this.IsBusy;
        }

        public void Load()
        {
            this.BaseSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SpaceEngineers\Saves");
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

                        if (ZipTools.IsGzipedFile(filename))
                        {
                            // New file format is compressed.
                            // These steps could probably be combined, but would have to use a MemoryStream, which has memory limits before it causes performance issues when chunking memory.
                            // Using a temporary file in this situation has less performance issues as it's moved straight to disk.
                            var tempFilename = TempfileUtil.NewFilename();
                            ZipTools.GZipUncompress(filename, tempFilename);
                            this.SectorData = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(tempFilename);
                            _compressedSectorFormat = true;
                        }
                        else
                        {
                            // Old file format is raw XML.
                            this.SectorData = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(filename);
                            _compressedSectorFormat = false;
                        }
                    }
                    this.LoadSectorDetail();
                    this.IsModified = false;
                    this.IsBusy = false;
                }));
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
                File.Delete(checkpointBackupFilename);
                //FileSystem.DeleteFile(checkpointBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }

            File.Move(checkpointFilename, checkpointBackupFilename);

            if (this.ActiveWorld.CompressedCheckpointFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                SpaceEngineersAPI.WriteSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(this.ActiveWorld.Content, tempFilename);
                ZipTools.GZipCompress(tempFilename, checkpointFilename);
            }
            else
            {
                SpaceEngineersAPI.WriteSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(this.ActiveWorld.Content, checkpointFilename);
            }

            if (File.Exists(sectorBackupFilename))
            {
                File.Delete(sectorBackupFilename);
                //FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }

            File.Move(sectorFilename, sectorBackupFilename);

            if (_compressedSectorFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                SpaceEngineersAPI.WriteSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(this.SectorData, tempFilename);
                ZipTools.GZipCompress(tempFilename, sectorFilename);
            }
            else
                SpaceEngineersAPI.WriteSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(this.SectorData, sectorFilename);

            // Manages the adding of new voxel files.
            foreach (var entity in this.Structures)
            {
                if (entity is StructureVoxelModel)
                {
                    var voxel = (StructureVoxelModel)entity;
                    if (voxel.SourceVoxelFilepath != null && File.Exists(voxel.SourceVoxelFilepath))
                    {
                        File.Copy(voxel.SourceVoxelFilepath, voxel.VoxelFilepath);
                        voxel.SourceVoxelFilepath = null;
                    }
                }
            }

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

        public string SaveTemporarySandbox()
        {
            this.IsBusy = true;

            var tempFilename = TempfileUtil.NewFilename() + ".xml";
            SpaceEngineersAPI.WriteSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(this.SectorData, tempFilename);

            this.IsBusy = false;
            return tempFilename;
        }

        /// <summary>
        /// Loads the content from the directory and SE objects, creating object models.
        /// </summary>
        private void LoadSectorDetail()
        {
            this.Structures.Clear();
            this.manageDeleteVoxelList.Clear();
            this.ThePlayerCharacter = null;

            if (this.SectorData != null)
            {
                foreach (var entityBase in this.SectorData.SectorObjects)
                {
                    var structure = StructureBaseModel.Create(entityBase, this.ActiveWorld.Savepath);

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
                            var character = StructureBaseModel.Create(cockpit.Pilot, null);

                            if (this.ActiveWorld.Content != null && cockpit.EntityId == this.ActiveWorld.Content.ControlledObject)
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
                        structure.PlayerDistance = (this.ThePlayerCharacter.PositionAndOrientation.Value.Position.ToVector3() - structure.PositionAndOrientation.Value.Position.ToVector3()).Length();
                    }
                }
            }

            this.RaisePropertyChanged(() => Structures);
        }

        public void SaveEntity(IStructureBase strucutre, string filename)
        {
            if (strucutre.EntityBase is MyObjectBuilder_CubeGrid)
            {
                SpaceEngineersAPI.WriteSpaceEngineersFile<MyObjectBuilder_CubeGrid, MyObjectBuilder_CubeGridSerializer>((MyObjectBuilder_CubeGrid)strucutre.EntityBase, filename);
            }
        }

        public List<string> LoadEntities(string[] filenames)
        {
            this.IsBusy = true;
            var idReplacementTable = new Dictionary<long, long>();
            var badfiles = new List<string>();

            foreach (var filename in filenames)
            {
                MyObjectBuilder_CubeGrid entity = null;

                try
                {
                    entity = SpaceEngineersAPI.ReadSpaceEngineersFile<MyObjectBuilder_CubeGrid, MyObjectBuilder_CubeGridSerializer>(filename);
                }
                catch
                {
                    badfiles.Add(filename);
                }

                this.MergeData(entity, ref idReplacementTable);
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
                var structure = StructureBaseModel.Create(entity, this.ActiveWorld.Savepath);
                structure.PlayerDistance = (this.ThePlayerCharacter.PositionAndOrientation.Value.Position.ToVector3() - structure.PositionAndOrientation.Value.Position.ToVector3()).Length();
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

        public bool ContainsVoxelFilename(string filename, MyObjectBuilder_EntityBase[] additionalList)
        {
            bool contains = this.Structures.Any(s => s is StructureVoxelModel && ((StructureVoxelModel)s).Filename.ToUpper() == filename.ToUpper()) || this.manageDeleteVoxelList.Any(f => f.ToUpper() == filename.ToUpper());

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

                    if (asteroid.SourceVoxelFilepath != null)
                        entity.SourceVoxelFilepath = asteroid.SourceVoxelFilepath;  // Source Voxel file is temporary. Hasn't been saved yet.
                    else
                        entity.SourceVoxelFilepath = asteroid.VoxelFilepath;  // Source Voxel file exists.
                }
                else if (item is StructureFloatingObjectModel)
                {
                    var floatObject = item as StructureFloatingObjectModel;
                    this.AddEntity(floatObject.FloatingObject);
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
                idReplacementTable[currentId] = SpaceEngineersAPI.GenerateEntityId();
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

        public void Test(StructureCubeGridModel viewModel)
        {
            //var corners = viewModel.CubeGrid.CubeBlocks.Where(b => b.SubtypeName.Contains("ArmorCorner")).ToList();
            var corners = viewModel.CubeGrid.CubeBlocks.OfType<MyObjectBuilder_CubeBlock>().ToArray();

            var list = new List<SerializableBlockOrientation>();
            var list2 = new List<string>();

            foreach (var corner in corners.Where(corner => !list.Contains(corner.BlockOrientation) && !SpaceEngineersAPI.ValidOrientations.Contains(corner.BlockOrientation)))
            {
                list.Add(corner.BlockOrientation);
            }

            var z = list.Count;
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

        #endregion
    }
}
