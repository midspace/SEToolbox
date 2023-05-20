namespace SEToolbox.ViewModels
{
    using System;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System.Windows.Forms;
    using System.Windows.Input;

    public class ErrorDialogViewModel : BaseViewModel
    {
        #region Fields

        private readonly ErrorDialogModel _dataModel;
        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region ctor

        public ErrorDialogViewModel(BaseViewModel parentViewModel, ErrorDialogModel dataModel)
            : base(parentViewModel)
        {

            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

        public ICommand CopyCommand => new DelegateCommand(CopyExecuted, CopyCanExecute);

        public ICommand OkayCommand => new DelegateCommand(OkayExecuted, OkayCanExecute);

        public ICommand CancelCommand => new DelegateCommand(CancelExecuted, CancelCanExecute);

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
                        Application.DoEvents();
                    }
                }
            }
        }

        public string ErrorDescription
        {
            get { return _dataModel.ErrorDescription; }
            set { _dataModel.ErrorDescription = value; }
        }

        public string ErrorText
        {
            get { return _dataModel.ErrorText; }
            set { _dataModel.ErrorText = value; }
        }

        public bool IsWarning
        {
            get { return _dataModel.CanContinue; }
            set { _dataModel.CanContinue = value; }
        }

        public bool IsError
        {
            get { return !_dataModel.CanContinue; }
            set { _dataModel.CanContinue = !value; }
        }

        #endregion

        #region methods

        #region commands

        public bool CopyCanExecute()
        {
            return true;
        }

        public void CopyExecuted()
        {
            Clipboard.SetText(_dataModel.ErrorDescription + Environment.NewLine + _dataModel.ErrorText);
        }

        public bool OkayCanExecute()
        {
            return true;
        }

        public void OkayExecuted()
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
