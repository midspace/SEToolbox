namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using VRage.Game;

    public class StructureFloatingObjectViewModel : StructureBaseViewModel<StructureFloatingObjectModel>
    {
        #region ctor

        public StructureFloatingObjectViewModel(BaseViewModel parentViewModel, StructureFloatingObjectModel dataModel)
            : base(parentViewModel, dataModel)
        {
            // Will bubble property change events from the Model to the ViewModel.
            DataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties

        protected new StructureFloatingObjectModel DataModel
        {
            get { return base.DataModel as StructureFloatingObjectModel; }
        }

        public MyObjectBuilder_InventoryItem Item
        {
            get { return DataModel.Item; }
            set { DataModel.Item = value; }
        }

        public string SubTypeName
        {
            get { return DataModel.Item.PhysicalContent.SubtypeName; }
        }

        public double? Volume
        {
            get { return DataModel.Volume; }
            set { DataModel.Volume = value; }
        }

        public decimal? Units
        {
            get { return DataModel.Units; }
            set { DataModel.Units = value; }
        }

        #endregion
    }
}
