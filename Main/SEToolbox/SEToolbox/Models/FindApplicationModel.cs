namespace SEToolbox.Models
{
    using System;
    using System.IO;
    using System.Windows.Forms;
    using SEToolbox.Support;

    public class FindApplicationModel : BaseModel
    {
        #region Fields

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        private string _baseSavePath;

        private bool _isActive;

        private bool _isBusy;

        private bool _isModified;

        private bool _isBaseSaveChanged;

        #endregion

        #region Constructors

        public FindApplicationModel()
        {
        }

        #endregion

        #region Properties

        public string BaseSavePath
        {
            get
            {
                return this._baseSavePath;
            }

            set
            {
                if (value != this._baseSavePath)
                {
                    this._baseSavePath = value;
                    this.RaisePropertyChanged(() => BaseSavePath);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is available.  This is based on the IsInError and IsBusy properties
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }

            set
            {
                if (value != this._isActive)
                {
                    this._isActive = value;
                    this.RaisePropertyChanged(() => IsActive);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this._isBusy;
            }

            set
            {
                if (value != this._isBusy)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    this.SetActiveStatus();
                    if (this._isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View content has been changed.
        /// </summary>
        public bool IsModified
        {
            get
            {
                return this._isModified;
            }

            set
            {
                if (value != this._isModified)
                {
                    this._isModified = value;
                    this.RaisePropertyChanged(() => IsModified);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the base SE save content has changed.
        /// </summary>
        public bool IsBaseSaveChanged
        {
            get
            {
                return this._isBaseSaveChanged;
            }

            set
            {
                if (value != this._isBaseSaveChanged)
                {
                    this._isBaseSaveChanged = value;
                    this.RaisePropertyChanged(() => IsBaseSaveChanged);
                }
            }
        }

        #endregion

        #region Methods

        public void SetActiveStatus()
        {
            this.IsActive = !this.IsBusy;
        }

        public void Load()
        {
            this.BaseSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SpaceEngineers\Saves");
            this.SetActiveStatus();
        }

        public string UserLocateSpaceEngineersInstall(string startPath)
        {
            if (string.IsNullOrEmpty(startPath))
            {
                startPath = ToolboxUpdater.GetSteamFilePath();
                if (!string.IsNullOrEmpty(startPath))
                {
                    startPath = Path.Combine(startPath, @"SteamApps\common");
                }
            }

            var findAppDialog = new System.Windows.Forms.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "exe",
                FileName = "SpaceEngineers",
                Filter = Properties.Resources.LocateApplicationFilter,
                InitialDirectory = startPath,
                Multiselect = false,
                Title = Properties.Resources.LocateApplicationTitle,
            };

            var ret = findAppDialog.ShowDialog();
            if (ret == DialogResult.OK)
            {
                return Path.GetDirectoryName(Path.GetDirectoryName(findAppDialog.FileName));
            }

            return null;
        }

        #endregion
    }
}
