namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

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
        public bool Light
        {
            get
            {
                return this.Character.LightEnabled;
            }

            set
            {
                if (value != this.Character.LightEnabled)
                {
                    this.Character.LightEnabled = value;
                    this.RaisePropertyChanged(() => Light);
                }
            }
        }

        [XmlIgnore]
        public bool JetPack
        {
            get
            {
                return this.Character.JetpackEnabled;
            }

            set
            {
                if (value != this.Character.JetpackEnabled)
                {
                    this.Character.JetpackEnabled = value;
                    this.RaisePropertyChanged(() => JetPack);
                }
            }
        }

        [XmlIgnore]
        public bool Dampeners
        {
            get
            {
                return this.Character.DampenersEnabled;
            }

            set
            {
                if (value != this.Character.DampenersEnabled)
                {
                    this.Character.DampenersEnabled = value;
                    this.RaisePropertyChanged(() => Dampeners);
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

        [XmlIgnore]
        public double Speed
        {
            get
            {
                return this.Character.LinearVelocity.ToVector3().LinearVector();
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
            this.DisplayName = this.Character.Name;
        }

        public void ResetVelocity()
        {
            this.Character.LinearVelocity = new VRageMath.Vector3(0, 0, 0);
            this.RaisePropertyChanged(() => Speed);
        }

        public void ReverseVelocity()
        {
            this.Character.LinearVelocity = new VRageMath.Vector3(this.Character.LinearVelocity.X * -1, this.Character.LinearVelocity.Y * -1, this.Character.LinearVelocity.Z * -1);
            this.RaisePropertyChanged(() => Speed);
        }

        #endregion
    }
}
