namespace SEToolbox.Models.Asteroids
{
    public class AsteroidByteFillProperties : BaseModel, IMyVoxelFillProperties
    {
        #region fields

        private int _index;
        private int _totalPercent;
        private GenerateVoxelDetailModel _voxelFile;
        private MaterialSelectionModel _mainMaterial;
        private MaterialSelectionModel _secondMaterial;
        private int _secondPercent;
        private MaterialSelectionModel _thirdMaterial;
        private int _thirdPercent;
        private MaterialSelectionModel _fourthMaterial;
        private int _fourthPercent;
        private MaterialSelectionModel _fifthMaterial;
        private int _fifthPercent;
        private MaterialSelectionModel _sixthMaterial;
        private int _sixthPercent;
        private MaterialSelectionModel _seventhMaterial;
        private int _seventhPercent;

        #endregion

        #region properties

        public int Index
        {
            get { return _index; }

            set
            {
                if (value != _index)
                {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }

        public int TotalPercent
        {
            get { return _totalPercent; }

            set
            {
                if (value != _totalPercent)
                {
                    _totalPercent = value;
                    OnPropertyChanged(nameof(TotalPercent));
                }
            }
        }

        public GenerateVoxelDetailModel VoxelFile
        {
            get { return _voxelFile; }

            set
            {
                if (value != _voxelFile)
                {
                    _voxelFile = value;
                    OnPropertyChanged(nameof(VoxelFile));
                }
            }
        }

        public MaterialSelectionModel MainMaterial
        {
            get { return _mainMaterial; }

            set
            {
                if (value != _mainMaterial)
                {
                    _mainMaterial = value;
                    OnPropertyChanged(nameof(MainMaterial));
                }
            }
        }

        public MaterialSelectionModel SecondMaterial
        {
            get { return _secondMaterial; }

            set
            {
                if (value != _secondMaterial)
                {
                    _secondMaterial = value;
                    OnPropertyChanged(nameof(SecondMaterial));
                }
            }
        }

        public int SecondPercent
        {
            get { return _secondPercent; }

            set
            {
                if (value != _secondPercent)
                {
                    _secondPercent = value;
                    OnPropertyChanged(nameof(SecondPercent));
                    UpdateTotal();
                }
            }
        }

        public MaterialSelectionModel ThirdMaterial
        {
            get { return _thirdMaterial; }

            set
            {
                if (value != _thirdMaterial)
                {
                    _thirdMaterial = value;
                    OnPropertyChanged(nameof(ThirdMaterial));
                }
            }
        }

        public int ThirdPercent
        {
            get { return _thirdPercent; }

            set
            {
                if (value != _thirdPercent)
                {
                    _thirdPercent = value;
                    OnPropertyChanged(nameof(ThirdPercent));
                    UpdateTotal();
                }
            }
        }

        public MaterialSelectionModel FourthMaterial
        {
            get { return _fourthMaterial; }

            set
            {
                if (value != _fourthMaterial)
                {
                    _fourthMaterial = value;
                    OnPropertyChanged(nameof(FourthMaterial));
                }
            }
        }

        public int FourthPercent
        {
            get { return _fourthPercent; }

            set
            {
                if (value != _fourthPercent)
                {
                    _fourthPercent = value;
                    OnPropertyChanged(nameof(FourthPercent));
                    UpdateTotal();
                }
            }
        }

        public MaterialSelectionModel FifthMaterial
        {
            get { return _fifthMaterial; }

            set
            {
                if (value != _fifthMaterial)
                {
                    _fifthMaterial = value;
                    OnPropertyChanged(nameof(FifthMaterial));
                }
            }
        }

        public int FifthPercent
        {
            get { return _fifthPercent; }

            set
            {
                if (value != _fifthPercent)
                {
                    _fifthPercent = value;
                    OnPropertyChanged(nameof(FifthPercent));
                    UpdateTotal();
                }
            }
        }

        public MaterialSelectionModel SixthMaterial
        {
            get { return _sixthMaterial; }

            set
            {
                if (value != _sixthMaterial)
                {
                    _sixthMaterial = value;
                    OnPropertyChanged(nameof(SixthMaterial));
                }
            }
        }

        public int SixthPercent
        {
            get { return _sixthPercent; }

            set
            {
                if (value != _sixthPercent)
                {
                    _sixthPercent = value;
                    OnPropertyChanged(nameof(SixthPercent));
                    UpdateTotal();
                }
            }
        }

        public MaterialSelectionModel SeventhMaterial
        {
            get { return _seventhMaterial; }

            set
            {
                if (value != _seventhMaterial)
                {
                    _seventhMaterial = value;
                    OnPropertyChanged(nameof(SeventhMaterial));
                }
            }
        }

        public int SeventhPercent
        {
            get { return _seventhPercent; }

            set
            {
                if (value != _seventhPercent)
                {
                    _seventhPercent = value;
                    OnPropertyChanged(nameof(SeventhPercent));
                    UpdateTotal();
                }
            }
        }

        #endregion

        public IMyVoxelFillProperties Clone()
        {
            return new AsteroidByteFillProperties
            {
                Index = Index,
                TotalPercent = TotalPercent,
                VoxelFile = VoxelFile,
                MainMaterial = MainMaterial,
                SecondMaterial = SecondMaterial,
                SecondPercent = SecondPercent,
                ThirdMaterial = ThirdMaterial,
                ThirdPercent = ThirdPercent,
                FourthMaterial = FourthMaterial,
                FourthPercent = FourthPercent,
                FifthMaterial = FifthMaterial,
                FifthPercent = FifthPercent,
                SixthMaterial = SixthMaterial,
                SixthPercent = SixthPercent,
                SeventhMaterial = SeventhMaterial,
                SeventhPercent = SeventhPercent,
            };
        }

        private void UpdateTotal()
        {
            TotalPercent = SecondPercent + ThirdPercent + FourthPercent + FifthPercent + SixthPercent + SeventhPercent;
        }
    }
}
