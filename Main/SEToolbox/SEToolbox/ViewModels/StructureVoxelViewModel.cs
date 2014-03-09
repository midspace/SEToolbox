namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using System.ComponentModel;

    public class StructureVoxelViewModel : StructureBaseViewModel<StructureVoxelModel>
    {
        #region ctor

        public StructureVoxelViewModel(BaseViewModel parentViewModel, StructureVoxelModel dataModel)
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

        protected new StructureVoxelModel DataModel
        {
            get
            {
                return base.DataModel as StructureVoxelModel;
            }
        }

        public string Filename
        {
            get
            {
                return this.DataModel.Filename;
            }

            set
            {
                this.DataModel.Filename = value;
            }
        }

        public BindableSize3DIModel Size
        {
            get
            {
                return new BindableSize3DIModel(this.DataModel.Size);
            }

            set
            {
                this.DataModel.Size = value.ToVector3I();
            }
        }

        public BindableSize3DIModel ContentSize
        {
            get
            {
                return new BindableSize3DIModel(this.DataModel.ContentSize);
            }

            set
            {
                this.DataModel.ContentSize = value.ToVector3I();
            }
        }

        public long VoxCells
        {
            get
            {
                return this.DataModel.VoxCells;
            }

            set
            {
                this.DataModel.VoxCells = value;
            }
        }

        public double Volume
        {
            get
            {
                return this.DataModel.Volume;
            }
        }

        #endregion
    }
}
