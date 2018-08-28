namespace SEToolbox.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    using SEToolbox.Models;
    using SEToolbox.Services;

    public class ProgressCancelViewModel : BaseViewModel
    {
        #region Fields

        private readonly ProgressCancelModel _dataModel;
        private bool? _closeResult;

        #endregion

        #region event handlers

        public event EventHandler CloseRequested;

        #endregion

        #region Constructors

        public ProgressCancelViewModel(BaseViewModel parentViewModel, ProgressCancelModel dataModel)
            : base(parentViewModel)
        {
            _dataModel = dataModel;

            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Command Properties

        public ICommand ClosingCommand
        {
            get { return new DelegateCommand<CancelEventArgs>(ClosingExecuted, ClosingCanExecute); }
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
            get
            {
                return _closeResult;
            }

            set
            {
                _closeResult = value;
                OnPropertyChanged(nameof(CloseResult));
            }
        }

        public string Title
        {
            get { return _dataModel.Title; }
            set { _dataModel.Title = value; }
        }
        public string SubTitle
        {
            get { return _dataModel.SubTitle; }
            set { _dataModel.SubTitle = value; }
        }

        public string DialogText
        {
            get { return _dataModel.DialogText; }
            set { _dataModel.DialogText = value; }
        }

        public double Progress
        {
            get { return _dataModel.Progress; }
            set { _dataModel.Progress = value; }
        }

        public double MaximumProgress
        {
            get { return _dataModel.MaximumProgress; }
            set { _dataModel.MaximumProgress = value; }
        }

        public TimeSpan? EstimatedTimeLeft
        {
            get { return _dataModel.EstimatedTimeLeft; }
            set { _dataModel.EstimatedTimeLeft = value; }
        }

        #endregion

        #region Command Methods

        public bool ClosingCanExecute(CancelEventArgs e)
        {
            return true;
        }

        public void ClosingExecuted(CancelEventArgs e)
        {
            if (CloseResult == null)
            {
                if (CloseRequested != null)
                {
                    CloseRequested(this, EventArgs.Empty);
                }

                CloseResult = false;
            }

            _dataModel.ClearProgress();
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            if (CloseRequested != null)
            {
                CloseRequested(this, EventArgs.Empty);
            }

            CloseResult = false;
        }

        #endregion

        public void Close()
        {
            CloseResult = true;
        }
    }
}