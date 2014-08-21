namespace SEToolbox.Models
{
    using System.Collections.ObjectModel;
    using System.Windows.Media.Media3D;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;

    public class Import3DAsteroidModel : BaseModel
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
        private MyVoxelRayTracer.TraceType _traceType;
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

        public Import3DAsteroidModel()
        {
            _outsideMaterialsCollection = new ObservableCollection<MaterialSelectionModel>();
            _insideMaterialsCollection = new ObservableCollection<MaterialSelectionModel>
            {
                new MaterialSelectionModel {Value = null, DisplayName = "Empty"}
            };

            foreach (var material in SpaceEngineersApi.GetMaterialList())
            {
                _outsideMaterialsCollection.Add(new MaterialSelectionModel { Value = material.Id.SubtypeId, DisplayName = material.Id.SubtypeId });
                _insideMaterialsCollection.Add(new MaterialSelectionModel { Value = material.Id.SubtypeId, DisplayName = material.Id.SubtypeId });
            }

            InsideStockMaterial = InsideMaterialsCollection[0];
            OutsideStockMaterial = OutsideMaterialsCollection[0];
        }

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

        public Model3D Model
        {
            get
            {
                return _model;
            }

            set
            {
                if (value != _model)
                {
                    _model = value;
                    RaisePropertyChanged(() => Model);
                }
            }
        }


        public bool IsValidModel
        {
            get
            {
                return _isValidModel;
            }

            set
            {
                if (value != _isValidModel)
                {
                    _isValidModel = value;
                    RaisePropertyChanged(() => IsValidModel);
                }
            }
        }

        public BindableSize3DModel OriginalModelSize
        {
            get
            {
                return _originalModelSize;
            }

            set
            {
                if (value != _originalModelSize)
                {
                    _originalModelSize = value;
                    RaisePropertyChanged(() => OriginalModelSize);
                }
            }
        }

        public BindableSize3DIModel NewModelSize
        {
            get
            {
                return _newModelSize;
            }

            set
            {
                if (value != _newModelSize)
                {
                    _newModelSize = value;
                    RaisePropertyChanged(() => NewModelSize);
                }
            }
        }

        public BindablePoint3DModel NewModelScale
        {
            get
            {
                return _newModelScale;
            }

            set
            {
                if (value != _newModelScale)
                {
                    _newModelScale = value;
                    RaisePropertyChanged(() => NewModelScale);
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

        public SEToolbox.Interop.Asteroids.MyVoxelRayTracer.TraceType TraceType
        {
            get
            {
                return _traceType;
            }

            set
            {
                if (value != _traceType)
                {
                    _traceType = value;
                    RaisePropertyChanged(() => TraceType);
                }
            }
        }

        public double MultipleScale
        {
            get
            {
                return _multipleScale;
            }

            set
            {
                if (value != _multipleScale)
                {
                    _multipleScale = value;
                    RaisePropertyChanged(() => MultipleScale);
                }
            }
        }

        public double MaxLengthScale
        {
            get
            {
                return _maxLengthScale;
            }

            set
            {
                if (value != _maxLengthScale)
                {
                    _maxLengthScale = value;
                    RaisePropertyChanged(() => MaxLengthScale);
                }
            }
        }

        public double BuildDistance
        {
            get
            {
                return _buildDistance;
            }

            set
            {
                if (value != _buildDistance)
                {
                    _buildDistance = value;
                    RaisePropertyChanged(() => BuildDistance);
                }
            }
        }

        public bool IsMultipleScale
        {
            get
            {
                return _isMultipleScale;
            }

            set
            {
                if (value != _isMultipleScale)
                {
                    _isMultipleScale = value;
                    RaisePropertyChanged(() => IsMultipleScale);
                }
            }
        }

        public bool IsMaxLengthScale
        {
            get
            {
                return _isMaxLengthScale;
            }

            set
            {
                if (value != _isMaxLengthScale)
                {
                    _isMaxLengthScale = value;
                    RaisePropertyChanged(() => IsMaxLengthScale);
                }
            }
        }

        public ObservableCollection<MaterialSelectionModel> OutsideMaterialsCollection
        {
            get
            {
                return _outsideMaterialsCollection;
            }
        }


        public ObservableCollection<MaterialSelectionModel> InsideMaterialsCollection
        {
            get
            {
                return _insideMaterialsCollection;
            }
        }

        public MaterialSelectionModel OutsideStockMaterial
        {
            get
            {
                return _outsideStockMaterial;
            }

            set
            {
                if (value != _outsideStockMaterial)
                {
                    _outsideStockMaterial = value;
                    RaisePropertyChanged(() => OutsideStockMaterial);
                }
            }
        }

        public MaterialSelectionModel InsideStockMaterial
        {
            get
            {
                return _insideStockMaterial;
            }

            set
            {
                if (value != _insideStockMaterial)
                {
                    _insideStockMaterial = value;
                    RaisePropertyChanged(() => InsideStockMaterial);
                }
            }
        }

        public string SourceFile
        {
            get
            {
                return _sourceFile;
            }

            set
            {
                if (value != _sourceFile)
                {
                    _sourceFile = value;
                    RaisePropertyChanged(() => SourceFile);
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
    }
}
