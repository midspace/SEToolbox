﻿namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Models.Asteroids;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using VRageMath;

    public class GenerateVoxelFieldViewModel : BaseViewModel
    {
        #region Fields

        private readonly GenerateVoxelFieldModel _dataModel;
        private AsteroidByteFillProperties _selectedRow;

        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region Constructors

        public GenerateVoxelFieldViewModel(BaseViewModel parentViewModel, GenerateVoxelFieldModel dataModel)
            : base(parentViewModel)
        {
            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties

        public ICommand ClearRowsCommand
        {
            get { return new DelegateCommand(ClearRowsExecuted, ClearRowsCanExecute); }
        }

        public ICommand AddRandomRowCommand
        {
            get { return new DelegateCommand(AddRandomRowExecuted, AddRandomRowCanExecute); }
        }

        public ICommand AddRowCommand
        {
            get { return new DelegateCommand(AddRowExecuted, AddRowCanExecute); }
        }

        public ICommand DeleteRowCommand
        {
            get { return new DelegateCommand(DeleteRowExecuted, DeleteRowCanExecute); }
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

        public AsteroidByteFillProperties SelectedRow
        {
            get { return _selectedRow; }

            set
            {
                _selectedRow = value;
                RaisePropertyChanged(() => SelectedRow);
            }
        }

        public ObservableCollection<AsteroidByteFillProperties> VoxelCollection
        {
            get { return _dataModel.VoxelCollection; }
            set { _dataModel.VoxelCollection = value; }
        }

        public int MinimumRange
        {
            get { return _dataModel.MinimumRange; }
            set { _dataModel.MinimumRange = value; }
        }

        public int MaximumRange
        {
            get { return _dataModel.MaximumRange; }
            set { _dataModel.MaximumRange = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    RaisePropertyChanged(() => IsBusy);
                    if (_isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        public ObservableCollection<GenerateVoxelDetailModel> VoxelFileList
        {
            get { return _dataModel.VoxelFileList; }
        }

        public ObservableCollection<MaterialSelectionModel> MaterialsCollection
        {
            get { return _dataModel.MaterialsCollection; }
        }

        public List<int> PercentList
        {
            get { return _dataModel.PercentList; }
        }

        public float CenterPositionX
        {
            get { return _dataModel.CenterPositionX; }
            set { _dataModel.CenterPositionX = value; }
        }

        public float CenterPositionY
        {
            get { return _dataModel.CenterPositionY; }
            set { _dataModel.CenterPositionY = value; }
        }

        public float CenterPositionZ
        {
            get { return _dataModel.CenterPositionZ; }
            set { _dataModel.CenterPositionZ = value; }
        }

        #endregion

        #region methods

        public bool ClearRowsCanExecute()
        {
            return VoxelCollection.Count > 0;
        }

        public void ClearRowsExecuted()
        {
            VoxelCollection.Clear();
            MinimumRange = 400;
            MaximumRange = 800;
        }

        public bool AddRandomRowCanExecute()
        {
            return true;
        }

        public void AddRandomRowExecuted()
        {
            var filler = new AsteroidByteFiller();
            var randomModel = (AsteroidByteFillProperties)filler.CreateRandom(VoxelCollection.Count + 1, _dataModel.BaseMaterial, MaterialsCollection, VoxelFileList);

            if (SelectedRow != null)
            {
                VoxelCollection.Insert(VoxelCollection.IndexOf(SelectedRow) + 1, randomModel);
            }
            else
            {
                VoxelCollection.Add(randomModel);
            }

            _dataModel.RenumberCollection();
        }

        public bool AddRowCanExecute()
        {
            return true;
        }

        public void AddRowExecuted()
        {
            if (SelectedRow != null)
            {
                VoxelCollection.Insert(VoxelCollection.IndexOf(SelectedRow) + 1, (AsteroidByteFillProperties)SelectedRow.Clone());
            }
            else
            {
                VoxelCollection.Add(_dataModel.NewDefaultVoxel(VoxelCollection.Count + 1));
            }

            _dataModel.RenumberCollection();
        }

        public bool DeleteRowCanExecute()
        {
            return SelectedRow != null;
        }

        public void DeleteRowExecuted()
        {
            var index = VoxelCollection.IndexOf(SelectedRow);
            VoxelCollection.Remove(SelectedRow);
            _dataModel.RenumberCollection();

            while (index >= VoxelCollection.Count)
            {
                index--;
            }
            if (index >= 0)
            {
                SelectedRow = VoxelCollection[index];
            }
        }

        public bool CreateCanExecute()
        {
            var valid = VoxelCollection.Count > 0;
            return VoxelCollection.Aggregate(valid, (current, t) => current);
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

        public void BuildEntities(out string[] sourceVoxelFiles, out MyObjectBuilder_EntityBase[] sourceEntities)
        {
            var entities = new List<MyObjectBuilder_EntityBase>();
            var sourceFiles = new List<string>();

            MainViewModel.ResetProgress(0, VoxelCollection.Count);

            foreach (var voxelDesign in VoxelCollection)
            {
                MainViewModel.Progress++;
                if (string.IsNullOrEmpty(voxelDesign.VoxelFile.SourceFilename) || !MyVoxelMap.IsVoxelMapFile(voxelDesign.VoxelFile.SourceFilename))
                    continue;

                var asteroid = new MyVoxelMap();

                asteroid.Load(voxelDesign.VoxelFile.SourceFilename, voxelDesign.MainMaterial.Value, false);

                var filler = new AsteroidByteFiller();
                filler.FillAsteroid(asteroid, voxelDesign);

                var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
                asteroid.Save(tempfilename);

                // automatically number all files, and check for duplicate filenames.
                var filename = MainViewModel.CreateUniqueVoxelStorageName(voxelDesign.VoxelFile.Name + MyVoxelMap.V2FileExtension, entities.ToArray());

                var radius = RandomUtil.GetDouble(MinimumRange, MaximumRange);
                var longitude = RandomUtil.GetDouble(0, 2 * Math.PI);
                var latitude = RandomUtil.GetDouble(-Math.PI / 2, (Math.PI / 2) + double.Epsilon);

                // Test data. Place asteroids items into a circle.
                //radius = 500;
                //longitude = Math.PI * 2 * ((double)voxelDesign.Index / VoxelCollection.Count);
                //latitude = 0;

                var x = radius * Math.Cos(latitude) * Math.Cos(longitude);
                var z = radius * Math.Cos(latitude) * Math.Sin(longitude);
                var y = radius * Math.Sin(latitude);

                var center = new Vector3(CenterPositionX, CenterPositionY, CenterPositionZ);
                var position = center + new Vector3((float)x, (float)y, (float)z) - asteroid.BoundingContent.Center;
                var entity = new MyObjectBuilder_VoxelMap(position, filename)
                {
                    EntityId = SpaceEngineersApi.GenerateEntityId(),
                    PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                    Filename = filename,
                    PositionAndOrientation = new MyPositionAndOrientation
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
