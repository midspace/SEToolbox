namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;

    public class ComponentListViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly ComponentListModel _dataModel;
        private bool? _closeResult;

        #endregion

        #region Constructors

        public ComponentListViewModel(BaseViewModel parentViewModel, ComponentListModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public ComponentListViewModel(BaseViewModel parentViewModel, ComponentListModel dataModel, IDialogService dialogService)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            this._dialogService = dialogService;
            this._dataModel = dataModel;
            this._dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

        public ICommand CloseCommand
        {
            get
            {
                return new DelegateCommand(new Action(CloseExecuted), new Func<bool>(CloseCanExecute));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get
            {
                return this._closeResult;
            }

            set
            {
                this._closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this._dataModel.IsBusy;
            }

            set
            {
                this._dataModel.IsBusy = value;
            }
        }

        public ObservableCollection<ComonentItemModel> CubeAssets
        {
            get
            {
                return this._dataModel.CubeAssets;
            }

            set
            {
                this._dataModel.CubeAssets = value;
            }
        }

        public ObservableCollection<ComonentItemModel> ComponentAssets
        {
            get
            {
                return this._dataModel.ComponentAssets;
            }

            set
            {
                this._dataModel.ComponentAssets = value;
            }
        }

        public ObservableCollection<ComonentItemModel> ItemAssets
        {
            get
            {
                return this._dataModel.ItemAssets;
            }

            set
            {
                this._dataModel.ItemAssets = value;
            }
        }

        public ObservableCollection<ComonentItemModel> MaterialAssets
        {
            get
            {
                return this._dataModel.MaterialAssets;
            }

            set
            {
                this._dataModel.MaterialAssets = value;
            }
        }

        #endregion

        #region methods

        public bool CloseCanExecute()
        {
            return true;
        }

        public void CloseExecuted()
        {
            this.CloseResult = false;
        }

        #endregion
    }
}
