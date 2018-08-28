namespace SEToolbox.Models
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Sandbox.Definitions;
    using SEToolbox.Interop;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using Res = SEToolbox.Properties.Resources;

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
                    OnPropertyChanged(nameof(Item));
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
                    OnPropertyChanged(nameof(Units));
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_FloatingObject>(FloatingObject);
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_FloatingObject>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.FloatingObject;

            var cd = (MyPhysicalItemDefinition)MyDefinitionManager.Static.GetDefinition(FloatingObject.Item.PhysicalContent.TypeId, FloatingObject.Item.PhysicalContent.SubtypeName);
            var friendlyName = cd != null ? SpaceEngineersApi.GetResourceName(cd.DisplayNameText) : FloatingObject.Item.PhysicalContent.SubtypeName;

            if (FloatingObject.Item.PhysicalContent is MyObjectBuilder_Ore)
            {
                DisplayName = friendlyName;
                Units = (decimal)FloatingObject.Item.Amount;
                Volume = cd == null ? 0 : cd.Volume * SpaceEngineersConsts.VolumeMultiplyer * (double)FloatingObject.Item.Amount;
                Mass = cd == null ? 0 : cd.Mass * (double)FloatingObject.Item.Amount;
                Description = string.Format("{0:#,##0.00} {1}", Mass, Res.GlobalSIMassKilogram);
            }
            else if (FloatingObject.Item.PhysicalContent is MyObjectBuilder_Ingot)
            {
                DisplayName = friendlyName;
                Units = (decimal)FloatingObject.Item.Amount;
                Volume = cd == null ? 0 : cd.Volume * SpaceEngineersConsts.VolumeMultiplyer * (double)FloatingObject.Item.Amount;
                Mass = cd == null ? 0 : cd.Mass * (double)FloatingObject.Item.Amount;
                Description = string.Format("{0:#,##0.00} {1}", Mass, Res.GlobalSIMassKilogram);
            }
            else
            {
                DisplayName = friendlyName;
                Description = string.Format("x {0}", FloatingObject.Item.Amount);
                Units = (decimal)FloatingObject.Item.Amount;
                Volume = cd == null ? 0 : cd.Volume * SpaceEngineersConsts.VolumeMultiplyer * (double)FloatingObject.Item.Amount;
                Mass = cd == null ? 0 : cd.Mass * (double)FloatingObject.Item.Amount;
            }
        }

        #endregion
    }
}
