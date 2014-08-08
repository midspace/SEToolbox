namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Interop;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable]
    public class StructureCharacterModel : StructureBaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private bool _isPlayer;

        [NonSerialized]
        private bool _isPilot;

        [NonSerialized]
        private List<string> _characterModels;

        [NonSerialized]
        private InventoryEditorModel _inventory;

        #endregion

        #region ctor

        public StructureCharacterModel(MyObjectBuilder_EntityBase entityBase, MySessionSettings settings)
            : base(entityBase, settings)
        {
            this.CharacterModels = new List<string>(Enum.GetNames(typeof(MyCharacterModelEnum)));
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_Character Character
        {
            get
            {
                return this.EntityBase as MyObjectBuilder_Character;
            }
        }

        [XmlIgnore]
        public SerializableVector3 Color
        {
            get
            {
                return this.Character.ColorMaskHSV;
            }

            set
            {
                if (!EqualityComparer<SerializableVector3>.Default.Equals(value, this.Character.ColorMaskHSV))
                {
                    this.Character.ColorMaskHSV = value;
                    this.RaisePropertyChanged(() => Color);
                    this.UpdateGeneralFromEntityBase();
                }
            }
        }

        [XmlIgnore]
        public bool Light
        {
            get
            {
                return this.Character.LightEnabled;
            }

            set
            {
                if (value != this.Character.LightEnabled)
                {
                    this.Character.LightEnabled = value;
                    this.RaisePropertyChanged(() => Light);
                }
            }
        }

        [XmlIgnore]
        public bool JetPack
        {
            get
            {
                return this.Character.JetpackEnabled;
            }

            set
            {
                if (value != this.Character.JetpackEnabled)
                {
                    this.Character.JetpackEnabled = value;
                    this.RaisePropertyChanged(() => JetPack);
                }
            }
        }

        [XmlIgnore]
        public bool Dampeners
        {
            get
            {
                return this.Character.DampenersEnabled;
            }

            set
            {
                if (value != this.Character.DampenersEnabled)
                {
                    this.Character.DampenersEnabled = value;
                    this.RaisePropertyChanged(() => Dampeners);
                }
            }
        }

        [XmlIgnore]
        public float BatteryCapacity
        {
            get
            {
                return this.Character.Battery.CurrentCapacity;
            }

            set
            {
                if (value != this.Character.Battery.CurrentCapacity)
                {
                    this.Character.Battery.CurrentCapacity = value;
                    this.RaisePropertyChanged(() => BatteryCapacity);
                }
            }
        }

        [XmlIgnore]
        public float? Health
        {
            get
            {
                return this.Character.Health;
            }

            set
            {
                if (value != this.Character.Health)
                {
                    this.Character.Health = value;
                    this.RaisePropertyChanged(() => Health);
                }
            }
        }

        [XmlIgnore]
        public List<string> CharacterModels
        {
            get { return this._characterModels; }
            set
            {
                if (value != this._characterModels)
                {
                    this._characterModels = value;
                    this.RaisePropertyChanged(() => CharacterModels);
                }
            }
        }

        [XmlIgnore]
        public bool IsPlayer
        {
            get
            {
                return this._isPlayer;
            }

            set
            {
                if (value != this._isPlayer)
                {
                    this._isPlayer = value;
                    this.RaisePropertyChanged(() => IsPlayer);
                }
            }
        }

        [XmlIgnore]
        public double LinearVelocity
        {
            get
            {
                return this.Character.LinearVelocity.ToVector3().LinearVector();
            }
        }

        [XmlIgnore]
        public bool IsPilot
        {
            get
            {
                return this._isPilot;
            }

            set
            {
                if (value != this._isPilot)
                {
                    this._isPilot = value;
                    this.RaisePropertyChanged(() => IsPilot);
                }
            }
        }

        [XmlIgnore]
        public InventoryEditorModel Inventory
        {
            get
            {
                return this._inventory;
            }

            set
            {
                if (value != this._inventory)
                {
                    this._inventory = value;
                    this.RaisePropertyChanged(() => Inventory);
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            this.SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_Character>(this.Character);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            this.EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_Character>(this.SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            this.ClassType = ClassType.Character;
            this.Description = "Player";
            this.DisplayName = this.Character.DisplayName;
            this.Mass = SpaceEngineersConsts.PlayerMass;

            if (this.Inventory == null)
            {
                this.Inventory = new InventoryEditorModel(this.Character.Inventory, Settings, 0.4f * 1000 * Settings.InventorySizeMultiplier, this.Character);
                this.Mass += this.Inventory.TotalMass;
            }
        }

        public void ResetVelocity()
        {
            this.Character.LinearVelocity = new VRageMath.Vector3(0, 0, 0);
            this.RaisePropertyChanged(() => LinearVelocity);
        }

        public void ReverseVelocity()
        {
            this.Character.LinearVelocity = new VRageMath.Vector3(this.Character.LinearVelocity.X * -1, this.Character.LinearVelocity.Y * -1, this.Character.LinearVelocity.Z * -1);
            this.RaisePropertyChanged(() => LinearVelocity);
        }

        #endregion
    }
}
