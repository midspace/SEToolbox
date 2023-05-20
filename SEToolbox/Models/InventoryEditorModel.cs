namespace SEToolbox.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Sandbox.Definitions;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage.Game;

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
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        [XmlIgnore]
        public bool IsValid
        {
            get { return _isValid; }

            set
            {
                if (value != _isValid)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
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
                    OnPropertyChanged(nameof(Items));
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
                    OnPropertyChanged(nameof(SelectedRow));
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
                    OnPropertyChanged(nameof(TotalVolume));
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
                    OnPropertyChanged(nameof(TotalMass));
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
                    OnPropertyChanged(nameof(MaxVolume));
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
            var definition = MyDefinitionManager.Static.GetDefinition(item.PhysicalContent.TypeId, item.PhysicalContent.SubtypeName) as MyPhysicalItemDefinition;

            string name;
            string textureFile;
            double massMultiplyer;
            double volumeMultiplyer;

            if (definition == null)
            {
                name = item.PhysicalContent.SubtypeName + " " + item.PhysicalContent.TypeId;
                massMultiplyer = 1;
                volumeMultiplyer = 1;
                textureFile = null;
            }
            else
            {
                name = definition.DisplayNameText;
                massMultiplyer = definition.Mass;
                volumeMultiplyer = definition.Volume * SpaceEngineersConsts.VolumeMultiplyer;
                textureFile = (definition.Icons == null || definition.Icons.First() == null) ? null : SpaceEngineersCore.GetDataPathOrDefault(definition.Icons.First(), Path.Combine(contentPath, definition.Icons.First()));
            }

            var newItem = new InventoryModel(item)
            {
                Name = name,
                Amount = (decimal)item.Amount,
                SubtypeId = item.PhysicalContent.SubtypeName,
                TypeId = item.PhysicalContent.TypeId,
                MassMultiplyer = massMultiplyer,
                VolumeMultiplyer = volumeMultiplyer,
                TextureFile = textureFile,
                IsUnique = item.PhysicalContent.TypeId == SpaceEngineersTypes.PhysicalGunObject || item.PhysicalContent.TypeId == SpaceEngineersTypes.OxygenContainerObject,
                IsInteger = item.PhysicalContent.TypeId == SpaceEngineersTypes.Component || item.PhysicalContent.TypeId == SpaceEngineersTypes.AmmoMagazine,
                IsDecimal = item.PhysicalContent.TypeId == SpaceEngineersTypes.Ore || item.PhysicalContent.TypeId == SpaceEngineersTypes.Ingot,
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
            if (_character != null && _character.HandWeapon != null && invItem.PhysicalContent.TypeId == SpaceEngineersTypes.PhysicalGunObject)
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
