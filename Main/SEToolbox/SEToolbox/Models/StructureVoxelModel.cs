namespace SEToolbox.Models
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
    using VRageMath;

    [Serializable]
    public class StructureVoxelModel : StructureBaseModel
    {
        #region fields

        [NonSerialized]
        private static readonly object Locker = new object();

        private string _sourceVoxelFilepath;
        private string _voxelFilepath;
        private Vector3I _size;
        private Vector3 _voxelCenter;
        private Vector3I _contentSize;
        private long _voxCells;

        [NonSerialized]
        private VoxelMaterialAssetModel _selectedMaterialAsset;

        [NonSerialized]
        private List<VoxelMaterialAssetModel> _materialAssets;

        [NonSerialized]
        private List<VoxelMaterialAssetModel> _gameMaterialList;

        [NonSerialized]
        private List<VoxelMaterialAssetModel> _editMaterialList;

        #endregion

        #region ctor

        public StructureVoxelModel(MyObjectBuilder_EntityBase entityBase, MySessionSettings settings, string voxelPath)
            : base(entityBase, settings)
        {
            if (voxelPath != null)
            {
                VoxelFilepath = Path.Combine(voxelPath, VoxelMap.Filename);
                ReadVoxelDetails(VoxelFilepath);
            }

            var materialList = SpaceEngineersApi.GetMaterialList().Select(m => m.Id.SubtypeName).OrderBy(s => s).ToList();
            
            GameMaterialList = new List<VoxelMaterialAssetModel>(materialList.Select(s => new VoxelMaterialAssetModel { MaterialName = s, DisplayName = s }));
            EditMaterialList = new List<VoxelMaterialAssetModel> { new VoxelMaterialAssetModel { MaterialName = null, DisplayName = "Delete/Remove" } };
            EditMaterialList.AddRange(materialList.Select(s => new VoxelMaterialAssetModel { MaterialName = s, DisplayName = s }));
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_VoxelMap VoxelMap
        {
            get
            {
                return EntityBase as MyObjectBuilder_VoxelMap;
            }
        }

        [XmlIgnore]
        public string Filename
        {
            get
            {
                return VoxelMap.Filename;
            }

            set
            {
                if (value != VoxelMap.Filename)
                {
                    VoxelMap.Filename = value;
                    RaisePropertyChanged(() => Filename);
                }
            }
        }

        /// <summary>
        /// This is the location of the temporary source file for importing/generating a Voxel file.
        /// </summary>
        public string SourceVoxelFilepath
        {
            get
            {
                return _sourceVoxelFilepath;
            }

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
            get
            {
                return _voxelFilepath;
            }

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
            get
            {
                return _size;
            }

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
            get
            {
                return _contentSize;
            }

            set
            {
                if (value != _contentSize)
                {
                    _contentSize = value;
                    RaisePropertyChanged(() => ContentSize);
                }
            }
        }

        [XmlIgnore]
        public long VoxCells
        {
            get
            {
                return _voxCells;
            }

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
            get
            {
                return (double)_voxCells / 255;
            }
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
            DisplayName = Path.GetFileNameWithoutExtension(VoxelMap.Filename);
        }

        public override void InitializeAsync()
        {
            var worker = new BackgroundWorker();

            worker.DoWork += delegate(object s, DoWorkEventArgs workArgs)
            {
                lock (Locker)
                {
                    if (MaterialAssets == null || MaterialAssets.Count == 0)
                    {
                        IsBusy = true;
                        var filename = SourceVoxelFilepath;
                        if (string.IsNullOrEmpty(filename))
                            filename = VoxelFilepath;

                        Dictionary<string, long> details;
                        try
                        {
                            details = MyVoxelMap.GetMaterialAssetDetails(filename);
                        }
                        catch
                        {
                            IsBusy = false;
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
                    }
                }
            };

            worker.RunWorkerAsync();
        }

        private void ReadVoxelDetails(string filename)
        {
            if (filename != null && File.Exists(filename))
            {
                MyVoxelMap.GetPreview(filename, out _size, out _contentSize, out _voxelCenter, out _voxCells);
                RaisePropertyChanged(() => Size);
                RaisePropertyChanged(() => ContentSize);
                RaisePropertyChanged(() => VoxCells);
                RaisePropertyChanged(() => Volume);
                Center = new Vector3(_voxelCenter.X + 0.5f + PositionX, _voxelCenter.Y + 0.5f + PositionY, _voxelCenter.Z + 0.5f + PositionZ);
            }
        }

        public override void RecalcPosition(Vector3 playerPosition)
        {
            base.RecalcPosition(playerPosition);
            Center = new Vector3(_voxelCenter.X + 0.5f + PositionX, _voxelCenter.Y + 0.5f + PositionY, _voxelCenter.Z + 0.5f + PositionZ);
        }

        #endregion
    }
}
