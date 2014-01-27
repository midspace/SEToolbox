namespace SEToolbox.Models
{
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Threading;
    using VRageMath;

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

        private List<string> manageDeleteVoxelList;

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
        }

        public void SaveCheckPointAndSandBox()
        {
            this.IsBusy = true;
            this.ActiveWorld.LastSaveTime = DateTime.Now;

            var checkpointFilename = Path.Combine(this.ActiveWorld.Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);
            SpaceEngineersAPI.WriteSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(this.ActiveWorld.Content, checkpointFilename);

            var sectorFilename = Path.Combine(this.ActiveWorld.Savepath, SpaceEngineersConsts.SandBoxSectorFilename);
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
                        structure.PlayerDistance = (this.ThePlayerCharacter.PositionAndOrientation.Value.Position - structure.PositionAndOrientation.Value.Position).Length();
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
                structure.PlayerDistance = (this.ThePlayerCharacter.PositionAndOrientation.Value.Position - structure.PositionAndOrientation.Value.Position).Length();
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

            var list = new List<Quaternion>();
            var list2 = new List<string>();

            foreach (var corner in corners.Where(corner => !list.Contains(corner.Orientation) && !SpaceEngineersAPI.Orientations.Contains(corner.Orientation)))
            {
                list.Add(corner.Orientation);
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
                    block.SubtypeName = SubtypeId.SmallBlockArmorBlockBlack.ToString();

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

            //OptimizeModel(viewModel);
        }

        public void MirrorModel(StructureCubeGridModel viewModel, bool oddMirror)
        {
            if (viewModel == null)
                return;

            var xMirror = Mirror.None;
            var yMirror = Mirror.None;
            var zMirror = Mirror.None;
            var xAxis = 0;
            var yAxis = 0;
            var zAxis = 0;

            // Find mirror Axis.
            if (!viewModel.CubeGrid.XMirroxPlane.HasValue && !viewModel.CubeGrid.YMirroxPlane.HasValue && !viewModel.CubeGrid.ZMirroxPlane.HasValue)
            {
                // Find the largest contigious exterior surface to use as the mirror.
                var minX = viewModel.CubeGrid.CubeBlocks.Min(c => c.Min.X);
                var maxX = viewModel.CubeGrid.CubeBlocks.Max(c => c.Min.X);
                var minY = viewModel.CubeGrid.CubeBlocks.Min(c => c.Min.Y);
                var maxY = viewModel.CubeGrid.CubeBlocks.Max(c => c.Min.Y);
                var minZ = viewModel.CubeGrid.CubeBlocks.Min(c => c.Min.Z);
                var maxZ = viewModel.CubeGrid.CubeBlocks.Max(c => c.Min.Z);

                var countMinX = viewModel.CubeGrid.CubeBlocks.Count(c => c.Min.X == minX);
                var countMinY = viewModel.CubeGrid.CubeBlocks.Count(c => c.Min.Y == minY);
                var countMinZ = viewModel.CubeGrid.CubeBlocks.Count(c => c.Min.Z == minZ);
                var countMaxX = viewModel.CubeGrid.CubeBlocks.Count(c => c.Min.X == maxX);
                var countMaxY = viewModel.CubeGrid.CubeBlocks.Count(c => c.Min.Y == maxY);
                var countMaxZ = viewModel.CubeGrid.CubeBlocks.Count(c => c.Min.Z == maxZ);

                if (countMinX > countMinY && countMinX > countMinZ && countMinX > countMaxX && countMinX > countMaxY && countMinX > countMaxZ)
                {
                    xMirror = oddMirror ? Mirror.Odd : Mirror.EvenDown;
                    xAxis = minX;
                }
                else if (countMinY > countMinX && countMinY > countMinZ && countMinY > countMaxX && countMinY > countMaxY && countMinY > countMaxZ)
                {
                    yMirror = oddMirror ? Mirror.Odd : Mirror.EvenDown;
                    yAxis = minY;
                }
                else if (countMinZ > countMinX && countMinZ > countMinY && countMinZ > countMaxX && countMinZ > countMaxY && countMinZ > countMaxZ)
                {
                    zMirror = oddMirror ? Mirror.Odd : Mirror.EvenDown;
                    zAxis = minZ;
                }
                else if (countMaxX > countMinX && countMaxX > countMinY && countMaxX > countMinZ && countMaxX > countMaxY && countMaxX > countMaxZ)
                {
                    xMirror = oddMirror ? Mirror.Odd : Mirror.EvenUp;
                    xAxis = maxX;
                }
                else if (countMaxY > countMinX && countMaxY > countMinY && countMaxY > countMinZ && countMaxY > countMaxX && countMaxY > countMaxZ)
                {
                    yMirror = oddMirror ? Mirror.Odd : Mirror.EvenUp;
                    yAxis = maxY;
                }
                else if (countMaxZ > countMinX && countMaxZ > countMinY && countMaxZ > countMinZ && countMaxZ > countMaxX && countMaxZ > countMaxY)
                {
                    zMirror = oddMirror ? Mirror.Odd : Mirror.EvenUp;
                    zAxis = maxZ;
                }

                viewModel.CubeGrid.CubeBlocks.AddRange(MirrorCubes(viewModel, false, xMirror, xAxis, yMirror, yAxis, zMirror, zAxis));
            }
            else
            {
                // Use the built in Mirror plane defined in game.
                if (viewModel.CubeGrid.XMirroxPlane.HasValue)
                {
                    xMirror = viewModel.CubeGrid.XMirroxOdd ? Mirror.EvenDown : Mirror.Odd; // Meaning is back to front? Or is it my reasoning?
                    xAxis = viewModel.CubeGrid.XMirroxPlane.Value.X;
                    viewModel.CubeGrid.CubeBlocks.AddRange(MirrorCubes(viewModel, true, xMirror, xAxis, Mirror.None, 0, Mirror.None, 0));
                }
                if (viewModel.CubeGrid.YMirroxPlane.HasValue)
                {
                    yMirror = viewModel.CubeGrid.YMirroxOdd ? Mirror.EvenDown : Mirror.Odd;
                    yAxis = viewModel.CubeGrid.YMirroxPlane.Value.Y;
                    viewModel.CubeGrid.CubeBlocks.AddRange(MirrorCubes(viewModel, true, Mirror.None, 0, yMirror, yAxis, Mirror.None, 0));
                }
                if (viewModel.CubeGrid.ZMirroxPlane.HasValue)
                {
                    zMirror = viewModel.CubeGrid.ZMirroxOdd ? Mirror.EvenUp : Mirror.Odd;
                    zAxis = viewModel.CubeGrid.ZMirroxPlane.Value.Z;
                    viewModel.CubeGrid.CubeBlocks.AddRange(MirrorCubes(viewModel, true, Mirror.None, 0, Mirror.None, 0, zMirror, zAxis));
                }
            }

            // OptimizeModel(viewModel);
            viewModel.UpdateFromEntityBase();
            this.IsModified = true;
        }

        #region ValidMirrorBlocks

        private static readonly SubtypeId[] ValidMirrorBlocks = new SubtypeId[] {
            SubtypeId.LargeBlockArmorBlock,
            SubtypeId.LargeBlockArmorBlockRed,
            SubtypeId.LargeBlockArmorBlockYellow,
            SubtypeId.LargeBlockArmorBlockBlue,
            SubtypeId.LargeBlockArmorBlockGreen,
            SubtypeId.LargeBlockArmorBlockBlack,
            SubtypeId.LargeBlockArmorBlockWhite,
            SubtypeId.LargeBlockArmorSlope,
            SubtypeId.LargeBlockArmorSlopeRed,
            SubtypeId.LargeBlockArmorSlopeYellow,
            SubtypeId.LargeBlockArmorSlopeBlue,
            SubtypeId.LargeBlockArmorSlopeGreen,
            SubtypeId.LargeBlockArmorSlopeBlack,
            SubtypeId.LargeBlockArmorSlopeWhite,
            SubtypeId.LargeBlockArmorCorner,
            SubtypeId.LargeBlockArmorCornerRed,
            SubtypeId.LargeBlockArmorCornerYellow,
            SubtypeId.LargeBlockArmorCornerBlue,
            SubtypeId.LargeBlockArmorCornerGreen,
            SubtypeId.LargeBlockArmorCornerBlack,
            SubtypeId.LargeBlockArmorCornerWhite,
            SubtypeId.LargeBlockArmorCornerInv,
            SubtypeId.LargeBlockArmorCornerInvRed,
            SubtypeId.LargeBlockArmorCornerInvYellow,
            SubtypeId.LargeBlockArmorCornerInvBlue,
            SubtypeId.LargeBlockArmorCornerInvGreen,
            SubtypeId.LargeBlockArmorCornerInvBlack,
            SubtypeId.LargeBlockArmorCornerInvWhite,
            SubtypeId.LargeHeavyBlockArmorBlock,
            SubtypeId.LargeHeavyBlockArmorBlockRed,
            SubtypeId.LargeHeavyBlockArmorBlockYellow,
            SubtypeId.LargeHeavyBlockArmorBlockBlue,
            SubtypeId.LargeHeavyBlockArmorBlockGreen,
            SubtypeId.LargeHeavyBlockArmorBlockBlack,
            SubtypeId.LargeHeavyBlockArmorBlockWhite,
            SubtypeId.LargeHeavyBlockArmorSlope,
            SubtypeId.LargeHeavyBlockArmorSlopeRed,
            SubtypeId.LargeHeavyBlockArmorSlopeYellow,
            SubtypeId.LargeHeavyBlockArmorSlopeBlue,
            SubtypeId.LargeHeavyBlockArmorSlopeGreen,
            SubtypeId.LargeHeavyBlockArmorSlopeBlack,
            SubtypeId.LargeHeavyBlockArmorSlopeWhite,
            SubtypeId.LargeHeavyBlockArmorCorner,
            SubtypeId.LargeHeavyBlockArmorCornerRed,
            SubtypeId.LargeHeavyBlockArmorCornerYellow,
            SubtypeId.LargeHeavyBlockArmorCornerBlue,
            SubtypeId.LargeHeavyBlockArmorCornerGreen,
            SubtypeId.LargeHeavyBlockArmorCornerBlack,
            SubtypeId.LargeHeavyBlockArmorCornerWhite,
            SubtypeId.LargeHeavyBlockArmorCornerInv,
            SubtypeId.LargeHeavyBlockArmorCornerInvRed,
            SubtypeId.LargeHeavyBlockArmorCornerInvYellow,
            SubtypeId.LargeHeavyBlockArmorCornerInvBlue,
            SubtypeId.LargeHeavyBlockArmorCornerInvGreen,
            SubtypeId.LargeHeavyBlockArmorCornerInvBlack,
            SubtypeId.LargeHeavyBlockArmorCornerInvWhite,
            SubtypeId.SmallBlockArmorBlock,
            SubtypeId.SmallBlockArmorBlockRed,
            SubtypeId.SmallBlockArmorBlockYellow,
            SubtypeId.SmallBlockArmorBlockBlue,
            SubtypeId.SmallBlockArmorBlockGreen,
            SubtypeId.SmallBlockArmorBlockBlack,
            SubtypeId.SmallBlockArmorBlockWhite,
            SubtypeId.SmallBlockArmorSlope,
            SubtypeId.SmallBlockArmorSlopeRed,
            SubtypeId.SmallBlockArmorSlopeYellow,
            SubtypeId.SmallBlockArmorSlopeBlue,
            SubtypeId.SmallBlockArmorSlopeGreen,
            SubtypeId.SmallBlockArmorSlopeBlack,
            SubtypeId.SmallBlockArmorSlopeWhite,
            SubtypeId.SmallBlockArmorCorner,
            SubtypeId.SmallBlockArmorCornerRed,
            SubtypeId.SmallBlockArmorCornerYellow,
            SubtypeId.SmallBlockArmorCornerBlue,
            SubtypeId.SmallBlockArmorCornerGreen,
            SubtypeId.SmallBlockArmorCornerBlack,
            SubtypeId.SmallBlockArmorCornerWhite,
            SubtypeId.SmallBlockArmorCornerInv,
            SubtypeId.SmallBlockArmorCornerInvRed,
            SubtypeId.SmallBlockArmorCornerInvYellow,
            SubtypeId.SmallBlockArmorCornerInvBlue,
            SubtypeId.SmallBlockArmorCornerInvGreen,
            SubtypeId.SmallBlockArmorCornerInvBlack,
            SubtypeId.SmallBlockArmorCornerInvWhite,
            SubtypeId.SmallHeavyBlockArmorBlock,
            SubtypeId.SmallHeavyBlockArmorBlockRed,
            SubtypeId.SmallHeavyBlockArmorBlockYellow,
            SubtypeId.SmallHeavyBlockArmorBlockBlue,
            SubtypeId.SmallHeavyBlockArmorBlockGreen,
            SubtypeId.SmallHeavyBlockArmorBlockBlack,
            SubtypeId.SmallHeavyBlockArmorBlockWhite,
            SubtypeId.SmallHeavyBlockArmorSlope,
            SubtypeId.SmallHeavyBlockArmorSlopeRed,
            SubtypeId.SmallHeavyBlockArmorSlopeYellow,
            SubtypeId.SmallHeavyBlockArmorSlopeBlue,
            SubtypeId.SmallHeavyBlockArmorSlopeGreen,
            SubtypeId.SmallHeavyBlockArmorSlopeBlack,
            SubtypeId.SmallHeavyBlockArmorSlopeWhite,
            SubtypeId.SmallHeavyBlockArmorCorner,
            SubtypeId.SmallHeavyBlockArmorCornerRed,
            SubtypeId.SmallHeavyBlockArmorCornerYellow,
            SubtypeId.SmallHeavyBlockArmorCornerBlue,
            SubtypeId.SmallHeavyBlockArmorCornerGreen,
            SubtypeId.SmallHeavyBlockArmorCornerBlack,
            SubtypeId.SmallHeavyBlockArmorCornerWhite,
            SubtypeId.SmallHeavyBlockArmorCornerInv,
            SubtypeId.SmallHeavyBlockArmorCornerInvRed,
            SubtypeId.SmallHeavyBlockArmorCornerInvYellow,
            SubtypeId.SmallHeavyBlockArmorCornerInvBlue,
            SubtypeId.SmallHeavyBlockArmorCornerInvGreen,
            SubtypeId.SmallHeavyBlockArmorCornerInvBlack,
            SubtypeId.SmallHeavyBlockArmorCornerInvWhite,
            SubtypeId.LargeRamp,
        }; 

        #endregion

        private static IEnumerable<MyObjectBuilder_CubeBlock> MirrorCubes(StructureCubeGridModel viewModel, bool integrate, Mirror xMirror, int xAxis, Mirror yMirror, int yAxis, Mirror zMirror, int zAxis)
        {
            var blocks = new List<MyObjectBuilder_CubeBlock>();
            SubtypeId outVal;

            foreach (var block in viewModel.CubeGrid.CubeBlocks.Where(b => Enum.TryParse<SubtypeId>(b.SubtypeName, out outVal) && ValidMirrorBlocks.Contains(outVal)))
            {
                var newBlock = new MyObjectBuilder_CubeBlock()
                {
                    SubtypeName = block.SubtypeName,
                    EntityId = block.EntityId == 0 ? 0 : SpaceEngineersAPI.GenerateEntityId(),
                    PersistentFlags = block.PersistentFlags,
                    Min = block.Min.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis),
                    Max = block.Max.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis),
                    Orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up))
                    //Orientation = MirrorCubeOrientation(block.SubtypeName, block.Orientation, xMirror, yMirror, zMirror);
                };
                MirrorCubeOrientation(block.SubtypeName, block.Orientation, xMirror, yMirror, zMirror, ref newBlock);

                // Don't place a block it one already exists there in the mirror.
                if (integrate && viewModel.CubeGrid.CubeBlocks.Any(b => b.Min == newBlock.Min || b.Max == newBlock.Min))
                    continue;

                if (block.PositionAndOrientation.HasValue)
                    newBlock.PositionAndOrientation = new MyPositionAndOrientation()
                    {
                        Forward = block.PositionAndOrientation.Value.Forward,
                        Position = block.PositionAndOrientation.Value.Position,
                        Up = block.PositionAndOrientation.Value.Up,
                    };

                blocks.Add(newBlock);
            }
            return blocks;
        }

        // TODO: change to a return type later when finished testing.
        private static void MirrorCubeOrientation(string subtypeName, Quaternion orientation, Mirror xMirror, Mirror yMirror, Mirror zMirror, ref MyObjectBuilder_CubeBlock block)
        {
            if (xMirror != Mirror.None)
            {
                if (subtypeName.Contains("ArmorSlope"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("Slope"));
                    switch (cubeType.Key)
                    {
                        case CubeType.SlopeCenterBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackTop]; break;
                        case CubeType.SlopeRightBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftBackCenter]; break;
                        case CubeType.SlopeLeftBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightBackCenter]; break;
                        case CubeType.SlopeCenterBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackBottom]; break;
                        case CubeType.SlopeRightCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterTop]; break;
                        case CubeType.SlopeLeftCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterTop]; break;
                        case CubeType.SlopeRightCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterBottom]; break;
                        case CubeType.SlopeLeftCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterBottom]; break;
                        case CubeType.SlopeCenterFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontTop]; break;
                        case CubeType.SlopeRightFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftFrontCenter]; break;
                        case CubeType.SlopeLeftFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightFrontCenter]; break;
                        case CubeType.SlopeCenterFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCornerInv"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("InverseCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.InverseCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontTop]; break;
                        case CubeType.InverseCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontTop]; break;
                        case CubeType.InverseCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackTop]; break;
                        case CubeType.InverseCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackTop]; break;
                        case CubeType.InverseCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontBottom]; break;
                        case CubeType.InverseCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontBottom]; break;
                        case CubeType.InverseCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackBottom]; break;
                        case CubeType.InverseCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCorner"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                        case CubeType.NormalCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                    }
                }
                else if (subtypeName.Contains("LargeRamp"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                        case CubeType.NormalCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                    }
                }
                // TODO: Other block types.
            }
            else if (yMirror != Mirror.None)
            {
                if (subtypeName.Contains("ArmorSlope"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("Slope"));
                    switch (cubeType.Key)
                    {
                        case CubeType.SlopeCenterBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontTop]; break;
                        case CubeType.SlopeRightBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightFrontCenter]; break;
                        case CubeType.SlopeLeftBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftFrontCenter]; break;
                        case CubeType.SlopeCenterBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontBottom]; break;
                        case CubeType.SlopeRightCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterTop]; break;
                        case CubeType.SlopeLeftCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterTop]; break;
                        case CubeType.SlopeRightCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterBottom]; break;
                        case CubeType.SlopeLeftCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterBottom]; break;
                        case CubeType.SlopeCenterFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackTop]; break;
                        case CubeType.SlopeRightFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightBackCenter]; break;
                        case CubeType.SlopeLeftFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftBackCenter]; break;
                        case CubeType.SlopeCenterFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCornerInv"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("InverseCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.InverseCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackTop]; break;
                        case CubeType.InverseCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackTop]; break;
                        case CubeType.InverseCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontTop]; break;
                        case CubeType.InverseCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontTop]; break;
                        case CubeType.InverseCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackBottom]; break;
                        case CubeType.InverseCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackBottom]; break;
                        case CubeType.InverseCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontBottom]; break;
                        case CubeType.InverseCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontBottom]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCorner"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                        case CubeType.NormalCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                    }
                }
                // TODO: Other block types.
            }
            else if (zMirror != Mirror.None)
            {
                if (subtypeName.Contains("ArmorSlope"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("Slope"));
                    switch (cubeType.Key)
                    {
                        case CubeType.SlopeCenterBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackBottom]; break;
                        case CubeType.SlopeRightBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightBackCenter]; break;
                        case CubeType.SlopeLeftBackCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftBackCenter]; break;
                        case CubeType.SlopeCenterBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterBackTop]; break;
                        case CubeType.SlopeRightCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterBottom]; break;
                        case CubeType.SlopeLeftCenterTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterBottom]; break;
                        case CubeType.SlopeRightCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightCenterTop]; break;
                        case CubeType.SlopeLeftCenterBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftCenterTop]; break;
                        case CubeType.SlopeCenterFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontBottom]; break;
                        case CubeType.SlopeRightFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeRightFrontCenter]; break;
                        case CubeType.SlopeLeftFrontCenter: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeLeftFrontCenter]; break;
                        case CubeType.SlopeCenterFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.SlopeCenterFrontTop]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCornerInv"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("InverseCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.InverseCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontBottom]; break;
                        case CubeType.InverseCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontBottom]; break;
                        case CubeType.InverseCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackBottom]; break;
                        case CubeType.InverseCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackBottom]; break;
                        case CubeType.InverseCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftFrontTop]; break;
                        case CubeType.InverseCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightFrontTop]; break;
                        case CubeType.InverseCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerLeftBackTop]; break;
                        case CubeType.InverseCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.InverseCornerRightBackTop]; break;
                    }
                }
                else if (subtypeName.Contains("ArmorCorner"))
                {
                    var cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(x => x.Value == orientation && x.Key.ToString().StartsWith("NormalCorner"));
                    switch (cubeType.Key)
                    {
                        case CubeType.NormalCornerLeftFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontBottom]; break;
                        case CubeType.NormalCornerRightFrontTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontBottom]; break;
                        case CubeType.NormalCornerLeftBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackBottom]; break;
                        case CubeType.NormalCornerRightBackTop: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackBottom]; break;
                        case CubeType.NormalCornerLeftFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftFrontTop]; break;
                        case CubeType.NormalCornerRightFrontBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightFrontTop]; break;
                        case CubeType.NormalCornerLeftBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerLeftBackTop]; break;
                        case CubeType.NormalCornerRightBackBottom: block.Orientation = SpaceEngineersAPI.CubeOrientations[CubeType.NormalCornerRightBackTop]; break;
                    }
                }
                // TODO: Other block types.
            }
        }

        #endregion
    }
}
