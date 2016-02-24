namespace SEToolbox.Models
{
    using System;
    using System.Runtime.Serialization;

    using SEToolbox.Interop;
    using VRage.ObjectBuilders;

    [Serializable]
    public class StructureUnknownModel : StructureBaseModel
    {
        #region ctor

        public StructureUnknownModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region methods

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_EntityBase>(EntityBase);
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_EntityBase>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Unknown;
            DisplayName = EntityBase.TypeId.ToString();
        }

        #endregion
    }
}
