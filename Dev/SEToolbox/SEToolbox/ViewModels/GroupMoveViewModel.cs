namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;

    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;

    public class GroupMoveViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly GroupMoveModel _dataModel;
        private bool? _closeResult;

        #endregion

        #region Constructors

        public GroupMoveViewModel(BaseViewModel parentViewModel, GroupMoveModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public GroupMoveViewModel(BaseViewModel parentViewModel, GroupMoveModel dataModel, IDialogService dialogService)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            _dialogService = dialogService;
            _dataModel = dataModel;

            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command Properties

        public ICommand ApplyCommand
        {
            get { return new DelegateCommand(ApplyExecuted, ApplyCanExecute); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(CancelExecuted, CancelCanExecute); }
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
                return _closeResult;
            }

            set
            {
                _closeResult = value;
                OnPropertyChanged(nameof(CloseResult));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _dataModel.IsBusy;
            }

            set
            {
                _dataModel.IsBusy = value;
            }
        }

        public float GlobalOffsetPositionX
        {
            get
            {
                return _dataModel.GlobalOffsetPositionX;
            }

            set
            {
                _dataModel.GlobalOffsetPositionX = value;
                _dataModel.CalcOffsetDistances();
            }
        }

        public float GlobalOffsetPositionY
        {
            get
            {
                return _dataModel.GlobalOffsetPositionY;
            }

            set
            {
                _dataModel.GlobalOffsetPositionY = value;
                _dataModel.CalcOffsetDistances();
            }
        }

        public float GlobalOffsetPositionZ
        {
            get
            {
                return _dataModel.GlobalOffsetPositionZ;
            }

            set
            {
                _dataModel.GlobalOffsetPositionZ = value;
                _dataModel.CalcOffsetDistances();
            }
        }

        public bool IsGlobalOffsetPosition
        {
            get
            {
                return _dataModel.IsGlobalOffsetPosition;
            }

            set
            {
                _dataModel.IsGlobalOffsetPosition = value;
                _dataModel.CalcOffsetDistances();
            }
        }

        public float SinglePositionX
        {
            get
            {
                return _dataModel.SinglePositionX;
            }

            set
            {
                _dataModel.SinglePositionX = value;
                _dataModel.CalcOffsetDistances();
            }
        }

        public float SinglePositionY
        {
            get
            {
                return _dataModel.SinglePositionY;
            }

            set
            {
                _dataModel.SinglePositionY = value;
                _dataModel.CalcOffsetDistances();
            }
        }

        public float SinglePositionZ
        {
            get
            {
                return _dataModel.SinglePositionZ;
            }

            set
            {
                _dataModel.SinglePositionZ = value;
                _dataModel.CalcOffsetDistances();
            }
        }

        public bool IsSinglePosition
        {
            get
            {
                return _dataModel.IsSinglePosition;
            }

            set
            {
                _dataModel.IsSinglePosition = value;
                _dataModel.CalcOffsetDistances();
            }
        }

        public ObservableCollection<GroupMoveItemModel> Selections
        {
            get
            {
                return _dataModel.Selections;
            }

            set
            {
                _dataModel.Selections = value;
            }
        }

        #endregion

        #region methods

        public bool ApplyCanExecute()
        {
            return IsSinglePosition ||
                (IsGlobalOffsetPosition && (GlobalOffsetPositionX != 0 || GlobalOffsetPositionY != 0 || GlobalOffsetPositionZ != 0));
        }

        public void ApplyExecuted()
        {
            CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            CloseResult = false;
        }

        #endregion
    }
}
