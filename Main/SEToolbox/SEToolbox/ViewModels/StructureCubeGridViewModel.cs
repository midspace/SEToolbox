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
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

    public class StructureCubeGridViewModel : StructureBaseViewModel<StructureCubeGridModel>
    {
        #region fields

        private readonly IDialogService _dialogService;
        private readonly Func<IColorDialog> _colorDialogFactory;
        private ObservableCollection<CubeItemModel> _selections;
        private string[] _filerView;

        #endregion

        #region ctor

        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IColorDialog>)
        {
            this.CubeSelections = new ObservableCollection<CubeItemModel>();
        }

        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel, IDialogService dialogService, Func<IColorDialog> colorDialogFactory)
            : base(parentViewModel, dataModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(colorDialogFactory != null);

            this._dialogService = dialogService;
            this._colorDialogFactory = colorDialogFactory;

            this.DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

        public ICommand OptimizeObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(OptimizeObjectExecuted), new Func<bool>(OptimizeObjectCanExecute));
            }
        }

        public ICommand RepairObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(RepairObjectExecuted), new Func<bool>(RepairObjectCanExecute));
            }
        }

        public ICommand ResetVelocityCommand
        {
            get
            {
                return new DelegateCommand(new Action(ResetVelocityExecuted), new Func<bool>(ResetVelocityCanExecute));
            }
        }

        public ICommand ReverseVelocityCommand
        {
            get
            {
                return new DelegateCommand(new Action(ReverseVelocityExecuted), new Func<bool>(ReverseVelocityCanExecute));
            }
        }

        public ICommand MaxVelocityAtPlayerCommand
        {
            get
            {
                return new DelegateCommand(new Action(MaxVelocityAtPlayerExecuted), new Func<bool>(MaxVelocityAtPlayerCanExecute));
            }
        }

        public ICommand ConvertCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertExecuted), new Func<bool>(ConvertCanExecute));
            }
        }

        public ICommand ConvertToHeavyArmorCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertToHeavyArmorExecuted), new Func<bool>(ConvertToHeavyArmorCanExecute));
            }
        }

        public ICommand ConvertToLightArmorCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertToLightArmorExecuted), new Func<bool>(ConvertToLightArmorCanExecute));
            }
        }

        public ICommand FrameworkCommand
        {
            get
            {
                return new DelegateCommand(new Action(FrameworkExecuted), new Func<bool>(FrameworkCanExecute));
            }
        }

        public ICommand ConvertToFrameworkCommand
        {
            get
            {
                return new DelegateCommand<double>(new Action<double>(ConvertToFrameworkExecuted), new Func<double, bool>(ConvertToFrameworkCanExecute));
            }
        }

        public ICommand ConvertToStationCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertToStationExecuted), new Func<bool>(ConvertToStationCanExecute));
            }
        }

        public ICommand ReorientStationCommand
        {
            get
            {
                return new DelegateCommand(new Action(ReorientStationExecuted), new Func<bool>(ReorientStationCanExecute));
            }
        }

        public ICommand RotateCubesYawPositiveCommand
        {
            get
            {
                return new DelegateCommand(new Action(RotateCubesYawPositiveExecuted), new Func<bool>(RotateCubesYawPositiveCanExecute));
            }
        }

        public ICommand RotateCubesYawNegativeCommand
        {
            get
            {
                return new DelegateCommand(new Action(RotateCubesYawNegativeExecuted), new Func<bool>(RotateCubesYawNegativeCanExecute));
            }
        }

        public ICommand RotateCubesPitchPositiveCommand
        {
            get
            {
                return new DelegateCommand(new Action(RotateCubesPitchPositiveExecuted), new Func<bool>(RotateCubesPitchPositiveCanExecute));
            }
        }

        public ICommand RotateCubesPitchNegativeCommand
        {
            get
            {
                return new DelegateCommand(new Action(RotateCubesPitchNegativeExecuted), new Func<bool>(RotateCubesPitchNegativeCanExecute));
            }
        }

        public ICommand RotateCubesRollPositiveCommand
        {
            get
            {
                return new DelegateCommand(new Action(RotateCubesRollPositiveExecuted), new Func<bool>(RotateCubesRollPositiveCanExecute));
            }
        }

        public ICommand RotateCubesRollNegativeCommand
        {
            get
            {
                return new DelegateCommand(new Action(RotateCubesRollNegativeExecuted), new Func<bool>(RotateCubesRollNegativeCanExecute));
            }
        }

        public ICommand ConvertToShipCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertToShipExecuted), new Func<bool>(ConvertToShipCanExecute));
            }
        }

        public ICommand ConvertToCornerArmorCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertToCornerArmorExecuted), new Func<bool>(ConvertToCornerArmorCanExecute));
            }
        }

        public ICommand ConvertToRoundArmorCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertToRoundArmorExecuted), new Func<bool>(ConvertToRoundArmorCanExecute));
            }
        }

        public ICommand ConvertLadderToPassageCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertLadderToPassageExecuted), new Func<bool>(ConvertLadderToPassageCanExecute));
            }
        }

        public ICommand MirrorStructureByPlaneCommand
        {
            get
            {
                return new DelegateCommand(new Action(MirrorStructureByPlaneExecuted), new Func<bool>(MirrorStructureByPlaneCanExecute));
            }
        }

        public ICommand MirrorStructureGuessOddCommand
        {
            get
            {
                return new DelegateCommand(new Action(MirrorStructureGuessOddExecuted), new Func<bool>(MirrorStructureGuessOddCanExecute));
            }
        }

        public ICommand MirrorStructureGuessEvenCommand
        {
            get
            {
                return new DelegateCommand(new Action(MirrorStructureGuessEvenExecuted), new Func<bool>(MirrorStructureGuessEvenCanExecute));
            }
        }

        public ICommand CopyDetailCommand
        {
            get
            {
                return new DelegateCommand(new Action(CopyDetailExecuted), new Func<bool>(CopyDetailCanExecute));
            }
        }

        public ICommand FilterStartCommand
        {
            get
            {
                return new DelegateCommand(new Action(FilterStartExecuted), new Func<bool>(FilterStartCanExecute));
            }
        }

        public ICommand FilterTabStartCommand
        {
            get
            {
                return new DelegateCommand(new Action(FilterTabStartExecuted), new Func<bool>(FilterTabStartCanExecute));
            }
        }

        public ICommand FilterClearCommand
        {
            get
            {
                return new DelegateCommand(new Action(FilterClearExecuted), new Func<bool>(FilterClearCanExecute));
            }
        }

        public ICommand DeleteCubesCommand
        {
            get
            {
                return new DelegateCommand(new Action(DeleteCubesExecuted), new Func<bool>(DeleteCubesCanExecute));
            }
        }

        public ICommand ReplaceCubesCommand
        {
            get
            {
                return new DelegateCommand(new Action(ReplaceCubesExecuted), new Func<bool>(ReplaceCubesCanExecute));
            }
        }

        public ICommand ColorCubesCommand
        {
            get
            {
                return new DelegateCommand(new Action(ColorCubesExecuted), new Func<bool>(ColorCubesCanExecute));
            }
        }

        public ICommand FrameworkCubesCommand
        {
            get
            {
                return new DelegateCommand(new Action(FrameworkCubesExecuted), new Func<bool>(FrameworkCubesCanExecute));
            }
        }

        #endregion

        #region Properties

        protected new StructureCubeGridModel DataModel
        {
            get
            {
                return base.DataModel as StructureCubeGridModel;
            }
        }

        public bool IsDamaged
        {
            get
            {
                return this.DataModel.IsDamaged;
            }
        }

        public int DamageCount
        {
            get
            {
                return this.DataModel.DamageCount;
            }
        }

        public Sandbox.Common.ObjectBuilders.MyCubeSize GridSize
        {
            get
            {
                return this.DataModel.GridSize;
            }

            set
            {
                this.DataModel.GridSize = value;
            }
        }

        public bool IsStatic
        {
            get
            {
                return this.DataModel.IsStatic;
            }

            set
            {
                this.DataModel.IsStatic = value;
            }
        }

        public bool Dampeners
        {
            get
            {
                return this.DataModel.Dampeners;
            }

            set
            {
                this.DataModel.Dampeners = value;
                this.MainViewModel.IsModified = true;
            }
        }

        public Point3D Min
        {
            get
            {
                return this.DataModel.Min;
            }

            set
            {
                this.DataModel.Min = value;
            }
        }

        public Point3D Max
        {
            get
            {
                return this.DataModel.Max;
            }

            set
            {
                this.DataModel.Max = value;
            }
        }

        public Vector3D Scale
        {
            get
            {
                return this.DataModel.Scale;
            }

            set
            {
                this.DataModel.Scale = value;
            }
        }

        public BindableSize3DModel Size
        {
            get
            {
                return new BindableSize3DModel(this.DataModel.Size);
            }
        }

        public bool IsPiloted
        {
            get
            {
                return this.DataModel.IsPiloted;
            }
        }

        public double LinearVelocity
        {
            get
            {
                return this.DataModel.LinearVelocity;
            }
        }

        public double Mass
        {
            get
            {
                return this.DataModel.Mass;
            }
        }

        public TimeSpan TimeToProduce
        {
            get
            {
                return this.DataModel.TimeToProduce;
            }

            set
            {
                this.DataModel.TimeToProduce = value;
            }
        }

        public int BlockCount
        {
            get { return this.DataModel.BlockCount; }
        }

        public string CockpitOrientation
        {
            get { return this.DataModel.CockpitOrientation; }
        }

        public List<CubeAssetModel> CubeAssets
        {
            get
            {
                return this.DataModel.CubeAssets;
            }

            set
            {
                this.DataModel.CubeAssets = value;
            }
        }

        public List<CubeAssetModel> ComponentAssets
        {
            get
            {
                return this.DataModel.ComponentAssets;
            }

            set
            {
                this.DataModel.ComponentAssets = value;
            }
        }

        public List<OreAssetModel> IngotAssets
        {
            get
            {
                return this.DataModel.IngotAssets;
            }

            set
            {
                this.DataModel.IngotAssets = value;
            }
        }

        public List<OreAssetModel> OreAssets
        {
            get
            {
                return this.DataModel.OreAssets;
            }

            set
            {
                this.DataModel.OreAssets = value;
            }
        }

        public string ActiveComponentFilter
        {
            get
            {
                return this.DataModel.ActiveComponentFilter;
            }

            set
            {
                this.DataModel.ActiveComponentFilter = value;
            }
        }

        public string ComponentFilter
        {
            get
            {
                return this.DataModel.ComponentFilter;
            }

            set
            {
                this.DataModel.ComponentFilter = value;
            }
        }

        public ObservableCollection<CubeItemModel> CubeList
        {
            get
            {
                return this.DataModel.CubeList;
            }
        }

        public ObservableCollection<CubeItemModel> CubeSelections
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
                    this.RaisePropertyChanged(() => CubeSelections);
                }
            }
        }

        public CubeItemModel SelectedCubeItem
        {
            get
            {
                return this.DataModel.SelectedCubeItem;
            }

            set
            {
                this.DataModel.SelectedCubeItem = value;
            }
        }

        public bool IsConstructionNotReady
        {
            get
            {
                return this.DataModel.IsConstructionNotReady;
            }

            set
            {
                this.DataModel.IsConstructionNotReady = value;
            }
        }

        public bool IsSubsSystemNotReady
        {
            get
            {
                return this.DataModel.IsSubsSystemNotReady;
            }

            set
            {
                this.DataModel.IsSubsSystemNotReady = value;
            }
        }

        #endregion

        #region command methods

        public bool OptimizeObjectCanExecute()
        {
            return true;
        }

        public void OptimizeObjectExecuted()
        {
            this.MainViewModel.OptimizeModel(this);
            this.IsSubsSystemNotReady = true;
            this.DataModel.InitializeAsync();
        }

        public bool RepairObjectCanExecute()
        {
            return this.IsDamaged;
        }

        public void RepairObjectExecuted()
        {
            this.DataModel.RepairAllDamage();
            this.MainViewModel.IsModified = true;
        }

        public bool ResetVelocityCanExecute()
        {
            return this.DataModel.LinearVelocity != 0f || this.DataModel.AngularSpeed != 0f;
        }

        public void ResetVelocityExecuted()
        {
            this.DataModel.ResetVelocity();
            this.MainViewModel.IsModified = true;
        }

        public bool ReverseVelocityCanExecute()
        {
            return this.DataModel.LinearVelocity != 0f || this.DataModel.AngularSpeed != 0f;
        }

        public void ReverseVelocityExecuted()
        {
            this.DataModel.ReverseVelocity();
            this.MainViewModel.IsModified = true;
        }

        public bool MaxVelocityAtPlayerCanExecute()
        {
            return this.MainViewModel.ThePlayerCharacter != null;
        }

        public void MaxVelocityAtPlayerExecuted()
        {

            var position = this.MainViewModel.ThePlayerCharacter.PositionAndOrientation.Value.Position;
            this.DataModel.MaxVelocityAtPlayer(position);
            this.MainViewModel.IsModified = true;
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
            if (this.DataModel.ConvertFromLightToHeavyArmor())
            {
                this.MainViewModel.IsModified = true;
            }
        }

        public bool ConvertToLightArmorCanExecute()
        {
            return true;
        }

        public void ConvertToLightArmorExecuted()
        {
            if (this.DataModel.ConvertFromHeavyToLightArmor())
            {
                this.MainViewModel.IsModified = true;
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
            this.DataModel.ConvertToFramework((float)value);
            this.MainViewModel.IsModified = true;
            this.IsSubsSystemNotReady = true;
            this.DataModel.InitializeAsync();
        }

        public bool ConvertToStationCanExecute()
        {
            return !this.DataModel.IsStatic && this.DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToStationExecuted()
        {
            this.DataModel.ConvertToStation();
            this.MainViewModel.IsModified = true;
        }

        public bool ReorientStationCanExecute()
        {
            return this.DataModel.GridSize == MyCubeSize.Large;
        }

        public void ReorientStationExecuted()
        {
            this.DataModel.ReorientStation();
            this.MainViewModel.IsModified = true;
        }

        public bool RotateCubesYawPositiveCanExecute()
        {
            return true;
        }

        public void RotateCubesPitchPositiveExecuted()
        {
            // +90 around X
            this.DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, VRageMath.MathHelper.PiOver2, 0));
            this.MainViewModel.IsModified = true;
            this.IsSubsSystemNotReady = true;
            this.DataModel.InitializeAsync();
        }

        public bool RotateCubesPitchNegativeCanExecute()
        {
            return true;
        }

        public void RotateCubesPitchNegativeExecuted()
        {
            // -90 around X
            this.DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, -VRageMath.MathHelper.PiOver2, 0));
            this.MainViewModel.IsModified = true;
            this.IsSubsSystemNotReady = true;
            this.DataModel.InitializeAsync();
        }

        public bool RotateCubesRollPositiveCanExecute()
        {
            return true;
        }

        public void RotateCubesYawPositiveExecuted()
        {
            // +90 around Y
            this.DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(VRageMath.MathHelper.PiOver2, 0, 0));
            this.MainViewModel.IsModified = true;
            this.IsSubsSystemNotReady = true;
            this.DataModel.InitializeAsync();
        }

        public bool RotateCubesYawNegativeCanExecute()
        {
            return true;
        }

        public void RotateCubesYawNegativeExecuted()
        {
            // -90 around Y
            this.DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(-VRageMath.MathHelper.PiOver2, 0, 0));
            this.MainViewModel.IsModified = true;
            this.IsSubsSystemNotReady = true;
            this.DataModel.InitializeAsync();
        }

        public bool RotateCubesPitchPositiveCanExecute()
        {
            return true;
        }

        public void RotateCubesRollPositiveExecuted()
        {
            // +90 around Z
            this.DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, VRageMath.MathHelper.PiOver2));
            this.MainViewModel.IsModified = true;
            this.IsSubsSystemNotReady = true;
            this.DataModel.InitializeAsync();
        }

        public bool RotateCubesRollNegativeCanExecute()
        {
            return true;
        }

        public void RotateCubesRollNegativeExecuted()
        {
            // -90 around Z
            this.DataModel.RotateCubes(VRageMath.Quaternion.CreateFromYawPitchRoll(0, 0, -VRageMath.MathHelper.PiOver2));
            this.MainViewModel.IsModified = true;
            this.IsSubsSystemNotReady = true;
            this.DataModel.InitializeAsync();
        }

        public bool ConvertToShipCanExecute()
        {
            return this.DataModel.IsStatic && this.DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToShipExecuted()
        {
            this.DataModel.ConvertToShip();
            this.MainViewModel.IsModified = true;
        }

        public bool ConvertToCornerArmorCanExecute()
        {
            return this.DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToCornerArmorExecuted()
        {
            if (this.DataModel.ConvertToCornerArmor())
            {
                this.MainViewModel.IsModified = true;
            }
        }

        public bool ConvertToRoundArmorCanExecute()
        {
            return this.DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertToRoundArmorExecuted()
        {
            if (this.DataModel.ConvertToRoundArmor())
            {
                this.MainViewModel.IsModified = true;
            }
        }

        public bool ConvertLadderToPassageCanExecute()
        {
            return this.DataModel.GridSize == MyCubeSize.Large;
        }

        public void ConvertLadderToPassageExecuted()
        {
            if (this.DataModel.ConvertLadderToPassage())
            {
                this.MainViewModel.IsModified = true;
            }
        }

        public bool MirrorStructureByPlaneCanExecute()
        {
            return true;
        }

        public void MirrorStructureByPlaneExecuted()
        {
            this.MainViewModel.IsBusy = true;
            if (this.DataModel.MirrorModel(true, false))
            {
                this.MainViewModel.IsModified = true;
                this.IsSubsSystemNotReady = true;
                this.IsConstructionNotReady = true;
                this.DataModel.InitializeAsync();
            }
            this.MainViewModel.IsBusy = false;
        }

        public bool MirrorStructureGuessOddCanExecute()
        {
            return true;
        }

        public void MirrorStructureGuessOddExecuted()
        {
            this.MainViewModel.IsBusy = true;
            if (this.DataModel.MirrorModel(false, true))
            {
                this.MainViewModel.IsModified = true;
                this.IsSubsSystemNotReady = true;
                this.IsConstructionNotReady = true;
                this.DataModel.InitializeAsync();
            }
            this.MainViewModel.IsBusy = false;
        }

        public bool MirrorStructureGuessEvenCanExecute()
        {
            return true;
        }

        public void MirrorStructureGuessEvenExecuted()
        {
            this.MainViewModel.IsBusy = true;
            if (this.DataModel.MirrorModel(false, false))
            {
                this.MainViewModel.IsModified = true;
                this.IsSubsSystemNotReady = true;
                this.IsConstructionNotReady = true;
                this.DataModel.InitializeAsync();
            }
            this.MainViewModel.IsBusy = false;
        }

        public bool CopyDetailCanExecute()
        {
            return true;
        }

        public void CopyDetailExecuted()
        {
            var cubes = new StringBuilder();
            if (this.CubeAssets != null)
            {
                foreach (var mat in this.CubeAssets)
                {
                    cubes.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.00} Kg\t{3:hh\\:mm\\:ss\\.ff}\r\n", mat.FriendlyName, mat.Count, mat.Mass, mat.Time);
                }
            }

            var components = new StringBuilder();
            if (this.ComponentAssets != null)
            {
                foreach (var mat in this.ComponentAssets)
                {
                    components.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0} Kg\t{3:#,##0.00} L\t{4:hh\\:mm\\:ss\\.ff}\r\n", mat.FriendlyName, mat.Count, mat.Mass, mat.Volume, mat.Time);
                }
            }

            var ingots = new StringBuilder();
            if (this.IngotAssets != null)
            {
                foreach (var mat in this.IngotAssets)
                {
                    ingots.AppendFormat("{0}\t{1:#,##0.00}\t{2:#,##0.00} Kg\t{3:#,##0.00} L\t{4:hh\\:mm\\:ss\\.ff}\r\n", mat.FriendlyName, mat.Amount, mat.Mass, mat.Volume, mat.Time);
                }
            }

            var ores = new StringBuilder();
            if (this.OreAssets != null)
            {
                foreach (var mat in this.OreAssets)
                {
                    ores.AppendFormat("{0}\t{1:#,##0}\t{2:#,##0.00} Kg\t{3:#,##0.00} L\r\n", mat.FriendlyName, mat.Amount, mat.Mass, mat.Volume);
                }
            }

            var detail = string.Format(Properties.Resources.CubeDetail,
                this.ClassType,
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
            catch { }
        }

        public bool FilterStartCanExecute()
        {
            return this.ActiveComponentFilter != this.ComponentFilter;
        }

        public void FilterStartExecuted()
        {
            this.ActiveComponentFilter = this.ComponentFilter;
            ApplyCubeFilter();
        }

        public bool FilterTabStartCanExecute()
        {
            return true;
        }

        public void FilterTabStartExecuted()
        {
            this.ActiveComponentFilter = this.ComponentFilter;
            ApplyCubeFilter();
            FrameworkExtension.FocusedElementMoveFocus();
        }

        public bool FilterClearCanExecute()
        {
            return !string.IsNullOrEmpty(this.ComponentFilter);
        }

        public void FilterClearExecuted()
        {
            this.ComponentFilter = string.Empty;
            this.ActiveComponentFilter = this.ComponentFilter;
            ApplyCubeFilter();
        }

        public bool DeleteCubesCanExecute()
        {
            return this.SelectedCubeItem != null;
        }

        public void DeleteCubesExecuted()
        {
            this.IsBusy = true;

            this.MainViewModel.ResetProgress(0, this.CubeSelections.Count);

            while (this.CubeSelections.Count > 0)
            {
                this.MainViewModel.Progress++;
                var cube = this.CubeSelections[0];
                if (this.DataModel.CubeGrid.CubeBlocks.Remove(cube.Cube))
                    this.DataModel.CubeList.Remove(cube);
            }

            this.MainViewModel.ClearProgress();
            this.IsBusy = false;
        }

        public bool ReplaceCubesCanExecute()
        {
            return this.SelectedCubeItem != null;
        }

        public void ReplaceCubesExecuted()
        {
            var model = new SelectCubeModel();
            var loadVm = new SelectCubeViewModel(this, model);
            model.Load(this.GridSize, this.SelectedCubeItem.Cube.TypeId, this.SelectedCubeItem.SubtypeId);
            var result = this._dialogService.ShowDialog<WindowSelectCube>(this, loadVm);
            if (result == true)
            {
                this.MainViewModel.IsBusy = true;
                var contentPath = ToolboxUpdater.GetApplicationContentPath();
                var change = false;
                this.MainViewModel.ResetProgress(0, this.CubeSelections.Count);

                foreach (var cube in this.CubeSelections)
                {
                    this.MainViewModel.Progress++;
                    if (cube.TypeId != model.CubeItem.TypeId || cube.SubtypeId != model.CubeItem.SubtypeId)
                    {
                        var idx = this.DataModel.CubeGrid.CubeBlocks.IndexOf(cube.Cube);
                        this.DataModel.CubeGrid.CubeBlocks.RemoveAt(idx);

                        var cubeDefinition = SpaceEngineersAPI.GetCubeDefinition(model.CubeItem.TypeId, this.GridSize, model.CubeItem.SubtypeId);
                        var newCube = cube.CreateCube(model.CubeItem.TypeId, model.CubeItem.SubtypeId, cubeDefinition, DataModel.Settings);
                        cube.TextureFile = Path.Combine(contentPath, cubeDefinition.Icon + ".dds");

                        this.DataModel.CubeGrid.CubeBlocks.Insert(idx, newCube);

                        change = true;
                    }
                }

                this.MainViewModel.ClearProgress();
                if (change)
                {
                    this.MainViewModel.IsModified = true;
                }
                this.MainViewModel.IsBusy = false;
            }
        }

        public bool ColorCubesCanExecute()
        {
            return this.SelectedCubeItem != null;
        }

        public void ColorCubesExecuted()
        {
            var colorDialog = _colorDialogFactory();
            colorDialog.FullOpen = true;
            colorDialog.BrushColor = this.SelectedCubeItem.Color as System.Windows.Media.SolidColorBrush;
            colorDialog.CustomColors = this.MainViewModel.CreativeModeColors;

            var result = _dialogService.ShowColorDialog(this.OwnerViewModel, colorDialog);

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.MainViewModel.IsBusy = true;
                this.MainViewModel.ResetProgress(0, this.CubeSelections.Count);

                foreach (var cube in this.CubeSelections)
                {
                    this.MainViewModel.Progress++;
                    cube.UpdateColor(colorDialog.DrawingColor.Value.ToSandboxHsvColor());
                }

                this.MainViewModel.ClearProgress();
                this.MainViewModel.IsModified = true;
                this.MainViewModel.IsBusy = false;
            }

            this.MainViewModel.CreativeModeColors = colorDialog.CustomColors;
        }

        public bool FrameworkCubesCanExecute()
        {
            return this.SelectedCubeItem != null;
        }

        public void FrameworkCubesExecuted()
        {
            var model = new FrameworkBuildModel { BuildPercent = this.SelectedCubeItem.BuildPercent * 100 };
            var loadVm = new FrameworkBuildViewModel(this, model);
            var result = this._dialogService.ShowDialog<WindowFrameworkBuild>(this, loadVm);
            if (result == true)
            {
                this.MainViewModel.IsBusy = true;
                this.MainViewModel.ResetProgress(0, this.CubeSelections.Count);

                foreach (var cube in this.CubeSelections)
                {
                    this.MainViewModel.Progress++;
                    cube.UpdateBuildPercent(model.BuildPercent.Value / 100);
                }

                this.MainViewModel.ClearProgress();
                this.MainViewModel.IsModified = true;
                this.MainViewModel.IsBusy = false;
            }
        }

        #endregion

        #region methods

        private void ApplyCubeFilter()
        {
            // Prepare filter beforehand.
            if (string.IsNullOrEmpty(this.ActiveComponentFilter))
                _filerView = new string[0];
            else
                _filerView = this.ActiveComponentFilter.ToLowerInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();

            var view = (CollectionView)CollectionViewSource.GetDefaultView(this.CubeList);
            view.Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            if (_filerView.Length == 0)
                return true;

            var cube = (CubeItemModel)item;
            return _filerView.All(s => cube.FriendlyName.ToLowerInvariant().Contains(s) || cube.ColorText.ToLowerInvariant().Contains(s));
        }

        #endregion
    }
}
