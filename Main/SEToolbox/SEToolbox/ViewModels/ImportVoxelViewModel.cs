namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using VRageMath;
    using Res = SEToolbox.Properties.Resources;

    public class ImportVoxelViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly ImportVoxelModel _dataModel;

        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region Constructors

        public ImportVoxelViewModel(BaseViewModel parentViewModel, ImportVoxelModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>)
        {
        }

        public ImportVoxelViewModel(BaseViewModel parentViewModel, ImportVoxelModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            this._dialogService = dialogService;
            this._openFileDialogFactory = openFileDialogFactory;
            this._dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties

        public ICommand BrowseVoxelCommand
        {
            get
            {
                return new DelegateCommand(new Action(BrowseVoxelExecuted), new Func<bool>(BrowseVoxelCanExecute));
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
            }
        }

        public string SourceFile
        {
            get
            {
                return this._dataModel.SourceFile;
            }

            set
            {
                this._dataModel.SourceFile = value;
                this.SourceFileChanged();
            }
        }

        public bool IsValidVoxelFile
        {
            get
            {
                return this._dataModel.IsValidVoxelFile;
            }

            set
            {
                this._dataModel.IsValidVoxelFile = value;
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

        public bool IsStockVoxel
        {
            get
            {
                return this._dataModel.IsStockVoxel;
            }

            set
            {
                this._dataModel.IsStockVoxel = value;
            }
        }

        public bool IsFileVoxel
        {
            get
            {
                return this._dataModel.IsFileVoxel;
            }

            set
            {
                this._dataModel.IsFileVoxel = value;
            }
        }

        public string StockVoxel
        {
            get
            {
                return this._dataModel.StockVoxel;
            }

            set
            {
                this._dataModel.StockVoxel = value;
            }
        }

        public List<string> StockVoxelFileList
        {
            get
            {
                return this._dataModel.StockVoxelFileList;
            }
        }

        public ObservableCollection<MaterialSelectionModel> MaterialsCollection
        {
            get
            {
                return this._dataModel.MaterialsCollection;
            }
        }

        public MaterialSelectionModel StockMaterial
        {
            get
            {
                return this._dataModel.StockMaterial;
            }

            set
            {
                this._dataModel.StockMaterial = value;
            }
        }

        #endregion

        #region methods

        public bool BrowseVoxelCanExecute()
        {
            return true;
        }

        public void BrowseVoxelExecuted()
        {
            this.BrowseVoxel();
        }

        public bool CreateCanExecute()
        {
            return (this.IsValidVoxelFile && this.IsFileVoxel) || (this.IsStockVoxel && this.StockVoxel != null);
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

        #region heleprs

        private void BrowseVoxel()
        {
            this.IsValidVoxelFile = false;
            var openFileDialog = _openFileDialogFactory();
            openFileDialog.Filter = Res.DialogImportVoxelFilter;
            openFileDialog.Title = Res.DialogImportVoxelTitle;

            // Open the dialog
            var result = _dialogService.ShowOpenFileDialog(this.OwnerViewModel, openFileDialog);

            if (result == DialogResult.OK)
            {
                this.SourceFile = openFileDialog.FileName;
            }
        }

        private void SourceFileChanged()
        {
            this.ProcessSourceFile(this.SourceFile);
        }

        private void ProcessSourceFile(string filename)
        {
            this.IsBusy = true;

            if (File.Exists(filename))
            {
                this.IsValidVoxelFile = MyVoxelMap.IsVoxelMapFile(filename);
                this.IsFileVoxel = true;
            }
            else
            {
                this.IsValidVoxelFile = false;
                this.IsFileVoxel = false;
            }

            this.IsBusy = false;
        }

        public MyObjectBuilder_EntityBase BuildEntity()
        {
            var asteroidCenter = new Vector3();
            var asteroidSize = new Vector3I();

            string originalFile = null;
            if (this.IsStockVoxel)
            {
                var stockfile = Path.Combine(Path.Combine(ToolboxUpdater.GetApplicationFilePath(), @"Content\VoxelMaps"), this.StockVoxel + ".vox");

                if (this.StockMaterial == null || this.StockMaterial.Value == null)
                {
                    this.SourceFile = stockfile;
                    originalFile = this.SourceFile;
                    var asteroid = new MyVoxelMap();
                    asteroid.Load(stockfile, SpaceEngineersAPI.GetMaterialName(0), false);
                    asteroidCenter = asteroid.ContentCenter;
                    asteroidSize = asteroid.ContentSize;
                }
                else
                {
                    var asteroid = new MyVoxelMap();
                    asteroid.Load(stockfile, this.StockMaterial.Value);
                    asteroid.ForceBaseMaterial(this.StockMaterial.Value);
                    this.SourceFile = TempfileUtil.NewFilename();
                    asteroid.Save(this.SourceFile);
                    originalFile = this.StockVoxel + ".vox";
                    asteroidCenter = asteroid.ContentCenter;
                    asteroidSize = asteroid.ContentSize;
                }
            }
            else if (this.IsFileVoxel)
            {
                originalFile = this.SourceFile;

                var asteroid = new MyVoxelMap();
                asteroid.Load(this.SourceFile, SpaceEngineersAPI.GetMaterialName(0), false);
                asteroidCenter = asteroid.ContentCenter;
                asteroidSize = asteroid.ContentSize;
            }

            // automatically number all files, and check for duplicate filenames.
            this.Filename = this.MainViewModel.CreateUniqueVoxelFilename(originalFile);


            // Figure out where the Character is facing, and plant the new constrcut right in front.
            // Calculate the hypotenuse, as it will be the safest distance to place in front.
            double distance = Math.Sqrt(Math.Pow(asteroidSize.X, 2) + Math.Pow(asteroidSize.Y, 2) + Math.Pow(asteroidSize.Z, 2)) / 2;

            var vector = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward).Vector3D;
            vector.Normalize();
            vector = Vector3D.Multiply(vector, distance);
            this.Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(this._dataModel.CharacterPosition.Position).Point3D, vector));
            //this.Forward = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward);
            //this.Up = new BindableVector3DModel(this._dataModel.CharacterPosition.Up);
            this.Forward = new BindableVector3DModel(Vector3.Forward);  // Asteroids currently don't have any orientation.
            this.Up = new BindableVector3DModel(Vector3.Up);


            var entity = new MyObjectBuilder_VoxelMap(this.Position.ToVector3() - asteroidCenter, this.Filename);
            entity.EntityId = SpaceEngineersAPI.GenerateEntityId();
            entity.PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene;
            entity.Filename = this.Filename;

            entity.PositionAndOrientation = new MyPositionAndOrientation()
            {
                Position = this.Position.ToVector3() - asteroidCenter,
                Forward = this.Forward.ToVector3(),
                Up = this.Up.ToVector3()
            };

            return entity;
        }

        #endregion
    }
}
