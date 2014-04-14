namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable]
    public class StructureUnknownModel : StructureBaseModel
    {
        #region ctor

        public StructureUnknownModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region Properties

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            this.SerializedEntity = SpaceEngineersAPI.Serialize<MyObjectBuilder_EntityBase>(this.EntityBase);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            this.EntityBase = SpaceEngineersAPI.Deserialize<MyObjectBuilder_EntityBase>(this.SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            this.ClassType = ClassType.Unknown;
            this.DisplayName = this.EntityBase.TypeId.ToString();
        }

        #endregion
    }
}
