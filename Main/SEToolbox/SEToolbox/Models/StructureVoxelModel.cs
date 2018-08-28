namespace SEToolbox.Models
{
    using Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Voxels;
    using VRageMath;
    using Res = SEToolbox.Properties.Resources;

    [Serializable]
    public class StructureVoxelModel : StructureBaseModel
    {
        #region fields

        private string _sourceVoxelFilepath;
        private string _voxelFilepath;
        private Vector3I _size;
        private BoundingBoxI _contentBounds;
        private BoundingBoxI _inflatedContentBounds;
        private long _voxCells;

        [NonSerialized]
        private BackgroundWorker _asyncWorker;

        [NonSerialized]
        private MyVoxelMap _voxelMap;

        [NonSerialized]
        private VoxelMaterialAssetModel _selectedMaterialAsset;

        [NonSerialized]
        private List<VoxelMaterialAssetModel> _materialAssets;

        [NonSerialized]
        private List<VoxelMaterialAssetModel> _gameMaterialList;

        [NonSerialized]
        private List<VoxelMaterialAssetModel> _editMaterialList;

        [NonSerialized]
        private bool _isLoadingAsync;

        #endregion

        #region ctor

        public StructureVoxelModel(MyObjectBuilder_EntityBase entityBase, string voxelPath)
            : base(entityBase)
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            if (voxelPath != null)
            {
                VoxelFilepath = Path.Combine(voxelPath, Name + MyVoxelMap.V2FileExtension);
                var previewFile = VoxelFilepath;

                if (!File.Exists(VoxelFilepath))
                {
                    var oldFilepath = Path.Combine(voxelPath, Name + MyVoxelMap.V1FileExtension);
                    if (File.Exists(oldFilepath))
                    {
                        SourceVoxelFilepath = oldFilepath;
                        previewFile = oldFilepath;
                        SpaceEngineersCore.ManageDeleteVoxelList.Add(oldFilepath);
                    }
                }

                ReadVoxelDetails(previewFile);
            }

            var materialList = new Dictionary<string, string>();
            foreach (MyVoxelMaterialDefinition item in SpaceEngineersCore.Resources.VoxelMaterialDefinitions.OrderBy(m => m.Id.SubtypeName))
            {
                string texture = item.GetVoxelDisplayTexture();
                materialList.Add(item.Id.SubtypeName, texture == null ? null : SpaceEngineersCore.GetDataPathOrDefault(texture, Path.Combine(contentPath, texture)));
            }

            GameMaterialList = new List<VoxelMaterialAssetModel>(materialList.Select(m => new VoxelMaterialAssetModel { MaterialName = m.Key, DisplayName = m.Key, TextureFile = m.Value }));
            EditMaterialList = new List<VoxelMaterialAssetModel> { new VoxelMaterialAssetModel { MaterialName = null, DisplayName = Res.CtlVoxelMnuRemoveMaterial } };
            EditMaterialList.AddRange(materialList.Select(m => new VoxelMaterialAssetModel { MaterialName = m.Key, DisplayName = m.Key, TextureFile = m.Value }));
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_VoxelMap VoxelMap
        {
            get { return EntityBase as MyObjectBuilder_VoxelMap; }
        }

        [XmlIgnore]
        public string Name
        {
            get { return VoxelMap.StorageName; }

            set
            {
                if (value != VoxelMap.StorageName)
                {
                    VoxelMap.StorageName = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// This is the location of the temporary source file for importing/generating a Voxel file.
        /// </summary>
        public string SourceVoxelFilepath
        {
            get { return _sourceVoxelFilepath; }

            set
            {
                if (value != _sourceVoxelFilepath)
                {
                    _sourceVoxelFilepath = value;
                    OnPropertyChanged(nameof(SourceVoxelFilepath));
                    ReadVoxelDetails(SourceVoxelFilepath);
                }
            }
        }

        /// <summary>
        /// This is the actual file/path for the Voxel file. It may not exist yet.
        /// </summary>
        public string VoxelFilepath
        {
            get { return _voxelFilepath; }

            set
            {
                if (value != _voxelFilepath)
                {
                    _voxelFilepath = value;
                    OnPropertyChanged(nameof(VoxelFilepath));
                }
            }
        }

        [XmlIgnore]
        public Vector3I Size
        {
            get { return _size; }

            set
            {
                if (value != _size)
                {
                    _size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        [XmlIgnore]
        public Vector3I ContentSize
        {
            get { return _contentBounds.Size + 1; } // Content size
        }

        /// <summary>
        /// Represents the Cell content, not the Cell boundary.
        /// So Min and Max values are both inclusive.
        /// </summary>
        [XmlIgnore]
        public BoundingBoxI ContentBounds
        {
            get { return _contentBounds; }

            set
            {
                if (value != _contentBounds)
                {
                    _contentBounds = value;
                    OnPropertyChanged(nameof(ContentBounds));
                }
            }
        }

        [XmlIgnore]
        public BoundingBoxI InflatedContentBounds => _inflatedContentBounds;

        [XmlIgnore]
        public long VoxCells
        {
            get { return _voxCells; }

            set
            {
                if (value != _voxCells)
                {
                    _voxCells = value;
                    OnPropertyChanged(nameof(VoxCells), nameof(Volume));
                }
            }
        }

        [XmlIgnore]
        public double Volume
        {
            get { return (double)_voxCells / 255; }
        }

        /// <summary>
        /// This is detail of the breakdown of ores in the asteroid.
        /// </summary>
        [XmlIgnore]
        public List<VoxelMaterialAssetModel> MaterialAssets
        {
            get { return _materialAssets; }

            set
            {
                if (value != _materialAssets)
                {
                    _materialAssets = value;
                    OnPropertyChanged(nameof(MaterialAssets));
                }
            }
        }

        [XmlIgnore]
        public VoxelMaterialAssetModel SelectedMaterialAsset
        {
            get { return _selectedMaterialAsset; }

            set
            {
                if (value != _selectedMaterialAsset)
                {
                    _selectedMaterialAsset = value;
                    OnPropertyChanged(nameof(SelectedMaterialAsset));
                }
            }
        }

        [XmlIgnore]
        public List<VoxelMaterialAssetModel> GameMaterialList
        {
            get { return _gameMaterialList; }

            set
            {
                if (value != _gameMaterialList)
                {
                    _gameMaterialList = value;
                    OnPropertyChanged(nameof(GameMaterialList));
                }
            }
        }

        [XmlIgnore]
        public List<VoxelMaterialAssetModel> EditMaterialList
        {
            get { return _editMaterialList; }

            set
            {
                if (value != _editMaterialList)
                {
                    _editMaterialList = value;
                    OnPropertyChanged(nameof(EditMaterialList));
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_VoxelMap>(VoxelMap);
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_VoxelMap>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Voxel;
            DisplayName = Name;
        }

        public override void InitializeAsync()
        {
            _asyncWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _asyncWorker.DoWork += delegate
            {
                if (!_isLoadingAsync)
                {
                    _isLoadingAsync = true;

                    IsBusy = true;
                    LoadDetailsSync();
                    IsBusy = false;

                    _isLoadingAsync = false;
                }
            };
            _asyncWorker.RunWorkerCompleted += delegate
            {
                OnPropertyChanged(nameof(Size), nameof(ContentSize), nameof(ContentBounds), nameof(Center), nameof(VoxCells), nameof(Volume));
            };

            _asyncWorker.RunWorkerAsync();
        }

        public void LoadDetailsSync()
        {
            if (_voxelMap != null && (MaterialAssets == null || MaterialAssets.Count == 0))
            {
                Dictionary<string, long> details = _voxelMap.RefreshAssets();
                _contentBounds = _voxelMap.BoundingContent;
                _inflatedContentBounds = _voxelMap.InflatedBoundingContent;
                _voxCells = _voxelMap.VoxCells;
                Center = new Vector3D(_voxelMap.ContentCenter.X + 0.5f + PositionX, _voxelMap.ContentCenter.Y + 0.5f + PositionY, _voxelMap.ContentCenter.Z + 0.5f + PositionZ);

                var sum = details.Values.ToList().Sum();
                var list = new List<VoxelMaterialAssetModel>();

                foreach (var kvp in details)
                    list.Add(new VoxelMaterialAssetModel {MaterialName = kvp.Key, Volume = (double) kvp.Value/255, Percent = (double) kvp.Value/(double) sum});

                MaterialAssets = list;
            }
        }

        public override void CancelAsync()
        {
            if (_asyncWorker != null && _asyncWorker.IsBusy && _asyncWorker.WorkerSupportsCancellation)
            {
                _asyncWorker.CancelAsync();
            }
        }

        private void ReadVoxelDetails(string filename)
        {
            if (_voxelMap == null && filename != null && File.Exists(filename))
            {
                _voxelMap = new MyVoxelMap();
                _voxelMap.Load(filename);

                Size = _voxelMap.Size;
                ContentBounds = _voxelMap.BoundingContent;
                IsValid = _voxelMap.IsValid;

                OnPropertyChanged(nameof(Size), nameof(ContentSize), nameof(IsValid));
                Center = new Vector3D(_voxelMap.ContentCenter.X + 0.5f + PositionX, _voxelMap.ContentCenter.Y + 0.5f + PositionY, _voxelMap.ContentCenter.Z + 0.5f + PositionZ);
                WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
            }
        }

        public override void RecalcPosition(Vector3D playerPosition)
        {
            base.RecalcPosition(playerPosition);
            if (IsValid)
            {
                Center = new Vector3D(_voxelMap.ContentCenter.X + 0.5f + PositionX, _voxelMap.ContentCenter.Y + 0.5f + PositionY, _voxelMap.ContentCenter.Z + 0.5f + PositionZ);
                WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
            }
        }

        public void UpdateNewSource(MyVoxelMap newMap, string fileName)
        {
            if (_voxelMap != null)
                _voxelMap.Dispose();
            _voxelMap = newMap;
            SourceVoxelFilepath = fileName;

            Size = _voxelMap.Size;
            ContentBounds = _voxelMap.BoundingContent;
            IsValid = _voxelMap.IsValid;

            OnPropertyChanged(nameof(Size), nameof(ContentSize), nameof(IsValid));
            Center = new Vector3D(_voxelMap.ContentCenter.X + 0.5f + PositionX, _voxelMap.ContentCenter.Y + 0.5f + PositionY, _voxelMap.ContentCenter.Z + 0.5f + PositionZ);
            WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
        }

        public void RotateAsteroid(Quaternion quaternion)
        {
            var sourceFile = SourceVoxelFilepath ?? VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile);

            var newAsteroid = new MyVoxelMap();
            var newSize = asteroid.Size;
            newAsteroid.Create(newSize, SpaceEngineersCore.Resources.GetDefaultMaterialIndex());

            Vector3I block;
            var halfSize = asteroid.Storage.Size / 2;
            
            // Don't use anything smaller than 64 for smaller voxels, as it trashes the cache.
            var cacheSize = new Vector3I(64); 
            var halfCacheSize = new Vector3I(32); // This should only be used for the Transform, not the cache.

            // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
            for (block.Z = 0; block.Z < asteroid.Storage.Size.Z; block.Z += 64)
                for (block.Y = 0; block.Y < asteroid.Storage.Size.Y; block.Y += 64)
                    for (block.X = 0; block.X < asteroid.Storage.Size.X; block.X += 64)
                    {
                        #region source voxel

                        var cache = new MyStorageData();
                        cache.Resize(cacheSize);
                        // LOD1 is not detailed enough for content information on asteroids.
                        asteroid.Storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, block, block + cacheSize - 1);

                        #endregion

                        #region target Voxel

                        // the block is a cubiod. The entire space needs to rotate, to be able to gauge where the new block position starts from.
                        var newBlockMin = Vector3I.Transform(block - halfSize, quaternion) + halfSize;
                        var newBlockMax = Vector3I.Transform(block + 64 - halfSize, quaternion) + halfSize;
                        var newBlock = Vector3I.Min(newBlockMin, newBlockMax);

                        var newCache = new MyStorageData();
                        newCache.Resize(cacheSize);
                        newAsteroid.Storage.ReadRange(newCache, MyStorageDataTypeFlags.ContentAndMaterial, 0, newBlock, newBlock + cacheSize - 1);

                        #endregion

                        bool changed = false;
                        Vector3I p;
                        for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                            for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                {
                                    byte volume = cache.Content(ref p);
                                    byte cellMaterial = cache.Material(ref p);

                                    var newP1 = Vector3I.Transform(p - halfCacheSize, quaternion) + halfCacheSize;
                                    var newP2 = Vector3I.Transform(p + 1 - halfCacheSize, quaternion) + halfCacheSize;
                                    var newP = Vector3I.Min(newP1, newP2);

                                    newCache.Content(ref newP, volume);
                                    newCache.Material(ref newP, cellMaterial);
                                    changed = true;
                                }

                        if (changed)
                            newAsteroid.Storage.WriteRange(newCache, MyStorageDataTypeFlags.ContentAndMaterial, newBlock, newBlock + cacheSize - 1);
                    }


            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            newAsteroid.Save(tempfilename);

            SourceVoxelFilepath = tempfilename;
        }

        public bool ExtractStationIntersect(IMainView mainViewModel, bool tightIntersection)
        {
            // Make a shortlist of station Entities in the bounding box of the asteroid.
            var asteroidWorldAABB = new BoundingBoxD((Vector3D)ContentBounds.Min + PositionAndOrientation.Value.Position, (Vector3D)ContentBounds.Max + PositionAndOrientation.Value.Position);
            var stations = mainViewModel.GetIntersectingEntities(asteroidWorldAABB).Where(e => e.ClassType == ClassType.LargeStation).Cast<StructureCubeGridModel>().ToList();

            if (stations.Count == 0)
                return false;

            var modified = false;
            var sourceFile = SourceVoxelFilepath ?? VoxelFilepath;
            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile);

            var total = stations.Sum(s => s.CubeGrid.CubeBlocks.Count);
            mainViewModel.ResetProgress(0, total);

            // Search through station entities cubes for intersection with this voxel.
            foreach (var station in stations)
            {
                var quaternion = station.PositionAndOrientation.Value.ToQuaternion();

                foreach (var cube in station.CubeGrid.CubeBlocks)
                {
                    mainViewModel.IncrementProgress();

                    var definition = SpaceEngineersApi.GetCubeDefinition(cube.TypeId, station.CubeGrid.GridSizeEnum, cube.SubtypeName);

                    var orientSize = definition.Size.Transform(cube.BlockOrientation).Abs();
                    var min = cube.Min.ToVector3() * station.CubeGrid.GridSizeEnum.ToLength();
                    var max = (cube.Min + orientSize) * station.CubeGrid.GridSizeEnum.ToLength();
                    var p1 = Vector3D.Transform(min, quaternion) + station.PositionAndOrientation.Value.Position - (station.CubeGrid.GridSizeEnum.ToLength() / 2);
                    var p2 = Vector3D.Transform(max, quaternion) + station.PositionAndOrientation.Value.Position - (station.CubeGrid.GridSizeEnum.ToLength() / 2);
                    var cubeWorldAABB = new BoundingBoxD(Vector3.Min(p1, p2), Vector3.Max(p1, p2));

                    // find worldAABB of block.
                    if (asteroidWorldAABB.Intersects(cubeWorldAABB))
                    {
                        Vector3I block;
                        var cacheSize = new Vector3I(64);
                        Vector3D position = PositionAndOrientation.Value.Position;

                        // read the asteroid in chunks of 64 to avoid the Arithmetic overflow issue.
                        for (block.Z = 0; block.Z < asteroid.Storage.Size.Z; block.Z += 64)
                            for (block.Y = 0; block.Y < asteroid.Storage.Size.Y; block.Y += 64)
                                for (block.X = 0; block.X < asteroid.Storage.Size.X; block.X += 64)
                                {
                                    var cache = new MyStorageData();
                                    cache.Resize(cacheSize);
                                    // LOD1 is not detailed enough for content information on asteroids.
                                    Vector3I maxRange = block + cacheSize - 1;
                                    asteroid.Storage.ReadRange(cache, MyStorageDataTypeFlags.Content, 0, block, maxRange);

                                    bool changed = false;
                                    Vector3I p;
                                    for (p.Z = 0; p.Z < cacheSize.Z; ++p.Z)
                                        for (p.Y = 0; p.Y < cacheSize.Y; ++p.Y)
                                            for (p.X = 0; p.X < cacheSize.X; ++p.X)
                                            {
                                                BoundingBoxD voxelCellBox = new BoundingBoxD(position + p + block, position + p + block + 1);
                                                ContainmentType contains = cubeWorldAABB.Contains(voxelCellBox);

                                                // TODO: finish tightIntersection. Will require high interpretation of voxel content volumes.

                                                if (contains == ContainmentType.Contains || contains == ContainmentType.Intersects)
                                                {
                                                    cache.Content(ref p, 0);
                                                    changed = true;
                                                }
                                            }

                                    if (changed)
                                    {
                                        asteroid.Storage.WriteRange(cache, MyStorageDataTypeFlags.Content, block, maxRange);
                                        modified = true;
                                    }
                                }

                    }
                }
            }

            mainViewModel.ClearProgress();

            if (modified)
            {
                var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
                asteroid.Save(tempfilename);
                // replaces the existing asteroid file, as it is still the same size and dimentions.
                UpdateNewSource(asteroid, tempfilename);
                MaterialAssets = null;
                InitializeAsync();
            }
            return modified;
        }

        #endregion
    }
}
