namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Interop;
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable]
    public class StructureFloatingObjectModel : StructureBaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private double? _volume;

        [NonSerialized]
        private double? _mass;

        [NonSerialized]
        private double? _units;

        #endregion

        #region ctor

        public StructureFloatingObjectModel(MyObjectBuilder_EntityBase entityBase, MySessionSettings settings)
            : base(entityBase, settings)
        {
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_FloatingObject FloatingObject
        {
            get
            {
                return this.EntityBase as MyObjectBuilder_FloatingObject;
            }
        }

        [XmlIgnore]
        public MyObjectBuilder_InventoryItem Item
        {
            get
            {
                return this.FloatingObject.Item;
            }

            set
            {
                if (value != this.FloatingObject.Item)
                {
                    this.FloatingObject.Item = value;
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

        [XmlIgnore]
        public double? Mass
        {
            get
            {
                return this._mass;
            }

            set
            {
                if (value != this._mass)
                {
                    this._mass = value;
                    this.RaisePropertyChanged(() => Mass);
                }
            }
        }

        [XmlIgnore]
        public double? Units
        {
            get
            {
                return this._units;
            }

            set
            {
                if (value != this._units)
                {
                    this._units = value;
                    this.RaisePropertyChanged(() => Units);
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            this.SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_FloatingObject>(this.FloatingObject);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            this.EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_FloatingObject>(this.SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            this.ClassType = ClassType.FloatingObject;

            var cd = SpaceEngineersApi.GetDefinition(this.FloatingObject.Item.Content.TypeId, this.FloatingObject.Item.Content.SubtypeName) as MyObjectBuilder_PhysicalItemDefinition;
            var friendlyName = SpaceEngineersApi.GetResourceName(cd.DisplayName);

            if (this.FloatingObject.Item.Content is MyObjectBuilder_Ore)
            {
                this.DisplayName = friendlyName;
                this.Volume = cd.Volume.Value * this.FloatingObject.Item.Amount;
                this.Mass = cd.Mass * this.FloatingObject.Item.Amount;
                this.Description = string.Format("{0:#,##0.00} Kg", this.Mass);
            }
            else if (this.FloatingObject.Item.Content is MyObjectBuilder_Ingot)
            {
                this.DisplayName = friendlyName;
                this.Volume = cd.Volume.Value * this.FloatingObject.Item.Amount;
                this.Mass = cd.Mass * this.FloatingObject.Item.Amount;
                this.Description = string.Format("{0:#,##0.00} Kg", this.Mass);
            }
            else if (this.FloatingObject.Item.Content is MyObjectBuilder_EntityBase)
            {
                this.DisplayName = friendlyName;
                this.Description = string.Format("x {0}", this.FloatingObject.Item.Amount);
                this.Units = this.FloatingObject.Item.Amount;
                this.Volume = cd.Volume.Value * this.FloatingObject.Item.Amount;
                this.Mass = cd.Mass * this.FloatingObject.Item.Amount;
            }
            else
            {
                this.DisplayName = friendlyName;
                this.Description = string.Format("x {0}", this.FloatingObject.Item.Amount);
                this.Units = this.FloatingObject.Item.Amount;
                this.Volume = cd.Volume.Value * this.FloatingObject.Item.Amount;
                this.Mass = cd.Mass * this.FloatingObject.Item.Amount;
            }
        }

        #endregion
    }
}
