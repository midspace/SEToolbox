namespace SEToolbox.Models
{
    using VRageMath;

    public class GenerateVoxelDetailModel : BaseModel
    {
        #region Fields

        private string _name;
        private string _sourceFilename;
        private string _voxelFilename;
        private Vector3I _size;

        #endregion

        #region Properties

        public string Name
        {
            get
            {
                return this._name;
            }

            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    this.RaisePropertyChanged(() => Name);
                }
            }
        }

        public string SourceFilename
        {
            get
            {
                return this._sourceFilename;
            }

            set
            {
                if (value != this._sourceFilename)
                {
                    this._sourceFilename = value;
                    this.RaisePropertyChanged(() => SourceFilename);
                }
            }
        }

        public string VoxelFilename
        {
            get
            {
                return this._voxelFilename;
            }

            set
            {
                if (value != this._voxelFilename)
                {
                    this._voxelFilename = value;
                    this.RaisePropertyChanged(() => VoxelFilename);
                }
            }
        }

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

        #endregion
    }
}
