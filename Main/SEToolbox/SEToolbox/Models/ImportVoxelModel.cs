namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;

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
        private bool _isCustomVoxel;
        private bool _isFileVoxel;
        private string _stockVoxel;
        private MaterialSelectionModel _stockMaterial;
        private string _customVoxel;
        private List<string> _stockVoxelFileList;
        private List<string> _customVoxelFileList;
        private readonly ObservableCollection<MaterialSelectionModel> _materialsCollection;

        #endregion

        #region ctor

        public ImportVoxelModel()
        {
            _stockVoxelFileList = new List<string>();
            _customVoxelFileList = new List<string>();
            _materialsCollection = new ObservableCollection<MaterialSelectionModel>
            {
                new MaterialSelectionModel() {Value = null, DisplayName = "No change"}
            };

            foreach (var material in SpaceEngineersAPI.GetMaterialList())
            {
                _materialsCollection.Add(new MaterialSelectionModel() { Value = material.Name, DisplayName = material.Name });
            }
        }

        #endregion

        #region Properties

        public string Filename
        {
            get
            {
                return this._filename;
            }

            set
            {
                if (value != this._filename)
                {
                    this._filename = value;
                    this.RaisePropertyChanged(() => Filename);
                }
            }
        }

        public string SourceFile
        {
            get
            {
                return this._sourceFile;
            }

            set
            {
                if (value != this._sourceFile)
                {
                    this._sourceFile = value;
                    this.RaisePropertyChanged(() => SourceFile);
                }
            }
        }

        public bool IsValidVoxelFile
        {
            get
            {
                return this._isValidVoxelFile;
            }

            set
            {
                if (value != this._isValidVoxelFile)
                {
                    this._isValidVoxelFile = value;
                    this.RaisePropertyChanged(() => IsValidVoxelFile);
                }
            }
        }

        public BindablePoint3DModel Position
        {
            get
            {
                return this._position;
            }

            set
            {
                if (value != this._position)
                {
                    this._position = value;
                    this.RaisePropertyChanged(() => Position);
                }
            }
        }

        public BindableVector3DModel Forward
        {
            get
            {
                return this._forward;
            }

            set
            {
                if (value != this._forward)
                {
                    this._forward = value;
                    this.RaisePropertyChanged(() => Forward);
                }
            }
        }

        public BindableVector3DModel Up
        {
            get
            {
                return this._up;
            }

            set
            {
                if (value != this._up)
                {
                    this._up = value;
                    this.RaisePropertyChanged(() => Up);
                }
            }
        }

        public MyPositionAndOrientation CharacterPosition
        {
            get
            {
                return this._characterPosition;
            }

            set
            {
                //if (value != this.characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                this._characterPosition = value;
                this.RaisePropertyChanged(() => CharacterPosition);
            }
        }

        public bool IsStockVoxel
        {
            get
            {
                return this._isStockVoxel;
            }

            set
            {
                if (value != this._isStockVoxel)
                {
                    this._isStockVoxel = value;
                    this.RaisePropertyChanged(() => IsStockVoxel);
                }
            }
        }

        public bool IsCustomVoxel
        {
            get
            {
                return this._isCustomVoxel;
            }

            set
            {
                if (value != this._isCustomVoxel)
                {
                    this._isCustomVoxel = value;
                    this.RaisePropertyChanged(() => IsCustomVoxel);
                }
            }
        }

        public bool IsFileVoxel
        {
            get
            {
                return this._isFileVoxel;
            }

            set
            {
                if (value != this._isFileVoxel)
                {
                    this._isFileVoxel = value;
                    this.RaisePropertyChanged(() => IsFileVoxel);
                }
            }
        }

        public string StockVoxel
        {
            get
            {
                return this._stockVoxel;
            }

            set
            {
                if (value != this._stockVoxel)
                {
                    this._stockVoxel = value;
                    this.RaisePropertyChanged(() => StockVoxel);
                    this.IsStockVoxel = true;
                    if (this.StockMaterial == null)
                    {
                        this.StockMaterial = this.MaterialsCollection[0];
                    }
                }
            }
        }

        public string CustomVoxel
        {
            get
            {
                return this._customVoxel;
            }

            set
            {
                if (value != this._customVoxel)
                {
                    this._customVoxel = value;
                    this.RaisePropertyChanged(() => CustomVoxel);
                    this.IsCustomVoxel = true;
                }
            }
        }

        public List<string> StockVoxelFileList
        {
            get
            {
                return this._stockVoxelFileList;
            }

            set
            {
                if (value != this._stockVoxelFileList)
                {
                    this._stockVoxelFileList = value;
                    this.RaisePropertyChanged(() => StockVoxelFileList);
                }
            }
        }

        public List<string> CustomVoxelFileList
        {
            get
            {
                return this._customVoxelFileList;
            }

            set
            {
                if (value != this._customVoxelFileList)
                {
                    this._customVoxelFileList = value;
                    this.RaisePropertyChanged(() => CustomVoxelFileList);
                }
            }
        }

        public ObservableCollection<MaterialSelectionModel> MaterialsCollection
        {
            get
            {
                return this._materialsCollection;
            }
        }

        public MaterialSelectionModel StockMaterial
        {
            get
            {
                return this._stockMaterial;
            }

            set
            {
                if (value != this._stockMaterial)
                {
                    this._stockMaterial = value;
                    this.RaisePropertyChanged(() => StockMaterial);
                    this.IsStockVoxel = true;
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            this.CharacterPosition = characterPosition;
            var files = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationFilePath(), @"Content\VoxelMaps"), "*.vox");

            foreach (var file in files)
            {
                this.StockVoxelFileList.Add(Path.GetFileNameWithoutExtension(file));
            }

            var resourceSet = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, false, false);

            foreach (DictionaryEntry entry in resourceSet)
            {
                var name = (string)entry.Key;
                if (name.StartsWith("asteroid_", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.CustomVoxelFileList.Add(name);
                }
            }
        }

        #endregion
    }
}
