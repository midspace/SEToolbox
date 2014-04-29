namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using Res = SEToolbox.Properties.Resources;

    public class ImportImageViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private ImportImageModel _dataModel;

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

            this._dialogService = dialogService;
            this._openFileDialogFactory = openFileDialogFactory;
            this._dataModel = dataModel;
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties

        public ICommand BrowseImageCommand
        {
            get
            {
                return new DelegateCommand(new Action(BrowseImageExecuted), new Func<bool>(BrowseImageCanExecute));
            }
        }

        public ICommand SetToOriginalSizeCommand
        {
            get
            {
                return new DelegateCommand(new Action(SetToOriginalSizeExecuted), new Func<bool>(SetToOriginalSizeCanExecute));
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

        public bool IsValidImage
        {
            get
            {
                return this._dataModel.IsValidImage;
            }

            set
            {
                this._dataModel.IsValidImage = value;
            }
        }

        public Size OriginalImageSize
        {
            get
            {
                return this._dataModel.OriginalImageSize;
            }

            set
            {
                this._dataModel.OriginalImageSize = value;
            }
        }

        public BindableSizeModel NewImageSize
        {
            get
            {
                return this._dataModel.NewImageSize;
            }

            set
            {
                this._dataModel.NewImageSize = value;
                this.ProcessImage();
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

        public ImportClassType ClassType
        {
            get
            {
                return this._dataModel.ClassType;
            }

            set
            {
                this._dataModel.ClassType = value;
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

        public BitmapImage NewImage
        {
            get
            {
                return this._newImage;
            }

            set
            {
                if (value != this._newImage)
                {
                    this._newImage = value;
                    this.RaisePropertyChanged(() => NewImage);
                }
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

        #endregion

        #region methods

        public bool BrowseImageCanExecute()
        {
            return true;
        }

        public void BrowseImageExecuted()
        {
            this.IsValidImage = false;

            var openFileDialog = _openFileDialogFactory();
            openFileDialog.Filter = Res.DialogImportImageFilter;
            openFileDialog.Title = Res.DialogImportImageTitle;

            // Open the dialog
            var result = _dialogService.ShowOpenFileDialog(this, openFileDialog);

            if (result == DialogResult.OK)
            {
                this.Filename = openFileDialog.FileName;
            }
        }

        private void FilenameChanged()
        {
            this.ProcessFilename(this.Filename);
        }

        public bool SetToOriginalSizeCanExecute()
        {
            return this.IsValidImage;
        }

        public void SetToOriginalSizeExecuted()
        {
            this.NewImageSize.Height = this._sourceImage.Height;
            this.NewImageSize.Width = this._sourceImage.Width;
        }

        public bool CreateCanExecute()
        {
            return this.IsValidImage;
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
            this.IsBusy = true;

            if (File.Exists(filename))
            {
                // TODO: validate file is a real image.

                // TODO: read image properties.

                if (this._sourceImage != null)
                {
                    this._sourceImage.Dispose();
                }

                this._sourceImage = Image.FromFile(filename);
                this.OriginalImageSize = new Size(this._sourceImage.Width, this._sourceImage.Height);

                this.NewImageSize = new BindableSizeModel(this._sourceImage.Width, this._sourceImage.Height);
                this.NewImageSize.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
                {
                    this.ProcessImage();
                };


                //this.Position = new BindablePoint3DModel(VRageMath.Vector3.Zero);
                //this.Forward = new BindableVector3DModel(VRageMath.Vector3.Forward);
                //this.Up = new BindableVector3DModel(VRageMath.Vector3.Up);


                // Figure out where the Character is facing, and plant the new constrcut right in front, by "10" units, facing the Character.
                var vector = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward).Vector3D;
                vector.Normalize();
                vector = Vector3D.Multiply(vector, 10);
                this.Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(this._dataModel.CharacterPosition.Position).Point3D, vector));
                this.Forward = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward).Negate();
                this.Up = new BindableVector3DModel(this._dataModel.CharacterPosition.Up);

                this.ClassType = ImportClassType.SmallShip;
                this.ArmorType = ImportArmorType.Light;

                this.IsValidImage = true;
            }
            else
            {
                this.IsValidImage = false;
                this.Position = new BindablePoint3DModel(0, 0, 0);
                this.OriginalImageSize = new Size(0, 0);
                this.NewImageSize = new BindableSizeModel(0, 0);
            }

            this.IsBusy = false;
        }

        private void ProcessImage()
        {
            if (this._sourceImage != null)
            {
                var image = ToolboxExtensions.ResizeImage(this._sourceImage, this.NewImageSize.Size);

                if (image != null)
                {
                    this.NewImage = ToolboxExtensions.ConvertBitmapToBitmapImage(image);

                    //ToolboxExtensions.SavePng(@"C:\temp\test.png", image);
                }
                else
                {
                    this.NewImage = null;
                }
            }
            else
            {
                this.NewImage = null;
            }
        }

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

            //double scaleFactor = 2.5;

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
            }

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

            entity.CubeBlocks = new System.Collections.Generic.List<MyObjectBuilder_CubeBlock>();
            var image = ToolboxExtensions.ResizeImage(this._sourceImage, this.NewImageSize.Size);

            using (var palatteImage = new Bitmap(image))
            {
                // Optimal order load. from grid 0,0,0 and out.
                for (var x = palatteImage.Width - 1; x >= 0; x--)
                {
                    for (var y = palatteImage.Height - 1; y >= 0; y--)
                    {
                        var z = 0;
                        var color = palatteImage.GetPixel(x, y);

                        // Specifically ignore anything with "Transparent" Alpha.
                        if (color.A == 0xFF)
                        {
                            // Parse the string through the Enumeration to check that the 'subtypeid' is still valid in the game engine.
                            var armor = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "Block");

                            MyObjectBuilder_CubeBlock newCube;
                            entity.CubeBlocks.Add(newCube = new MyObjectBuilder_CubeBlock());
                            newCube.SubtypeName = armor.ToString();
                            newCube.EntityId = 0;
                            newCube.BlockOrientation = SpaceEngineersAPI.GetCubeOrientation(CubeType.Cube);
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
