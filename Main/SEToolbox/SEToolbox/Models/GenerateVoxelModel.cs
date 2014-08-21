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
        private MaterialSelectionModel _sixthMaterial;
        private int _sixthPercent;
        private MaterialSelectionModel _seventhMaterial;
        private int _seventhPercent;

        #endregion

        #region Properties

        public int Index
        {
            get
            {
                return _index;
            }

            set
            {
                if (value != _index)
                {
                    _index = value;
                    RaisePropertyChanged(() => Index);
                }
            }
        }

        public GenerateVoxelDetailModel VoxelFile
        {
            get
            {
                return _voxelFile;
            }

            set
            {
                if (value != _voxelFile)
                {
                    _voxelFile = value;
                    RaisePropertyChanged(() => VoxelFile);
                }
            }
        }

        public MaterialSelectionModel MainMaterial
        {
            get
            {
                return _mainMaterial;
            }

            set
            {
                if (value != _mainMaterial)
                {
                    _mainMaterial = value;
                    RaisePropertyChanged(() => MainMaterial);
                }
            }
        }

        public MaterialSelectionModel SecondMaterial
        {
            get
            {
                return _secondMaterial;
            }

            set
            {
                if (value != _secondMaterial)
                {
                    _secondMaterial = value;
                    RaisePropertyChanged(() => SecondMaterial);
                }
            }
        }

        public int SecondPercent
        {
            get
            {
                return _secondPercent;
            }

            set
            {
                if (value != _secondPercent)
                {
                    _secondPercent = value;
                    RaisePropertyChanged(() => SecondPercent);
                }
            }
        }

        public MaterialSelectionModel ThirdMaterial
        {
            get
            {
                return _thirdMaterial;
            }

            set
            {
                if (value != _thirdMaterial)
                {
                    _thirdMaterial = value;
                    RaisePropertyChanged(() => ThirdMaterial);
                }
            }
        }

        public int ThirdPercent
        {
            get
            {
                return _thirdPercent;
            }

            set
            {
                if (value != _thirdPercent)
                {
                    _thirdPercent = value;
                    RaisePropertyChanged(() => ThirdPercent);
                }
            }
        }

        public MaterialSelectionModel ForthMaterial
        {
            get
            {
                return _forthMaterial;
            }

            set
            {
                if (value != _forthMaterial)
                {
                    _forthMaterial = value;
                    RaisePropertyChanged(() => ForthMaterial);
                }
            }
        }

        public int ForthPercent
        {
            get
            {
                return _forthPercent;
            }

            set
            {
                if (value != _forthPercent)
                {
                    _forthPercent = value;
                    RaisePropertyChanged(() => ForthPercent);
                }
            }
        }

        public MaterialSelectionModel FifthMaterial
        {
            get
            {
                return _fifthMaterial;
            }

            set
            {
                if (value != _fifthMaterial)
                {
                    _fifthMaterial = value;
                    RaisePropertyChanged(() => FifthMaterial);
                }
            }
        }

        public int FifthPercent
        {
            get
            {
                return _fifthPercent;
            }

            set
            {
                if (value != _fifthPercent)
                {
                    _fifthPercent = value;
                    RaisePropertyChanged(() => FifthPercent);
                }
            }
        }

        public MaterialSelectionModel SixthMaterial
        {
            get
            {
                return _sixthMaterial;
            }

            set
            {
                if (value != _sixthMaterial)
                {
                    _sixthMaterial = value;
                    RaisePropertyChanged(() => SixthMaterial);
                }
            }
        }

        public int SixthPercent
        {
            get
            {
                return _sixthPercent;
            }

            set
            {
                if (value != _sixthPercent)
                {
                    _sixthPercent = value;
                    RaisePropertyChanged(() => SixthPercent);
                }
            }
        }

        public MaterialSelectionModel SeventhMaterial
        {
            get
            {
                return _seventhMaterial;
            }

            set
            {
                if (value != _seventhMaterial)
                {
                    _seventhMaterial = value;
                    RaisePropertyChanged(() => SeventhMaterial);
                }
            }
        }

        public int SeventhPercent
        {
            get
            {
                return _seventhPercent;
            }

            set
            {
                if (value != _seventhPercent)
                {
                    _seventhPercent = value;
                    RaisePropertyChanged(() => SeventhPercent);
                }
            }
        }

        #endregion

        public GenerateVoxelModel Clone()
        {
            return new GenerateVoxelModel
            {
                Index = Index,
                VoxelFile = VoxelFile,
                MainMaterial = MainMaterial,
                SecondMaterial = SecondMaterial,
                SecondPercent = SecondPercent,
                ThirdMaterial = ThirdMaterial,
                ThirdPercent = ThirdPercent,
                ForthMaterial = ForthMaterial,
                ForthPercent = ForthPercent,
                FifthMaterial = FifthMaterial,
                FifthPercent = FifthPercent,
                SixthMaterial = SixthMaterial,
                SixthPercent = SixthPercent,
                SeventhMaterial = SeventhMaterial,
                SeventhPercent = SeventhPercent,
            };
        }
    }
}
