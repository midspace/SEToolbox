namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Res = SEToolbox.Properties.Resources;

    public class BlueprintDialogViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly BlueprintDialogModel _dataModel;
        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region ctor

        public BlueprintDialogViewModel(BaseViewModel parentViewModel, BlueprintDialogModel dataModel, IDialogService dialogService)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);

            _dialogService = dialogService;
            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

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

        public string BlueprintName
        {
            get { return _dataModel.BlueprintName; }
            set { _dataModel.BlueprintName = value; }
        }

        public string DialogTitle
        {
            get { return _dataModel.DialogTitle; }
            set { _dataModel.DialogTitle = value; }
        }

        public bool CheckForExisting
        {
            get { return _dataModel.CheckForExisting; }
            set { _dataModel.CheckForExisting = value; }
        }

        #endregion

        #region methods

        #region commands

        public bool OkayCanExecute()
        {
            return !string.IsNullOrWhiteSpace(BlueprintName);
        }

        public void OkayExecuted()
        {
            BlueprintName = BlueprintName.Trim();

            if (BlueprintName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                _dialogService.ShowMessageBox(this, Res.ErrorInvalidBlueprintCharactersUsed, Res.ErrorInvalidBlueprintNameTitle, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
                return;
            }

            string filePath = Path.Combine(_dataModel.LocalBlueprintsFolder, BlueprintName);

            try
            {
                // check for invalid filename charactrers.
                filePath = Path.GetFullPath(filePath);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessageBox(this, ex.Message, Res.ErrorInvalidBlueprintNameTitle, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
                return;
            }

            // validate existing.
            if (CheckForExisting && Directory.Exists(filePath))
            {
                // dialog confirm overwrite....
                if (_dialogService.ShowMessageBox(this, Res.ErrorInvalidBlueprintExists, Res.ErrorInvalidBlueprintExistsTitle, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes)
                    return;
            }

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
