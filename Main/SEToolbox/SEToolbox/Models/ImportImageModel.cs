namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using System.Drawing;

    public class ImportImageModel : BaseModel
    {
        #region Fields

        private string filename;
        private bool isValidImage;

        private Size originalImageSize;
        private BindableSizeModel newImageSize;
        private BindablePoint3DModel position;
        private BindableVector3DModel forward;
        private BindableVector3DModel up;
        private ImportClassType classType;
        private ImportArmorType armorType;
        private MyPositionAndOrientation characterPosition;

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
                return this.filename;
            }

            set
            {
                if (value != this.filename)
                {
                    this.filename = value;
                    this.RaisePropertyChanged(() => Filename);
                }
            }
        }

        public bool IsValidImage
        {
            get
            {
                return this.isValidImage;
            }

            set
            {
                if (value != this.isValidImage)
                {
                    this.isValidImage = value;
                    this.RaisePropertyChanged(() => IsValidImage);
                }
            }
        }

        public Size OriginalImageSize
        {
            get
            {
                return this.originalImageSize;
            }

            set
            {
                if (value != this.originalImageSize)
                {
                    this.originalImageSize = value;
                    this.RaisePropertyChanged(() => OriginalImageSize);
                }
            }
        }

        public BindableSizeModel NewImageSize
        {
            get
            {
                return this.newImageSize;
            }

            set
            {
                if (value != this.newImageSize)
                {
                    this.newImageSize = value;
                    this.RaisePropertyChanged(() => NewImageSize);
                }
            }
        }

        public BindablePoint3DModel Position
        {
            get
            {
                return this.position;
            }

            set
            {
                if (value != this.position)
                {
                    this.position = value;
                    this.RaisePropertyChanged(() => Position);
                }
            }
        }

        public BindableVector3DModel Forward
        {
            get
            {
                return this.forward;
            }

            set
            {
                if (value != this.forward)
                {
                    this.forward = value;
                    this.RaisePropertyChanged(() => Forward);
                }
            }
        }

        public BindableVector3DModel Up
        {
            get
            {
                return this.up;
            }

            set
            {
                if (value != this.up)
                {
                    this.up = value;
                    this.RaisePropertyChanged(() => Up);
                }
            }
        }

        public ImportClassType ClassType
        {
            get
            {
                return this.classType;
            }

            set
            {
                if (value != this.classType)
                {
                    this.classType = value;
                    this.RaisePropertyChanged(() => ClassType);
                }
            }
        }

        public ImportArmorType ArmorType
        {
            get
            {
                return this.armorType;
            }

            set
            {
                if (value != this.armorType)
                {
                    this.armorType = value;
                    this.RaisePropertyChanged(() => ArmorType);
                }
            }
        }


        public MyPositionAndOrientation CharacterPosition
        {
            get
            {
                return this.characterPosition;
            }

            set
            {
                //if (value != this.characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                this.characterPosition = value;
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
