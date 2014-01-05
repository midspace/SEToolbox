namespace SEToolbox.Models
{
    public class MaterialSelectionModel : BaseModel
    {
        private string _displayName;

        private string _value;

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

        #endregion
    }
}
