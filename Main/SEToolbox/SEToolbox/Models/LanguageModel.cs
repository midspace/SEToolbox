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
            get
            {
                return this._ietfLanguageTag;
            }

            set
            {
                if (value != this._ietfLanguageTag)
                {
                    this._ietfLanguageTag = value;
                    this.RaisePropertyChanged(() => IetfLanguageTag);
                }
            }
        }

        public string ImageName
        {
            get
            {
                return this._imageName;
            }

            set
            {
                if (value != this._imageName)
                {
                    this._imageName = value;
                    this.RaisePropertyChanged(() => ImageName);
                }
            }
        }

        public string Name
        {
            get
            {
                if (this.NativeName == this.LanguageName)
                    return this.NativeName;
                else
                    return string.Format("{0} / {1}", this.NativeName, this.LanguageName);
            }
        }

        /// <summary>
        /// Localized language name.
        /// </summary>
        public string LanguageName
        {
            get
            {
                return this._languageName;
            }

            set
            {
                if (value != this._languageName)
                {
                    this._languageName = value;
                    this.RaisePropertyChanged(() => LanguageName);
                    this.RaisePropertyChanged(() => Name);
                }
            }
        }

        public string NativeName
        {
            get
            {
                return this._nativeName;
            }

            set
            {
                if (value != this._nativeName)
                {
                    this._nativeName = value;
                    this.RaisePropertyChanged(() => NativeName);
                    this.RaisePropertyChanged(() => Name);
                }
            }
        }

        #endregion
    }
}
