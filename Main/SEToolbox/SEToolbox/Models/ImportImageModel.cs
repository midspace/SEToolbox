namespace SEToolbox.Models
{
    using System.Drawing;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;

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

        #endregion
        
        #region Properties

        public string Filename
        {
            get
            {
                return _filename;
            }

            set
            {
                if (value != _filename)
                {
                    _filename = value;
                    RaisePropertyChanged(() => Filename);
                }
            }
        }

        public bool IsValidImage
        {
            get
            {
                return _isValidImage;
            }

            set
            {
                if (value != _isValidImage)
                {
                    _isValidImage = value;
                    RaisePropertyChanged(() => IsValidImage);
                }
            }
        }

        public Size OriginalImageSize
        {
            get
            {
                return _originalImageSize;
            }

            set
            {
                if (value != _originalImageSize)
                {
                    _originalImageSize = value;
                    RaisePropertyChanged(() => OriginalImageSize);
                }
            }
        }

        public BindableSizeModel NewImageSize
        {
            get
            {
                return _newImageSize;
            }

            set
            {
                if (value != _newImageSize)
                {
                    _newImageSize = value;
                    RaisePropertyChanged(() => NewImageSize);
                }
            }
        }

        public BindablePoint3DModel Position
        {
            get
            {
                return _position;
            }

            set
            {
                if (value != _position)
                {
                    _position = value;
                    RaisePropertyChanged(() => Position);
                }
            }
        }

        public BindableVector3DModel Forward
        {
            get
            {
                return _forward;
            }

            set
            {
                if (value != _forward)
                {
                    _forward = value;
                    RaisePropertyChanged(() => Forward);
                }
            }
        }

        public BindableVector3DModel Up
        {
            get
            {
                return _up;
            }

            set
            {
                if (value != _up)
                {
                    _up = value;
                    RaisePropertyChanged(() => Up);
                }
            }
        }

        public ImportImageClassType ClassType
        {
            get
            {
                return _classType;
            }

            set
            {
                if (value != _classType)
                {
                    _classType = value;
                    RaisePropertyChanged(() => ClassType);
                }
            }
        }

        public ImportArmorType ArmorType
        {
            get
            {
                return _armorType;
            }

            set
            {
                if (value != _armorType)
                {
                    _armorType = value;
                    RaisePropertyChanged(() => ArmorType);
                }
            }
        }


        public MyPositionAndOrientation CharacterPosition
        {
            get
            {
                return _characterPosition;
            }

            set
            {
                //if (value != characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                _characterPosition = value;
                RaisePropertyChanged(() => CharacterPosition);
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
