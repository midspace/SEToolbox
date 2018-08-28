namespace SEToolbox.Models
{
    using SEToolbox.Converters;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Res = SEToolbox.Properties.Resources;

    public class SelectWorldModel : BaseModel
    {
        #region Fields

        private WorldResource _selectedWorld;

        private ObservableCollection<WorldResource> _worlds;

        private bool _isBusy;

        #endregion

        #region ctor

        public SelectWorldModel()
        {
            SelectedWorld = null;
            Worlds = new ObservableCollection<WorldResource>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The base path of the save files, minus the userid.
        /// </summary>
        public UserDataPath BaseLocalPath { get; set; }

        public UserDataPath BaseDedicatedServerHostPath { get; set; }

        public UserDataPath BaseDedicatedServerServicePath { get; set; }

        public WorldResource SelectedWorld
        {
            get
            {
                return _selectedWorld;
            }

            set
            {
                if (value != _selectedWorld)
                {
                    _selectedWorld = value;
                    OnPropertyChanged(nameof(SelectedWorld));
                }
            }
        }

        public ObservableCollection<WorldResource> Worlds
        {
            get
            {
                return _worlds;
            }

            set
            {
                if (value != _worlds)
                {
                    _worlds = value;
                    OnPropertyChanged(nameof(Worlds));
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
                return _isBusy;
            }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    if (_isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        #endregion

        #region methods

        public void Load(UserDataPath baseLocalPath, UserDataPath baseDedicatedServerHostPath, UserDataPath baseDedicatedServerServicePath)
        {
            BaseLocalPath = baseLocalPath;
            BaseDedicatedServerHostPath = baseDedicatedServerHostPath;
            BaseDedicatedServerServicePath = baseDedicatedServerServicePath;
            LoadSaveList();
        }

        public void Refresh()
        {
            LoadSaveList();
        }

        #endregion

        #region helpers

        private void LoadSaveList()
        {
            Worlds.Clear();
            var list = new List<WorldResource>();

            #region local saves

            if (Directory.Exists(BaseLocalPath.SavesPath))
            {
                var userPaths = Directory.GetDirectories(BaseLocalPath.SavesPath);

                foreach (var userPath in userPaths)
                {
                    var userName = Path.GetFileName(userPath);
                    list.AddRange(FindSaveFiles(userPath, userName, SaveWorldType.Local, BaseLocalPath));
                }
            }

            #endregion

            #region Host Server

            if (Directory.Exists(BaseDedicatedServerHostPath.SavesPath))
            {
                list.AddRange(FindSaveFiles(BaseDedicatedServerHostPath.SavesPath, "Local / Console", SaveWorldType.DedicatedServerHost, BaseDedicatedServerHostPath));
            }

            #endregion

            #region Service Server

            if (Directory.Exists(BaseDedicatedServerServicePath.SavesPath))
            {
                var instancePaths = Directory.GetDirectories(BaseDedicatedServerServicePath.SavesPath);

                foreach (var instancePath in instancePaths)
                {
                    var lastLoadedPath = Path.Combine(instancePath, SpaceEngineersConsts.SavesFolder);

                    if (Directory.Exists(lastLoadedPath))
                    {
                        var instanceName = Path.GetFileName(instancePath);
                        var dataPath = new UserDataPath(instancePath, SpaceEngineersConsts.SavesFolder, SpaceEngineersConsts.ModsFolder, SpaceEngineersConsts.BlueprintsFolder);
                        list.AddRange(FindSaveFiles(lastLoadedPath, instanceName, SaveWorldType.DedicatedServerService, dataPath));
                    }
                }
            }

            #endregion

            foreach (var item in list.OrderByDescending(w => w.LastSaveTime))
                Worlds.Add(item);
        }

        private IEnumerable<WorldResource> FindSaveFiles(string lastLoadedPath, string userName, SaveWorldType saveType, UserDataPath dataPath)
        {
            var list = new List<WorldResource>();

            // Ignore any other base Save paths without the LastLoaded file.
            if (Directory.Exists(lastLoadedPath))
            {
                var savePaths = Directory.GetDirectories(lastLoadedPath);

                // Still check every potential game world path.
                foreach (var savePath in savePaths)
                {
                    var saveResource = LoadSaveFromPath(savePath, userName, saveType, dataPath);

                    // This should still allow Games to be copied into the Save path manually.
                    saveResource.LoadWorldInfo();
                    list.Add(saveResource);
                }
            }

            return list;
        }

        internal WorldResource LoadSaveFromPath(string savePath, string userName, SaveWorldType saveType, UserDataPath dataPath)
        {
            var saveResource = new WorldResource
            {
                GroupDescription = $"{new EnumToResouceConverter().Convert(saveType, typeof (string), null, CultureInfo.CurrentUICulture)}: {userName}",
                SaveType = saveType,
                Savename = Path.GetFileName(savePath),
                UserName = userName,
                Savepath = savePath,
                DataPath = dataPath,
            };

            return saveResource;
        }

        internal static bool FindSaveSession(string baseSavePath, string findSession, out WorldResource saveResource, out string errorInformation)
        {
            if (Directory.Exists(baseSavePath))
            {
                var userPaths = Directory.GetDirectories(baseSavePath);

                foreach (var userPath in userPaths)
                {
                    // Ignore any other base Save paths without the LastLoaded file.
                    if (Directory.Exists(userPath))
                    {
                        var savePaths = Directory.GetDirectories(userPath);

                        // Still check every potential game world path.
                        foreach (var savePath in savePaths)
                        {
                            saveResource = new WorldResource
                            {
                                Savename = Path.GetFileName(savePath),
                                UserName = Path.GetFileName(userPath),
                                Savepath = savePath,
                                DataPath = UserDataPath.FindFromSavePath(savePath)
                            };

                            saveResource.LoadWorldInfo();
                            if (saveResource.IsValid && (saveResource.Savename.ToUpper() == findSession || saveResource.SessionName.ToUpper() == findSession))
                            {
                                return saveResource.LoadCheckpoint(out errorInformation);
                            }
                        }
                    }
                }
            }

            saveResource = null;
            errorInformation = Res.ErrorGameNotFound;
            return false;
        }

        internal static bool LoadSession(string savePath, out WorldResource saveResource, out string errorInformation)
        {
            if (Directory.Exists(savePath))
            {
                var userPath = Path.GetDirectoryName(savePath);

                saveResource = new WorldResource
                {
                    Savename = Path.GetFileName(savePath),
                    UserName = Path.GetFileName(userPath),
                    Savepath = savePath,
                    DataPath = UserDataPath.FindFromSavePath(savePath)
                };

                return saveResource.LoadCheckpoint(out errorInformation);
            }

            saveResource = null;
            errorInformation = Res.ErrorDirectoryNotFound;
            return false;
        }

        #endregion
    }
}
