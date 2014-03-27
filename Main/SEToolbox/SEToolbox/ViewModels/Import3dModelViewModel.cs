namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Properties;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using VRageMath;

    public class Import3dModelViewModel : BaseViewModel
    {
        #region Fields

        private static readonly object Locker = new object();

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly Import3dModelModel _dataModel;

        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region Constructors

        public Import3dModelViewModel(BaseViewModel parentViewModel, Import3dModelModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), () => ServiceLocator.Resolve<IOpenFileDialog>())
        {
        }

        public Import3dModelViewModel(BaseViewModel parentViewModel, Import3dModelModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            this._dialogService = dialogService;
            this._openFileDialogFactory = openFileDialogFactory;
            this._dataModel = dataModel;
            this._dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };

            this.IsMultipleScale = true;
            this.MultipleScale = 1;
            this.MaxLengthScale = 100;
            this.ClassType = ImportClassType.SmallShip;
            this.ArmorType = ImportArmorType.Light;
        }

        #endregion

        #region Properties

        public ICommand Browse3dModelCommand
        {
            get
            {
                return new DelegateCommand(new Action(Browse3dModelExecuted), new Func<bool>(Browse3dModelCanExecute));
            }
        }

        public ICommand CreateCommand
        {
            get
            {
                return new DelegateCommand(new Action(CreateExecuted), new Func<bool>(CreateCanExecute));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(new Action(CancelExecuted), new Func<bool>(CancelCanExecute));
            }
        }

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get
            {
                return this._closeResult;
            }

            set
            {
                this._closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }

            set
            {
                if (value != this._isBusy)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    if (this._isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        public string Filename
        {
            get
            {
                return this._dataModel.Filename;
            }

            set
            {
                this._dataModel.Filename = value;
                this.FilenameChanged();
            }
        }

        public Model3D Model
        {
            get
            {
                return this._dataModel.Model;
            }

            set
            {
                this._dataModel.Model = value;
            }
        }

        public bool IsValidModel
        {
            get
            {
                return this._dataModel.IsValidModel;
            }

            set
            {
                this._dataModel.IsValidModel = value;
            }
        }

        public BindableSize3DModel OriginalModelSize
        {
            get
            {
                return this._dataModel.OriginalModelSize;
            }

            set
            {
                this._dataModel.OriginalModelSize = value;
            }
        }

        public BindableSize3DIModel NewModelSize
        {
            get
            {
                return this._dataModel.NewModelSize;
            }

            set
            {
                this._dataModel.NewModelSize = value;
                this.ProcessModelScale();
            }
        }

        public BindablePoint3DModel NewModelScale
        {
            get
            {
                return this._dataModel.NewModelScale;
            }

            set
            {
                this._dataModel.NewModelScale = value;
            }
        }

        public BindablePoint3DModel Position
        {
            get
            {
                return this._dataModel.Position;
            }

            set
            {
                this._dataModel.Position = value;
            }
        }

        public BindableVector3DModel Forward
        {
            get
            {
                return this._dataModel.Forward;
            }

            set
            {
                this._dataModel.Forward = value;
            }
        }

        public BindableVector3DModel Up
        {
            get
            {
                return this._dataModel.Up;
            }

            set
            {
                this._dataModel.Up = value;
            }
        }

        public ModelTraceVoxel TraceType
        {
            get
            {
                return this._dataModel.TraceType;
            }

            set
            {
                this._dataModel.TraceType = value;
            }
        }

        public ImportClassType ClassType
        {
            get
            {
                return this._dataModel.ClassType;
            }

            set
            {
                this._dataModel.ClassType = value;
                this.ProcessModelScale();
            }
        }

        public ImportArmorType ArmorType
        {
            get
            {
                return this._dataModel.ArmorType;
            }

            set
            {
                this._dataModel.ArmorType = value;
            }
        }


        public double MultipleScale
        {
            get
            {
                return this._dataModel.MultipleScale;
            }

            set
            {
                this._dataModel.MultipleScale = value;
                this.ProcessModelScale();
            }
        }

        public double MaxLengthScale
        {
            get
            {
                return this._dataModel.MaxLengthScale;
            }

            set
            {
                this._dataModel.MaxLengthScale = value;
                this.ProcessModelScale();
            }
        }

        public double BuildDistance
        {
            get
            {
                return this._dataModel.BuildDistance;
            }

            set
            {
                this._dataModel.BuildDistance = value;
                this.ProcessModelScale();
            }
        }

        public bool IsMultipleScale
        {
            get
            {
                return this._dataModel.IsMultipleScale;
            }

            set
            {
                this._dataModel.IsMultipleScale = value;
                this.ProcessModelScale();
            }
        }

        public bool IsMaxLengthScale
        {
            get
            {
                return this._dataModel.IsMaxLengthScale;
            }

            set
            {
                this._dataModel.IsMaxLengthScale = value;
                this.ProcessModelScale();
            }
        }

        #endregion

        #region methods

        public bool Browse3dModelCanExecute()
        {
            return true;
        }

        public void Browse3dModelExecuted()
        {
            this.IsValidModel = false;

            IOpenFileDialog openFileDialog = _openFileDialogFactory();
            openFileDialog.Filter = Resources.ImportModelFilter;
            openFileDialog.Title = Resources.ImportModelTitle;

            // Open the dialog
            DialogResult result = _dialogService.ShowOpenFileDialog(this, openFileDialog);

            if (result == DialogResult.OK)
            {
                this.Filename = openFileDialog.FileName;
            }
        }

        private void FilenameChanged()
        {
            this.ProcessFilename(this.Filename);
        }

        public bool CreateCanExecute()
        {
            return this.IsValidModel;
        }

        public void CreateExecuted()
        {
            this.CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            this.CloseResult = false;
        }

        #endregion

        #region methods

        private void ProcessFilename(string filename)
        {
            this.IsValidModel = false;
            this.IsBusy = true;

            this.OriginalModelSize = new BindableSize3DModel(0, 0, 0);
            this.NewModelSize = new BindableSize3DIModel(0, 0, 0);
            this.Position = new BindablePoint3DModel(0, 0, 0);

            if (File.Exists(filename))
            {
                // validate file is a real model.
                // read model properties.
                Model3D model;
                var size = PreviewModelVolmetic(filename, out model);
                this.Model = model;

                if (size != null && size.Height != 0 && size.Width != 0 && size.Depth != 0)
                {
                    this.OriginalModelSize = size;
                    this.BuildDistance = 10;
                    this.IsValidModel = true;
                    this.ProcessModelScale();
                }
            }

            this.IsBusy = false;
        }

        private void ProcessModelScale()
        {
            if (this.IsValidModel)
            {
                if (this.IsMaxLengthScale)
                {
                    var factor = this.MaxLengthScale / Math.Max(Math.Max(this.OriginalModelSize.Height, this.OriginalModelSize.Width), this.OriginalModelSize.Depth);

                    this.NewModelSize.Height = (int)(factor * this.OriginalModelSize.Height);
                    this.NewModelSize.Width = (int)(factor * this.OriginalModelSize.Width);
                    this.NewModelSize.Depth = (int)(factor * this.OriginalModelSize.Depth);
                }
                else if (this.IsMultipleScale)
                {
                    this.NewModelSize.Height = (int)(this.MultipleScale * this.OriginalModelSize.Height);
                    this.NewModelSize.Width = (int)(this.MultipleScale * this.OriginalModelSize.Width);
                    this.NewModelSize.Depth = (int)(this.MultipleScale * this.OriginalModelSize.Depth);
                }

                double vectorDistance = this.BuildDistance;
                double scaleMultiplyer = 2.5;

                switch (this.ClassType)
                {
                    case ImportClassType.SmallShip: scaleMultiplyer = 0.5; break;
                    case ImportClassType.LargeShip: scaleMultiplyer = 2.5; break;
                    case ImportClassType.Station: scaleMultiplyer = 2.5; break;
                }
                vectorDistance += this.NewModelSize.Depth * scaleMultiplyer;
                this.NewModelScale = new BindablePoint3DModel(this.NewModelSize.Width * scaleMultiplyer, this.NewModelSize.Height * scaleMultiplyer, this.NewModelSize.Depth * scaleMultiplyer);

                // Figure out where the Character is facing, and plant the new constrcut right in front, by "10" units, facing the Character.
                var vector = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward).Vector3D;
                vector.Normalize();
                vector = Vector3D.Multiply(vector, vectorDistance);
                this.Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(this._dataModel.CharacterPosition.Position).Point3D, vector));
                this.Forward = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward);
                this.Up = new BindableVector3DModel(this._dataModel.CharacterPosition.Up);
            }
        }

        #endregion

        #region BuildTestEntity

        public MyObjectBuilder_CubeGrid BuildTestEntity()
        {
            var entity = new MyObjectBuilder_CubeGrid
            {
                EntityId = SpaceEngineersAPI.GenerateEntityId(),
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
            var vector = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward).Vector3D;
            vector.Normalize();
            vector = Vector3D.Multiply(vector, 6);
            this.Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(this._dataModel.CharacterPosition.Position).Point3D, vector));
            this.Forward = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward);
            this.Up = new BindableVector3DModel(this._dataModel.CharacterPosition.Up);

            entity.PositionAndOrientation = new MyPositionAndOrientation()
            {
                // TODO: reposition based scale.
                Position = this.Position.ToVector3(),
                Forward = this.Forward.ToVector3(),
                Up = this.Up.ToVector3()
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

            var fixScale = 0;
            if (this.IsMultipleScale && this.MultipleScale == 1)
            {
                fixScale = 0;
            }
            else
            {
                fixScale = Math.Max(Math.Max(this.NewModelSize.Height, this.NewModelSize.Width), this.NewModelSize.Depth);
            }

            var smoothObject = true;

            #region Read in voxel and set main cube space.

            //// Splayed diagonal plane.
            //var max = 40;
            //var ccubic = new CubeType[max, max, max];

            //for (var z = 0; z < max; z++)
            //{
            //    for (var j = 0; j < max; j++)
            //    {
            //        {
            //            var x = j + z;
            //            var y = j;
            //            if (x >= 0 && y >= 0 && z >= 0 && x < max && y < max && z < max) ccubic[x, y, z] = CubeType.Cube;
            //        }
            //        {
            //            var x = j;
            //            var y = j + z;
            //            if (x >= 0 && y >= 0 && z >= 0 && x < max && y < max && z < max) ccubic[x, y, z] = CubeType.Cube;
            //        }
            //        {
            //            var x = j + z;
            //            var y = max - j;
            //            if (x >= 0 && y >= 0 && z >= 0 && x < max && y < max && z < max) ccubic[x, y, z] = CubeType.Cube;
            //        }
            //        {
            //            var x = j;
            //            var y = max - (j + z);
            //            if (x >= 0 && y >= 0 && z >= 0 && x < max && y < max && z < max) ccubic[x, y, z] = CubeType.Cube;
            //        }
            //    }
            //}

            #endregion

            #region Read in voxel and set main cube space.

            // Sloped diagonal plane.
            //var max = 20;
            //var ccubic = new CubeType[max, max, max];
            //var dx = 1;
            //var dy = 6;
            //var dz = 0;

            //for (var i = 0; i < max; i++)
            //{
            //    for (var j = 0; j < max; j++)
            //    {
            //        if (dx + j >= 0 && dy + j - i >= 0 && dz + i >= 0 &&
            //            dx + j < max && dy + j - i < max && dz + i < max)
            //        {
            //            ccubic[dx + j, dy + j - i, dz + i] = CubeType.Cube;
            //        }
            //    }
            //}

            #endregion

            #region Read in voxel and set main cube space.

            // Staggered star.

            var ccubic = new CubeType[9, 9, 9];

            for (var i = 2; i < 7; i++)
            {
                for (var j = 2; j < 7; j++)
                {
                    ccubic[i, j, 4] = CubeType.Cube;
                    ccubic[i, 4, j] = CubeType.Cube;
                    ccubic[4, i, j] = CubeType.Cube;
                }
            }

            for (var i = 0; i < 9; i++)
            {
                ccubic[i, 4, 4] = CubeType.Cube;
                ccubic[4, i, 4] = CubeType.Cube;
                ccubic[4, 4, i] = CubeType.Cube;
            }

            #endregion

            #region Read in voxel and set main cube space.

            //// Tray shape

            //var max = 20;
            //var offset = 5;

            //var ccubic = new CubeType[max, max, max];

            //for (var x = 0; x < max; x++)
            //{
            //    for (var y = 0; y < max; y++)
            //    {
            //        ccubic[2, x, y] = CubeType.Cube;
            //    }
            //}

            //for (var z = 1; z < 4; z += 2)
            //{
            //    for (int i = 0; i < max; i++)
            //    {
            //        ccubic[z, i, 0] = CubeType.Cube;
            //        ccubic[z, i, max - 1] = CubeType.Cube;
            //        ccubic[z, 0, i] = CubeType.Cube;
            //        ccubic[z, max - 1, i] = CubeType.Cube;
            //    }

            //    for (int i = 0 + offset; i < max - offset; i++)
            //    {
            //        ccubic[z, i, i] = CubeType.Cube;
            //        ccubic[z, max - i - 1, i] = CubeType.Cube;
            //    }
            //}

            #endregion

            if (smoothObject)
            {
                CalculateInverseCorners(ccubic);
                CalculateSlopes(ccubic);
                CalculateCorners(ccubic);
            }

            BuildStructureFromCubic(entity, ccubic, blockType, slopeBlockType, cornerBlockType, inverseCornerBlockType);

            return entity;
        }

        #endregion

        #region BuildEntity

        public MyObjectBuilder_CubeGrid BuildEntity()
        {
            var entity = new MyObjectBuilder_CubeGrid
            {
                EntityId = SpaceEngineersAPI.GenerateEntityId(),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                Skeleton = new System.Collections.Generic.List<BoneInfo>(),
                LinearVelocity = new VRageMath.Vector3(0, 0, 0),
                AngularVelocity = new VRageMath.Vector3(0, 0, 0)
            };

            var blockPrefix = "";
            switch (this.ClassType)
            {
                case ImportClassType.SmallShip:
                    entity.GridSizeEnum = MyCubeSize.Small;
                    blockPrefix += "Small";
                    entity.IsStatic = false;
                    break;

                case ImportClassType.LargeShip:
                    entity.GridSizeEnum = MyCubeSize.Large;
                    blockPrefix += "Large";
                    entity.IsStatic = false;
                    break;

                case ImportClassType.Station:
                    entity.GridSizeEnum = MyCubeSize.Large;
                    blockPrefix += "Large";
                    entity.IsStatic = true;
                    this.Position = this.Position.RoundOff(2.5);
                    this.Forward = this.Forward.RoundToAxis();
                    this.Up = this.Up.RoundToAxis();
                    break;
            }

            switch (this.ArmorType)
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

            double multiplier = 1;
            if (this.IsMultipleScale)
            {
                multiplier = this.MultipleScale;
            }
            else
            {
                multiplier = this.MaxLengthScale / Math.Max(Math.Max(this.OriginalModelSize.Height, this.OriginalModelSize.Width), this.OriginalModelSize.Depth);
            }

            var ccubic = ReadModelVolmetic(this.Filename, multiplier, null, this.TraceType);

            // TODO: fillobject UI.
            //var fillObject = false;

            BuildStructureFromCubic(entity, ccubic, blockType, slopeBlockType, cornerBlockType, inverseCornerBlockType);

            entity.PositionAndOrientation = new MyPositionAndOrientation()
            {
                // TODO: reposition based scale.
                Position = this.Position.ToVector3(),
                Forward = this.Forward.ToVector3(),
                Up = this.Up.ToVector3()
            };

            return entity;
        }

        #endregion

        #region ReadModelVolmetic

        public static BindableSize3DModel PreviewModelVolmetic(string modelFile, out Model3D model)
        {
            var size = new BindableSize3DModel();

            if (modelFile != null)
            {
                try
                {
                    model = MeshHelper.Load(modelFile, ignoreErrors: true);
                }
                catch
                {
                    model = null;
                    return null;
                }

                if (model.Bounds == Rect3D.Empty)
                {
                    model = null;
                    return null;
                }

                return new BindableSize3DModel(model.Bounds);
            }

            model = null;
            return null;
        }

        public static CubeType[, ,] ReadModelVolmetic(string modelFile, double scaleMultiplyier, Transform3D transform, ModelTraceVoxel traceType)
        {
            return ReadModelVolmetic(modelFile, scaleMultiplyier, scaleMultiplyier, scaleMultiplyier, transform, traceType);
        }

        /// <summary>
        /// Volumes are calculated across axis where they are whole numbers (rounded to 0 decimal places).
        /// </summary>
        /// <param name="modelFile"></param>
        /// <param name="scaleMultiplyierX"></param>
        /// <param name="scaleMultiplyierY"></param>
        /// <param name="scaleMultiplyierZ"></param>
        /// <param name="transform"></param>
        /// <param name="traceType"></param>
        /// <returns></returns>
        public static CubeType[, ,] ReadModelVolmetic(string modelFile, double scaleMultiplyierX, double scaleMultiplyierY, double scaleMultiplyierZ, Transform3D transform, ModelTraceVoxel traceType)
        {
            var model = MeshHelper.Load(modelFile, ignoreErrors: true);

            // How far to check in from the proposed Volumetric edge.
            // This number is just made up, but small enough that it still represents the corner edge of the Volumetric space.
            // But still large enough that it isn't the exact corner.
            const double offset = 0.00000456f;

            if (scaleMultiplyierX > 0 && scaleMultiplyierY > 0 && scaleMultiplyierZ > 0 && scaleMultiplyierX != 1.0f && scaleMultiplyierY != 1.0f && scaleMultiplyierZ != 1.0f)
            {
                model.TransformScale(scaleMultiplyierX, scaleMultiplyierY, scaleMultiplyierZ);
            }

            var tbounds = model.Bounds;
            if (transform != null)
                tbounds = transform.TransformBounds(tbounds);

            var xMin = (int)Math.Floor(tbounds.X);
            var yMin = (int)Math.Floor(tbounds.Y);
            var zMin = (int)Math.Floor(tbounds.Z);

            var xMax = (int)Math.Ceiling(tbounds.X + tbounds.SizeX);
            var yMax = (int)Math.Ceiling(tbounds.Y + tbounds.SizeY);
            var zMax = (int)Math.Ceiling(tbounds.Z + tbounds.SizeZ);

            var xCount = xMax - xMin;
            var yCount = yMax - yMin;
            var zCount = zMax - zMin;

            var ccubic = new CubeType[xCount + 0, yCount + 0, zCount + 0];

            #region basic ray trace of every individual triangle.

            foreach (var model3D in model.Children)
            {
                var gm = (GeometryModel3D)model3D;
                var g = gm.Geometry as MeshGeometry3D;

                for (var t = 0; t < g.TriangleIndices.Count; t += 3)
                {
                    var p1 = g.Positions[g.TriangleIndices[t]];
                    var p2 = g.Positions[g.TriangleIndices[t + 1]];
                    var p3 = g.Positions[g.TriangleIndices[t + 2]];

                    if (transform != null)
                    {
                        p1 = transform.Transform(p1);
                        p2 = transform.Transform(p2);
                        p3 = transform.Transform(p3);
                    }

                    var minBound = MeshHelper.Min(p1, p2, p3).Floor();
                    var maxBound = MeshHelper.Max(p1, p2, p3).Ceiling();

                    Point3D[] rays;

                    for (var y = minBound.Y; y < maxBound.Y; y++)
                    {
                        for (var z = minBound.Z; z < maxBound.Z; z++)
                        {
                            if (traceType == ModelTraceVoxel.Thin || traceType == ModelTraceVoxel.ThinSmoothed)
                            {
                                rays = new Point3D[] // 1 point ray trace in the center.
                                    {
                                        new Point3D(xMin, y + 0.5 + offset, z + 0.5 + offset), new Point3D(xMax, y + 0.5 + offset, z + 0.5 + offset)
                                    };
                            }
                            else
                            {
                                rays = new Point3D[] // 4 point ray trace within each corner of the expected Volumetric cube.
                                    {
                                        new Point3D(xMin, y + offset, z + offset), new Point3D(xMax, y + offset, z + offset),
                                        new Point3D(xMin, y + 1 - offset, z + offset), new Point3D(xMax, y + 1 - offset, z + offset),
                                        new Point3D(xMin, y + offset, z + 1 - offset), new Point3D(xMax, y + offset, z + 1 - offset),
                                        new Point3D(xMin, y + 1 - offset, z + 1 - offset), new Point3D(xMax, y + 1 - offset, z + 1 - offset)
                                    };
                            }

                            Point3D intersect;
                            if (MeshHelper.RayIntersetTriangleRound(p1, p2, p3, rays, out intersect))
                            {
                                ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                            }
                        }
                    }

                    for (var x = minBound.X; x < maxBound.X; x++)
                    {
                        for (var z = minBound.Z; z < maxBound.Z; z++)
                        {
                            if (traceType == ModelTraceVoxel.Thin || traceType == ModelTraceVoxel.ThinSmoothed)
                            {
                                rays = new Point3D[] // 1 point ray trace in the center.
                                    {
                                        new Point3D(x + 0.5 + offset, yMin, z + 0.5 + offset), new Point3D(x + 0.5 + offset, yMax, z + 0.5 + offset)
                                    };
                            }
                            else
                            {
                                rays = new Point3D[] // 4 point ray trace within each corner of the expected Volumetric cube.
                                    {
                                        new Point3D(x + offset, yMin, z + offset), new Point3D(x + offset, yMax, z + offset),
                                        new Point3D(x + 1 - offset, yMin, z + offset), new Point3D(x + 1 - offset, yMax, z + offset),
                                        new Point3D(x + offset, yMin, z + 1 - offset), new Point3D(x + offset, yMax, z + 1 - offset),
                                        new Point3D(x + 1 - offset, yMin, z + 1 - offset), new Point3D(x + 1 - offset, yMax, z + 1 - offset)
                                    };
                            }

                            Point3D intersect;
                            if (MeshHelper.RayIntersetTriangleRound(p1, p2, p3, rays, out intersect))
                            {
                                ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                            }
                        }
                    }

                    for (var x = minBound.X; x < maxBound.X; x++)
                    {
                        for (var y = minBound.Y; y < maxBound.Y; y++)
                        {
                            if (traceType == ModelTraceVoxel.Thin || traceType == ModelTraceVoxel.ThinSmoothed)
                            {
                                rays = new Point3D[] // 1 point ray trace in the center.
                                    {
                                        new Point3D(x + 0.5 + offset, y + 0.5 + offset, zMin), new Point3D(x + 0.5 + offset, y + 0.5 + offset, zMax),
                                    };
                            }
                            else
                            {
                                rays = new Point3D[] // 4 point ray trace within each corner of the expected Volumetric cube.
                                    {
                                        new Point3D(x + offset, y + offset, zMin), new Point3D(x + offset, y + offset, zMax),
                                        new Point3D(x + 1 - offset, y + offset, zMin), new Point3D(x + 1 - offset, y + offset, zMax),
                                        new Point3D(x + offset, y + 1 - offset, zMin), new Point3D(x + offset, y + 1 - offset, zMax),
                                        new Point3D(x + 1 - offset, y + 1 - offset, zMin), new Point3D(x + 1 - offset, y + 1 - offset, zMax)
                                    };
                            }

                            Point3D intersect;
                            if (MeshHelper.RayIntersetTriangleRound(p1, p2, p3, rays, out intersect))
                            {
                                ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                            }
                        }
                    }
                }
            }

            #endregion

            CrawlExterior(ccubic);

            if (traceType == ModelTraceVoxel.ThinSmoothed || traceType == ModelTraceVoxel.ThickSmoothed)
            {
                CalculateInverseCorners(ccubic);
                CalculateSlopes(ccubic);
                CalculateCorners(ccubic);
            }

            return ccubic;
        }

        // WIP.
        public static CubeType[, ,] ReadModelVolmeticAlt(string modelFile, double voxelUnitSize)
        {
            var model = MeshHelper.Load(modelFile, ignoreErrors: true);

            var min = model.Bounds;
            var max = new Point3D(model.Bounds.Location.X + model.Bounds.Size.X, model.Bounds.Location.X + model.Bounds.Size.Z, model.Bounds.Location.Z + model.Bounds.Size.Z);

            //int xCount = xMax - xMin;
            //int yCount = yMax - yMin;
            //int zCount = zMax - zMin;

            //var ccubic = new CubeType[xCount, yCount, zCount];
            var ccubic = new CubeType[0, 0, 0];
            var blockDict = new Dictionary<Point3D, byte[]>();

            #region basic ray trace of every individual triangle.

            foreach (GeometryModel3D gm in model.Children)
            {
                var g = gm.Geometry as MeshGeometry3D;

                for (int t = 0; t < g.TriangleIndices.Count; t += 3)
                {
                    var p1 = g.Positions[g.TriangleIndices[t]];
                    var p2 = g.Positions[g.TriangleIndices[t + 1]];
                    var p3 = g.Positions[g.TriangleIndices[t + 2]];

                    var minBound = MeshHelper.Min(p1, p2, p3).Floor();
                    var maxBound = MeshHelper.Max(p1, p2, p3).Ceiling();

                    //for (var y = minBound.Y; y < maxBound.Y; y++)
                    //{
                    //    for (var z = minBound.Z; z < maxBound.Z; z++)
                    //    {
                    //        var r1 = new Point3D(xMin, y, z);
                    //        var r2 = new Point3D(xMax, y, z);

                    //        Point3D intersect;
                    //        if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r1, r2, out intersect)) // Ray
                    //        {
                    //            //var blockPoint = intersect.Round();
                    //            //var cornerHit = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };

                    //            //if (!blockDict.ContainsKey(blockPoint))
                    //            //    blockDict[blockPoint] = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    //            //if (Math.Round(intersect.X) - intersect.X < 0 && Math.Round(intersect.Y) - intersect.Y < 0 && Math.Round(intersect.Z) - intersect.Z < 0)
                    //            //    cornerHit = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X < 0 && Math.Round(intersect.Y) - intersect.Y > 0 && Math.Round(intersect.Z) - intersect.Z < 0)
                    //            //    cornerHit = new byte[] { 0, 1, 0, 0, 0, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X < 0 && Math.Round(intersect.Y) - intersect.Y < 0 && Math.Round(intersect.Z) - intersect.Z > 0)
                    //            //    cornerHit = new byte[] { 0, 0, 1, 0, 0, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X < 0 && Math.Round(intersect.Y) - intersect.Y > 0 && Math.Round(intersect.Z) - intersect.Z > 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X > 0 && Math.Round(intersect.Y) - intersect.Y < 0 && Math.Round(intersect.Z) - intersect.Z < 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 0, 1, 0, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X > 0 && Math.Round(intersect.Y) - intersect.Y > 0 && Math.Round(intersect.Z) - intersect.Z < 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X > 0 && Math.Round(intersect.Y) - intersect.Y < 0 && Math.Round(intersect.Z) - intersect.Z > 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 0, 0, 0, 1, 0 };
                    //            //else if (Math.Round(intersect.X) - intersect.X > 0 && Math.Round(intersect.Y) - intersect.Y > 0 && Math.Round(intersect.Z) - intersect.Z > 0)
                    //            //    cornerHit = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }; 

                    //            //blockDict[blockPoint]=[int(bool(a+b)) for a,b in zip(blockDict[blockPoint],cornerHit)]


                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //        else if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r2, r1, out intersect)) // Reverse Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //    }
                    //}

                    //for (var x = minBound.X; x < maxBound.X; x++)
                    //{
                    //    for (var z = minBound.Z; z < maxBound.Z; z++)
                    //    {
                    //        var r1 = new Point3D(x, yMin, z);
                    //        var r2 = new Point3D(x, yMax, z);

                    //        Point3D intersect;
                    //        if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r1, r2, out intersect)) // Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //        else if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r2, r1, out intersect)) // Reverse Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //    }
                    //}

                    //for (var x = minBound.X; x < maxBound.X; x++)
                    //{
                    //    for (var y = minBound.Y; y < maxBound.Y; y++)
                    //    {
                    //        var r1 = new Point3D(x, y, zMin);
                    //        var r2 = new Point3D(x, y, zMax);

                    //        Point3D intersect;
                    //        if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r1, r2, out intersect)) // Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //        else if (MeshHelper.RayIntersetTriangle(p1, p2, p3, r2, r1, out intersect)) // Reverse Ray
                    //        {
                    //            ccubic[(int)Math.Floor(intersect.X) - xMin, (int)Math.Floor(intersect.Y) - yMin, (int)Math.Floor(intersect.Z) - zMin] = CubeType.Cube;
                    //        }
                    //    }
                    //}
                }
            }

            #endregion

            return ccubic;
        }

        public static void CrawlExterior(CubeType[, ,] cubic)
        {
            var xMax = cubic.GetLength(0);
            var yMax = cubic.GetLength(1);
            var zMax = cubic.GetLength(2);
            var list = new Queue<Vector3I>();

            // Add basic check points from the corner blocks.
            if (cubic[0, 0, 0] == CubeType.None)
                list.Enqueue(new Vector3I(0, 0, 0));
            if (cubic[xMax - 1, 0, 0] == CubeType.None)
                list.Enqueue(new Vector3I(xMax - 1, 0, 0));
            if (cubic[0, yMax - 1, 0] == CubeType.None)
                list.Enqueue(new Vector3I(0, yMax - 1, 0));
            if (cubic[0, 0, zMax - 1] == CubeType.None)
                list.Enqueue(new Vector3I(0, 0, zMax - 1));
            if (cubic[xMax - 1, yMax - 1, 0] == CubeType.None)
                list.Enqueue(new Vector3I(xMax - 1, yMax - 1, 0));
            if (cubic[0, yMax - 1, zMax - 1] == CubeType.None)
                list.Enqueue(new Vector3I(0, yMax - 1, zMax - 1));
            if (cubic[xMax - 1, 0, zMax - 1] == CubeType.None)
                list.Enqueue(new Vector3I(xMax - 1, 0, zMax - 1));
            if (cubic[xMax - 1, yMax - 1, zMax - 1] == CubeType.None)
                list.Enqueue(new Vector3I(xMax - 1, yMax - 1, zMax - 1));

            while (list.Count > 0)
            {
                var item = list.Dequeue();

                if (cubic[item.X, item.Y, item.Z] == CubeType.None)
                {
                    cubic[item.X, item.Y, item.Z] = CubeType.Exterior;

                    if (item.X - 1 >= 0 && item.Y >= 0 && item.Z >= 0 && item.X - 1 < xMax && item.Y < yMax && item.Z < zMax && cubic[item.X - 1, item.Y, item.Z] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X - 1, item.Y, item.Z));
                    }
                    if (item.X >= 0 && item.Y - 1 >= 0 && item.Z >= 0 && item.X < xMax && item.Y - 1 < yMax && item.Z < zMax && cubic[item.X, item.Y - 1, item.Z] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X, item.Y - 1, item.Z));
                    }
                    if (item.X >= 0 && item.Y >= 0 && item.Z - 1 >= 0 && item.X < xMax && item.Y < yMax && item.Z - 1 < zMax && cubic[item.X, item.Y, item.Z - 1] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X, item.Y, item.Z - 1));
                    }
                    if (item.X + 1 >= 0 && item.Y >= 0 && item.Z >= 0 && item.X + 1 < xMax && item.Y < yMax && item.Z < zMax && cubic[item.X + 1, item.Y, item.Z] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X + 1, item.Y, item.Z));
                    }
                    if (item.X >= 0 && item.Y + 1 >= 0 && item.Z >= 0 && item.X < xMax && item.Y + 1 < yMax && item.Z < zMax && cubic[item.X, item.Y + 1, item.Z] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X, item.Y + 1, item.Z));
                    }
                    if (item.X >= 0 && item.Y >= 0 && item.Z + 1 >= 0 && item.X < xMax && item.Y < yMax && item.Z + 1 < zMax && cubic[item.X, item.Y, item.Z + 1] == CubeType.None)
                    {
                        list.Enqueue(new Vector3I(item.X, item.Y, item.Z + 1));
                    }
                }
            }

            // switch values around to work with current enum logic.
            for (var x = 0; x < xMax; x++)
            {
                for (var y = 0; y < yMax; y++)
                {
                    for (var z = 0; z < zMax; z++)
                    {
                        if (cubic[x, y, z] == CubeType.None)
                            cubic[x, y, z] = CubeType.Interior;
                        else if (cubic[x, y, z] == CubeType.Exterior)
                            cubic[x, y, z] = CubeType.None;
                    }
                }
            }
        }

        public static Dictionary<CubeType, int> CountCubic(CubeType[, ,] cubic)
        {
            var assetCount = new Dictionary<CubeType, int>();

            for (var x = 0; x < cubic.GetLength(0); x++)
            {
                for (var y = 0; y < cubic.GetLength(1); y++)
                {
                    for (var z = 0; z < cubic.GetLength(2); z++)
                    {
                        if (assetCount.ContainsKey(cubic[x, y, z]))
                        {
                            assetCount[cubic[x, y, z]]++;
                        }
                        else
                        {
                            assetCount.Add(cubic[x, y, z], 1);
                        }
                    }
                }
            }

            return assetCount;
        }

        #endregion


        /*  Official SE orientation.
                  (top/up)+Y|   /-Z(forward/front)
                            |  /
                            | /
               -X(left)     |/       +X(right)
               -------------+-----------------
                           /|
                          / |
                         /  |
                (back)+Z/   |-Y(bottom/down)
        */

        #region CalculateSlopes

        public static void CalculateSlopes(CubeType[, ,] ccubic)
        {
            var xCount = ccubic.GetLength(0);
            var yCount = ccubic.GetLength(1);
            var zCount = ccubic.GetLength(2);

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        if (ccubic[x, y, z] == CubeType.None)
                        {
                            if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, 1, 1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeCenterFrontTop;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, 1, 0))
                            {
                                ccubic[x, y, z] = CubeType.SlopeLeftFrontCenter;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, 1, 0))
                            {
                                ccubic[x, y, z] = CubeType.SlopeRightFrontCenter;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, 1, -1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeCenterFrontBottom;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeLeftCenterTop;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, 0, 1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeRightCenterTop;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, -1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeLeftCenterBottom;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, 0, -1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeRightCenterBottom;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeCenterBackTop;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, -1, -1, 0))
                            {
                                ccubic[x, y, z] = CubeType.SlopeLeftBackCenter;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 1, -1, 0))
                            {
                                ccubic[x, y, z] = CubeType.SlopeRightBackCenter;
                            }
                            else if (CheckAdjacentCubic(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, -1))
                            {
                                ccubic[x, y, z] = CubeType.SlopeCenterBackBottom;
                            }
                        }
                    }
                }
            }

        }

        #endregion

        #region CalculateCorners

        public static void CalculateCorners(CubeType[, ,] ccubic)
        {
            var xCount = ccubic.GetLength(0);
            var yCount = ccubic.GetLength(1);
            var zCount = ccubic.GetLength(2);

            for (var x = 0; x < xCount; x++)
            {
                for (var y = 0; y < yCount; y++)
                {
                    for (var z = 0; z < zCount; z++)
                    {
                        if (ccubic[x, y, z] == CubeType.None)
                        {
                            if (CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftFrontCenter, -1, 0, 0, CubeType.SlopeCenterFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftFrontCenter, 0, +1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.SlopeLeftFrontCenter) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.SlopeLeftCenterTop, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.SlopeLeftFrontCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerLeftFrontTop;
                            }
                            else if (CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightFrontCenter, +1, 0, 0, CubeType.SlopeCenterFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightFrontCenter, 0, +1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.SlopeRightFrontCenter) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.SlopeRightCenterTop, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.SlopeRightFrontCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerRightFrontTop;
                            }
                            else if (CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftFrontCenter, -1, 0, 0, CubeType.SlopeCenterFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftFrontCenter, 0, +1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.SlopeLeftFrontCenter) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.SlopeLeftCenterBottom, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.SlopeLeftFrontCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerLeftFrontBottom;
                            }
                            else if (CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightFrontCenter, +1, 0, 0, CubeType.SlopeCenterFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightFrontCenter, 0, +1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.SlopeRightFrontCenter) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.SlopeRightCenterBottom, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.SlopeRightFrontCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerRightFrontBottom;
                            }
                            else if (CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftBackCenter, -1, 0, 0, CubeType.SlopeCenterBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeLeftBackCenter, 0, -1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.SlopeLeftCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.SlopeLeftBackCenter) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.SlopeLeftCenterTop, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.SlopeLeftBackCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerLeftBackTop;
                            }
                            else if (CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightBackCenter, +1, 0, 0, CubeType.SlopeCenterBackTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, +1, CubeType.SlopeRightBackCenter, 0, -1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.SlopeRightCenterTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.SlopeRightBackCenter) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.SlopeRightCenterTop, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.SlopeRightBackCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerRightBackTop;
                            }
                            else if (CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftBackCenter, -1, 0, 0, CubeType.SlopeCenterBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeLeftBackCenter, 0, -1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackBottom, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.SlopeLeftBackCenter) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.SlopeLeftCenterBottom, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.SlopeLeftBackCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerLeftBackBottom;
                            }
                            else if (CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightBackCenter, +1, 0, 0, CubeType.SlopeCenterBackBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, 0, -1, CubeType.SlopeRightBackCenter, 0, -1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.SlopeRightCenterBottom) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.SlopeRightBackCenter) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.SlopeRightCenterBottom, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic2(ccubic, x, y, z, xCount, yCount, zCount, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.SlopeRightBackCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerRightBackBottom;
                            }


                            // ########### Triplet checks
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.SlopeRightCenterBottom, 0, 0, -1, CubeType.InverseCornerLeftFrontTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontTop, 0, -1, 0, CubeType.InverseCornerLeftFrontTop, 0, 0, -1, CubeType.SlopeRightBackCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerRightBackBottom;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                  CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                  CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.SlopeLeftCenterTop, 0, 0, +1, CubeType.InverseCornerRightBackBottom) ||
                                  CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackBottom, 0, +1, 0, CubeType.InverseCornerRightBackBottom, 0, 0, +1, CubeType.SlopeLeftFrontCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerLeftFrontTop;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackBottom, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.SlopeLeftCenterBottom, 0, 0, -1, CubeType.InverseCornerRightFrontTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontTop, 0, -1, 0, CubeType.InverseCornerRightFrontTop, 0, 0, -1, CubeType.SlopeLeftBackCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerLeftBackBottom;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontTop, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.SlopeRightCenterTop, 0, 0, +1, CubeType.InverseCornerLeftBackBottom) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackBottom, 0, +1, 0, CubeType.InverseCornerLeftBackBottom, 0, 0, +1, CubeType.SlopeRightFrontCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerRightFrontTop;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.SlopeRightCenterBottom, 0, 0, -1, CubeType.InverseCornerLeftBackTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftBackTop, 0, +1, 0, CubeType.InverseCornerLeftBackTop, 0, 0, -1, CubeType.SlopeRightFrontCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerRightFrontBottom;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.SlopeLeftCenterTop, 0, 0, +1, CubeType.InverseCornerRightFrontBottom) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightFrontBottom, 0, -1, 0, CubeType.InverseCornerRightFrontBottom, 0, 0, +1, CubeType.SlopeLeftBackCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerLeftBackTop;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.SlopeCenterFrontBottom, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.SlopeLeftCenterBottom, 0, 0, -1, CubeType.InverseCornerRightBackTop) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.InverseCornerRightBackTop, 0, +1, 0, CubeType.InverseCornerRightBackTop, 0, 0, -1, CubeType.SlopeLeftFrontCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerLeftFrontBottom;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.SlopeCenterBackTop, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.SlopeRightCenterTop, 0, 0, +1, CubeType.InverseCornerLeftFrontBottom) ||
                                CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.InverseCornerLeftFrontBottom, 0, -1, 0, CubeType.InverseCornerLeftFrontBottom, 0, 0, +1, CubeType.SlopeRightBackCenter))
                            {
                                ccubic[x, y, z] = CubeType.NormalCornerRightBackTop;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region CalculateInverseCorners

        public static void CalculateInverseCorners(CubeType[, ,] ccubic)
        {
            var xCount = ccubic.GetLength(0);
            var yCount = ccubic.GetLength(1);
            var zCount = ccubic.GetLength(2);

            for (var x = 0; x < xCount; x++)
            {
                for (var y = 0; y < yCount; y++)
                {
                    for (var z = 0; z < zCount; z++)
                    {
                        if (ccubic[x, y, z] == CubeType.None)
                        {
                            if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.Cube, 0, -1, 0, CubeType.Cube, 0, 0, -1, CubeType.Cube))
                            {
                                ccubic[x, y, z] = CubeType.InverseCornerLeftFrontTop;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.Cube, 0, +1, 0, CubeType.Cube, 0, 0, -1, CubeType.Cube))
                            {
                                ccubic[x, y, z] = CubeType.InverseCornerRightBackTop;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.Cube, 0, +1, 0, CubeType.Cube, 0, 0, -1, CubeType.Cube))
                            {
                                ccubic[x, y, z] = CubeType.InverseCornerLeftBackTop;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.Cube, 0, -1, 0, CubeType.Cube, 0, 0, -1, CubeType.Cube))
                            {
                                ccubic[x, y, z] = CubeType.InverseCornerRightFrontTop;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.Cube, 0, +1, 0, CubeType.Cube, 0, 0, +1, CubeType.Cube))
                            {
                                ccubic[x, y, z] = CubeType.InverseCornerLeftBackBottom;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.Cube, 0, +1, 0, CubeType.Cube, 0, 0, +1, CubeType.Cube))
                            {
                                ccubic[x, y, z] = CubeType.InverseCornerRightBackBottom;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, +1, 0, 0, CubeType.Cube, 0, -1, 0, CubeType.Cube, 0, 0, +1, CubeType.Cube))
                            {
                                ccubic[x, y, z] = CubeType.InverseCornerLeftFrontBottom;
                            }
                            else if (CheckAdjacentCubic3(ccubic, x, y, z, xCount, yCount, zCount, -1, 0, 0, CubeType.Cube, 0, -1, 0, CubeType.Cube, 0, 0, +1, CubeType.Cube))
                            {
                                ccubic[x, y, z] = CubeType.InverseCornerRightFrontBottom;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region CheckAdjacentCubic

        private static bool IsValidRange(int x, int y, int z, int xCount, int yCount, int zCount, int xDelta, int yDelta, int zDelta)
        {
            if (x + xDelta >= 0 && x + xDelta < xCount
            && y + yDelta >= 0 && y + yDelta < yCount
            && z + zDelta >= 0 && z + zDelta < zCount)
            {
                return true;
            }

            return false;
        }

        private static bool CheckAdjacentCubic(CubeType[, ,] ccubic, int x, int y, int z, int xCount, int yCount, int zCount, int xDelta, int yDelta, int zDelta)
        {
            if (ccubic[x, y, z] == CubeType.None && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta, yDelta, zDelta))
            {
                if (xDelta != 0 && ccubic[x + xDelta, y, z] == CubeType.Cube &&
                    yDelta != 0 && ccubic[x, y + yDelta, z] == CubeType.Cube &&
                    zDelta == 0)
                {
                    return true;
                }

                if (xDelta != 0 && ccubic[x + xDelta, y, z] == CubeType.Cube &&
                    yDelta == 0 &&
                    zDelta != 0 && ccubic[x, y, z + zDelta] == CubeType.Cube)
                {
                    return true;
                }

                if (xDelta == 0 &&
                    yDelta != 0 && ccubic[x, y + yDelta, z] == CubeType.Cube &&
                    zDelta != 0 && ccubic[x, y, z + zDelta] == CubeType.Cube)
                {
                    return true;
                }

                if (xDelta != 0 && ccubic[x + xDelta, y, z] == CubeType.Cube &&
                    yDelta != 0 && ccubic[x, y + yDelta, z] == CubeType.Cube &&
                    zDelta != 0 && ccubic[x, y, z + zDelta] == CubeType.Cube)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CheckAdjacentCubic1(CubeType[, ,] ccubic, int x, int y, int z, int xCount, int yCount, int zCount,
           int xDelta, int yDelta, int zDelta, CubeType cubeType)
        {
            if (IsValidRange(x, y, z, xCount, yCount, zCount, xDelta, yDelta, zDelta))
            {
                return ccubic[x + xDelta, y + yDelta, z + zDelta] == cubeType;
            }

            return false;
        }

        private static bool CheckAdjacentCubic2(CubeType[, ,] ccubic, int x, int y, int z, int xCount, int yCount, int zCount,
            int xDelta1, int yDelta1, int zDelta1, CubeType cubeType1,
            int xDelta2, int yDelta2, int zDelta2, CubeType cubeType2)
        {
            if (IsValidRange(x, y, z, xCount, yCount, zCount, xDelta1, yDelta1, zDelta1) && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta2, yDelta2, zDelta2))
            {
                return ccubic[x + xDelta1, y + yDelta1, z + zDelta1] == cubeType1 && ccubic[x + xDelta2, y + yDelta2, z + zDelta2] == cubeType2;
            }

            return false;
        }

        private static bool CheckAdjacentCubic3(CubeType[, ,] ccubic, int x, int y, int z, int xCount, int yCount, int zCount,
            int xDelta1, int yDelta1, int zDelta1, CubeType cubeType1,
            int xDelta2, int yDelta2, int zDelta2, CubeType cubeType2,
            int xDelta3, int yDelta3, int zDelta3, CubeType cubeType3)
        {
            if (IsValidRange(x, y, z, xCount, yCount, zCount, xDelta1, yDelta1, zDelta1)
                && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta2, yDelta2, zDelta2)
                && IsValidRange(x, y, z, xCount, yCount, zCount, xDelta3, yDelta3, zDelta3))
            {
                return ccubic[x + xDelta1, y + yDelta1, z + zDelta1] == cubeType1
                    && ccubic[x + xDelta2, y + yDelta2, z + zDelta2] == cubeType2
                    && ccubic[x + xDelta3, y + yDelta3, z + zDelta3] == cubeType3;
            }

            return false;
        }

        #endregion

        #region BuildStructureFromCubic

        private static void BuildStructureFromCubic(MyObjectBuilder_CubeGrid entity, CubeType[, ,] ccubic, SubtypeId blockType, SubtypeId slopeBlockType, SubtypeId cornerBlockType, SubtypeId inverseCornerBlockType)
        {
            var xCount = ccubic.GetLength(0);
            var yCount = ccubic.GetLength(1);
            var zCount = ccubic.GetLength(2);

            for (var x = 0; x < xCount; x++)
            {
                for (var y = 0; y < yCount; y++)
                {
                    for (var z = 0; z < zCount; z++)
                    {
                        if (ccubic[x, y, z] != CubeType.None && ccubic[x, y, z] != CubeType.Interior)
                        {
                            MyObjectBuilder_CubeBlock newCube;
                            entity.CubeBlocks.Add(newCube = new MyObjectBuilder_CubeBlock());

                            if (ccubic[x, y, z].ToString().StartsWith("Cube"))
                            {
                                newCube.SubtypeName = blockType.ToString();
                            }
                            else if (ccubic[x, y, z].ToString().StartsWith("Slope"))
                            {
                                newCube.SubtypeName = slopeBlockType.ToString();
                            }
                            else if (ccubic[x, y, z].ToString().StartsWith("NormalCorner"))
                            {
                                newCube.SubtypeName = cornerBlockType.ToString();
                            }
                            else if (ccubic[x, y, z].ToString().StartsWith("InverseCorner"))
                            {
                                newCube.SubtypeName = inverseCornerBlockType.ToString();
                            }

                            newCube.EntityId = 0;
                            newCube.BlockOrientation = SpaceEngineersAPI.GetCubeOrientation(ccubic[x, y, z]);
                            newCube.Min = new VRageMath.Vector3I(x, y, z);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
