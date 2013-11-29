namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Properties;
    using SEToolbox.Services;
    using SEToolbox.Support;
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

        public string SourceFilename
        {
            get
            {
                return this.dataModel.SourceFilename;
            }

            set
            {
                this.dataModel.SourceFilename = value;
            }
        }

        public bool IsValidVoxel
        {
            get
            {
                return this.dataModel.IsValidVoxel;
            }

            set
            {
                this.dataModel.IsValidVoxel = value;
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

        #endregion

        #region methods

        public bool BrowseVoxel()
        {
            this.IsValidVoxel = false;

            IOpenFileDialog openFileDialog = openFileDialogFactory();
            openFileDialog.Filter = Resources.ImportVoxelFilter;
            openFileDialog.Title = Resources.ImportVoxelTitle;
            openFileDialog.InitialDirectory = Path.Combine(SpaceEngineersAPI.GetApplicationFilePath(), @"Content\VoxelMaps");

            // Open the dialog
            DialogResult result = dialogService.ShowOpenFileDialog(this.OwnerViewModel, openFileDialog);

            if (result == DialogResult.OK)
            {
                this.IsBusy = true;

                if (File.Exists(openFileDialog.FileName))
                {
                    //this.Position = new BindablePoint3DModel(0, 0, 0);
                    //this.Position = new ThreeDPointModel(0, 0, 0);
                    //this.Forward = new ThreeDPointModel(0, 0, 1);
                    //this.Up = new ThreeDPointModel(0, 1, 0);


                    // Figure out where the Character is facing, and plant the new constrcut right in front, by "5" units, facing the Character.
                    double distance = 5;
                    var vector = new BindableVector3DModel(this.dataModel.CharacterPosition.Forward).Vector3D;
                    vector.Normalize();
                    vector = Vector3D.Multiply(vector, distance);
                    this.Position = new BindablePoint3DModel(Point3D.Add(new BindablePoint3DModel(this.dataModel.CharacterPosition.Position).Point3D, vector));
                    this.Forward = new BindableVector3DModel(this.dataModel.CharacterPosition.Forward);
                    this.Up = new BindableVector3DModel(this.dataModel.CharacterPosition.Up);

                    // automatically number all files, and check for duplicate filenames.
                    var filepartname = Path.GetFileNameWithoutExtension(openFileDialog.FileName).ToLower();
                    var extension = Path.GetExtension(openFileDialog.FileName).ToLower();
                    int index = 0;

                    var filename = filepartname + index.ToString() + extension;

                    while (((ExplorerViewModel)this.OwnerViewModel).ContainsVoxelFilename(filename))
                    {
                        index++;
                        filename = filepartname + index.ToString() + extension;
                    }
                    this.Filename = filename;

                    this.SourceFilename = openFileDialog.FileName;
                    this.IsValidVoxel = true;
                }

                this.IsBusy = false;
            }

            return this.IsValidVoxel;
        }
        
        #endregion

        #region methods
        
        public MyObjectBuilder_EntityBase BuildEntity()
        {
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
