﻿namespace SEToolbox.Models
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
        private ObservableCollection<ComponentItemModel> _stockItemList;
        private ComponentItemModel _stockItem;

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
            _stockItemList = new ObservableCollection<ComponentItemModel>();
            Multiplier = 1;
        }

        #endregion

        #region Properties

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

        public ObservableCollection<ComponentItemModel> StockItemList
        {
            get
            {
                return _stockItemList;
            }

            set
            {
                if (value != _stockItemList)
                {
                    _stockItemList = value;
                    RaisePropertyChanged(() => StockItemList);
                }
            }
        }

        public ComponentItemModel StockItem
        {
            get
            {
                return _stockItem;
            }

            set
            {
                if (value != _stockItem)
                {
                    _stockItem = value;
                    RaisePropertyChanged(() => StockItem);
                    SetMassVolume();
                }
            }
        }

        public bool IsValidItemToImport
        {
            get
            {
                return _isValidItemToImport;
            }

            set
            {
                if (value != _isValidItemToImport)
                {
                    _isValidItemToImport = value;
                    RaisePropertyChanged(() => IsValidItemToImport);
                }
            }
        }

        public double? Volume
        {
            get
            {
                return _volume;
            }

            set
            {
                if (value != _volume)
                {
                    _volume = value;
                    RaisePropertyChanged(() => Volume);
                }
            }
        }

        public double? Mass
        {
            get
            {
                return _mass;
            }

            set
            {
                if (value != _mass)
                {
                    _mass = value;
                    RaisePropertyChanged(() => Mass);
                }
            }
        }

        public int? Units
        {
            get
            {
                return _units;
            }

            set
            {
                if (value != _units)
                {
                    _units = value;
                    RaisePropertyChanged(() => Units);
                    SetMassVolume();
                }
            }
        }

        public decimal? DecimalUnits
        {
            get
            {
                return _decimalUnits;
            }

            set
            {
                if (value != _decimalUnits)
                {
                    _decimalUnits = value;
                    RaisePropertyChanged(() => DecimalUnits);
                    SetMassVolume();
                }
            }
        }

        public bool IsDecimal
        {
            get
            {
                return _isDecimal;
            }

            set
            {
                if (value != _isDecimal)
                {
                    _isDecimal = value;
                    RaisePropertyChanged(() => IsDecimal);
                }
            }
        }

        public bool IsInt
        {
            get
            {
                return _isInt;
            }

            set
            {
                if (value != _isInt)
                {
                    _isInt = value;
                    RaisePropertyChanged(() => IsInt);
                }
            }
        }

        public bool IsUnique
        {
            get
            {
                return _isUnique;
            }

            set
            {
                if (value != _isUnique)
                {
                    _isUnique = value;
                    RaisePropertyChanged(() => IsUnique);
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
                return _multiplier;
            }

            set
            {
                if (value != _multiplier)
                {
                    _multiplier = value;
                    RaisePropertyChanged(() => Multiplier);
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
                return _maxFloatingObjects;
            }

            set
            {
                if (value != _maxFloatingObjects)
                {
                    _maxFloatingObjects = value;
                    RaisePropertyChanged(() => MaxFloatingObjects);
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition, float maxFloatingObjects)
        {
            MaxFloatingObjects = maxFloatingObjects;
            CharacterPosition = characterPosition;
            StockItemList.Clear();
            var list = new List<ComponentItemModel>();
            var contentPath = ToolboxUpdater.GetApplicationContentPath();

            foreach (var componentDefinition in SpaceEngineersCore.Resources.Definitions.Components)
            {
                var bp = SpaceEngineersApi.GetBlueprint(componentDefinition.Id.TypeId, componentDefinition.Id.SubtypeId);
                list.Add(new ComponentItemModel
                {
                    Name = componentDefinition.DisplayName,
                    TypeId = componentDefinition.Id.TypeId,
                    SubtypeId = componentDefinition.Id.SubtypeId,
                    Mass = componentDefinition.Mass,
                    TextureFile = componentDefinition.Icon == null ? null : SpaceEngineersCore.GetDataPathOrDefault(componentDefinition.Icon, Path.Combine(contentPath, componentDefinition.Icon)),
                    Volume = componentDefinition.Volume.HasValue ? componentDefinition.Volume.Value : 0f,
                    Accessible = componentDefinition.Public,
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersCore.Resources.Definitions.PhysicalItems)
            {
                if (physicalItemDefinition.Id.SubtypeId == "CubePlacerItem" || physicalItemDefinition.Id.SubtypeId == "WallPlacerItem")
                    continue;

                var bp = SpaceEngineersApi.GetBlueprint(physicalItemDefinition.Id.TypeId, physicalItemDefinition.Id.SubtypeId);
                list.Add(new ComponentItemModel
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = physicalItemDefinition.Icon == null ? null : SpaceEngineersCore.GetDataPathOrDefault(physicalItemDefinition.Icon, Path.Combine(contentPath, physicalItemDefinition.Icon)),
                    Accessible = physicalItemDefinition.Public,
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var physicalItemDefinition in SpaceEngineersCore.Resources.Definitions.AmmoMagazines)
            {
                var bp = SpaceEngineersApi.GetBlueprint(physicalItemDefinition.Id.TypeId, physicalItemDefinition.Id.SubtypeId);
                list.Add(new ComponentItemModel
                {
                    Name = physicalItemDefinition.DisplayName,
                    TypeId = physicalItemDefinition.Id.TypeId,
                    SubtypeId = physicalItemDefinition.Id.SubtypeId,
                    Mass = physicalItemDefinition.Mass,
                    Volume = physicalItemDefinition.Volume.HasValue ? physicalItemDefinition.Volume.Value : 0f,
                    TextureFile = physicalItemDefinition.Icon == null ? null : SpaceEngineersCore.GetDataPathOrDefault(physicalItemDefinition.Icon, Path.Combine(contentPath, physicalItemDefinition.Icon)),
                    Accessible = !string.IsNullOrEmpty(physicalItemDefinition.Model),
                    Time = bp != null ? new TimeSpan((long)(TimeSpan.TicksPerSecond * bp.BaseProductionTimeInSeconds)) : (TimeSpan?)null,
                });
            }

            foreach (var item in list.OrderBy(i => i.FriendlyName))
            {
                StockItemList.Add(item);
            }

            //list.Clear();

            //foreach (var cubeDefinition in SpaceEngineersAPI.CubeBlockDefinitions)
            //{
            //    list.Add(new ComponentItemModel
            //    {
            //        Name = cubeDefinition.DisplayName,
            //        TypeId = cubeDefinition.Id.TypeId,
            //        SubtypeId = cubeDefinition.Id.SubtypeId,
            //        CubeSize = cubeDefinition.CubeSize,
            //        TextureFile = cubeDefinition.Icon == null ? null : Path.Combine(contentPath, cubeDefinition.Icon),
            //        Accessible = !string.IsNullOrEmpty(cubeDefinition.Model),
            //    });
            //}

            //foreach (var item in list.OrderBy(i => i.FriendlyName))
            //{
            //    StockItemList.Add(item);
            //}
        }

        private void SetMassVolume()
        {
            if (StockItem == null)
            {
                Mass = null;
                Volume = null;
            }
            else
            {
                if (StockItem.TypeId == SpaceEngineersConsts.Ore ||
                    StockItem.TypeId == SpaceEngineersConsts.Ingot)
                {
                    IsDecimal = true;
                    IsUnique = IsInt = false;
                    if (DecimalUnits.HasValue)
                    {
                        Mass = (double)DecimalUnits * StockItem.Mass;
                        Volume = (double)DecimalUnits * StockItem.Volume;
                    }
                    else
                    {
                        Mass = null;
                        Volume = null;
                    }
                }
                else if (StockItem.TypeId == SpaceEngineersConsts.Component ||
                    StockItem.TypeId == SpaceEngineersConsts.AmmoMagazine)
                {
                    IsInt = true;
                    IsUnique = IsDecimal = false;
                    if (Units.HasValue)
                    {
                        Mass = Units.Value * StockItem.Mass;
                        Volume = Units.Value * StockItem.Volume;
                    }
                    else
                    {
                        Mass = null;
                        Volume = null;
                    }
                }
                else if (StockItem.TypeId == SpaceEngineersConsts.PhysicalGunObject)
                {
                    IsUnique = true;
                    IsInt = IsDecimal = false;
                    Mass = UniqueUnits * StockItem.Mass;
                    Volume = UniqueUnits * StockItem.Volume;
                }
            }
        }

        #endregion
    }
}
