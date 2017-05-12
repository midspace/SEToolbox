namespace SEToolbox.Models
{
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
                    RaisePropertyChanged(() => Name, () => DisplayName);
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
                    RaisePropertyChanged(() => Model);
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
                    RaisePropertyChanged(() => PlayerId);
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
                    RaisePropertyChanged(() => IsPlayer, () => DisplayName);
                }
            }
        }

        public string DisplayName
        {
            get
            {
                if (_isPlayer || _playerId == 0)
                    return _name;

                return string.Format("{0} (dead)", _name);
            }
        }

        #endregion
    }
}
