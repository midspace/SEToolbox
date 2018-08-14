namespace SEToolbox.Models
{
    using System.Collections.ObjectModel;

    public class RegeneratePlanetModel : BaseModel
    {
        #region Fields

        private ObservableCollection<ComponentItemModel> _cubeList;
        private ComponentItemModel _cubeItem;

        private int _seed;
        private decimal _diameter;
        private bool _invalidKeenRange;

        #endregion

        #region ctor

        public RegeneratePlanetModel()
        {
            _cubeList = new ObservableCollection<ComponentItemModel>();
        }

        #endregion

        #region Properties

        public int Seed
        {
            get { return _seed; }

            set
            {
                if (value != _seed)
                {
                    _seed = value;
                    RaisePropertyChanged(() => Seed);
                }
            }
        }

        public decimal Diameter
        {
            get { return _diameter; }

            set
            {
                if (value != _diameter)
                {
                    _diameter = value;
                    RaisePropertyChanged(() => Diameter);
                    InvalidKeenRange = _diameter < 19000 || _diameter > 120000;
                }
            }
        }

        public bool InvalidKeenRange
        {
            get { return _invalidKeenRange; }

            set
            {
                if (value != _invalidKeenRange)
                {
                    _invalidKeenRange = value;
                    RaisePropertyChanged(() => InvalidKeenRange);
                }
            }
        }

        #endregion

        #region methods

        public void Load(int seed, float radius)
        {
            Seed = seed;
            Diameter = (decimal)(radius * 2f);

        }

        #endregion
    }
}
