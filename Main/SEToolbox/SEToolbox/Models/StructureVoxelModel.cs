namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using VRageMath;

    [Serializable]
    public class StructureVoxelModel : StructureBaseModel
    {
        private string _sourceVoxelFilepath;
        private string _voxelFilepath;
        private Vector3I _size;
        private Vector3I _contentSize;

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

        public override void UpdateFromEntityBase()
        {
            this.ClassType = ClassType.Voxel;
            this.Description = Path.GetFileNameWithoutExtension(this.VoxelMap.Filename);
        }

        private void ReadVoxelDetails(string filename)
        {
            if (filename != null && File.Exists(filename))
            {
                MyVoxelMap.GetPreview(filename, out this._size, out this._contentSize);
                this.RaisePropertyChanged(() => Size);
                this.RaisePropertyChanged(() => ContentSize);
            }
        }

        #endregion
    }
}
