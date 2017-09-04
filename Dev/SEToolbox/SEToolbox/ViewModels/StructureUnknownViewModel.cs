namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;

    public class StructureUnknownViewModel : StructureBaseViewModel<StructureUnknownModel>
    {
        #region ctor

        public StructureUnknownViewModel(BaseViewModel parentViewModel, StructureUnknownModel dataModel)
            : base(parentViewModel, dataModel)
        {
            // Will bubble property change events from the Model to the ViewModel.
            DataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties

        protected new StructureUnknownModel DataModel
        {
            get { return base.DataModel as StructureUnknownModel; }
        }

        #endregion
    }
}
