namespace SEToolbox.Models
{
    public class OwnerModel : BaseModel
    {
        #region fields

        private string _name;

        private bool _isDead;

        private string _model;

        private long _playerId;

        private ulong _steamId;

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
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        public bool IsDead
        {
            get { return _isDead; }

            set
            {
                if (value != _isDead)
                {
                    _isDead = value;
                    RaisePropertyChanged(() => IsDead);
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

        public ulong SteamId
        {
            get { return _steamId; }

            set
            {
                if (value != _steamId)
                {
                    _steamId = value;
                    RaisePropertyChanged(() => SteamId);
                }
            }
        }

        #endregion
    }
}
