namespace SEToolbox.ViewModels
{
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

        public ICommand RepairObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(RepairObjectExecuted), new Func<bool>(RepairObjectCanExecute));
            }
        }

        public ICommand ResetSpeedCommand
        {
            get
            {
                return new DelegateCommand(new Action(ResetSpeedExecuted), new Func<bool>(ResetSpeedCanExecute));
            }
        }

        public ICommand ConvertCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertExecuted), new Func<bool>(ConvertCanExecute));
            }
        }

        public ICommand ConvertToHeavyCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertToHeavyExecuted), new Func<bool>(ConvertToHeavyCanExecute));
            }
        }

        public ICommand ConvertToLightCommand
        {
            get
            {
                return new DelegateCommand(new Action(ConvertToLightExecuted), new Func<bool>(ConvertToLightCanExecute));
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

        public Vector3D Size
        {
            get
            {
                return this.DataModel.Size;
            }

            set
            {
                this.DataModel.Size = value;
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

        #endregion

        #region methods

        public bool RepairObjectCanExecute()
        {
            return this.IsDamaged;
        }

        public void RepairObjectExecuted()
        {
            this.DataModel.RepairAllDamage();
            ((ExplorerViewModel)this.OwnerViewModel).IsModified = true;
        }

        public bool ResetSpeedCanExecute()
        {
            return true;
        }

        public void ResetSpeedExecuted()
        {
            this.DataModel.ResetSpeed();
            ((ExplorerViewModel)this.OwnerViewModel).IsModified = true;
        }

        public bool ConvertCanExecute()
        {
            return true;
        }

        public void ConvertExecuted()
        {
        }

        public bool ConvertToHeavyCanExecute()
        {
            return true;
        }

        public void ConvertToHeavyExecuted()
        {
            this.DataModel.ConvertFromLightToHeavyArmor();
            ((ExplorerViewModel)this.OwnerViewModel).IsModified = true;
        }

        public bool ConvertToLightCanExecute()
        {
            return true;
        }

        public void ConvertToLightExecuted()
        {
            this.DataModel.ConvertFromHeavyToLightArmor();
            ((ExplorerViewModel)this.OwnerViewModel).IsModified = true;
        }

        #endregion
    }
}
