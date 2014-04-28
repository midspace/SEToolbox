namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Res = SEToolbox.Properties.Resources;

    public class ComponentListViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly Func<ISaveFileDialog> _saveFileDialogFactory;
        private readonly ComponentListModel _dataModel;
        private bool? _closeResult;

        #endregion

        #region Constructors

        public ComponentListViewModel(BaseViewModel parentViewModel, ComponentListModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<ISaveFileDialog>)
        {
        }

        public ComponentListViewModel(BaseViewModel parentViewModel, ComponentListModel dataModel, IDialogService dialogService, Func<ISaveFileDialog> saveFileDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(saveFileDialogFactory != null);

            this._dialogService = dialogService;
            this._saveFileDialogFactory = saveFileDialogFactory;
            this._dataModel = dataModel;
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command Properties

        public ICommand ExportReportCommand
        {
            get
            {
                return new DelegateCommand(new Action(ExportReportExecuted), new Func<bool>(ExportReportCanExecute));
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
                return this._dataModel.IsBusy;
            }

            set
            {
                this._dataModel.IsBusy = value;
            }
        }

        public ObservableCollection<ComonentItemModel> CubeAssets
        {
            get
            {
                return this._dataModel.CubeAssets;
            }

            set
            {
                this._dataModel.CubeAssets = value;
            }
        }

        public ObservableCollection<ComonentItemModel> ComponentAssets
        {
            get
            {
                return this._dataModel.ComponentAssets;
            }

            set
            {
                this._dataModel.ComponentAssets = value;
            }
        }

        public ObservableCollection<ComonentItemModel> ItemAssets
        {
            get
            {
                return this._dataModel.ItemAssets;
            }

            set
            {
                this._dataModel.ItemAssets = value;
            }
        }

        public ObservableCollection<ComonentItemModel> MaterialAssets
        {
            get
            {
                return this._dataModel.MaterialAssets;
            }

            set
            {
                this._dataModel.MaterialAssets = value;
            }
        }

        public ComonentItemModel SelectedCubeAsset
        {
            get
            {
                return this._dataModel.SelectedCubeAsset;
            }

            set
            {
                this._dataModel.SelectedCubeAsset = value;
            }
        }

        #endregion

        #region methods

        public bool ExportReportCanExecute()
        {
            return true;
        }

        public void ExportReportExecuted()
        {
            var saveFileDialog = this._saveFileDialogFactory();
            saveFileDialog.Filter = Res.DialogExportReportFilter;
            saveFileDialog.Title = string.Format(Res.DialogExportReportTitle, "Component Item Report");
            saveFileDialog.FileName = "Space Engineers Component Item Report";
            saveFileDialog.OverwritePrompt = true;

            // Open the dialog
            var result = this._dialogService.ShowSaveFileDialog(this, saveFileDialog);

            if (result == DialogResult.OK)
            {
                this._dataModel.GenerateHtmlReport(saveFileDialog.FileName);
            }
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
