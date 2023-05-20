namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Sandbox.Definitions;
    using SEToolbox.Interop;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using Res = SEToolbox.Properties.Resources;

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
        private InventoryEditorModel _inventory;

        #endregion

        #region ctor

        public StructureCharacterModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
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
                    OnPropertyChanged(nameof(Color));
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
                    OnPropertyChanged(nameof(Light));
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
                    OnPropertyChanged(nameof(JetPack));
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
                    OnPropertyChanged(nameof(Dampeners));
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
                    OnPropertyChanged(nameof(BatteryCapacity));
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
                    OnPropertyChanged(nameof(Health));
                }
            }
        }

        [XmlIgnore]
        public float OxygenLevel
        {
            get
            {
                if (Character.StoredGases == null)
                    return 0;
                // doesn't matter if Oxygen is not there, as it will still be 0.
                MyObjectBuilder_Character.StoredGas gas = Character.StoredGases.FirstOrDefault(e => e.Id.SubtypeName == "Oxygen");
                return gas.FillLevel;
            }

            set
            {
                if (ReplaceGasValue("Oxygen", value))
                    OnPropertyChanged(nameof(OxygenLevel));
            }
        }

        [XmlIgnore]
        public float HydrogenLevel
        {
            get
            {
                if (Character.StoredGases == null)
                    return 0;
                // doesn't matter if Hydrogen is not there, as it will still be 0.
                MyObjectBuilder_Character.StoredGas gas = Character.StoredGases.FirstOrDefault(e => e.Id.SubtypeName == "Hydrogen");
                return gas.FillLevel;
            }

            set
            {
                if (ReplaceGasValue("Hydrogen", value))
                    OnPropertyChanged(nameof(HydrogenLevel));
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
                    OnPropertyChanged(nameof(IsPlayer));
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
                    OnPropertyChanged(nameof(IsPilot));
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
                    OnPropertyChanged(nameof(Inventory));
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_Character>(Character);
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_Character>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Character;
            string dead = Character.MovementState == MyCharacterMovementEnum.Died ? $" | {Res.ClsCharacterDead}" : "";

            if (string.IsNullOrEmpty(Character.DisplayName))
            {
                Description = Res.ClsCharacterNPC;
                DisplayName = Character.CharacterModel + dead;
                Mass = SpaceEngineersConsts.PlayerMass; // no idea what an npc body weighs.
            }
            else
            {
                Description = Res.ClsCharacterPlayer;
                DisplayName = Character.DisplayName + dead;
                Mass = SpaceEngineersConsts.PlayerMass;
            }

            if (Inventory == null)
            {
                var inventories = Character.ComponentContainer.GetInventory();
                if (inventories.Count > 0)
                {
                    Inventory = inventories[0];
                    Mass += Inventory.TotalMass;
                }
                else
                    Inventory = null;
            }
        }

        public void ResetVelocity()
        {
            Character.LinearVelocity = new VRageMath.Vector3(0, 0, 0);
            OnPropertyChanged(nameof(LinearVelocity));
        }

        public void ReverseVelocity()
        {
            Character.LinearVelocity = new VRageMath.Vector3(Character.LinearVelocity.X * -1, Character.LinearVelocity.Y * -1, Character.LinearVelocity.Z * -1);
            OnPropertyChanged(nameof(LinearVelocity));
        }

        private bool ReplaceGasValue(string gasName, float value)
        {
            if (Character.StoredGases == null)
                Character.StoredGases = new List<MyObjectBuilder_Character.StoredGas>();

            // Find the existing gas value.
            for (int i = 0; i < Character.StoredGases.Count; i++)
            {
                MyObjectBuilder_Character.StoredGas gas = Character.StoredGases[i];
                if (gas.Id.SubtypeName == gasName)
                {
                    if (value != gas.FillLevel)
                    {
                        gas.FillLevel = value;
                        Character.StoredGases[i] = gas;
                        return true;
                    }
                }
            }

            // If it doesn't exist for old save games, add it in.
            MyObjectBuilder_Character.StoredGas newGas = new MyObjectBuilder_Character.StoredGas
            {
                // This could cause an exception if the gas names are ever changed, even in casing.
                Id = MyDefinitionManager.Static.GetGasDefinitions().FirstOrDefault(e => e.Id.SubtypeName == gasName).Id,
                FillLevel = value
            };
            Character.StoredGases.Add(newGas);
            return true;
        }

        #endregion
    }
}
