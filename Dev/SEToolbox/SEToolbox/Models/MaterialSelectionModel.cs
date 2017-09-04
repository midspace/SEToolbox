namespace SEToolbox.Models
{
    public class MaterialSelectionModel : BaseModel
    {
        private string _displayName;

        private string _value;

        private bool _isRare;

        private float _minedRatio;

        #region Properties

        public string DisplayName
        {
            get
            {
                return _displayName;
            }

            set
            {
                if (value != _displayName)
                {
                    _displayName = value;
                    RaisePropertyChanged(() => DisplayName);
                }
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                if (value != _value)
                {
                    _value = value;
                    RaisePropertyChanged(() => Value);
                }
            }
        }

        public bool IsRare
        {
            get
            {
                return _isRare;
            }

            set
            {
                if (value != _isRare)
                {
                    _isRare = value;
                    RaisePropertyChanged(() => IsRare);
                }
            }
        }

        public float MinedRatio
        {
            get
            {
                return _minedRatio;
            }

            set
            {
                if (value != _minedRatio)
                {
                    _minedRatio = value;
                    RaisePropertyChanged(() => MinedRatio);
                }
            }
        }

        #endregion
    }
}
