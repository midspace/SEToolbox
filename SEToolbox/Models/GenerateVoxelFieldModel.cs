namespace SEToolbox.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models.Asteroids;
    using SEToolbox.Support;
    using VRage;

    public class GenerateVoxelFieldModel : BaseModel
    {
        private static readonly List<AsteroidByteFillProperties> VoxelStore;

        #region Fields

        private static int _minimumRange = 400;
        private static int _maximumRange = 800;
        private ObservableCollection<GenerateVoxelDetailModel> _voxelFileList;
        private readonly ObservableCollection<MaterialSelectionModel> _materialsCollection;
        private ObservableCollection<AsteroidByteFillProperties> _voxelCollection;
        private readonly List<int> _percentList;
        private static bool _isInitialValueSet;
        private static double _centerPositionX;
        private static double _centerPositionY;
        private static double _centerPositionZ;
        private static AsteroidFillType _asteroidFillType;

        #endregion

        #region ctor

        public GenerateVoxelFieldModel()
        {
            _voxelFileList = new ObservableCollection<GenerateVoxelDetailModel>();
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
            get{return _voxelCollection;}

            set
            {
                if (value != _voxelCollection)
                {
                    _voxelCollection = value;
                    OnPropertyChanged(nameof(VoxelCollection));
                }
            }
        }

        public int MinimumRange
        {
            get{return _minimumRange;}

            set
            {
                if (value != _minimumRange)
                {
                    _minimumRange = value;
                    OnPropertyChanged(nameof(MinimumRange));
                }
            }
        }

        public int MaximumRange
        {
            get{return _maximumRange;}

            set
            {
                if (value != _maximumRange)
                {
                    _maximumRange = value;
                    OnPropertyChanged(nameof(MaximumRange));
                }
            }
        }

        public ObservableCollection<GenerateVoxelDetailModel> VoxelFileList
        {
            get{return _voxelFileList;}

            set
            {
                if (value != _voxelFileList)
                {
                    _voxelFileList = value;
                    OnPropertyChanged(nameof(VoxelFileList));
                }
            }
        }

        public ObservableCollection<MaterialSelectionModel> MaterialsCollection
        {
            get{return _materialsCollection;}
        }

        public List<int> PercentList
        {
            get{return _percentList;}
        }

        public MaterialSelectionModel BaseMaterial { get; set; }

        public double CenterPositionX
        {
            get { return _centerPositionX; }

            set
            {
                if (value != _centerPositionX)
                {
                    _centerPositionX = value;
                    OnPropertyChanged(nameof(CenterPositionX));
                }
            }
        }

        public double CenterPositionY
        {
            get { return _centerPositionY; }

            set
            {
                if (value != _centerPositionY)
                {
                    _centerPositionY = value;
                    OnPropertyChanged(nameof(CenterPositionY));
                }
            }
        }

        public double CenterPositionZ
        {
            get { return _centerPositionZ; }

            set
            {
                if (value != _centerPositionZ)
                {
                    _centerPositionZ = value;
                    OnPropertyChanged(nameof(CenterPositionZ));
                }
            }
        }

        public AsteroidFillType AsteroidFillType
        {
            get { return _asteroidFillType; }

            set
            {
                if (value != _asteroidFillType)
                {
                    _asteroidFillType = value;
                    OnPropertyChanged(nameof(AsteroidFillType));
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            if (!_isInitialValueSet)
            {
                // only set the position first time opened and cache.
                CenterPositionX = characterPosition.Position.X;
                CenterPositionY = characterPosition.Position.Y;
                CenterPositionZ = characterPosition.Position.Z;
                AsteroidFillType = AsteroidFillType.ByteFiller;
                _isInitialValueSet = true;
            }

            MaterialsCollection.Clear();
            foreach (var material in SpaceEngineersCore.Resources.VoxelMaterialDefinitions)
            {
                MaterialsCollection.Add(new MaterialSelectionModel { Value = material.Id.SubtypeName, DisplayName = material.Id.SubtypeName, IsRare = material.IsRare, MinedRatio = material.MinedOreRatio });
            }

            BaseMaterial = MaterialsCollection.FirstOrDefault(m => m.IsRare == false) ?? MaterialsCollection.FirstOrDefault();

            // Voxel Map Storage, includes stock and mod asteroids.
            var vms = SpaceEngineersCore.Resources.VoxelMapStorageDefinitions;
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            var list = new List<GenerateVoxelDetailModel>();

            foreach (var voxelMap in vms)
            {
                var fileName = SpaceEngineersCore.GetDataPathOrDefault(voxelMap.StorageFile, Path.Combine(contentPath, voxelMap.StorageFile));

                if (!File.Exists(fileName))
                    continue;

                var voxel = new GenerateVoxelDetailModel
                {
                    Name = Path.GetFileNameWithoutExtension(voxelMap.StorageFile),
                    SourceFilename = fileName,
                    FileSize = new FileInfo(fileName).Length,
                    Size = MyVoxelMap.LoadVoxelSize(fileName)
                };
                list.Add(voxel);
            }

            // Custom voxel files directory.
            List<string> files = new List<string>();
            if (!string.IsNullOrEmpty(GlobalSettings.Default.CustomVoxelPath) && Directory.Exists(GlobalSettings.Default.CustomVoxelPath))
	        {
 	            files.AddRange(Directory.GetFiles(GlobalSettings.Default.CustomVoxelPath, "*" + MyVoxelMap.V1FileExtension));
                files.AddRange(Directory.GetFiles(GlobalSettings.Default.CustomVoxelPath, "*" + MyVoxelMap.V2FileExtension));
 	        }

            list.AddRange(files.Select(file => new GenerateVoxelDetailModel
            {
 	            Name = Path.GetFileNameWithoutExtension(file),
 	            SourceFilename = file,
 	            FileSize = new FileInfo(file).Length,
 	            Size = MyVoxelMap.LoadVoxelSize(file)
 	        }));

            VoxelFileList = new ObservableCollection<GenerateVoxelDetailModel>(list.OrderBy(s => s.Name));

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
                    v1.VoxelFile = VoxelFileList.FirstOrDefault(v => v.Name == v1.VoxelFile.Name);
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
                VoxelFile = VoxelFileList[0],
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
