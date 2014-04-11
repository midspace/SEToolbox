namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Views;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;

    public class FindApplicationViewModel : BaseViewModel
    {
        #region Fields

        private readonly FindApplicationModel _dataModel;
        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly Func<ISaveFileDialog> _saveFileDialogFactory;
        private bool? _closeResult;


        // If true, when adding new models to the collection, the new models will be highlighted as selected in the UI.
        private bool _selectNewStructure;

        #endregion

        #region event handlers

        public event EventHandler CloseRequested;

        #endregion

        #region Constructors

        public FindApplicationViewModel(FindApplicationModel dataModel)
            : this(dataModel, ServiceLocator.Resolve<IDialogService>(), () => ServiceLocator.Resolve<IOpenFileDialog>(), () => ServiceLocator.Resolve<ISaveFileDialog>())
        {
        }

        public FindApplicationViewModel(FindApplicationModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory, Func<ISaveFileDialog> saveFileDialogFactory)
            : base(null)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);
            Contract.Requires(saveFileDialogFactory != null);

            this._dialogService = dialogService;
            this._openFileDialogFactory = openFileDialogFactory;
            this._saveFileDialogFactory = saveFileDialogFactory;
            this._dataModel = dataModel;

            // Will bubble property change events from the Model to the ViewModel.
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Command Properties

        public ICommand ClosingCommand
        {
            get
            {
                return new DelegateCommand<CancelEventArgs>(new Action<CancelEventArgs>(ClosingExecuted), new Func<CancelEventArgs, bool>(ClosingCanExecute));
            }
        }

        public ICommand OpenComponentListCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenComponentListExecuted), new Func<bool>(OpenComponentListCanExecute));
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                return new DelegateCommand(new Action(AboutExecuted), new Func<bool>(AboutCanExecute));
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

        /// <summary>
        /// Gets or sets a value indicating whether the View is available.  This is based on the IsInError and IsBusy properties
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this._dataModel.IsActive;
            }

            set
            {
                this._dataModel.IsActive = value;
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
                return this._dataModel.IsBusy;
            }

            set
            {
                this._dataModel.IsBusy = value;
            }
        }

        public bool IsDebugger
        {
            get
            {
                return Debugger.IsAttached;
            }
        }

        public bool IsModified
        {
            get
            {
                return this._dataModel.IsModified;
            }

            set
            {
                this._dataModel.IsModified = value;
            }
        }

        public bool IsBaseSaveChanged
        {
            get
            {
                return this._dataModel.IsBaseSaveChanged;
            }

            set
            {
                this._dataModel.IsBaseSaveChanged = value;
            }
        }

        #endregion

        #region Command Methods

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
      
        public bool OpenComponentListCanExecute()
        {
            return true;
        }

        public void OpenComponentListExecuted()
        {
            var model = new ComponentListModel();
            model.Load();
            var loadVm = new ComponentListViewModel(this, model);
            var result = this._dialogService.ShowDialog<WindowComponentList>(this, loadVm);
        }

        public bool AboutCanExecute()
        {
            return true;
        }

        public void AboutExecuted()
        {
            var loadVm = new AboutViewModel(this);
            var result = _dialogService.ShowDialog<WindowAbout>(this, loadVm);
        }

        #endregion

        #region methods

        #endregion
      
    }
}