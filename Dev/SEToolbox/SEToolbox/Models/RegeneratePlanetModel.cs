namespace SEToolbox.Models
{
    using System.Collections.ObjectModel;

    public class RegeneratePlanetModel : BaseModel
    {
        #region Fields

        private int _seed;
        private decimal _diameter;
        private bool _invalidKeenRange;

        #endregion

        #region ctor

        public RegeneratePlanetModel()
        {
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
                    OnPropertyChanged(nameof(Seed));
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
                    OnPropertyChanged(nameof(Diameter));
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
                    OnPropertyChanged(nameof(InvalidKeenRange));
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
