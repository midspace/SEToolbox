namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Xml.Serialization;

    [Serializable]
    public class InventoryModel : BaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private ObservableCollection<ComponentItemModel> _items;

        [NonSerialized]
        private ComponentItemModel _selectedRow;

        [NonSerialized]
        private double _totalVolume;

        [NonSerialized]
        private float _maxVolume;

        [NonSerialized]
        private readonly MySessionSettings _settings;

        [NonSerialized]
        private readonly MyObjectBuilder_Inventory _inventory;

        // not required for Cube inventories.
        [NonSerialized]
        private readonly MyObjectBuilder_Character _character;

        #endregion

        #region ctor

        public InventoryModel(MyObjectBuilder_Inventory inventory, MySessionSettings settings, float maxVolume, MyObjectBuilder_Character character = null)
        {
            this._inventory = inventory;
            this._settings = settings;
            this._maxVolume = maxVolume;
            this._character = character;
            UpdateGeneralFromEntityBase();

            // this.CUbe.InventorySize.X * this.CUbe.InventorySize.Y * this.CUbe.InventorySize.Z * 1000 * Sandbox.InventorySizeMultiplier;
            // or this.Cube.InventoryMaxVolume * 1000 * Sandbox.InventorySizeMultiplier;
            //this.Character.Inventory = 0.4 * 1000 * Sandbox.InventorySizeMultiplier;
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public ObservableCollection<ComponentItemModel> Items
        {
            get
            {
                return this._items;
            }

            set
            {
                if (value != this._items)
                {
                    this._items = value;
                    this.RaisePropertyChanged(() => Items);
                }
            }
        }

        [XmlIgnore]
        public ComponentItemModel SelectedRow
        {
            get
            {
                return this._selectedRow;
            }

            set
            {
                if (value != this._selectedRow)
                {
                    this._selectedRow = value;
                    this.RaisePropertyChanged(() => SelectedRow);
                }
            }
        }

        [XmlIgnore]
        public double TotalVolume
        {
            get
            {
                return this._totalVolume;
            }

            set
            {
                if (value != this._totalVolume)
                {
                    this._totalVolume = value;
                    this.RaisePropertyChanged(() => TotalVolume);
                }
            }
        }

        [XmlIgnore]
        public float MaxVolume
        {
            get
            {
                return this._maxVolume;
            }

            set
            {
                if (value != this._maxVolume)
                {
                    this._maxVolume = value;
                    this.RaisePropertyChanged(() => MaxVolume);
                }
            }
        }

        [XmlIgnore]
        public MySessionSettings Settings
        {
            get { return this._settings; }
        }

        #endregion

        #region methods

        private void UpdateGeneralFromEntityBase()
        {
            var list = new ObservableCollection<ComponentItemModel>();
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            this.TotalVolume = 0;

            if (_inventory != null)
            {
                foreach (MyObjectBuilder_InventoryItem item in _inventory.Items)
                {
                    list.Add(CreateItem(item, contentPath));
                }
            }

            this.Items = list;
        }

        private ComponentItemModel CreateItem(MyObjectBuilder_InventoryItem item, string contentPath)
        {
            var definition = SpaceEngineersAPI.GetDefinition(item.Content.TypeId, item.Content.SubtypeName) as MyObjectBuilder_PhysicalItemDefinition;

            string name;
            string textureFile;
            double mass;
            double volume;

            if (definition == null)
            {
                name = item.Content.SubtypeName + " " + item.Content.TypeId.ToString();
                mass = (double)item.AmountDecimal;
                volume = (double)item.AmountDecimal;
                textureFile = null;
            }
            else
            {
                name = definition.DisplayName;
                mass = definition.Mass*(double) item.AmountDecimal;
                volume = definition.Volume.Value*(double) item.AmountDecimal;
                textureFile = Path.Combine(contentPath, definition.Icon + ".dds");
            }

            var newItem = new ComponentItemModel()
            {
                Name = name,
                Count = item.AmountDecimal,
                SubtypeId = item.Content.SubtypeName,
                TypeId = item.Content.TypeId,
                Mass = mass,
                Volume = volume,
                TextureFile = textureFile,
                Accessible = definition != null, // item no longer exists in Space Engineers definitions.
            };

            this.TotalVolume += newItem.Volume;

            return newItem;
        }

        internal void Additem(MyObjectBuilder_InventoryItem item)
        {
            var contentPath = ToolboxUpdater.GetApplicationContentPath();
            item.ItemId = this._inventory.nextItemId++;
            this._inventory.Items.Add(item);
            this.Items.Add(CreateItem(item, contentPath));
        }

        internal void RemoveItem(int index)
        {
            var invItem = this._inventory.Items[index];

            // Remove HandWeapon if item is HandWeapon.
            if (this._character != null && this._character.HandWeapon != null && invItem.Content.TypeId == MyObjectBuilderTypeEnum.PhysicalGunObject)
            {
                if (((MyObjectBuilder_PhysicalGunObject)invItem.PhysicalContent).GunEntity.EntityId == this._character.HandWeapon.EntityId)
                {
                    this._character.HandWeapon = null;
                }
            }

            this.TotalVolume -= this.Items[index].Volume;
            this.Items.RemoveAt(index);
            this._inventory.Items.RemoveAt(index);
            this._inventory.nextItemId--;

            // Re-index ItemId.
            for (uint i = 0; i < this._inventory.Items.Count; i++)
            {
                this._inventory.Items[0].ItemId = i;
            }
        }

        #endregion
    }
}
