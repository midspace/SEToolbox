namespace SEToolbox.Models
{
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.IO;

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
                return this._groupDescription;
            }

            set
            {
                if (value != this._groupDescription)
                {
                    this._groupDescription = value;
                    this.RaisePropertyChanged(() => GroupDescription);
                }
            }
        }

        public SaveWorldType SaveType
        {
            get
            {
                return this._saveType;
            }

            set
            {
                if (value != this._saveType)
                {
                    this._saveType = value;
                    this.RaisePropertyChanged(() => SaveType);
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
                return this._userName;
            }

            set
            {
                if (value != this._userName)
                {
                    this._userName = value;
                    this.RaisePropertyChanged(() => UserName);
                }
            }
        }

        public string Savename
        {
            get
            {
                return this._saveName;
            }

            set
            {
                if (value != this._saveName)
                {
                    this._saveName = value;
                    this.RaisePropertyChanged(() => Savename);
                }
            }
        }

        public string Savepath
        {
            get
            {
                return this._savePath;
            }

            set
            {
                if (value != this._savePath)
                {
                    this._savePath = value;
                    this.RaisePropertyChanged(() => Savepath);
                }
            }
        }

        public UserDataPath DataPath { get; set; }

        public MyObjectBuilder_Checkpoint Content
        {
            get
            {
                return this._content;
            }

            set
            {
                if (value != this._content)
                {
                    this._content = value;
                    this.RaisePropertyChanged(() => Content, () => SessionName, () => LastSaveTime, () => IsValid);
                }
            }
        }

        public bool CompressedCheckpointFormat
        {
            get
            {
                return this._compressedCheckpointFormat;
            }

            set
            {
                if (value != this._compressedCheckpointFormat)
                {
                    this._compressedCheckpointFormat = value;
                    this.RaisePropertyChanged(() => CompressedCheckpointFormat);
                }
            }
        }

        public string SessionName
        {
            get
            {
                if (this._content == null)
                    return " # Invalid Save # ";

                return this._content.SessionName;
            }

            set
            {
                if (value != this._content.SessionName)
                {
                    this._content.SessionName = value;
                    this.RaisePropertyChanged(() => SessionName);
                }
            }
        }

        public DateTime? LastSaveTime
        {
            get
            {
                if (this._content == null)
                    return null;

                return this._content.LastSaveTime;
            }

            set
            {
                if (value != this._content.LastSaveTime)
                {
                    this._content.LastSaveTime = value.Value;
                    this.RaisePropertyChanged(() => LastSaveTime);
                }
            }
        }

        public DateTime LastLoadTime
        {
            get
            {
                return this._lastLoadTime;
            }

            set
            {
                if (value != this._lastLoadTime)
                {
                    this._lastLoadTime = value;
                    this.RaisePropertyChanged(() => LastLoadTime);
                }
            }
        }

        public bool IsWorkshopItem
        {
            get
            {
                return this._content.WorkshopId.HasValue;
            }
        }

        public ulong? WorkshopId
        {
            get
            {
                if (this._content == null)
                    return null;

                return this._content.WorkshopId;
            }
        }

        public bool IsValid
        {
            get
            {
                return this.Content != null;
            }
        }

        public string ThumbnailImageFilename
        {
            get
            {
                return Path.Combine(this._savePath, SpaceEngineersConsts.ThumbnailImageFilename);
            }
        }

        public override string ToString()
        {
            return this.SessionName;
        }

        #endregion

        /// <summary>
        /// Loads checkpoint file.
        /// </summary>
        public void LoadCheckpoint()
        {
            var filename = Path.Combine(this.Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);

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
                        this.Content = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(tempFilename);
                        this.CompressedCheckpointFormat = true;
                    }
                    else
                    {
                        this.Content = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(filename);
                        this.CompressedCheckpointFormat = false;
                    }
                }
                catch
                {
                    this.Content = null;
                }
            }
            else
            {
                this.Content = null;
            }
        }
    }
}
