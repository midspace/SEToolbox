namespace SEToolbox.Models
{
    public class BlueprintDialogModel : BaseModel
    {
        #region Fields

        private string _blueprintName;
        private string _dialogTitle;
        private bool _checkForExisting;
        private string _localBlueprintsFolder;

        #endregion

        #region Properties

        public string BlueprintName
        {
            get { return _blueprintName; }

            set
            {
                if (value != _blueprintName)
                {
                    _blueprintName = value;
                    RaisePropertyChanged(() => BlueprintName);
                }
            }
        }

        public string DialogTitle
        {
            get { return _dialogTitle; }

            set
            {
                if (value != _dialogTitle)
                {
                    _dialogTitle = value;
                    RaisePropertyChanged(() => DialogTitle);
                }
            }
        }

        public bool CheckForExisting
        {
            get { return _checkForExisting; }

            set
            {
                if (value != _checkForExisting)
                {
                    _checkForExisting = value;
                    RaisePropertyChanged(() => CheckForExisting);
                }
            }
        }

        public string LocalBlueprintsFolder
        {
            get { return _localBlueprintsFolder; }

            set
            {
                if (value != _localBlueprintsFolder)
                {
                    _localBlueprintsFolder = value;
                    RaisePropertyChanged(() => LocalBlueprintsFolder);
                }
            }
        }

        #endregion

        #region methods

        public void Load(string dialogText, bool checkForExisting, string localBlueprintsFolder)
        {
            DialogTitle = dialogText;
            CheckForExisting = checkForExisting;
            LocalBlueprintsFolder = localBlueprintsFolder;
        }

        #endregion
    }
}
