namespace SEToolbox.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Documents;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml.Serialization;
    using VRageMath;

    [Serializable]
    public class StructureVoxelModel : StructureBaseModel
    {
        [XmlIgnore]
        private static readonly object Locker = new object();

        private string _sourceVoxelFilepath;
        private string _voxelFilepath;
        private Vector3I _size;
        private Vector3I _contentSize;
        private long _voxCells;

        [XmlIgnore]
        private List<VoxelMaterialAssetModel> _materialAssets;

        #region ctor

        public StructureVoxelModel(MyObjectBuilder_EntityBase entityBase, string voxelPath)
            : base(entityBase)
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

        [XmlIgnore]
        public double PositionX
        {
            get
            {
                return this.VoxelMap.PositionAndOrientation.Value.Position.X.ToDouble();
            }

            set
            {
                if ((float)value != this.VoxelMap.PositionAndOrientation.Value.Position.X)
                {
                    var pos = this.VoxelMap.PositionAndOrientation.Value;
                    pos.Position.X = (float)value;
                    this.VoxelMap.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionX);
                }
            }
        }

        [XmlIgnore]
        public double PositionY
        {
            get
            {
                return this.VoxelMap.PositionAndOrientation.Value.Position.Y.ToDouble();
            }

            set
            {
                if ((float)value != this.VoxelMap.PositionAndOrientation.Value.Position.Y)
                {
                    var pos = this.VoxelMap.PositionAndOrientation.Value;
                    pos.Position.Y = (float)value;
                    this.VoxelMap.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionY);
                }
            }
        }

        [XmlIgnore]
        public double PositionZ
        {
            get
            {
                return this.VoxelMap.PositionAndOrientation.Value.Position.Z.ToDouble();
            }

            set
            {
                if ((float)value != this.VoxelMap.PositionAndOrientation.Value.Position.Z)
                {
                    var pos = this.VoxelMap.PositionAndOrientation.Value;
                    pos.Position.Z = (float)value;
                    this.VoxelMap.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionZ);
                }
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
                        var details = MyVoxelMap.GetMaterialAssetDetails(filename);
                        var sum = details.Values.ToList().Sum();
                        var list = new List<VoxelMaterialAssetModel>();

                        foreach (var kvp in details)
                        {
                            list.Add(new VoxelMaterialAssetModel() { OreName = kvp.Key, Volume = (double)kvp.Value / 255, Percent = (double)kvp.Value / (double)sum });
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
                MyVoxelMap.GetPreview(filename, out this._size, out this._contentSize, out this._voxCells);
                this.RaisePropertyChanged(() => Size);
                this.RaisePropertyChanged(() => ContentSize);
                this.RaisePropertyChanged(() => VoxCells);
            }
        }

        #endregion
    }
}
