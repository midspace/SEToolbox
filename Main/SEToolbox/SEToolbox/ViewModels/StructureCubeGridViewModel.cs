namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using SEToolbox.Views;

    public class StructureCubeGridViewModel : StructureBaseViewModel<StructureCubeGridModel>
    {
        #region fields

        private readonly IDialogService _dialogService;
        private readonly Func<IColorDialog> _colorDialogFactory;
        private Lazy<ObservableCollection<CubeItemViewModel>> _cubeList;
        private ObservableCollection<CubeItemViewModel> _selections;
        private CubeItemViewModel _selectedCubeItem;
        private string[] _filerView;

        #endregion

        #region ctor

        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IColorDialog>)
        {
            Selections = new ObservableCollection<CubeItemViewModel>();
        }

        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel, IDialogService dialogService, Func<IColorDialog> colorDialogFactory)
            : base(parentViewModel, dataModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(colorDialogFactory != null);

            _dialogService = dialogService;
            _colorDialogFactory = colorDialogFactory;

            Func<CubeItemModel, CubeItemViewModel> viewModelCreator = model => new CubeItemViewModel(this, model);
            Func<ObservableCollection<CubeItemViewModel>> collectionCreator =
                () => new ObservableViewModelCollection<CubeItemViewModel, CubeItemModel>(dataModel.CubeList, viewModelCreator);
            _cubeList = new Lazy<ObservableCollection<CubeItemViewModel>>(collectionCreator);

            DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "CubeList")
                {
                    collectionCreator.Invoke();
                    _cubeList = new Lazy<ObservableCollection<CubeItemViewModel>>(collectionCreator);
                }
                // Will bubble property change events from the Model to the ViewModel.
                OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

        public ICommand OptimizeObjectCommand
        {
            get { return new DelegateCommand(OptimizeObjectExecuted, OptimizeObjectCanExecute); }
        }

        public ICommand RepairObjectCommand
        {
            get { return new DelegateCommand(RepairObjectExecuted, RepairObjectCanExecute); }
        }

        public ICommand ResetVelocityCommand
        {
            get { return new DelegateCommand(ResetVelocityExecuted, ResetVelocityCanExecute); }
        }

        public ICommand ReverseVelocityCommand
        {
            get { return new DelegateCommand(ReverseVelocityExecuted, ReverseVelocityCanExecute); }
        }

        public ICommand MaxVelocityAtPlayerCommand
        {
            get { return new DelegateCommand(MaxVelocityAtPlayerExecuted, MaxVelocityAtPlayerCanExecute); }
        }

        public ICommand ConvertCommand
        {
            get { return new DelegateCommand(ConvertExecuted, ConvertCanExecute); }
        }

        public ICommand ConvertToHeavyArmorCommand
        {
            get { return new DelegateCommand(ConvertToHeavyArmorExecuted, ConvertToHeavyArmorCanExecute); }
        }

        public ICommand ConvertToLightArmorCommand
        {
            get { return new DelegateCommand(ConvertToLightArmorExecuted, ConvertToLightArmorCanExecute); }
        }

        public ICommand FrameworkCommand
        {
            get { return new DelegateCommand(FrameworkExecuted, FrameworkCanExecute); }
        }

        public ICommand ConvertToFrameworkCommand
        {
            get { return new DelegateCommand<double>(ConvertToFrameworkExecuted, ConvertToFrameworkCanExecute); }
        }

        public ICommand ConvertToStationCommand
        {
            get { return new DelegateCommand(ConvertToStationExecuted, ConvertToStationCanExecute); }
        }

        public ICommand ReorientStationCommand
        {
            get { return new DelegateCommand(ReorientStationExecuted, ReorientStationCanExecute); }
        }

        public ICommand RotateCubesYawPositiveCommand
        {
            get { return new DelegateCommand(RotateCubesYawPositiveExecuted, RotateCubesYawPositiveCanExecute); }
        }

        public ICommand RotateCubesYawNegativeCommand
        {
            get { return new DelegateCommand(RotateCubesYawNegativeExecuted, RotateCubesYawNegativeCanExecute); }
        }

        public ICommand RotateCubesPitchPositiveCommand
        {
            get { return new DelegateCommand(RotateCubesPitchPositiveExecuted, RotateCubesPitchPositiveCanExecute); }
        }

        public ICommand RotateCubesPitchNegativeCommand
        {
            get { return new DelegateCommand(RotateCubesPitchNegativeExecuted, RotateCubesPitchNegativeCanExecute); }
        }

        public ICommand RotateCubesRollPositiveCommand
        {
            get { return new DelegateCommand(RotateCubesRollPositiveExecuted, RotateCubesRollPositiveCanExecute); }
        }

        public ICommand RotateCubesRollNegativeCommand
        {
            get { return new DelegateCommand(RotateCubesRollNegativeExecuted, RotateCubesRollNegativeCanExecute); }
        }

        public ICommand ConvertToShipCommand
        {
            get { return new DelegateCommand(ConvertToShipExecuted, ConvertToShipCanExecute); }
        }

        public ICommand ConvertToCornerArmorCommand
        {
            get { return new DelegateCommand(ConvertToCornerArmorExecuted, ConvertToCornerArmorCanExecute); }
        }

        public ICommand ConvertToRoundArmorCommand
        {
            get { return new DelegateCommand(ConvertToRoundArmorExecuted, ConvertToRoundArmorCanExecute); }
        }

        public ICommand ConvertLadderToPassageCommand
        {
            get { return new DelegateCommand(ConvertLadderToPassageExecuted, ConvertLadderToPassageCanExecute); }
        }

        public ICommand MirrorStructureByPlaneCommand
        {
            get { return new DelegateCommand(MirrorStructureByPlaneExecuted, MirrorStructureByPlaneCanExecute); }
        }

        public ICommand MirrorStructureGuessOddCommand
        {
            get { return new DelegateCommand(MirrorStructureGuessOddExecuted, MirrorStructureGuessOddCanExecute); }
        }

        public ICommand MirrorStructureGuessEvenCommand
        {
            get { return new DelegateCommand(MirrorStructureGuessEvenExecuted, MirrorStructureGuessEvenCanExecute); }
        }

        public ICommand CopyDetailCommand
        {
            get { return new DelegateCommand(CopyDetailExecuted, CopyDetailCanExecute); }
        }

        public ICommand FilterStartCommand
        {
            get { return new DelegateCommand(FilterStartExecuted, FilterStartCanExecute); }
        }

        public ICommand FilterTabStartCommand
        {
            get { return new DelegateCommand(FilterTabStartExecuted, FilterTabStartCanExecute); }
        }

        public ICommand FilterClearCommand
        {
            get { return new DelegateCommand(FilterClearExecuted, FilterClearCanExecute); }
        }

        public ICommand DeleteCubesCommand
        {
            get { return new DelegateCommand(DeleteCubesExecuted, DeleteCubesCanExecute); }
        }

        public ICommand ReplaceCubesCommand
        {
            get { return new DelegateCommand(ReplaceCubesExecuted, ReplaceCubesCanExecute); }
        }

        public ICommand ColorCubesCommand
        {
            get { return new DelegateCommand(ColorCubesExecuted, ColorCubesCanExecute); }
        }

        public ICommand FrameworkCubesCommand
        {
            get { return new DelegateCommand(FrameworkCubesExecuted, FrameworkCubesCanExecute); }
        }

        #endregion

        #region Properties

        public ObservableCollection<CubeItemViewModel> CubeList
        {
            get { return _cubeList.Value; }
        }

        public ObservableCollection<CubeItemViewModel> Selections
        {
            get { return _selections; }

            set
            {
                if (value != _selections)
                {
                    _selections = value;
                    RaisePropertyChanged(() => Selections);
                }
            }
        }

        public CubeItemViewModel SelectedCubeItem
        {
            get { return _selectedCubeItem; }

            set
            {
                if (value != _selectedCubeItem)
                {
                    _selectedCubeItem = value;
                    RaisePropertyChanged(() => SelectedCubeItem);
                }
            }
        }

        protected new StructureCubeGridModel DataModel
        {
            get { return base.DataModel as StructureCubeGridModel; }
        }

        public bool IsDamaged
        {
            get { return DataModel.IsDamaged; }
        }

        public int DamageCount
        {
            get { return DataModel.DamageCount; }
        }

        public MyCubeSize GridSize
        {
            get { return DataModel.GridSize; }
            set { DataModel.GridSize = value; }
        }

        public bool IsStatic
        {
            get { return DataModel.IsStatic; }
            set { DataModel.IsStatic = value; }
        }

        public bool Dampeners
        {
            get { return DataModel.Dampeners; }

            set
            {
                DataModel.Dampeners = value;
                MainViewModel.IsModified = true;
            }
        }

        public Point3D Min
        {
            get { return DataModel.Min; }
            set { DataModel.Min = value; }
        }

        public Point3D Max
        {
            get { return DataModel.Max; }
            set { DataModel.Max = value; }
        }

        public Vector3D Scale
        {
            get { return DataModel.Scale; }
            set { DataModel.Scale = value; }
        }

        public BindableSize3DModel Size
        {
            get { return new BindableSize3DModel(DataModel.Size); }
        }

        public bool IsPiloted
        {
            get { return DataModel.IsPiloted; }
        }

        public double LinearVelocity
        {
            get { return DataModel.LinearVelocity; }
        }

        public TimeSpan TimeToProduce
        {
            get { return DataModel.TimeToProduce; }
            set { DataModel.TimeToProduce = value; }
        }

        public string CockpitOrientation
        {
            get { return DataModel.CockpitOrientation; }
        }

        public List<CubeAssetModel> CubeAssets
        {
            get { return DataModel.CubeAssets; }
            set { DataModel.CubeAssets = value; }
        }

        public List<CubeAssetModel> ComponentAssets
        {
            get { return DataModel.ComponentAssets; }
            set { DataModel.ComponentAssets = value; }
        }

        public List<OreAssetModel> IngotAssets
        {
            get { return DataModel.IngotAssets; }
            set { DataModel.IngotAssets = value; }
        }

        public List<OreAssetModel> OreAssets
        {
            get { return DataModel.OreAssets; }
            set { DataModel.OreAssets = value; }
        }

        public string ActiveComponentFilter
        {
            get { return DataModel.ActiveComponentFilter; }
            set { DataModel.ActiveComponentFilter = value; }
        }

        public string ComponentFilter
        {
            get { return DataModel.ComponentFilter; }
            set { DataModel.ComponentFilter = value; }
        }

        public bool IsConstructionNotReady
        {
            get { return DataModel.IsConstructionNotReady; }
            set { DataModel.IsConstructionNotReady = value; }
        }

        public bool IsSubsSystemNotReady
        {
            get { return DataModel.IsSubsSystemNotReady; }
            set { DataModel.IsSubsSystemNotReady = value; }
        }

        #endregion

        #region command methods

        public bool OptimizeObjectCanExecute()
        {
            return true;
        }

        public void OptimizeObjectExecuted()
        {
            MainViewModel.OptimizeModel(this);
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RepairObjectCanExecute()
        {
            return IsDamaged;
        }

        public void RepairObjectExecuted()
        {
            DataModel.RepairAllDamage();
            MainViewModel.IsModified = true;
        }

        public bool ResetVelocityCanExecute()
        {
            return DataModel.LinearVelocity != 0f || DataModel.AngularSpeed != 0f;
        }

        public void ResetVelocityExecuted()
        {
            DataModel.ResetVelocity();
            MainViewModel.IsModified = true;
        }

        public bool ReverseVelocityCanExecute()
        {
            return DataModel.LinearVelocity != 0f || DataModel.AngularSpeed != 0f;
        }

        public void ReverseVelocityExecuted()
        {
            DataModel.ReverseVelocity();
            MainViewModel.IsModified = true;
        }

        public bool MaxVelocityAtPlayerCanExecute()
        {
            return MainViewModel.ThePlayerCharacter != null;
        }

        public void MaxVelocityAtPlayerExecuted()
        {

            var position = MainViewModel.ThePlayerCharacter.PositionAndOrientation.Value.Position;
            DataModel.MaxVelocityAtPlayer(position);
            MainViewModel.IsModified = true;
        }

        public bool ConvertCanExecute()
        {
            return true;
        }

        public void ConvertExecuted()
        {
        }

        public bool ConvertToHeavyArmorCanExecute()
        {
            return true;
        }

        public void ConvertToHeavyArmorExecuted()
        {
            if (DataModel.ConvertFromLightToHeavyArmor())
            {
                MainViewModel.IsModified = true;
            }
        }

        public bool ConvertToLightArmorCanExecute()
        {
            return true;
        }

        public void ConvertToLightArmorExecuted()
        {
            if (DataModel.ConvertFromHeavyToLightArmor())
            {
                MainViewModel.IsModified = true;
            }
        }

        public bool FrameworkCanExecute()
        {
            return true;
        }

        public void FrameworkExecuted()
        {
            // placeholder for menu only.
        }

        public bool ConvertToFrameworkCanExecute(double value)
        {
            return true;
        }

        public void ConvertToFrameworkExecuted(double value)
        {
            DataModel.ConvertToFramework((float)value);
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool ConvertToStationCanExecute()
        {
            return !DataModel.IsStatic && DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToStationExecuted()
        {
            DataModel.ConvertToStation();
            MainViewModel.IsModified = true;
        }

        public bool ReorientStationCanExecute()
        {
            return DataModel.GridSize == MyCubeSize.Large;
        }

        public void ReorientStationExecuted()
        {
            DataModel.ReorientStation();
            MainViewModel.IsModified = true;
        }

        public bool RotateCubesYawPositiveCanExecute()
        {
            return true;
        }

        public void RotateCubesPitchPositiveExecuted()
        {
            // +90 around X
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, VRageMath.MathHelper.PiOver2, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesPitchNegativeCanExecute()
        {
            return true;
        }

        public void RotateCubesPitchNegativeExecuted()
        {
            // -90 around X
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, -VRageMath.MathHelper.PiOver2, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesRollPositiveCanExecute()
        {
            return true;
        }

        public void RotateCubesYawPositiveExecuted()
        {
            // +90 around Y
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(VRageMath.MathHelper.PiOver2, 0, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesYawNegativeCanExecute()
        {
            return true;
        }

        public void RotateCubesYawNegativeExecuted()
        {
            // -90 around Y
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(-VRageMath.MathHelper.PiOver2, 0, 0));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesPitchPositiveCanExecute()
        {
            return true;
        }

        public void RotateCubesRollPositiveExecuted()
        {
            // +90 around Z
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, VRageMath.MathHelper.PiOver2));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool RotateCubesRollNegativeCanExecute()
        {
            return true;
        }

        public void RotateCubesRollNegativeExecuted()
        {
            // -90 around Z
            DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, -VRageMath.MathHelper.PiOver2));
            MainViewModel.IsModified = true;
            IsSubsSystemNotReady = true;
            DataModel.InitializeAsync();
        }

        public bool ConvertToShipCanExecute()
        {
            return DataModel.IsStatic && DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToShipExecuted()
        {
            DataModel.ConvertToShip();
            MainViewModel.IsModified = true;
        }

        public bool ConvertToCornerArmorCanExecute()
        {
            return DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToCornerArmorExecuted()
        {
            if (DataModel.ConvertToCornerArmor())
            {
                MainViewModel.IsModified = true;
            }
        }

        public bool ConvertToRoundArmorCanExecute()
        {
            return DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToRoundArmorExecuted()
        {
            if (DataModel.ConvertToRoundArmor())
            {
                MainViewModel.IsModified = true;
            }
        }

        public bool ConvertLadderToPassageCanExecute()
        {
            return DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertLadderToPassageExecuted()
        {
            if (DataModel.ConvertLadderToPassage())
            {
                MainViewModel.IsModified = true;
            }
        }

        public bool MirrorStructureByPlaneCanExecute()
        {
            return true;
        }

        public void MirrorStructureByPlaneExecuted()
        {
            MainViewModel.IsBusy = true;
            if (DataModel.MirrorModel(true, false))
            {
                MainViewModel.IsModified = true;
                IsSubsSystemNotReady = true;
                IsConstructionNotReady = true;
                DataModel.InitializeAsync();
            }
            MainViewModel.IsBusy = false;
        }

        public bool MirrorStructureGuessOddCanExecute()
        {
            return true;
        }

        public void MirrorStructureGuessOddExecuted()
        {
            MainViewModel.IsBusy = true;
            if (DataModel.MirrorModel(false, true))
            {
                MainViewModel.IsModified = true;
                IsSubsSystemNotReady = true;
                IsConstructionNotReady = true;
                DataModel.InitializeAsync();
            }
            MainViewModel.IsBusy = false;
        }

        public bool MirrorStructureGuessEvenCanExecute()
        {
            return true;
        }

        public void MirrorStructureGuessEvenExecuted()
        {
            MainViewModel.IsBusy = true;
            if (DataModel.MirrorModel(false, false))
            {
                MainViewModel.IsModified = true;
                IsSubsSystemNotReady = true;
                IsConstructionNotReady = true;
                DataModel.InitializeAsync();
            }
            MainViewModel.IsBusy = false;
        }

        public bool CopyDetailCanExecute()
        {
            return true;
        }

        public void CopyDetailExecuted()
        {
            var cubes = new StringBuilder();
            if (CubeAssets != null)
            {
                foreach (var mat in CubeAssets)
                {
                    cubes.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.00} Kg\t{3:hh\\:mm\\:ss\\.ff}\r\n", mat.FriendlyName, mat.Count, mat.Mass, mat.Time);
                }
            }

            var components = new StringBuilder();
            if (ComponentAssets != null)
            {
                foreach (var mat in ComponentAssets)
                {
                    components.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0} Kg\t{3:#,##0.00} L\t{4:hh\\:mm\\:ss\\.ff}\r\n", mat.FriendlyName, mat.Count, mat.Mass, mat.Volume, mat.Time);
                }
            }

            var ingots = new StringBuilder();
            if (IngotAssets != null)
            {
                foreach (var mat in IngotAssets)
                {
                    ingots.AppendFormat("{0}\t{1:#,##0.00}\t{2:#,##0.00} Kg\t{3:#,##0.00} L\t{4:hh\\:mm\\:ss\\.ff}\r\n", mat.FriendlyName, mat.Amount, mat.Mass, mat.Volume, mat.Time);
                }
            }

            var ores = new StringBuilder();
            if (OreAssets != null)
            {
                foreach (var mat in OreAssets)
                {
                    ores.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.00} Kg\t{3:#,##0.00} L\r\n", mat.FriendlyName, mat.Amount, mat.Mass, mat.Volume);
                }
            }

            var detail = string.Format(Properties.Resources.CubeDetail,
                DisplayName,
                ClassType,
                IsPiloted,
                DamageCount,
                LinearVelocity,
                PlayerDistance,
                Scale.X, Scale.Y, Scale.Z,
                Size.Width, Size.Height, Size.Depth,
                Mass,
                BlockCount,
                PositionAndOrientation.Value.Position.X, PositionAndOrientation.Value.Position.Y, PositionAndOrientation.Value.Position.Z,
                TimeToProduce,
                cubes.ToString(),
                components.ToString(),
                ingots.ToString(),
                ores.ToString());

            try
            {
                Clipboard.Clear();
                Clipboard.SetText(detail);
            }
            catch
            {
                // Ignore exception which may be generated by a Remote desktop session where Clipboard access has not been granted.
            }
        }

        public bool FilterStartCanExecute()
        {
            return ActiveComponentFilter != ComponentFilter;
        }

        public void FilterStartExecuted()
        {
            ActiveComponentFilter = ComponentFilter;
            ApplyCubeFilter();
        }

        public bool FilterTabStartCanExecute()
        {
            return true;
        }

        public void FilterTabStartExecuted()
        {
            ActiveComponentFilter = ComponentFilter;
            ApplyCubeFilter();
            FrameworkExtension.FocusedElementMoveFocus();
        }

        public bool FilterClearCanExecute()
        {
            return !string.IsNullOrEmpty(ComponentFilter);
        }

        public void FilterClearExecuted()
        {
            ComponentFilter = string.Empty;
            ActiveComponentFilter = ComponentFilter;
            ApplyCubeFilter();
        }

        public bool DeleteCubesCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void DeleteCubesExecuted()
        {
            IsBusy = true;

            MainViewModel.ResetProgress(0, Selections.Count);

            while (Selections.Count > 0)
            {
                MainViewModel.Progress++;
                var cube = Selections[0];
                if (DataModel.CubeGrid.CubeBlocks.Remove(cube.Cube))
                    DataModel.CubeList.Remove(cube.DataModel);
            }

            MainViewModel.ClearProgress();
            IsBusy = false;
        }

        public bool ReplaceCubesCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void ReplaceCubesExecuted()
        {
            var model = new SelectCubeModel();
            var loadVm = new SelectCubeViewModel(this, model);
            model.Load(GridSize, SelectedCubeItem.Cube.TypeId, SelectedCubeItem.SubtypeId);
            var result = _dialogService.ShowDialog<WindowSelectCube>(this, loadVm);
            if (result == true)
            {
                MainViewModel.IsBusy = true;
                var contentPath = ToolboxUpdater.GetApplicationContentPath();
                var change = false;
                MainViewModel.ResetProgress(0, Selections.Count);

                foreach (var cube in Selections)
                {
                    MainViewModel.Progress++;
                    if (cube.TypeId != model.CubeItem.TypeId || cube.SubtypeId != model.CubeItem.SubtypeId)
                    {
                        var idx = DataModel.CubeGrid.CubeBlocks.IndexOf(cube.Cube);
                        DataModel.CubeGrid.CubeBlocks.RemoveAt(idx);

                        var cubeDefinition = SpaceEngineersApi.GetCubeDefinition(model.CubeItem.TypeId, GridSize, model.CubeItem.SubtypeId);
                        var newCube = cube.CreateCube(model.CubeItem.TypeId, model.CubeItem.SubtypeId, cubeDefinition, DataModel.Settings);
                        cube.TextureFile = SpaceEngineersCore.GetDataPathOrDefault(cubeDefinition.Icon, Path.Combine(contentPath, cubeDefinition.Icon));
                        DataModel.CubeGrid.CubeBlocks.Insert(idx, newCube);

                        change = true;
                    }
                }

                MainViewModel.ClearProgress();
                if (change)
                {
                    MainViewModel.IsModified = true;
                }
                MainViewModel.IsBusy = false;
            }
        }

        public bool ColorCubesCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void ColorCubesExecuted()
        {
            var colorDialog = _colorDialogFactory();
            colorDialog.FullOpen = true;
            colorDialog.BrushColor = SelectedCubeItem.Color as System.Windows.Media.SolidColorBrush;
            colorDialog.CustomColors = MainViewModel.CreativeModeColors;

            var result = _dialogService.ShowColorDialog(OwnerViewModel, colorDialog);

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                MainViewModel.IsBusy = true;
                MainViewModel.ResetProgress(0, Selections.Count);

                foreach (var cube in Selections)
                {
                    MainViewModel.Progress++;
                    if (colorDialog.DrawingColor.HasValue)
                        cube.UpdateColor(colorDialog.DrawingColor.Value.ToSandboxHsvColor());
                }

                MainViewModel.ClearProgress();
                MainViewModel.IsModified = true;
                MainViewModel.IsBusy = false;
            }

            MainViewModel.CreativeModeColors = colorDialog.CustomColors;
        }

        public bool FrameworkCubesCanExecute()
        {
            return SelectedCubeItem != null;
        }

        public void FrameworkCubesExecuted()
        {
            var model = new FrameworkBuildModel { BuildPercent = SelectedCubeItem.BuildPercent * 100 };
            var loadVm = new FrameworkBuildViewModel(this, model);
            var result = _dialogService.ShowDialog<WindowFrameworkBuild>(this, loadVm);
            if (result == true)
            {
                MainViewModel.IsBusy = true;
                MainViewModel.ResetProgress(0, Selections.Count);

                foreach (var cube in Selections)
                {
                    MainViewModel.Progress++;
                    cube.UpdateBuildPercent(model.BuildPercent.Value / 100);
                }

                MainViewModel.ClearProgress();
                MainViewModel.IsModified = true;
                MainViewModel.IsBusy = false;
            }
        }

        #endregion

        #region methods

        private void ApplyCubeFilter()
        {
            // Prepare filter beforehand.
            if (string.IsNullOrEmpty(ActiveComponentFilter))
                _filerView = new string[0];
            else
                _filerView = ActiveComponentFilter.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();

            var view = (CollectionView)CollectionViewSource.GetDefaultView(CubeList);
            view.Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            if (_filerView.Length == 0)
                return true;

            var cube = (CubeItemViewModel)item;
            return _filerView.All(s => cube.FriendlyName.ToLowerInvariant().Contains(s) || cube.ColorText.ToLowerInvariant().Contains(s));
        }

        #endregion
    }
}
