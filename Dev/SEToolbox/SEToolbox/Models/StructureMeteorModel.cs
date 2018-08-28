namespace SEToolbox.Models
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using SEToolbox.Interop;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRageMath;
    using Res = SEToolbox.Properties.Resources;

    [Serializable]
    public class StructureMeteorModel : StructureBaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private double? _volume;

        #endregion

        #region ctor

        public StructureMeteorModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_Meteor Meteor
        {
            get
            {
                return EntityBase as MyObjectBuilder_Meteor;
            }
        }

        [XmlIgnore]
        public MyObjectBuilder_InventoryItem Item
        {
            get
            {
                return Meteor.Item;
            }

            set
            {
                if (value != Meteor.Item)
                {
                    Meteor.Item = value;
                    OnPropertyChanged(nameof(Item));
                }
            }
        }

        [XmlIgnore]
        public float Integrity
        {
            get
            {
                return Meteor.Integrity;
            }

            set
            {
                if (value != Meteor.Integrity)
                {
                    Meteor.Integrity = value;
                    OnPropertyChanged(nameof(Integrity));
                }
            }
        }

        [XmlIgnore]
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
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        /// This is not to be taken as an accurate representation.
        [XmlIgnore]
        public double AngularVelocity
        {
            get
            {
                return Meteor.AngularVelocity.LinearVector();
            }
        }

        [XmlIgnore]
        public override double LinearVelocity
        {
            get
            {
                return Meteor.LinearVelocity.LinearVector();
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_Meteor>(Meteor);
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_Meteor>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Meteor;
            float compMass = 1;
            float compVolume = 1;
            double amount = 1;

            if (Meteor.Item != null && Meteor.Item.PhysicalContent is MyObjectBuilder_Ore)
            {
                var def = (MyPhysicalItemDefinition)MyDefinitionManager.Static.GetDefinition(Meteor.Item.PhysicalContent.TypeId, Meteor.Item.PhysicalContent.SubtypeName);

                compMass = def.Mass;
                compVolume = def.Volume;
                amount = (double)Meteor.Item.Amount;

                DisplayName = string.Format("{0} {1}", Meteor.Item.PhysicalContent.SubtypeName, Res.CtlMeteorOre);
                Volume = compVolume * amount;
                Mass = compMass * amount;
                Description = string.Format("{0:#,##0.00} {1}", Mass, Res.GlobalSIMassKilogram);
            }
            else
            {
                DisplayName = Res.CtlMeteorDisplayName;
                Description = string.Format("x {0}", amount);
                Volume = compVolume * amount;
                Mass = compMass * amount;
            }
        }

        public void ResetVelocity()
        {
            Meteor.LinearVelocity = new Vector3(0, 0, 0);
            Meteor.AngularVelocity = new Vector3(0, 0, 0);
            OnPropertyChanged(nameof(LinearVelocity));
        }

        public void ReverseVelocity()
        {
            Meteor.LinearVelocity = new Vector3(Meteor.LinearVelocity.X * -1, Meteor.LinearVelocity.Y * -1, Meteor.LinearVelocity.Z * -1);
            Meteor.AngularVelocity = new Vector3(Meteor.AngularVelocity.X * -1, Meteor.AngularVelocity.Y * -1, Meteor.AngularVelocity.Z * -1);
            OnPropertyChanged(nameof(LinearVelocity));
        }

        public void MaxVelocityAtPlayer(Vector3D playerPosition)
        {
            var v = playerPosition - Meteor.PositionAndOrientation.Value.Position;
            v.Normalize();
            v = Vector3.Multiply(v, SpaceEngineersConsts.MaxMeteorVelocity);

            Meteor.LinearVelocity = v;
            Meteor.AngularVelocity = new Vector3(0, 0, 0);
            OnPropertyChanged(nameof(LinearVelocity));
        }

        #endregion
    }
}
