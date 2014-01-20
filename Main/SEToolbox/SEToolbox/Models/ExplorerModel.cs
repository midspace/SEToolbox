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
            // Optimise ordering of CubeBlocks within structure, so that loops can load quickly based on {X++, Y++, Z++}.
            //var neworder = viewModel.CubeGrid.CubeBlocks.OrderBy(c => c.Min.Z).ThenBy(c => c.Min.Y).ThenBy(c => c.Min.X).ToList();

            var neworder = viewModel.CubeGrid.CubeBlocks.OrderBy(c => c.Min.Z).ThenBy(c => c.Min.Y).ThenBy(c => c.Min.X).ToList();
            //var neworder = viewModel.CubeGrid.CubeBlocks.OrderBy(c => c.Min.Z).ThenByDescending(c => c.Min.Y).ThenBy(c => c.Min.X).ToList(); // {X++, Y--, Z++}.
            viewModel.CubeGrid.CubeBlocks = neworder;
            this.IsModified = true;
        }

        public void MirrorModel(StructureCubeGridModel viewModel, bool oddMirror)
        {
            // Find mirror Axis.
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

            var xMirror = Mirror.None;
            var yMirror = Mirror.None;
            var zMirror = Mirror.None;
            var xAxis = 0;
            var yAxis = 0;
            var zAxis = 0;

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

            var blocks = new List<MyObjectBuilder_CubeBlock>();

            foreach (var block in viewModel.CubeGrid.CubeBlocks)
            {
                if (block.SubtypeName.Contains("Armor"))
                {
                    var newBlock = new MyObjectBuilder_CubeBlock()
                    {
                        SubtypeName = block.SubtypeName,
                        EntityId = block.EntityId == 0 ? 0 : SpaceEngineersAPI.GenerateEntityId(),
                        PersistentFlags = block.PersistentFlags,
                        Min = block.Min.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis),
                        Max = block.Max.Mirror(xMirror, xAxis, yMirror, yAxis, zMirror, zAxis),
                        Orientation = block.Orientation
                        //Orientation = MirrorCubeOrientation(block.SubtypeName, block.Orientation, xMirror, yMirror, zMirror)
                    };
                    MirrorCubeOrientation(ref newBlock, block.Orientation, xMirror, yMirror, zMirror);

                    if (block.PositionAndOrientation.HasValue)
                        newBlock.PositionAndOrientation = new MyPositionAndOrientation()
                        {
                            Forward = block.PositionAndOrientation.Value.Forward,
                            Position = block.PositionAndOrientation.Value.Position,
                            Up = block.PositionAndOrientation.Value.Up,
                        };

                    blocks.Add(newBlock);
                }
            }

            viewModel.CubeGrid.CubeBlocks.AddRange(blocks);
            viewModel.UpdateFromEntityBase();
            this.IsModified = true;
        }

        private static void MirrorCubeOrientation(ref MyObjectBuilder_CubeBlock block, /*string subtypeName, */ Quaternion orientation, Mirror xMirror, Mirror yMirror, Mirror zMirror)
        {
            var normal = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));

            if (xMirror != Mirror.None)
            {
                var nominal = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up));

                // TODO: 
                if (block.SubtypeName.Contains("ArmorSlope"))
                {
                    if (orientation == normal)
                    {
                        block.SubtypeName += "Red";
                    }
                    block.Orientation.Y = -orientation.Y;
                    block.Orientation.Z = -orientation.Z;
                }
                else if (block.SubtypeName.Contains("ArmorCorner"))
                {
                    if (orientation == normal)
                    {
                        block.SubtypeName += "Red";
                    }
                    //block.Orientation.X = -orientation.X;
                    block.Orientation.Y = -orientation.Y;
                    block.Orientation.Z = -orientation.Z;
                    //block.Orientation.W = -orientation.W;

                    //SharpDX.
                    //orientation.Axis
                    //Quaternion.
                    //var zz = orientation * Vector3.Forward;
                    //orientation.

                    //MathUtil..
                    

                }
                // TODO: Other block types.
            }
            else if (yMirror != Mirror.None)
            {
                var nominal = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, Vector3.Right, Vector3.Up));
                // TODO: 

            }
            else if (zMirror != Mirror.None)
            {
                var nominal = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Right));
                // TODO: 

            }
            //var m = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

            //newOrientation = Quaternion.Add(orientation, Quaternion.Subtract(normal, orientation));

            // nominal
            // normal

            //MathUtil.matrix
            //MathUtil.MatrixToEulerXYZ();

            //Matrix.
            //m = Matrix.Invert(m);
            //m = Matrix.Normalize(m);
            //newOrientation =  Quaternion.CreateFromRotationMatrix(m);
            //newOrientation = Quaternion.Negate(orientation);
            //newOrientation = Quaternion.Inverse(orientation);

            //return newOrientation;
        }

        #endregion
    }
}
