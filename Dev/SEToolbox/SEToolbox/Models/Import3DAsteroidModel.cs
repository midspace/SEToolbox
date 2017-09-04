namespace SEToolbox.Models
{
    using System.Collections.ObjectModel;
    using System.Windows.Media.Media3D;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using VRage;

    public class Import3DAsteroidModel : BaseModel
    {
        #region Fields

        private string _filename;
        private Model3D _model;
        private bool _isValidModel;
        private bool _isValidEntity;

        private BindableSize3DModel _originalModelSize;
        private BindableSize3DIModel _newModelSize;
        private BindablePoint3DModel _newModelScale;
        private BindablePoint3DModel _position;
        private BindableVector3DModel _forward;
        private BindableVector3DModel _up;
        private MyPositionAndOrientation _characterPosition;
        private TraceType _traceType;
        private TraceCount _traceCount;
        private TraceDirection _traceDirection;
        private double _multipleScale;
        private double _maxLengthScale;
        private double _buildDistance;
        private bool _isMultipleScale;
        private bool _isMaxLengthScale;
        private bool _isAbsolutePosition;
        private bool _isInfrontofPlayer;
        private readonly ObservableCollection<MaterialSelectionModel> _outsideMaterialsCollection;
        private readonly ObservableCollection<MaterialSelectionModel> _insideMaterialsCollection;
        private MaterialSelectionModel _outsideStockMaterial;
        private MaterialSelectionModel _insideStockMaterial;
        private int _outsideMaterialDepth;
        private string _sourceFile;
        private double _rotateYaw;
        private double _rotatePitch;
        private double _rotateRoll;
        private bool _beepWhenFinished;
        private bool _saveWhenFinsihed;
        private bool _shutdownWhenFinished;
        private bool _runInLowPrioity;
        // TODO: pause

        #endregion

        #region ctor

        public Import3DAsteroidModel()
        {
            _outsideMaterialsCollection = new ObservableCollection<MaterialSelectionModel>();
            _insideMaterialsCollection = new ObservableCollection<MaterialSelectionModel>();

            foreach (var material in SpaceEngineersCore.Resources.VoxelMaterialDefinitions)
            {
                _outsideMaterialsCollection.Add(new MaterialSelectionModel { Value = material.Id.SubtypeName, DisplayName = material.Id.SubtypeName });
                _insideMaterialsCollection.Add(new MaterialSelectionModel { Value = material.Id.SubtypeName, DisplayName = material.Id.SubtypeName });
            }

            InsideStockMaterial = InsideMaterialsCollection[0];
            OutsideStockMaterial = OutsideMaterialsCollection[0];

            TraceType = TraceType.Odd;
            TraceCount = TraceCount.Trace5;
            TraceDirection = TraceDirection.X;

            BeepWhenFinished = true;
            RunInLowPrioity = true;
        }

        #endregion

        #region Properties

        public string Filename
        {
            get { return _filename; }

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
            get { return _model; }

            set
            {
                if (value != _model)
                {
                    _model = value;
                    RaisePropertyChanged(() => Model);
                }
            }
        }

        /// <summary>
        /// Indicates if the selected model file the user has specified is a valid model.
        /// </summary>
        public bool IsValidModel
        {
            get { return _isValidModel; }

            set
            {
                if (value != _isValidModel)
                {
                    _isValidModel = value;
                    RaisePropertyChanged(() => IsValidModel);
                }
            }
        }

        /// <summary>
        /// Indicates if the Entity created at the end of processing is valid.
        /// </summary>
        public bool IsValidEntity
        {
            get { return _isValidEntity; }

            set
            {
                if (value != _isValidEntity)
                {
                    _isValidEntity = value;
                    RaisePropertyChanged(() => IsValidEntity);
                }
            }
        }

        public BindableSize3DModel OriginalModelSize
        {
            get { return _originalModelSize; }

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
            get { return _newModelSize; }

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
            get { return _newModelScale; }

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
            get { return _position; }

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
            get { return _forward; }

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
            get { return _up; }

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
            get { return _characterPosition; }

            set
            {
                //if (value != characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                _characterPosition = value;
                RaisePropertyChanged(() => CharacterPosition);
            }
        }

        public TraceType TraceType
        {
            get { return _traceType; }

            set
            {
                if (value != _traceType)
                {
                    _traceType = value;
                    RaisePropertyChanged(() => TraceType);
                }
            }
        }

        public TraceCount TraceCount
        {
            get { return _traceCount; }

            set
            {
                if (value != _traceCount)
                {
                    _traceCount = value;
                    RaisePropertyChanged(() => TraceCount);
                }
            }
        }

        public TraceDirection TraceDirection
        {
            get { return _traceDirection; }

            set
            {
                if (value != _traceDirection)
                {
                    _traceDirection = value;
                    RaisePropertyChanged(() => TraceDirection);
                }
            }
        }

        public double MultipleScale
        {
            get { return _multipleScale; }

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
            get { return _maxLengthScale; }

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
            get { return _buildDistance; }

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
            get { return _isMultipleScale; }

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
            get { return _isMaxLengthScale; }

            set
            {
                if (value != _isMaxLengthScale)
                {
                    _isMaxLengthScale = value;
                    RaisePropertyChanged(() => IsMaxLengthScale);
                }
            }
        }

        public bool IsAbsolutePosition
        {
            get { return _isAbsolutePosition; }

            set
            {
                if (value != _isAbsolutePosition)
                {
                    _isAbsolutePosition = value;
                    RaisePropertyChanged(() => IsAbsolutePosition);
                }
            }
        }

        public bool IsInfrontofPlayer
        {
            get { return _isInfrontofPlayer; }

            set
            {
                if (value != _isInfrontofPlayer)
                {
                    _isInfrontofPlayer = value;
                    RaisePropertyChanged(() => IsInfrontofPlayer);
                }
            }
        }

        public ObservableCollection<MaterialSelectionModel> OutsideMaterialsCollection
        {
            get { return _outsideMaterialsCollection; }
        }

        public int OutsideMaterialDepth
        {
            get { return _outsideMaterialDepth; }

            set
            {
                if (value != _outsideMaterialDepth)
                {
                    _outsideMaterialDepth = value;
                    RaisePropertyChanged(() => OutsideMaterialDepth);
                }
            }
        }

        public ObservableCollection<MaterialSelectionModel> InsideMaterialsCollection
        {
            get { return _insideMaterialsCollection; }
        }

        public MaterialSelectionModel OutsideStockMaterial
        {
            get { return _outsideStockMaterial; }

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
            get { return _insideStockMaterial; }

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
            get { return _sourceFile; }

            set
            {
                if (value != _sourceFile)
                {
                    _sourceFile = value;
                    RaisePropertyChanged(() => SourceFile);
                }
            }
        }

        public double RotateYaw
        {
            get { return _rotateYaw; }

            set
            {
                if (value != _rotateYaw)
                {
                    _rotateYaw = value;
                    RaisePropertyChanged(() => RotateYaw);
                }
            }
        }

        public double RotatePitch
        {
            get { return _rotatePitch; }

            set
            {
                if (value != _rotatePitch)
                {
                    _rotatePitch = value;
                    RaisePropertyChanged(() => RotatePitch);
                }
            }
        }

        public double RotateRoll
        {
            get { return _rotateRoll; }

            set
            {
                if (value != _rotateRoll)
                {
                    _rotateRoll = value;
                    RaisePropertyChanged(() => RotateRoll);
                }
            }
        }

        public bool BeepWhenFinished
        {
            get { return _beepWhenFinished; }

            set
            {
                if (value != _beepWhenFinished)
                {
                    _beepWhenFinished = value;
                    RaisePropertyChanged(() => BeepWhenFinished);
                }
            }
        }

        public bool SaveWhenFinsihed
        {
            get { return _saveWhenFinsihed; }

            set
            {
                if (value != _saveWhenFinsihed)
                {
                    _saveWhenFinsihed = value;
                    RaisePropertyChanged(() => SaveWhenFinsihed);
                }
            }
        }

        public bool ShutdownWhenFinished
        {
            get { return _shutdownWhenFinished; }

            set
            {
                if (value != _shutdownWhenFinished)
                {
                    _shutdownWhenFinished = value;
                    RaisePropertyChanged(() => ShutdownWhenFinished);
                }
            }
        }

        public bool RunInLowPrioity
        {
            get { return _runInLowPrioity; }

            set
            {
                if (value != _runInLowPrioity)
                {
                    _runInLowPrioity = value;
                    RaisePropertyChanged(() => RunInLowPrioity);
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
