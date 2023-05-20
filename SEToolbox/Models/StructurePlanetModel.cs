namespace SEToolbox.Models
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using Sandbox.Engine.Voxels.Planet;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Support;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [Serializable]
    public class StructurePlanetModel : StructureBaseModel
    {
        #region fields

        private string _sourceVoxelFilepath;
        private string _voxelFilepath;
        private Vector3I _size;
        private Vector3D _contentCenter;

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
                    OnPropertyChanged(nameof(Name));
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
                    OnPropertyChanged(nameof(SourceVoxelFilepath));
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
                    OnPropertyChanged(nameof(VoxelFilepath));
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
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        [XmlIgnore]
        public int Seed
        {
            get { return Planet.Seed; }
            set
            {
                if (value != Planet.Seed)
                {
                    Planet.Seed = value;
                    OnPropertyChanged(nameof(Seed));
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
                    OnPropertyChanged(nameof(Radius));
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
                    OnPropertyChanged(nameof(HasAtmosphere));
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
                    OnPropertyChanged(nameof(AtmosphereRadius));
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
                    OnPropertyChanged(nameof(MinimumSurfaceRadius));
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
                    OnPropertyChanged(nameof(MaximumHillRadius));
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
                    OnPropertyChanged(nameof(GravityFalloff));
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
                    OnPropertyChanged(nameof(SurfaceGravity));
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
                    OnPropertyChanged(nameof(SpawnsFlora));
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
                    OnPropertyChanged(nameof(ShowGPS));
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
                    OnPropertyChanged(nameof(PlanetGenerator));
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
                    _contentCenter = _voxelMap.ContentCenter;
                    Center = new Vector3D(_contentCenter.X + 0.5f + PositionX, _contentCenter.Y + 0.5f + PositionY, _contentCenter.Z + 0.5f + PositionZ);

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
                _contentCenter = _voxelMap.ContentCenter;
                IsValid = _voxelMap.IsValid;
                OnPropertyChanged(nameof(Size), nameof(IsValid));
                Center = new Vector3D(_contentCenter.X + 0.5f + PositionX, _contentCenter.Y + 0.5f + PositionY, _contentCenter.Z + 0.5f + PositionZ);
                WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
            }
        }

        public override void RecalcPosition(Vector3D playerPosition)
        {
            base.RecalcPosition(playerPosition);
            Center = new Vector3D(_contentCenter.X + 0.5f + PositionX, _contentCenter.Y + 0.5f + PositionY, _contentCenter.Z + 0.5f + PositionZ);
            WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
        }

        /// <summary>
        /// Regenerate the Planet voxel.
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="radius"></param>
        public void RegeneratePlanet(int seed, float radius)
        {
            MyPlanetStorageProvider provider = new MyPlanetStorageProvider();
            MyPlanetGeneratorDefinition planetDefinition = MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(Planet.PlanetGenerator));
            provider.Init(seed, planetDefinition, radius, true);

            float minHillSize = provider.Radius * planetDefinition.HillParams.Min;
            float maxHillSize = provider.Radius * planetDefinition.HillParams.Max;

            float atmosphereRadius = planetDefinition.AtmosphereSettings.HasValue && planetDefinition.AtmosphereSettings.Value.Scale > 1f ? 1 + planetDefinition.AtmosphereSettings.Value.Scale : 1.75f;
            atmosphereRadius *= provider.Radius;

            Planet.Seed = seed;
            Planet.Radius = radius;
            Planet.AtmosphereRadius = atmosphereRadius;
            Planet.MinimumSurfaceRadius = radius + minHillSize;
            Planet.MaximumHillRadius = radius + maxHillSize;

            provider.Init(Planet.Seed, planetDefinition, radius, true);

            var asteroid = new MyVoxelMap();
            asteroid.Storage = new MyOctreeStorage(provider, provider.StorageSize);
            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            asteroid.Save(tempfilename);
            //SourceVoxelFilepath = tempfilename;
            UpdateNewSource(asteroid, tempfilename);

            OnPropertyChanged(nameof(Seed), nameof(Radius), nameof(AtmosphereRadius), nameof(MinimumSurfaceRadius), nameof(MaximumHillRadius));

            //Size = _voxelMap.Size;
            //_contentCenter = _voxelMap.ContentCenter;
            //IsValid = _voxelMap.IsValid;
            //OnPropertyChanged(nameof(Size);
            //OnPropertyChanged(nameof(IsValid);
            //Center = new Vector3D(_contentCenter.X + 0.5f + PositionX, _contentCenter.Y + 0.5f + PositionY, _contentCenter.Z + 0.5f + PositionZ);
            //WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
        }

        public void UpdateNewSource(MyVoxelMap newMap, string fileName)
        {
            if (_voxelMap != null)
                _voxelMap.Dispose();
            _voxelMap = newMap;
            SourceVoxelFilepath = fileName;

            Size = _voxelMap.Size;
            //ContentBounds = _voxelMap.BoundingContent;
            IsValid = _voxelMap.IsValid;

            OnPropertyChanged(nameof(Size));
            //OnPropertyChanged(nameof(ContentSize);
            OnPropertyChanged(nameof(IsValid));
            Center = new Vector3D(_voxelMap.ContentCenter.X + 0.5f + PositionX, _voxelMap.ContentCenter.Y + 0.5f + PositionY, _voxelMap.ContentCenter.Z + 0.5f + PositionZ);
            WorldAABB = new BoundingBoxD(PositionAndOrientation.Value.Position, PositionAndOrientation.Value.Position + new Vector3D(Size));
        }

        #endregion
    }
}
