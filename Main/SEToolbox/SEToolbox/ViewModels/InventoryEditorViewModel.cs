namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Views;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;
    using VRageMath;

    public class InventoryEditorViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly InventoryEditorModel _dataModel;
        private ObservableCollection<InventoryModel> _selections;

        #endregion

        #region Constructors

        public InventoryEditorViewModel(BaseViewModel parentViewModel, InventoryEditorModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public InventoryEditorViewModel(BaseViewModel parentViewModel, InventoryEditorModel dataModel, IDialogService dialogService)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);

            this._dialogService = dialogService;
            this._dataModel = dataModel;
            this.Selections = new ObservableCollection<InventoryModel>();
            // Will bubble property change events from the Model to the ViewModel.
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

        public ICommand AddItemCommand
        {
            get
            {
                return new DelegateCommand(new Action(AddItemExecuted), new Func<bool>(AddItemCanExecute));
            }
        }

        public ICommand DeleteItemCommand
        {
            get
            {
                return new DelegateCommand(new Action(DeleteItemExecuted), new Func<bool>(DeleteItemCanExecute));
            }
        }

        #endregion

        #region properties

        public ObservableCollection<InventoryModel> Selections
        {
            get
            {
                return this._selections;
            }

            set
            {
                if (value != this._selections)
                {
                    this._selections = value;
                    this.RaisePropertyChanged(() => Selections);
                }
            }
        }

        public ObservableCollection<InventoryModel> Items
        {
            get
            {
                return this._dataModel.Items;
            }

            set
            {
                this._dataModel.Items = value;
            }
        }

        public InventoryModel SelectedRow
        {
            get
            {
                return this._dataModel.SelectedRow;
            }

            set
            {
                this._dataModel.SelectedRow = value;
            }
        }

        public double TotalVolume
        {
            get
            {
                return this._dataModel.TotalVolume;
            }

            set
            {
                this._dataModel.TotalVolume = value;
            }
        }

        public float MaxVolume
        {
            get
            {
                return this._dataModel.MaxVolume;
            }

            set
            {
                this._dataModel.MaxVolume = value;
            }
        }

        #endregion

        #region command methods

        public bool AddItemCanExecute()
        {
            return true;
        }

        public void AddItemExecuted()
        {
            var model = new GenerateFloatingObjectModel();
            var position = new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);

            model.Load(position, _dataModel.Settings.MaxFloatingObjects);
            var loadVm = new GenerateFloatingObjectViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowGenerateFloatingObject>(this, loadVm);
            if (result == true)
            {
                var newEntities = loadVm.BuildEntities();
                if (loadVm.IsValidItemToImport)
                {
                    for (var i = 0; i < newEntities.Length; i++)
                    {
                        var item = (MyObjectBuilder_InventoryItem)((MyObjectBuilder_FloatingObject)newEntities[i]).Item;
                        _dataModel.Additem(item);
                    }

                    //  TODO: need to bubble change up to this.MainViewModel.IsModified = true;
                }
            }
        }

        public bool DeleteItemCanExecute()
        {
            return this.SelectedRow != null;
        }

        public void DeleteItemExecuted()
        {
            var index = this.Items.IndexOf(this.SelectedRow);
            _dataModel.RemoveItem(index);

            //  TODO: need to bubble change up to this.MainViewModel.IsModified = true;
        }

        #endregion
    }
}
