namespace SEToolbox.ViewModels
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Input;

    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using Res = SEToolbox.Properties.Resources;

    public class SettingsViewModel : BaseViewModel
    {
        #region Fields

        private readonly SettingsModel _dataModel;
        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly Func<IFolderBrowserDialog> _folderDialogFactory;
        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region Constructors

        public SettingsViewModel(BaseViewModel parentViewModel, SettingsModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>, ServiceLocator.Resolve<IFolderBrowserDialog>)
        {
        }

        public SettingsViewModel(BaseViewModel parentViewModel, SettingsModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory, Func<IFolderBrowserDialog> folderDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);

            _dialogService = dialogService;
            _openFileDialogFactory = openFileDialogFactory;
            _folderDialogFactory = folderDialogFactory;
            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command Properties

        public ICommand BrowseAppPathCommand
        {
            get { return new DelegateCommand(BrowseAppPathExecuted, BrowseAppPathCanExecute); }
        }


        public ICommand BrowseVoxelPathCommand
        {
            get { return new DelegateCommand(BrowseVoxelPathExecuted, BrowseVoxelPathCanExecute); }
        }

        public ICommand OkayCommand
        {
            get { return new DelegateCommand(OkayExecuted, OkayCanExecute); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(CancelExecuted, CancelCanExecute); }
        }

        #endregion

        #region properties

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

        public string SEBinPath
        {
            get { return _dataModel.SEBinPath; }
            set { _dataModel.SEBinPath = value; }
        }

        public string CustomVoxelPath
        {
            get { return _dataModel.CustomVoxelPath; }
            set { _dataModel.CustomVoxelPath = value; }
        }

        public bool? AlwaysCheckForUpdates
        {
            get { return _dataModel.AlwaysCheckForUpdates; }
            set { _dataModel.AlwaysCheckForUpdates = value; }
        }

        public bool? UseCustomResource
        {
            get { return _dataModel.UseCustomResource; }
            set { _dataModel.UseCustomResource = value; }
        }

        public bool IsValid
        {
            get { return _dataModel.IsValid; }
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

        #endregion

        #region command methods

        public bool BrowseAppPathCanExecute()
        {
            return true;
        }

        public void BrowseAppPathExecuted()
        {
            var startPath = SEBinPath;
            if (string.IsNullOrEmpty(startPath))
            {
                startPath = ToolboxUpdater.GetSteamFilePath();
                if (!string.IsNullOrEmpty(startPath))
                {
                    startPath = Path.Combine(startPath, @"SteamApps\common");
                }
            }

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
                var gameBinPath = openFileDialog.FileName;

                if (!string.IsNullOrEmpty(gameBinPath))
                {
                    try
                    {
                        var fullPath = Path.GetFullPath(gameBinPath);
                        if (File.Exists(fullPath))
                        {
                            gameBinPath = Path.GetDirectoryName(fullPath);
                        }
                    }
                    catch { }
                }

                SEBinPath = gameBinPath;
            }
        }

        public bool BrowseVoxelPathCanExecute()
        {
            return true;
        }

        public void BrowseVoxelPathExecuted()
        {
            var folderDialog = _folderDialogFactory();
            folderDialog.Description = Res.DialogLocationCustomVoxelFolder;
            folderDialog.SelectedPath = CustomVoxelPath;
            folderDialog.ShowNewFolderButton = true;

            // Open the dialog
            if (_dialogService.ShowFolderBrowserDialog(this, folderDialog) == DialogResult.OK)
            {
                CustomVoxelPath = folderDialog.SelectedPath;
            }
        }

        public bool OkayCanExecute()
        {
            return IsValid;
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
    }
}
