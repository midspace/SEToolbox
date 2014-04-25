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
        public const int UniqueUnits = 1;

        #region Fields

        private MyPositionAndOrientation _characterPosition;
        private ObservableCollection<ComonentItemModel> _stockItemList;
        private ComonentItemModel _stockItem;

        private bool _isValidItemToImport;

        private double? _volume;
        private double? _mass;
        private int? _units;
        private decimal? _decimalUnits;
        private bool _isDecimal;
        private bool _isInt;
        private bool _isUnique;
        private int _multiplier;
        private float _maxFloatingObjects;

        #endregion

        #region ctor

        public GenerateFloatingObjectModel()
        {
            this._stockItemList = new ObservableCollection<ComonentItemModel>();
            this.Multiplier = 1;
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

        public int? Units
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

        public decimal? DecimalUnits
        {
            get
            {
                return this._decimalUnits;
            }

            set
            {
                if (value != this._decimalUnits)
                {
                    this._decimalUnits = value;
                    this.RaisePropertyChanged(() => DecimalUnits);
                    SetMassVolume();
                }
            }
        }

        public bool IsDecimal
        {
            get
            {
                return this._isDecimal;
            }

            set
            {
                if (value != this._isDecimal)
                {
                    this._isDecimal = value;
                    this.RaisePropertyChanged(() => IsDecimal);
                }
            }
        }

        public bool IsInt
        {
            get
            {
                return this._isInt;
            }

            set
            {
                if (value != this._isInt)
                {
                    this._isInt = value;
                    this.RaisePropertyChanged(() => IsInt);
                }
            }
        }

        public bool IsUnique
        {
            get
            {
                return this._isUnique;
            }

            set
            {
                if (value != this._isUnique)
                {
                    this._isUnique = value;
                    this.RaisePropertyChanged(() => IsUnique);
                }
            }
        }

        /// <summary>
        /// Generates this many individual items.
        /// </summary>
        public int Multiplier
        {
            get
            {
                return this._multiplier;
            }

            set
            {
                if (value != this._multiplier)
                {
                    this._multiplier = value;
                    this.RaisePropertyChanged(() => Multiplier);
                }
            }
        }

        /// <summary>
        /// The maximum number of floating objects as defined in the World.
        /// </summary>
        public float MaxFloatingObjects
        {
            get
            {
                return this._maxFloatingObjects;
            }

            set
            {
                if (value != this._maxFloatingObjects)
                {
                    this._maxFloatingObjects = value;
                    this.RaisePropertyChanged(() => MaxFloatingObjects);
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition, float maxFloatingObjects)
        {
            this.MaxFloatingObjects = maxFloatingObjects;
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
            if (this.StockItem == null)
            {
                this.Mass = null;
                this.Volume = null;
            }
            else
            {
                if (this.StockItem.TypeId == MyObjectBuilderTypeEnum.Ore ||
                    this.StockItem.TypeId == MyObjectBuilderTypeEnum.Ingot)
                {
                    this.IsDecimal = true;
                    this.IsUnique = this.IsInt = false;
                    if (this.DecimalUnits.HasValue)
                    {
                        this.Mass = (double)this.DecimalUnits * this.StockItem.Mass;
                        this.Volume = (double)this.DecimalUnits * this.StockItem.Volume;
                    }
                    else
                    {
                        this.Mass = null;
                        this.Volume = null;
                    }
                }
                else if (this.StockItem.TypeId == MyObjectBuilderTypeEnum.Component ||
                    this.StockItem.TypeId == MyObjectBuilderTypeEnum.AmmoMagazine)
                {
                    this.IsInt = true;
                    this.IsUnique = this.IsDecimal = false;
                    if (this.Units.HasValue)
                    {
                        this.Mass = (double)this.Units * this.StockItem.Mass;
                        this.Volume = (double)this.Units * this.StockItem.Volume;
                    }
                    else
                    {
                        this.Mass = null;
                        this.Volume = null;
                    }
                }
                else if (this.StockItem.TypeId == MyObjectBuilderTypeEnum.PhysicalGunObject)
                {
                    this.IsUnique = true;
                    this.IsInt = this.IsDecimal = false;
                    this.Mass = (double)UniqueUnits * this.StockItem.Mass;
                    this.Volume = (double)UniqueUnits * this.StockItem.Volume;
                }
            }
        }

        #endregion
    }
}
