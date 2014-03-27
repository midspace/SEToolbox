namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable]
    public class StructureFloatingObjectModel : StructureBaseModel
    {
        #region Fields

        private double? _volume;
        private double? _mass;
        private double? _units;

        #endregion

        #region ctor

        public StructureFloatingObjectModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
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
        public double PositionX
        {
            get
            {
                return this.FloatingObject.PositionAndOrientation.Value.Position.X.ToDouble();
            }

            set
            {
                if ((float)value != this.FloatingObject.PositionAndOrientation.Value.Position.X)
                {
                    var pos = this.FloatingObject.PositionAndOrientation.Value;
                    pos.Position.X = (float)value;
                    this.FloatingObject.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionX);
                }
            }
        }

        [XmlIgnore]
        public double PositionY
        {
            get
            {
                return this.FloatingObject.PositionAndOrientation.Value.Position.Y.ToDouble();
            }

            set
            {
                if ((float)value != this.FloatingObject.PositionAndOrientation.Value.Position.Y)
                {
                    var pos = this.FloatingObject.PositionAndOrientation.Value;
                    pos.Position.Y = (float)value;
                    this.FloatingObject.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionY);
                }
            }
        }

        [XmlIgnore]
        public double PositionZ
        {
            get
            {
                return this.FloatingObject.PositionAndOrientation.Value.Position.Z.ToDouble();
            }

            set
            {
                if ((float)value != this.FloatingObject.PositionAndOrientation.Value.Position.Z)
                {
                    var pos = this.FloatingObject.PositionAndOrientation.Value;
                    pos.Position.Z = (float)value;
                    this.FloatingObject.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionZ);
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
            this.SerializedEntity = SpaceEngineersAPI.Serialize<MyObjectBuilder_FloatingObject>(this.FloatingObject);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            this.EntityBase = SpaceEngineersAPI.Deserialize<MyObjectBuilder_FloatingObject>(this.SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            this.ClassType = ClassType.FloatingObject;
            var compMass = SpaceEngineersAPI.GetItemMass(this.FloatingObject.Item.Content.GetType(), this.FloatingObject.Item.Content.SubtypeName);
            var compVolume = SpaceEngineersAPI.GetItemVolume(this.FloatingObject.Item.Content.GetType(), this.FloatingObject.Item.Content.SubtypeName);

            if (this.FloatingObject.Item.Content is MyObjectBuilder_Ore)
            {
                this.DisplayName = string.Format("{0} Ore", this.FloatingObject.Item.Content.SubtypeName);
                this.Volume = compVolume * this.FloatingObject.Item.Amount;
                this.Mass = compMass * this.FloatingObject.Item.Amount;
                this.Description = string.Format("{0:#,##0.00} Kg", this.Mass);
            }
            else if (this.FloatingObject.Item.Content is MyObjectBuilder_Ingot)
            {
                this.DisplayName = string.Format("{0} Ingot", this.FloatingObject.Item.Content.SubtypeName);
                this.Volume = compVolume * this.FloatingObject.Item.Amount;
                this.Mass = compMass * this.FloatingObject.Item.Amount;
                this.Description = string.Format("{0:#,##0.00} Kg", this.Mass);
            }
            else if (this.FloatingObject.Item.Content is MyObjectBuilder_EntityBase)
            {
                var name = this.FloatingObject.Item.Content.GetType().Name;
                name = name.Split('_')[1];
                this.DisplayName = name;
                this.Description = string.Format("x {0}", this.FloatingObject.Item.Amount);
                this.Units = this.FloatingObject.Item.Amount;
                this.Volume = compVolume * this.FloatingObject.Item.Amount;
                this.Mass = compMass * this.FloatingObject.Item.Amount;
            }
            else
            {
                this.DisplayName = this.FloatingObject.Item.Content.SubtypeName;
                this.Description = string.Format("x {0}", this.FloatingObject.Item.Amount);
                this.Units = this.FloatingObject.Item.Amount;
                this.Volume = compVolume * this.FloatingObject.Item.Amount;
                this.Mass = compMass * this.FloatingObject.Item.Amount;
            }
        }

        #endregion
    }
}
