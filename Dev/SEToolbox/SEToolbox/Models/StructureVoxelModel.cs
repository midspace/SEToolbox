namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Sandbox.Definitions;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;

    [Serializable]
    public class StructureVoxelModel : StructureBaseModel
    {
        #region fields

        private string _sourceVoxelFilepath;
        private string _voxelFilepath;
        private Vector3I _size;
        private BoundingBoxD _contentBounds;
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
            EditMaterialList = new List<VoxelMaterialAssetModel> { new VoxelMaterialAssetModel { MaterialName = null, DisplayName = "Delete/Remove" } };
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
                    RaisePropertyChanged(() => Name);
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
                    RaisePropertyChanged(() => SourceVoxelFilepath);
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
                    RaisePropertyChanged(() => VoxelFilepath);
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
                    RaisePropertyChanged(() => Size);
                }
            }
        }

        [XmlIgnore]
        public Vector3I ContentSize
        {
            get { return _contentBounds.SizeInt() + 1; } // Content size
        }

        /// <summary>
        /// Represents the Cell content, not the Cell boundary.
        /// So Min and Max values are both inclusive.
        /// </summary>
        [XmlIgnore]
        public BoundingBoxD ContentBounds
        {
            get { return _contentBounds; }

            set
            {
                if (value != _contentBounds)
                {
                    _contentBounds = value;
                    RaisePropertyChanged(() => ContentBounds);
                }
            }
        }

        [XmlIgnore]
        public long VoxCells
        {
            get { return _voxCells; }

            set
            {
                if (value != _voxCells)
                {
                    _voxCells = value;
                    RaisePropertyChanged(() => VoxCells);
                    RaisePropertyChanged(() => Volume);
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
                    RaisePropertyChanged(() => MaterialAssets);
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
                    RaisePropertyChanged(() => SelectedMaterialAsset);
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
                    RaisePropertyChanged(() => GameMaterialList);
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
                    RaisePropertyChanged(() => EditMaterialList);
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
                if (!_isLoadingAsync && (MaterialAssets == null || MaterialAssets.Count == 0))
                {
                    _isLoadingAsync = true;

                    IsBusy = true;

                    Dictionary<string, long> details;

                    details = _voxelMap.RefreshAssets();
                    _contentBounds = _voxelMap.BoundingContent;
                    _voxCells = _voxelMap.VoxCells;
                    Center = new Vector3D(_contentBounds.Center.X + 0.5f + PositionX, _contentBounds.Center.Y + 0.5f + PositionY, _contentBounds.Center.Z + 0.5f + PositionZ);

                    var sum = details.Values.ToList().Sum();
                    var list = new List<VoxelMaterialAssetModel>();

                    foreach (var kvp in details)
                        list.Add(new VoxelMaterialAssetModel { MaterialName = kvp.Key, Volume = (double)kvp.Value / 255, Percent = (double)kvp.Value / (double)sum });

                    MaterialAssets = list;
                    IsBusy = false;

                    _isLoadingAsync = false;
                }
            };
            _asyncWorker.RunWorkerCompleted += delegate
            {
                RaisePropertyChanged(() => Size);
                RaisePropertyChanged(() => ContentSize);
                RaisePropertyChanged(() => ContentBounds);
                RaisePropertyChanged(() => Center);
                RaisePropertyChanged(() => VoxCells);
                RaisePropertyChanged(() => Volume);
            };

            _asyncWorker.RunWorkerAsync();
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

                RaisePropertyChanged(() => Size);
                RaisePropertyChanged(() => ContentSize);
                RaisePropertyChanged(() => IsValid);
                Center = new Vector3D(_contentBounds.Center.X + 0.5f + PositionX, _contentBounds.Center.Y + 0.5f + PositionY, _contentBounds.Center.Z + 0.5f + PositionZ);
                WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
            }
        }

        public override void RecalcPosition(Vector3D playerPosition)
        {
            base.RecalcPosition(playerPosition);
            Center = new Vector3D(_contentBounds.Center.X + 0.5f + PositionX, _contentBounds.Center.Y + 0.5f + PositionY, _contentBounds.Center.Z + 0.5f + PositionZ);
            WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
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

            RaisePropertyChanged(() => Size);
            RaisePropertyChanged(() => ContentSize);
            RaisePropertyChanged(() => IsValid);
            Center = new Vector3D(_contentBounds.Center.X + 0.5f + PositionX, _contentBounds.Center.Y + 0.5f + PositionY, _contentBounds.Center.Z + 0.5f + PositionZ);
            WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
        }

        public void RotateAsteroid(VRageMath.Quaternion quaternion)
        {
            var sourceFile = SourceVoxelFilepath ?? VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile);

            var newAsteroid = new MyVoxelMap();
            var transSize = Vector3I.Transform(asteroid.Size, quaternion);
            var newSize = Vector3I.Abs(transSize);
            newAsteroid.Create(newSize, SpaceEngineersCore.Resources.GetDefaultMaterialName());

            Vector3I coords;
            for (coords.Z = 0; coords.Z < asteroid.Size.Z; coords.Z++)
            {
                for (coords.Y = 0; coords.Y < asteroid.Size.Y; coords.Y++)
                {
                    for (coords.X = 0; coords.X < asteroid.Size.X; coords.X++)
                    {
                        byte volume = 0xff;
                        string cellMaterial;

                        asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                        var newCoord = Vector3I.Transform(coords, quaternion);
                        // readjust the points, as rotation occurs arround 0,0,0.
                        newCoord.X = newCoord.X < 0 ? newCoord.X - transSize.X : newCoord.X;
                        newCoord.Y = newCoord.Y < 0 ? newCoord.Y - transSize.Y : newCoord.Y;
                        newCoord.Z = newCoord.Z < 0 ? newCoord.Z - transSize.Z : newCoord.Z;
                        newAsteroid.SetVoxelContent(volume, ref newCoord);
                        newAsteroid.SetVoxelMaterialAndIndestructibleContent(cellMaterial, 0xff, ref newCoord);
                    }
                }
            }

            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            newAsteroid.Save(tempfilename);

            SourceVoxelFilepath = tempfilename;
        }

        #endregion
    }
}
