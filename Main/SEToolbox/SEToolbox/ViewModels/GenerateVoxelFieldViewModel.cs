namespace SEToolbox.ViewModels
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
    using SEToolbox.Services;
    using SEToolbox.Support;
    using VRageMath;

    public class GenerateVoxelFieldViewModel : BaseViewModel
    {
        #region Fields

        private readonly GenerateVoxelFieldModel _dataModel;
        private GenerateVoxelModel _selectedRow;

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

        public GenerateVoxelModel SelectedRow
        {
            get { return _selectedRow; }

            set
            {
                _selectedRow = value;
                RaisePropertyChanged(() => SelectedRow);
            }
        }

        public ObservableCollection<GenerateVoxelModel> VoxelCollection
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

        public ObservableCollection<GenerateVoxelDetailModel> StockVoxelFileList
        {
            get { return _dataModel.StockVoxelFileList; }
        }

        public List<GenerateVoxelDetailModel> SmallVoxelFileList
        {
            get { return _dataModel.SmallVoxelFileList; }
        }

        public List<GenerateVoxelDetailModel> LargeVoxelFileList
        {
            get { return _dataModel.LargeVoxelFileList; }
        }

        public ObservableCollection<MaterialSelectionModel> MaterialsCollection
        {
            get { return _dataModel.MaterialsCollection; }
        }

        public List<int> PercentList
        {
            get { return _dataModel.PercentList; }
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
            int idx;

            var randomModel = new GenerateVoxelModel
            {
                Index = VoxelCollection.Count + 1,
                MainMaterial = _dataModel.BaseMaterial,
                SecondMaterial = _dataModel.BaseMaterial,
                ThirdMaterial = _dataModel.BaseMaterial,
                ForthMaterial = _dataModel.BaseMaterial,
                FifthMaterial = _dataModel.BaseMaterial,
                SixthMaterial = _dataModel.BaseMaterial,
                SeventhMaterial = _dataModel.BaseMaterial,
            };

            // Random Asteroid.
            var d = RandomUtil.GetDouble(1, 100);
            var islarge = false;
            if (d > 70)
            {
                idx = RandomUtil.GetInt(LargeVoxelFileList.Count);
                randomModel.VoxelFile = LargeVoxelFileList[idx];
                islarge = true;
            }
            else
            {
                idx = RandomUtil.GetInt(SmallVoxelFileList.Count);
                randomModel.VoxelFile = SmallVoxelFileList[idx];
            }

            // Random Main Material non-Rare.
            var nonRare = MaterialsCollection.Where(m => m.IsRare == false).ToArray();
            idx = RandomUtil.GetInt(nonRare.Length);
            randomModel.MainMaterial = nonRare[idx];

            int percent;
            var rare = MaterialsCollection.Where(m => m.IsRare && m.MinedRatio >= 1).ToList();
            var superRare = MaterialsCollection.Where(m => m.IsRare && m.MinedRatio < 1).ToList();

            if (islarge)
            {
                // Random 1. Rare.
                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    percent = RandomUtil.GetInt(40, 60);
                    randomModel.SecondMaterial = rare[idx];
                    randomModel.SecondPercent = percent;
                    rare.RemoveAt(idx);
                }

                // Random 2. Rare.
                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    percent = RandomUtil.GetInt(6, 12);
                    randomModel.ThirdMaterial = rare[idx];
                    randomModel.ThirdPercent = percent;
                    rare.RemoveAt(idx);
                }

                // Random 3. Rare.
                if (rare.Count > 0)
                {
                    idx = RandomUtil.GetInt(rare.Count);
                    percent = RandomUtil.GetInt(6, 12);
                    randomModel.ThirdMaterial = rare[idx];
                    randomModel.ThirdPercent = percent;
                    rare.RemoveAt(idx);
                }

                // Random 4. Super Rare.
                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    percent = RandomUtil.GetInt(2, 4);
                    randomModel.ForthMaterial = superRare[idx];
                    randomModel.ForthPercent = percent;
                    superRare.RemoveAt(idx);
                }

                // Random 5. Super Rare.
                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    percent = RandomUtil.GetInt(1, 3);
                    randomModel.FifthMaterial = superRare[idx];
                    randomModel.FifthPercent = percent;
                    superRare.RemoveAt(idx);
                }

                // Random 6. Super Rare.
                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    percent = RandomUtil.GetInt(1, 3);
                    randomModel.SixthMaterial = superRare[idx];
                    randomModel.SixthPercent = percent;
                    superRare.RemoveAt(idx);
                }

                // Random 7. Super Rare.
                if (superRare.Count > 0)
                {
                    idx = RandomUtil.GetInt(superRare.Count);
                    percent = RandomUtil.GetInt(1, 3);
                    randomModel.SeventhMaterial = superRare[idx];
                    randomModel.SeventhPercent = percent;
                    superRare.RemoveAt(idx);
                }
            }
            else // Small Asteroid.
            {
                // Random 1. Rare.
                idx = RandomUtil.GetInt(rare.Count);
                percent = RandomUtil.GetInt(6, 13);
                randomModel.SecondMaterial = rare[idx];
                randomModel.SecondPercent = percent;

                // Random 2. Super Rare.
                idx = RandomUtil.GetInt(superRare.Count);
                percent = RandomUtil.GetInt(2, 4);
                randomModel.ThirdMaterial = superRare[idx];
                randomModel.ThirdPercent = percent;
                superRare.RemoveAt(idx);
            }

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
                VoxelCollection.Insert(VoxelCollection.IndexOf(SelectedRow) + 1, SelectedRow.Clone());
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
            return VoxelCollection.Aggregate(valid, (current, t) => current & (t.SecondPercent + t.ThirdPercent + t.ForthPercent + t.FifthPercent + t.SixthPercent + t.SeventhPercent) <= 100);
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

                IList<byte> baseAssets;
                Dictionary<byte, long> materialVoxelCells;

                asteroid.CalculateMaterialCellAssets(out baseAssets, out materialVoxelCells);

                var distribution = new List<double> { Double.NaN };
                var materialSelection = new List<byte> { SpaceEngineersApi.GetMaterialIndex(voxelDesign.MainMaterial.Value) };

                if (voxelDesign.SecondPercent > 0)
                {
                    distribution.Add((double)voxelDesign.SecondPercent / 100);
                    materialSelection.Add(SpaceEngineersApi.GetMaterialIndex(voxelDesign.SecondMaterial.Value));
                }
                if (voxelDesign.ThirdPercent > 0)
                {
                    distribution.Add((double)voxelDesign.ThirdPercent / 100);
                    materialSelection.Add(SpaceEngineersApi.GetMaterialIndex(voxelDesign.ThirdMaterial.Value));
                }
                if (voxelDesign.ForthPercent > 0)
                {
                    distribution.Add((double)voxelDesign.ForthPercent / 100);
                    materialSelection.Add(SpaceEngineersApi.GetMaterialIndex(voxelDesign.ForthMaterial.Value));
                }
                if (voxelDesign.FifthPercent > 0)
                {
                    distribution.Add((double)voxelDesign.FifthPercent / 100);
                    materialSelection.Add(SpaceEngineersApi.GetMaterialIndex(voxelDesign.FifthMaterial.Value));
                }
                if (voxelDesign.SixthPercent > 0)
                {
                    distribution.Add((double)voxelDesign.SixthPercent / 100);
                    materialSelection.Add(SpaceEngineersApi.GetMaterialIndex(voxelDesign.SixthMaterial.Value));
                }
                if (voxelDesign.SeventhPercent > 0)
                {
                    distribution.Add((double)voxelDesign.SeventhPercent / 100);
                    materialSelection.Add(SpaceEngineersApi.GetMaterialIndex(voxelDesign.SeventhMaterial.Value));
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
                asteroid.ForceVoxelFaceMaterial(_dataModel.BaseMaterial.DisplayName);

                var tempfilename = TempfileUtil.NewFilename();
                asteroid.Save(tempfilename);

                // automatically number all files, and check for duplicate filenames.
                var filename = MainViewModel.CreateUniqueVoxelFilename(voxelDesign.VoxelFile.Name + ".vox", entities.ToArray());

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

                var position = _dataModel.CharacterPosition.Position + new Vector3((float)x, (float)y, (float)z) - asteroid.ContentCenter;
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
