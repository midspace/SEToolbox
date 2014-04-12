namespace SEToolbox.Models
{
    using SEToolbox.Support;
    using System.IO;

    public class FindApplicationModel : BaseModel
    {
        #region Fields

        private string _gameApplicationPath;

        private string _gameRootPath;

        private bool isValidApplication;

        private bool _isWrongApplication;

        #endregion

        #region Constructors

        public FindApplicationModel()
        {
        }

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

        public string GameRootPath
        {
            get
            {
                return this._gameRootPath;
            }

            set
            {
                if (value != this._gameRootPath)
                {
                    this._gameRootPath = value;
                    this.RaisePropertyChanged(() => GameRootPath);
                }
            }
        }

        public bool IsValidApplication
        {
            get
            {
                return this.isValidApplication;
            }

            set
            {
                if (value != this.isValidApplication)
                {
                    this.isValidApplication = value;
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
            this.GameRootPath = null;

            if (!string.IsNullOrEmpty(GameApplicationPath))
            {
                try
                {
                    Path.GetFullPath(GameApplicationPath);
                    if (File.Exists(GameApplicationPath))
                    {
                        this.GameRootPath = Path.GetDirectoryName(Path.GetDirectoryName(GameApplicationPath));
                    }
                }
                catch { }
            }

            this.IsValidApplication = ToolboxUpdater.ValidateSpaceEngineersInstall(this.GameRootPath);
            this.IsWrongApplication = !this.IsValidApplication;
        }

        #endregion
    }
}
