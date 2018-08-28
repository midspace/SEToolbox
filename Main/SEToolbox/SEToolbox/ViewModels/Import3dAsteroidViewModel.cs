namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using SEToolbox.Views;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;
    using Res = SEToolbox.Properties.Resources;

    public class Import3DAsteroidViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly Import3DAsteroidModel _dataModel;

        private bool? _closeResult;
        private bool _isBusy;
        private Rect3D _originalBounds;

        #endregion

        #region Constructors

        public Import3DAsteroidViewModel(BaseViewModel parentViewModel, Import3DAsteroidModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>)
        {
        }

        public Import3DAsteroidViewModel(BaseViewModel parentViewModel, Import3DAsteroidModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            _dialogService = dialogService;
            _openFileDialogFactory = openFileDialogFactory;
            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);

            IsMultipleScale = true;
            MultipleScale = 1;
            MaxLengthScale = 100;
            OutsideMaterialDepth = 1;
            IsInfrontofPlayer = true;
            Position = new BindablePoint3DModel();
            BuildDistance = 10;
        }

        #endregion

        #region command Properties

        public ICommand Browse3DModelCommand
        {
            get { return new DelegateCommand(Browse3DModelExecuted, Browse3DModelCanExecute); }
        }

        public ICommand CreateCommand
        {
            get { return new DelegateCommand(CreateExecuted, CreateCanExecute); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(CancelExecuted, CancelCanExecute); }
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get { return _closeResult; }

            set
            {
                _closeResult = value;
                OnPropertyChanged(nameof(CloseResult));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    if (_isBusy)
                    {
                        Application.DoEvents();
                    }
                }
            }
        }

        public string Filename
        {
            get { return _dataModel.Filename; }

            set
            {
                _dataModel.Filename = value;
                FilenameChanged();
            }
        }

        public Model3D Model
        {
            get { return _dataModel.Model; }
            set { _dataModel.Model = value; }
        }

        public bool IsValidModel
        {
            get { return _dataModel.IsValidModel; }
            set
            {
                _dataModel.IsValidModel = value;
                OnPropertyChanged(nameof(IsWrongModel));
            }
        }

        public bool IsValidEntity
        {
            get { return _dataModel.IsValidEntity; }
            set { _dataModel.IsValidEntity = value; }
        }

        public bool IsWrongModel
        {
            get { return !_dataModel.IsValidModel; }
        }

        public BindableSize3DModel OriginalModelSize
        {
            get { return _dataModel.OriginalModelSize; }
            set { _dataModel.OriginalModelSize = value; }
        }

        public BindableSize3DIModel NewModelSize
        {
            get { return _dataModel.NewModelSize; }
            set { _dataModel.NewModelSize = value; ProcessModelScale(); }
        }

        public BindablePoint3DModel NewModelScale
        {
            get { return _dataModel.NewModelScale; }
            set { _dataModel.NewModelScale = value; }
        }

        public BindablePoint3DModel Position
        {
            get { return _dataModel.Position; }
            set { _dataModel.Position = value; }
        }

        public BindableVector3DModel Forward
        {
            get { return _dataModel.Forward; }
            set { _dataModel.Forward = value; }
        }

        public BindableVector3DModel Up
        {
            get { return _dataModel.Up; }
            set { _dataModel.Up = value; }
        }

        public TraceType TraceType
        {
            get { return _dataModel.TraceType; }
            set { _dataModel.TraceType = value; }
        }

        public TraceCount TraceCount
        {
            get { return _dataModel.TraceCount; }
            set { _dataModel.TraceCount = value; }
        }

        public TraceDirection TraceDirection
        {
            get { return _dataModel.TraceDirection; }
            set { _dataModel.TraceDirection = value; }
        }

        public double MultipleScale
        {
            get { return _dataModel.MultipleScale; }
            set { _dataModel.MultipleScale = value; ProcessModelScale(); }
        }

        public double MaxLengthScale
        {
            get { return _dataModel.MaxLengthScale; }
            set { _dataModel.MaxLengthScale = value; ProcessModelScale(); }
        }

        public double BuildDistance
        {
            get { return _dataModel.BuildDistance; }
            set { _dataModel.BuildDistance = value; ProcessModelScale(); }
        }

        public bool IsMultipleScale
        {
            get { return _dataModel.IsMultipleScale; }
            set { _dataModel.IsMultipleScale = value; ProcessModelScale(); }
        }

        public bool IsMaxLengthScale
        {
            get { return _dataModel.IsMaxLengthScale; }
            set { _dataModel.IsMaxLengthScale = value; ProcessModelScale(); }
        }

        public bool IsAbsolutePosition
        {
            get { return _dataModel.IsAbsolutePosition; }
            set { _dataModel.IsAbsolutePosition = value; }
        }

        public bool IsInfrontofPlayer
        {
            get { return _dataModel.IsInfrontofPlayer; }
            set { _dataModel.IsInfrontofPlayer = value; }
        }

        public ObservableCollection<MaterialSelectionModel> OutsideMaterialsCollection
        {
            get { return _dataModel.OutsideMaterialsCollection; }
        }

        public int OutsideMaterialDepth
        {
            get { return _dataModel.OutsideMaterialDepth; }
            set { _dataModel.OutsideMaterialDepth = value; }
        }

        public ObservableCollection<MaterialSelectionModel> InsideMaterialsCollection
        {
            get { return _dataModel.InsideMaterialsCollection; }
        }

        public MaterialSelectionModel OutsideStockMaterial
        {
            get { return _dataModel.OutsideStockMaterial; }
            set { _dataModel.OutsideStockMaterial = value; }
        }

        public MaterialSelectionModel InsideStockMaterial
        {
            get { return _dataModel.InsideStockMaterial; }
            set { _dataModel.InsideStockMaterial = value; }
        }

        public string SourceFile
        {
            get { return _dataModel.SourceFile; }
            set { _dataModel.SourceFile = value; }
        }

        public double RotatePitch
        {
            get { return _dataModel.RotatePitch; }
            set { _dataModel.RotatePitch = value; ProcessModelScale(); }
        }

        public double RotateYaw
        {
            get { return _dataModel.RotateYaw; }
            set { _dataModel.RotateYaw = value; ProcessModelScale(); }
        }

        public double RotateRoll
        {
            get { return _dataModel.RotateRoll; }
            set { _dataModel.RotateRoll = value; ProcessModelScale(); }
        }

        public bool BeepWhenFinished
        {
            get { return _dataModel.BeepWhenFinished; }
            set { _dataModel.BeepWhenFinished = value; }
        }

        public bool SaveWhenFinsihed
        {
            get { return _dataModel.SaveWhenFinsihed; }
            set { _dataModel.SaveWhenFinsihed = value; }
        }

        public bool ShutdownWhenFinished
        {
            get { return _dataModel.ShutdownWhenFinished; }
            set { _dataModel.ShutdownWhenFinished = value; }
        }

        public bool RunInLowPrioity
        {
            get { return _dataModel.RunInLowPrioity; }
            set { _dataModel.RunInLowPrioity = value; }
        }

        public MyObjectBuilder_VoxelMap NewEntity { get; set; }

        #endregion

        #region command methods

        public bool Browse3DModelCanExecute()
        {
            return true;
        }

        public void Browse3DModelExecuted()
        {
            IsValidModel = false;
            IsValidEntity = false;

            var openFileDialog = _openFileDialogFactory();
            openFileDialog.Filter = AppConstants.ModelFilter;
            openFileDialog.Title = Res.DialogImportModelTitle;

            // Open the dialog
            if (_dialogService.ShowOpenFileDialog(this, openFileDialog) == DialogResult.OK)
            {
                Filename = openFileDialog.FileName;
            }
        }

        private void FilenameChanged()
        {
            ProcessFilename(Filename);
        }

        public bool CreateCanExecute()
        {
            return IsValidModel;
        }

        public void CreateExecuted()
        {
            var ok = BuildEntity();

            // do not close if cancelled.
            if (ok)
                CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            CloseResult = false;
        }

        #endregion

        #region methods

        private void ProcessFilename(string filename)
        {
            IsValidModel = false;
            IsValidEntity = false;
            IsBusy = true;

            OriginalModelSize = new BindableSize3DModel(0, 0, 0);
            NewModelSize = new BindableSize3DIModel(0, 0, 0);
            Position = new BindablePoint3DModel(0, 0, 0);

            if (File.Exists(filename))
            {
                // validate file is a real model.
                // read model properties.
                Model3D model;
                _originalBounds = Modelling.PreviewModelVolmetic(filename, out model);

                if (!_originalBounds.IsEmpty && _originalBounds.SizeX != 0 && _originalBounds.SizeY != 0 && _originalBounds.SizeZ != 0)
                {
                    Model = model;
                    var rotateTransform = MeshHelper.TransformVector(new System.Windows.Media.Media3D.Vector3D(0, 0, 0), -RotateRoll, RotateYaw - 90, RotatePitch + 90);
                    var bounds = _originalBounds;
                    if (rotateTransform != null)
                    {
                        bounds = rotateTransform.TransformBounds(bounds);
                    }

                    OriginalModelSize = new BindableSize3DModel(bounds);
                    IsValidModel = true;
                    ProcessModelScale();
                }
            }

            IsBusy = false;
        }

        private void ProcessModelScale()
        {
            if (IsValidModel)
            {
                var rotateTransform = MeshHelper.TransformVector(new System.Windows.Media.Media3D.Vector3D(0, 0, 0), -RotateRoll, RotateYaw - 90, RotatePitch + 90);
                var bounds = _originalBounds;
                if (rotateTransform != null)
                {
                    bounds = rotateTransform.TransformBounds(bounds);
                }

                var newSize = new BindableSize3DModel(bounds);

                if (IsMaxLengthScale)
                {
                    var factor = MaxLengthScale / Math.Max(Math.Max(newSize.Height, newSize.Width), newSize.Depth);

                    NewModelSize.Height = (int)(factor * newSize.Height);
                    NewModelSize.Width = (int)(factor * newSize.Width);
                    NewModelSize.Depth = (int)(factor * newSize.Depth);
                }
                else if (IsMultipleScale)
                {
                    NewModelSize.Height = (int)(MultipleScale * newSize.Height);
                    NewModelSize.Width = (int)(MultipleScale * newSize.Width);
                    NewModelSize.Depth = (int)(MultipleScale * newSize.Depth);
                }

                NewModelScale = new BindablePoint3DModel(NewModelSize.Width, NewModelSize.Height, NewModelSize.Depth);
            }
        }

        #endregion

        #region BuildEntity

        private bool BuildEntity()
        {
            var filenamepart = Path.GetFileNameWithoutExtension(Filename);
            var filename = MainViewModel.CreateUniqueVoxelStorageName(filenamepart + MyVoxelMap.V2FileExtension);

            double multiplier;
            if (IsMultipleScale)
            {
                multiplier = MultipleScale;
            }
            else
            {
                multiplier = MaxLengthScale / Math.Max(Math.Max(OriginalModelSize.Height, OriginalModelSize.Width), OriginalModelSize.Depth);
            }

            var scale = new ScaleTransform3D(multiplier, multiplier, multiplier);
            var rotateTransform = MeshHelper.TransformVector(new System.Windows.Media.Media3D.Vector3D(0, 0, 0), -RotateRoll, RotateYaw - 90, RotatePitch + 90);
            SourceFile = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);

            var model = MeshHelper.Load(Filename, ignoreErrors: true);

            var meshes = new List<MyVoxelRayTracer.MyMeshModel>();
            var geometeries = new List<MeshGeometry3D>();
            foreach (var model3D in model.Children)
            {
                var gm = (GeometryModel3D)model3D;
                var geometry = gm.Geometry as MeshGeometry3D;

                if (geometry != null)
                    geometeries.Add(geometry);
            }
            meshes.Add(new MyVoxelRayTracer.MyMeshModel(geometeries.ToArray(), InsideStockMaterial.MaterialIndex, InsideStockMaterial.MaterialIndex));

            #region handle dialogs and process the conversion

            var doCancel = false;

            var progressModel = new ProgressCancelModel { Title = Res.WnProgressTitle, SubTitle = Res.WnProgressTitle, DialogText = Res.WnProgressTxtTimeRemain + " " + Res.WnProgressTxtTimeCalculating };
            var progressVm = new ProgressCancelViewModel(this, progressModel);
            progressVm.CloseRequested += delegate(object sender, EventArgs e)
            {
                doCancel = true;
            };

            var cancelFunc = (Func<bool>)delegate
            {
                return doCancel;
            };

            var completedAction = (Action)delegate
            {
                progressVm.Close();
            };

            MyVoxelMap voxelMap = null;

            var action = (Action)delegate
            {
                voxelMap = MyVoxelRayTracer.ReadModelAsteroidVolmetic(model, meshes, scale, rotateTransform, TraceType, TraceCount, TraceDirection,
                    progressModel.ResetProgress, progressModel.IncrementProgress, cancelFunc, completedAction);
            };

            if (RunInLowPrioity)
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Idle;

            _dialogService.ShowDialog<WindowProgressCancel>(this, progressVm, action);

            if (RunInLowPrioity)
                System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;

            #endregion

            if (doCancel || voxelMap == null)
            {
                IsValidEntity = false;
                NewEntity = null;
            }
            else
            {
                voxelMap.ForceShellMaterial(OutsideStockMaterial.Value, (byte)OutsideMaterialDepth);
                voxelMap.Save(SourceFile);

                var position = VRageMath.Vector3D.Zero;
                var forward = Vector3.Forward;
                var up = Vector3.Up;

                if (IsAbsolutePosition)
                {
                    position = Position.ToVector3();
                }
                else if (IsInfrontofPlayer)
                {
                    // Figure out where the Character is facing, and plant the new construct centered in front of the Character, but "BuildDistance" units out in front.
                    var lookVector = (VRageMath.Vector3D)_dataModel.CharacterPosition.Forward.ToVector3();
                    lookVector.Normalize();

                    BoundingBoxD content = voxelMap.BoundingContent.ToBoundingBoxD();
                    VRageMath.Vector3D? boundingIntersectPoint = content.IntersectsRayAt(content.Center, -lookVector * 5000d);

                    if (!boundingIntersectPoint.HasValue)
                    {
                        boundingIntersectPoint = content.Center;
                    }

                    var distance = VRageMath.Vector3D.Distance(boundingIntersectPoint.Value, content.Center) + (float)BuildDistance;
                    VRageMath.Vector3D vector = lookVector * distance;
                    position = VRageMath.Vector3D.Add(_dataModel.CharacterPosition.Position, vector) - content.Center;
                }

                var entity = new MyObjectBuilder_VoxelMap(position, filename)
                {
                    EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ASTEROID),
                    PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                    StorageName = Path.GetFileNameWithoutExtension(filename)
                };

                entity.PositionAndOrientation = new MyPositionAndOrientation
                {
                    Position = position,
                    Forward = forward,
                    Up = up
                };

                IsValidEntity = voxelMap.BoundingContent.Size.Volume() > 0;

                NewEntity = entity;

                if (BeepWhenFinished)
                    System.Media.SystemSounds.Asterisk.Play();
            }

            return !doCancel;
        }

        #endregion
    }
}
