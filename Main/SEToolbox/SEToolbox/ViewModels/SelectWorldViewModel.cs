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

        private readonly IDialogService dialogService;
        private SelectWorldModel dataModel;
        private bool? closeResult;

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
            this.dialogService = dialogService;
            this.dataModel = dataModel;
            this.dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region Properties

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get
            {
                return this.closeResult;
            }

            set
            {
                this.closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
            }
        }

        public SaveResource SelectedWorld
        {
            get
            {
                return this.dataModel.SelectedWorld;
            }
            set
            {
                if (value != this.dataModel.SelectedWorld)
                {
                    this.dataModel.SelectedWorld = value;
                }
            }
        }

        public ObservableCollection<SaveResource> Worlds
        {
            get
            {
                return this.dataModel.Worlds;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this.dataModel.IsBusy;
            }

            set
            {
                this.dataModel.IsBusy = value;
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
            var results = this.dataModel.RepairSandBox();
            this.IsBusy = false;
            var result = dialogService.ShowMessageBox(this, results, "Repair results", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.None);
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

        #endregion
    }
}
