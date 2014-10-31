﻿namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using VRageMath;

    [Serializable]
    public class StructureVoxelModel : StructureBaseModel
    {
        #region fields

        private string _sourceVoxelFilepath;
        private string _voxelFilepath;
        private Vector3I _size;
        private BoundingBox _contentBounds;
        private long _voxCells;

        [NonSerialized]
        private BackgroundWorker _asyncWorker;

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

            var materialList = SpaceEngineersCore.Resources.GetMaterialList().Select(m => m.Id.SubtypeName).OrderBy(s => s).ToList();

            GameMaterialList = new List<VoxelMaterialAssetModel>(materialList.Select(s => new VoxelMaterialAssetModel { MaterialName = s, DisplayName = s }));
            EditMaterialList = new List<VoxelMaterialAssetModel> { new VoxelMaterialAssetModel { MaterialName = null, DisplayName = "Delete/Remove" } };
            EditMaterialList.AddRange(materialList.Select(s => new VoxelMaterialAssetModel { MaterialName = s, DisplayName = s }));
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
        public BoundingBox ContentBounds
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
        internal void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_VoxelMap>(VoxelMap);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
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
            _asyncWorker = new BackgroundWorker { WorkerSupportsCancellation = true};
            _asyncWorker.DoWork += delegate
            {
                if (!_isLoadingAsync && (MaterialAssets == null || MaterialAssets.Count == 0))
                {
                    _isLoadingAsync = true;

                    IsBusy = true;
                    var filename = SourceVoxelFilepath ?? VoxelFilepath;

                    Dictionary<string, long> details;
                    try
                    {
                        details = MyVoxelMap.GetMaterialAssetDetails(filename);
                    }
                    catch
                    {
                        IsBusy = false;
                        _isLoadingAsync = false;
                        return;
                    }
                    var sum = details.Values.ToList().Sum();
                    var list = new List<VoxelMaterialAssetModel>();

                    foreach (var kvp in details)
                    {
                        list.Add(new VoxelMaterialAssetModel { MaterialName = kvp.Key, Volume = (double)kvp.Value / 255, Percent = (double)kvp.Value / (double)sum });
                    }

                    MaterialAssets = list;
                    IsBusy = false;

                    _isLoadingAsync = false;
                }
            };

            _asyncWorker.RunWorkerAsync();
        }

        public override void CancelAsync()
        {
            if (_asyncWorker != null && _asyncWorker.IsBusy && _asyncWorker.WorkerSupportsCancellation)
            {
                _asyncWorker.CancelAsync();

                // TODO: kill file access to the Zip reader?
            }
        }

        private void ReadVoxelDetails(string filename)
        {
            if (filename != null && File.Exists(filename))
            {
                MyVoxelMap.GetPreview(filename, out _size, out _contentBounds, out _voxCells);
                RaisePropertyChanged(() => Size);
                RaisePropertyChanged(() => ContentSize);
                RaisePropertyChanged(() => ContentBounds);
                RaisePropertyChanged(() => VoxCells);
                RaisePropertyChanged(() => Volume);
                Center = new Vector3(_contentBounds.Center.X + 0.5f + PositionX, _contentBounds.Center.Y + 0.5f + PositionY, _contentBounds.Center.Z + 0.5f + PositionZ);
                WorldAABB = new BoundingBox(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3(Size));
            }
        }

        public override void RecalcPosition(Vector3 playerPosition)
        {
            base.RecalcPosition(playerPosition);
            Center = new Vector3(_contentBounds.Center.X + 0.5f + PositionX, _contentBounds.Center.Y + 0.5f + PositionY, _contentBounds.Center.Z + 0.5f + PositionZ);
            WorldAABB = new BoundingBox(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3(Size));
        }

        public void RotateAsteroid(VRageMath.Quaternion quaternion)
        {
            var sourceFile = SourceVoxelFilepath ?? VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            var newAsteroid = new MyVoxelMap();
            var transSize = Vector3I.Transform(asteroid.Size, quaternion);
            var newSize = Vector3I.Abs(transSize);
            newAsteroid.Init(Vector3.Zero, newSize, SpaceEngineersCore.Resources.GetDefaultMaterialName());

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
