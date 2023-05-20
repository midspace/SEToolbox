namespace SEToolbox.Models
{
    using System.Drawing;

    using SEToolbox.Interop;
    using VRage;

    public class ImportImageModel : BaseModel
    {
        #region Fields

        private string _filename;
        private bool _isValidImage;

        private Size _originalImageSize;
        private BindableSizeModel _newImageSize;
        private BindablePoint3DModel _position;
        private BindableVector3DModel _forward;
        private BindableVector3DModel _up;
        private ImportImageClassType _classType;
        private ImportArmorType _armorType;
        private MyPositionAndOrientation _characterPosition;
        private int _alphaLevel;
        private System.Windows.Media.Color _keyColor;
        private bool _isAlphaLevel;
        private bool _isKeyColor;

        #endregion

        public ImportImageModel()
        {
            AlphaLevel = 127;
            KeyColor = System.Windows.Media.Color.FromArgb(0, 255, 0, 255);
            IsAlphaLevel = true;
        }

        #region Properties

        public string Filename
        {
            get { return _filename; }

            set
            {
                if (value != _filename)
                {
                    _filename = value;
                    OnPropertyChanged(nameof(Filename));
                }
            }
        }

        public bool IsValidImage
        {
            get { return _isValidImage; }

            set
            {
                if (value != _isValidImage)
                {
                    _isValidImage = value;
                    OnPropertyChanged(nameof(IsValidImage));
                }
            }
        }

        public Size OriginalImageSize
        {
            get { return _originalImageSize; }

            set
            {
                if (value != _originalImageSize)
                {
                    _originalImageSize = value;
                    OnPropertyChanged(nameof(OriginalImageSize));
                }
            }
        }

        public BindableSizeModel NewImageSize
        {
            get { return _newImageSize; }

            set
            {
                if (value != _newImageSize)
                {
                    _newImageSize = value;
                    OnPropertyChanged(nameof(NewImageSize));
                }
            }
        }

        public BindablePoint3DModel Position
        {
            get { return _position; }

            set
            {
                if (value != _position)
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        public BindableVector3DModel Forward
        {
            get { return _forward; }

            set
            {
                if (value != _forward)
                {
                    _forward = value;
                    OnPropertyChanged(nameof(Forward));
                }
            }
        }

        public BindableVector3DModel Up
        {
            get { return _up; }

            set
            {
                if (value != _up)
                {
                    _up = value;
                    OnPropertyChanged(nameof(Up));
                }
            }
        }

        public ImportImageClassType ClassType
        {
            get { return _classType; }

            set
            {
                if (value != _classType)
                {
                    _classType = value;
                    OnPropertyChanged(nameof(ClassType));
                }
            }
        }

        public ImportArmorType ArmorType
        {
            get { return _armorType; }

            set
            {
                if (value != _armorType)
                {
                    _armorType = value;
                    OnPropertyChanged(nameof(ArmorType));
                }
            }
        }

        public MyPositionAndOrientation CharacterPosition
        {
            get { return _characterPosition; }

            set
            {
                //if (value != characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                _characterPosition = value;
                OnPropertyChanged(nameof(CharacterPosition));
            }
        }

        public int AlphaLevel
        {
            get { return _alphaLevel; }

            set
            {
                if (value != _alphaLevel)
                {
                    _alphaLevel = value;
                    OnPropertyChanged(nameof(AlphaLevel));
                }
            }
        }

        public System.Windows.Media.Color KeyColor
        {
            get { return _keyColor; }

            set
            {
                if (_keyColor != value)
                {
                    _keyColor = value;
                    OnPropertyChanged(nameof(KeyColor));
                }
            }
        }

        public bool IsAlphaLevel
        {
            get { return _isAlphaLevel; }

            set
            {
                if (value != _isAlphaLevel)
                {
                    _isAlphaLevel = value;
                    OnPropertyChanged(nameof(IsAlphaLevel));
                }
            }
        }

        public bool IsKeyColor
        {
            get { return _isKeyColor; }

            set
            {
                if (value != _isKeyColor)
                {
                    _isKeyColor = value;
                    OnPropertyChanged(nameof(IsKeyColor));
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            CharacterPosition = characterPosition;
        }

        #endregion

        #region helpers

        #endregion
    }
}
