namespace SEToolbox.Models
{
    using SEToolbox.Support;

    public class SettingsModel : BaseModel
    {
        #region Fields

        private string _seBinPath;
        private string _customVoxelPath;
        private bool? _alwaysCheckForUpdates;
        private bool? _useCustomResource;
        private bool _isValid;

        #endregion

        #region Properties

        public string SEBinPath
        {
            get { return _seBinPath; }

            set
            {
                if (value != _seBinPath)
                {
                    _seBinPath = value;
                    OnPropertyChanged(nameof(SEBinPath));
                    Validate();
                }
            }
        }

        public string CustomVoxelPath
        {
            get { return _customVoxelPath; }

            set
            {
                if (value != _customVoxelPath)
                {
                    _customVoxelPath = value;
                    OnPropertyChanged(nameof(CustomVoxelPath));
                    Validate();
                }
            }
        }

        public bool? AlwaysCheckForUpdates
        {
            get { return _alwaysCheckForUpdates; }

            set
            {
                if (value != _alwaysCheckForUpdates)
                {
                    _alwaysCheckForUpdates = value;
                    OnPropertyChanged(nameof(AlwaysCheckForUpdates));
                    Validate();
                }
            }
        }

        public bool? UseCustomResource
        {
            get { return _useCustomResource; }

            set
            {
                if (value != _useCustomResource)
                {
                    _useCustomResource = value;
                    OnPropertyChanged(nameof(UseCustomResource));
                    Validate();
                }
            }
        }
        

        public bool IsValid
        {
            get { return _isValid; }

            private set
            {
                if (value != _isValid)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        #endregion

        #region methods

        public void Load(string seBinPath, string customVoxelPath, bool? alwaysCheckForUpdates, bool? useCustomResource)
        {
            SEBinPath = seBinPath;
            CustomVoxelPath = customVoxelPath;
            AlwaysCheckForUpdates = alwaysCheckForUpdates;
            UseCustomResource = useCustomResource;
        }

        private void Validate()
        {
            IsValid = ToolboxUpdater.ValidateSpaceEngineersInstall(SEBinPath);
            // no need to check CustomVoxelPath, AlwaysCheckForUpdates, or UseCustomResource.
        }

        #endregion
    }
}
