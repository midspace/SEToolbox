using System.Linq;

namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;

    public class GenerateVoxelFieldModel : BaseModel
    {
        private static readonly List<GenerateVoxelModel> VoxelStore;

        #region Fields

        private static int _minimumRange;
        private static int _maximumRange;
        private MyPositionAndOrientation _characterPosition;
        private ObservableCollection<GenerateVoxelDetailModel> _stockVoxelFileList;
        private readonly ObservableCollection<MaterialSelectionModel> _materialsCollection;
        private ObservableCollection<GenerateVoxelModel> _voxelCollection;
        private readonly List<int> _percentList;

        #endregion

        #region ctor

        public GenerateVoxelFieldModel()
        {
            this._stockVoxelFileList = new ObservableCollection<GenerateVoxelDetailModel>();
            this._materialsCollection = new ObservableCollection<MaterialSelectionModel>();
            this._voxelCollection = new ObservableCollection<GenerateVoxelModel>();
            this._percentList = new List<int>();
        }

        static GenerateVoxelFieldModel()
        {
            VoxelStore = new List<GenerateVoxelModel>();
        }

        #endregion

        #region Properties

        public ObservableCollection<GenerateVoxelModel> VoxelCollection
        {
            get
            {
                return this._voxelCollection;
            }

            set
            {
                if (value != this._voxelCollection)
                {
                    this._voxelCollection = value;
                    this.RaisePropertyChanged(() => VoxelCollection);
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
                    this.RaisePropertyChanged(() => MinimumRange);
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
                    this.RaisePropertyChanged(() => MaximumRange);
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

        public ObservableCollection<GenerateVoxelDetailModel> StockVoxelFileList
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

        public ObservableCollection<MaterialSelectionModel> MaterialsCollection
        {
            get
            {
                return this._materialsCollection;
            }
        }

        public List<int> PercentList
        {
            get
            {
                return this._percentList;
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            this.CharacterPosition = characterPosition;

            this.MaterialsCollection.Clear();
            foreach (var material in SpaceEngineersAPI.GetMaterialList())
            {
                this.MaterialsCollection.Add(new MaterialSelectionModel() { Value = material.Name, DisplayName = material.Name });
            }

            var files = Directory.GetFiles(Path.Combine(ToolboxUpdater.GetApplicationFilePath(), @"Content\VoxelMaps"), "*.vox");

            this.StockVoxelFileList.Clear();
            foreach (var file in files)
            {
                this.StockVoxelFileList.Add(new GenerateVoxelDetailModel()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    SourceFilename = file,
                });
            }

            // Set up a default start.
            if (VoxelStore.Count == 0)
            {
                this.VoxelCollection.Add(this.NewDefaultVoxel(1));
            }
            else
            {
                foreach (var item in VoxelStore)
                {
                    var v1 = item.Clone();
                    v1.VoxelFile = this.StockVoxelFileList.FirstOrDefault(v => v.Name == v1.VoxelFile.Name);
                    v1.MainMaterial = this.MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.MainMaterial.DisplayName);
                    v1.SecondMaterial = this.MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.SecondMaterial.DisplayName);
                    v1.ThirdMaterial = this.MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.ThirdMaterial.DisplayName);
                    v1.ForthMaterial = this.MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.ForthMaterial.DisplayName);
                    v1.FifthMaterial = this.MaterialsCollection.FirstOrDefault(v => v.DisplayName == v1.FifthMaterial.DisplayName);
                    this.VoxelCollection.Add(v1);
                }
                this.RenumberCollection();
            }

            for (var i = 0; i < 100; i++)
            {
                this.PercentList.Add(i);
            }

            this.MinimumRange = 400;
            this.MaximumRange = 800;
        }

        public void Unload()
        {
            VoxelStore.Clear();
            VoxelStore.AddRange(this._voxelCollection);
        }

        public GenerateVoxelModel NewDefaultVoxel(int index)
        {
            return new GenerateVoxelModel()
                {
                    Index = index,
                    VoxelFile = this.StockVoxelFileList[0],
                    MainMaterial = this.MaterialsCollection[0],
                    SecondMaterial = this.MaterialsCollection[0],
                    ThirdMaterial = this.MaterialsCollection[0],
                    ForthMaterial = this.MaterialsCollection[0],
                    FifthMaterial = this.MaterialsCollection[0],
                };
        }

        public void RenumberCollection()
        {
            for (var i = 0; i < this.VoxelCollection.Count; i++)
            {
                this.VoxelCollection[i].Index = i + 1;
            }
        }

        #endregion
    }
}
