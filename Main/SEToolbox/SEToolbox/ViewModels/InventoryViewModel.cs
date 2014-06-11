namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using VRageMath;
    using Res = SEToolbox.Properties.Resources;
    using SEToolbox.Views;

    public class InventoryViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly InventoryModel _dataModel;

        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region Constructors

        public InventoryViewModel(BaseViewModel parentViewModel, InventoryModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public InventoryViewModel(BaseViewModel parentViewModel, InventoryModel dataModel, IDialogService dialogService)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);

            this._dialogService = dialogService;
            this._dataModel = dataModel;
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

        public ObservableCollection<ComponentItemModel> Items
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

        public ComponentItemModel SelectedRow
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
        }

        #endregion
    }
}
