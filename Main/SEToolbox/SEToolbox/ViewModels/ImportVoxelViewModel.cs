namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Properties;
    using SEToolbox.Services;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

    public class ImportVoxelViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService dialogService;
        private readonly Func<IOpenFileDialog> openFileDialogFactory;
        private ImportVoxelModel dataModel;

        private bool? closeResult;
        private bool isBusy;

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
            }
        }

        public string SourceFile
        {
            get
            {
                return this.dataModel.SourceFile;
            }

            set
            {
                this.dataModel.SourceFile = value;
            }
        }

        public bool IsValidVoxelFile
        {
            get
            {
                return this.dataModel.IsValidVoxelFile;
            }

            set
            {
                this.dataModel.IsValidVoxelFile = value;
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

        public bool IsStockVoxel
        {
            get
            {
                return this.dataModel.IsStockVoxel;
            }

            set
            {
                this.dataModel.IsStockVoxel = value;
            }
        }

        public bool IsCustomVoxel
        {
            get
            {
                return this.dataModel.IsCustomVoxel;
            }

            set
            {
                this.dataModel.IsCustomVoxel = value;
            }
        }

        public bool IsFileVoxel
        {
            get
            {
                return this.dataModel.IsFileVoxel;
            }

            set
            {
                this.dataModel.IsFileVoxel = value;
            }
        }

        public string StockVoxel
        {
            get
            {
                return this.dataModel.StockVoxel;
            }

            set
            {
                this.dataModel.StockVoxel = value;
            }
        }

        public string CustomVoxel
        {
            get
            {
                return this.dataModel.CustomVoxel;
            }

            set
            {
                this.dataModel.CustomVoxel = value;
            }
        }

        public List<string> StockVoxelFileList
        {
            get
            {
                return this.dataModel.StockVoxelFileList;
            }
        }

        public List<string> CustomVoxelFileList
        {
            get
            {
                return this.dataModel.CustomVoxelFileList;
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

            IOpenFileDialog openFileDialog = openFileDialogFactory();
            openFileDialog.Filter = Resources.ImportVoxelFilter;
            openFileDialog.Title = Resources.ImportVoxelTitle;

            // Open the dialog
            DialogResult result = dialogService.ShowOpenFileDialog(this.OwnerViewModel, openFileDialog);

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
            var vector = new BindableVector3DModel(this.dataModel.CharacterPosition.Forward).Vector3D;
            vector.Normalize();
            //vector = Vector3D.Multiply(vector, distance);
            this.Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(this.dataModel.CharacterPosition.Position).Point3D, vector));
            this.Forward = new BindableVector3DModel(this.dataModel.CharacterPosition.Forward);
            this.Up = new BindableVector3DModel(this.dataModel.CharacterPosition.Up);


            string originalFile = null;
            if (this.IsStockVoxel)
            {
                this.SourceFile = Path.Combine(Path.Combine(SpaceEngineersAPI.GetApplicationFilePath(), @"Content\VoxelMaps"), this.StockVoxel + ".vox");
                originalFile = this.SourceFile;
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
            var filepartname = Path.GetFileNameWithoutExtension(originalFile).ToLower();
            var extension = Path.GetExtension(originalFile).ToLower();
            int index = 0;
            var filename = filepartname + index.ToString() + extension;

            while (((ExplorerViewModel)this.OwnerViewModel).ContainsVoxelFilename(filename))
            {
                index++;
                filename = filepartname + index.ToString() + extension;
            }
            this.Filename = filename;


            MyObjectBuilder_VoxelMap entity = new MyObjectBuilder_VoxelMap(this.Position.ToVector3(), this.Filename);
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
