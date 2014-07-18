namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using SEToolbox.Views;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Shell;
    using VRageMath;
    using WPFLocalizeExtension.Engine;
    using Res = SEToolbox.Properties.Resources;

    public class ExplorerViewModel : BaseViewModel, IDropable, IMainView
    {
        #region Fields

        private readonly ExplorerModel _dataModel;
        private readonly IDialogService _dialogService;
        private readonly Func<IOpenFileDialog> _openFileDialogFactory;
        private readonly Func<ISaveFileDialog> _saveFileDialogFactory;
        private bool? _closeResult;

        private bool _ignoreUpdateSelection;
        private IStructureViewBase _selectedStructure;
        private IStructureViewBase _preSelectedStructure;
        private ObservableCollection<IStructureViewBase> _selections;
        private ObservableCollection<IStructureViewBase> _structures;
        private ObservableCollection<LanguageModel> _languages;

        // If true, when adding new models to the collection, the new models will be highlighted as selected in the UI.
        private bool _selectNewStructure;

        #endregion

        #region event handlers

        public event EventHandler CloseRequested;

        #endregion

        #region Constructors

        public ExplorerViewModel(ExplorerModel dataModel)
            : this(dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IOpenFileDialog>, ServiceLocator.Resolve<ISaveFileDialog>)
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
            this.Selections.CollectionChanged += (sender, e) => this.RaisePropertyChanged(() => IsMultipleSelections);

            this.Structures = new ObservableCollection<IStructureViewBase>();
            foreach (var item in this._dataModel.Structures)
            {
                this.AddViewModel(item);
            }

            this.UpdateLanguages();

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
                return new DelegateCommand(new Func<bool>(ImportCanExecute));
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

        public ICommand OpenComponentListCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenComponentListExecuted), new Func<bool>(OpenComponentListCanExecute));
            }
        }

        public ICommand WorldReportCommand
        {
            get
            {
                return new DelegateCommand(new Action(WorldReportExecuted), new Func<bool>(WorldReportCanExecute));
            }
        }

        public ICommand OpenFolderCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenFolderExecuted), new Func<bool>(OpenFolderCanExecute));
            }
        }

        public ICommand ViewSandboxCommand
        {
            get
            {
                return new DelegateCommand(new Action(ViewSandboxExecuted), new Func<bool>(ViewSandboxCanExecute));
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

        public ICommand CreateFloatingItemCommand
        {
            get
            {
                return new DelegateCommand(new Action(CreateFloatingItemExecuted), new Func<bool>(CreateFloatingItemCanExecute));
            }
        }

        public ICommand GenerateVoxelFieldCommand
        {
            get
            {
                return new DelegateCommand(new Action(GenerateVoxelFieldExecuted), new Func<bool>(GenerateVoxelFieldCanExecute));
            }
        }

        public ICommand Test1Command
        {
            get
            {
                return new DelegateCommand(new Action(Test1Executed), new Func<bool>(Test1CanExecute));
            }
        }

        public ICommand Test2Command
        {
            get
            {
                return new DelegateCommand(new Action(Test2Executed), new Func<bool>(Test2CanExecute));
            }
        }

        public ICommand Test3Command
        {
            get
            {
                return new DelegateCommand(new Action(Test3Executed), new Func<bool>(Test3CanExecute));
            }
        }

        public ICommand Test4Command
        {
            get
            {
                return new DelegateCommand(new Action(Test4Executed), new Func<bool>(Test4CanExecute));
            }
        }

        public ICommand Test5Command
        {
            get
            {
                return new DelegateCommand(new Action(Test5Executed), new Func<bool>(Test5CanExecute));
            }
        }

        public ICommand Test6Command
        {
            get
            {
                return new DelegateCommand(new Action(Test6Executed), new Func<bool>(Test6CanExecute));
            }
        }

        public ICommand OpenUpdatesLinkCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenUpdatesLinkExecuted), new Func<bool>(OpenUpdatesLinkCanExecute));
            }
        }

        public ICommand OpenDocumentationLinkCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenDocumentationLinkExecuted), new Func<bool>(OpenDocumentationLinkCanExecute));
            }
        }

        public ICommand OpenSupportLinkCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenSupportLinkExecuted), new Func<bool>(OpenSupportLinkCanExecute));
            }
        }

        public ICommand AboutCommand
        {
            get
            {
                return new DelegateCommand(new Action(AboutExecuted), new Func<bool>(AboutCanExecute));
            }
        }

        public ICommand LanguageCommand
        {
            get
            {
                return new DelegateCommand(new Func<bool>(LanguageCanExecute));
            }
        }

        public ICommand SetLanguageCommand
        {
            get
            {
                return new DelegateCommand<string>(new Action<string>(SetLanguageExecuted), new Func<string, bool>(SetLanguageCanExecute));
            }
        }

        public ICommand DeleteObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(DeleteObjectExecuted), new Func<bool>(DeleteObjectCanExecute));
            }
        }

        public ICommand GroupMoveCommand
        {
            get
            {
                return new DelegateCommand(new Action(GroupMoveExecuted), new Func<bool>(GroupMoveCanExecute));
            }
        }

        public ICommand RejoinShipCommand
        {
            get
            {
                return new DelegateCommand(new Action(RejoinShipExecuted), new Func<bool>(RejoinShipCanExecute));
            }
        }

        public ICommand JoinShipPartsCommand
        {
            get
            {
                return new DelegateCommand(new Action(JoinShipPartsExecuted), new Func<bool>(JoinShipPartsCanExecute));
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
                    if (this._selectedStructure != null && !_ignoreUpdateSelection)
                        this._selectedStructure.DataModel.InitializeAsync();
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

        public bool IsMultipleSelections
        {
            get
            {
                return this._selections.Count > 1;
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

        public StructureCharacterModel ThePlayerCharacter
        {
            get
            {
                return this._dataModel.ThePlayerCharacter;
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

        public ObservableCollection<LanguageModel> Languages
        {
            get
            {
                return this._languages;
            }

            private set
            {
                if (value != this._languages)
                {
                    this._languages = value;
                    this.RaisePropertyChanged(() => Languages);
                }
            }
        }

        #endregion

        #region Command Methods

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
            var model = new SelectWorldModel();
            model.Load(this._dataModel.BaseLocalSavePath, this._dataModel.BaseDedicatedServerHostSavePath, this._dataModel.BaseDedicatedServerServiceSavePath);
            var loadVm = new SelectWorldViewModel(this, model);

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
            return this._dataModel.ActiveWorld != null &&
                ((this._dataModel.ActiveWorld.SaveType != SaveWorldType.DedicatedServerService && this._dataModel.ActiveWorld.SaveType != SaveWorldType.CustomAdminRequired)
                || (this._dataModel.ActiveWorld.SaveType == SaveWorldType.DedicatedServerService || this._dataModel.ActiveWorld.SaveType == SaveWorldType.CustomAdminRequired
                    && ToolboxUpdater.CheckIsRuningElevated()));
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

        public bool ImportVoxelCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void ImportVoxelExecuted()
        {
            var model = new ImportVoxelModel();
            var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new ImportVoxelViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportVoxel>(this, loadVm);
            if (result == true)
            {
                this.IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                var structure = this._dataModel.AddEntity(newEntity);
                ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                if (this._preSelectedStructure != null)
                    this.SelectedStructure = this._preSelectedStructure;
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
            var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
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
            var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3dModelViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportModel>(this, loadVm);
            if (result == true)
            {
                this.IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                if (loadVm.IsValidModel)
                {
                    this._dataModel.CollisionCorrectEntity(newEntity);
                    var structure = this._dataModel.AddEntity(newEntity);

                    if (structure is StructureVoxelModel)
                    {
                        ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                    }

                    if (this._preSelectedStructure != null)
                        this.SelectedStructure = this._preSelectedStructure;
                }
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

        public bool OpenComponentListCanExecute()
        {
            return true;
        }

        public void OpenComponentListExecuted()
        {
            var model = new ComponentListModel();
            model.Load();
            var loadVm = new ComponentListViewModel(this, model);
            this._dialogService.Show<WindowComponentList>(this, loadVm);
        }

        public bool WorldReportCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void WorldReportExecuted()
        {
            var model = new ResourceReportModel();
            model.Load(this._dataModel.ActiveWorld.Savename, this._dataModel.Structures);
            var loadVm = new ResourceReportViewModel(this, model);
            this._dialogService.ShowDialog<WindowResourceReport>(this, loadVm);
        }

        public bool OpenFolderCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void OpenFolderExecuted()
        {
            System.Diagnostics.Process.Start("Explorer", string.Format("\"{0}\"", this._dataModel.ActiveWorld.Savepath));
        }

        public bool ViewSandboxCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void ViewSandboxExecuted()
        {
            if (this._dataModel != null)
            {
                var filename = this._dataModel.SaveTemporarySandbox();
                System.Diagnostics.Process.Start(string.Format("\"{0}\"", filename));
            }
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

        public bool CreateFloatingItemCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void CreateFloatingItemExecuted()
        {
            var model = new GenerateFloatingObjectModel();
            var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position, this._dataModel.ActiveWorld.Content.MaxFloatingObjects);
            var loadVm = new GenerateFloatingObjectViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowGenerateFloatingObject>(this, loadVm);
            if (result == true)
            {
                this.IsBusy = true;
                var newEntities = loadVm.BuildEntities();
                if (loadVm.IsValidItemToImport)
                {
                    this._selectNewStructure = true;
                    for (var i = 0; i < newEntities.Length; i++)
                    {
                        this._dataModel.AddEntity(newEntities[i]);
                    }
                    this._selectNewStructure = false;
                }
                this.IsBusy = false;
            }
        }

        public bool GenerateVoxelFieldCanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void GenerateVoxelFieldExecuted()
        {
            var model = new GenerateVoxelFieldModel();
            var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new GenerateVoxelFieldViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowGenerateVoxelField>(this, loadVm);
            model.Unload();
            if (result == true)
            {
                this.IsBusy = true;
                string[] sourceVoxelFiles;
                MyObjectBuilder_EntityBase[] newEntities;
                loadVm.BuildEntities(out sourceVoxelFiles, out newEntities);
                this._selectNewStructure = true;

                this.ResetProgress(0, newEntities.Length);

                for (var i = 0; i < newEntities.Length; i++)
                {
                    var structure = this._dataModel.AddEntity(newEntities[i]);
                    ((StructureVoxelModel)structure).SourceVoxelFilepath = sourceVoxelFiles[i]; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                    this.Progress++;
                }
                this._selectNewStructure = false;
                this.IsBusy = false;
                this.ClearProgress();
            }
        }

        public bool Test1CanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void Test1Executed()
        {
            var model = new Import3dModelModel();
            var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3dModelViewModel(this, model);

            this.IsBusy = true;
            var newEntity = loadVm.BuildTestEntity();

            // Split object where X=28|29.
            //newEntity.CubeBlocks.RemoveAll(c => c.Min.X <= 3);
            //newEntity.CubeBlocks.RemoveAll(c => c.Min.X > 4);

            this._selectNewStructure = true;
            this._dataModel.CollisionCorrectEntity(newEntity);
            var structure = this._dataModel.AddEntity(newEntity);
            this._selectNewStructure = false;
            this.IsBusy = false;
        }

        public bool Test2CanExecute()
        {
            return this._dataModel.ActiveWorld != null && this.Selections.Count > 0;
        }

        public void Test2Executed()
        {
            this.TestCalcCubesModel(this.Selections.ToArray());
            //this.OptimizeModel(this.Selections.ToArray());
        }

        public bool Test3CanExecute()
        {
            return this._dataModel.ActiveWorld != null;
        }

        public void Test3Executed()
        {
            var model = new Import3dModelModel();
            var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3dModelViewModel(this, model);

            loadVm.ArmorType = ImportArmorType.Light;
            loadVm.BuildDistance = 10;
            loadVm.ClassType = ImportModelClassType.SmallShip;
            loadVm.Filename = @"D:\Development\SpaceEngineers\building 3D\models\algos.obj";
            loadVm.Forward = new BindableVector3DModel(Vector3.Forward);
            loadVm.IsMaxLengthScale = false;
            loadVm.IsMultipleScale = true;
            loadVm.IsValidModel = true;
            loadVm.MultipleScale = 1;
            loadVm.Up = new BindableVector3DModel(Vector3.Up);

            this.IsBusy = true;
            var newEntity = loadVm.BuildEntity();

            // Split object where X=28|29.
            ((MyObjectBuilder_CubeGrid)newEntity).CubeBlocks.RemoveAll(c => c.Min.X <= 28);

            this._selectNewStructure = true;
            this._dataModel.CollisionCorrectEntity(newEntity);
            var structure = this._dataModel.AddEntity(newEntity);
            this._selectNewStructure = false;
            this.IsBusy = false;
        }

        public bool Test4CanExecute()
        {
            return this._dataModel.ActiveWorld != null && this.Selections.Count > 0;
        }

        public void Test4Executed()
        {
            this.MirrorModel(false, this.Selections.ToArray());
        }

        public bool Test5CanExecute()
        {
            return this._dataModel.ActiveWorld != null && this.Selections.Count > 0;
        }

        public void Test5Executed()
        {
            this._dataModel.TestDisplayRotation(this.Selections[0].DataModel as StructureCubeGridModel);
        }

        public bool Test6CanExecute()
        {
            return this._dataModel.ActiveWorld != null && this.Selections.Count > 0;
        }

        public void Test6Executed()
        {
            this._dataModel.TestConvert(this.Selections[0].DataModel as StructureCubeGridModel);
        }

        public bool OpenUpdatesLinkCanExecute()
        {
            return true;
        }

        public void OpenUpdatesLinkExecuted()
        {
            Process.Start(AppConstants.UpdatesUrl);
        }

        public bool OpenDocumentationLinkCanExecute()
        {
            return true;
        }

        public void OpenDocumentationLinkExecuted()
        {
            Process.Start(AppConstants.DocumentationUrl);
        }

        public bool OpenSupportLinkCanExecute()
        {
            return true;
        }

        public void OpenSupportLinkExecuted()
        {
            Process.Start(AppConstants.SupportUrl);
        }

        public bool LanguageCanExecute()
        {
            return true;
        }

        public bool SetLanguageCanExecute(string code)
        {
            return true;
        }

        public void SetLanguageExecuted(string code)
        {
            GlobalSettings.Default.LanguageCode = code;
            LocalizeDictionary.Instance.SetCurrentThreadCulture = false;
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfoByIetfLanguageTag(code);
            Thread.CurrentThread.CurrentUICulture = LocalizeDictionary.Instance.Culture;

            Sandbox.Common.Localization.MyTextsWrapper.Init();
            this.UpdateLanguages();
            
            // Causes all bindings to update.
            this.OnPropertyChanged("");
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

        public bool GroupMoveCanExecute()
        {
            return this._dataModel.ActiveWorld != null && this.Selections.Count > 1;
        }

        public void GroupMoveExecuted()
        {
            var model = new GroupMoveModel();
            var position = this.ThePlayerCharacter != null ? this.ThePlayerCharacter.PositionAndOrientation.Value.Position.ToVector3() : Vector3.Zero;
            model.Load(this.Selections, position);
            var loadVm = new GroupMoveViewModel(this, model);

            var result = this._dialogService.ShowDialog<WindowGroupMove>(this, loadVm);
            if (result == true)
            {
                model.ApplyNewPositions();
                this._dataModel.CalcDistances();
                this.IsModified = true;
            }
        }

        public bool RejoinShipCanExecute()
        {
            return this._dataModel.ActiveWorld != null && this.Selections.Count == 2 &&
                ((this.Selections[0].DataModel.ClassType == this.Selections[1].DataModel.ClassType && this.Selections[0].DataModel.ClassType == ClassType.LargeShip) ||
                (this.Selections[0].DataModel.ClassType == this.Selections[1].DataModel.ClassType && this.Selections[0].DataModel.ClassType == ClassType.SmallShip));
        }

        public void RejoinShipExecuted()
        {
            this.IsBusy = true;
            this.RejoinShipModels(this.Selections[0], this.Selections[1]);
            this.IsBusy = false;
        }

        public bool JoinShipPartsCanExecute()
        {
            return this._dataModel.ActiveWorld != null && this.Selections.Count == 2 &&
                ((this.Selections[0].DataModel.ClassType == this.Selections[1].DataModel.ClassType && this.Selections[0].DataModel.ClassType == ClassType.LargeShip) ||
                (this.Selections[0].DataModel.ClassType == this.Selections[1].DataModel.ClassType && this.Selections[0].DataModel.ClassType == ClassType.SmallShip));
        }

        public void JoinShipPartsExecuted()
        {
            this.IsBusy = true;
            this.MergeShipPartModels(this.Selections[0], this.Selections[1]);
            this.IsBusy = false;
        }

        #endregion

        #region methods

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
            else if (structureBase is StructureMeteorModel)
                item = new StructureMeteorViewModel(this, structureBase as StructureMeteorModel);
            else if (structureBase is StructureUnknownModel)
                item = new StructureUnknownViewModel(this, structureBase as StructureUnknownModel);
            else
            {
                throw new NotImplementedException("As yet undefined ViewModel has been called.");
            }

            if (item != null)
            {
                this._structures.Add(item);
                this._preSelectedStructure = item;

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
        /// <param name="model"></param>
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

            this._ignoreUpdateSelection = true;
            
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

            this._ignoreUpdateSelection = false;

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

        public void OptimizeModel(params IStructureViewBase[] viewModels)
        {
            foreach (var viewModel in viewModels.OfType<StructureCubeGridViewModel>())
            {
                this._dataModel.OptimizeModel(viewModel.DataModel as StructureCubeGridModel);
            }
        }

        public void MirrorModel(bool oddMirror, params IStructureViewBase[] viewModels)
        {
            foreach (var model in viewModels.OfType<StructureCubeGridViewModel>())
            {
                ((StructureCubeGridModel)model.DataModel).MirrorModel(true, false);
            }
        }

        private void RejoinShipModels(IStructureViewBase viewModel1, IStructureViewBase viewModel2)
        {
            var ship1 = (StructureCubeGridViewModel)viewModel1;
            var ship2 = (StructureCubeGridViewModel)viewModel2;

            this._dataModel.RejoinBrokenShip((StructureCubeGridModel)ship1.DataModel, (StructureCubeGridModel)ship2.DataModel);

            // Delete ship2.
            DeleteModel(viewModel2);

            // Deleting ship2 will also ensure the removal of any duplicate UniqueIds.
            // Any overlapping blocks between the two, will automatically be removed by Space Engineers when the world is loaded.
        }

        private void MergeShipPartModels(IStructureViewBase viewModel1, IStructureViewBase viewModel2)
        {
            var ship1 = (StructureCubeGridViewModel)viewModel1;
            var ship2 = (StructureCubeGridViewModel)viewModel2;

            if (this._dataModel.MergeShipParts((StructureCubeGridModel)ship1.DataModel, (StructureCubeGridModel)ship2.DataModel))
            {
                // Delete ship2.
                DeleteModel(viewModel2);

                // Deleting ship2 will also ensure the removal of any duplicate UniqueIds.
                // Any overlapping blocks between the two, will automatically be removed by Space Engineers when the world is loaded.

                viewModel1.DataModel.UpdateGeneralFromEntityBase();
            }
        }

        /// <inheritdoc />
        public string CreateUniqueVoxelFilename(string originalFile)
        {
            return this._dataModel.CreateUniqueVoxelFilename(originalFile, null);
        }

        public string CreateUniqueVoxelFilename(string originalFile, MyObjectBuilder_EntityBase[] additionalList)
        {
            return this._dataModel.CreateUniqueVoxelFilename(originalFile, additionalList);
        }

        public void ImportSandboxObjectFromFile()
        {
            var openFileDialog = this._openFileDialogFactory();
            openFileDialog.Filter = Res.DialogImportSandboxObjectFilter;
            openFileDialog.Title = Res.DialogImportSandboxObjectTitle;
            openFileDialog.Multiselect = true;

            // Open the dialog
            var result = this._dialogService.ShowOpenFileDialog(this, openFileDialog);

            if (result == DialogResult.OK)
            {
                var badfiles = this._dataModel.LoadEntities(openFileDialog.FileNames);

                foreach (var filename in badfiles)
                {
                    this._dialogService.ShowMessageBox(this, string.Format("Could not load '{0}', because the file is either corrupt or invalid.", Path.GetFileName(filename)), "Could not import", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public void ExportSandboxObjectToFile(params IStructureViewBase[] viewModels)
        {
            foreach (var viewModel in viewModels)
            {
                if (viewModel is StructureCharacterViewModel)
                {
                    var structure = (StructureCharacterViewModel)viewModel;

                    var saveFileDialog = this._saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportSandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, structure.Description);
                    saveFileDialog.FileName = string.Format("{0}_{1}", structure.ClassType, structure.Description);
                    saveFileDialog.OverwritePrompt = true;

                    if (this._dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        this._dataModel.SaveEntity(viewModel.DataModel.EntityBase, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureVoxelViewModel)
                {
                    var structure = (StructureVoxelViewModel)viewModel;
                    var saveFileDialog = this._saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportVoxelFilter;
                    saveFileDialog.Title = Res.DialogExportVoxelTitle;
                    saveFileDialog.FileName = structure.Filename;
                    saveFileDialog.OverwritePrompt = true;

                    if (this._dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        var asteroid = (StructureVoxelModel)structure.DataModel;
                        string sourceFile;

                        if (asteroid.SourceVoxelFilepath != null)
                            sourceFile = asteroid.SourceVoxelFilepath;  // Source Voxel file is temporary. Hasn't been saved yet.
                        else
                            sourceFile = asteroid.VoxelFilepath;  // Source Voxel file exists.
                        File.Copy(sourceFile, saveFileDialog.FileName, true);
                    }
                }
                else if (viewModel is StructureFloatingObjectViewModel)
                {
                    var structure = (StructureFloatingObjectViewModel)viewModel;

                    var saveFileDialog = this._saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportSandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, structure.DisplayName);
                    saveFileDialog.FileName = string.Format("{0}_{1}_{2}", structure.ClassType, structure.DisplayName, structure.Description);
                    saveFileDialog.OverwritePrompt = true;

                    if (this._dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        this._dataModel.SaveEntity(viewModel.DataModel.EntityBase, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureMeteorViewModel)
                {
                    var structure = (StructureMeteorViewModel)viewModel;

                    var saveFileDialog = this._saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportSandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, structure.DisplayName);
                    saveFileDialog.FileName = string.Format("{0}_{1}_{2}", structure.ClassType, structure.DisplayName, structure.Description);
                    saveFileDialog.OverwritePrompt = true;

                    if (this._dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        this._dataModel.SaveEntity(viewModel.DataModel.EntityBase, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureCubeGridViewModel)
                {
                    var structure = (StructureCubeGridViewModel)viewModel;

                    var partname = string.IsNullOrEmpty(structure.DisplayName) ? structure.EntityId.ToString() : structure.DisplayName.Replace("|", "_").Replace("\\", "_").Replace("/", "_");
                    var saveFileDialog = this._saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportSandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, partname);
                    saveFileDialog.FileName = string.Format("{0}_{1}", structure.ClassType, partname);
                    saveFileDialog.OverwritePrompt = true;

                    if (this._dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        var cloneEntity = (MyObjectBuilder_CubeGrid)viewModel.DataModel.EntityBase.Clone();

                        // Call to ToArray() to force Linq to update the value.

                        // Clear Medical room SteamId.
                        cloneEntity.CubeBlocks.Where(c => c.TypeId == SpaceEngineersConsts.MedicalRoom).Select(c => { ((MyObjectBuilder_MedicalRoom)c).SteamUserId = 0; return c; }).ToArray();

                        // Clear Owners.
                        cloneEntity.CubeBlocks.Select(c => { c.Owner = 0; c.ShareMode = MyOwnershipShareModeEnum.None; return c; }).ToArray();

                        // Remove any pilots.
                        cloneEntity.CubeBlocks.Where(c => c.TypeId == SpaceEngineersConsts.Cockpit).Select(c =>
                        {
                            ((MyObjectBuilder_Cockpit)c).ClearPilotAndAutopilot();
                            ((MyObjectBuilder_Cockpit)c).PilotRelativeWorld = null;
                            return c;
                        }).ToArray();

                        this._dataModel.SaveEntity(cloneEntity, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureUnknownViewModel)
                {
                    // Need to use the specific serializer when exporting to generate the correct XML, so Unknown should never be export.
                    this._dialogService.ShowMessageBox(this, "Cannot export Unknown currently", "Cannot export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
        }

        public void TestCalcCubesModel(params IStructureViewBase[] viewModels)
        {
            var bld = new StringBuilder();

            foreach (var viewModel in viewModels.OfType<StructureCubeGridViewModel>())
            {
                var model = viewModel.DataModel as StructureCubeGridModel;
                //var list = model.CubeGrid.CubeBlocks.Where(b => b.SubtypeName.Contains("Red") ||
                //    b.SubtypeName.Contains("Blue") ||
                //    b.SubtypeName.Contains("Green") ||
                //    b.SubtypeName.Contains("Yellow") ||
                //    b.SubtypeName.Contains("White") ||
                //    b.SubtypeName.Contains("Black")).ToArray();

                var list = model.CubeGrid.CubeBlocks.Where(b => b is MyObjectBuilder_Cockpit).ToArray();
                //var list = model.CubeGrid.CubeBlocks.Where(b => b.SubtypeName.Contains("Conveyor")).ToArray();

                foreach (var b in list)
                {
                    CubeType cubeType = CubeType.Exterior;

                    //if (b.SubtypeName.Contains("ArmorSlope"))
                    //{
                    //    var keys = SpaceEngineersAPI.CubeOrientations.Keys.Where(k => k.ToString().Contains("Slope")).ToArray();
                    //    cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(c => keys.Contains(c.Key) && c.Value.Forward == b.BlockOrientation.Forward && c.Value.Up == b.BlockOrientation.Up).Key;
                    //}
                    //else if (b.SubtypeName.Contains("ArmorCornerInv"))
                    //{
                    //    var keys = SpaceEngineersAPI.CubeOrientations.Keys.Where(k => k.ToString().Contains("InverseCorner")).ToArray();
                    //    cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(c => keys.Contains(c.Key) && c.Value.Forward == b.BlockOrientation.Forward && c.Value.Up == b.BlockOrientation.Up).Key;
                    //}
                    //else if (b.SubtypeName.Contains("ArmorCorner"))
                    //{
                    //    var keys = SpaceEngineersAPI.CubeOrientations.Keys.Where(k => k.ToString().Contains("NormalCorner")).ToArray();
                    //    cubeType = SpaceEngineersAPI.CubeOrientations.FirstOrDefault(c => keys.Contains(c.Key) && c.Value.Forward == b.BlockOrientation.Forward && c.Value.Up == b.BlockOrientation.Up).Key;
                    //}

                    //SpaceEngineersAPI.CubeOrientations

                    // XYZ= (7, 15, 3)   Orientation = (0, 0, 0, 1)  SmallBlockArmorSlopeBlue               CubeType.SlopeCenterBackBottom
                    // XYZ= (8, 14, 3)   Orientation = (1, 0, 0, 0)  SmallBlockArmorCornerInvBlue           CubeType.InverseCornerRightFrontTop
                    // XYZ= (8, 15, 3)   Orientation = (0, 0, -0.7071068, 0.7071068)  SmallBlockArmorCornerBlue     CubeType.NormalCornerLeftBackBottom

                    // XYZ= (13, 9, 3)   Orientation = (1, 0, 0, 0)  SmallBlockArmorCornerInvGreen          CubeType.InverseCornerRightFrontTop
                    // XYZ= (14, 8, 3)   Orientation = (0, 0, -0.7071068, 0.7071068)  SmallBlockArmorSlopeGreen     CubeType.SlopeLeftCenterBottom
                    // XYZ= (14, 9, 3)   Orientation = (0, 0, -0.7071068, 0.7071068)  SmallBlockArmorCornerGreen        CubeType.NormalCornerLeftBackBottom


                    bld.AppendFormat("// XYZ= ({0}, {1}, {2})   Orientation = ({3}, {4})  {5}    CubeType.{6}\r\n",
                        b.Min.X, b.Min.Y, b.Min.Z, b.BlockOrientation.Forward, b.BlockOrientation.Up, b.SubtypeName, cubeType);
                }
            }
            Debug.Write(bld.ToString());
        }

        public void CalcDistances()
        {
            this._dataModel.CalcDistances();
        }

        private void UpdateLanguages()
        {
            var list = new List<LanguageModel>();

            foreach (var kvp in AppConstants.SupportedLanguages)
            {
                var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(kvp.Key);
                list.Add(new LanguageModel() {IetfLanguageTag = culture.IetfLanguageTag, LanguageName = culture.DisplayName, NativeName = culture.NativeName, ImageName = kvp.Value});
            }

            this.Languages = new ObservableCollection<LanguageModel>(list);
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

        #region IMainView Interface

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

        public TaskbarItemProgressState ProgressState
        {
            get
            {
                return this._dataModel.ProgressState;
            }

            set
            {
                this._dataModel.ProgressState = value;
            }
        }

        public double ProgressValue
        {
            get
            {
                return this._dataModel.ProgressValue;
            }

            set
            {
                this._dataModel.ProgressValue = value;
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

        public void ResetProgress(double initial, double maximumProgress)
        {
            this._dataModel.ResetProgress(initial, maximumProgress);
        }

        public void ClearProgress()
        {
            this._dataModel.ClearProgress();
        }

        public void IncrementProgress()
        {
            this._dataModel.IncrementProgress();
        }

        public MyObjectBuilder_Checkpoint Checkpoint
        {
            get { return this.ActiveWorld.Content; }
        }

        /// <summary>
        /// Read in current 'world' color Palette.
        /// </summary>
        public int[] CreativeModeColors
        {
            get
            {
                return this._dataModel.CreativeModeColors;
            }

            set
            {
                this._dataModel.CreativeModeColors = value;
            }
        }

        #endregion
    }
}