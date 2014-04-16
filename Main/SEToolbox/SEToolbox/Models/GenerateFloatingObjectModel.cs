namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;

    public class GenerateFloatingObjectModel : BaseModel
    {
        #region Fields

        private MyPositionAndOrientation _characterPosition;
        private ObservableCollection<ComonentItemModel> _stockItemList;
        private ComonentItemModel _stockItem;

        private bool _isValidItemToImport;

        private double? _volume;
        private double? _mass;
        private decimal? _units;

        #endregion

        #region ctor

        public GenerateFloatingObjectModel()
        {
            this._stockItemList = new ObservableCollection<ComonentItemModel>();
        }

        #endregion

        #region Properties

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

        public ObservableCollection<ComonentItemModel> StockItemList
        {
            get
            {
                return this._stockItemList;
            }

            set
            {
                if (value != this._stockItemList)
                {
                    this._stockItemList = value;
                    this.RaisePropertyChanged(() => StockItemList);
                }
            }
        }

        public ComonentItemModel StockItem
        {
            get
            {
                return this._stockItem;
            }

            set
            {
                if (value != this._stockItem)
                {
                    this._stockItem = value;
                    this.RaisePropertyChanged(() => StockItem);
                    SetMassVolume();
                }
            }
        }

        public bool IsValidItemToImport
        {
            get
            {
                return this._isValidItemToImport;
            }

            set
            {
                if (value != this._isValidItemToImport)
                {
                    this._isValidItemToImport = value;
                    this.RaisePropertyChanged(() => IsValidItemToImport);
                }
            }
        }

        public double? Volume
        {
            get
            {
                return this._volume;
            }

            set
            {
                if (value != this._volume)
                {
                    this._volume = value;
                    this.RaisePropertyChanged(() => Volume);
                }
            }
        }

        public double? Mass
        {
            get
            {
                return this._mass;
            }

            set
            {
                if (value != this._mass)
                {
                    this._mass = value;
                    this.RaisePropertyChanged(() => Mass);
                }
            }
        }

        public decimal? Units
        {
            get
            {
                return this._units;
            }

            set
            {
                if (value != this._units)
                {
                    this._units = value;
                    this.RaisePropertyChanged(() => Units);
                    SetMassVolume();
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            this.CharacterPosition = characterPosition;
            this.StockItemList.Clear();
            var list = new SortedList<string, ComonentItemModel>();
            var contentPath = Path.Combine(ToolboxUpdater.GetApplicationFilePath(), "Content");

            foreach (var componentDefinition in SpaceEngineersAPI.ComponentDefinitions)
            {
                var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == componentDefinition.Id.SubtypeId && b.Result.TypeId == componentDefinition.Id.TypeId);
                list.Add(componentDefinition.DisplayName, new ComonentItemModel()
                {
                    Name = componentDefinition.DisplayName,
                    TypeId = componentDefinition.Id.TypeId,
                    SubtypeId = componentDefinition.Id.SubtypeId,
                    Mass = componentDefinition.Mass,
                    TextureFile = componentDefinition.Icon == null ? null : Path.Combine(contentPath, componentDefinition.Icon + ".dds"),
                    Volume = componentDefinition.Volume.HasValue ? componentDefinition.Volume.Value : 0f,
                    Accessible = componentDefinition.Public,
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersAPI.PhysicalItemDefinitions)
            {
                if (physicalItemDefinition.Id.SubtypeId == "CubePlacerItem")
                    continue;

                var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == physicalItemDefinition.Id.SubtypeId && b.Result.TypeId == physicalItemDefinition.Id.TypeId);
                list.Add(physicalItemDefinition.DisplayName, new ComonentItemModel()
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = physicalItemDefinition.Icon == null ? null : Path.Combine(contentPath, physicalItemDefinition.Icon + ".dds"),
                    Accessible = physicalItemDefinition.Public,
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersAPI.AmmoMagazineDefinitions)
            {
                var bp = SpaceEngineersAPI.BlueprintDefinitions.FirstOrDefault(b => b.Result.SubtypeId == physicalItemDefinition.Id.SubtypeId && b.Result.TypeId == physicalItemDefinition.Id.TypeId);
                list.Add(physicalItemDefinition.DisplayName, new ComonentItemModel()
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = physicalItemDefinition.Icon == null ? null : Path.Combine(contentPath, physicalItemDefinition.Icon + ".dds"),
                    Accessible = !string.IsNullOrEmpty(physicalItemDefinition.Model),
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var kvp in list)
            {
                this.StockItemList.Add(kvp.Value);
            }
        }

        private void SetMassVolume()
        {
            if (this._stockItem == null)
            {
                this.Mass = null;
                this.Volume = null;
            }
            else
            {
                this.Mass = (double)this.Units * this._stockItem.Mass;
                this.Volume = (double)this.Units * this._stockItem.Volume;
            }
        }

        #endregion
    }
}
