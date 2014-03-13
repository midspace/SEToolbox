namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.ComponentModel;
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

        public Sandbox.CommonLib.ObjectBuilders.MyCubeSize GridSize
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

        public double Speed
        {
            get
            {
                return this.DataModel.Speed;
            }
        }

        public double Mass
        {
            get
            {
                return this.DataModel.Mass;
            }
        }

        public string Report
        {
            get
            {
                return this.DataModel.Report;
            }

            set
            {
                this.DataModel.Report = value;
            }
        }

        public int BlockCount
        {
            get { return this.DataModel.BlockCount; }
        }

        public double PositionX
        {
            get
            {
                return this.DataModel.PositionX;
            }

            set
            {
                this.DataModel.PositionX = value;
                this.MainViewModel.CalcDistances();
            }
        }

        public double PositionY
        {
            get
            {
                return this.DataModel.PositionY;
            }

            set
            {
                this.DataModel.PositionY = value;
                this.MainViewModel.CalcDistances();
            }
        }

        public double PositionZ
        {
            get
            {
                return this.DataModel.PositionZ;
            }

            set
            {
                this.DataModel.PositionZ = value;
                this.MainViewModel.CalcDistances();
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
            return this.DataModel.Speed != 0f || this.DataModel.AngularSpeed != 0f;
        }

        public void ResetVelocityExecuted()
        {
            this.DataModel.ResetVelocity();
            this.MainViewModel.IsModified = true;
        }

        public bool ReverseVelocityCanExecute()
        {
            return this.DataModel.Speed != 0f || this.DataModel.AngularSpeed != 0f;
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
            return this.DataModel.IsStatic && this.DataModel.GridSize == MyCubeSize.Large;
        }

        public void ReorientStationExecuted()
        {
            this.DataModel.ReorientStation();
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

        #endregion
    }
}
