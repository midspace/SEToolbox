namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;

    public class StructureVoxelViewModel : StructureBaseViewModel
    {
        #region ctor

        public StructureVoxelViewModel(BaseViewModel parentViewModel, StructureVoxelModel dataModel)
            : base(parentViewModel, dataModel)
        {
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
                if (value != this.DataModel.Filename)
                {
                    this.DataModel.Filename = value;
                    this.RaisePropertyChanged(() => Filename);
                }
            }
        }

        #endregion

        #region methods

        #endregion
    }
}
