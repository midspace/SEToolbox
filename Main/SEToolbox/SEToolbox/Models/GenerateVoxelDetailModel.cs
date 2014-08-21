﻿namespace SEToolbox.Models
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
                return _name;
            }

            set
            {
                if (value != _name)
                {
                    _name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        public string SourceFilename
        {
            get
            {
                return _sourceFilename;
            }

            set
            {
                if (value != _sourceFilename)
                {
                    _sourceFilename = value;
                    RaisePropertyChanged(() => SourceFilename);
                }
            }
        }

        public string VoxelFilename
        {
            get
            {
                return _voxelFilename;
            }

            set
            {
                if (value != _voxelFilename)
                {
                    _voxelFilename = value;
                    RaisePropertyChanged(() => VoxelFilename);
                }
            }
        }

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

        #endregion
    }
}
