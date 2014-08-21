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
        public double? Units
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
            var friendlyName = SpaceEngineersApi.GetResourceName(cd.DisplayName);

            if (FloatingObject.Item.Content is MyObjectBuilder_Ore)
            {
                DisplayName = friendlyName;
                Volume = cd.Volume.Value * FloatingObject.Item.Amount;
                Mass = cd.Mass * FloatingObject.Item.Amount;
                Description = string.Format("{0:#,##0.00} Kg", Mass);
            }
            else if (FloatingObject.Item.Content is MyObjectBuilder_Ingot)
            {
                DisplayName = friendlyName;
                Volume = cd.Volume.Value * FloatingObject.Item.Amount;
                Mass = cd.Mass * FloatingObject.Item.Amount;
                Description = string.Format("{0:#,##0.00} Kg", Mass);
            }
            else if (FloatingObject.Item.Content is MyObjectBuilder_EntityBase)
            {
                DisplayName = friendlyName;
                Description = string.Format("x {0}", FloatingObject.Item.Amount);
                Units = FloatingObject.Item.Amount;
                Volume = cd.Volume.Value * FloatingObject.Item.Amount;
                Mass = cd.Mass * FloatingObject.Item.Amount;
            }
            else
            {
                DisplayName = friendlyName;
                Description = string.Format("x {0}", FloatingObject.Item.Amount);
                Units = FloatingObject.Item.Amount;
                Volume = cd.Volume.Value * FloatingObject.Item.Amount;
                Mass = cd.Mass * FloatingObject.Item.Amount;
            }
        }

        #endregion
    }
}
