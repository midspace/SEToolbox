namespace SEToolbox.Models
{
    public class ErrorDialogModel : BaseModel
    {
        #region Fields

        private string _errorDescription;
        private string _errorText;
        private bool _canContinue;

        #endregion

        #region Properties

        public string ErrorDescription
        {
            get { return _errorDescription; }

            set
            {
                if (value != _errorDescription)
                {
                    _errorDescription = value;
                    OnPropertyChanged(nameof(ErrorDescription));
                }
            }
        }

        public string ErrorText
        {
            get { return _errorText; }

            set
            {
                if (value != _errorText)
                {
                    _errorText = value;
                    OnPropertyChanged(nameof(ErrorText));
                }
            }
        }

        public bool CanContinue
        {
            get { return _canContinue; }

            set
            {
                if (value != _canContinue)
                {
                    _canContinue = value;
                    OnPropertyChanged(nameof(CanContinue));
                }
            }
        }

        #endregion

        #region methods

        public void Load(string errorDescription, string errorText, bool canContinue)
        {
            ErrorDescription = errorDescription;
            ErrorText = errorText;
            CanContinue = canContinue;
        }

        #endregion
    }
}
