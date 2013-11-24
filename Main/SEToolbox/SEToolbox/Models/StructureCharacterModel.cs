namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;

    public class StructureCharacterModel : StructureBaseModel
    {
        #region ctor

        public StructureCharacterModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region Properties

        public MyObjectBuilder_Character Character
        {
            get
            {
                return this.EntityBase as MyObjectBuilder_Character;
            }
        }

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
                }
            }
        }

        #endregion

        #region methods

        public override void UpdateFromEntityBase()
        {
            this.ClassType = ClassType.Character;
        }

        #endregion
    }
}
