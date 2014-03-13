namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using System.ComponentModel;

    public class StructureFloatingObjectViewModel : StructureBaseViewModel<StructureFloatingObjectModel>
    {
        #region ctor

        public StructureFloatingObjectViewModel(BaseViewModel parentViewModel, StructureFloatingObjectModel dataModel)
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

        protected new StructureFloatingObjectModel DataModel
        {
            get
            {
                return base.DataModel as StructureFloatingObjectModel;
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

        public double? Units
        {
            get
            {
                return this.DataModel.Units;
            }

            set
            {
                this.DataModel.Units = value;
            }
        }

        #endregion
    }
}
