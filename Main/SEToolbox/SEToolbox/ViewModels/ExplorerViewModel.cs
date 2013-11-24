namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Views;

    public class ExplorerViewModel : BaseViewModel
    {
        #region Fields

        private ExplorerModel dataModel;
        private readonly IDialogService dialogService;
        private bool? closeResult;

        #endregion

        #region Constructors

        public ExplorerViewModel(ExplorerModel dataModel)
            : this(dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public ExplorerViewModel(ExplorerModel dataModel, IDialogService dialogService)
            : base(null)
        {
            Contract.Requires(dialogService != null);

            this.dialogService = dialogService;

            this.dataModel = dataModel;

            this.dataModel.PropertyChanged += this.OnPropertyChanged;
        }

        #endregion

        #region Properties

        public ICommand ClosingCommand
        {
            get
            {
                return new DelegateCommand<CancelEventArgs>(new Action<CancelEventArgs>(ClosingExecuted), new Func<CancelEventArgs, bool>(ClosingCanExecute));
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenExecuted), new Func<bool>(OpenCanExecute));
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new DelegateCommand(new Action(SaveExecuted), new Func<bool>(SaveCanExecute));
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new DelegateCommand(new Action(ClearExecuted), new Func<bool>(ClearCanExecute));
            }
        }

        public ICommand ReloadCommand
        {
            get
            {
                return new DelegateCommand(new Action(ReloadExecuted), new Func<bool>(ReloadCanExecute));
            }
        }

        public ICommand ImportCommand
        {
            get
            {
                return new DelegateCommand(new Action(ImportExecuted), new Func<bool>(ImportCanExecute));
            }
        }

        public ICommand ImportImageCommand
        {
            get
            {
                return new DelegateCommand(new Action(ImportImageExecuted), new Func<bool>(ImportImageCanExecute));
            }
        }

        public ICommand ImportModelCommand
        {
            get
            {
                return new DelegateCommand(new Action(ImportModelExecuted), new Func<bool>(ImportModelCanExecute));
            }
        }

        public ICommand ExportExcelCommand
        {
            get
            {
                return new DelegateCommand(new Action(ExportExcelExecuted), new Func<bool>(ExportExcelCanExecute));
            }
        }

        public ICommand TestCommand
        {
            get
            {
                return new DelegateCommand(new Action(TestExecuted), new Func<bool>(TestCanExecute));
            }
        }

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

        //public string TfsUri
        //{
        //    get
        //    {
        //        return this.dataModel.TfsUri;
        //    }

        //    set
        //    {
        //        this.dataModel.TfsUri = value;
        //    }
        //}

        //public int Columns
        //{
        //    get
        //    {
        //        return this.dataModel.Columns;
        //    }

        //    set
        //    {
        //        this.dataModel.Columns = value;
        //    }
        //}

        //public int Screen
        //{
        //    get
        //    {
        //        return this.dataModel.Screen;
        //    }

        //    set
        //    {
        //        this.dataModel.Screen = value;
        //    }
        //}

        //public int UpdateInterval
        //{
        //    get
        //    {
        //        return this.dataModel.UpdateInterval;
        //    }

        //    set
        //    {
        //        this.dataModel.UpdateInterval = value;
        //    }
        //}

        //public int StaleThreshold
        //{
        //    get
        //    {
        //        return this.dataModel.StaleThreshold;
        //    }

        //    set
        //    {
        //        this.dataModel.StaleThreshold = value;
        //    }
        //}

        public ObservableCollection<IStructureBase> Structures
        {
            get
            {
                return this.dataModel.Structures;
            }
        }

        public IStructureBase SelectedStructure
        {
            get
            {
                return this.dataModel.SelectedStructure;
            }
            set
            {
                this.dataModel.SelectedStructure = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is available.  This is based on the IsInError and IsBusy properties
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this.dataModel.IsActive;
            }

            set
            {
                this.dataModel.IsActive = value;
                //if (this.Dispatcher.CheckAccess())
                ////{
                //if (this.isActive != value)
                //{
                //    this.isActive = value;
                //    this.RaisePropertyChanged(() => IsActive);
                //}
                //}
                //else
                //{
                //    this.Dispatcher.Invoke(DispatcherPriority.Input, (Action)delegate { this.IsActive = value; });
                //}
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
                //if (this.Dispatcher.CheckAccess())
                //{
                //if (this.isBusy != value)
                //{
                //    this.isBusy = value;
                //    this.RaisePropertyChanged(() => IsBusy);
                //    this.SetActiveStatus();
                //}
                //}
                //else
                //{
                //    this.Dispatcher.Invoke(DispatcherPriority.Input, (Action)delegate { this.IsBusy = value; });
                //}
            }
        }

        public bool IsDebugger
        {
            get
            {
                return Debugger.IsAttached;
            }
        }

        #endregion

        #region Methods

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        public bool ClosingCanExecute(CancelEventArgs e)
        {
            return true;
        }

        public void ClosingExecuted(CancelEventArgs e)
        {
            // TODO: dialog on outstanding changes.


            //if (this.CheckCloseWindow() == false)
            //{
            //e.Cancel = true;
            //this.CloseResult = null;
            //}
            //else
            {
                if (this.CloseRequested != null)
                {
                    this.CloseRequested(this, EventArgs.Empty);
                }
            }
        }

        public bool OpenCanExecute()
        {
            return true;
        }

        public void OpenExecuted()
        {
            SelectWorldModel model = new SelectWorldModel();
            model.Load(this.dataModel.BaseSavePath);
            SelectWorldViewModel loadVm = new SelectWorldViewModel(this, model);

            var result = dialogService.ShowDialog<WindowLoad>(this, loadVm);
            if (result == true)
            {
                this.dataModel.ActiveWorld = model.SelectedWorld;
                this.dataModel.LoadSandBox();

                // TODO: work out whether to wrap the model with a viewmodel here, or elsewhere.
            }
        }

        public bool SaveCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void SaveExecuted()
        {
            if (this.dataModel != null)
            {
                this.dataModel.SaveSandBox();
            }
        }

        public bool ClearCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ClearExecuted()
        {
            // TODO: clear
        }

        public bool ReloadCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ReloadExecuted()
        {
            // TODO: check is save directory is still valid.

            // TODO: reload Checkpoint file.

            // Load Sector file.
            this.dataModel.LoadSandBox();
        }

        public bool ImportCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ImportExecuted()
        {
            // do nothing. Only required for base menu.
        }

        public bool ImportImageCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ImportImageExecuted()
        {
            ImportImageModel model = new ImportImageModel();
            model.Load(/*this.dataModel.BaseSavePath*/);
            ImportImageViewModel loadVm = new ImportImageViewModel(this, model);

            var result = dialogService.ShowDialog<WindowImportImage>(this, loadVm);
            if (result == true)
            {
                // TODO:
                //this.dataModel.ActiveWorld = model.SelectedWorld;
                //this.dataModel.LoadSandBox();
            }
        }

        public bool ImportModelCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ImportModelExecuted()
        {
            Import3dModelModel model = new Import3dModelModel();
            model.Load(/*this.dataModel.BaseSavePath*/);
            Import3dModelViewModel loadVm = new Import3dModelViewModel(this, model);

            var result = dialogService.ShowDialog<WindowImportModel>(this, loadVm);
            if (result == true)
            {
                // TODO:
                //this.dataModel.ActiveWorld = model.SelectedWorld;
                //this.dataModel.LoadSandBox();
            }
        }

        public bool ExportExcelCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ExportExcelExecuted()
        {
            // TODO:
        }

        public bool TestCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void TestExecuted()
        {
            // TODO:
        }

        #endregion

        public event EventHandler CloseRequested;

    }
}
