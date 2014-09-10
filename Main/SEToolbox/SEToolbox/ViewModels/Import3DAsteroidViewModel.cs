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

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using SEToolbox.Views;
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
                RaisePropertyChanged(() => CloseResult);
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
                    RaisePropertyChanged(() => IsBusy);
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
                RaisePropertyChanged(() => IsWrongModel);
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
            openFileDialog.Filter = Res.DialogImportModelFilter;
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
                    var rotateTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), RotateYaw + 90, RotatePitch, RotateRoll - 90);
                    var bounds = _originalBounds;
                    if (rotateTransform != null)
                    {
                        bounds = rotateTransform.TransformBounds(bounds);
                    }

                    OriginalModelSize = new BindableSize3DModel(bounds);
                    BuildDistance = 10;
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
                var rotateTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), RotateYaw + 90, RotatePitch, RotateRoll - 90);
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

                double vectorDistance = BuildDistance;

                vectorDistance += NewModelSize.Depth;
                NewModelScale = new BindablePoint3DModel(NewModelSize.Width, NewModelSize.Height, NewModelSize.Depth);

                // Figure out where the Character is facing, and plant the new construct right in front, by "10" units, facing the Character.
                var vector = new BindableVector3DModel(_dataModel.CharacterPosition.Forward).Vector3D;
                vector.Normalize();
                vector = Vector3D.Multiply(vector, vectorDistance);
                Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(_dataModel.CharacterPosition.Position).Point3D, vector));
                Forward = new BindableVector3DModel(_dataModel.CharacterPosition.Forward);
                Up = new BindableVector3DModel(_dataModel.CharacterPosition.Up);
            }
        }

        #endregion

        #region BuildEntity

        private bool BuildEntity()
        {
            var filenamepart = Path.GetFileNameWithoutExtension(Filename);
            var filename = MainViewModel.CreateUniqueVoxelFilename(filenamepart + ".vox");
            Position = Position.RoundOff(1.0);
            Forward = Forward.RoundToAxis();
            Up = Up.RoundToAxis();

            var entity = new MyObjectBuilder_VoxelMap(Position.ToVector3(), filename)
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
            };

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
            var rotateTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), RotateYaw + 90, RotatePitch, RotateRoll - 90);
            SourceFile = TempfileUtil.NewFilename();

            //var baseMaterial = SpaceEngineersApi.GetMaterialList().FirstOrDefault(m => m.IsRare == false);
            //if (baseMaterial == null)
            //    baseMaterial = SpaceEngineersApi.GetMaterialList().FirstOrDefault();

            //var voxelMap = MyVoxelBuilder.BuildAsteroidFromModel(true, Filename, SourceFile, OutsideStockMaterial.Value, baseMaterial.Name, InsideStockMaterial.Value != null, InsideStockMaterial.Value, ModelTraceVoxel.ThinSmoothed, multiplier, transform, MainViewModel.ResetProgress, MainViewModel.IncrementProgress);

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
            meshes.Add(new MyVoxelRayTracer.MyMeshModel(geometeries.ToArray(), InsideStockMaterial.Value, InsideStockMaterial.Value));

            #region handle dialogs and process the conversion

            var doCancel = false;

            var progressModel = new ProgressCancelModel { Title = "Processing...", SubTitle = "Processing...", DialogText = "Time remaining: Calculating..." };
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

            _dialogService.ShowDialog<WindowProgressCancel>(this, progressVm, action);

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

                entity.PositionAndOrientation = new MyPositionAndOrientation
                {
                    Position = Position.ToVector3(),
                    Forward = Forward.ToVector3(),
                    Up = Up.ToVector3()
                };

                IsValidEntity = voxelMap.ContentSize.X > 0 && voxelMap.ContentSize.Y > 0 && voxelMap.ContentSize.Z > 0;

                NewEntity = entity;
            }

            return !doCancel;
        }

        #endregion
    }
}
