namespace SEToolbox.Models
{
    using System;
    using System.IO;

    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;

    public class SaveResource : BaseModel
    {
        #region Fields

        private string _groupDescription;
        private SaveWorldType _saveType;
        private string _userName;
        private string _saveName;
        private string _savePath;
        private MyObjectBuilder_Checkpoint _content;
        private bool _compressedCheckpointFormat;

        /// <summary>
        /// Populated from LastLoadedTimes
        /// </summary>
        private DateTime _lastLoadTime;

        #endregion

        #region Properties

        public string GroupDescription
        {
            get
            {
                return _groupDescription;
            }

            set
            {
                if (value != _groupDescription)
                {
                    _groupDescription = value;
                    RaisePropertyChanged(() => GroupDescription);
                }
            }
        }

        public SaveWorldType SaveType
        {
            get
            {
                return _saveType;
            }

            set
            {
                if (value != _saveType)
                {
                    _saveType = value;
                    RaisePropertyChanged(() => SaveType);
                }
            }
        }

        /// <summary>
        /// This will be the SteamId of the local user, or the Instance name of the Server.
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;
            }

            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    RaisePropertyChanged(() => UserName);
                }
            }
        }

        public string Savename
        {
            get
            {
                return _saveName;
            }

            set
            {
                if (value != _saveName)
                {
                    _saveName = value;
                    RaisePropertyChanged(() => Savename);
                }
            }
        }

        public string Savepath
        {
            get
            {
                return _savePath;
            }

            set
            {
                if (value != _savePath)
                {
                    _savePath = value;
                    RaisePropertyChanged(() => Savepath);
                }
            }
        }

        public UserDataPath DataPath { get; set; }

        public MyObjectBuilder_Checkpoint Content
        {
            get
            {
                return _content;
            }

            set
            {
                if (value != _content)
                {
                    _content = value;
                    RaisePropertyChanged(() => Content, () => SessionName, () => LastSaveTime, () => IsValid);
                }
            }
        }

        public bool CompressedCheckpointFormat
        {
            get
            {
                return _compressedCheckpointFormat;
            }

            set
            {
                if (value != _compressedCheckpointFormat)
                {
                    _compressedCheckpointFormat = value;
                    RaisePropertyChanged(() => CompressedCheckpointFormat);
                }
            }
        }

        public string SessionName
        {
            get
            {
                if (_content == null)
                    return " # Invalid Save # ";

                return _content.SessionName;
            }

            set
            {
                if (value != _content.SessionName)
                {
                    _content.SessionName = value;
                    RaisePropertyChanged(() => SessionName);
                }
            }
        }

        public DateTime? LastSaveTime
        {
            get
            {
                if (_content == null)
                    return null;

                return _content.LastSaveTime;
            }

            set
            {
                if (value != _content.LastSaveTime)
                {
                    _content.LastSaveTime = value.Value;
                    RaisePropertyChanged(() => LastSaveTime);
                }
            }
        }

        public DateTime LastLoadTime
        {
            get
            {
                return _lastLoadTime;
            }

            set
            {
                if (value != _lastLoadTime)
                {
                    _lastLoadTime = value;
                    RaisePropertyChanged(() => LastLoadTime);
                }
            }
        }

        public bool IsWorkshopItem
        {
            get
            {
                return _content.WorkshopId.HasValue;
            }
        }

        public ulong? WorkshopId
        {
            get
            {
                if (_content == null)
                    return null;

                return _content.WorkshopId;
            }
        }

        public bool IsValid
        {
            get
            {
                return Content != null;
            }
        }

        public string ThumbnailImageFilename
        {
            get
            {
                return Path.Combine(_savePath, SpaceEngineersConsts.ThumbnailImageFilename);
            }
        }

        public override string ToString()
        {
            return SessionName;
        }

        #endregion

        /// <summary>
        /// Loads checkpoint file.
        /// </summary>
        public void LoadCheckpoint()
        {
            var filename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);

            if (File.Exists(filename))
            {
                try
                {
                    if (ZipTools.IsGzipedFile(filename))
                    {
                        // New file format is compressed.
                        // These steps could probably be combined, but would have to use a MemoryStream, which has memory limits before it causes performance issues when chunking memory.
                        // Using a temporary file in this situation has less performance issues as it's moved straight to disk.
                        var tempFilename = TempfileUtil.NewFilename();
                        ZipTools.GZipUncompress(filename, tempFilename);
                        Content = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(tempFilename);
                        CompressedCheckpointFormat = true;
                    }
                    else
                    {
                        Content = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(filename);
                        CompressedCheckpointFormat = false;
                    }
                }
                catch
                {
                    Content = null;
                }
            }
            else
            {
                Content = null;
            }
        }
    }
}
