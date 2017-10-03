namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Microsoft.VisualBasic.FileIO;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Components;

    public class WorldResource : BaseModel
    {
        #region Fields

        private string _groupDescription;
        private SaveWorldType _saveType;
        private string _userName;
        private string _saveName;
        private string _savePath;
        private MyObjectBuilder_Checkpoint _checkpoint;
        private bool _compressedCheckpointFormat;
        private MyObjectBuilder_Sector _sectorData;
        private bool _compressedSectorFormat;
        private readonly SpaceEngineersResources _resources;
        private Version _version;

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

        public MyObjectBuilder_Checkpoint Checkpoint
        {
            get { return _checkpoint; }

            set
            {
                if (value != _checkpoint)
                {
                    _checkpoint = value;
                    RaisePropertyChanged(() => Checkpoint, () => SessionName, () => LastSaveTime, () => IsValid);
                }
            }
        }

        public string SessionName
        {
            get
            {
                if (_checkpoint == null)
                    return " # Invalid Save # ";

                return _checkpoint.SessionName;
            }

            set
            {
                if (value != _checkpoint.SessionName)
                {
                    _checkpoint.SessionName = value;
                    RaisePropertyChanged(() => SessionName);
                }
            }
        }

        public DateTime? LastSaveTime
        {
            get
            {
                if (_checkpoint == null)
                    return null;

                return _checkpoint.LastSaveTime;
            }

            set
            {
                if (value != _checkpoint.LastSaveTime)
                {
                    _checkpoint.LastSaveTime = value.Value;
                    RaisePropertyChanged(() => LastSaveTime);
                }
            }
        }

        public Version Version
        {
            get
            {
                if (_version == null && _checkpoint != null)
                {
                    var str = _checkpoint.AppVersion.ToString(CultureInfo.InvariantCulture);
                    if (str == "0")
                        _version = new Version();
                    else
                    {
                        try
                        {
                            str = str.Substring(0, str.Length - 6) + "." + str.Substring(str.Length - 6, 3) + "." + str.Substring(str.Length - 3);
                            _version = new Version(str);
                        }
                        catch
                        {
                            _version = new Version();
                        }
                    }
                }
                return _version;
            }
        }

        public bool IsWorkshopItem
        {
            get { return _checkpoint.WorkshopId.HasValue; }
        }

        public ulong? WorkshopId
        {
            get
            {
                if (_checkpoint == null)
                    return null;

                return _checkpoint.WorkshopId;
            }
        }

        public bool IsValid
        {
            get { return Checkpoint != null; }
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

            MyObjectBuilder_Checkpoint checkpoint;
            SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Checkpoint>(filename, out checkpoint, out _compressedCheckpointFormat, snapshot);
            Checkpoint = checkpoint;
        }

        public void LoadDefinitionsAndMods()
        {
            if (_resources == null || Checkpoint == null || Checkpoint.Mods == null)
                return;

            var modList = Checkpoint.Mods.ToArray();
            for (int i = 0; i < modList.Length; i++)
                modList[i].FriendlyName = modList[i].Name;

            _resources.LoadDefinitionsAndMods(DataPath.ModsPath, modList);
        }

        public void LoadSector(bool snapshot = false)
        {
            var filename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxSectorFilename);
            MyObjectBuilder_Sector sectorData;

            SpaceEngineersApi.TryReadSpaceEngineersFile<MyObjectBuilder_Sector>(filename, out sectorData, out _compressedSectorFormat, snapshot);
            SectorData = sectorData;
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
                SpaceEngineersApi.WriteSpaceEngineersFile(Checkpoint, tempFilename);
                ZipTools.GZipCompress(tempFilename, checkpointFilename);
            }
            else
            {
                SpaceEngineersApi.WriteSpaceEngineersFile(Checkpoint, checkpointFilename);
            }
        }

        public void SaveSector(bool backupFile)
        {
            var sectorFilename = Path.Combine(Savepath, SpaceEngineersConsts.SandBoxSectorFilename);

            if (backupFile)
            {
                // xml sector file.  (it may or may not be compressed)
                var sectorBackupFilename = sectorFilename + ".bak";

                if (File.Exists(sectorBackupFilename))
                    FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                File.Move(sectorFilename, sectorBackupFilename);

                // binary sector file. (it may or may not be compressed)
                sectorBackupFilename = sectorFilename + SpaceEngineersConsts.ProtobuffersExtension + ".bak";

                if (File.Exists(sectorBackupFilename))
                    FileSystem.DeleteFile(sectorBackupFilename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                // The protoBuf .sbsPB may not exist in older save games.
                if (File.Exists(sectorFilename + SpaceEngineersConsts.ProtobuffersExtension))
                    File.Move(sectorFilename + SpaceEngineersConsts.ProtobuffersExtension, sectorBackupFilename);
            }

            if (_compressedSectorFormat)
            {
                var tempFilename = TempfileUtil.NewFilename();
                SpaceEngineersApi.WriteSpaceEngineersFile(SectorData, tempFilename);
                ZipTools.GZipCompress(tempFilename, sectorFilename);
            }
            else
            {
                SpaceEngineersApi.WriteSpaceEngineersFile(SectorData, sectorFilename);
            }
            SpaceEngineersApi.WriteSpaceEngineersFilePB(SectorData, sectorFilename + SpaceEngineersConsts.ProtobuffersExtension, _compressedSectorFormat);
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
            Checkpoint.AppVersion = SpaceEngineersConsts.GetSEVersionInt();
            SectorData.AppVersion = SpaceEngineersConsts.GetSEVersionInt();
            SaveCheckPoint(backupFile);
            SaveSector(backupFile);
        }

        #endregion

        #region Miscellaneous

        public MyObjectBuilder_Character FindPlayerCharacter()
        {
            if (SectorData == null || Checkpoint == null)
                return null;

            foreach (var entityBase in SectorData.SectorObjects)
            {
                var character = entityBase as MyObjectBuilder_Character;
                if (character != null && character.EntityId == Checkpoint.ControlledObject)
                {
                    return character;
                }

                var cubeGrid = entityBase as MyObjectBuilder_CubeGrid;
                if (cubeGrid != null)
                {
                    foreach (MyObjectBuilder_CubeBlock cube in cubeGrid.CubeBlocks.Where<MyObjectBuilder_CubeBlock>(e => e.EntityId == Checkpoint.ControlledObject && e is MyObjectBuilder_Cockpit))
                    {
                        List<MyObjectBuilder_Character> pilots = cube.GetHierarchyCharacters();
                        if (pilots.Count > 0)
                            return pilots[0];
                    }
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
                        foreach (MyObjectBuilder_CubeBlock cube in ((MyObjectBuilder_CubeGrid)entityBase).CubeBlocks.Where<MyObjectBuilder_CubeBlock>(e => e is MyObjectBuilder_Cockpit))
                        {
                            List<MyObjectBuilder_Character> pilots = cube.GetHierarchyCharacters();
                            if (pilots.Count > 0)
                                return (MyObjectBuilder_Cockpit)cube;
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
