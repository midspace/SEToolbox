namespace SEToolbox.ViewModels
{
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

            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

        public ICommand OkayCommand
        {
            get { return new DelegateCommand(OkayExecuted, OkayCanExecute); }
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

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

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

        public ObservableCollection<ComponentItemModel> CubeList
        {
            get
            {
                return _dataModel.CubeList;
            }
        }

        public ComponentItemModel CubeItem
        {
            get
            {
                return _dataModel.CubeItem;
            }

            set
            {
                _dataModel.CubeItem = value;
            }
        }

        #endregion

        #region methods

        #region commands

        public bool OkayCanExecute()
        {
            return CubeItem != null;
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

        #endregion
    }
}
