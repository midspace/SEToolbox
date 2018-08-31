namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using Sandbox.Definitions;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;

    public class CubeItemViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly CubeItemModel _dataModel;
        private Lazy<ObservableCollection<InventoryEditorViewModel>> _inventory;

        #endregion

        #region Constructors

        public CubeItemViewModel(BaseViewModel parentViewModel, CubeItemModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public CubeItemViewModel(BaseViewModel parentViewModel, CubeItemModel dataModel, IDialogService dialogService)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            _dialogService = dialogService;
            _dataModel = dataModel;

            Func<InventoryEditorModel, InventoryEditorViewModel> viewModelCreator = model => new InventoryEditorViewModel(this, model);
            Func<ObservableCollection<InventoryEditorViewModel>> collectionCreator =
                () => new ObservableViewModelCollection<InventoryEditorViewModel, InventoryEditorModel>(dataModel.Inventory, viewModelCreator);
            _inventory = new Lazy<ObservableCollection<InventoryEditorViewModel>>(collectionCreator);

            _dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "Inventory")
                {
                    collectionCreator.Invoke();
                    _inventory = new Lazy<ObservableCollection<InventoryEditorViewModel>>(collectionCreator);
                }
                // Will bubble property change events from the Model to the ViewModel.
                OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

        //public ICommand ApplyCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand(ApplyExecuted, ApplyCanExecute);
        //    }
        //}

        //public ICommand CancelCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand(CancelExecuted, CancelCanExecute);
        //    }
        //}

        #endregion

        #region Properties

        public CubeItemModel DataModel
        {
            get { return _dataModel; }
        }

        public bool IsSelected
        {
            get { return _dataModel.IsSelected; }
            set { _dataModel.IsSelected = value; }
        }

        public MyObjectBuilder_CubeBlock Cube
        {
            get { return _dataModel.Cube; }
            set { _dataModel.Cube = value; }
        }

        public MyObjectBuilderType TypeId
        {
            get { return _dataModel.TypeId; }
            set { _dataModel.TypeId = value; }
        }

        public string SubtypeId
        {
            get { return _dataModel.SubtypeId; }
            set { _dataModel.SubtypeId = value; }
        }

        public string TextureFile
        {
            get { return _dataModel.TextureFile; }
            set { _dataModel.TextureFile = value; }
        }

        public MyCubeSize CubeSize
        {
            get { return _dataModel.CubeSize; }
            set { _dataModel.CubeSize = value; }
        }

        public string FriendlyName
        {
            get { return _dataModel.FriendlyName; }
            set { _dataModel.FriendlyName = value; }
        }

        public string OwnerName
        {
            get { return _dataModel.OwnerName; }
            set { _dataModel.OwnerName = value; }
        }

        public long Owner
        {
            get { return _dataModel.Owner; }
            set { _dataModel.Owner = value; }
        }

        public string BuiltByName
        {
            get { return _dataModel.BuiltByName; }
            set { _dataModel.BuiltByName = value; }
        }

        public long BuiltBy
        {
            get { return _dataModel.BuiltBy; }
            set { _dataModel.BuiltBy = value; }
        }

        public string ColorText
        {
            get { return _dataModel.ColorText; }
            set { _dataModel.ColorText = value; }
        }

        public float ColorHue
        {
            get { return _dataModel.ColorHue; }
            set { _dataModel.ColorHue = value; }
        }

        public float ColorSaturation
        {
            get { return _dataModel.ColorSaturation; }
            set { _dataModel.ColorSaturation = value; }
        }

        public float ColorLuminance
        {
            get { return _dataModel.ColorLuminance; }
            set { _dataModel.ColorLuminance = value; }
        }

        public BindablePoint3DIModel Position
        {
            get { return _dataModel.Position; }
            set { _dataModel.Position = value; }
        }

        public double BuildPercent
        {
            get { return _dataModel.BuildPercent; }
            set { _dataModel.BuildPercent = value; }
        }

        public System.Windows.Media.Brush Color
        {
            get { return _dataModel.Color; }
            set { _dataModel.Color = value; }
        }

        public int PCU
        {
            get { return _dataModel.PCU; }
            set { _dataModel.PCU = value; }
        }

        public ObservableCollection<InventoryEditorViewModel> Inventory
        {
            get { return _inventory.Value; }
        }

        public override string ToString()
        {
            return FriendlyName;
        }

        #endregion

        #region methods

        public void UpdateColor(SerializableVector3 vector3)
        {
            _dataModel.UpdateColor(vector3);
        }

        public void UpdateBuildPercent(double buildPercent)
        {
            _dataModel.UpdateBuildPercent(buildPercent);
        }

        public bool ConvertFromLightToHeavyArmor()
        {
            return CubeItemModel.ConvertFromLightToHeavyArmor(_dataModel.Cube);
        }

        public bool ConvertFromHeavyToLightArmor()
        {
            return CubeItemModel.ConvertFromHeavyToLightArmor(_dataModel.Cube);
        }

        public MyObjectBuilder_CubeBlock CreateCube(MyObjectBuilderType typeId, string subTypeId, MyCubeBlockDefinition definition)
        {
            return _dataModel.CreateCube(typeId, subTypeId, definition);
        }

        public bool ChangeOwner(long newOwnerId)
        {
            return _dataModel.ChangeOwner(newOwnerId);
        }

        public bool ChangeBuiltBy(long newBuiltById)
        {
            return _dataModel.ChangeBuiltBy(newBuiltById);
        }

        #endregion
    }
}