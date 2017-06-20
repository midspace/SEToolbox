namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Media;
    using System.Windows.Forms;
    using System.Windows.Input;

    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using Res = SEToolbox.Properties.Resources;

    public class SelectWorldViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly SelectWorldModel _dataModel;
        private bool? _closeResult;
        private bool _zoomThumbnail;

        #endregion

        #region Constructors

        public SelectWorldViewModel(BaseViewModel parentViewModel, SelectWorldModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>)
        {
        }

        public SelectWorldViewModel(BaseViewModel parentViewModel, SelectWorldModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory)
            : base(parentViewModel)
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

        #region command Properties

        public ICommand LoadCommand
        {
            get { return new DelegateCommand(LoadExecuted, LoadCanExecute); }
        }

        public ICommand RefreshCommand
        {
            get { return new DelegateCommand(RefreshExecuted, RefreshCanExecute); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(CancelExecuted, CancelCanExecute); }
        }

        public ICommand RepairCommand
        {
            get { return new DelegateCommand(RepairExecuted, RepairCanExecute); }
        }

        public ICommand BrowseCommand
        {
            get { return new DelegateCommand(BrowseExecuted, BrowseCanExecute); }
        }

        public ICommand OpenFolderCommand
        {
            get { return new DelegateCommand(OpenFolderExecuted, OpenFolderCanExecute); }
        }

        public ICommand OpenWorkshopCommand
        {
            get { return new DelegateCommand(OpenWorkshopExecuted, OpenWorkshopCanExecute); }
        }

        public ICommand ZoomThumbnailCommand
        {
            get { return new DelegateCommand(ZoomThumbnailExecuted, ZoomThumbnailCanExecute); }
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
                if (_closeResult != value)
                {
                    _closeResult = value;
                    RaisePropertyChanged(() => CloseResult);
                }
            }
        }

        public bool ZoomThumbnail
        {
            get { return _zoomThumbnail; }

            set
            {
                if (_zoomThumbnail != value)
                {
                    _zoomThumbnail = value;
                    RaisePropertyChanged(() => ZoomThumbnail);
                }
            }
        }

        public WorldResource SelectedWorld
        {
            get { return _dataModel.SelectedWorld; }
            set { _dataModel.SelectedWorld = value; }
        }

        public ObservableCollection<WorldResource> Worlds
        {
            get { return _dataModel.Worlds; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get { return _dataModel.IsBusy; }
            set { _dataModel.IsBusy = value; }
        }

        #endregion

        #region methods

        public bool LoadCanExecute()
        {
            return SelectedWorld != null && SelectedWorld.IsValid;
        }

        public void LoadExecuted()
        {
            CloseResult = true;
        }

        public bool RefreshCanExecute()
        {
            return !IsBusy;
        }

        public void RefreshExecuted()
        {
            _dataModel.Refresh();
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            CloseResult = false;
        }

        public bool RepairCanExecute()
        {
            return SelectedWorld != null &&
                (SelectedWorld.SaveType != SaveWorldType.DedicatedServerService ||
                (SelectedWorld.SaveType == SaveWorldType.DedicatedServerService && ToolboxUpdater.IsRuningElevated()));
        }

        public void RepairExecuted()
        {
            IsBusy = true;
            var results = SpaceEngineersRepair.RepairSandBox(_dataModel.SelectedWorld);
            IsBusy = false;
            _dialogService.ShowMessageBox(this, results, Res.ClsRepairTitle, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.None);
        }

        public bool BrowseCanExecute()
        {
            return true;
        }

        public void BrowseExecuted()
        {
            var openFileDialog = _openFileDialogFactory();
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.DefaultExt = "sbc";
            openFileDialog.FileName = "Sandbox.sbc";
            openFileDialog.Filter = AppConstants.SandboxFilter;
            openFileDialog.Multiselect = false;
            openFileDialog.Title = Res.DialogLocateSandboxTitle;

            if (_dialogService.ShowOpenFileDialog(this, openFileDialog) == DialogResult.OK)
            {
                var savePath = Path.GetDirectoryName(openFileDialog.FileName);
                var userName = Environment.UserName;
                var saveType = SaveWorldType.Custom;

                try
                {
                    using (var fs = File.OpenWrite(openFileDialog.FileName))
                    {
                        // test opening the file to verify that we have Write Access.
                    }
                }
                catch
                {
                    saveType = SaveWorldType.CustomAdminRequired;
                }

                // Determine the correct UserDataPath for this custom save game if at all possible for the mods.
                var dp = UserDataPath.FindFromSavePath(savePath);

                var saveResource = _dataModel.LoadSaveFromPath(savePath, userName, saveType, dp);
                saveResource.LoadCheckpoint();

                if (saveResource.IsValid)
                {
                    SelectedWorld = saveResource;
                    CloseResult = true;
                }
                else
                {
                    SystemSounds.Beep.Play();
                }
            }
        }

        public bool OpenFolderCanExecute()
        {
            return SelectedWorld != null;
        }

        public void OpenFolderExecuted()
        {
            System.Diagnostics.Process.Start("Explorer", string.Format("\"{0}\"", SelectedWorld.Savepath));
        }

        public bool OpenWorkshopCanExecute()
        {
            return SelectedWorld != null && SelectedWorld.WorkshopId.HasValue &&
                   SelectedWorld.WorkshopId.Value != 0;
        }

        public void OpenWorkshopExecuted()
        {
            if (SelectedWorld.WorkshopId.HasValue)
                System.Diagnostics.Process.Start(string.Format("http://steamcommunity.com/sharedfiles/filedetails/?id={0}", SelectedWorld.WorkshopId.Value), null);
        }

        public bool ZoomThumbnailCanExecute()
        {
            return SelectedWorld != null && SelectedWorld.ThumbnailImageFilename != null;
        }

        public void ZoomThumbnailExecuted()
        {
            ZoomThumbnail = !ZoomThumbnail;
        }

        #endregion
    }
}
