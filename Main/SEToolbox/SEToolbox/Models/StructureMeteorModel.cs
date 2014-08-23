namespace SEToolbox.Models
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using VRageMath;

    [Serializable]
    public class StructureMeteorModel : StructureBaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private double? _volume;

        #endregion

        #region ctor

        public StructureMeteorModel(MyObjectBuilder_EntityBase entityBase, MySessionSettings settings)
            : base(entityBase, settings)
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
                    RaisePropertyChanged(() => Item);
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
                    RaisePropertyChanged(() => Volume);
                }
            }
        }

        /// This is not to be taken as an accurate representation.
        [XmlIgnore]
        public double AngularSpeed
        {
            get
            {
                return Meteor.AngularVelocity.LinearVector();
            }
        }

        [XmlIgnore]
        public double LinearVelocity
        {
            get
            {
                return Meteor.LinearVelocity.LinearVector();
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_Meteor>(Meteor);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_Meteor>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Meteor;
            var compMass = SpaceEngineersApi.GetItemMass(Meteor.Item.Content.TypeId, Meteor.Item.Content.SubtypeName);
            var compVolume = SpaceEngineersApi.GetItemVolume(Meteor.Item.Content.TypeId, Meteor.Item.Content.SubtypeName);

            if (Meteor.Item.Content is MyObjectBuilder_Ore)
            {
                DisplayName = string.Format("{0} Ore", Meteor.Item.Content.SubtypeName);
                Volume = compVolume * (double)Meteor.Item.Amount;
                Mass = compMass * (double)Meteor.Item.Amount;
                Description = string.Format("{0:#,##0.00} Kg", Mass);
            }
            else
            {
                DisplayName = Meteor.Item.Content.SubtypeName;
                Description = string.Format("x {0}", Meteor.Item.Amount);
                Volume = compVolume * (double)Meteor.Item.Amount;
                Mass = compMass * (double)Meteor.Item.Amount;
            }
        }

        public void ResetVelocity()
        {
            Meteor.LinearVelocity = new Vector3(0, 0, 0);
            Meteor.AngularVelocity = new Vector3(0, 0, 0);
            RaisePropertyChanged(() => LinearVelocity);
        }

        public void ReverseVelocity()
        {
            Meteor.LinearVelocity = new Vector3(Meteor.LinearVelocity.X * -1, Meteor.LinearVelocity.Y * -1, Meteor.LinearVelocity.Z * -1);
            Meteor.AngularVelocity = new Vector3(Meteor.AngularVelocity.X * -1, Meteor.AngularVelocity.Y * -1, Meteor.AngularVelocity.Z * -1);
            RaisePropertyChanged(() => LinearVelocity);
        }

        public void MaxVelocityAtPlayer(Vector3 playerPosition)
        {
            var v = playerPosition - Meteor.PositionAndOrientation.Value.Position;
            v.Normalize();
            v = Vector3.Multiply(v, SpaceEngineersConsts.MaxMeteorVelocity);

            Meteor.LinearVelocity = v;
            Meteor.AngularVelocity = new Vector3(0, 0, 0);
            RaisePropertyChanged(() => LinearVelocity);
        }

        #endregion
    }
}
