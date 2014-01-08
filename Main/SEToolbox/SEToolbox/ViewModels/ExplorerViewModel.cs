﻿namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Input;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Properties;
    using SEToolbox.Services;
    using SEToolbox.Views;

    public class ExplorerViewModel : BaseViewModel, IDropable
    {
        #region Fields

        private readonly ExplorerModel _dataModel;
        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly Func<ISaveFileDialog> _saveFileDialogFactory;
        private bool? _closeResult;

        private IStructureViewBase _selectedStructure;
        private ObservableCollection<IStructureViewBase> _selections;

        private ObservableCollection<IStructureViewBase> _structures;
        private bool _selectNewStructure;

        #endregion

        #region event handlers

        public event EventHandler CloseRequested;

        #endregion

        #region Constructors

        public ExplorerViewModel(ExplorerModel dataModel)
            : this(dataModel, ServiceLocator.Resolve<IDialogService>(), () => ServiceLocator.Resolve<IOpenFileDialog>(), () => ServiceLocator.Resolve<ISaveFileDialog>())
        {
        }

        public ExplorerViewModel(ExplorerModel dataModel, IDialogService dialogService, Func<IOpenFileDialog> openFileDialogFactory, Func<ISaveFileDialog> saveFileDialogFactory)
            : base(null)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(openFileDialogFactory != null);
            Contract.Requires(saveFileDialogFactory != null);

            this._dialogService = dialogService;
            this._openFileDialogFactory = openFileDialogFactory;
            this._saveFileDialogFactory = saveFileDialogFactory;
            this._dataModel = dataModel;

            this.Selections = new ObservableCollection<IStructureViewBase>();
            this.Structures = new ObservableCollection<IStructureViewBase>();
            foreach (var item in this._dataModel.Structures)
            {
                this.AddViewModel(item);
            }

            this._dataModel.Structures.CollectionChanged += Structures_CollectionChanged;
            // Will bubble property change events from the Model to the ViewModel.
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Command Properties

        public ICommand ClosingCommand
        {
            get
            {
                return new DelegateCommand<CancelEventArgs>(new Action<CancelEventArgs>(ClosingExecuted), new Func<CancelEventArgs, bool>(ClosingCanExecute));
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenExecuted), new Func<bool>(OpenCanExecute));
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new DelegateCommand(new Action(SaveExecuted), new Func<bool>(SaveCanExecute));
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new DelegateCommand(new Action(ClearExecuted), new Func<bool>(ClearCanExecute));
            }
        }

        public ICommand ReloadCommand
        {
            get
            {
                return new DelegateCommand(new Action(ReloadExecuted), new Func<bool>(ReloadCanExecute));
            }
        }

        public ICommand ImportCommand
        {
            get
            {
                return new DelegateCommand(new Action(ImportExecuted), new Func<bool>(ImportCanExecute));
            }
        }

        public ICommand ImportVoxelCommand
        {
            get
            {
                return new DelegateCommand(new Action(ImportVoxelExecuted), new Func<bool>(ImportVoxelCanExecute));
            }
        }

        public ICommand ImportImageCommand
        {
            get
            {
                return new DelegateCommand(new Action(ImportImageExecuted), new Func<bool>(ImportImageCanExecute));
            }
        }

        public ICommand ImportModelCommand
        {
            get
            {
                return new DelegateCommand(new Action(ImportModelExecuted), new Func<bool>(ImportModelCanExecute));
            }
        }

        public ICommand ImportSandboxObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(ImportSandboxObjectExecuted), new Func<bool>(ImportSandboxObjectCanExecute));
            }
        }

        public ICommand WorldCommand
        {
            get
            {
                return new DelegateCommand(new Action(WorldExecuted), new Func<bool>(WorldCanExecute));
            }
        }

        public ICommand OpenFolderCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenFolderExecuted), new Func<bool>(OpenFolderCanExecute));
            }
        }

        public ICommand OpenWorkshopCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenWorkshopExecuted), new Func<bool>(OpenWorkshopCanExecute));
            }
        }

        public ICommand ExportSandboxObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(ExportSandboxObjectExecuted), new Func<bool>(ExportSandboxObjectCanExecute));
            }
        }

        public ICommand TestCommand
        {
            get
            {
                return new DelegateCommand(new Action(TestExecuted), new Func<bool>(TestCanExecute));
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                return new DelegateCommand(new Action(AboutExecuted), new Func<bool>(AboutCanExecute));
            }
        }

        public ICommand DeleteObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(DeleteObjectExecuted), new Func<bool>(DeleteObjectCanExecute));
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

        public ObservableCollection<IStructureViewBase> Structures
        {
            get
            {
                return this._structures;
            }

            private set
            {
                if (value != this._structures)
                {
                    this._structures = value;
                    this.RaisePropertyChanged(() => Structures);
                }
            }
        }

        public IStructureViewBase SelectedStructure
        {
            get
            {
                return this._selectedStructure;
            }

            set
            {
                if (value != this._selectedStructure)
                {
                    this._selectedStructure = value;
                    this.RaisePropertyChanged(() => SelectedStructure);
                }
            }
        }

        public ObservableCollection<IStructureViewBase> Selections
        {
            get
            {
                return this._selections;
            }

            set
            {
                if (value != this._selections)
                {
                    this._selections = value;
                    this.RaisePropertyChanged(() => Selections);
                }
            }
        }

        public SaveResource ActiveWorld
        {
            get
            {
                return this._dataModel.ActiveWorld;
            }
            set
            {
                this._dataModel.ActiveWorld = value;
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
                //if (this.Dispatcher.CheckAccess())
                ////{
                //if (this.isActive != value)
                //{
                //    this.isActive = value;
                //    this.RaisePropertyChanged(() => IsActive);
                //}
                //}
                //else
                //{
                //    this.Dispatcher.Invoke(DispatcherPriority.Input, (Action)delegate { this.IsActive = value; });
                //}
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

        public bool IsDebugger
        {
            get
            {
                return Debugger.IsAttached;
            }
        }

        public bool IsModified
        {
            get
            {
                return this._dataModel.IsModified;
            }

            set
            {
                this._dataModel.IsModified = value;
            }
        }

        public bool IsBaseSaveChanged
        {
            get
            {
                return this._dataModel.IsBaseSaveChanged;
            }

            set
            {
                this._dataModel.IsBaseSaveChanged = value;
            }
        }

        #endregion

        #region Methods

        public bool ClosingCanExecute(CancelEventArgs e)
        {
            return true;
        }

        public void ClosingExecuted(CancelEventArgs e)
        {
            // TODO: dialog on outstanding changes.


            //if (this.CheckCloseWindow() == false)
            //{
            //e.Cancel = true;
            //this.CloseResult = null;
            //}
            //else
            {
                if (this.CloseRequested != null)
                {
                    this.CloseRequested(this, EventArgs.Empty);
                }
            }
        }

        public bool OpenCanExecute()
        {
            return true;
        }

        public void OpenExecuted()
        {
            SelectWorldModel model = new SelectWorldModel();
            model.Load(this._dataModel.BaseSavePath);
            SelectWorldViewModel loadVm = new SelectWorldViewModel(this, model);

            var result = this._dialogService.ShowDialog<WindowLoad>(this, loadVm);
            if (result == true)
            {
                this._dataModel.ActiveWorld = model.SelectedWorld;
                this.ActiveWorld.LoadCheckpoint();
                this._dataModel.LoadSandBox();
            }
        }

        public bool SaveCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void SaveExecuted()
        {
            if (this._dataModel != null)
            {
                this._dataModel.SaveCheckPointAndSandBox();
            }
        }

        public bool ClearCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void ClearExecuted()
        {
            // TODO: clear loaded data.
        }

        public bool ReloadCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void ReloadExecuted()
        {
            // TODO: check is save directory is still valid.

            // Reload Checkpoint file.
            this.ActiveWorld.LoadCheckpoint();

            // Load Sector file.
            this._dataModel.LoadSandBox();
        }

        public bool ImportCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void ImportExecuted()
        {
            // do nothing. Only required for base menu.
        }

        public bool ImportVoxelCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void ImportVoxelExecuted()
        {
            var model = new ImportVoxelModel();
            model.Load(this._dataModel.ThePlayerCharacter.PositionAndOrientation.Value);
            var loadVm = new ImportVoxelViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportVoxel>(this, loadVm);
            if (result == true)
            {
                this.IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                this._selectNewStructure = true;
                var structure = this._dataModel.AddEntity(newEntity);
                ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                this._selectNewStructure = false;
                this.IsBusy = false;
            }
        }

        public bool ImportImageCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void ImportImageExecuted()
        {
            var model = new ImportImageModel();
            model.Load(this._dataModel.ThePlayerCharacter.PositionAndOrientation.Value);
            var loadVm = new ImportImageViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportImage>(this, loadVm);
            if (result == true)
            {
                this.IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                this._selectNewStructure = true;
                this._dataModel.CollisionCorrectEntity(newEntity);
                var structure = this._dataModel.AddEntity(newEntity);
                this._selectNewStructure = false;
                this.IsBusy = false;
            }
        }

        public bool ImportModelCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void ImportModelExecuted()
        {
            var model = new Import3dModelModel();
            model.Load(this._dataModel.ThePlayerCharacter.PositionAndOrientation.Value);
            var loadVm = new Import3dModelViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportModel>(this, loadVm);
            if (result == true)
            {
                this.IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                this._selectNewStructure = true;
                this._dataModel.CollisionCorrectEntity(newEntity);
                var structure = this._dataModel.AddEntity(newEntity);
                this._selectNewStructure = false;
                this.IsBusy = false;
            }
        }

        public bool ImportSandboxObjectCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void ImportSandboxObjectExecuted()
        {
            this.ImportSandboxObjectFromFile();
        }

        public bool WorldCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void WorldExecuted()
        {
            // do nothing. Only required for base menu.
        }

        public bool OpenFolderCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void OpenFolderExecuted()
        {
            System.Diagnostics.Process.Start("Explorer", this._dataModel.ActiveWorld.Savepath);
        }

        public bool OpenWorkshopCanExecute()
        {
            return this._dataModel.ActiveWorld != null && this._dataModel.ActiveWorld.WorkshopId.HasValue &&
                   this._dataModel.ActiveWorld.WorkshopId.Value != 0;
        }

        public void OpenWorkshopExecuted()
        {
            System.Diagnostics.Process.Start(string.Format("http://steamcommunity.com/sharedfiles/filedetails/?id={0}", this._dataModel.ActiveWorld.WorkshopId.Value), null);
        }

        public bool ExportSandboxObjectCanExecute()
        {
            return this._dataModel.ActiveWorld != null && this.Selections.Count > 0;
        }

        public void ExportSandboxObjectExecuted()
        {
            this.ExportSandboxObjectToFile(this.Selections.ToArray());
        }

        public bool TestCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void TestExecuted()
        {
            // TODO: test code goes here.

            var model = new Import3dModelModel();
            model.Load(this._dataModel.ThePlayerCharacter.PositionAndOrientation.Value);
            var loadVm = new Import3dModelViewModel(this, model);

            this.IsBusy = true;
            var newEntity = loadVm.BuildTestEntity();
            this._selectNewStructure = true;
            this._dataModel.CollisionCorrectEntity(newEntity);
            var structure = this._dataModel.AddEntity(newEntity);
            this._selectNewStructure = false;
            this.IsBusy = false;
        }

        public bool AboutCanExecute()
        {
            return true;
        }

        public void AboutExecuted()
        {
            var loadVm = new AboutViewModel(this);
            var result = _dialogService.ShowDialog<WindowAbout>(this, loadVm);
        }

        public bool DeleteObjectCanExecute()
        {
            return this._dataModel.ActiveWorld != null && this.Selections.Count > 0;
        }

        public void DeleteObjectExecuted()
        {
            this.DeleteModel(this.Selections.ToArray());
        }

        void Structures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: this.AddViewModel(e.NewItems[0] as IStructureBase); break;
                case NotifyCollectionChangedAction.Remove: this.RemoveViewModel(e.OldItems[0] as IStructureBase); break;
                case NotifyCollectionChangedAction.Reset: this._structures.Clear(); break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move: throw new NotImplementedException();
            }
        }

        private void AddViewModel(IStructureBase structureBase)
        {
            IStructureViewBase item = null;
            if (structureBase is StructureCharacterModel)
                item = new StructureCharacterViewModel(this, structureBase as StructureCharacterModel);
            else if (structureBase is StructureCubeGridModel)
                item = new StructureCubeGridViewModel(this, structureBase as StructureCubeGridModel);
            else if (structureBase is StructureVoxelModel)
                item = new StructureVoxelViewModel(this, structureBase as StructureVoxelModel);
            else if (structureBase is StructureFloatingObjectModel)
                item = new StructureFloatingObjectViewModel(this, structureBase as StructureFloatingObjectModel);
            else
            {
                throw new NotImplementedException();
            }

            if (item != null)
            {
                this._structures.Add(item);

                if (this._selectNewStructure)
                {
                    this.SelectedStructure = item;
                }
            }
        }

        /// <summary>
        /// Find and remove ViewModel, with the specied Model.
        /// Remove the Entity also.
        /// </summary>
        /// <param name="structureBase"></param>
        private void RemoveViewModel(IStructureBase model)
        {
            var viewModel = this.Structures.FirstOrDefault(s => s.DataModel == model);
            if (viewModel != null && this._dataModel.RemoveEntity(model.EntityBase))
            {
                this.Structures.Remove(viewModel);
            }
        }

        // remove Model from collection, causing sync to happen.
        public void DeleteModel(params IStructureViewBase[] viewModels)
        {
            int index = -1;
            if (viewModels.Length > 0)
            {
                index = this.Structures.IndexOf(viewModels[0]);
            }

            foreach (var viewModel in viewModels)
            {
                bool canDelete = true;

                if (viewModel == null)
                {
                    canDelete = false;
                }
                else if (viewModel is StructureCharacterViewModel)
                {
                    canDelete = !((StructureCharacterViewModel)viewModel).IsPlayer;
                }
                else if (viewModel is StructureCubeGridViewModel)
                {
                    canDelete = !((StructureCubeGridViewModel)viewModel).IsPiloted;
                }

                if (canDelete)
                {
                    this._dataModel.Structures.Remove(viewModel.DataModel);
                }
            }

            // Find and select next object
            while (index >= this.Structures.Count)
            {
                index--;
            }

            if (index > -1)
            {
                this.SelectedStructure = this.Structures[index];
            }
        }

        /// <inheritdoc />
        public string CreateUniqueVoxelFilename(string originalFile)
        {
            return this._dataModel.CreateUniqueVoxelFilename(originalFile);
        }

        public void ImportSandboxObjectFromFile()
        {
            IOpenFileDialog openFileDialog = this._openFileDialogFactory();
            openFileDialog.Filter = Resources.ExportSandboxObjectFilter;
            openFileDialog.Title = Resources.ImportSandboxObjectTitle;
            openFileDialog.Multiselect = true;

            // Open the dialog
            DialogResult result = this._dialogService.ShowOpenFileDialog(this, openFileDialog);

            if (result == DialogResult.OK)
            {
                var badfiles = this._dataModel.LoadEntities(openFileDialog.FileNames);

                foreach(var filename in badfiles)
                {
                    this._dialogService.ShowMessageBox(this, string.Format("Could not load '{0}', because the file is either corrupt or invalid.", Path.GetFileName(filename)), "Could not import", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public void ExportSandboxObjectToFile(params IStructureViewBase[] viewModels)
        {
            //".sbs" Sand Box Content. (app content)
            //".sbc" Sand Box Checkpoint. (game content)
            //".sbs" Sand Box Sector. (game content)
            //".sbo" Sand Box Object. ??

            foreach (var viewModel in viewModels)
            {
                if (viewModel is StructureCharacterViewModel)
                {
                    this._dialogService.ShowMessageBox(this, "Cannot export Player Characters.", "Cannot export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else if (viewModel is StructureVoxelViewModel)
                {
                    this._dialogService.ShowMessageBox(this, "Cannot export Asteroids currently", "Cannot export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else if (viewModel is StructureFloatingObjectViewModel)
                {
                    this._dialogService.ShowMessageBox(this, "Cannot export Floating objects currently", "Cannot export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else if (viewModel is StructureCubeGridViewModel)
                {
                    var structure = (StructureCubeGridViewModel)viewModel;
                    //structure.IsPiloted // TODO: preemptively remove pilots?

                    ISaveFileDialog saveFileDialog = this._saveFileDialogFactory();
                    saveFileDialog.Filter = Resources.ExportSandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Resources.ExportSandboxObjectTitle, structure.ClassType);
                    saveFileDialog.FileName = string.Format("{0}_{1}", structure.ClassType, structure.EntityId);
                    saveFileDialog.OverwritePrompt = true;

                    // Open the dialog
                    DialogResult result = this._dialogService.ShowSaveFileDialog(this, saveFileDialog);

                    if (result == DialogResult.OK)
                    {
                        this._dataModel.SaveEntity(viewModel.DataModel, saveFileDialog.FileName);
                    }
                }
            }
        }

        #endregion

        #region IDragable Interface

        Type IDropable.DataType
        {
            get { return typeof(DataBaseViewModel); }
        }

        void IDropable.Drop(object data, int index)
        {
            this._dataModel.MergeData((IList<IStructureBase>)data);
        }

        #endregion
    }
}