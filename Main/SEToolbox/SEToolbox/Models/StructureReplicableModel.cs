namespace SEToolbox.Models
{
    using System;
    using System.Runtime.Serialization;

    using SEToolbox.Interop;
    using VRage.ObjectBuilders;

    [Serializable]
    public class StructureReplicableModel : StructureBaseModel
    {
        #region ctor

        public StructureReplicableModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_EntityBase>(EntityBase);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_EntityBase>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Replicable;
            DisplayName = EntityBase.EntityDefinitionId.HasValue ? EntityBase.EntityDefinitionId.Value.SubtypeName : null;
        }

        #endregion
    }
}
