namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;

    public class GroupMoveViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService dialogService;
        private GroupMoveModel dataModel;
        private bool? closeResult;

        #endregion

        #region Constructors

        public GroupMoveViewModel(BaseViewModel parentViewModel, GroupMoveModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public GroupMoveViewModel(BaseViewModel parentViewModel, GroupMoveModel dataModel, IDialogService dialogService)
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

        #region command Properties

        public ICommand ApplyCommand
        {
            get
            {
                return new DelegateCommand(new Action(ApplyExecuted), new Func<bool>(ApplyCanExecute));
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
                return this.closeResult;
            }

            set
            {
                this.closeResult = value;
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
                return this.dataModel.IsBusy;
            }

            set
            {
                this.dataModel.IsBusy = value;
            }
        }

        public double GlobalOffsetPositionX
        {
            get
            {
                return this.dataModel.GlobalOffsetPositionX;
            }

            set
            {
                this.dataModel.GlobalOffsetPositionX = value;
                this.dataModel.CalcOffsetDistances();
            }
        }

        public double GlobalOffsetPositionY
        {
            get
            {
                return this.dataModel.GlobalOffsetPositionY;
            }

            set
            {
                this.dataModel.GlobalOffsetPositionY = value;
                this.dataModel.CalcOffsetDistances();
            }
        }

        public double GlobalOffsetPositionZ
        {
            get
            {
                return this.dataModel.GlobalOffsetPositionZ;
            }

            set
            {
                this.dataModel.GlobalOffsetPositionZ = value;
                this.dataModel.CalcOffsetDistances();
            }
        }

        public bool IsGlobalOffsetPosition
        {
            get
            {
                return this.dataModel.IsGlobalOffsetPosition;
            }

            set
            {
                this.dataModel.IsGlobalOffsetPosition = value;
                this.dataModel.CalcOffsetDistances();
            }
        }

        public double SinglePositionX
        {
            get
            {
                return this.dataModel.SinglePositionX;
            }

            set
            {
                this.dataModel.SinglePositionX = value;
                this.dataModel.CalcOffsetDistances();
            }
        }

        public double SinglePositionY
        {
            get
            {
                return this.dataModel.SinglePositionY;
            }

            set
            {
                this.dataModel.SinglePositionY = value;
                this.dataModel.CalcOffsetDistances();
            }
        }

        public double SinglePositionZ
        {
            get
            {
                return this.dataModel.SinglePositionZ;
            }

            set
            {
                this.dataModel.SinglePositionZ = value;
                this.dataModel.CalcOffsetDistances();
            }
        }

        public bool IsSinglePosition
        {
            get
            {
                return this.dataModel.IsSinglePosition;
            }

            set
            {
                this.dataModel.IsSinglePosition = value;
                this.dataModel.CalcOffsetDistances();
            }
        }

        public ObservableCollection<GroupMoveItemModel> Selections
        {
            get
            {
                return this.dataModel.Selections;
            }

            set
            {
                this.dataModel.Selections = value;
            }
        }

        #endregion

        #region methods

        public bool ApplyCanExecute()
        {
            return this.IsSinglePosition ||
                (this.IsGlobalOffsetPosition && (this.GlobalOffsetPositionX != 0 || this.GlobalOffsetPositionY != 0 || this.GlobalOffsetPositionZ != 0));
        }

        public void ApplyExecuted()
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
    }
}
