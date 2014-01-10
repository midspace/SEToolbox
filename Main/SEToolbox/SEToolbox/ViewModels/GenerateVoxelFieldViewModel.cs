namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Input;
    using Sandbox.CommonLib.ObjectBuilders;
    using Sandbox.CommonLib.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using VRageMath;

    public class GenerateVoxelFieldViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly GenerateVoxelFieldModel _dataModel;
        private GenerateVoxelModel _selectedRow;

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

        public ICommand AddRowCommand
        {
            get
            {
                return new DelegateCommand(new Action(AddRowExecuted), new Func<bool>(AddRowCanExecute));
            }
        }

        public ICommand DeleteRowCommand
        {
            get
            {
                return new DelegateCommand(new Action(DeleteRowExecuted), new Func<bool>(DeleteRowCanExecute));
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

        public GenerateVoxelModel SelectedRow
        {
            get
            {
                return this._selectedRow;
            }

            set
            {
                this._selectedRow = value;
                this.RaisePropertyChanged(() => SelectedRow);
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

        public bool AddRowCanExecute()
        {
            return true;
        }

        public void AddRowExecuted()
        {
            if (this.SelectedRow != null)
            {
                this.VoxelCollection.Insert(this.VoxelCollection.IndexOf(this.SelectedRow) + 1, this.SelectedRow.Clone());
            }
            else
            {
                this.VoxelCollection.Add(this._dataModel.NewDefaultVoxel(this.VoxelCollection.Count + 1));
            }

            this.RenumberCollection();
        }

        public bool DeleteRowCanExecute()
        {
            return this.SelectedRow != null;
        }

        public void DeleteRowExecuted()
        {
            var index = this.VoxelCollection.IndexOf(this.SelectedRow);
            this.VoxelCollection.Remove(this.SelectedRow);
            this.RenumberCollection();

            while (index >= this.VoxelCollection.Count)
            {
                index--;
            }
            if (index >= 0)
            {
                this.SelectedRow = this.VoxelCollection[index];
            }
        }

        public bool CreateCanExecute()
        {
            return true;
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
                    var filename = ((ExplorerViewModel)this.OwnerViewModel).CreateUniqueVoxelFilename(voxelDesign.VoxelFile.Name + ".vox", entities.ToArray());

                    var radius = RandomUtil.GetDouble(this.MinimumRange, this.MaximumRange);
                    var longitude = RandomUtil.GetDouble(0, 2 * Math.PI);
                    var latitude = RandomUtil.GetDouble(-Math.PI / 2, Math.PI / 2 + double.Epsilon);

                    // Test data. Place asteroids items into a circle.
                    //radius = 500;
                    //longitude = Math.PI * 2 * ((double)voxelDesign.Index / this.VoxelCollection.Count);
                    //latitude = 0;

                    double x = radius * Math.Cos(latitude) * Math.Cos(longitude);
                    double z = radius * Math.Cos(latitude) * Math.Sin(longitude);
                    double y = radius * Math.Sin(latitude);

                    Vector3 position = this._dataModel.CharacterPosition.Position + new Vector3((float)x, (float)y, (float)z) - asteroid.ContentCenter;
                    var entity = new MyObjectBuilder_VoxelMap(position, filename);
                    entity.EntityId = SpaceEngineersAPI.GenerateEntityId();
                    entity.PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene;
                    entity.Filename = filename;

                    entity.PositionAndOrientation = new MyPositionAndOrientation()
                    {
                        Position = position,
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

        public void RenumberCollection()
        {
            for (var i = 0; i < this.VoxelCollection.Count; i++)
            {
                this.VoxelCollection[i].Index = i + 1;
            }
        }

        #endregion
    }
}
