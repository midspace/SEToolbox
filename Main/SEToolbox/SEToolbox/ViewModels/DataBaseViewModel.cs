namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Services;

    public class DataBaseViewModel : BaseViewModel, IDragable
    {
        #region fields

        protected IStructureBase dataModel;

        #endregion

        #region ctor

        public DataBaseViewModel(BaseViewModel parentViewModel, IStructureBase dataModel)
            : base(parentViewModel)
        {
            this.dataModel = dataModel;
        }

        #endregion

        #region properties

        public IStructureBase DataModel
        {
            get { return this.dataModel; }
            set
            {
                if (value != this.dataModel)
                {
                    this.dataModel = value;
                    this.RaisePropertyChanged(() => DataModel);
                }
            }
        }

        #endregion

        #region IDragable Interface

        //[XmlIgnore]
        System.Type IDragable.DataType
        {
            get { return typeof(DataBaseViewModel); }
        }

        void IDragable.Remove(object i)
        {

        }

        #endregion
    }
}