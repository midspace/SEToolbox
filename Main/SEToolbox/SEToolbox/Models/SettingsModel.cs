namespace SEToolbox.Models
{
    using SEToolbox.Support;

    public class SettingsModel : BaseModel
    {
        #region Fields

        private string _seBinPath;
        private string _customVoxelPath;
        private bool? _alwaysCheckForUpdates;
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
                    RaisePropertyChanged(() => SEBinPath);
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
                    RaisePropertyChanged(() => CustomVoxelPath);
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
                    RaisePropertyChanged(() => AlwaysCheckForUpdates);
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
                    RaisePropertyChanged(() => IsValid);
                }
            }
        }

        #endregion

        #region methods

        public void Load(string seBinPath, string customVoxelPath, bool? alwaysCheckForUpdates)
        {
            SEBinPath = seBinPath;
            CustomVoxelPath = customVoxelPath;
            AlwaysCheckForUpdates = alwaysCheckForUpdates;
        }

        private void Validate()
        {
            IsValid = ToolboxUpdater.ValidateSpaceEngineersInstall(SEBinPath);
            // no need to check CustomVoxelPath or AlwaysCheckForUpdates.
        }

        #endregion
    }
}
