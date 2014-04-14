namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows.Input;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
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

        public ICommand ClearRowsCommand
        {
            get
            {
                return new DelegateCommand(new Action(ClearRowsExecuted), new Func<bool>(ClearRowsCanExecute));
            }
        }

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

        public List<int> PercentList
        {
            get
            {
                return this._dataModel.PercentList;
            }
        }

        #endregion

        #region methods

        public bool ClearRowsCanExecute()
        {
            return this.VoxelCollection.Count > 0;
        }

        public void ClearRowsExecuted()
        {
            this.VoxelCollection.Clear();
            this.MinimumRange = 400;
            this.MaximumRange = 800;
        }

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

            this._dataModel.RenumberCollection();
        }

        public bool DeleteRowCanExecute()
        {
            return this.SelectedRow != null;
        }

        public void DeleteRowExecuted()
        {
            var index = this.VoxelCollection.IndexOf(this.SelectedRow);
            this.VoxelCollection.Remove(this.SelectedRow);
            this._dataModel.RenumberCollection();

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
            var valid = this.VoxelCollection.Count > 0;
            return this.VoxelCollection.Aggregate(valid, (current, t) => current & (t.SecondPercent + t.ThirdPercent + t.ForthPercent + t.FifthPercent) <= 100);
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

        public void BuildEntities(out string[] sourceVoxelFiles, out MyObjectBuilder_EntityBase[] sourceEntities)
        {
            var entities = new List<MyObjectBuilder_EntityBase>();
            var sourceFiles = new List<string>();

            foreach (var voxelDesign in this.VoxelCollection)
            {
                if (string.IsNullOrEmpty(voxelDesign.VoxelFile.SourceFilename) || !MyVoxelMap.IsVoxelMapFile(voxelDesign.VoxelFile.SourceFilename))
                    continue;

                var asteroid = new MyVoxelMap();
                asteroid.Load(voxelDesign.VoxelFile.SourceFilename, voxelDesign.MainMaterial.Value);

                IList<byte> baseAssets;
                Dictionary<byte, long> materialVoxelCells;

                asteroid.CalculateMaterialCellAssets(out baseAssets, out materialVoxelCells);

                var distribution = new List<double> { Double.NaN };
                var materialSelection = new List<byte> { SpaceEngineersAPI.GetMaterialIndex(voxelDesign.MainMaterial.Value) };

                if (voxelDesign.SecondPercent > 0)
                {
                    distribution.Add((double)voxelDesign.SecondPercent / 100);
                    materialSelection.Add(SpaceEngineersAPI.GetMaterialIndex(voxelDesign.SecondMaterial.Value));
                }
                if (voxelDesign.ThirdPercent > 0)
                {
                    distribution.Add((double)voxelDesign.ThirdPercent / 100);
                    materialSelection.Add(SpaceEngineersAPI.GetMaterialIndex(voxelDesign.ThirdMaterial.Value));
                }
                if (voxelDesign.ForthPercent > 0)
                {
                    distribution.Add((double)voxelDesign.ForthPercent / 100);
                    materialSelection.Add(SpaceEngineersAPI.GetMaterialIndex(voxelDesign.ForthMaterial.Value));
                }
                if (voxelDesign.FifthPercent > 0)
                {
                    distribution.Add((double)voxelDesign.FifthPercent / 100);
                    materialSelection.Add(SpaceEngineersAPI.GetMaterialIndex(voxelDesign.FifthMaterial.Value));
                }

                var newDistributiuon = new List<byte>();
                int count;
                for (var i = 1; i < distribution.Count(); i++)
                {
                    count = (int)Math.Floor(distribution[i] * baseAssets.Count); // Round down.
                    for (var j = 0; j < count; j++)
                    {
                        newDistributiuon.Add(materialSelection[i]);
                    }
                }
                count = baseAssets.Count - newDistributiuon.Count;
                for (var j = 0; j < count; j++)
                {
                    newDistributiuon.Add(materialSelection[0]);
                }

                newDistributiuon.Shuffle();
                asteroid.SetMaterialAssets(newDistributiuon);

                var tempfilename = TempfileUtil.NewFilename();
                asteroid.Save(tempfilename);

                // automatically number all files, and check for duplicate filenames.
                var filename = this.MainViewModel.CreateUniqueVoxelFilename(voxelDesign.VoxelFile.Name + ".vox", entities.ToArray());

                var radius = RandomUtil.GetDouble(this.MinimumRange, this.MaximumRange);
                var longitude = RandomUtil.GetDouble(0, 2 * Math.PI);
                var latitude = RandomUtil.GetDouble(-Math.PI / 2, (Math.PI / 2) + double.Epsilon);

                // Test data. Place asteroids items into a circle.
                //radius = 500;
                //longitude = Math.PI * 2 * ((double)voxelDesign.Index / this.VoxelCollection.Count);
                //latitude = 0;

                var x = radius * Math.Cos(latitude) * Math.Cos(longitude);
                var z = radius * Math.Cos(latitude) * Math.Sin(longitude);
                var y = radius * Math.Sin(latitude);

                var position = this._dataModel.CharacterPosition.Position + new Vector3((float)x, (float)y, (float)z) - asteroid.ContentCenter;
                var entity = new MyObjectBuilder_VoxelMap(position, filename)
                {
                    EntityId = SpaceEngineersAPI.GenerateEntityId(),
                    PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                    Filename = filename,
                    PositionAndOrientation = new MyPositionAndOrientation()
                    {
                        Position = position,
                        Forward = Vector3.Forward, // Asteroids currently don't have any orientation.
                        Up = Vector3.Up
                    }
                };

                entities.Add(entity);
                sourceFiles.Add(tempfilename);
            }

            sourceVoxelFiles = sourceFiles.ToArray();
            sourceEntities = entities.ToArray();
        }

        #endregion
    }
}
