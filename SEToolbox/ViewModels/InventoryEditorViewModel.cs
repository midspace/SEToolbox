namespace SEToolbox.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Views;
    using VRage;
    using VRage.Game;
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

            _dialogService = dialogService;
            _dataModel = dataModel;
            Selections = new ObservableCollection<InventoryModel>();
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

        public ICommand AddItemCommand
        {
            get { return new DelegateCommand(AddItemExecuted, AddItemCanExecute); }
        }

        public ICommand DeleteItemCommand
        {
            get { return new DelegateCommand(DeleteItemExecuted, DeleteItemCanExecute); }
        }

        #endregion

        #region properties

        public ObservableCollection<InventoryModel> Selections
        {
            get { return _selections; }

            set
            {
                if (value != _selections)
                {
                    _selections = value;
                    OnPropertyChanged(nameof(Selections));
                }
            }
        }

        public ObservableCollection<InventoryModel> Items
        {
            get { return _dataModel.Items; }
            set { _dataModel.Items = value; }
        }

        public InventoryModel SelectedRow
        {
            get { return _dataModel.SelectedRow; }
            set { _dataModel.SelectedRow = value; }
        }

        public double TotalVolume
        {
            get { return _dataModel.TotalVolume; }
            set { _dataModel.TotalVolume = value; }
        }

        public float MaxVolume
        {
            get { return _dataModel.MaxVolume; }
            set { _dataModel.MaxVolume = value; }
        }

        public string Name
        {
            get { return _dataModel.Name; }
            set { _dataModel.Name = value; }
        }

        public bool IsValid
        {
            get { return _dataModel.IsValid; }
            set { _dataModel.IsValid = value; }
        }

        #endregion

        #region command methods

        public bool AddItemCanExecute()
        {
            return _dataModel.IsValid;
        }

        public void AddItemExecuted()
        {
            var model = new GenerateFloatingObjectModel();
            var position = new MyPositionAndOrientation(Vector3D.Zero, Vector3.Forward, Vector3.Up);
            var settings = SpaceEngineersCore.WorldResource.Checkpoint.Settings;

            model.Load(position, settings.MaxFloatingObjects);
            var loadVm = new GenerateFloatingObjectViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowGenerateFloatingObject>(this, loadVm);
            if (result == true)
            {
                var newEntities = loadVm.BuildEntities();
                if (loadVm.IsValidItemToImport)
                {
                    for (var i = 0; i < newEntities.Length; i++)
                    {
                        var item = ((MyObjectBuilder_FloatingObject)newEntities[i]).Item;
                        _dataModel.Additem(item);
                    }

                    //  TODO: need to bubble change up to MainViewModel.IsModified = true;
                }
            }
        }

        public bool DeleteItemCanExecute()
        {
            return SelectedRow != null;
        }

        public void DeleteItemExecuted()
        {
            var index = Items.IndexOf(SelectedRow);
            _dataModel.RemoveItem(index);

            //  TODO: need to bubble change up to MainViewModel.IsModified = true;
        }

        #endregion
    }
}
