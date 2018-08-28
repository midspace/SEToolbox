namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Services;

    public class DataBaseViewModel : BaseViewModel, IDragable
    {
        #region fields

        private IStructureBase _dataModel;

        #endregion

        #region ctor

        public DataBaseViewModel(BaseViewModel parentViewModel, IStructureBase dataModel)
            : base(parentViewModel)
        {
            _dataModel = dataModel;
        }

        #endregion

        #region properties

        public IStructureBase DataModel
        {
            get { return _dataModel; }
            set
            {
                if (value != _dataModel)
                {
                    _dataModel = value;
                    OnPropertyChanged(nameof(DataModel));
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