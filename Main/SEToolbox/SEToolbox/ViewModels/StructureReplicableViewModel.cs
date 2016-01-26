namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Models;
    using VRage.ObjectBuilders;

    public class StructureReplicableViewModel : StructureBaseViewModel<StructureReplicableModel>
    {
        #region ctor

        public StructureReplicableViewModel(BaseViewModel parentViewModel, StructureReplicableModel dataModel)
            : base(parentViewModel, dataModel)
        {
            // Will bubble property change events from the Model to the ViewModel.
            DataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties

        protected new StructureReplicableModel DataModel
        {
            get { return base.DataModel as StructureReplicableModel; }
        }

        #endregion
    }
}
