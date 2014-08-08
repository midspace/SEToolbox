namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System.Collections.ObjectModel;
    using System.Windows.Media.Media3D;

    public class Import3dAsteroidModel : BaseModel
    {
        #region Fields

        private string _filename;
        private Model3D _model;
        private bool _isValidModel;

        private BindableSize3DModel _originalModelSize;
        private BindableSize3DIModel _newModelSize;
        private BindablePoint3DModel _newModelScale;
        private BindablePoint3DModel _position;
        private BindableVector3DModel _forward;
        private BindableVector3DModel _up;
        private MyPositionAndOrientation _characterPosition;
        private SEToolbox.Interop.Asteroids.MyVoxelRayTracer.TraceType _traceType;
        private double _multipleScale;
        private double _maxLengthScale;
        private double _buildDistance;
        private bool _isMultipleScale;
        private bool _isMaxLengthScale;
        private readonly ObservableCollection<MaterialSelectionModel> _outsideMaterialsCollection;
        private readonly ObservableCollection<MaterialSelectionModel> _insideMaterialsCollection;
        private MaterialSelectionModel _outsideStockMaterial;
        private MaterialSelectionModel _insideStockMaterial;
        private string _sourceFile;

        #endregion

        #region ctor

        public Import3dAsteroidModel()
        {
            _outsideMaterialsCollection = new ObservableCollection<MaterialSelectionModel>();
            _insideMaterialsCollection = new ObservableCollection<MaterialSelectionModel>
            {
                new MaterialSelectionModel() {Value = null, DisplayName = "Empty"}
            };

            foreach (var material in SpaceEngineersApi.GetMaterialList())
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

        public Model3D Model
        {
            get
            {
                return this._model;
            }

            set
            {
                if (value != this._model)
                {
                    this._model = value;
                    this.RaisePropertyChanged(() => Model);
                }
            }
        }


        public bool IsValidModel
        {
            get
            {
                return this._isValidModel;
            }

            set
            {
                if (value != this._isValidModel)
                {
                    this._isValidModel = value;
                    this.RaisePropertyChanged(() => IsValidModel);
                }
            }
        }

        public BindableSize3DModel OriginalModelSize
        {
            get
            {
                return this._originalModelSize;
            }

            set
            {
                if (value != this._originalModelSize)
                {
                    this._originalModelSize = value;
                    this.RaisePropertyChanged(() => OriginalModelSize);
                }
            }
        }

        public BindableSize3DIModel NewModelSize
        {
            get
            {
                return this._newModelSize;
            }

            set
            {
                if (value != this._newModelSize)
                {
                    this._newModelSize = value;
                    this.RaisePropertyChanged(() => NewModelSize);
                }
            }
        }

        public BindablePoint3DModel NewModelScale
        {
            get
            {
                return this._newModelScale;
            }

            set
            {
                if (value != this._newModelScale)
                {
                    this._newModelScale = value;
                    this.RaisePropertyChanged(() => NewModelScale);
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

        public SEToolbox.Interop.Asteroids.MyVoxelRayTracer.TraceType TraceType
        {
            get
            {
                return this._traceType;
            }

            set
            {
                if (value != this._traceType)
                {
                    this._traceType = value;
                    this.RaisePropertyChanged(() => TraceType);
                }
            }
        }

        public double MultipleScale
        {
            get
            {
                return this._multipleScale;
            }

            set
            {
                if (value != this._multipleScale)
                {
                    this._multipleScale = value;
                    this.RaisePropertyChanged(() => MultipleScale);
                }
            }
        }

        public double MaxLengthScale
        {
            get
            {
                return this._maxLengthScale;
            }

            set
            {
                if (value != this._maxLengthScale)
                {
                    this._maxLengthScale = value;
                    this.RaisePropertyChanged(() => MaxLengthScale);
                }
            }
        }

        public double BuildDistance
        {
            get
            {
                return this._buildDistance;
            }

            set
            {
                if (value != this._buildDistance)
                {
                    this._buildDistance = value;
                    this.RaisePropertyChanged(() => BuildDistance);
                }
            }
        }

        public bool IsMultipleScale
        {
            get
            {
                return this._isMultipleScale;
            }

            set
            {
                if (value != this._isMultipleScale)
                {
                    this._isMultipleScale = value;
                    this.RaisePropertyChanged(() => IsMultipleScale);
                }
            }
        }

        public bool IsMaxLengthScale
        {
            get
            {
                return this._isMaxLengthScale;
            }

            set
            {
                if (value != this._isMaxLengthScale)
                {
                    this._isMaxLengthScale = value;
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
