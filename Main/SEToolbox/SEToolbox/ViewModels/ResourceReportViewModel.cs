namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Windows.Input;

    public class ResourceReportViewModel : BaseViewModel
    {
        #region Fields

        private readonly ResourceReportModel _dataModel;
        private bool? _closeResult;

        #endregion

        #region ctor

        public ResourceReportViewModel(BaseViewModel parentViewModel, ResourceReportModel dataModel)
            : base(parentViewModel)
        {

            this._dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

        public ICommand GenerateCommand
        {
            get
            {
                return new DelegateCommand(new Action(GenerateExecuted), new Func<bool>(GenerateCanExecute));
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                return new DelegateCommand(new Action(CopyExecuted), new Func<bool>(CopyCanExecute));
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new DelegateCommand(new Action(CloseExecuted), new Func<bool>(CloseCanExecute));
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

        public string ReportText
        {
            get
            {
                return this._dataModel.ReportText;
            }

            set
            {
                this._dataModel.ReportText = value;
            }
        }

        public bool ShowProgress
        {
            get
            {
                return this._dataModel.ShowProgress;
            }

            set
            {
                this._dataModel.ShowProgress = value;
            }
        }

        public double Progress
        {
            get
            {
                return this._dataModel.Progress;
            }

            set
            {
                this._dataModel.Progress = value;
            }
        }

        public double MaximumProgress
        {
            get
            {
                return this._dataModel.MaximumProgress;
            }

            set
            {
                this._dataModel.MaximumProgress = value;
            }
        }

        #endregion

        #region command methods

        public bool GenerateCanExecute()
        {
            return true;
        }

        public void GenerateExecuted()
        {
            this.IsBusy = true;
            this._dataModel.GenerateReport();
            this.IsBusy = false;
        }

        public bool CopyCanExecute()
        {
            return false;
        }

        public void CopyExecuted()
        {
            // TODO:
        }

        public bool CloseCanExecute()
        {
            return true;
        }

        public void CloseExecuted()
        {
            this.CloseResult = false;
        }

        #endregion
    }
}
