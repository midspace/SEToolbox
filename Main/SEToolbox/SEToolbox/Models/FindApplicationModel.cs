namespace SEToolbox.Models
{
    using SEToolbox.Support;
    using System.IO;

    public class FindApplicationModel : BaseModel
    {
        #region Fields

        private string _gameApplicationPath;

        private string _gameBinPath;

        private bool _isValidApplication;

        private bool _isWrongApplication;

        #endregion

        #region Properties

        public string GameApplicationPath
        {
            get
            {
                return this._gameApplicationPath;
            }

            set
            {
                if (value != this._gameApplicationPath)
                {
                    this._gameApplicationPath = value;
                    this.RaisePropertyChanged(() => GameApplicationPath);
                    this.Validate();
                }
            }
        }

        public string GameBinPath
        {
            get
            {
                return this._gameBinPath;
            }

            set
            {
                if (value != this._gameBinPath)
                {
                    this._gameBinPath = value;
                    this.RaisePropertyChanged(() => GameBinPath);
                }
            }
        }

        public bool IsValidApplication
        {
            get
            {
                return this._isValidApplication;
            }

            set
            {
                if (value != this._isValidApplication)
                {
                    this._isValidApplication = value;
                    this.RaisePropertyChanged(() => IsValidApplication);
                }
            }
        }

        public bool IsWrongApplication
        {
            get
            {
                return this._isWrongApplication;
            }

            set
            {
                if (value != this._isWrongApplication)
                {
                    this._isWrongApplication = value;
                    this.RaisePropertyChanged(() => IsWrongApplication);
                }
            }
        }

        #endregion

        #region Methods

        public void Validate()
        {
            this.GameBinPath = null;

            if (!string.IsNullOrEmpty(GameApplicationPath))
            {
                try
                {
                    var fullPath = Path.GetFullPath(GameApplicationPath);
                    if (File.Exists(fullPath))
                    {
                        this.GameBinPath = Path.GetDirectoryName(fullPath);
                    }
                }
                catch { }
            }

            this.IsValidApplication = ToolboxUpdater.ValidateSpaceEngineersInstall(this.GameBinPath);
            this.IsWrongApplication = !this.IsValidApplication;
        }

        #endregion
    }
}
