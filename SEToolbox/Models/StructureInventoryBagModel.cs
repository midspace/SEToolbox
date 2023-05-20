namespace SEToolbox.Models
{
    using System;
    using System.Runtime.Serialization;

    using SEToolbox.Interop;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;

    [Serializable]
    public class StructureInventoryBagModel : StructureBaseModel
    {
        #region ctor

        public StructureInventoryBagModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region methods

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_InventoryBagEntity>(EntityBase);
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_InventoryBagEntity>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.InventoryBag;
            DisplayName = EntityBase.EntityDefinitionId.HasValue ? EntityBase.EntityDefinitionId.Value.SubtypeName : null;
        }

        #endregion
    }
}
