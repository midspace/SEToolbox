namespace SEToolbox.Models
{
    using Res = SEToolbox.Properties.Resources;

    public class OwnerModel : BaseModel
    {
        #region fields

        private string _name;

        private string _model;

        private long _playerId;

        private bool _isPlayer;

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }

            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name), nameof(DisplayName));
                }
            }
        }

        public string Model
        {
            get { return _model; }

            set
            {
                if (value != _model)
                {
                    _model = value;
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        public long PlayerId
        {
            get { return _playerId; }

            set
            {
                if (value != _playerId)
                {
                    _playerId = value;
                    OnPropertyChanged(nameof(PlayerId));
                }
            }
        }

        public bool IsPlayer
        {
            get { return _isPlayer; }

            set
            {
                if (value != _isPlayer)
                {
                    _isPlayer = value;
                    OnPropertyChanged(nameof(IsPlayer), nameof(DisplayName));
                }
            }
        }

        public string DisplayName
        {
            get
            {
                if (_isPlayer || _playerId == 0)
                    return _name;

                return string.Format("{0} ({1})", _name, Res.ClsCharacterDead);
            }
        }

        #endregion
    }
}
