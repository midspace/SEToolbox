namespace SEToolbox.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using SEToolbox.Models;
    using SEToolbox.Services;

    public class ChangeOwnerViewModel : BaseViewModel
    {
        #region Fields

        private readonly ChangeOwnerModel _dataModel;
        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region ctor

        public ChangeOwnerViewModel(BaseViewModel parentViewModel, ChangeOwnerModel dataModel)
            : base(parentViewModel)
        {

            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

        public ICommand ChangeCommand
        {
            get { return new DelegateCommand(ChangeExecuted, ChangeCanExecute); }
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
            get { return _closeResult; }

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
            get { return _isBusy; }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    if (_isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        public ObservableCollection<OwnerModel> PlayerList
        {
            get { return _dataModel.PlayerList; }
        }

        public OwnerModel SelectedPlayer
        {
            get { return _dataModel.SelectedPlayer; }
            set { _dataModel.SelectedPlayer = value; }
        }

        public string Title
        {
            get { return _dataModel.Title; }
        }

        #endregion

        #region methods

        #region commands

        public bool ChangeCanExecute()
        {
            return SelectedPlayer != null;
        }

        public void ChangeExecuted()
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

        #endregion
    }
}
