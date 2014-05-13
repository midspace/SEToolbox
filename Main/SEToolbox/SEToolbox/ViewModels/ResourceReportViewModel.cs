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
        private bool _isBusy;

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
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }

            set
            {
                if (value != this._isBusy)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    if (this._isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        // TODO:
        //IsActive
        //ReportText
        //Progress

        #endregion

        #region command methods

        public bool GenerateCanExecute()
        {
            return false;
        }

        public void GenerateExecuted()
        {
            // TODO:
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
