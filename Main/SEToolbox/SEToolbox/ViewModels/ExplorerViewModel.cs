namespace SEToolbox.ViewModels
{
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

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using SEToolbox.Views;
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

            _dialogService = dialogService;
            _openFileDialogFactory = openFileDialogFactory;
            _saveFileDialogFactory = saveFileDialogFactory;
            _dataModel = dataModel;

            Selections = new ObservableCollection<IStructureViewBase>();
            Selections.CollectionChanged += (sender, e) => RaisePropertyChanged(() => IsMultipleSelections);

            Structures = new ObservableCollection<IStructureViewBase>();
            foreach (var item in _dataModel.Structures)
            {
                AddViewModel(item);
            }

            UpdateLanguages();

            _dataModel.Structures.CollectionChanged += Structures_CollectionChanged;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Command Properties

        public ICommand ClosingCommand
        {
            get { return new DelegateCommand<CancelEventArgs>(ClosingExecuted, ClosingCanExecute); }
        }

        public ICommand OpenCommand
        {
            get { return new DelegateCommand(OpenExecuted, OpenCanExecute); }
        }

        public ICommand SaveCommand
        {
            get { return new DelegateCommand(SaveExecuted, SaveCanExecute); }
        }

        public ICommand ClearCommand
        {
            get { return new DelegateCommand(ClearExecuted, ClearCanExecute); }
        }

        public ICommand ReloadCommand
        {
            get { return new DelegateCommand(ReloadExecuted, ReloadCanExecute); }
        }

        public ICommand IsActiveCommand
        {
            get { return new DelegateCommand(new Func<bool>(IsActiveCanExecute)); }
        }

        public ICommand ImportVoxelCommand
        {
            get { return new DelegateCommand(ImportVoxelExecuted, ImportVoxelCanExecute); }
        }

        public ICommand ImportImageCommand
        {
            get { return new DelegateCommand(ImportImageExecuted, ImportImageCanExecute); }
        }

        public ICommand ImportModelCommand
        {
            get { return new DelegateCommand(ImportModelExecuted, ImportModelCanExecute); }
        }

        public ICommand ImportAsteroidModelCommand
        {
            get { return new DelegateCommand(ImportAsteroidModelExecuted, ImportAsteroidModelCanExecute); }
        }

        public ICommand ImportSandboxObjectCommand
        {
            get { return new DelegateCommand(ImportSandboxObjectExecuted, ImportSandboxObjectCanExecute); }
        }

        public ICommand OpenComponentListCommand
        {
            get { return new DelegateCommand(OpenComponentListExecuted, OpenComponentListCanExecute); }
        }

        public ICommand WorldReportCommand
        {
            get { return new DelegateCommand(WorldReportExecuted, WorldReportCanExecute); }
        }

        public ICommand OpenFolderCommand
        {
            get { return new DelegateCommand(OpenFolderExecuted, OpenFolderCanExecute); }
        }

        public ICommand ViewSandboxCommand
        {
            get { return new DelegateCommand(ViewSandboxExecuted, ViewSandboxCanExecute); }
        }

        public ICommand OpenWorkshopCommand
        {
            get { return new DelegateCommand(OpenWorkshopExecuted, OpenWorkshopCanExecute); }
        }

        public ICommand ExportSandboxObjectCommand
        {
            get { return new DelegateCommand(ExportSandboxObjectExecuted, ExportSandboxObjectCanExecute); }
        }

        public ICommand ExportBasicSandboxObjectCommand
        {
            get { return new DelegateCommand(ExportBasicSandboxObjectExecuted, ExportBasicSandboxObjectCanExecute); }
        }

        public ICommand CreateFloatingItemCommand
        {
            get { return new DelegateCommand(CreateFloatingItemExecuted, CreateFloatingItemCanExecute); }
        }

        public ICommand GenerateVoxelFieldCommand
        {
            get { return new DelegateCommand(GenerateVoxelFieldExecuted, GenerateVoxelFieldCanExecute); }
        }

        public ICommand Test1Command
        {
            get { return new DelegateCommand(Test1Executed, Test1CanExecute); }
        }

        public ICommand Test2Command
        {
            get { return new DelegateCommand(Test2Executed, Test2CanExecute); }
        }

        public ICommand Test3Command
        {
            get { return new DelegateCommand(Test3Executed, Test3CanExecute); }
        }

        public ICommand Test4Command
        {
            get { return new DelegateCommand(Test4Executed, Test4CanExecute); }
        }

        public ICommand Test5Command
        {
            get { return new DelegateCommand(Test5Executed, Test5CanExecute); }
        }

        public ICommand Test6Command
        {
            get { return new DelegateCommand(Test6Executed, Test6CanExecute); }
        }

        public ICommand OpenUpdatesLinkCommand
        {
            get { return new DelegateCommand(OpenUpdatesLinkExecuted, OpenUpdatesLinkCanExecute); }
        }

        public ICommand OpenDocumentationLinkCommand
        {
            get { return new DelegateCommand(OpenDocumentationLinkExecuted, OpenDocumentationLinkCanExecute); }
        }

        public ICommand OpenSupportLinkCommand
        {
            get { return new DelegateCommand(OpenSupportLinkExecuted, OpenSupportLinkCanExecute); }
        }

        public ICommand AboutCommand
        {
            get { return new DelegateCommand(AboutExecuted, AboutCanExecute); }
        }

        public ICommand LanguageCommand
        {
            get { return new DelegateCommand(new Func<bool>(LanguageCanExecute)); }
        }

        public ICommand SetLanguageCommand
        {
            get { return new DelegateCommand<string>(SetLanguageExecuted, SetLanguageCanExecute); }
        }

        public ICommand DeleteObjectCommand
        {
            get { return new DelegateCommand(DeleteObjectExecuted, DeleteObjectCanExecute); }
        }

        public ICommand GroupMoveCommand
        {
            get { return new DelegateCommand(GroupMoveExecuted, GroupMoveCanExecute); }
        }

        public ICommand RejoinShipCommand
        {
            get { return new DelegateCommand(RejoinShipExecuted, RejoinShipCanExecute); }
        }

        public ICommand JoinShipPartsCommand
        {
            get { return new DelegateCommand(JoinShipPartsExecuted, JoinShipPartsCanExecute); }
        }

        public ICommand RepairShipsCommand
        {
            get { return new DelegateCommand(RepairShipsExecuted, RepairShipsCanExecute); }
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
                RaisePropertyChanged(() => CloseResult);
            }
        }

        public ObservableCollection<IStructureViewBase> Structures
        {
            get
            {
                return _structures;
            }

            private set
            {
                if (value != _structures)
                {
                    _structures = value;
                    RaisePropertyChanged(() => Structures);
                }
            }
        }

        public IStructureViewBase SelectedStructure
        {
            get
            {
                return _selectedStructure;
            }

            set
            {
                if (value != _selectedStructure)
                {
                    _selectedStructure = value;
                    if (_selectedStructure != null && !_ignoreUpdateSelection)
                        _selectedStructure.DataModel.InitializeAsync();
                    RaisePropertyChanged(() => SelectedStructure);
                }
            }
        }

        public ObservableCollection<IStructureViewBase> Selections
        {
            get
            {
                return _selections;
            }

            set
            {
                if (value != _selections)
                {
                    _selections = value;
                    RaisePropertyChanged(() => Selections);
                }
            }
        }

        public bool IsMultipleSelections
        {
            get
            {
                return _selections.Count > 1;
            }
        }

        public WorldResource ActiveWorld
        {
            get
            {
                return _dataModel.ActiveWorld;
            }
            set
            {
                _dataModel.ActiveWorld = value;
            }
        }

        public StructureCharacterModel ThePlayerCharacter
        {
            get
            {
                return _dataModel.ThePlayerCharacter;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is available.  This is based on the IsInError and IsBusy properties
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _dataModel.IsActive;
            }

            set
            {
                _dataModel.IsActive = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _dataModel.IsBusy;
            }

            set
            {
                _dataModel.IsBusy = value;
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
                return _dataModel.IsModified;
            }

            set
            {
                _dataModel.IsModified = value;
            }
        }

        public bool IsBaseSaveChanged
        {
            get
            {
                return _dataModel.IsBaseSaveChanged;
            }

            set
            {
                _dataModel.IsBaseSaveChanged = value;
            }
        }

        public ObservableCollection<LanguageModel> Languages
        {
            get
            {
                return _languages;
            }

            private set
            {
                if (value != _languages)
                {
                    _languages = value;
                    RaisePropertyChanged(() => Languages);
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


            //if (CheckCloseWindow() == false)
            //{
            //e.Cancel = true;
            //CloseResult = null;
            //}
            //else
            {
                if (CloseRequested != null)
                {
                    CloseRequested(this, EventArgs.Empty);
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
            model.Load(SpaceEngineersConsts.BaseLocalPath, SpaceEngineersConsts.BaseDedicatedServerHostPath, SpaceEngineersConsts.BaseDedicatedServerServicePath);
            var loadVm = new SelectWorldViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowLoad>(this, loadVm);
            if (result == true)
            {
                _dataModel.BeginLoad();
                _dataModel.ActiveWorld = model.SelectedWorld;
                ActiveWorld.LoadCheckpoint();
                ActiveWorld.LoadDefinitionsAndMods();
                ActiveWorld.LoadSector();
                _dataModel.ParseSandBox();
                _dataModel.EndLoad();
            }
        }

        public bool SaveCanExecute()
        {
            return _dataModel.ActiveWorld != null &&
                ((_dataModel.ActiveWorld.SaveType != SaveWorldType.DedicatedServerService && _dataModel.ActiveWorld.SaveType != SaveWorldType.CustomAdminRequired)
                || (_dataModel.ActiveWorld.SaveType == SaveWorldType.DedicatedServerService || _dataModel.ActiveWorld.SaveType == SaveWorldType.CustomAdminRequired
                    && ToolboxUpdater.IsRuningElevated()));
        }

        public void SaveExecuted()
        {
            if (_dataModel != null)
            {
                _dataModel.SaveCheckPointAndSandBox();
            }
        }

        public bool ClearCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ClearExecuted()
        {
            // TODO: clear loaded data.
        }

        public bool ReloadCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ReloadExecuted()
        {
            // TODO: check is save directory is still valid.

            _dataModel.BeginLoad();

            // Reload Checkpoint file.
            ActiveWorld.LoadCheckpoint();

            // Reload Definitions, Mods, and clear out Materials, Textures.
            ActiveWorld.LoadDefinitionsAndMods();
            Converters.DDSConverter.ClearCache();

            // Load Sector file.
            ActiveWorld.LoadSector();

            _dataModel.ParseSandBox();
            _dataModel.EndLoad();
        }

        public bool IsActiveCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public bool ImportVoxelCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportVoxelExecuted()
        {
            var model = new ImportVoxelModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new ImportVoxelViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportVoxel>(this, loadVm);
            if (result == true)
            {
                IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                var structure = _dataModel.AddEntity(newEntity);
                ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                if (_preSelectedStructure != null)
                    SelectedStructure = _preSelectedStructure;
                IsBusy = false;
            }
        }

        public bool ImportImageCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportImageExecuted()
        {
            var model = new ImportImageModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new ImportImageViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportImage>(this, loadVm);
            if (result == true)
            {
                IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                _selectNewStructure = true;
                _dataModel.CollisionCorrectEntity(newEntity);
                _dataModel.AddEntity(newEntity);
                _selectNewStructure = false;
                IsBusy = false;
            }
        }

        public bool ImportModelCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportModelExecuted()
        {
            var model = new Import3DModelModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3DModelViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportModel>(this, loadVm);
            if (result == true)
            {
                IsBusy = true;
                var newEntity = loadVm.BuildEntity();
                if (loadVm.IsValidModel)
                {
                    _dataModel.CollisionCorrectEntity(newEntity);
                    var structure = _dataModel.AddEntity(newEntity);

                    if (structure is StructureVoxelModel)
                    {
                        ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                    }

                    if (_preSelectedStructure != null)
                        SelectedStructure = _preSelectedStructure;
                }
                IsBusy = false;
            }
        }

        public bool ImportAsteroidModelCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportAsteroidModelExecuted()
        {
            var model = new Import3DAsteroidModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3DAsteroidViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowImportAsteroidModel>(this, loadVm);
            if (result == true && loadVm.IsValidEntity)
            {
                IsBusy = true;
                _dataModel.CollisionCorrectEntity(loadVm.NewEntity);
                var structure = _dataModel.AddEntity(loadVm.NewEntity);
                ((StructureVoxelModel)structure).SourceVoxelFilepath = loadVm.SourceFile; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                if (_preSelectedStructure != null)
                    SelectedStructure = _preSelectedStructure;
                IsBusy = false;
            }
        }

        public bool ImportSandboxObjectCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ImportSandboxObjectExecuted()
        {
            ImportSandboxObjectFromFile();
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
            _dialogService.Show<WindowComponentList>(this, loadVm);
        }

        public bool WorldReportCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void WorldReportExecuted()
        {
            var model = new ResourceReportModel();
            model.Load(_dataModel.ActiveWorld.Savename, _dataModel.Structures);
            var loadVm = new ResourceReportViewModel(this, model);
            _dialogService.ShowDialog<WindowResourceReport>(this, loadVm);
        }

        public bool OpenFolderCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void OpenFolderExecuted()
        {
            Process.Start("Explorer", string.Format("\"{0}\"", _dataModel.ActiveWorld.Savepath));
        }

        public bool ViewSandboxCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void ViewSandboxExecuted()
        {
            if (_dataModel != null)
            {
                var filename = _dataModel.SaveTemporarySandbox();
                Process.Start(string.Format("\"{0}\"", filename));
            }
        }
        public bool OpenWorkshopCanExecute()
        {
            return _dataModel.ActiveWorld != null && _dataModel.ActiveWorld.WorkshopId.HasValue &&
                   _dataModel.ActiveWorld.WorkshopId.Value != 0;
        }

        public void OpenWorkshopExecuted()
        {
            Process.Start(string.Format("http://steamcommunity.com/sharedfiles/filedetails/?id={0}", _dataModel.ActiveWorld.WorkshopId.Value), null);
        }

        public bool ExportSandboxObjectCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void ExportSandboxObjectExecuted()
        {
            ExportSandboxObjectToFile(false, Selections.ToArray());
        }

        public bool ExportBasicSandboxObjectCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void ExportBasicSandboxObjectExecuted()
        {
            ExportSandboxObjectToFile(true, Selections.ToArray());
        }

        public bool CreateFloatingItemCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void CreateFloatingItemExecuted()
        {
            var model = new GenerateFloatingObjectModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position, _dataModel.ActiveWorld.Checkpoint.MaxFloatingObjects);
            var loadVm = new GenerateFloatingObjectViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowGenerateFloatingObject>(this, loadVm);
            if (result == true)
            {
                IsBusy = true;
                var newEntities = loadVm.BuildEntities();
                if (loadVm.IsValidItemToImport)
                {
                    _selectNewStructure = true;
                    for (var i = 0; i < newEntities.Length; i++)
                    {
                        _dataModel.AddEntity(newEntities[i]);
                    }
                    _selectNewStructure = false;
                }
                IsBusy = false;
            }
        }

        public bool GenerateVoxelFieldCanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void GenerateVoxelFieldExecuted()
        {
            var model = new GenerateVoxelFieldModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new GenerateVoxelFieldViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowGenerateVoxelField>(this, loadVm);
            model.Unload();
            if (result == true)
            {
                IsBusy = true;
                string[] sourceVoxelFiles;
                MyObjectBuilder_EntityBase[] newEntities;
                loadVm.BuildEntities(out sourceVoxelFiles, out newEntities);
                _selectNewStructure = true;

                ResetProgress(0, newEntities.Length);

                for (var i = 0; i < newEntities.Length; i++)
                {
                    var structure = _dataModel.AddEntity(newEntities[i]);
                    ((StructureVoxelModel)structure).SourceVoxelFilepath = sourceVoxelFiles[i]; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.
                    Progress++;
                }
                _selectNewStructure = false;
                IsBusy = false;
                ClearProgress();
            }
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
            UpdateLanguages();

            // Causes all bindings to update.
            OnPropertyChanged("");
        }

        public bool AboutCanExecute()
        {
            return true;
        }

        public void AboutExecuted()
        {
            var loadVm = new AboutViewModel(this);
            _dialogService.ShowDialog<WindowAbout>(this, loadVm);
        }

        public bool DeleteObjectCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void DeleteObjectExecuted()
        {
            DeleteModel(Selections.ToArray());
        }

        public bool GroupMoveCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 1;
        }

        public void GroupMoveExecuted()
        {
            var model = new GroupMoveModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value.Position.ToVector3() : Vector3.Zero;
            model.Load(Selections, position);
            var loadVm = new GroupMoveViewModel(this, model);

            var result = _dialogService.ShowDialog<WindowGroupMove>(this, loadVm);
            if (result == true)
            {
                model.ApplyNewPositions();
                _dataModel.CalcDistances();
                IsModified = true;
            }
        }

        public bool RejoinShipCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count == 2 &&
                ((Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.LargeShip) ||
                (Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.SmallShip));
        }

        public void RejoinShipExecuted()
        {
            IsBusy = true;
            RejoinShipModels(Selections[0], Selections[1]);
            IsBusy = false;
        }

        public bool JoinShipPartsCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count == 2 &&
                ((Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.LargeShip) ||
                (Selections[0].DataModel.ClassType == Selections[1].DataModel.ClassType && Selections[0].DataModel.ClassType == ClassType.SmallShip));
        }

        public void JoinShipPartsExecuted()
        {
            IsBusy = true;
            MergeShipPartModels(Selections[0], Selections[1]);
            IsBusy = false;
        }

        public bool RepairShipsCanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void RepairShipsExecuted()
        {
            IsBusy = true;
            RepairShips(Selections);
            IsBusy = false;
        }

        #endregion

        #region Test command methods

        public bool Test1CanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void Test1Executed()
        {
            var model = new Import3DModelModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3DModelViewModel(this, model);

            IsBusy = true;
            var newEntity = loadVm.BuildTestEntity();

            // Split object where X=28|29.
            //newEntity.CubeBlocks.RemoveAll(c => c.Min.X <= 3);
            //newEntity.CubeBlocks.RemoveAll(c => c.Min.X > 4);

            _selectNewStructure = true;
            _dataModel.CollisionCorrectEntity(newEntity);
            _dataModel.AddEntity(newEntity);
            _selectNewStructure = false;
            IsBusy = false;
        }

        public bool Test2CanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void Test2Executed()
        {
            TestCalcCubesModel(Selections.ToArray());
            //OptimizeModel(Selections.ToArray());
        }

        public bool Test3CanExecute()
        {
            return _dataModel.ActiveWorld != null;
        }

        public void Test3Executed()
        {
            var model = new Import3DModelModel();
            var position = ThePlayerCharacter != null ? ThePlayerCharacter.PositionAndOrientation.Value : new MyPositionAndOrientation(Vector3.Zero, Vector3.Forward, Vector3.Up);
            model.Load(position);
            var loadVm = new Import3DModelViewModel(this, model);

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

            IsBusy = true;
            var newEntity = loadVm.BuildEntity();

            // Split object where X=28|29.
            ((MyObjectBuilder_CubeGrid)newEntity).CubeBlocks.RemoveAll(c => c.Min.X <= 28);

            _selectNewStructure = true;
            _dataModel.CollisionCorrectEntity(newEntity);
            _dataModel.AddEntity(newEntity);
            _selectNewStructure = false;
            IsBusy = false;
        }

        public bool Test4CanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void Test4Executed()
        {
            MirrorModel(false, Selections.ToArray());
        }

        public bool Test5CanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void Test5Executed()
        {
            _dataModel.TestDisplayRotation(Selections[0].DataModel as StructureCubeGridModel);
        }

        public bool Test6CanExecute()
        {
            return _dataModel.ActiveWorld != null && Selections.Count > 0;
        }

        public void Test6Executed()
        {
            _dataModel.TestConvert(Selections[0].DataModel as StructureCubeGridModel);
        }

        #endregion

        #region methods

        void Structures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: AddViewModel(e.NewItems[0] as IStructureBase); break;
                case NotifyCollectionChangedAction.Remove: RemoveViewModel(e.OldItems[0] as IStructureBase); break;
                case NotifyCollectionChangedAction.Reset: _structures.Clear(); break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move: throw new NotImplementedException();
            }
        }

        private void AddViewModel(IStructureBase structureBase)
        {
            IStructureViewBase item;

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

            _structures.Add(item);
            _preSelectedStructure = item;

            if (_selectNewStructure)
            {
                SelectedStructure = item;
            }
        }

        /// <summary>
        /// Find and remove ViewModel, with the specied Model.
        /// Remove the Entity also.
        /// </summary>
        /// <param name="model"></param>
        private void RemoveViewModel(IStructureBase model)
        {
            var viewModel = Structures.FirstOrDefault(s => s.DataModel == model);
            if (viewModel != null && _dataModel.RemoveEntity(model.EntityBase))
            {
                Structures.Remove(viewModel);
            }
        }

        // remove Model from collection, causing sync to happen.
        public void DeleteModel(params IStructureViewBase[] viewModels)
        {
            int index = -1;
            if (viewModels.Length > 0)
            {
                index = Structures.IndexOf(viewModels[0]);
            }

            _ignoreUpdateSelection = true;

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
                    _dataModel.Structures.Remove(viewModel.DataModel);
                }
            }

            _ignoreUpdateSelection = false;

            // Find and select next object
            while (index >= Structures.Count)
            {
                index--;
            }

            if (index > -1)
            {
                SelectedStructure = Structures[index];
            }
        }

        public void OptimizeModel(params IStructureViewBase[] viewModels)
        {
            foreach (var viewModel in viewModels.OfType<StructureCubeGridViewModel>())
            {
                _dataModel.OptimizeModel(viewModel.DataModel as StructureCubeGridModel);
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

            _dataModel.RejoinBrokenShip((StructureCubeGridModel)ship1.DataModel, (StructureCubeGridModel)ship2.DataModel);

            // Delete ship2.
            DeleteModel(viewModel2);

            // Deleting ship2 will also ensure the removal of any duplicate UniqueIds.
            // Any overlapping blocks between the two, will automatically be removed by Space Engineers when the world is loaded.
        }

        private void MergeShipPartModels(IStructureViewBase viewModel1, IStructureViewBase viewModel2)
        {
            var ship1 = (StructureCubeGridViewModel)viewModel1;
            var ship2 = (StructureCubeGridViewModel)viewModel2;

            if (_dataModel.MergeShipParts((StructureCubeGridModel)ship1.DataModel, (StructureCubeGridModel)ship2.DataModel))
            {
                // Delete ship2.
                DeleteModel(viewModel2);

                // Deleting ship2 will also ensure the removal of any duplicate UniqueIds.
                // Any overlapping blocks between the two, will automatically be removed by Space Engineers when the world is loaded.

                viewModel1.DataModel.UpdateGeneralFromEntityBase();
            }
        }

        private void RepairShips(IEnumerable<IStructureViewBase> structures)
        {
            foreach (var structure in structures)
            {
                if (structure.DataModel.ClassType == ClassType.SmallShip
                    || structure.DataModel.ClassType == ClassType.LargeShip
                    || structure.DataModel.ClassType == ClassType.Station)
                {
                    ((StructureCubeGridViewModel)structure).RepairObjectExecuted();
                }
            }
        }

        /// <inheritdoc />
        public string CreateUniqueVoxelFilename(string originalFile)
        {
            return _dataModel.CreateUniqueVoxelFilename(originalFile, null);
        }

        public string CreateUniqueVoxelFilename(string originalFile, MyObjectBuilder_EntityBase[] additionalList)
        {
            return _dataModel.CreateUniqueVoxelFilename(originalFile, additionalList);
        }

        public void ImportSandboxObjectFromFile()
        {
            var openFileDialog = _openFileDialogFactory();
            openFileDialog.Filter = Res.DialogImportSandboxObjectFilter;
            openFileDialog.Title = Res.DialogImportSandboxObjectTitle;
            openFileDialog.Multiselect = true;

            // Open the dialog
            var result = _dialogService.ShowOpenFileDialog(this, openFileDialog);

            if (result == DialogResult.OK)
            {
                var badfiles = _dataModel.LoadEntities(openFileDialog.FileNames);

                foreach (var filename in badfiles)
                {
                    _dialogService.ShowMessageBox(this, string.Format("Could not load '{0}', because the file is either corrupt or invalid.", Path.GetFileName(filename)), "Could not import", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public void ExportSandboxObjectToFile(bool blankOwnerAndMedBays, params IStructureViewBase[] viewModels)
        {
            foreach (var viewModel in viewModels)
            {
                if (viewModel is StructureCharacterViewModel)
                {
                    var structure = (StructureCharacterViewModel)viewModel;

                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportSandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, structure.Description);
                    saveFileDialog.FileName = string.Format("{0}_{1}", structure.ClassType, structure.Description);
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        _dataModel.SaveEntity(viewModel.DataModel.EntityBase, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureVoxelViewModel)
                {
                    var structure = (StructureVoxelViewModel)viewModel;
                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportVoxelFilter;
                    saveFileDialog.Title = Res.DialogExportVoxelTitle;
                    saveFileDialog.FileName = structure.Filename;
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
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

                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportSandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, structure.DisplayName);
                    saveFileDialog.FileName = string.Format("{0}_{1}_{2}", structure.ClassType, structure.DisplayName, structure.Description);
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        _dataModel.SaveEntity(viewModel.DataModel.EntityBase, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureMeteorViewModel)
                {
                    var structure = (StructureMeteorViewModel)viewModel;

                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportSandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, structure.DisplayName);
                    saveFileDialog.FileName = string.Format("{0}_{1}_{2}", structure.ClassType, structure.DisplayName, structure.Description);
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        _dataModel.SaveEntity(viewModel.DataModel.EntityBase, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureCubeGridViewModel)
                {
                    var structure = (StructureCubeGridViewModel)viewModel;

                    var partname = string.IsNullOrEmpty(structure.DisplayName) ? structure.EntityId.ToString() : structure.DisplayName.Replace("|", "_").Replace("\\", "_").Replace("/", "_");
                    var saveFileDialog = _saveFileDialogFactory();
                    saveFileDialog.Filter = Res.DialogExportSandboxObjectFilter;
                    saveFileDialog.Title = string.Format(Res.DialogExportSandboxObjectTitle, structure.ClassType, partname);
                    saveFileDialog.FileName = string.Format("{0}_{1}", structure.ClassType, partname);
                    saveFileDialog.OverwritePrompt = true;

                    if (_dialogService.ShowSaveFileDialog(this, saveFileDialog) == DialogResult.OK)
                    {
                        var cloneEntity = (MyObjectBuilder_CubeGrid)viewModel.DataModel.EntityBase.Clone();

                        if (blankOwnerAndMedBays)
                        {
                            // Call to ToArray() to force Linq to update the value.

                            // Clear Medical room SteamId.
                            cloneEntity.CubeBlocks.Where(c => c.TypeId == SpaceEngineersConsts.MedicalRoom).Select(c => { ((MyObjectBuilder_MedicalRoom)c).SteamUserId = 0; return c; }).ToArray();

                            // Clear Owners.
                            cloneEntity.CubeBlocks.Select(c => { c.Owner = 0; c.ShareMode = MyOwnershipShareModeEnum.None; return c; }).ToArray();
                        }

                        // Remove any pilots.
                        cloneEntity.CubeBlocks.Where(c => c.TypeId == SpaceEngineersConsts.Cockpit).Select(c =>
                        {
                            ((MyObjectBuilder_Cockpit)c).ClearPilotAndAutopilot();
                            ((MyObjectBuilder_Cockpit)c).PilotRelativeWorld = null;
                            return c;
                        }).ToArray();

                        _dataModel.SaveEntity(cloneEntity, saveFileDialog.FileName);
                    }
                }
                else if (viewModel is StructureUnknownViewModel)
                {
                    // Need to use the specific serializer when exporting to generate the correct XML, so Unknown should never be export.
                    _dialogService.ShowMessageBox(this, "Cannot export Unknown currently", "Cannot export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
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
            _dataModel.CalcDistances();
        }

        private void UpdateLanguages()
        {
            var list = new List<LanguageModel>();

            foreach (var kvp in AppConstants.SupportedLanguages)
            {
                var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(kvp.Key);
                list.Add(new LanguageModel { IetfLanguageTag = culture.IetfLanguageTag, LanguageName = culture.DisplayName, NativeName = culture.NativeName, ImageName = kvp.Value });
            }

            Languages = new ObservableCollection<LanguageModel>(list);
        }

        public IStructureBase AddEntity(MyObjectBuilder_EntityBase entity)
        {
            return _dataModel.AddEntity(entity);
        }

        #endregion

        #region IDragable Interface

        Type IDropable.DataType
        {
            get { return typeof(DataBaseViewModel); }
        }

        void IDropable.Drop(object data, int index)
        {
            _dataModel.MergeData((IList<IStructureBase>)data);
        }

        #endregion

        #region IMainView Interface

        public bool ShowProgress
        {
            get
            {
                return _dataModel.ShowProgress;
            }

            set
            {
                _dataModel.ShowProgress = value;
            }
        }

        public double Progress
        {
            get
            {
                return _dataModel.Progress;
            }

            set
            {
                _dataModel.Progress = value;
            }
        }

        public TaskbarItemProgressState ProgressState
        {
            get
            {
                return _dataModel.ProgressState;
            }

            set
            {
                _dataModel.ProgressState = value;
            }
        }

        public double ProgressValue
        {
            get
            {
                return _dataModel.ProgressValue;
            }

            set
            {
                _dataModel.ProgressValue = value;
            }
        }

        public double MaximumProgress
        {
            get
            {
                return _dataModel.MaximumProgress;
            }

            set
            {
                _dataModel.MaximumProgress = value;
            }
        }

        public void ResetProgress(double initial, double maximumProgress)
        {
            _dataModel.ResetProgress(initial, maximumProgress);
        }

        public void ClearProgress()
        {
            _dataModel.ClearProgress();
        }

        public void IncrementProgress()
        {
            _dataModel.IncrementProgress();
        }

        public MyObjectBuilder_Checkpoint Checkpoint
        {
            get { return ActiveWorld.Checkpoint; }
        }

        /// <summary>
        /// Read in current 'world' color Palette.
        /// </summary>
        public int[] CreativeModeColors
        {
            get
            {
                return _dataModel.CreativeModeColors;
            }

            set
            {
                _dataModel.CreativeModeColors = value;
            }
        }

        #endregion
    }
}