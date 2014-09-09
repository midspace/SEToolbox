namespace SEToolbox.Models
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Microsoft.VisualBasic.FileIO;
    using Microsoft.Xml.Serialization.GeneratedAssembly;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;

    public class WorldResource : BaseModel
    {
        #region Fields

        private string _groupDescription;
        private SaveWorldType _saveType;
        private string _userName;
        private string _saveName;
        private string _savePath;
        private MyObjectBuilder_Checkpoint _content;
        private bool _compressedCheckpointFormat;
        private MyObjectBuilder_Sector _sectorData;
        private bool _compressedSectorFormat;
        private readonly SpaceEngineersResources _resources;

        /// <summary>
        /// Populated from LastLoadedTimes
        /// </summary>
        private DateTime _lastLoadTime;

        #endregion

        public WorldResource()
        {
            _resources = new SpaceEngineersResources();
        }

        #region Properties

        public string GroupDescription
        {
            get { return _groupDescription; }

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
            get { return _saveType; }

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
            get { return _userName; }

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
            get { return _saveName; }

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
            get { return _savePath; }

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
            get { return _content; }

            set
            {
                if (value != _content)
                {
                    _content = value;
                    RaisePropertyChanged(() => Content, () => SessionName, () => LastSaveTime, () => IsValid);
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
            get { return _lastLoadTime; }

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
            get { return _content.WorkshopId.HasValue; }
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
            get { return Content != null; }
        }

        public string ThumbnailImageFilename
        {
            get { return Path.Combine(_savePath, SpaceEngineersConsts.ThumbnailImageFilename); }
        }

        public override string ToString()
        {
            return SessionName;
        }

        public MyObjectBuilder_Sector SectorData
        {
            get { return _sectorData; }

            set
            {
                if (value != _sectorData)
                {
                    _sectorData = value;
                    RaisePropertyChanged(() => SectorData);
                }
            }
        }

        public SpaceEngineersResources Resources
        {
            get { return _resources; }
        }

        #endregion

        #region methods

        #region Load and Save

        /// <summary>
        /// Loads checkpoint file.
        /// </summary>
        public void LoadCheckpoint(bool snapshot = false)
        {
            var filename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);

            if (File.Exists(filename))
            {
                try
                {
                    Content = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Checkpoint>(filename, out _compressedCheckpointFormat, snapshot);
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

        public void LoadDefinitionsAndMods()
        {
            _resources.LoadDefinitionsAndMods(DataPath.ModsPath, Content.Mods.ToArray());
        }

        public void LoadSector(bool snapshot = false)
        {
            var filename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxSectorFilename);
            if (File.Exists(filename))
            {
                try
                {
                    SectorData = SpaceEngineersApi.ReadSpaceEngineersFile<MyObjectBuilder_Sector>(filename, out _compressedSectorFormat, snapshot);
                }
                catch
                {
                    SectorData = null;
                }
            }
            else
            {
                SectorData = null;
            }
        }

        public void SaveCheckPoint(bool backupFile)
        {
            var checkpointFilename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);

            if (backupFile)
            {
                var checkpointBackupFilename = checkpointFilename + ".bak";

                if (File.Exists(checkpointBackupFilename))
                {
                    FileSystem.DeleteFile(checkpointBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }

                File.Move(checkpointFilename, checkpointBackupFilename);
            }

            if (_compressedCheckpointFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(Content, tempFilename);
                ZipTools.GZipCompress(tempFilename, checkpointFilename);
            }
            else
            {
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Checkpoint, MyObjectBuilder_CheckpointSerializer>(Content, checkpointFilename);
            }
        }

        public void SaveSector(bool backupFile)
        {
            var sectorFilename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxSectorFilename);

            if (backupFile)
            {
                var sectorBackupFilename = sectorFilename + ".bak";

                if (File.Exists(sectorBackupFilename))
                {
                    FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }

                File.Move(sectorFilename, sectorBackupFilename);
            }

            if (_compressedSectorFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(SectorData, tempFilename);
                ZipTools.GZipCompress(tempFilename, sectorFilename);
            }
            else
            {
                SpaceEngineersApi.WriteSpaceEngineersFile<MyObjectBuilder_Sector, MyObjectBuilder_SectorSerializer>(SectorData, sectorFilename);
            }
        }

        public XmlDocument LoadSectorXml()
        {
            var filename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxCheckpointFilename);
            var xDoc = new XmlDocument();

            try
            {
                if (ZipTools.IsGzipedFile(filename))
                {
                    // New file format is compressed.
                    // These steps could probably be combined, but would have to use a MemoryStream, which has memory limits before it causes performance issues when chunking memory.
                    // Using a temporary file in this situation has less performance issues as it's moved straight to disk.
                    var tempFilename = TempfileUtil.NewFilename();
                    ZipTools.GZipUncompress(filename, tempFilename);
                    xDoc.Load(tempFilename);
                    _compressedCheckpointFormat = true;
                }
                else
                {
                    // Old file format is raw XML.
                    xDoc.Load(filename);
                    _compressedCheckpointFormat = false;
                }
            }
            catch
            {
                return null;
            }

            return xDoc;
        }

        public void SaveSectorXml(bool backupFile, XmlDocument xDoc)
        {
            var sectorFilename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxSectorFilename);

            if (backupFile)
            {
                var sectorBackupFilename = sectorFilename + ".bak";

                if (File.Exists(sectorBackupFilename))
                {
                    FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }

                File.Move(sectorFilename, sectorBackupFilename);
            }

            if (_compressedSectorFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                xDoc.Save(tempFilename);
                ZipTools.GZipCompress(tempFilename, sectorFilename);
            }
            else
            {
                xDoc.Save(sectorFilename);
            }
        }

        public void SaveCheckPointAndSector(bool backupFile)
        {
            LastSaveTime = DateTime.Now;
            Content.AppVersion = Sandbox.Common.MyFinalBuildConstants.APP_VERSION;
            SectorData.AppVersion = Sandbox.Common.MyFinalBuildConstants.APP_VERSION;
            SaveCheckPoint(backupFile);
            SaveSector(backupFile);
        }

        #endregion

        #region Miscellaneous

        public MyObjectBuilder_Character FindPlayerCharacter()
        {
            if (SectorData == null || Content == null)
                return null;

            foreach (var entityBase in SectorData.SectorObjects)
            {
                var character = entityBase as MyObjectBuilder_Character;
                if (character != null && character.EntityId == Content.ControlledObject)
                {
                    return character;
                }

                var cubeGrid = entityBase as MyObjectBuilder_CubeGrid;
                if (cubeGrid != null)
                {
                    var cockpit = (MyObjectBuilder_Cockpit)cubeGrid.CubeBlocks.FirstOrDefault(e => e.EntityId == Content.ControlledObject && e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Pilot != null);
                    if (cockpit != null)
                        return cockpit.Pilot;
                }
            }

            return null;
        }

        public MyObjectBuilder_Character FindAstronautCharacter()
        {
            return SectorData != null ? SectorData.SectorObjects.OfType<MyObjectBuilder_Character>().FirstOrDefault() : null;
        }

        public MyObjectBuilder_Cockpit FindPilotCharacter()
        {
            if (SectorData != null)
            {
                foreach (var entityBase in SectorData.SectorObjects)
                {
                    if (entityBase is MyObjectBuilder_CubeGrid)
                    {
                        var cubes = ((MyObjectBuilder_CubeGrid)entityBase).CubeBlocks.Where<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit && ((MyObjectBuilder_Cockpit)e).Pilot != null).ToList();
                        if (cubes.Count > 0)
                        {
                            return (MyObjectBuilder_Cockpit)cubes[0];
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #endregion
    }
}
