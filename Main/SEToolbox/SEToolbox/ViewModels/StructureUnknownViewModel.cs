namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using System.ComponentModel;

    public class StructureUnknownViewModel : StructureBaseViewModel<StructureUnknownModel>
    {
        #region ctor

        public StructureUnknownViewModel(BaseViewModel parentViewModel, StructureUnknownModel dataModel)
            : base(parentViewModel, dataModel)
        {
            this.DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region Properties

        protected new StructureUnknownModel DataModel
        {
            get
            {
                return base.DataModel as StructureUnknownModel;
            }
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
                this.MainViewModel.IsModified = true;
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
                this.MainViewModel.IsModified = true;
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
                this.MainViewModel.IsModified = true;
                this.MainViewModel.CalcDistances();
            }
        }

        #endregion
    }
}
