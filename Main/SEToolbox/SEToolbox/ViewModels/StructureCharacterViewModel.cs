namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Models;

    public class StructureCharacterViewModel : StructureBaseViewModel
    {
        #region ctor

        public StructureCharacterViewModel(BaseViewModel parentViewModel, StructureCharacterModel dataModel)
            : base(parentViewModel, dataModel)
        {
        }

        #endregion

        #region Properties

        protected new StructureCharacterModel DataModel
        {
            get
            {
                return base.DataModel as StructureCharacterModel;
            }
        }

        public MyCharacterModelEnum CharacterModel
        {
            get
            {
                return this.DataModel.CharacterModel;
            }

            set
            {
                if (value != this.DataModel.CharacterModel)
                {
                    this.DataModel.CharacterModel = value;
                    this.RaisePropertyChanged(() => CharacterModel);
                }
            }
        }

        #endregion

        #region methods

        #endregion
    }
}
