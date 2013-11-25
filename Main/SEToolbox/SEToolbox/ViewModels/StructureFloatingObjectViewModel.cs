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

        public ICommand DeleteObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(DeleteObjectExecuted), new Func<bool>(DeleteObjectCanExecute));
            }
        }

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

        public bool DeleteObjectCanExecute()
        {
            return true;
        }

        public void DeleteObjectExecuted()
        {
            ((ExplorerViewModel)this.OwnerViewModel).DeleteModel(this);
        }

        #endregion
    }
}
