namespace SEToolbox.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq.Expressions;
    using System.Windows.Forms;
    using System.Windows.Input;

    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using Res = SEToolbox.Properties.Resources;

    // Do not inherit from BaseViewModel, as this implments part of Keen's assemblies, which we do not want for this VM.
    public class FindApplicationViewModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly FindApplicationModel _dataModel;
        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private bool? _closeResult;

        #endregion

        #region Constructors

        public FindApplicationViewModel(FindApplicationModel dataModel)
            : this(dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>)
        {
        }

        public FindApplicationViewModel(FindApplicationModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            _dialogService = dialogService;
            _openFileDialogFactory = openFileDialogFactory;
            _dataModel = dataModel;

            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Command Properties

        public ICommand BrowseApplicationCommand
        {
            get { return new DelegateCommand(BrowseApplicationExecuted, BrowseApplicationCanExecute); }
        }

        public ICommand ContinueCommand
        {
            get { return new DelegateCommand(ContinueExecuted, ContinueCanExecute); }
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
                RaisePropertyChanged(() => CloseResult);
            }
        }

        public string GameApplicationPath
        {
            get { return _dataModel.GameApplicationPath; }
            set { _dataModel.GameApplicationPath = value; }
        }

        public string GameBinPath
        {
            get { return _dataModel.GameBinPath; }
            set { _dataModel.GameBinPath = value; }
        }


        public bool IsValidApplication
        {
            get { return _dataModel.IsValidApplication; }
            set { _dataModel.IsValidApplication = value; }
        }

        public bool IsWrongApplication
        {
            get { return _dataModel.IsWrongApplication; }
            set { _dataModel.IsWrongApplication = value; }
        }

        #endregion

        #region Command Methods

        public bool BrowseApplicationCanExecute()
        {
            return true;
        }

        public void BrowseApplicationExecuted()
        {
            var startPath = GameApplicationPath;
            if (string.IsNullOrEmpty(startPath))
            {
                startPath = ToolboxUpdater.GetSteamFilePath();
                if (!string.IsNullOrEmpty(startPath))
                {
                    startPath = Path.Combine(startPath, @"SteamApps\common");
                }
            }

            IsValidApplication = false;
            IsWrongApplication = false;

            var openFileDialog = _openFileDialogFactory();
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.DefaultExt = "exe";
            openFileDialog.FileName = "SpaceEngineers";
            openFileDialog.Filter = AppConstants.SpaceEngineersApplicationFilter;
            openFileDialog.InitialDirectory = startPath;
            openFileDialog.Multiselect = false;
            openFileDialog.Title = Res.DialogLocateApplicationTitle;

            // Open the dialog
            if (_dialogService.ShowOpenFileDialog(this, openFileDialog) == DialogResult.OK)
            {
                GameApplicationPath = openFileDialog.FileName;
            }
        }

        public bool ContinueCanExecute()
        {
            return IsValidApplication;
        }

        public void ContinueExecuted()
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

        #region Methods

        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        //[Obsolete("Use RaisePropertyChanged(() => PropertyName) instead.")]
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RaisePropertyChanged(params Expression<Func<object>>[] expression)
        {
            PropertyChanged.Raise(expression);
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}