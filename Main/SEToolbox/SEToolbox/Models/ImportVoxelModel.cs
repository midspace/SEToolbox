namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Support;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    public class ImportVoxelModel : BaseModel
    {
        #region Fields

        private string filename;
        private string sourceFile;
        private bool isValidVoxelFile;
        private BindablePoint3DModel position;
        private BindableVector3DModel forward;
        private BindableVector3DModel up;
        private MyPositionAndOrientation characterPosition;
        private bool isStockVoxel;
        private bool isCustomVoxel;
        private bool isFileVoxel;
        private string stockVoxel;
        private string customVoxel;
        private List<string> stockVoxelFileList;
        private List<string> customVoxelFileList;

        #endregion

        #region ctor

        public ImportVoxelModel()
        {
            stockVoxelFileList = new List<string>();
            customVoxelFileList = new List<string>();
        }

        #endregion

        #region Properties

        public string Filename
        {
            get
            {
                return this.filename;
            }

            set
            {
                if (value != this.filename)
                {
                    this.filename = value;
                    this.RaisePropertyChanged(() => Filename);
                }
            }
        }

        public string SourceFile
        {
            get
            {
                return this.sourceFile;
            }

            set
            {
                if (value != this.sourceFile)
                {
                    this.sourceFile = value;
                    this.RaisePropertyChanged(() => SourceFile);
                }
            }
        }

        public bool IsValidVoxelFile
        {
            get
            {
                return this.isValidVoxelFile;
            }

            set
            {
                if (value != this.isValidVoxelFile)
                {
                    this.isValidVoxelFile = value;
                    this.RaisePropertyChanged(() => IsValidVoxelFile);
                }
            }
        }

        public BindablePoint3DModel Position
        {
            get
            {
                return this.position;
            }

            set
            {
                if (value != this.position)
                {
                    this.position = value;
                    this.RaisePropertyChanged(() => Position);
                }
            }
        }

        public BindableVector3DModel Forward
        {
            get
            {
                return this.forward;
            }

            set
            {
                if (value != this.forward)
                {
                    this.forward = value;
                    this.RaisePropertyChanged(() => Forward);
                }
            }
        }

        public BindableVector3DModel Up
        {
            get
            {
                return this.up;
            }

            set
            {
                if (value != this.up)
                {
                    this.up = value;
                    this.RaisePropertyChanged(() => Up);
                }
            }
        }

        public MyPositionAndOrientation CharacterPosition
        {
            get
            {
                return this.characterPosition;
            }

            set
            {
                //if (value != this.characterPosition) // Unable to check for equivilence, without long statement. And, mostly uncessary.
                this.characterPosition = value;
                this.RaisePropertyChanged(() => CharacterPosition);
            }
        }

        public bool IsStockVoxel
        {
            get
            {
                return this.isStockVoxel;
            }

            set
            {
                if (value != this.isStockVoxel)
                {
                    this.isStockVoxel = value;
                    this.RaisePropertyChanged(() => IsStockVoxel);
                }
            }
        }

        public bool IsCustomVoxel
        {
            get
            {
                return this.isCustomVoxel;
            }

            set
            {
                if (value != this.isCustomVoxel)
                {
                    this.isCustomVoxel = value;
                    this.RaisePropertyChanged(() => IsCustomVoxel);
                }
            }
        }

        public bool IsFileVoxel
        {
            get
            {
                return this.isFileVoxel;
            }

            set
            {
                if (value != this.isFileVoxel)
                {
                    this.isFileVoxel = value;
                    this.RaisePropertyChanged(() => IsFileVoxel);
                }
            }
        }

        public string StockVoxel
        {
            get
            {
                return this.stockVoxel;
            }

            set
            {
                if (value != this.stockVoxel)
                {
                    this.stockVoxel = value;
                    this.RaisePropertyChanged(() => StockVoxel);
                    this.IsStockVoxel = true;
                }
            }
        }

        public string CustomVoxel
        {
            get
            {
                return this.customVoxel;
            }

            set
            {
                if (value != this.customVoxel)
                {
                    this.customVoxel = value;
                    this.RaisePropertyChanged(() => CustomVoxel);
                    this.IsCustomVoxel = true;
                }
            }
        }

        public List<string> StockVoxelFileList
        {
            get
            {
                return this.stockVoxelFileList;
            }

            set
            {
                if (value != this.stockVoxelFileList)
                {
                    this.stockVoxelFileList = value;
                    this.RaisePropertyChanged(() => StockVoxelFileList);
                }
            }
        }

        public List<string> CustomVoxelFileList
        {
            get
            {
                return this.customVoxelFileList;
            }

            set
            {
                if (value != this.customVoxelFileList)
                {
                    this.customVoxelFileList = value;
                    this.RaisePropertyChanged(() => CustomVoxelFileList);
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
