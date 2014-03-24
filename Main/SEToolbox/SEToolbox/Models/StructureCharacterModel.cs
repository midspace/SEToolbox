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
        private bool _isPlayer;

        [NonSerialized]
        private bool _isPilot;

        [NonSerialized]
        private List<string> _characterModels;

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
                    this.UpdateGeneralFromEntityBase();
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
            get { return this._characterModels; }
            set
            {
                if (value != this._characterModels)
                {
                    this._characterModels = value;
                    this.RaisePropertyChanged(() => CharacterModels);
                } 
            }
        }

        [XmlIgnore]
        public bool IsPlayer
        {
            get
            {
                return this._isPlayer;
            }

            set
            {
                if (value != this._isPlayer)
                {
                    this._isPlayer = value;
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

        [XmlIgnore]
        public bool IsPilot
        {
            get
            {
                return this._isPilot;
            }

            set
            {
                if (value != this._isPilot)
                {
                    this._isPilot = value;
                    this.RaisePropertyChanged(() => IsPilot);
                }
            }
        }

        [XmlIgnore]
        public double PositionX
        {
            get
            {
                return this.Character.PositionAndOrientation.Value.Position.X.ToDouble();
            }

            set
            {
                if ((float)value != this.Character.PositionAndOrientation.Value.Position.X)
                {
                    var pos = this.Character.PositionAndOrientation.Value;
                    pos.Position.X = (float)value;
                    this.Character.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionX);
                    //this.RaisePropertyChanged(() => Position);
                }
            }
        }

        [XmlIgnore]
        public double PositionY
        {
            get
            {
                return this.Character.PositionAndOrientation.Value.Position.Y.ToDouble();
            }

            set
            {
                if ((float)value != this.Character.PositionAndOrientation.Value.Position.Y)
                {
                    var pos = this.Character.PositionAndOrientation.Value;
                    pos.Position.Y = (float)value;
                    this.Character.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionY);
                    //this.RaisePropertyChanged(() => Position);
                }
            }
        }

        [XmlIgnore]
        public double PositionZ
        {
            get
            {
                return this.Character.PositionAndOrientation.Value.Position.Z.ToDouble();
            }

            set
            {
                if ((float)value != this.Character.PositionAndOrientation.Value.Position.Z)
                {
                    var pos = this.Character.PositionAndOrientation.Value;
                    pos.Position.Z = (float)value;
                    this.Character.PositionAndOrientation = pos;
                    this.RaisePropertyChanged(() => PositionZ);
                    //this.RaisePropertyChanged(() => Position);
                }
            }
        }

        // Very messy.
        //[XmlIgnore]
        //public BindablePoint3DModel Position
        //{
        //    get
        //    {
        //        var m = new BindablePoint3DModel(this.Character.PositionAndOrientation.Value.Position);
        //        m.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
        //        {
        //            this.Position = ((BindablePoint3DModel) sender);
        //        };
        //        return m;
        //    }

        //    set
        //    {
        //        var pos = this.Character.PositionAndOrientation.Value;
        //        pos.Position = value.ToVector3();
        //        this.Character.PositionAndOrientation = pos;
        //        this.RaisePropertyChanged(() => Position);
        //    }
        //}

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

        public override void UpdateGeneralFromEntityBase()
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
