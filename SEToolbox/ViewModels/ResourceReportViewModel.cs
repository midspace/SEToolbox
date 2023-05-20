namespace SEToolbox.ViewModels
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;

    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using Support;
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

            _dialogService = dialogService;
            _saveFileDialogFactory = saveFileDialogFactory;
            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

        public ICommand GenerateCommand
        {
            get { return new DelegateCommand(GenerateExecuted, GenerateCanExecute); }
        }

        public ICommand ExportCommand
        {
            get { return new DelegateCommand(new Func<bool>(ExportCanExecute)); }
        }

        public ICommand CopyCommand
        {
            get { return new DelegateCommand(CopyExecuted, CopyCanExecute); }
        }

        public ICommand ExportTextCommand
        {
            get { return new DelegateCommand(ExportTextExecuted, ExportTextCanExecute); }
        }

        public ICommand ExportHtmlCommand
        {
            get { return new DelegateCommand(ExportHtmlExecuted, ExportHtmlCanExecute); }
        }

        public ICommand ExportXmlCommand
        {
            get { return new DelegateCommand(ExportXmlExecuted, ExportXmlCanExecute); }
        }

        public ICommand CloseCommand
        {
            get { return new DelegateCommand(CloseExecuted, CloseCanExecute); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get { return _closeResult; }

            set
            {
                _closeResult = value;
                OnPropertyChanged(nameof(CloseResult));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is available.  This is based on the IsInError and IsBusy properties
        /// </summary>
        public bool IsActive
        {
            get { return _dataModel.IsActive; }
            set { _dataModel.IsActive = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get { return _dataModel.IsBusy; }
            set { _dataModel.IsBusy = value; }
        }

        public bool IsReportReady
        {
            get { return _dataModel.IsReportReady; }
            set { _dataModel.IsReportReady = value; }
        }

        public string ReportHtml
        {
            get { return _dataModel.ReportHtml; }
            set { _dataModel.ReportHtml = value; }
        }

        public bool ShowProgress
        {
            get { return _dataModel.ShowProgress; }
            set { _dataModel.ShowProgress = value; }
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

        #endregion

        #region command methods

        public bool GenerateCanExecute()
        {
            return true;
        }

        public void GenerateExecuted()
        {
            IsBusy = true;
            _dataModel.GenerateReport();
            ReportHtml = _dataModel.CreateHtmlReport();
            IsBusy = false;
        }

        public bool ExportCanExecute()
        {
            return IsReportReady;
        }

        public bool CopyCanExecute()
        {
            return IsReportReady;
        }

        public void CopyExecuted()
        {
            try
            {
                Clipboard.Clear();
                Clipboard.SetText(_dataModel.CreateTextReport());
            }
            catch
            {
                // Ignore exception which may be generated by a Remote desktop session where Clipboard access has not been granted.
            }
        }

        public bool ExportTextCanExecute()
        {
            return IsReportReady;
        }

        public void ExportTextExecuted()
        {
            var saveFileDialog = _saveFileDialogFactory();
            saveFileDialog.Filter = AppConstants.TextFileFilter;
            saveFileDialog.Title = string.Format(Res.DialogExportTextFileTitle, "Resource Report");
            saveFileDialog.FileName = string.Format("Resource Report - {0}.txt", _dataModel.SaveName);
            saveFileDialog.OverwritePrompt = true;

            if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, _dataModel.CreateTextReport());
            }
        }

        public bool ExportHtmlCanExecute()
        {
            return IsReportReady;
        }

        public void ExportHtmlExecuted()
        {
            var saveFileDialog = _saveFileDialogFactory();
            saveFileDialog.Filter = AppConstants.HtmlFilter;
            saveFileDialog.Title = string.Format(Res.DialogExportHtmlFileTitle, "Resource Report");
            saveFileDialog.FileName = string.Format("Resource Report - {0}.html", _dataModel.SaveName);
            saveFileDialog.OverwritePrompt = true;

            if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, _dataModel.CreateHtmlReport());
            }
        }

        public bool ExportXmlCanExecute()
        {
            return IsReportReady;
        }

        public void ExportXmlExecuted()
        {
            var saveFileDialog = _saveFileDialogFactory();
            saveFileDialog.Filter = AppConstants.XmlFileFilter;
            saveFileDialog.Title = string.Format(Res.DialogExportXmlFileTitle, "Resource Report");
            saveFileDialog.FileName = string.Format("Resource Report - {0}.xml", _dataModel.SaveName);
            saveFileDialog.OverwritePrompt = true;

            if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, _dataModel.CreateXmlReport());
            }
        }

        public bool CloseCanExecute()
        {
            return true;
        }

        public void CloseExecuted()
        {
            CloseResult = false;
        }

        #endregion
    }
}
