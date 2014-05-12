namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using SEToolbox.Models;
    using SEToolbox.Services;

    public class SelectCubeViewModel : BaseViewModel
    {
        #region Fields

        private readonly SelectCubeModel _dataModel;
        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region ctor

        public SelectCubeViewModel(BaseViewModel parentViewModel, SelectCubeModel dataModel)
            : base(parentViewModel)
        {

            this._dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

        public ICommand OkayCommand
        {
            get
            {
                return new DelegateCommand(new Action(OkayExecuted), new Func<bool>(OkayCanExecute));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(new Action(CancelExecuted), new Func<bool>(CancelCanExecute));
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

        public ObservableCollection<ComponentItemModel> CubeList
        {
            get
            {
                return this._dataModel.CubeList;
            }
        }

        public ComponentItemModel CubeItem
        {
            get
            {
                return this._dataModel.CubeItem;
            }

            set
            {
                this._dataModel.CubeItem = value;
            }
        }

        #endregion

        #region methods

        #region commands

        public bool OkayCanExecute()
        {
            return this.CubeItem != null;
        }

        public void OkayExecuted()
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

        #endregion

        #endregion
    }
}
