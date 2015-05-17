﻿namespace SEToolbox.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml.Serialization;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Interop;
    using SEToolbox.Support;

    [Serializable]
    public class InventoryEditorModel : BaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private string _name;

        [NonSerialized]
        private bool _isValid;

        [NonSerialized]
        private ObservableCollection<InventoryModel> _items;

        [NonSerialized]
        private InventoryModel _selectedRow;

        [NonSerialized]
        private double _totalVolume;

        [NonSerialized]
        private double _totalMass;

        [NonSerialized]
        private float _maxVolume;

        [NonSerialized]
        private readonly MyObjectBuilder_Inventory _inventory;

        // not required for Cube inventories.
        [NonSerialized]
        private readonly MyObjectBuilder_Character _character;

        #endregion

        #region ctor

        public InventoryEditorModel(bool isValid)
        {
            IsValid = isValid;
        }

        public InventoryEditorModel(MyObjectBuilder_Inventory inventory, float maxVolume, MyObjectBuilder_Character character = null)
        {
            _inventory = inventory;
            _maxVolume = maxVolume;
            _character = character;
            UpdateGeneralFromEntityBase();

            // Cube.InventorySize.X * CUbe.InventorySize.Y * CUbe.InventorySize.Z * 1000 * Sandbox.InventorySizeMultiplier;
            // or Cube.InventoryMaxVolume * 1000 * Sandbox.InventorySizeMultiplier;
            //Character.Inventory = 0.4 * 1000 * Sandbox.InventorySizeMultiplier;
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (value != _name)
                {
                    _name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                return _isValid;
            }

            set
            {
                if (value != _isValid)
                {
                    _isValid = value;
                    RaisePropertyChanged(() => IsValid);
                }
            }
        }

        [XmlIgnore]
        public ObservableCollection<InventoryModel> Items
        {
            get
            {
                return _items;
            }

            set
            {
                if (value != _items)
                {
                    _items = value;
                    RaisePropertyChanged(() => Items);
                }
            }
        }

        [XmlIgnore]
        public InventoryModel SelectedRow
        {
            get
            {
                return _selectedRow;
            }

            set
            {
                if (value != _selectedRow)
                {
                    _selectedRow = value;
                    RaisePropertyChanged(() => SelectedRow);
                }
            }
        }

        [XmlIgnore]
        public double TotalVolume
        {
            get
            {
                return _totalVolume;
            }

            set
            {
                if (value != _totalVolume)
                {
                    _totalVolume = value;
                    RaisePropertyChanged(() => TotalVolume);
                }
            }
        }

        [XmlIgnore]
        public double TotalMass
        {
            get
            {
                return _totalMass;
            }

            set
            {
                if (value != _totalMass)
                {
                    _totalMass = value;
                    RaisePropertyChanged(() => TotalMass);
                }
            }
        }

        [XmlIgnore]
        public float MaxVolume
        {
            get
            {
                return _maxVolume;
            }

            set
            {
                if (value != _maxVolume)
                {
                    _maxVolume = value;
                    RaisePropertyChanged(() => MaxVolume);
                }
            }
        }

        #endregion

        #region methods

        private void UpdateGeneralFromEntityBase()
        {
            var list = new ObservableCollection<InventoryModel>();
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            TotalVolume = 0;
            TotalMass = 0;

            if (_inventory != null)
            {
                foreach (MyObjectBuilder_InventoryItem item in _inventory.Items)
                {
                    list.Add(CreateItem(item, contentPath));
                }
            }

            Items = list;
        }

        private InventoryModel CreateItem(MyObjectBuilder_InventoryItem item, string contentPath)
        {
            var definition = SpaceEngineersApi.GetDefinition(item.Content.TypeId, item.Content.SubtypeName) as MyObjectBuilder_PhysicalItemDefinition;

            string name;
            string textureFile;
            double massMultiplyer;
            double volumeMultiplyer;

            if (definition == null)
            {
                name = item.Content.SubtypeName + " " + item.Content.TypeId.ToString();
                massMultiplyer = 1;
                volumeMultiplyer = 1;
                textureFile = null;
            }
            else
            {
                name = definition.DisplayName;
                massMultiplyer = definition.Mass;
                volumeMultiplyer = definition.Volume.Value;
                textureFile = SpaceEngineersCore.GetDataPathOrDefault(definition.Icon, Path.Combine(contentPath, definition.Icon));
            }

            var newItem = new InventoryModel(item)
            {
                Name = name,
                Amount = (decimal)item.Amount,
                SubtypeId = item.Content.SubtypeName,
                TypeId = item.Content.TypeId,
                MassMultiplyer = massMultiplyer,
                VolumeMultiplyer = volumeMultiplyer,
                TextureFile = textureFile,
                IsUnique = item.Content.TypeId == SpaceEngineersConsts.PhysicalGunObject || item.Content.TypeId == SpaceEngineersConsts.OxygenContainerObject,
                IsInteger = item.Content.TypeId == SpaceEngineersConsts.Component || item.Content.TypeId == SpaceEngineersConsts.AmmoMagazine,
                IsDecimal = item.Content.TypeId == SpaceEngineersConsts.Ore || item.Content.TypeId == SpaceEngineersConsts.Ingot,
                Exists = definition != null, // item no longer exists in Space Engineers definitions.
            };

            TotalVolume += newItem.Volume;
            TotalMass += newItem.Mass;

            return newItem;
        }

        internal void Additem(MyObjectBuilder_InventoryItem item)
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            item.ItemId = _inventory.nextItemId++;
            _inventory.Items.Add(item);
            Items.Add(CreateItem(item, contentPath));
        }

        internal void RemoveItem(int index)
        {
            var invItem = _inventory.Items[index];

            // Remove HandWeapon if item is HandWeapon.
            if (_character != null && _character.HandWeapon != null && invItem.Content.TypeId == SpaceEngineersConsts.PhysicalGunObject)
            {
                if (((MyObjectBuilder_PhysicalGunObject)invItem.PhysicalContent).GunEntity != null &&
                    ((MyObjectBuilder_PhysicalGunObject)invItem.PhysicalContent).GunEntity.EntityId == _character.HandWeapon.EntityId)
                {
                    _character.HandWeapon = null;
                }
            }

            TotalVolume -= Items[index].Volume;
            TotalMass -= Items[index].Mass;
            Items.RemoveAt(index);
            _inventory.Items.RemoveAt(index);
            _inventory.nextItemId--;

            // Re-index ItemId.
            for (uint i = 0; i < _inventory.Items.Count; i++)
            {
                _inventory.Items[(int)i].ItemId = i;
            }
        }

        #endregion
    }
}
