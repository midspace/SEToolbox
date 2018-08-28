namespace SEToolbox.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using VRage;
    using Res = SEToolbox.Properties.Resources;

    public class ImportVoxelModel : BaseModel
    {
        #region Fields

        private string _filename;
        private string _sourceFile;
        private bool _isValidVoxelFile;
        private BindablePoint3DModel _position;
        private BindableVector3DModel _forward;
        private BindableVector3DModel _up;
        private MyPositionAndOrientation _characterPosition;
        private bool _isStockVoxel;
        private bool _isFileVoxel;
        private bool _isSphere;
        private GenerateVoxelDetailModel _stockVoxel;
        private MaterialSelectionModel _stockMaterial;
        private List<GenerateVoxelDetailModel> _voxelFileList;
        private readonly ObservableCollection<MaterialSelectionModel> _materialsCollection;
        private int _sphereRadius;
        private int _sphereShellRadius;

        #endregion

        #region ctor

        public ImportVoxelModel()
        {
            _voxelFileList = new List<GenerateVoxelDetailModel>();
            _materialsCollection = new ObservableCollection<MaterialSelectionModel>
            {
                new MaterialSelectionModel { Value = null, DisplayName = Res.WnImportAsteroidNoChange }
            };

            foreach (var material in SpaceEngineersCore.Resources.VoxelMaterialDefinitions.OrderBy(m => m.Id.SubtypeName))
            {
                _materialsCollection.Add(new MaterialSelectionModel { Value = material.Id.SubtypeName, DisplayName = material.Id.SubtypeName });
            }

            SphereRadius = 150;
            SphereShellRadius = 0;
        }

        #endregion

        #region Properties

        public string Filename
        {
            get { return _filename; }

            set
            {
                if (value != _filename)
                {
                    _filename = value;
                    OnPropertyChanged(nameof(Filename));
                }
            }
        }

        public string SourceFile
        {
            get { return _sourceFile; }

            set
            {
                if (value != _sourceFile)
                {
                    _sourceFile = value;
                    OnPropertyChanged(nameof(SourceFile));
                    if (StockMaterial == null)
                    {
                        StockMaterial = MaterialsCollection[0];
                    }
                }
            }
        }

        public bool IsValidVoxelFile
        {
            get { return _isValidVoxelFile; }

            set
            {
                if (value != _isValidVoxelFile)
                {
                    _isValidVoxelFile = value;
                    OnPropertyChanged(nameof(IsValidVoxelFile));
                }
            }
        }

        public BindablePoint3DModel Position
        {
            get { return _position; }

            set
            {
                if (value != _position)
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        public BindableVector3DModel Forward
        {
            get { return _forward; }

            set
            {
                if (value != _forward)
                {
                    _forward = value;
                    OnPropertyChanged(nameof(Forward));
                }
            }
        }

        public BindableVector3DModel Up
        {
            get { return _up; }

            set
            {
                if (value != _up)
                {
                    _up = value;
                    OnPropertyChanged(nameof(Up));
                }
            }
        }

        public MyPositionAndOrientation CharacterPosition
        {
            get { return _characterPosition; }

            set
            {
                //if (value != characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                _characterPosition = value;
                OnPropertyChanged(nameof(CharacterPosition));
            }
        }

        public bool IsStockVoxel
        {
            get { return _isStockVoxel; }

            set
            {
                if (value != _isStockVoxel)
                {
                    _isStockVoxel = value;
                    OnPropertyChanged(nameof(IsStockVoxel));
                }
            }
        }

        public bool IsFileVoxel
        {
            get { return _isFileVoxel; }

            set
            {
                if (value != _isFileVoxel)
                {
                    _isFileVoxel = value;
                    OnPropertyChanged(nameof(IsFileVoxel));
                }
            }
        }

        public bool IsSphere
        {
            get { return _isSphere; }

            set
            {
                if (value != _isSphere)
                {
                    _isSphere = value;
                    OnPropertyChanged(nameof(IsSphere));
                }
            }
        }

        public GenerateVoxelDetailModel StockVoxel
        {
            get { return _stockVoxel; }

            set
            {
                if (value != _stockVoxel)
                {
                    _stockVoxel = value;
                    OnPropertyChanged(nameof(StockVoxel));
                    IsStockVoxel = true;
                    if (StockMaterial == null)
                    {
                        StockMaterial = MaterialsCollection[0];
                    }
                }
            }
        }

        public List<GenerateVoxelDetailModel> VoxelFileList
        {
            get { return _voxelFileList; }

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
            get { return _materialsCollection; }
        }

        public MaterialSelectionModel StockMaterial
        {
            get { return _stockMaterial; }

            set
            {
                if (value != _stockMaterial)
                {
                    _stockMaterial = value;
                    OnPropertyChanged(nameof(StockMaterial));
                }
            }
        }

        public int SphereRadius
        {
            get { return _sphereRadius; }

            set
            {
                if (value != _sphereRadius)
                {
                    _sphereRadius = value;
                    OnPropertyChanged(nameof(SphereRadius));
                }
            }
        }

        public int SphereShellRadius
        {
            get { return _sphereShellRadius; }

            set
            {
                if (value != _sphereShellRadius)
                {
                    _sphereShellRadius = value;
                    OnPropertyChanged(nameof(SphereShellRadius));
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            CharacterPosition = characterPosition;

            var vms = SpaceEngineersCore.Resources.VoxelMapStorageDefinitions;
            var contentPath = ToolboxUpdater.GetApplicationContentPath();

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
                VoxelFileList.Add(voxel);
            }

            // Custom voxel files directory.
            List<string> files = new List<string>();
            if (!string.IsNullOrEmpty(GlobalSettings.Default.CustomVoxelPath) && Directory.Exists(GlobalSettings.Default.CustomVoxelPath))
            {
                files.AddRange(Directory.GetFiles(GlobalSettings.Default.CustomVoxelPath, "*" + MyVoxelMap.V1FileExtension));
                files.AddRange(Directory.GetFiles(GlobalSettings.Default.CustomVoxelPath, "*" + MyVoxelMap.V2FileExtension));
            }

            VoxelFileList.AddRange(files.Select(file => new GenerateVoxelDetailModel
            {
                Name = Path.GetFileNameWithoutExtension(file),
                SourceFilename = file,
                FileSize = new FileInfo(file).Length,
                Size = MyVoxelMap.LoadVoxelSize(file)
            }));


            VoxelFileList = VoxelFileList.OrderBy(s => s.Name).ToList();
        }

        #endregion
    }
}
