namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;
    using Res = SEToolbox.Properties.Resources;

    public class Import3DModelViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly Import3DModelModel _dataModel;

        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region Constructors

        public Import3DModelViewModel(BaseViewModel parentViewModel, Import3DModelModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>)
        {
        }

        public Import3DModelViewModel(BaseViewModel parentViewModel, Import3DModelModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
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
            ClassType = ImportModelClassType.SmallShip;
            ArmorType = ImportArmorType.Light;
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
            set { _dataModel.IsValidModel = value; }
        }

        public BindableSize3DModel OriginalModelSize
        {
            get { return _dataModel.OriginalModelSize; }
            set { _dataModel.OriginalModelSize = value; }
        }

        public BindableSize3DIModel NewModelSize
        {
            get { return _dataModel.NewModelSize; }

            set
            {
                _dataModel.NewModelSize = value;
                ProcessModelScale();
            }
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

        public ModelTraceVoxel TraceType
        {
            get { return _dataModel.TraceType; }
            set { _dataModel.TraceType = value; }
        }

        public ImportModelClassType ClassType
        {
            get { return _dataModel.ClassType; }

            set
            {
                _dataModel.ClassType = value;
                ProcessModelScale();
            }
        }

        public bool IsAsteroid
        {
            get { return _dataModel.IsAsteroid; }
        }

        public bool IsShip
        {
            get { return _dataModel.IsShip; }
        }

        public ImportArmorType ArmorType
        {
            get { return _dataModel.ArmorType; }
            set { _dataModel.ArmorType = value; }
        }


        public double MultipleScale
        {
            get { return _dataModel.MultipleScale; }

            set
            {
                _dataModel.MultipleScale = value;
                ProcessModelScale();
            }
        }

        public double MaxLengthScale
        {
            get { return _dataModel.MaxLengthScale; }

            set
            {
                _dataModel.MaxLengthScale = value;
                ProcessModelScale();
            }
        }

        public double BuildDistance
        {
            get
            {
                return _dataModel.BuildDistance;
            }

            set
            {
                _dataModel.BuildDistance = value;
                ProcessModelScale();
            }
        }

        public bool IsMultipleScale
        {
            get { return _dataModel.IsMultipleScale; }

            set
            {
                _dataModel.IsMultipleScale = value;
                ProcessModelScale();
            }
        }

        public bool IsMaxLengthScale
        {
            get { return _dataModel.IsMaxLengthScale; }

            set
            {
                _dataModel.IsMaxLengthScale = value;
                ProcessModelScale();
            }
        }

        public ObservableCollection<MaterialSelectionModel> OutsideMaterialsCollection
        {
            get { return _dataModel.OutsideMaterialsCollection; }
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

        public bool FillObject
        {
            get { return _dataModel.FillObject; }
            set { _dataModel.FillObject = value; }
        }

        #endregion

        #region command methods

        public bool Browse3DModelCanExecute()
        {
            return true;
        }

        public void Browse3DModelExecuted()
        {
            IsValidModel = false;

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
            IsBusy = true;

            OriginalModelSize = new BindableSize3DModel(0, 0, 0);
            NewModelSize = new BindableSize3DIModel(0, 0, 0);
            Position = new BindablePoint3DModel(0, 0, 0);

            if (File.Exists(filename))
            {
                // validate file is a real model.
                // read model properties.
                Model3D model;
                var bounds = Modelling.PreviewModelVolmetic(filename, out model);
                var size = new BindableSize3DModel(bounds);
                Model = model;

                if (size != null && size.Height != 0 && size.Width != 0 && size.Depth != 0)
                {
                    OriginalModelSize = size;
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
                if (IsMaxLengthScale)
                {
                    var factor = MaxLengthScale / Math.Max(Math.Max(OriginalModelSize.Height, OriginalModelSize.Width), OriginalModelSize.Depth);

                    NewModelSize.Height = (int)(factor * OriginalModelSize.Height);
                    NewModelSize.Width = (int)(factor * OriginalModelSize.Width);
                    NewModelSize.Depth = (int)(factor * OriginalModelSize.Depth);
                }
                else if (IsMultipleScale)
                {
                    NewModelSize.Height = (int)(MultipleScale * OriginalModelSize.Height);
                    NewModelSize.Width = (int)(MultipleScale * OriginalModelSize.Width);
                    NewModelSize.Depth = (int)(MultipleScale * OriginalModelSize.Depth);
                }

                double vectorDistance = BuildDistance;
                double scaleMultiplyer = 1;

                switch (ClassType)
                {
                    case ImportModelClassType.SmallShip: scaleMultiplyer = MyCubeSize.Small.ToLength(); break;
                    case ImportModelClassType.SmallStation: scaleMultiplyer = MyCubeSize.Small.ToLength(); break;
                    case ImportModelClassType.LargeShip: scaleMultiplyer = MyCubeSize.Large.ToLength(); break;
                    case ImportModelClassType.LargeStation: scaleMultiplyer = MyCubeSize.Large.ToLength(); break;
                    case ImportModelClassType.Asteroid: scaleMultiplyer = 1; break;
                }
                vectorDistance += NewModelSize.Depth * scaleMultiplyer;
                NewModelScale = new BindablePoint3DModel(NewModelSize.Width * scaleMultiplyer, NewModelSize.Height * scaleMultiplyer, NewModelSize.Depth * scaleMultiplyer);

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

        #region BuildTestEntity

        public MyObjectBuilder_CubeGrid BuildTestEntity()
        {
            var entity = new MyObjectBuilder_CubeGrid
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ENTITY),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                Skeleton = new System.Collections.Generic.List<BoneInfo>(),
                LinearVelocity = new VRageMath.Vector3(0, 0, 0),
                AngularVelocity = new VRageMath.Vector3(0, 0, 0),
                GridSizeEnum = MyCubeSize.Large
            };

            var blockPrefix = entity.GridSizeEnum.ToString();
            var cornerBlockPrefix = entity.GridSizeEnum.ToString();

            entity.IsStatic = false;
            blockPrefix += "BlockArmor";        // HeavyBlockArmor|BlockArmor;
            cornerBlockPrefix += "BlockArmor"; // HeavyBlockArmor|BlockArmor|RoundArmor_;

            // Figure out where the Character is facing, and plant the new constrcut right in front, by "10" units, facing the Character.
            var vector = new BindableVector3DModel(_dataModel.CharacterPosition.Forward).Vector3D;
            vector.Normalize();
            vector = Vector3D.Multiply(vector, 6);
            Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(_dataModel.CharacterPosition.Position).Point3D, vector));
            Forward = new BindableVector3DModel(_dataModel.CharacterPosition.Forward);
            Up = new BindableVector3DModel(_dataModel.CharacterPosition.Up);

            entity.PositionAndOrientation = new MyPositionAndOrientation
            {
                Position = Position.ToVector3D(),
                Forward = Forward.ToVector3(),
                Up = Up.ToVector3()
            };

            // Large|BlockArmor|Corner
            // Large|RoundArmor_|Corner
            // Large|HeavyBlockArmor|Block,
            // Small|BlockArmor|Slope,
            // Small|HeavyBlockArmor|Corner,

            var blockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "Block");
            var slopeBlockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), cornerBlockPrefix + "Slope");
            var cornerBlockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), cornerBlockPrefix + "Corner");
            var inverseCornerBlockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), cornerBlockPrefix + "CornerInv");

            entity.CubeBlocks = new System.Collections.Generic.List<MyObjectBuilder_CubeBlock>();

            //var smoothObject = true;

            // Read in voxel and set main cube space.
            //var ccubic = TestCreateSplayedDiagonalPlane();
            //var ccubic = TestCreateSlopedDiagonalPlane();
            //var ccubic = TestCreateStaggeredStar();
            var ccubic = Modelling.TestCreateTrayShape();
            //var ccubic = ReadModelVolmetic(@"..\..\..\..\..\..\building 3D\models\Rhino_corrected.obj", 10, null, ModelTraceVoxel.ThickSmoothedDown);

            var fillObject = false;

            //if (smoothObject)
            //{
            //    CalculateAddedInverseCorners(ccubic);
            //    CalculateAddedSlopes(ccubic);
            //    CalculateAddedCorners(ccubic);
            //}

            Modelling.BuildStructureFromCubic(entity, ccubic, fillObject, blockType, slopeBlockType, cornerBlockType, inverseCornerBlockType);

            return entity;
        }

        #endregion

        #region BuildEntity

        public MyObjectBuilder_EntityBase BuildEntity()
        {
            if (ClassType == ImportModelClassType.Asteroid)
            {
                return BuildAsteroidEntity();
            }

            return BuildShipEntity();
        }

        private MyObjectBuilder_VoxelMap BuildAsteroidEntity()
        {
            var filenamepart = Path.GetFileNameWithoutExtension(Filename);
            var filename = MainViewModel.CreateUniqueVoxelStorageName(filenamepart + MyVoxelMap.V2FileExtension);
            Position = Position.RoundOff(1.0);
            Forward = Forward.RoundToAxis();
            Up = Up.RoundToAxis();

            var entity = new MyObjectBuilder_VoxelMap(Position.ToVector3(), filename)
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ASTEROID),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                StorageName = Path.GetFileNameWithoutExtension(filename)
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

            var transform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), 0, 0, 0);
            SourceFile = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);

            var baseMaterial = SpaceEngineersCore.Resources.VoxelMaterialDefinitions.FirstOrDefault(m => m.IsRare == false) ?? SpaceEngineersCore.Resources.VoxelMaterialDefinitions.FirstOrDefault();

            var voxelMap = MyVoxelBuilder.BuildAsteroidFromModel(true, Filename, OutsideStockMaterial.MaterialIndex.Value, baseMaterial.Index, InsideStockMaterial.Value != null, InsideStockMaterial.MaterialIndex, ModelTraceVoxel.ThinSmoothed, multiplier, transform, MainViewModel.ResetProgress, MainViewModel.IncrementProgress);
            voxelMap.Save(SourceFile);

            MainViewModel.ClearProgress();

            entity.PositionAndOrientation = new MyPositionAndOrientation
            {
                Position = Position.ToVector3D(),
                Forward = Forward.ToVector3(),
                Up = Up.ToVector3()
            };

            IsValidModel = voxelMap.BoundingContent.Size.Volume() > 0;

            return entity;
        }

        private MyObjectBuilder_CubeGrid BuildShipEntity()
        {
            var entity = new MyObjectBuilder_CubeGrid
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ENTITY),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                Skeleton = new System.Collections.Generic.List<BoneInfo>(),
                LinearVelocity = new VRageMath.Vector3(0, 0, 0),
                AngularVelocity = new VRageMath.Vector3(0, 0, 0)
            };

            var blockPrefix = "";
            switch (ClassType)
            {
                case ImportModelClassType.SmallShip:
                    entity.GridSizeEnum = MyCubeSize.Small;
                    blockPrefix += "Small";
                    entity.IsStatic = false;
                    break;

                case ImportModelClassType.SmallStation:
                    entity.GridSizeEnum = MyCubeSize.Small;
                    blockPrefix += "Small";
                    entity.IsStatic = true;
                    Position = Position.RoundOff(MyCubeSize.Small.ToLength());
                    Forward = Forward.RoundToAxis();
                    Up = Up.RoundToAxis();
                    break;

                case ImportModelClassType.LargeShip:
                    entity.GridSizeEnum = MyCubeSize.Large;
                    blockPrefix += "Large";
                    entity.IsStatic = false;
                    break;

                case ImportModelClassType.LargeStation:
                    entity.GridSizeEnum = MyCubeSize.Large;
                    blockPrefix += "Large";
                    entity.IsStatic = true;
                    Position = Position.RoundOff(MyCubeSize.Large.ToLength());
                    Forward = Forward.RoundToAxis();
                    Up = Up.RoundToAxis();
                    break;
            }

            switch (ArmorType)
            {
                case ImportArmorType.Heavy: blockPrefix += "HeavyBlockArmor"; break;
                case ImportArmorType.Light: blockPrefix += "BlockArmor"; break;

                // TODO: Rounded Armor.
                // Currently in development, and only specified as 'Light' on the 'Large' structures.
                //case ImportArmorType.Round: blockPrefix += "RoundArmor_"; break;
            }

            // Large|BlockArmor|Corner
            // Large|RoundArmor_|Corner
            // Large|HeavyBlockArmor|Block,
            // Small|BlockArmor|Slope,
            // Small|HeavyBlockArmor|Corner,

            var blockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "Block");
            var slopeBlockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "Slope");
            var cornerBlockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "Corner");
            var inverseCornerBlockType = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "CornerInv");

            entity.CubeBlocks = new System.Collections.Generic.List<MyObjectBuilder_CubeBlock>();

            double multiplier;
            if (IsMultipleScale)
            {
                multiplier = MultipleScale;
            }
            else
            {
                multiplier = MaxLengthScale / Math.Max(Math.Max(OriginalModelSize.Height, OriginalModelSize.Width), OriginalModelSize.Depth);
            }

            var ccubic = Modelling.ReadModelVolmetic(Filename, multiplier, null, TraceType, MainViewModel.ResetProgress, MainViewModel.IncrementProgress);

            Modelling.BuildStructureFromCubic(entity, ccubic, FillObject, blockType, slopeBlockType, cornerBlockType, inverseCornerBlockType);

            MainViewModel.ClearProgress();

            entity.PositionAndOrientation = new MyPositionAndOrientation
            {
                // TODO: reposition based scale.
                Position = Position.ToVector3D(),
                Forward = Forward.ToVector3(),
                Up = Up.ToVector3()
            };

            IsValidModel = entity.CubeBlocks.Count > 0;

            return entity;
        }

        #endregion
    }
}
