namespace SEToolbox.ViewModels
{
    using System.ComponentModel;
    using System.Windows.Input;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using VRage.Game;

    public class StructureMeteorViewModel : StructureBaseViewModel<StructureMeteorModel>
    {
        #region ctor

        public StructureMeteorViewModel(BaseViewModel parentViewModel, StructureMeteorModel dataModel)
            : base(parentViewModel, dataModel)
        {
            DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

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

        #endregion

        #region Properties

        protected new StructureMeteorModel DataModel
        {
            get { return base.DataModel as StructureMeteorModel; }
        }

        public MyObjectBuilder_InventoryItem Item
        {
            get { return DataModel.Item; }
            set { DataModel.Item = value; }
        }

        public string SubTypeName
        {
            get { return DataModel.Item.PhysicalContent.SubtypeName; }
        }

        public double? Volume
        {
            get { return DataModel.Volume; }
            set { DataModel.Volume = value; }
        }

        public override double LinearVelocity
        {
            get { return DataModel.LinearVelocity; }
        }

        public float Integrity
        {
            get { return DataModel.Integrity; }
        }

        #endregion

        #region methods

        public bool ResetVelocityCanExecute()
        {
            return DataModel.LinearVelocity != 0f || DataModel.AngularVelocity != 0f;
        }

        public void ResetVelocityExecuted()
        {
            DataModel.ResetVelocity();
            MainViewModel.IsModified = true;
        }

        public bool ReverseVelocityCanExecute()
        {
            return DataModel.LinearVelocity != 0f || DataModel.AngularVelocity != 0f;
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

        #endregion
    }
}
