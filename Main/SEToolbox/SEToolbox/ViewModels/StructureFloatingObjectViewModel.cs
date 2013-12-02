namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    public class StructureFloatingObjectViewModel : StructureBaseViewModel<StructureFloatingObjectModel>
    {
        #region ctor

        public StructureFloatingObjectViewModel(BaseViewModel parentViewModel, StructureFloatingObjectModel dataModel)
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

        protected new StructureFloatingObjectModel DataModel
        {
            get
            {
                return base.DataModel as StructureFloatingObjectModel;
            }
        }

        public MyObjectBuilder_InventoryItem Item
        {
            get
            {
                return this.DataModel.Item;
            }

            set
            {
                this.DataModel.Item = value;
            }
        }

        public string SubTypeName
        {
            get
            {
                return this.DataModel.Item.Content.SubtypeName;
            }
        }

        #endregion

        #region methods

        #endregion
    }
}
