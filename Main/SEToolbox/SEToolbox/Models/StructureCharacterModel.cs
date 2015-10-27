namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using VRage.ObjectBuilders;
    using VRage;

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

        public StructureCharacterModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
            CharacterModels = new List<string>(Enum.GetNames(typeof(MyCharacterModelEnum)));
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_Character Character
        {
            get
            {
                return EntityBase as MyObjectBuilder_Character;
            }
        }

        [XmlIgnore]
        public SerializableVector3 Color
        {
            get
            {
                return Character.ColorMaskHSV;
            }

            set
            {
                if (!EqualityComparer<SerializableVector3>.Default.Equals(value, Character.ColorMaskHSV))
                {
                    Character.ColorMaskHSV = value;
                    RaisePropertyChanged(() => Color);
                    UpdateGeneralFromEntityBase();
                }
            }
        }

        [XmlIgnore]
        public bool Light
        {
            get
            {
                return Character.LightEnabled;
            }

            set
            {
                if (value != Character.LightEnabled)
                {
                    Character.LightEnabled = value;
                    RaisePropertyChanged(() => Light);
                }
            }
        }

        [XmlIgnore]
        public bool JetPack
        {
            get
            {
                return Character.JetpackEnabled;
            }

            set
            {
                if (value != Character.JetpackEnabled)
                {
                    Character.JetpackEnabled = value;
                    RaisePropertyChanged(() => JetPack);
                }
            }
        }

        [XmlIgnore]
        public bool Dampeners
        {
            get
            {
                return Character.DampenersEnabled;
            }

            set
            {
                if (value != Character.DampenersEnabled)
                {
                    Character.DampenersEnabled = value;
                    RaisePropertyChanged(() => Dampeners);
                }
            }
        }

        [XmlIgnore]
        public float BatteryCapacity
        {
            get
            {
                return Character.Battery.CurrentCapacity;
            }

            set
            {
                if (value != Character.Battery.CurrentCapacity)
                {
                    Character.Battery.CurrentCapacity = value;
                    RaisePropertyChanged(() => BatteryCapacity);
                }
            }
        }

        [XmlIgnore]
        public float? Health
        {
            get
            {
                return Character.Health;
            }

            set
            {
                if (value != Character.Health)
                {
                    Character.Health = value;
                    RaisePropertyChanged(() => Health);
                }
            }
        }

        [XmlIgnore]
        public float OxygenLevel
        {
            get
            {
                return Character.OxygenLevel;
            }

            set
            {
                if (value != Character.OxygenLevel)
                {
                    Character.OxygenLevel = value;
                    RaisePropertyChanged(() => OxygenLevel);
                }
            }
        }

        [XmlIgnore]
        public List<string> CharacterModels
        {
            get { return _characterModels; }
            set
            {
                if (value != _characterModels)
                {
                    _characterModels = value;
                    RaisePropertyChanged(() => CharacterModels);
                }
            }
        }

        [XmlIgnore]
        public bool IsPlayer
        {
            get
            {
                return _isPlayer;
            }

            set
            {
                if (value != _isPlayer)
                {
                    _isPlayer = value;
                    RaisePropertyChanged(() => IsPlayer);
                }
            }
        }

        [XmlIgnore]
        public override double LinearVelocity
        {
            get
            {
                return Character.LinearVelocity.ToVector3().LinearVector();
            }
        }

        [XmlIgnore]
        public bool IsPilot
        {
            get
            {
                return _isPilot;
            }

            set
            {
                if (value != _isPilot)
                {
                    _isPilot = value;
                    RaisePropertyChanged(() => IsPilot);
                }
            }
        }

        [XmlIgnore]
        public InventoryEditorModel Inventory
        {
            get
            {
                return _inventory;
            }

            set
            {
                if (value != _inventory)
                {
                    _inventory = value;
                    RaisePropertyChanged(() => Inventory);
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_Character>(Character);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_Character>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Character;
            Description = "Player";
            DisplayName = Character.DisplayName;
            Mass = SpaceEngineersConsts.PlayerMass;

            if (Inventory == null)
            {
                var settings = SpaceEngineersCore.WorldResource.Checkpoint.Settings;
                Inventory = new InventoryEditorModel(Character.Inventory, 0.4f * 1000 * settings.InventorySizeMultiplier, Character);
                Mass += Inventory.TotalMass;
            }
        }

        public void ResetVelocity()
        {
            Character.LinearVelocity = new VRageMath.Vector3(0, 0, 0);
            RaisePropertyChanged(() => LinearVelocity);
        }

        public void ReverseVelocity()
        {
            Character.LinearVelocity = new VRageMath.Vector3(Character.LinearVelocity.X * -1, Character.LinearVelocity.Y * -1, Character.LinearVelocity.Z * -1);
            RaisePropertyChanged(() => LinearVelocity);
        }

        #endregion
    }
}
