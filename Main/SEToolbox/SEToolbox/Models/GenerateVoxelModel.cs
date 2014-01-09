namespace SEToolbox.Models
{
    public class GenerateVoxelModel : BaseModel
    {
        #region Fields

        private int _index;
        private GenerateVoxelDetailModel _voxelFile;
        private MaterialSelectionModel _mainMaterial;
        private MaterialSelectionModel _secondMaterial;
        private MaterialSelectionModel _thirdMaterial;
        private MaterialSelectionModel _forthMaterial;

        #endregion

        #region ctor

        public GenerateVoxelModel()
        {
        }

        #endregion

        #region Properties

        public int Index
        {
            get
            {
                return this._index;
            }

            set
            {
                if (value != this._index)
                {
                    this._index = value;
                    this.RaisePropertyChanged(() => Index);
                }
            }
        }

        public GenerateVoxelDetailModel VoxelFile
        {
            get
            {
                return this._voxelFile;
            }

            set
            {
                if (value != this._voxelFile)
                {
                    this._voxelFile = value;
                    this.RaisePropertyChanged(() => VoxelFile);
                }
            }
        }

        public MaterialSelectionModel MainMaterial
        {
            get
            {
                return this._mainMaterial;
            }

            set
            {
                if (value != this._mainMaterial)
                {
                    this._mainMaterial = value;
                    this.RaisePropertyChanged(() => MainMaterial);
                }
            }
        }

        public MaterialSelectionModel SecondMaterial
        {
            get
            {
                return this._secondMaterial;
            }

            set
            {
                if (value != this._secondMaterial)
                {
                    this._secondMaterial = value;
                    this.RaisePropertyChanged(() => SecondMaterial);
                }
            }
        }

        public MaterialSelectionModel ThirdMaterial
        {
            get
            {
                return this._thirdMaterial;
            }

            set
            {
                if (value != this._thirdMaterial)
                {
                    this._thirdMaterial = value;
                    this.RaisePropertyChanged(() => ThirdMaterial);
                }
            }
        }

        public MaterialSelectionModel ForthMaterial
        {
            get
            {
                return this._forthMaterial;
            }

            set
            {
                if (value != this._forthMaterial)
                {
                    this._forthMaterial = value;
                    this.RaisePropertyChanged(() => ForthMaterial);
                }
            }
        }

        #endregion
    }
}
