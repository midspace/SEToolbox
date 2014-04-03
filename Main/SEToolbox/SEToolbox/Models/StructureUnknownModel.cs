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

        [XmlIgnore]
        public double PositionX
        {
            get
            {
                return this.EntityBase.PositionAndOrientation.Value.Position.X.ToDouble();
            }

            set
            {
                if ((float)value != this.EntityBase.PositionAndOrientation.Value.Position.X)
                {
                    var pos = this.EntityBase.PositionAndOrientation.Value;
                    pos.Position.X = (float)value;
                    this.EntityBase.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionX);
                }
            }
        }

        [XmlIgnore]
        public double PositionY
        {
            get
            {
                return this.EntityBase.PositionAndOrientation.Value.Position.Y.ToDouble();
            }

            set
            {
                if ((float)value != this.EntityBase.PositionAndOrientation.Value.Position.Y)
                {
                    var pos = this.EntityBase.PositionAndOrientation.Value;
                    pos.Position.Y = (float)value;
                    this.EntityBase.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionY);
                }
            }
        }

        [XmlIgnore]
        public double PositionZ
        {
            get
            {
                return this.EntityBase.PositionAndOrientation.Value.Position.Z.ToDouble();
            }

            set
            {
                if ((float)value != this.EntityBase.PositionAndOrientation.Value.Position.Z)
                {
                    var pos = this.EntityBase.PositionAndOrientation.Value;
                    pos.Position.Z = (float)value;
                    this.EntityBase.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionZ);
                }
            }
        }

        #endregion

        #region methods

        //[OnSerializing]
        //internal void OnSerializingMethod(StreamingContext context)
        //{
        //    this.SerializedEntity = SpaceEngineersAPI.Serialize<MyObjectBuilder_FloatingObject>(this.FloatingObject);
        //}

        //[OnDeserialized]
        //internal void OnDeserializedMethod(StreamingContext context)
        //{
        //    this.EntityBase = SpaceEngineersAPI.Deserialize<MyObjectBuilder_FloatingObject>(this.SerializedEntity);
        //}

        public override void UpdateGeneralFromEntityBase()
        {
            this.ClassType = ClassType.Unknown;
            this.DisplayName = SpaceEngineersAPI.GetObjectBuilderName(this.EntityBase.GetType());
        }

        #endregion
    }
}
