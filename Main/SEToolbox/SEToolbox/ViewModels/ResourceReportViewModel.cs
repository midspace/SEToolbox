namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using Res = SEToolbox.Properties.Resources;

    public class ResourceReportViewModel : BaseViewModel
    {
        #region Fields

        private readonly ResourceReportModel _dataModel;
        private bool? _closeResult;
        private readonly IDialogService _dialogService;
        private readonly Func<ISaveFileDialog> _saveFileDialogFactory;

        #endregion

        #region ctor

        public ResourceReportViewModel(BaseViewModel parentViewModel, ResourceReportModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<ISaveFileDialog>)
        {
        }

        public ResourceReportViewModel(BaseViewModel parentViewModel, ResourceReportModel dataModel, IDialogService dialogService, Func<ISaveFileDialog> saveFileDialogFactory)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(saveFileDialogFactory != null);

            this._dialogService = dialogService;
            this._saveFileDialogFactory = saveFileDialogFactory;
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

        public ICommand ExportCommand
        {
            get
            {
                return new DelegateCommand(new Func<bool>(ExportCanExecute));
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                return new DelegateCommand(new Action(CopyExecuted), new Func<bool>(CopyCanExecute));
            }
        }

        public ICommand ExportTextCommand
        {
            get
            {
                return new DelegateCommand(new Action(ExportTextExecuted), new Func<bool>(ExportTextCanExecute));
            }
        }

        public ICommand ExportHtmlCommand
        {
            get
            {
                return new DelegateCommand(new Action(ExportHtmlExecuted), new Func<bool>(ExportHtmlCanExecute));
            }
        }

        public ICommand ExportXmlCommand
        {
            get
            {
                return new DelegateCommand(new Action(ExportXmlExecuted), new Func<bool>(ExportXmlCanExecute));
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

        public bool IsReportReady
        {
            get
            {
                return this._dataModel.IsReportReady;
            }

            set
            {
                this._dataModel.IsReportReady = value;
            }
        }

        public string ReportHtml
        {
            get
            {
                return this._dataModel.ReportHtml;
            }

            set
            {
                this._dataModel.ReportHtml = value;
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
            this.ReportHtml = this._dataModel.CreateHtmlReport();
            this.IsBusy = false;
        }

        public bool ExportCanExecute()
        {
            return this.IsReportReady;
        }

        public bool CopyCanExecute()
        {
            return this.IsReportReady;
        }

        public void CopyExecuted()
        {
            Clipboard.SetText(this._dataModel.CreateTextReport());
        }

        public bool ExportTextCanExecute()
        {
            return this.IsReportReady;
        }

        public void ExportTextExecuted()
        {
            var saveFileDialog = this._saveFileDialogFactory();
            saveFileDialog.Filter = Res.DialogExportTextFileFilter;
            saveFileDialog.Title = string.Format(Res.DialogExportTextFileTitle, "Resource Report");
            saveFileDialog.FileName = string.Format("Resource Report - {0}.txt", this._dataModel.SaveName);
            saveFileDialog.OverwritePrompt = true;

            if (this._dialogService.ShowSaveFileDialog(this, saveFileDialog) == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, this._dataModel.CreateTextReport());
            }
        }

        public bool ExportHtmlCanExecute()
        {
            return this.IsReportReady;
        }

        public void ExportHtmlExecuted()
        {
            var saveFileDialog = this._saveFileDialogFactory();
            saveFileDialog.Filter = Res.DialogExportHtmlFileFilter;
            saveFileDialog.Title = string.Format(Res.DialogExportHtmlFileTitle, "Resource Report");
            saveFileDialog.FileName = string.Format("Resource Report - {0}.html", this._dataModel.SaveName);
            saveFileDialog.OverwritePrompt = true;

            if (this._dialogService.ShowSaveFileDialog(this, saveFileDialog) == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, this._dataModel.CreateHtmlReport());
            }
        }

        public bool ExportXmlCanExecute()
        {
            return this.IsReportReady;
        }

        public void ExportXmlExecuted()
        {
            var saveFileDialog = this._saveFileDialogFactory();
            saveFileDialog.Filter = Res.DialogExportXmlFileFilter;
            saveFileDialog.Title = string.Format(Res.DialogExportXmlFileTitle, "Resource Report");
            saveFileDialog.FileName = string.Format("Resource Report - {0}.xml", this._dataModel.SaveName);
            saveFileDialog.OverwritePrompt = true;

            if (this._dialogService.ShowSaveFileDialog(this, saveFileDialog) == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, this._dataModel.CreateXmlReport());
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
