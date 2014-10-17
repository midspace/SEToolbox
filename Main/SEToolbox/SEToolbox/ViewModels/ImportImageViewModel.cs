namespace SEToolbox.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.ImageLibrary;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using Res = SEToolbox.Properties.Resources;

    public class ImportImageViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly ImportImageModel _dataModel;

        private bool? _closeResult;
        private Image _sourceImage;
        private BitmapImage _newImage;
        private bool _isBusy;

        #endregion

        #region Constructors

        public ImportImageViewModel(BaseViewModel parentViewModel, ImportImageModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>)
        {
        }

        public ImportImageViewModel(BaseViewModel parentViewModel, ImportImageModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            _dialogService = dialogService;
            _openFileDialogFactory = openFileDialogFactory;
            _dataModel = dataModel;
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties

        public ICommand BrowseImageCommand
        {
            get { return new DelegateCommand(BrowseImageExecuted, BrowseImageCanExecute); }
        }

        public ICommand SetToOriginalSizeCommand
        {
            get { return new DelegateCommand(SetToOriginalSizeExecuted, SetToOriginalSizeCanExecute); }
        }

        public ICommand CreateCommand
        {
            get { return new DelegateCommand(CreateExecuted, CreateCanExecute); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(CancelExecuted, CancelCanExecute); }
        }

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

        public string Filename
        {
            get { return _dataModel.Filename; }

            set
            {
                _dataModel.Filename = value;
                FilenameChanged();
            }
        }

        public bool IsValidImage
        {
            get { return _dataModel.IsValidImage; }

            set { _dataModel.IsValidImage = value; }
        }

        public Size OriginalImageSize
        {
            get { return _dataModel.OriginalImageSize; }

            set { _dataModel.OriginalImageSize = value; }
        }

        public BindableSizeModel NewImageSize
        {
            get { return _dataModel.NewImageSize; }

            set
            {
                _dataModel.NewImageSize = value;
                ProcessImage();
            }
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

        public ImportImageClassType ClassType
        {
            get { return _dataModel.ClassType; }
            set { _dataModel.ClassType = value; }
        }

        public ImportArmorType ArmorType
        {
            get { return _dataModel.ArmorType; }
            set { _dataModel.ArmorType = value; }
        }

        public BitmapImage NewImage
        {
            get { return _newImage; }

            set
            {
                if (value != _newImage)
                {
                    _newImage = value;
                    RaisePropertyChanged(() => NewImage);
                }
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

        #endregion

        #region methods

        public bool BrowseImageCanExecute()
        {
            return true;
        }

        public void BrowseImageExecuted()
        {
            IsValidImage = false;

            var openFileDialog = _openFileDialogFactory();
            openFileDialog.Filter = Res.DialogImportImageFilter;
            openFileDialog.Title = Res.DialogImportImageTitle;

            // Open the dialog
            var result = _dialogService.ShowOpenFileDialog(this, openFileDialog);

            if (result == DialogResult.OK)
            {
                Filename = openFileDialog.FileName;
            }
        }

        private void FilenameChanged()
        {
            ProcessFilename(Filename);
        }

        public bool SetToOriginalSizeCanExecute()
        {
            return IsValidImage;
        }

        public void SetToOriginalSizeExecuted()
        {
            NewImageSize.Height = _sourceImage.Height;
            NewImageSize.Width = _sourceImage.Width;
        }

        public bool CreateCanExecute()
        {
            return IsValidImage;
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
            IsBusy = true;

            if (File.Exists(filename))
            {
                // TODO: validate file is a real image.

                // TODO: read image properties.

                if (_sourceImage != null)
                {
                    _sourceImage.Dispose();
                }

                _sourceImage = Image.FromFile(filename);
                OriginalImageSize = new Size(_sourceImage.Width, _sourceImage.Height);

                NewImageSize = new BindableSizeModel(_sourceImage.Width, _sourceImage.Height);
                NewImageSize.PropertyChanged += (sender, e) => ProcessImage();

                // Figure out where the Character is facing, and plant the new constrcut right in front, by "10" units, facing the Character.
                var vector = new BindableVector3DModel(_dataModel.CharacterPosition.Forward).Vector3D;
                vector.Normalize();
                vector = Vector3D.Multiply(vector, 10);
                Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(_dataModel.CharacterPosition.Position).Point3D, vector));
                Forward = new BindableVector3DModel(_dataModel.CharacterPosition.Forward).Negate();
                Up = new BindableVector3DModel(_dataModel.CharacterPosition.Up);

                ClassType = ImportImageClassType.SmallShip;
                ArmorType = ImportArmorType.Light;

                IsValidImage = true;
            }
            else
            {
                IsValidImage = false;
                Position = new BindablePoint3DModel(0, 0, 0);
                OriginalImageSize = new Size(0, 0);
                NewImageSize = new BindableSizeModel(0, 0);
            }

            IsBusy = false;
        }

        private void ProcessImage()
        {
            if (_sourceImage != null)
            {
                var image = ImageHelper.ResizeImage(_sourceImage, NewImageSize.Size);

                if (image != null)
                {
                    NewImage = ImageHelper.ConvertBitmapToBitmapImage(image);

                    //ImageHelper.SavePng(@"C:\temp\test.png", image);
                }
                else
                {
                    NewImage = null;
                }
            }
            else
            {
                NewImage = null;
            }
        }

        public MyObjectBuilder_CubeGrid BuildEntity()
        {
            var entity = new MyObjectBuilder_CubeGrid
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                Skeleton = new System.Collections.Generic.List<BoneInfo>(),
                LinearVelocity = new VRageMath.Vector3(0, 0, 0),
                AngularVelocity = new VRageMath.Vector3(0, 0, 0)
            };

            var blockPrefix = "";
            switch (ClassType)
            {
                case ImportImageClassType.SmallShip:
                    entity.GridSizeEnum = MyCubeSize.Small;
                    blockPrefix += "Small";
                    entity.IsStatic = false;
                    break;

                case ImportImageClassType.LargeShip:
                    entity.GridSizeEnum = MyCubeSize.Large;
                    blockPrefix += "Large";
                    entity.IsStatic = false;
                    break;

                case ImportImageClassType.Station:
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
            }

            entity.PositionAndOrientation = new MyPositionAndOrientation
            {
                // TODO: reposition based scale.
                Position = Position.ToVector3(),
                Forward = Forward.ToVector3(),
                Up = Up.ToVector3()
            };

            // Large|BlockArmor|Corner
            // Large|RoundArmor_|Corner
            // Large|HeavyBlockArmor|Block,
            // Small|BlockArmor|Slope,
            // Small|HeavyBlockArmor|Corner,

            entity.CubeBlocks = new System.Collections.Generic.List<MyObjectBuilder_CubeBlock>();
            var image = ImageHelper.ResizeImage(_sourceImage, NewImageSize.Size);

            using (var palatteImage = new Bitmap(image))
            {
                // Optimal order load. from grid coordinate (0,0,0) and up.
                for (var x = palatteImage.Width - 1; x >= 0; x--)
                {
                    for (var y = palatteImage.Height - 1; y >= 0; y--)
                    {
                        const int z = 0;
                        var color = palatteImage.GetPixel(x, y);

                        // Specifically ignore anything with less than half "Transparent" Alpha.
                        if (color.A > 0x7F)
                        {
                            // Parse the string through the Enumeration to check that the 'subtypeid' is still valid in the game engine.
                            var armor = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "Block");

                            MyObjectBuilder_CubeBlock newCube;
                            entity.CubeBlocks.Add(newCube = new MyObjectBuilder_CubeBlock());
                            newCube.SubtypeName = armor.ToString();
                            newCube.EntityId = 0;
                            newCube.BlockOrientation = Modelling.GetCubeOrientation(CubeType.Cube);
                            newCube.Min = new VRageMath.Vector3I(palatteImage.Width - x - 1, palatteImage.Height - y - 1, z);
                            newCube.ColorMaskHSV = color.ToSandboxHsvColor();
                        }
                    }
                }
            }

            return entity;
        }

        #endregion
    }
}
