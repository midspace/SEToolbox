namespace SEToolbox.Models
{
    using System.IO;

    using SEToolbox.Support;

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
            get { return _gameApplicationPath; }

            set
            {
                if (value != _gameApplicationPath)
                {
                    _gameApplicationPath = value;
                    RaisePropertyChanged(() => GameApplicationPath);
                    Validate();
                }
            }
        }

        public string GameBinPath
        {
            get { return _gameBinPath; }

            set
            {
                if (value != _gameBinPath)
                {
                    _gameBinPath = value;
                    RaisePropertyChanged(() => GameBinPath);
                }
            }
        }

        public bool IsValidApplication
        {
            get { return _isValidApplication; }

            set
            {
                if (value != _isValidApplication)
                {
                    _isValidApplication = value;
                    RaisePropertyChanged(() => IsValidApplication);
                }
            }
        }

        public bool IsWrongApplication
        {
            get { return _isWrongApplication; }

            set
            {
                if (value != _isWrongApplication)
                {
                    _isWrongApplication = value;
                    RaisePropertyChanged(() => IsWrongApplication);
                }
            }
        }

        #endregion

        #region Methods

        public void Validate()
        {
            GameBinPath = null;

            if (!string.IsNullOrEmpty(GameApplicationPath))
            {
                try
                {
                    var fullPath = Path.GetFullPath(GameApplicationPath);
                    if (File.Exists(fullPath))
                    {
                        GameBinPath = Path.GetDirectoryName(fullPath);
                    }
                }
                catch { }
            }

            IsValidApplication = ToolboxUpdater.ValidateSpaceEngineersInstall(GameBinPath);
            IsWrongApplication = !IsValidApplication;
        }

        #endregion
    }
}
