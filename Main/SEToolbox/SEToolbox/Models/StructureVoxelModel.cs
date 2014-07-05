namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using VRageMath;

    [Serializable]
    public class StructureVoxelModel : StructureBaseModel
    {
        [NonSerialized]
        private static readonly object Locker = new object();

        private string _sourceVoxelFilepath;
        private string _voxelFilepath;
        private Vector3I _size;
        private Vector3 _voxelCenter;
        private Vector3I _contentSize;
        private long _voxCells;

        [NonSerialized]
        private List<VoxelMaterialAssetModel> _materialAssets;

        #region ctor

        public StructureVoxelModel(MyObjectBuilder_EntityBase entityBase, MySessionSettings settings, string voxelPath)
            : base(entityBase, settings)
        {
            if (voxelPath != null)
            {
                this.VoxelFilepath = Path.Combine(voxelPath, this.VoxelMap.Filename);
                this.ReadVoxelDetails(this.VoxelFilepath);
            }
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_VoxelMap VoxelMap
        {
            get
            {
                return this.EntityBase as MyObjectBuilder_VoxelMap;
            }
        }

        [XmlIgnore]
        public string Filename
        {
            get
            {
                return this.VoxelMap.Filename;
            }

            set
            {
                if (value != this.VoxelMap.Filename)
                {
                    this.VoxelMap.Filename = value;
                    this.RaisePropertyChanged(() => Filename);
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
                return this._sourceVoxelFilepath;
            }

            set
            {
                if (value != this._sourceVoxelFilepath)
                {
                    this._sourceVoxelFilepath = value;
                    this.RaisePropertyChanged(() => SourceVoxelFilepath);
                    this.ReadVoxelDetails(this.SourceVoxelFilepath);
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
                return this._voxelFilepath;
            }

            set
            {
                if (value != this._voxelFilepath)
                {
                    this._voxelFilepath = value;
                    this.RaisePropertyChanged(() => VoxelFilepath);
                }
            }
        }

        [XmlIgnore]
        public Vector3I Size
        {
            get
            {
                return this._size;
            }

            set
            {
                if (value != this._size)
                {
                    this._size = value;
                    this.RaisePropertyChanged(() => Size);
                }
            }
        }

        [XmlIgnore]
        public Vector3I ContentSize
        {
            get
            {
                return this._contentSize;
            }

            set
            {
                if (value != this._contentSize)
                {
                    this._contentSize = value;
                    this.RaisePropertyChanged(() => ContentSize);
                }
            }
        }

        [XmlIgnore]
        public long VoxCells
        {
            get
            {
                return this._voxCells;
            }

            set
            {
                if (value != this._voxCells)
                {
                    this._voxCells = value;
                    this.RaisePropertyChanged(() => VoxCells);
                }
            }
        }

        [XmlIgnore]
        public double Volume
        {
            get
            {
                return (double)this._voxCells / 255;
            }
        }

        /// <summary>
        /// This is detail of the breakdown of ores in the asteroid.
        /// </summary>
        [XmlIgnore]
        public List<VoxelMaterialAssetModel> MaterialAssets
        {
            get
            {
                return this._materialAssets;
            }

            set
            {
                if (value != this._materialAssets)
                {
                    this._materialAssets = value;
                    this.RaisePropertyChanged(() => MaterialAssets);
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            this.SerializedEntity = SpaceEngineersAPI.Serialize<MyObjectBuilder_VoxelMap>(this.VoxelMap);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            this.EntityBase = SpaceEngineersAPI.Deserialize<MyObjectBuilder_VoxelMap>(this.SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            this.ClassType = ClassType.Voxel;
            this.DisplayName = Path.GetFileNameWithoutExtension(this.VoxelMap.Filename);
        }

        public override void InitializeAsync()
        {
            var worker = new BackgroundWorker();

            worker.DoWork += delegate(object s, DoWorkEventArgs workArgs)
            {
                lock (Locker)
                {
                    if (this.MaterialAssets == null || this.MaterialAssets.Count == 0)
                    {
                        this.IsBusy = true;
                        var filename = this.SourceVoxelFilepath;
                        if (string.IsNullOrEmpty(filename))
                            filename = this.VoxelFilepath;

                        Dictionary<string, long> details;
                        try
                        {
                            details = MyVoxelMap.GetMaterialAssetDetails(filename);
                        }
                        catch
                        {
                            this.IsBusy = false;
                            return;
                        }
                        var sum = details.Values.ToList().Sum();
                        var list = new List<VoxelMaterialAssetModel>();

                        foreach (var kvp in details)
                        {
                            list.Add(new VoxelMaterialAssetModel() { MaterialName = kvp.Key, Volume = (double)kvp.Value / 255, Percent = (double)kvp.Value / (double)sum });
                        }

                        this.MaterialAssets = list;
                        this.IsBusy = false;
                    }
                }
            };

            worker.RunWorkerAsync();
        }

        private void ReadVoxelDetails(string filename)
        {
            if (filename != null && File.Exists(filename))
            {
                MyVoxelMap.GetPreview(filename, out this._size, out this._contentSize, out _voxelCenter, out this._voxCells);
                this.RaisePropertyChanged(() => Size);
                this.RaisePropertyChanged(() => ContentSize);
                this.RaisePropertyChanged(() => VoxCells);
                this.Center = new Vector3(_voxelCenter.X + 0.5f + this.PositionX, _voxelCenter.Y + 0.5f + this.PositionY, _voxelCenter.Z + 0.5f + this.PositionZ);
            }
        }

        public override void RecalcPosition(Vector3 playerPosition)
        {
            base.RecalcPosition(playerPosition);
            this.Center = new Vector3(_voxelCenter.X + 0.5f + this.PositionX, _voxelCenter.Y + 0.5f + this.PositionY, _voxelCenter.Z + 0.5f + this.PositionZ);
        }

        #endregion
    }
}
