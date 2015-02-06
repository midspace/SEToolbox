namespace SEToolbox.Models
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Interop;

    [Serializable]
    public class StructureFloatingObjectModel : StructureBaseModel
    {
        #region Fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private double? _volume;

        [NonSerialized]
        private decimal? _units;

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
                return EntityBase as MyObjectBuilder_FloatingObject;
            }
        }

        [XmlIgnore]
        public MyObjectBuilder_InventoryItem Item
        {
            get
            {
                return FloatingObject.Item;
            }

            set
            {
                if (value != FloatingObject.Item)
                {
                    FloatingObject.Item = value;
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

        [XmlIgnore]
        public decimal? Units
        {
            get
            {
                return _units;
            }

            set
            {
                if (value != _units)
                {
                    _units = value;
                    RaisePropertyChanged(() => Units);
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_FloatingObject>(FloatingObject);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_FloatingObject>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.FloatingObject;

            var cd = (MyObjectBuilder_PhysicalItemDefinition)SpaceEngineersApi.GetDefinition(FloatingObject.Item.Content.TypeId, FloatingObject.Item.Content.SubtypeName);
            var friendlyName = cd != null ? SpaceEngineersApi.GetResourceName(cd.DisplayName) : FloatingObject.Item.Content.SubtypeName;

            if (FloatingObject.Item.Content is MyObjectBuilder_Ore)
            {
                DisplayName = friendlyName;
                Units = (decimal)FloatingObject.Item.Amount;
                Volume = cd == null ? 0 : cd.Volume.Value * (Double)FloatingObject.Item.Amount;
                Mass = cd == null ? 0 : cd.Mass * (Double)FloatingObject.Item.Amount;
                Description = string.Format("{0:#,##0.00} Kg", Mass);
            }
            else if (FloatingObject.Item.Content is MyObjectBuilder_Ingot)
            {
                DisplayName = friendlyName;
                Units = (decimal)FloatingObject.Item.Amount;
                Volume = cd == null ? 0 : cd.Volume.Value * (Double)FloatingObject.Item.Amount;
                Mass = cd == null ? 0 : cd.Mass * (Double)FloatingObject.Item.Amount;
                Description = string.Format("{0:#,##0.00} Kg", Mass);
            }
            else if (FloatingObject.Item.Content is MyObjectBuilder_EntityBase)
            {
                DisplayName = friendlyName;
                Description = string.Format("x {0}", FloatingObject.Item.Amount);
                Units = (decimal)FloatingObject.Item.Amount;
                Volume = cd == null ? 0 : cd.Volume.Value * (Double)FloatingObject.Item.Amount;
                Mass = cd == null ? 0 : cd.Mass * (Double)FloatingObject.Item.Amount;
            }
            else
            {
                DisplayName = friendlyName;
                Description = string.Format("x {0}", FloatingObject.Item.Amount);
                Units = (decimal)FloatingObject.Item.Amount;
                Volume = cd == null ? 0 : cd.Volume.Value * (Double)FloatingObject.Item.Amount;
                Mass = cd == null ? 0 : cd.Mass * (Double)FloatingObject.Item.Amount;
            }
        }

        #endregion
    }
}
