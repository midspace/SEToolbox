﻿namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Converters;
    using SEToolbox.Interop;
    using SEToolbox.Support;

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
                    RaisePropertyChanged(() => SelectedWorld);
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
                    RaisePropertyChanged(() => Worlds);
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
                    RaisePropertyChanged(() => IsBusy);
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
                    var lastLoadedPath = Path.Combine(instancePath, "Saves");

                    if (Directory.Exists(lastLoadedPath))
                    {
                        var instanceName = Path.GetFileName(instancePath);
                        var dataPath = new UserDataPath(lastLoadedPath, Path.Combine(instancePath, "Mods"));
                        list.AddRange(FindSaveFiles(lastLoadedPath, instanceName, SaveWorldType.DedicatedServerService, dataPath));
                    }
                }
            }

            #endregion

            foreach (var item in list.OrderByDescending(w => w.LastLoadTime))
                Worlds.Add(item);
        }

        private IEnumerable<WorldResource> FindSaveFiles(string lastLoadedPath, string userName, SaveWorldType saveType, UserDataPath dataPath)
        {
            var lastLoadedFile = Path.Combine(lastLoadedPath, SpaceEngineersConsts.LoadLoadedFilename);
            var list = new List<WorldResource>();

            // Ignore any other base Save paths without the LastLoaded file.
            if (File.Exists(lastLoadedFile))
            {
                MyObjectBuilder_LastLoadedTimes lastLoaded = null;
                try
                {
                    lastLoaded = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_LastLoadedTimes>(lastLoadedFile);
                }
                catch { }
                var savePaths = Directory.GetDirectories(lastLoadedPath);

                // Still check every potential game world path.
                foreach (var savePath in savePaths)
                {
                    var saveResource = LoadSaveFromPath(savePath, userName, saveType, dataPath);
                    if (lastLoaded != null)
                    {
                        var last = lastLoaded.LastLoaded.Dictionary.FirstOrDefault(d => d.Key.Equals(savePath, StringComparison.OrdinalIgnoreCase));
                        if (last.Key != null)
                        {
                            saveResource.LastLoadTime = last.Value;
                        }
                    }

                    // This should still allow Games to be copied into the Save path manually.

                    saveResource.LoadCheckpoint();
                    list.Add(saveResource);
                }
            }

            return list;
        }

        internal WorldResource LoadSaveFromPath(string savePath, string userName, SaveWorldType saveType, UserDataPath dataPath)
        {
            var saveResource = new WorldResource
            {
                GroupDescription = string.Format("{0}: {1}", new EnumToResouceConverter().Convert(saveType, typeof(string), null, CultureInfo.CurrentUICulture), userName),
                SaveType = saveType,
                Savename = Path.GetFileName(savePath),
                UserName = userName,
                Savepath = savePath,
                DataPath = dataPath,
            };

            return saveResource;
        }

        internal static WorldResource FindSaveSession(string baseSavePath, string findSession)
        {
            if (Directory.Exists(baseSavePath))
            {
                var userPaths = Directory.GetDirectories(baseSavePath);

                foreach (var userPath in userPaths)
                {
                    var lastLoadedFile = Path.Combine(userPath, SpaceEngineersConsts.LoadLoadedFilename);

                    // Ignore any other base Save paths without the LastLoaded file.
                    if (File.Exists(lastLoadedFile))
                    {
                        var savePaths = Directory.GetDirectories(userPath);

                        // Still check every potential game world path.
                        foreach (var savePath in savePaths)
                        {
                            var saveResource = new WorldResource
                            {
                                Savename = Path.GetFileName(savePath),
                                UserName = Path.GetFileName(userPath),
                                Savepath = savePath,
                                DataPath = UserDataPath.FindFromSavePath(savePath)
                            };

                            saveResource.LoadCheckpoint();

                            if (saveResource.Savename.ToUpper() == findSession || saveResource.SessionName.ToUpper() == findSession)
                            {
                                return saveResource;
                            }
                        }
                    }
                }
            }

            return null;
        }

        internal static WorldResource LoadSession(string savePath)
        {
            if (Directory.Exists(savePath))
            {
                var userPath = Path.GetDirectoryName(savePath);

                var saveResource = new WorldResource
                {
                    Savename = Path.GetFileName(savePath),
                    UserName = Path.GetFileName(userPath),
                    Savepath = savePath,
                    DataPath = UserDataPath.FindFromSavePath(savePath)
                };

                saveResource.LoadCheckpoint();

                return saveResource;
            }

            return null;
        }

        #endregion
    }
}
