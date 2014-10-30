﻿namespace SEToolbox.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;

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
                new MaterialSelectionModel {Value = null, DisplayName = "No change"}
            };

            foreach (var material in SpaceEngineersCore.Resources.GetMaterialList().OrderBy(m => m.Id.SubtypeId))
            {
                _materialsCollection.Add(new MaterialSelectionModel { Value = material.Id.SubtypeId, DisplayName = material.Id.SubtypeId });
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
                    RaisePropertyChanged(() => Filename);
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
                    RaisePropertyChanged(() => SourceFile);
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
                    RaisePropertyChanged(() => IsValidVoxelFile);
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
                    RaisePropertyChanged(() => Position);
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
                    RaisePropertyChanged(() => Forward);
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
                    RaisePropertyChanged(() => Up);
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
                RaisePropertyChanged(() => CharacterPosition);
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
                    RaisePropertyChanged(() => IsStockVoxel);
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
                    RaisePropertyChanged(() => IsFileVoxel);
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
                    RaisePropertyChanged(() => IsSphere);
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
                    RaisePropertyChanged(() => StockVoxel);
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
                    RaisePropertyChanged(() => VoxelFileList);
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
                    RaisePropertyChanged(() => StockMaterial);
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
                    RaisePropertyChanged(() => SphereRadius);
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
                    RaisePropertyChanged(() => SphereShellRadius);
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            CharacterPosition = characterPosition;
            var filesV1 = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationContentPath(), @"VoxelMaps"), "*" + MyVoxelMap.V1FileExtension);
            var filesV2 = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationContentPath(), @"VoxelMaps"), "*" + MyVoxelMap.V2FileExtension);
            var files = filesV1.Concat(filesV2);

            if (!string.IsNullOrEmpty(GlobalSettings.Default.CustomVoxelPath) && Directory.Exists(GlobalSettings.Default.CustomVoxelPath))
            {
                files = files.Concat(Directory.GetFiles(GlobalSettings.Default.CustomVoxelPath, "*" + MyVoxelMap.V1FileExtension));
                files = files.Concat(Directory.GetFiles(GlobalSettings.Default.CustomVoxelPath, "*" + MyVoxelMap.V2FileExtension));
            }

            foreach (var file in files)
            {
                var voxel = new GenerateVoxelDetailModel
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    SourceFilename = file,
                    FileSize = new FileInfo(file).Length,
                    Size = MyVoxelMap.LoadVoxelSize(file)
                };
                VoxelFileList.Add(voxel);
            }

            VoxelFileList = VoxelFileList.OrderBy(s => s.Name).ToList();
        }

        #endregion
    }
}
