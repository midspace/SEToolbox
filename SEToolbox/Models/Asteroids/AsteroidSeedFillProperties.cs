namespace SEToolbox.Models.Asteroids
{
    public class AsteroidSeedFillProperties : BaseModel, IMyVoxelFillProperties
    {
        #region Fields

        private int _index;
        private GenerateVoxelDetailModel _voxelFile;
        private MaterialSelectionModel _mainMaterial, _firstMaterial, _secondMaterial, _thirdMaterial, _fourthMaterial, _fifthMaterial, _sixthMaterial, _seventhMaterial;
        private int _firstVeins, _secondVeins, _thirdVeins, _fourthVeins, _fifthVeins, _sixthVeins, _seventhVeins;
        private int _firstRadius, _secondRadius, _thirdRadius, _fourthRadius, _fifthRadius, _sixthRadius, _seventhRadius;

        #endregion

        #region Properties

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

        public MaterialSelectionModel FirstMaterial
        {
            get { return _firstMaterial; }

            set
            {
                if (value != _firstMaterial)
                {
                    _firstMaterial = value;
                    OnPropertyChanged(nameof(FirstMaterial));
                }
            }
        }


        public int FirstVeins
        {
            get { return _firstVeins; }
            set { _firstVeins = value; }
        }

        public int FirstRadius
        {
            get { return _firstRadius; }
            set { _firstRadius = value; }
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

        public int SecondVeins
        {
            get { return _secondVeins; }
            set { _secondVeins = value; }
        }

        public int SecondRadius
        {
            get { return _secondRadius; }
            set { _secondRadius = value; }
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

        public int ThirdVeins
        {
            get { return _thirdVeins; }
            set { _thirdVeins = value; }
        }

        public int ThirdRadius
        {
            get { return _thirdRadius; }
            set { _thirdRadius = value; }
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

        public int FourthVeins
        {
            get { return _fourthVeins; }
            set { _fourthVeins = value; }
        }

        public int FourthRadius
        {
            get { return _fourthRadius; }
            set { _fourthRadius = value; }
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

        public int FifthVeins
        {
            get { return _fifthVeins; }
            set { _fifthVeins = value; }
        }

        public int FifthRadius
        {
            get { return _fifthRadius; }
            set { _fifthRadius = value; }
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

        public int SixthVeins
        {
            get { return _sixthVeins; }
            set { _sixthVeins = value; }
        }

        public int SixthRadius
        {
            get { return _sixthRadius; }
            set { _sixthRadius = value; }
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


        public int SeventhVeins
        {
            get { return _seventhVeins; }
            set { _seventhVeins = value; }
        }

        public int SeventhRadius
        {
            get { return _seventhRadius; }
            set { _seventhRadius = value; }
        }

        #endregion

        public IMyVoxelFillProperties Clone()
        {
            return new AsteroidSeedFillProperties
            {
                Index = Index,
                VoxelFile = VoxelFile,
                MainMaterial = MainMaterial,
                FirstMaterial = FirstMaterial,
                FirstVeins = FirstVeins,
                FirstRadius = FirstRadius,
                SecondMaterial = SecondMaterial,
                SecondVeins = SecondVeins,
                SecondRadius = SecondRadius,
                ThirdMaterial = ThirdMaterial,
                ThirdVeins = ThirdVeins,
                ThirdRadius = ThirdRadius,
                FourthMaterial = FourthMaterial,
                FourthVeins = FourthVeins,
                FourthRadius = FourthRadius,
                FifthMaterial = FifthMaterial,
                FifthVeins = FifthVeins,
                FifthRadius = FifthRadius,
                SixthMaterial = SixthMaterial,
                SixthVeins = SixthVeins,
                SixthRadius = SixthRadius,
                SeventhMaterial = SeventhMaterial,
                SeventhVeins = SeventhVeins,
                SeventhRadius = SeventhRadius,
            };
        }
    }
}
