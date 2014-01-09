namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Input;
    using VRageMath;

    public class GenerateVoxelFieldViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly GenerateVoxelFieldModel _dataModel;

        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region Constructors

        public GenerateVoxelFieldViewModel(BaseViewModel parentViewModel, GenerateVoxelFieldModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), () => ServiceLocator.Resolve<IOpenFileDialog>())
        {
        }

        public GenerateVoxelFieldViewModel(BaseViewModel parentViewModel, GenerateVoxelFieldModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
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

        public ObservableCollection<GenerateVoxelModel> VoxelCollection
        {
            get
            {
                return this._dataModel.VoxelCollection;
            }

            set
            {
                this._dataModel.VoxelCollection = value;
            }
        }

        public int MinimumRange
        {
            get
            {
                return this._dataModel.MinimumRange;
            }

            set
            {
                this._dataModel.MinimumRange = value;
            }
        }

        public int MaximumRange
        {
            get
            {
                return this._dataModel.MaximumRange;
            }

            set
            {
                this._dataModel.MaximumRange = value;
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

        public ObservableCollection<GenerateVoxelDetailModel> StockVoxelFileList
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

        #endregion

        #region methods

        public bool CreateCanExecute()
        {
            return true;
            //return (this.IsValidVoxelFile && this.IsFileVoxel) || (this.IsCustomVoxel && this.CustomVoxel != null) || (this.IsStockVoxel && this.StockVoxel != null);
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

        public MyObjectBuilder_EntityBase[] BuildEntities(out string[] sourceVoxelFiles)
        {
            var entities = new List<MyObjectBuilder_EntityBase>();
            var sourceFiles = new List<string>();

            foreach (var voxelDesign in this.VoxelCollection)
            {
                if (!string.IsNullOrEmpty(voxelDesign.VoxelFile.SourceFilename))
                {
                    var asteroid = new MyVoxelMap();
                    asteroid.Load(voxelDesign.VoxelFile.SourceFilename, voxelDesign.MainMaterial.Value);
                    asteroid.ForceBaseMaterial(voxelDesign.MainMaterial.Value);
                    var tempfilename = Path.GetTempFileName();
                    asteroid.Save(tempfilename);

                    // automatically number all files, and check for duplicate filenames.
                    var filename = ((ExplorerViewModel)this.OwnerViewModel).CreateUniqueVoxelFilename(voxelDesign.VoxelFile.Name + ".vox");

                    // TODO: Complete Generate Voxel Field

                    // todo: Random range, from 
                    //this.MinimumRange 
                    //this.MaximumRange

                    // Random Longitude in Radians, from 0 to 2*Math.PI.
                    // Random Latitude in Radians, from -Math.PI/2 to +Math.PI/2.



                    // Figure out where the Character is facing, and plant the new constrcut right in front.
                    // Calculate the hypotenuse, as it will be the safest distance to place in front.
                    double distance = Math.Sqrt(Math.Pow(asteroid.ContentSize.X, 2) + Math.Pow(asteroid.ContentSize.Y, 2) + Math.Pow(asteroid.ContentSize.Z, 2)) / 2;

                    var vector = this._dataModel.CharacterPosition.Forward;
                    vector.Normalize();
                    vector = new Vector3(vector.X * (float)distance, vector.Y * (float)distance, vector.Z * (float)distance);
                    var position = this._dataModel.CharacterPosition.Position + vector;



                    var entity = new MyObjectBuilder_VoxelMap(position - asteroid.ContentCenter, filename);
                    entity.EntityId = SpaceEngineersAPI.GenerateEntityId();
                    entity.PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene;
                    entity.Filename = filename;

                    entity.PositionAndOrientation = new MyPositionAndOrientation()
                    {
                        Position = position - asteroid.ContentCenter,
                        Forward = Vector3.Forward,  // Asteroids currently don't have any orientation.
                        Up = Vector3.Up
                    };

                    entities.Add(entity);
                    sourceFiles.Add(tempfilename);
                }
            }

            sourceVoxelFiles = sourceFiles.ToArray();
            return entities.ToArray();
        }

        #endregion
    }
}
