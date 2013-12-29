using System.Collections.ObjectModel;

namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using System.Linq;

    [Serializable]
    public class StructureCharacterModel : StructureBaseModel
    {
        #region Fields

        [NonSerialized]
        private bool isPlayer;

        [NonSerialized]
        private List<string> characterModels;

        #endregion

        #region ctor

        public StructureCharacterModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
            this.CharacterModels = new List<string>(Enum.GetNames(typeof(MyCharacterModelEnum)));
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_Character Character
        {
            get
            {
                return this.EntityBase as MyObjectBuilder_Character;
            }
        }

        [XmlIgnore]
        public MyCharacterModelEnum CharacterModel
        {
            get
            {
                return this.Character.CharacterModel;
            }

            set
            {
                if (value != this.Character.CharacterModel)
                {
                    this.Character.CharacterModel = value;
                    this.RaisePropertyChanged(() => CharacterModel);
                    this.UpdateFromEntityBase();
                }
            }
        }

        [XmlIgnore]
        public List<string> CharacterModels
        {
            get { return this.characterModels; }
            set
            {
                if (value != this.characterModels)
                {
                    this.characterModels = value;
                    this.RaisePropertyChanged(() => CharacterModels);
                } 
            }
        }

        [XmlIgnore]
        public bool IsPlayer
        {
            get
            {
                return this.isPlayer;
            }

            set
            {
                if (value != this.isPlayer)
                {
                    this.isPlayer = value;
                    this.RaisePropertyChanged(() => IsPlayer);
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            this.SerializedEntity = SpaceEngineersAPI.Serialize<MyObjectBuilder_Character>(this.Character);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            this.EntityBase = SpaceEngineersAPI.Deserialize<MyObjectBuilder_Character>(this.SerializedEntity);
        }

        public override void UpdateFromEntityBase()
        {
            this.ClassType = ClassType.Character;
            this.Description = string.Format("{0}", this.CharacterModel);
        }

        #endregion
    }
}
