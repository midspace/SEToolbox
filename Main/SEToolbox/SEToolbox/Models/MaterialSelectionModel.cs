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
                return this._displayName;
            }

            set
            {
                if (value != this._displayName)
                {
                    this._displayName = value;
                    this.RaisePropertyChanged(() => DisplayName);
                }
            }
        }

        public string Value
        {
            get
            {
                return this._value;
            }

            set
            {
                if (value != this._value)
                {
                    this._value = value;
                    this.RaisePropertyChanged(() => Value);
                }
            }
        }

        public bool IsRare
        {
            get
            {
                return this._isRare;
            }

            set
            {
                if (value != this._isRare)
                {
                    this._isRare = value;
                    this.RaisePropertyChanged(() => IsRare);
                }
            }
        }

        public float MinedRatio
        {
            get
            {
                return this._minedRatio;
            }

            set
            {
                if (value != this._minedRatio)
                {
                    this._minedRatio = value;
                    this.RaisePropertyChanged(() => MinedRatio);
                }
            }
        }

        #endregion
    }
}
