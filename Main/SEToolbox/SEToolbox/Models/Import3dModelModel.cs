namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System.Collections.ObjectModel;
    using System.Windows.Media.Media3D;

    public class Import3dModelModel : BaseModel
    {
        #region Fields

        private string filename;
        private Model3D model;
        private bool isValidModel;

        private BindableSize3DModel originalModelSize;
        private BindableSize3DIModel newModelSize;
        private BindablePoint3DModel newModelScale;
        private BindablePoint3DModel position;
        private BindableVector3DModel forward;
        private BindableVector3DModel up;
        private ModelTraceVoxel traceType;
        private ImportModelClassType classType;
        private ImportArmorType armorType;
        private MyPositionAndOrientation characterPosition;
        private double multipleScale;
        private double maxLengthScale;
        private double buildDistance;
        private bool isMultipleScale;
        private bool isMaxLengthScale;
        private readonly ObservableCollection<MaterialSelectionModel> _outsideMaterialsCollection;
        private readonly ObservableCollection<MaterialSelectionModel> _insideMaterialsCollection;
        private MaterialSelectionModel _outsideStockMaterial;
        private MaterialSelectionModel _insideStockMaterial;
        private string _sourceFile;

        #endregion

        #region ctor

        public Import3dModelModel()
        {
            this.TraceType = ModelTraceVoxel.ThinSmoothed;

            _outsideMaterialsCollection = new ObservableCollection<MaterialSelectionModel>();
            _insideMaterialsCollection = new ObservableCollection<MaterialSelectionModel>
            {
                new MaterialSelectionModel() {Value = null, DisplayName = "Empty"}
            };

            foreach (var material in SpaceEngineersAPI.GetMaterialList())
            {
                _outsideMaterialsCollection.Add(new MaterialSelectionModel() { Value = material.Name, DisplayName = material.Name });
                _insideMaterialsCollection.Add(new MaterialSelectionModel() { Value = material.Name, DisplayName = material.Name });
            }

            this.InsideStockMaterial = this.InsideMaterialsCollection[0];
            this.OutsideStockMaterial = this.OutsideMaterialsCollection[0];
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

        public Model3D Model
        {
            get
            {
                return this.model;
            }

            set
            {
                if (value != this.model)
                {
                    this.model = value;
                    this.RaisePropertyChanged(() => Model);
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

        public BindableSize3DIModel NewModelSize
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

        public BindablePoint3DModel NewModelScale
        {
            get
            {
                return this.newModelScale;
            }

            set
            {
                if (value != this.newModelScale)
                {
                    this.newModelScale = value;
                    this.RaisePropertyChanged(() => NewModelScale);
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

        public ModelTraceVoxel TraceType
        {
            get
            {
                return this.traceType;
            }

            set
            {
                if (value != this.traceType)
                {
                    this.traceType = value;
                    this.RaisePropertyChanged(() => TraceType);
                }
            }
        }

        public ImportModelClassType ClassType
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
                    this.RaisePropertyChanged(() => IsAsteroid);
                    this.RaisePropertyChanged(() => IsShip);
                }
            }
        }

        public bool IsAsteroid
        {
            get
            {
                return this.classType == ImportModelClassType.Asteroid;
            }
        }

        public bool IsShip
        {
            get
            {
                return this.classType != ImportModelClassType.Asteroid;
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

        public ObservableCollection<MaterialSelectionModel> OutsideMaterialsCollection
        {
            get
            {
                return this._outsideMaterialsCollection;
            }
        }


        public ObservableCollection<MaterialSelectionModel> InsideMaterialsCollection
        {
            get
            {
                return this._insideMaterialsCollection;
            }
        }

        public MaterialSelectionModel OutsideStockMaterial
        {
            get
            {
                return this._outsideStockMaterial;
            }

            set
            {
                if (value != this._outsideStockMaterial)
                {
                    this._outsideStockMaterial = value;
                    this.RaisePropertyChanged(() => OutsideStockMaterial);
                }
            }
        }

        public MaterialSelectionModel InsideStockMaterial
        {
            get
            {
                return this._insideStockMaterial;
            }

            set
            {
                if (value != this._insideStockMaterial)
                {
                    this._insideStockMaterial = value;
                    this.RaisePropertyChanged(() => InsideStockMaterial);
                }
            }
        }

        public string SourceFile
        {
            get
            {
                return this._sourceFile;
            }

            set
            {
                if (value != this._sourceFile)
                {
                    this._sourceFile = value;
                    this.RaisePropertyChanged(() => SourceFile);
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
