namespace SEToolbox.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using SEToolbox.Models.Asteroids;

    public class GenerateVoxelFieldModel : BaseModel
    {
        private static readonly List<AsteroidByteFillProperties> VoxelStore;

        #region Fields

        private static int _minimumRange;
        private static int _maximumRange;
        private MyPositionAndOrientation _characterPosition;
        private ObservableCollection<GenerateVoxelDetailModel> _stockVoxelFileList;
        private readonly ObservableCollection<MaterialSelectionModel> _materialsCollection;
        private ObservableCollection<AsteroidByteFillProperties> _voxelCollection;
        private readonly List<int> _percentList;

        #endregion

        #region ctor

        public GenerateVoxelFieldModel()
        {
            _stockVoxelFileList = new ObservableCollection<GenerateVoxelDetailModel>();
            _materialsCollection = new ObservableCollection<MaterialSelectionModel>();
            _voxelCollection = new ObservableCollection<AsteroidByteFillProperties>();
            _percentList = new List<int>();
        }

        static GenerateVoxelFieldModel()
        {
            VoxelStore = new List<AsteroidByteFillProperties>();
        }

        #endregion

        #region Properties

        public ObservableCollection<AsteroidByteFillProperties> VoxelCollection
        {
            get
            {
                return _voxelCollection;
            }

            set
            {
                if (value != _voxelCollection)
                {
                    _voxelCollection = value;
                    RaisePropertyChanged(() => VoxelCollection);
                }
            }
        }

        public int MinimumRange
        {
            get
            {
                return _minimumRange;
            }

            set
            {
                if (value != _minimumRange)
                {
                    _minimumRange = value;
                    RaisePropertyChanged(() => MinimumRange);
                }
            }
        }

        public int MaximumRange
        {
            get
            {
                return _maximumRange;
            }

            set
            {
                if (value != _maximumRange)
                {
                    _maximumRange = value;
                    RaisePropertyChanged(() => MaximumRange);
                }
            }
        }

        public MyPositionAndOrientation CharacterPosition
        {
            get
            {
                return _characterPosition;
            }

            set
            {
                //if (value != characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                _characterPosition = value;
                RaisePropertyChanged(() => CharacterPosition);
            }
        }

        public ObservableCollection<GenerateVoxelDetailModel> StockVoxelFileList
        {
            get
            {
                return _stockVoxelFileList;
            }

            set
            {
                if (value != _stockVoxelFileList)
                {
                    _stockVoxelFileList = value;
                    RaisePropertyChanged(() => StockVoxelFileList);
                }
            }
        }

        public ObservableCollection<MaterialSelectionModel> MaterialsCollection
        {
            get
            {
                return _materialsCollection;
            }
        }

        public List<int> PercentList
        {
            get
            {
                return _percentList;
            }
        }

        public MaterialSelectionModel BaseMaterial { get; set; }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            CharacterPosition = characterPosition;

            MaterialsCollection.Clear();
            foreach (var material in SpaceEngineersCore.Resources.GetMaterialList())
            {
                MaterialsCollection.Add(new MaterialSelectionModel { Value = material.Id.SubtypeId, DisplayName = material.Id.SubtypeId, IsRare = material.IsRare, MinedRatio = material.MinedOreRatio });
            }

            BaseMaterial = MaterialsCollection.FirstOrDefault(m => m.IsRare == false) ?? MaterialsCollection.FirstOrDefault();

            var files = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationContentPath(), @"VoxelMaps"), "*.vox");

            StockVoxelFileList.Clear();
            foreach (var file in files)
            {
                var voxel = new GenerateVoxelDetailModel
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    SourceFilename = file,
                    FileSize = new FileInfo(file).Length
                };
                StockVoxelFileList.Add(voxel);
            }

            // Set up a default start.
            if (VoxelStore.Count == 0)
            {
                VoxelCollection.Add(NewDefaultVoxel(1));
            }
            else
            {
                foreach (var item in VoxelStore)
                {
                    var v1 = (AsteroidByteFillProperties)item.Clone();
                    v1.VoxelFile = StockVoxelFileList.FirstOrDefault(v => v.Name == v1.VoxelFile.Name);
                    v1.MainMaterial = MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.MainMaterial.DisplayName);
                    v1.SecondMaterial = MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.SecondMaterial.DisplayName);
                    v1.ThirdMaterial = MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.ThirdMaterial.DisplayName);
                    v1.FourthMaterial = MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.FourthMaterial.DisplayName);
                    v1.FifthMaterial = MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.FifthMaterial.DisplayName);
                    v1.SixthMaterial = MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.SixthMaterial.DisplayName);
                    v1.SeventhMaterial = MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.SeventhMaterial.DisplayName);
                    VoxelCollection.Add(v1);
                }
                RenumberCollection();
            }

            for (var i = 0; i < 100; i++)
            {
                PercentList.Add(i);
            }

            MinimumRange = 400;
            MaximumRange = 800;
        }

        public void Unload()
        {
            VoxelStore.Clear();
            VoxelStore.AddRange(_voxelCollection);
        }

        public AsteroidByteFillProperties NewDefaultVoxel(int index)
        {
            return new AsteroidByteFillProperties
            {
                Index = index,
                VoxelFile = StockVoxelFileList[0],
                MainMaterial = MaterialsCollection[0],
                SecondMaterial = MaterialsCollection[0],
                ThirdMaterial = MaterialsCollection[0],
                FourthMaterial = MaterialsCollection[0],
                FifthMaterial = MaterialsCollection[0],
                SixthMaterial = MaterialsCollection[0],
                SeventhMaterial = MaterialsCollection[0],
            };
        }

        public void RenumberCollection()
        {
            for (var i = 0; i < VoxelCollection.Count; i++)
            {
                VoxelCollection[i].Index = i + 1;
            }
        }

        #endregion
    }
}
