namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Common.ObjectBuilders.VRageData;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;

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
            this._dialogService = dialogService;
            this._dataModel = dataModel;

            Func<InventoryEditorModel, InventoryEditorViewModel> viewModelCreator = model => new InventoryEditorViewModel(this, model);
            Func<ObservableCollection<InventoryEditorViewModel>> collectionCreator =
                () => new ObservableViewModelCollection<InventoryEditorViewModel, InventoryEditorModel>(dataModel.Inventory, viewModelCreator);
            _inventory = new Lazy<ObservableCollection<InventoryEditorViewModel>>(collectionCreator);

            this._dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "Inventory")
                {
                    collectionCreator.Invoke();
                    _inventory = new Lazy<ObservableCollection<InventoryEditorViewModel>>(collectionCreator);
                }
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

        //public ICommand ApplyCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand(new Action(ApplyExecuted), new Func<bool>(ApplyCanExecute));
        //    }
        //}

        //public ICommand CancelCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand(new Action(CancelExecuted), new Func<bool>(CancelCanExecute));
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
            get { return this._dataModel.IsSelected; }
            set { this._dataModel.IsSelected = value; }
        }

        public MyObjectBuilder_CubeBlock Cube
        {
            get { return this._dataModel.Cube; }
            set { this._dataModel.Cube = value; }
        }

        public MyObjectBuilderType TypeId
        {
            get { return this._dataModel.TypeId; }
            set { this._dataModel.TypeId = value; }
        }

        public string SubtypeId
        {
            get { return this._dataModel.SubtypeId; }
            set { this._dataModel.SubtypeId = value; }
        }

        public string TextureFile
        {
            get { return this._dataModel.TextureFile; }
            set { this._dataModel.TextureFile = value; }
        }

        public MyCubeSize CubeSize
        {
            get { return this._dataModel.CubeSize; }
            set { this._dataModel.CubeSize = value; }
        }

        public string FriendlyName
        {
            get { return this._dataModel.FriendlyName; }
            set { this._dataModel.FriendlyName = value; }
        }

        public string ColorText
        {
            get { return this._dataModel.ColorText; }
            set { this._dataModel.ColorText = value; }
        }

        public float ColorHue
        {
            get { return this._dataModel.ColorHue; }
            set { this._dataModel.ColorHue = value; }
        }

        public float ColorSaturation
        {
            get { return this._dataModel.ColorSaturation; }
            set { this._dataModel.ColorSaturation = value; }
        }

        public float ColorLuminance
        {
            get { return this._dataModel.ColorLuminance; }
            set { this._dataModel.ColorLuminance = value; }
        }

        public BindablePoint3DIModel Position
        {
            get { return this._dataModel.Position; }
            set { this._dataModel.Position = value; }
        }

        public double BuildPercent
        {
            get { return this._dataModel.BuildPercent; }
            set { this._dataModel.BuildPercent = value; }
        }

        public System.Windows.Media.Brush Color
        {
            get { return this._dataModel.Color; }
            set { this._dataModel.Color = value; }
        }

        public ObservableCollection<InventoryEditorViewModel> Inventory
        {
            get { return this._inventory.Value; }
        }

        public MySessionSettings Settings
        {
            get { return this._dataModel.Settings; }
            set { this._dataModel.Settings = value; }
        }

        public override string ToString()
        {
            return this.FriendlyName;
        }

        #endregion

        #region methods

        public void UpdateColor(SerializableVector3 vector3)
        {
            this._dataModel.UpdateColor(vector3);
        }

        public void UpdateBuildPercent(double buildPercent)
        {
            this._dataModel.UpdateBuildPercent(buildPercent);
        }

        public MyObjectBuilder_CubeBlock CreateCube(MyObjectBuilderType typeId, string subTypeId, MyObjectBuilder_CubeBlockDefinition definition, MySessionSettings settings)
        {
            return this._dataModel.CreateCube(typeId, subTypeId, definition, settings);
        }

        #endregion
    }
}