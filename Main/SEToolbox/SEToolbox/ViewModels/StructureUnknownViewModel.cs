namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using System.ComponentModel;

    public class StructureUnknownViewModel : StructureBaseViewModel<StructureUnknownModel>
    {
        #region ctor

        public StructureUnknownViewModel(BaseViewModel parentViewModel, StructureUnknownModel dataModel)
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

        protected new StructureUnknownModel DataModel
        {
            get
            {
                return base.DataModel as StructureUnknownModel;
            }
        }

        #endregion
    }
}
