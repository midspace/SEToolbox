namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    public class StructureMeteorViewModel : StructureBaseViewModel<StructureMeteorModel>
    {
        #region ctor

        public StructureMeteorViewModel(BaseViewModel parentViewModel, StructureMeteorModel dataModel)
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
        #endregion

        #region Properties

        protected new StructureMeteorModel DataModel
        {
            get
            {
                return base.DataModel as StructureMeteorModel;
            }
        }

        public MyObjectBuilder_InventoryItem Item
        {
            get
            {
                return this.DataModel.Item;
            }

            set
            {
                this.DataModel.Item = value;
            }
        }

        public string SubTypeName
        {
            get
            {
                return this.DataModel.Item.Content.SubtypeName;
            }
        }

        public double? Volume
        {
            get
            {
                return this.DataModel.Volume;
            }

            set
            {
                this.DataModel.Volume = value;
            }
        }

        public double? Mass
        {
            get
            {
                return this.DataModel.Mass;
            }

            set
            {
                this.DataModel.Mass = value;
            }
        }

        public double LinearVelocity
        {
            get
            {
                return this.DataModel.LinearVelocity;
            }
        }

        #endregion

        #region methods

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

        #endregion
    }
}
