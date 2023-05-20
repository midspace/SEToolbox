namespace SEToolbox.Models
{
    public class LanguageModel : BaseModel
    {
        #region fields

        private string _ietfLanguageTag;
        private string _imageName;
        private string _languageName;
        private string _nativeName;

        #endregion

        #region Properties

        public string IetfLanguageTag
        {
            get { return _ietfLanguageTag; }

            set
            {
                if (value != _ietfLanguageTag)
                {
                    _ietfLanguageTag = value;
                    OnPropertyChanged(nameof(IetfLanguageTag));
                }
            }
        }

        public string ImageName
        {
            get { return _imageName; }

            set
            {
                if (value != _imageName)
                {
                    _imageName = value;
                    OnPropertyChanged(nameof(ImageName));
                }
            }
        }

        public string Name
        {
            get { return NativeName == LanguageName ? NativeName : string.Format("{0} / {1}", NativeName, LanguageName); }
        }

        /// <summary>
        /// Localized language name.
        /// </summary>
        public string LanguageName
        {
            get
            {
                return _languageName;
            }

            set
            {
                if (value != _languageName)
                {
                    _languageName = value;
                    OnPropertyChanged(nameof(LanguageName));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string NativeName
        {
            get
            {
                return _nativeName;
            }

            set
            {
                if (value != _nativeName)
                {
                    _nativeName = value;
                    OnPropertyChanged(nameof(NativeName));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        #endregion
    }
}
