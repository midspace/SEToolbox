namespace SEToolbox.Models
{
    public class GenerateVoxelModel : BaseModel
    {
        #region Fields

        private int _index;
        private GenerateVoxelDetailModel _voxelFile;
        private MaterialSelectionModel _mainMaterial;
        private MaterialSelectionModel _secondMaterial;
        private int _secondPercent;
        private MaterialSelectionModel _thirdMaterial;
        private int _thirdPercent;
        private MaterialSelectionModel _forthMaterial;
        private int _forthPercent;
        private MaterialSelectionModel _fifthMaterial;
        private int _fifthPercent;

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

        public int SecondPercent
        {
            get
            {
                return this._secondPercent;
            }

            set
            {
                if (value != this._secondPercent)
                {
                    this._secondPercent = value;
                    this.RaisePropertyChanged(() => SecondPercent);
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

        public int ThirdPercent
        {
            get
            {
                return this._thirdPercent;
            }

            set
            {
                if (value != this._thirdPercent)
                {
                    this._thirdPercent = value;
                    this.RaisePropertyChanged(() => ThirdPercent);
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

        public int ForthPercent
        {
            get
            {
                return this._forthPercent;
            }

            set
            {
                if (value != this._forthPercent)
                {
                    this._forthPercent = value;
                    this.RaisePropertyChanged(() => ForthPercent);
                }
            }
        }

        public MaterialSelectionModel FifthMaterial
        {
            get
            {
                return this._fifthMaterial;
            }

            set
            {
                if (value != this._fifthMaterial)
                {
                    this._fifthMaterial = value;
                    this.RaisePropertyChanged(() => FifthMaterial);
                }
            }
        }

        public int FifthPercent
        {
            get
            {
                return this._fifthPercent;
            }

            set
            {
                if (value != this._fifthPercent)
                {
                    this._fifthPercent = value;
                    this.RaisePropertyChanged(() => FifthPercent);
                }
            }
        }

        #endregion

        public GenerateVoxelModel Clone()
        {
            return new GenerateVoxelModel()
            {
                Index = this.Index,
                VoxelFile = this.VoxelFile,
                MainMaterial = this.MainMaterial,
                SecondMaterial = this.SecondMaterial,
                SecondPercent = this.SecondPercent,
                ThirdMaterial = this.ThirdMaterial,
                ThirdPercent = this.ThirdPercent,
                ForthMaterial = this.ForthMaterial,
                ForthPercent = this.ForthPercent,
                FifthMaterial = this.FifthMaterial,
                FifthPercent = this.FifthPercent
            };
        }
    }
}
