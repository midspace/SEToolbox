namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;

    public class ImportVoxelModel : BaseModel
    {
        #region Fields

        private string filename;
        private string sourceFilename;
        private bool isValidVoxel;
        private BindablePoint3DModel position;
        private BindableVector3DModel forward;
        private BindableVector3DModel up;
        private MyPositionAndOrientation characterPosition;

        #endregion

        #region ctor

        public ImportVoxelModel()
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

        public string SourceFilename
        {
            get
            {
                return this.sourceFilename;
            }

            set
            {
                if (value != this.sourceFilename)
                {
                    this.sourceFilename = value;
                    this.RaisePropertyChanged(() => SourceFilename);
                }
            }
        }

        public bool IsValidVoxel
        {
            get
            {
                return this.isValidVoxel;
            }

            set
            {
                if (value != this.isValidVoxel)
                {
                    this.isValidVoxel = value;
                    this.RaisePropertyChanged(() => IsValidVoxel);
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
