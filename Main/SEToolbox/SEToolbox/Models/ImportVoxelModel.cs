namespace SEToolbox.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
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
        private string _stockVoxel;
        private MaterialSelectionModel _stockMaterial;
        private List<string> _stockVoxelFileList;
        private readonly ObservableCollection<MaterialSelectionModel> _materialsCollection;
        private int _sphereRadius;
        private int _sphereShellRadius;

        #endregion

        #region ctor

        public ImportVoxelModel()
        {
            _stockVoxelFileList = new List<string>();
            _materialsCollection = new ObservableCollection<MaterialSelectionModel>
            {
                new MaterialSelectionModel {Value = null, DisplayName = "No change"}
            };

            foreach (var material in SpaceEngineersCore.Resources.GetMaterialList())
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

        public string StockVoxel
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

        public List<string> StockVoxelFileList
        {
            get { return _stockVoxelFileList; }

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
            var files = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationContentPath(), @"VoxelMaps"), "*.vox");

            foreach (var file in files)
            {
                StockVoxelFileList.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        #endregion
    }
}
