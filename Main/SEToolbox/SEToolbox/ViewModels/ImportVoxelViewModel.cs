namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Properties;
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
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), () => ServiceLocator.Resolve<IOpenFileDialog>())
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

        public bool IsCustomVoxel
        {
            get
            {
                return this._dataModel.IsCustomVoxel;
            }

            set
            {
                this._dataModel.IsCustomVoxel = value;
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

        public string CustomVoxel
        {
            get
            {
                return this._dataModel.CustomVoxel;
            }

            set
            {
                this._dataModel.CustomVoxel = value;
            }
        }

        public List<string> StockVoxelFileList
        {
            get
            {
                return this._dataModel.StockVoxelFileList;
            }
        }

        public List<string> CustomVoxelFileList
        {
            get
            {
                return this._dataModel.CustomVoxelFileList;
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
            return (this.IsValidVoxelFile && this.IsFileVoxel) || (this.IsCustomVoxel && this.CustomVoxel != null) || (this.IsStockVoxel && this.StockVoxel != null);
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

            IOpenFileDialog openFileDialog = _openFileDialogFactory();
            openFileDialog.Filter = Resources.ImportVoxelFilter;
            openFileDialog.Title = Resources.ImportVoxelTitle;

            // Open the dialog
            DialogResult result = _dialogService.ShowOpenFileDialog(this.OwnerViewModel, openFileDialog);

            if (result == DialogResult.OK)
            {
                this.IsBusy = true;

                if (File.Exists(openFileDialog.FileName))
                {
                    this.SourceFile = openFileDialog.FileName;
                    this.IsValidVoxelFile = true;
                    this.IsFileVoxel = true;
                }

                this.IsBusy = false;
            }
        }

        public MyObjectBuilder_EntityBase BuildEntity()
        {
            //this.Position = new BindablePoint3DModel(0, 0, 0);
            //this.Position = new ThreeDPointModel(0, 0, 0);
            //this.Forward = new ThreeDPointModel(0, 0, 1);
            //this.Up = new ThreeDPointModel(0, 1, 0);

            // Figure out where the Character is facing, and plant the new constrcut right in front, by "5" units, facing the Character.
            //double distance = 5;
            var vector = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward).Vector3D;
            vector.Normalize();
            //vector = Vector3D.Multiply(vector, distance);
            this.Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(this._dataModel.CharacterPosition.Position).Point3D, vector));
            this.Forward = new BindableVector3DModel(this._dataModel.CharacterPosition.Forward);
            this.Up = new BindableVector3DModel(this._dataModel.CharacterPosition.Up);


            string originalFile = null;
            if (this.IsStockVoxel)
            {
                var stockfile = Path.Combine(Path.Combine(ToolboxUpdater.GetApplicationFilePath(), @"Content\VoxelMaps"), this.StockVoxel + ".vox");

                if (this.StockMaterial == null)
                {
                    this.SourceFile = stockfile;
                    originalFile = this.SourceFile;
                }
                else
                {
                    var asteroid = new MyVoxelMap();
                    asteroid.Load(stockfile, this.StockMaterial.Value);
                    asteroid.ForceBaseMaterial(this.StockMaterial.Value);
                    this.SourceFile = Path.GetTempFileName();
                    asteroid.Save(this.SourceFile);
                    originalFile = this.StockVoxel + ".vox";
                }
            }
            else if (this.IsCustomVoxel)
            {
                this.SourceFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".vox");
                originalFile = this.CustomVoxel + ".vox";

                // Copy Resource to Temp file.
                File.WriteAllBytes(this.SourceFile, (byte[])Properties.Resources.ResourceManager.GetObject(this.CustomVoxel));
            }
            else if (this.IsFileVoxel)
            {
                originalFile = this.SourceFile;
            }

            // automatically number all files, and check for duplicate filenames.
            this.Filename = ((ExplorerViewModel)this.OwnerViewModel).CreateUniqueVoxelFilename(originalFile);

            var entity = new MyObjectBuilder_VoxelMap(this.Position.ToVector3(), this.Filename);
            entity.EntityId = SpaceEngineersAPI.GenerateEntityId();
            entity.PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene;
            entity.Filename = this.Filename;

            entity.PositionAndOrientation = new MyPositionAndOrientation()
            {
                Position = this.Position.ToVector3(),
                Forward = this.Forward.ToVector3(),
                Up = this.Up.ToVector3()
            };

            return entity;
        }

        #endregion
    }
}
