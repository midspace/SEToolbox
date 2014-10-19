namespace SEToolbox.Models
{
    public class OwnerModel : BaseModel
    {
        #region fields

        private string _name;

        private string _model;

        private long _playerId;

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

        #endregion
    }
}
