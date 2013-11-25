namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;

    public class Import3dModelModel : BaseModel
    {
        #region Fields

        private string filename;
        private bool isValidModel;

        private BindableSize3DModel originalModelSize;
        private BindableSize3DModel newModelSize;
        private BindablePoint3DModel position;
        private BindableVector3DModel forward;
        private BindableVector3DModel up;
        private ImportClassType classType;
        private ImportArmorType armorType;
        private MyPositionAndOrientation characterPosition;
        private double multipleScale;
        private double maxLengthScale;
        private double buildDistance;
        private bool isMultipleScale;
        private bool isMaxLengthScale;

        #endregion

        #region ctor

        public Import3dModelModel()
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

        public bool IsValidModel
        {
            get
            {
                return this.isValidModel;
            }

            set
            {
                if (value != this.isValidModel)
                {
                    this.isValidModel = value;
                    this.RaisePropertyChanged(() => IsValidModel);
                }
            }
        }

        public BindableSize3DModel OriginalModelSize
        {
            get
            {
                return this.originalModelSize;
            }

            set
            {
                if (value != this.originalModelSize)
                {
                    this.originalModelSize = value;
                    this.RaisePropertyChanged(() => OriginalModelSize);
                }
            }
        }

        public BindableSize3DModel NewModelSize
        {
            get
            {
                return this.newModelSize;
            }

            set
            {
                if (value != this.newModelSize)
                {
                    this.newModelSize = value;
                    this.RaisePropertyChanged(() => NewModelSize);
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

        public double MultipleScale
        {
            get
            {
                return this.multipleScale;
            }

            set
            {
                if (value != this.multipleScale)
                {
                    this.multipleScale = value;
                    this.RaisePropertyChanged(() => MultipleScale);
                }
            }
        }

        public double MaxLengthScale
        {
            get
            {
                return this.maxLengthScale;
            }

            set
            {
                if (value != this.maxLengthScale)
                {
                    this.maxLengthScale = value;
                    this.RaisePropertyChanged(() => MaxLengthScale);
                }
            }
        }

        public double BuildDistance
        {
            get
            {
                return this.buildDistance;
            }

            set
            {
                if (value != this.buildDistance)
                {
                    this.buildDistance = value;
                    this.RaisePropertyChanged(() => BuildDistance);
                }
            }
        }

        public bool IsMultipleScale
        {
            get
            {
                return this.isMultipleScale;
            }

            set
            {
                if (value != this.isMultipleScale)
                {
                    this.isMultipleScale = value;
                    this.RaisePropertyChanged(() => IsMultipleScale);
                }
            }
        }

        public bool IsMaxLengthScale
        {
            get
            {
                return this.isMaxLengthScale;
            }

            set
            {
                if (value != this.isMaxLengthScale)
                {
                    this.isMaxLengthScale = value;
                    this.RaisePropertyChanged(() => IsMaxLengthScale);
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            this.CharacterPosition = characterPosition;
        }

        #endregion
    }
}
