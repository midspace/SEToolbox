namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows.Input;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Views;

    public class ExplorerViewModel : BaseViewModel
    {
        #region Fields

        private ExplorerModel dataModel;
        private readonly IDialogService dialogService;
        private bool? closeResult;

        private IStructureViewBase selectedStructure;
        private ObservableCollection<IStructureViewBase> selections;

        private ObservableCollection<IStructureViewBase> structures;
        private bool selectNewStructure;

        #endregion

        #region event handlers

        public event EventHandler CloseRequested;

        #endregion

        #region Constructors

        public ExplorerViewModel(ExplorerModel dataModel)
            : this(dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public ExplorerViewModel(ExplorerModel dataModel, IDialogService dialogService)
            : base(null)
        {
            Contract.Requires(dialogService != null);

            this.dialogService = dialogService;
            this.dataModel = dataModel;

            this.Selections = new ObservableCollection<IStructureViewBase>();
            this.Structures = new ObservableCollection<IStructureViewBase>();
            foreach (IStructureBase item in this.dataModel.Structures)
            {
                this.AddViewModel(item);
            }

            this.dataModel.Structures.CollectionChanged += Structures_CollectionChanged;
            this.dataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
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

        public ICommand ImportSandBoxCommand
        {
            get
            {
                return new DelegateCommand(new Action(ImportSandBoxExecuted), new Func<bool>(ImportSandBoxCanExecute));
            }
        }

        public ICommand ExportObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(ExportObjectExecuted), new Func<bool>(ExportObjectCanExecute));
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
                return this.closeResult;
            }

            set
            {
                this.closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
            }
        }

        public ObservableCollection<IStructureViewBase> Structures
        {
            get
            {
                return this.structures;
            }

            private set
            {
                if (value != this.structures)
                {
                    this.structures = value;
                    this.RaisePropertyChanged(() => Structures);
                }
            }
        }

        public IStructureViewBase SelectedStructure
        {
            get
            {
                return this.selectedStructure;
            }

            set
            {
                if (value != this.selectedStructure)
                {
                    this.selectedStructure = value;
                    this.RaisePropertyChanged(() => SelectedStructure);
                }
            }
        }

        public ObservableCollection<IStructureViewBase> Selections
        {
            get
            {
                return this.selections;
            }

            set
            {
                if (value != this.selections)
                {
                    this.selections = value;
                    this.RaisePropertyChanged(() => Selections);
                }
            }
        }

        public SaveResource ActiveWorld
        {
            get
            {
                return this.dataModel.ActiveWorld;
            }
            set
            {
                this.dataModel.ActiveWorld = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is available.  This is based on the IsInError and IsBusy properties
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this.dataModel.IsActive;
            }

            set
            {
                this.dataModel.IsActive = value;
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
                return this.dataModel.IsBusy;
            }

            set
            {
                this.dataModel.IsBusy = value;
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
                return this.dataModel.IsModified;
            }

            set
            {
                this.dataModel.IsModified = value;
            }
        }

        public bool IsBaseSaveChanged
        {
            get
            {
                return this.dataModel.IsBaseSaveChanged;
            }

            set
            {
                this.dataModel.IsBaseSaveChanged = value;
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
            model.Load(this.dataModel.BaseSavePath);
            SelectWorldViewModel loadVm = new SelectWorldViewModel(this, model);

            var result = dialogService.ShowDialog<WindowLoad>(this, loadVm);
            if (result == true)
            {
                this.dataModel.ActiveWorld = model.SelectedWorld;
                this.ActiveWorld.LoadCheckpoint();
                this.dataModel.LoadSandBox();
            }
        }

        public bool SaveCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void SaveExecuted()
        {
            if (this.dataModel != null)
            {
                this.dataModel.SaveCheckPointAndSandBox();
            }
        }

        public bool ClearCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ClearExecuted()
        {
            // TODO: clear loaded data.
        }

        public bool ReloadCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ReloadExecuted()
        {
            // TODO: check is save directory is still valid.

            // Reload Checkpoint file.
            this.ActiveWorld.LoadCheckpoint();

            // Load Sector file.
            this.dataModel.LoadSandBox();
        }

        public bool ImportCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ImportExecuted()
        {
            // do nothing. Only required for base menu.
        }

        public bool ImportVoxelCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ImportVoxelExecuted()
        {
            ImportVoxelModel model = new ImportVoxelModel();
            model.Load(this.dataModel.ThePlayerCharacter.PositionAndOrientation.Value);
            ImportVoxelViewModel loadVm = new ImportVoxelViewModel(this, model);

            var result = dialogService.ShowDialog<WindowImportVoxel>(this, loadVm);
            if (result == true)
            {
                this.IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                this.dataModel.AddVoxelFile(loadVm.Filename, loadVm.SourceFile);
                this.selectNewStructure = true;
                var structure = this.dataModel.AddEntity(newEntity);
                this.selectNewStructure = false;
                this.IsBusy = false;
            }
        }

        public bool ImportSandBoxCanExecute()
        {
            return false;
            //return this.dataModel.ActiveWorld != null;
        }

        public void ImportSandBoxExecuted()
        {
            // TODO:
        }

        public bool ImportImageCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ImportImageExecuted()
        {
            ImportImageModel model = new ImportImageModel();
            model.Load(this.dataModel.ThePlayerCharacter.PositionAndOrientation.Value);
            ImportImageViewModel loadVm = new ImportImageViewModel(this, model);

            var result = dialogService.ShowDialog<WindowImportImage>(this, loadVm);
            if (result == true)
            {
                this.IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                this.selectNewStructure = true;
                var structure = this.dataModel.AddEntity(newEntity);
                this.selectNewStructure = false;
                this.IsBusy = false;
            }
        }

        public bool ImportModelCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void ImportModelExecuted()
        {
            Import3dModelModel model = new Import3dModelModel();
            model.Load(this.dataModel.ThePlayerCharacter.PositionAndOrientation.Value);
            Import3dModelViewModel loadVm = new Import3dModelViewModel(this, model);

            var result = dialogService.ShowDialog<WindowImportModel>(this, loadVm);
            if (result == true)
            {
                this.IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                this.selectNewStructure = true;
                var structure = this.dataModel.AddEntity(newEntity);
                this.selectNewStructure = false;
                this.IsBusy = false;
            }
        }

        public bool ExportObjectCanExecute()
        {
            return false;
            //return this.dataModel.ActiveWorld != null;
        }

        public void ExportObjectExecuted()
        {
            // TODO:
        }

        public bool TestCanExecute()
        {
            return this.dataModel.ActiveWorld != null;
        }

        public void TestExecuted()
        {
            // TODO: test code goes here.
        }

        public bool AboutCanExecute()
        {
            return true;
        }

        public void AboutExecuted()
        {
            AboutViewModel loadVm = new AboutViewModel(this);
            var result = dialogService.ShowDialog<WindowAbout>(this, loadVm);
        }

        public bool DeleteObjectCanExecute()
        {
            return this.dataModel.ActiveWorld != null && this.Selections.Count > 0;
        }

        public void DeleteObjectExecuted()
        {
            this.DeleteModel(this.Selections.ToArray());
        }

        #endregion

        void Structures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: this.AddViewModel(e.NewItems[0] as IStructureBase); break;
                case NotifyCollectionChangedAction.Remove: this.RemoveViewModel(e.OldItems[0] as IStructureBase); break;
                case NotifyCollectionChangedAction.Reset: this.structures.Clear(); break;
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
                this.structures.Add(item);

                if (this.selectNewStructure)
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
            if (viewModel != null && this.dataModel.RemoveEntity(model.EntityBase))
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
                    this.dataModel.Structures.Remove(viewModel.DataModel);
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

        public bool ContainsVoxelFilename(string filename)
        {
            return this.dataModel.ContainsVoxelFilename(filename);
        }
    }
}