namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;

    public class SelectWorldViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private SelectWorldModel _dataModel;
        private bool? _closeResult;
        private bool _zoomThumbnail;

        #endregion

        #region Constructors

        public SelectWorldViewModel(BaseViewModel parentViewModel, SelectWorldModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public SelectWorldViewModel(BaseViewModel parentViewModel, SelectWorldModel dataModel, IDialogService dialogService)
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

        public ICommand LoadCommand
        {
            get
            {
                return new DelegateCommand(new Action(LoadExecuted), new Func<bool>(LoadCanExecute));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(new Action(CancelExecuted), new Func<bool>(CancelCanExecute));
            }
        }

        public ICommand RepairCommand
        {
            get
            {
                return new DelegateCommand(new Action(RepairExecuted), new Func<bool>(RepairCanExecute));
            }
        }

        public ICommand OpenFolderCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenFolderExecuted), new Func<bool>(OpenFolderCanExecute));
            }
        }

        public ICommand OpenWorkshopCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenWorkshopExecuted), new Func<bool>(OpenWorkshopCanExecute));
            }
        }

        public ICommand ZoomThumbnailCommand
        {
            get
            {
                return new DelegateCommand(new Action(ZoomThumbnailExecuted), new Func<bool>(ZoomThumbnailCanExecute));
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
                if (this._closeResult != value)
                {
                this._closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
                }
            }
        }

        public bool ZoomThumbnail
        {
            get
            {
                return this._zoomThumbnail;
            }

            set
            {
                if (this._zoomThumbnail != value)
                {
                    this._zoomThumbnail = value;
                    this.RaisePropertyChanged(() => ZoomThumbnail);
                }
            }
        }

        public SaveResource SelectedWorld
        {
            get
            {
                return this._dataModel.SelectedWorld;
            }
            set
            {
                if (value != this._dataModel.SelectedWorld)
                {
                    this._dataModel.SelectedWorld = value;
                }
            }
        }

        public ObservableCollection<SaveResource> Worlds
        {
            get
            {
                return this._dataModel.Worlds;
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

        #endregion

        #region methods

        public bool LoadCanExecute()
        {
            return this.SelectedWorld != null && this.SelectedWorld.IsValid;
        }

        public void LoadExecuted()
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

        public bool RepairCanExecute()
        {
            return this.SelectedWorld != null &&
                (this.SelectedWorld.SaveType != SaveWorldType.DedicatedServerService ||
                (this.SelectedWorld.SaveType == SaveWorldType.DedicatedServerService && ToolboxUpdater.CheckIsRuningElevated()));
        }

        public void RepairExecuted()
        {
            this.IsBusy = true;
            var results = this._dataModel.RepairSandBox();
            this.IsBusy = false;
            var result = _dialogService.ShowMessageBox(this, results, "Repair results", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.None);
        }

        public bool OpenFolderCanExecute()
        {
            return this.SelectedWorld != null;
        }

        public void OpenFolderExecuted()
        {
            System.Diagnostics.Process.Start("Explorer", string.Format("\"{0}\"", this.SelectedWorld.Savepath));
        }

        public bool OpenWorkshopCanExecute()
        {
            return this.SelectedWorld != null && this.SelectedWorld.WorkshopId.HasValue &&
                   this.SelectedWorld.WorkshopId.Value != 0;
        }

        public void OpenWorkshopExecuted()
        {
            System.Diagnostics.Process.Start(string.Format("http://steamcommunity.com/sharedfiles/filedetails/?id={0}", this.SelectedWorld.WorkshopId.Value), null);
        }

        public bool ZoomThumbnailCanExecute()
        {
            return this.SelectedWorld != null && SelectedWorld.ThumbnailImageFilename != null;
        }

        public void ZoomThumbnailExecuted()
        {
            this.ZoomThumbnail = !this.ZoomThumbnail;
        }

        #endregion
    }
}
