namespace SEToolbox.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Properties;
    using SEToolbox.Services;
    using SEToolbox.Support;

    public class ImportImageViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService dialogService;
        private readonly Func<IOpenFileDialog> openFileDialogFactory;
        private ImportImageModel dataModel;

        private bool? closeResult;
        private Image sourceImage;
        private BitmapImage newImage;
        private bool isBusy;

        #endregion

        #region Constructors

        public ImportImageViewModel(BaseViewModel parentViewModel, ImportImageModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), () => ServiceLocator.Resolve<IOpenFileDialog>())
        {
        }

        public ImportImageViewModel(BaseViewModel parentViewModel, ImportImageModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            this.dialogService = dialogService;
            this.openFileDialogFactory = openFileDialogFactory;
            this.dataModel = dataModel;
            this.dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
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
                return this.closeResult;
            }

            set
            {
                this.closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
            }
        }

        public string Filename
        {
            get
            {
                return this.dataModel.Filename;
            }

            set
            {
                this.dataModel.Filename = value;
                this.FilenameChanged();
            }
        }

        public bool IsValidImage
        {
            get
            {
                return this.dataModel.IsValidImage;
            }

            set
            {
                this.dataModel.IsValidImage = value;
            }
        }

        public Size OriginalImageSize
        {
            get
            {
                return this.dataModel.OriginalImageSize;
            }

            set
            {
                this.dataModel.OriginalImageSize = value;
            }
        }

        public BindableSizeModel NewImageSize
        {
            get
            {
                return this.dataModel.NewImageSize;
            }

            set
            {
                this.dataModel.NewImageSize = value;
                this.ProcessImage();
            }
        }

        public BindablePoint3DModel Position
        {
            get
            {
                return this.dataModel.Position;
            }

            set
            {
                this.dataModel.Position = value;
            }
        }

        public BindableVector3DModel Forward
        {
            get
            {
                return this.dataModel.Forward;
            }

            set
            {
                this.dataModel.Forward = value;
            }
        }

        public BindableVector3DModel Up
        {
            get
            {
                return this.dataModel.Up;
            }

            set
            {
                this.dataModel.Up = value;
            }
        }

        public ImportClassType ClassType
        {
            get
            {
                return this.dataModel.ClassType;
            }

            set
            {
                this.dataModel.ClassType = value;
            }
        }

        public ImportArmorType ArmorType
        {
            get
            {
                return this.dataModel.ArmorType;
            }

            set
            {
                this.dataModel.ArmorType = value;
            }
        }

        public BitmapImage NewImage
        {
            get
            {
                return this.newImage;
            }

            set
            {
                if (value != this.newImage)
                {
                    this.newImage = value;
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
                return this.isBusy;
            }

            set
            {
                if (value != this.isBusy)
                {
                    this.isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    if (this.isBusy)
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

            IOpenFileDialog openFileDialog = openFileDialogFactory();
            openFileDialog.Filter = Resources.ImportImageFilter;
            openFileDialog.Title = Resources.ImportImageTitle;

            // Open the dialog
            DialogResult result = dialogService.ShowOpenFileDialog(this, openFileDialog);

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
            this.NewImageSize.Height = this.sourceImage.Height;
            this.NewImageSize.Width = this.sourceImage.Width;
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

                if (this.sourceImage != null)
                {
                    this.sourceImage.Dispose();
                }

                this.sourceImage = Image.FromFile(filename);
                this.OriginalImageSize = new Size(this.sourceImage.Width, this.sourceImage.Height);

                this.NewImageSize = new BindableSizeModel(this.sourceImage.Width, this.sourceImage.Height);
                this.NewImageSize.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
                {
                    this.ProcessImage();
                };


                //this.Position = new BindablePoint3DModel(VRageMath.Vector3.Zero);
                //this.Forward = new BindableVector3DModel(VRageMath.Vector3.Forward);
                //this.Up = new BindableVector3DModel(VRageMath.Vector3.Up);


                // Figure out where the Character is facing, and plant the new constrcut right in front, by "10" units, facing the Character.
                var vector = new BindableVector3DModel(this.dataModel.CharacterPosition.Forward).Vector3D;
                vector.Normalize();
                vector = Vector3D.Multiply(vector, 10);
                this.Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(this.dataModel.CharacterPosition.Position).Point3D, vector));
                this.Forward = new BindableVector3DModel(this.dataModel.CharacterPosition.Forward).Negate();
                this.Up = new BindableVector3DModel(this.dataModel.CharacterPosition.Up);

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
            if (this.sourceImage != null)
            {
                var image = ToolboxExtensions.ResizeImage(this.sourceImage, this.NewImageSize.Size);

                if (image != null)
                {
                    // process colors.
                    image = ToolboxExtensions.OptimizeImagePalette(image);

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
                case ImportArmorType.Heavy: blockPrefix += "HeavyBlock"; break;
                case ImportArmorType.Light: blockPrefix += "Block"; break;
            }

            entity.PositionAndOrientation = new MyPositionAndOrientation()
            {
                // TODO: reposition based scale.
                Position = this.Position.ToVector3(),
                Forward = this.Forward.ToVector3(),
                Up = this.Up.ToVector3()
            };


            // Large|Block|ArmorCorner|Yellow
            // Large|HeavyBlock|ArmorBlock,
            // Small|Block|ArmorBlock,
            // Small|HeavyBlock|ArmorBlock,

            entity.CubeBlocks = new System.Collections.Generic.List<MyObjectBuilder_CubeBlock>();

            MyObjectBuilder_CubeBlock newCube;

            var image = ToolboxExtensions.ResizeImage(this.sourceImage, this.NewImageSize.Size);
            // process colors.
            image = ToolboxExtensions.OptimizeImagePalette(image);

            using (Bitmap palatteImage = new Bitmap(image))
            {
                var palatteNames = ToolboxExtensions.GetPalatteNames();


                //for (int x = 0; x < palatteImage.Width; x++)
                //{
                //    for (int y = 0; y < palatteImage.Height; y++)
                //    {
                //        int z = 0;
                //        var color = palatteImage.GetPixel(x, y);

                //        var cname = palatteNames.First(c => c.Key.A == color.A && c.Key.R == color.R && c.Key.G == color.G && c.Key.B == color.B).Value;
                //        // Parse the string through the Enumeration to check that the 'subtypeid' is still valid in the game engine.
                //        SubtypeId armor = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "ArmorBlock" + cname);

                //        entity.CubeBlocks.Add(newCube = new MyObjectBuilder_CubeBlock());
                //        newCube.SubtypeName = armor.ToString();
                //        newCube.EntityId = 0;
                //        newCube.PersistentFlags = MyPersistentEntityFlags2.None;
                //        SpaceEngineersAPI.SetCubeOrientation(newCube, CubeType.Cube);
                //        //newCube.Min = new VRageMath.Vector3I(x, y, z);
                //        //newCube.Max = new VRageMath.Vector3I(x, y, z);
                //        newCube.Min = new VRageMath.Vector3I(palatteImage.Width - x - 1, palatteImage.Height - y - 1, z);
                //        newCube.Max = new VRageMath.Vector3I(palatteImage.Width - x - 1, palatteImage.Height - y - 1, z);
                //    }
                //}

                // Optimal order load. from grid 0,0,0 and out.
                for (int x = palatteImage.Width - 1; x >= 0; x--)
                {
                    for (int y = palatteImage.Height - 1; y >= 0; y--)
                    {
                        int z = 0;
                        var color = palatteImage.GetPixel(x, y);
                        var armorColor = palatteNames.FirstOrDefault(c => c.Key.A == color.A && c.Key.R == color.R && c.Key.G == color.G && c.Key.B == color.B);

                        // This will specifically ignore the "Transparent" Color, as there is no matching color specified in the palatteNames.
                        if (armorColor.Value != null)
                        {
                            // Parse the string through the Enumeration to check that the 'subtypeid' is still valid in the game engine.
                            SubtypeId armor = (SubtypeId)Enum.Parse(typeof(SubtypeId), blockPrefix + "ArmorBlock" + armorColor.Value);

                            entity.CubeBlocks.Add(newCube = new MyObjectBuilder_CubeBlock());
                            newCube.SubtypeName = armor.ToString();
                            newCube.EntityId = 0;
                            newCube.PersistentFlags = MyPersistentEntityFlags2.None;
                            SpaceEngineersAPI.SetCubeOrientation(newCube, CubeType.Cube);
                            newCube.Min = new VRageMath.Vector3I(palatteImage.Width - x - 1, palatteImage.Height - y - 1, z);
                            newCube.Max = new VRageMath.Vector3I(palatteImage.Width - x - 1, palatteImage.Height - y - 1, z);
                        }
                    }
                }
            }

            return entity;
        }

        #endregion
    }
}
