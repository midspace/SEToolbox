namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
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
                return this.EntityBase as MyObjectBuilder_Meteor;
            }
        }

        [XmlIgnore]
        public MyObjectBuilder_InventoryItem Item
        {
            get
            {
                return this.Meteor.Item;
            }

            set
            {
                if (value != this.Meteor.Item)
                {
                    this.Meteor.Item = value;
                    this.RaisePropertyChanged(() => Item);
                }
            }
        }

        [XmlIgnore]
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

        /// This is not to be taken as an accurate representation.
        [XmlIgnore]
        public double AngularSpeed
        {
            get
            {
                return this.Meteor.AngularVelocity.LinearVector();
            }
        }

        [XmlIgnore]
        public double LinearVelocity
        {
            get
            {
                return this.Meteor.LinearVelocity.LinearVector();
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            this.SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_Meteor>(this.Meteor);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            this.EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_Meteor>(this.SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            this.ClassType = ClassType.Meteor;
            var compMass = SpaceEngineersApi.GetItemMass(this.Meteor.Item.Content.TypeId, this.Meteor.Item.Content.SubtypeName);
            var compVolume = SpaceEngineersApi.GetItemVolume(this.Meteor.Item.Content.TypeId, this.Meteor.Item.Content.SubtypeName);

            if (this.Meteor.Item.Content is MyObjectBuilder_Ore)
            {
                this.DisplayName = string.Format("{0} Ore", this.Meteor.Item.Content.SubtypeName);
                this.Volume = compVolume * this.Meteor.Item.Amount;
                this.Mass = compMass * this.Meteor.Item.Amount;
                this.Description = string.Format("{0:#,##0.00} Kg", this.Mass);
            }
            else
            {
                this.DisplayName = this.Meteor.Item.Content.SubtypeName;
                this.Description = string.Format("x {0}", this.Meteor.Item.Amount);
                this.Volume = compVolume * this.Meteor.Item.Amount;
                this.Mass = compMass * this.Meteor.Item.Amount;
            }
        }

        public void ResetVelocity()
        {
            this.Meteor.LinearVelocity = new VRageMath.Vector3(0, 0, 0);
            this.Meteor.AngularVelocity = new VRageMath.Vector3(0, 0, 0);
            this.RaisePropertyChanged(() => LinearVelocity);
        }

        public void ReverseVelocity()
        {
            this.Meteor.LinearVelocity = new VRageMath.Vector3(this.Meteor.LinearVelocity.X * -1, this.Meteor.LinearVelocity.Y * -1, this.Meteor.LinearVelocity.Z * -1);
            this.Meteor.AngularVelocity = new VRageMath.Vector3(this.Meteor.AngularVelocity.X * -1, this.Meteor.AngularVelocity.Y * -1, this.Meteor.AngularVelocity.Z * -1);
            this.RaisePropertyChanged(() => LinearVelocity);
        }

        public void MaxVelocityAtPlayer(Vector3 playerPosition)
        {
            var v = playerPosition - this.Meteor.PositionAndOrientation.Value.Position;
            v.Normalize();
            v = Vector3.Multiply(v, SpaceEngineersConsts.MaxMeteorVelocity);

            this.Meteor.LinearVelocity = v;
            this.Meteor.AngularVelocity = new VRageMath.Vector3(0, 0, 0);
            this.RaisePropertyChanged(() => LinearVelocity);
        }

        #endregion
    }
}
