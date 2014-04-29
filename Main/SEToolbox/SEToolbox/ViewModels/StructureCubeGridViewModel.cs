namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

    public class StructureCubeGridViewModel : StructureBaseViewModel<StructureCubeGridModel>
    {
        #region ctor

        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel)
            : base(parentViewModel, dataModel)
        {
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

        #endregion

        #region methods

        public bool OptimizeObjectCanExecute()
        {
            return true;
        }

        public void OptimizeObjectExecuted()
        {
            this.MainViewModel.OptimizeModel(this);
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
            return true;
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

            Clipboard.SetText(detail);
        }

        public bool FilterStartCanExecute()
        {
            return false;
        }

        public void FilterStartExecuted()
        {
            // TODO:
        }

        public bool FilterClearCanExecute()
        {
            return !string.IsNullOrEmpty(this.ComponentFilter);
        }

        public void FilterClearExecuted()
        {
            this.ComponentFilter = string.Empty;
        }

        public bool DeleteCubesCanExecute()
        {
            return false;
        }

        public void DeleteCubesExecuted()
        {
            // TODO:
        }

        public bool ReplaceCubesCanExecute()
        {
            return false;
        }

        public void ReplaceCubesExecuted()
        {
            // TODO:
        }

        public bool ColorCubesCanExecute()
        {
            return false;
        }

        public void ColorCubesExecuted()
        {
            // TODO:
        }

        #endregion
    }
}
