namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using SEToolbox.Models;
    using SEToolbox.Services;

    public class ImportImageViewModel : BaseViewModel
    {
        #region Fields

        private ImportImageModel dataModel;
        private bool? closeResult;

        #endregion

        #region Constructors

        public ImportImageViewModel(BaseViewModel parentViewModel, ImportImageModel dataModel)
            : base(parentViewModel)
        {
            this.dataModel = dataModel;
        }

        #endregion

        #region Properties

        //public SaveResource SelectedWorld
        //{
        //    get
        //    {
        //        return this.dataModel.SelectedWorld;
        //    }
        //    set
        //    {
        //        if (value != this.dataModel.SelectedWorld)
                
        //        {
        //            this.dataModel.SelectedWorld = value;
        //            this.RaisePropertyChanged(() => SelectedWorld);
        //        }
        //    }
        //}

        //public ObservableCollection<SaveResource> Worlds
        //{
        //    get
        //    {
        //        return this.dataModel.Worlds;
        //    }
        //}

        public ICommand CreateCommand
        {
            get
            {
                return new DelegateCommand(new Action(CreateExecuted), new Func<bool>(CreateCanExecute));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(new Action(CancelExecuted), new Func<bool>(CancelCanExecute));
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

        #endregion

        #region methods

        public bool CreateCanExecute()
        {
            return false;
            //return this.SelectedWorld != null && this.SelectedWorld.IsValid;
        }

        public void CreateExecuted()
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
