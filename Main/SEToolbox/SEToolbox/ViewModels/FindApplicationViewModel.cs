namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Properties;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;

    public class FindApplicationViewModel : BaseViewModel
    {
        #region Fields

        private readonly FindApplicationModel _dataModel;
        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private bool? _closeResult;

        #endregion

        #region Constructors

        public FindApplicationViewModel(FindApplicationModel dataModel)
            : this(dataModel, ServiceLocator.Resolve<IDialogService>(), () => ServiceLocator.Resolve<IOpenFileDialog>())
        {
        }

        public FindApplicationViewModel(FindApplicationModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
            : base(null)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            this._dialogService = dialogService;
            this._openFileDialogFactory = openFileDialogFactory;
            this._dataModel = dataModel;

            // Will bubble property change events from the Model to the ViewModel.
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Command Properties

        public ICommand BrowseApplicationCommand
        {
            get
            {
                return new DelegateCommand(new Action(BrowseApplicationExecuted), new Func<bool>(BrowseApplicationCanExecute));
            }
        }

        public ICommand ContinueCommand
        {
            get
            {
                return new DelegateCommand(new Action(ContinueExecuted), new Func<bool>(ContinueCanExecute));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(new Action(CancelExecuted), new Func<bool>(CancelCanExecute));
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

        public string GameApplicationPath
        {
            get
            {
                return this._dataModel.GameApplicationPath;
            }

            set
            {
                this._dataModel.GameApplicationPath = value;
            }
        }

        public string GameRootPath
        {
            get
            {
                return this._dataModel.GameRootPath;
            }

            set
            {
                this._dataModel.GameRootPath = value;
            }
        }


        public bool IsValidApplication
        {
            get
            {
                return this._dataModel.IsValidApplication;
            }

            set
            {
                this._dataModel.IsValidApplication = value;
            }
        }

        public bool IsWrongApplication
        {
            get
            {
                return this._dataModel.IsWrongApplication;
            }

            set
            {
                this._dataModel.IsWrongApplication = value;
            }
        }

        #endregion

        #region Command Methods

        public bool BrowseApplicationCanExecute()
        {
            return true;
        }

        public void BrowseApplicationExecuted()
        {
            var startPath = this.GameApplicationPath;
            if (string.IsNullOrEmpty(startPath))
            {

                startPath = ToolboxUpdater.GetSteamFilePath();
                if (!string.IsNullOrEmpty(startPath))
                {
                    startPath = Path.Combine(startPath, @"SteamApps\common");
                }
            }

            this.IsValidApplication = false;
            this.IsWrongApplication = false;

            IOpenFileDialog openFileDialog = _openFileDialogFactory();
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.DefaultExt = "exe";
            openFileDialog.FileName = "SpaceEngineers";
            openFileDialog.Filter = Resources.LocateApplicationFilter;
            openFileDialog.InitialDirectory = startPath;
            openFileDialog.Multiselect = false;
            openFileDialog.Title = Resources.LocateApplicationTitle;

            // Open the dialog
            DialogResult result = _dialogService.ShowOpenFileDialog(this, openFileDialog);

            if (result == DialogResult.OK)
            {
                GameApplicationPath = openFileDialog.FileName;
            }
        }

        public bool ContinueCanExecute()
        {
            return this.IsValidApplication;
        }

        public void ContinueExecuted()
        {
            this.CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            this.CloseResult = false;
        }

        #endregion
    }
}