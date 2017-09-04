namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using VRage.Game;
    using VRageMath;
    using VRage.ObjectBuilders;

    [Serializable]
    public class StructurePlanetModel : StructureBaseModel
    {
        #region fields

        private string _sourceVoxelFilepath;
        private string _voxelFilepath;
        private Vector3I _size;
        private BoundingBoxD _contentBounds;

        [NonSerialized]
        private BackgroundWorker _asyncWorker;

        [NonSerialized]
        private MyVoxelMap _voxelMap;

        [NonSerialized]
        private bool _isLoadingAsync;

        #endregion

        #region ctor

        public StructurePlanetModel(MyObjectBuilder_EntityBase entityBase, string voxelPath)
            : base(entityBase)
        {
            if (voxelPath != null)
            {
                VoxelFilepath = Path.Combine(voxelPath, Name + MyVoxelMap.V2FileExtension);
                var previewFile = VoxelFilepath;

                if (!File.Exists(VoxelFilepath))
                {
                    var oldFilepath = Path.Combine(voxelPath, Name + MyVoxelMap.V1FileExtension);
                    if (File.Exists(oldFilepath))
                    {
                        SourceVoxelFilepath = oldFilepath;
                        previewFile = oldFilepath;
                        SpaceEngineersCore.ManageDeleteVoxelList.Add(oldFilepath);
                    }
                }

                ReadVoxelDetails(previewFile);
            }
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public MyObjectBuilder_Planet Planet
        {
            get { return EntityBase as MyObjectBuilder_Planet; }
        }

        [XmlIgnore]
        public string Name
        {
            get { return Planet.StorageName; }

            set
            {
                if (value != Planet.StorageName)
                {
                    Planet.StorageName = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        /// <summary>
        /// This is the location of the temporary source file for importing/generating a Voxel file.
        /// </summary>
        public string SourceVoxelFilepath
        {
            get { return _sourceVoxelFilepath; }

            set
            {
                if (value != _sourceVoxelFilepath)
                {
                    _sourceVoxelFilepath = value;
                    RaisePropertyChanged(() => SourceVoxelFilepath);
                    ReadVoxelDetails(SourceVoxelFilepath);
                }
            }
        }

        /// <summary>
        /// This is the actual file/path for the Voxel file. It may not exist yet.
        /// </summary>
        public string VoxelFilepath
        {
            get { return _voxelFilepath; }

            set
            {
                if (value != _voxelFilepath)
                {
                    _voxelFilepath = value;
                    RaisePropertyChanged(() => VoxelFilepath);
                }
            }
        }

        [XmlIgnore]
        public Vector3I Size
        {
            get { return _size; }

            set
            {
                if (value != _size)
                {
                    _size = value;
                    RaisePropertyChanged(() => Size);
                }
            }
        }

        [XmlIgnore]
        public float Radius
        {
            get { return Planet.Radius; }
            set
            {
                if (value != Planet.Radius)
                {
                    Planet.Radius = value;
                    RaisePropertyChanged(() => Radius);
                }
            }
        }

        public bool HasAtmosphere
        {
            get { return Planet.HasAtmosphere; }
            set
            {
                if (value != Planet.HasAtmosphere)
                {
                    Planet.HasAtmosphere = value;
                    RaisePropertyChanged(() => HasAtmosphere);
                }
            }
        }

        [XmlIgnore]
        public float AtmosphereRadius
        {
            get { return Planet.AtmosphereRadius; }
            set
            {
                if (value != Planet.AtmosphereRadius)
                {
                    Planet.AtmosphereRadius = value;
                    RaisePropertyChanged(() => AtmosphereRadius);
                }
            }
        }

        [XmlIgnore]
        public float MinimumSurfaceRadius
        {
            get { return Planet.MinimumSurfaceRadius; }
            set
            {
                if (value != Planet.MinimumSurfaceRadius)
                {
                    Planet.MinimumSurfaceRadius = value;
                    RaisePropertyChanged(() => MinimumSurfaceRadius);
                }
            }
        }

        [XmlIgnore]
        public float MaximumHillRadius
        {
            get { return Planet.MaximumHillRadius; }
            set
            {
                if (value != Planet.MaximumHillRadius)
                {
                    Planet.MaximumHillRadius = value;
                    RaisePropertyChanged(() => MaximumHillRadius);
                }
            }
        }

        [XmlIgnore]
        public float GravityFalloff
        {
            get { return Planet.GravityFalloff; }
            set
            {
                if (value != Planet.GravityFalloff)
                {
                    Planet.GravityFalloff = value;
                    RaisePropertyChanged(() => GravityFalloff);
                }
            }
        }

        [XmlIgnore]
        public float SurfaceGravity
        {
            get { return Planet.SurfaceGravity; }
            set
            {
                if (value != Planet.SurfaceGravity)
                {
                    Planet.SurfaceGravity = value;
                    RaisePropertyChanged(() => SurfaceGravity);
                }
            }
        }

        [XmlIgnore]
        public bool SpawnsFlora
        {
            get { return Planet.SpawnsFlora; }
            set
            {
                if (value != Planet.SpawnsFlora)
                {
                    Planet.SpawnsFlora = value;
                    RaisePropertyChanged(() => SpawnsFlora);
                }
            }
        }

        [XmlIgnore]
        public bool ShowGPS
        {
            get { return Planet.ShowGPS; }
            set
            {
                if (value != Planet.ShowGPS)
                {
                    Planet.ShowGPS = value;
                    RaisePropertyChanged(() => ShowGPS);
                }
            }
        }

        [XmlIgnore]
        public string PlanetGenerator
        {
            get { return Planet.PlanetGenerator; }
            set
            {
                if (value != Planet.PlanetGenerator)
                {
                    Planet.PlanetGenerator = value;
                    RaisePropertyChanged(() => PlanetGenerator);
                }
            }
        }

        #endregion

        #region methods

        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context)
        {
            SerializedEntity = SpaceEngineersApi.Serialize<MyObjectBuilder_Planet>(Planet);
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            EntityBase = SpaceEngineersApi.Deserialize<MyObjectBuilder_Planet>(SerializedEntity);
        }

        public override void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Planet;
            DisplayName = Name;
        }

        public override void InitializeAsync()
        {
            _asyncWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _asyncWorker.DoWork += delegate
            {
                if (!_isLoadingAsync)
                {
                    _isLoadingAsync = true;

                    IsBusy = true;


                    // TODO: planet details
                    
                    _voxelMap.RefreshAssets();
                    _contentBounds = _voxelMap.BoundingContent;
                    Center = new Vector3D(_contentBounds.Center.X + 0.5f + PositionX, _contentBounds.Center.Y + 0.5f + PositionY, _contentBounds.Center.Z + 0.5f + PositionZ);

                    IsBusy = false;

                    _isLoadingAsync = false;
                }
            };

            _asyncWorker.RunWorkerAsync();
        }

        public override void CancelAsync()
        {
            if (_asyncWorker != null && _asyncWorker.IsBusy && _asyncWorker.WorkerSupportsCancellation)
            {
                _asyncWorker.CancelAsync();

                // TODO: kill file access to the Zip reader?
            }
        }

        private void ReadVoxelDetails(string filename)
        {
            if (filename != null && File.Exists(filename) && _voxelMap == null)
            {
                _voxelMap = new MyVoxelMap();
                _voxelMap.Load(filename);

                Size = _voxelMap.Size;
                _contentBounds = _voxelMap.BoundingContent;
                IsValid = _voxelMap.IsValid;
                RaisePropertyChanged(() => Size);
                RaisePropertyChanged(() => IsValid);
                Center = new Vector3D(_contentBounds.Center.X + 0.5f + PositionX, _contentBounds.Center.Y + 0.5f + PositionY, _contentBounds.Center.Z + 0.5f + PositionZ);
                WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
            }
        }

        public override void RecalcPosition(Vector3D playerPosition)
        {
            base.RecalcPosition(playerPosition);
            Center = new Vector3D(_contentBounds.Center.X + 0.5f + PositionX, _contentBounds.Center.Y + 0.5f + PositionY, _contentBounds.Center.Z + 0.5f + PositionZ);
            WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
        }

        #endregion
    }
}
