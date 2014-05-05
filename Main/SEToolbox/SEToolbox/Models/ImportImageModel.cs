namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using System.Drawing;

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

        #region ctor

        public ImportImageModel()
        {
        }

        #endregion

        #region Properties

        public string Filename
        {
            get
            {
                return this._filename;
            }

            set
            {
                if (value != this._filename)
                {
                    this._filename = value;
                    this.RaisePropertyChanged(() => Filename);
                }
            }
        }

        public bool IsValidImage
        {
            get
            {
                return this._isValidImage;
            }

            set
            {
                if (value != this._isValidImage)
                {
                    this._isValidImage = value;
                    this.RaisePropertyChanged(() => IsValidImage);
                }
            }
        }

        public Size OriginalImageSize
        {
            get
            {
                return this._originalImageSize;
            }

            set
            {
                if (value != this._originalImageSize)
                {
                    this._originalImageSize = value;
                    this.RaisePropertyChanged(() => OriginalImageSize);
                }
            }
        }

        public BindableSizeModel NewImageSize
        {
            get
            {
                return this._newImageSize;
            }

            set
            {
                if (value != this._newImageSize)
                {
                    this._newImageSize = value;
                    this.RaisePropertyChanged(() => NewImageSize);
                }
            }
        }

        public BindablePoint3DModel Position
        {
            get
            {
                return this._position;
            }

            set
            {
                if (value != this._position)
                {
                    this._position = value;
                    this.RaisePropertyChanged(() => Position);
                }
            }
        }

        public BindableVector3DModel Forward
        {
            get
            {
                return this._forward;
            }

            set
            {
                if (value != this._forward)
                {
                    this._forward = value;
                    this.RaisePropertyChanged(() => Forward);
                }
            }
        }

        public BindableVector3DModel Up
        {
            get
            {
                return this._up;
            }

            set
            {
                if (value != this._up)
                {
                    this._up = value;
                    this.RaisePropertyChanged(() => Up);
                }
            }
        }

        public ImportImageClassType ClassType
        {
            get
            {
                return this._classType;
            }

            set
            {
                if (value != this._classType)
                {
                    this._classType = value;
                    this.RaisePropertyChanged(() => ClassType);
                }
            }
        }

        public ImportArmorType ArmorType
        {
            get
            {
                return this._armorType;
            }

            set
            {
                if (value != this._armorType)
                {
                    this._armorType = value;
                    this.RaisePropertyChanged(() => ArmorType);
                }
            }
        }


        public MyPositionAndOrientation CharacterPosition
        {
            get
            {
                return this._characterPosition;
            }

            set
            {
                //if (value != this.characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                this._characterPosition = value;
                this.RaisePropertyChanged(() => CharacterPosition);
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            this.CharacterPosition = characterPosition;
        }

        #endregion

        #region helpers

        #endregion
    }
}
