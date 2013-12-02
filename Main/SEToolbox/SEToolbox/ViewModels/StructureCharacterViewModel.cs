namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    public class StructureCharacterViewModel : StructureBaseViewModel<StructureCharacterModel>
    {
        #region ctor

        public StructureCharacterViewModel(BaseViewModel parentViewModel, StructureCharacterModel dataModel)
            : base(parentViewModel, dataModel)
        {
            this.DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
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

        public bool IsPlayer
        {
            get
            {
                return this.DataModel.IsPlayer;
            }

            set
            {
                this.DataModel.IsPlayer = value;
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
                this.DataModel.CharacterModel = value;
                ((ExplorerViewModel)this.OwnerViewModel).IsModified = true;
            }
        }

        #endregion

        #region methods

        #endregion
    }
}
