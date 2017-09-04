namespace SEToolbox.ViewModels
{
    using System.IO;
    using System.Windows.Input;

    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using VRageMath;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;
    using VRage.ObjectBuilders;
    using VRage;
    using VRage.Game;

    public class MergeVoxelViewModel : BaseViewModel
    {
        #region Fields

        private readonly MergeVoxelModel _dataModel;
        private bool? _closeResult;

        #endregion

        #region Constructors

        public MergeVoxelViewModel(BaseViewModel parentViewModel, MergeVoxelModel dataModel)
            : base(parentViewModel)
        {
            _dataModel = dataModel;

            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);

            MergeFileName = "merge";
        }

        #endregion

        #region command Properties

        public ICommand ApplyCommand
        {
            get { return new DelegateCommand(ApplyExecuted, ApplyCanExecute); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(CancelExecuted, CancelCanExecute); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get { return _closeResult; }

            set
            {
                _closeResult = value;
                RaisePropertyChanged(() => CloseResult);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get { return _dataModel.IsBusy; }
            set { _dataModel.IsBusy = value; }
        }

        public bool IsValidMerge
        {
            get { return _dataModel.IsValidMerge; }
            set { _dataModel.IsValidMerge = value; }
        }

        public MyObjectBuilder_VoxelMap NewEntity { get; set; }

        public StructureVoxelModel SelectionLeft
        {
            get { return (StructureVoxelModel)_dataModel.SelectionLeft; }
            set { _dataModel.SelectionLeft = value; }
        }

        public StructureVoxelModel SelectionRight
        {
            get { return (StructureVoxelModel)_dataModel.SelectionRight; }
            set { _dataModel.SelectionRight = value; }
        }

        public string SourceFile
        {
            get { return _dataModel.SourceFile; }
            set { _dataModel.SourceFile = value; }
        }

        public VoxelMergeType VoxelMergeType
        {
            get { return _dataModel.VoxelMergeType; }
            set { _dataModel.VoxelMergeType = value; }
        }

        public string MergeFileName
        {
            get { return _dataModel.MergeFileName; }
            set { _dataModel.MergeFileName = value; }
        }

        public bool RemoveOriginalAsteroids
        {
            get { return _dataModel.RemoveOriginalAsteroids; }
            set { _dataModel.RemoveOriginalAsteroids = value; }
        }

        #endregion

        #region methods

        public bool ApplyCanExecute()
        {
            return IsValidMerge;
        }

        public void ApplyExecuted()
        {
            CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            CloseResult = false;
        }

        #endregion

        public MyObjectBuilder_EntityBase BuildEntity()
        {
            // Realign both asteroids to a common grid, so voxels can be lined up.
            Vector3I roundedPosLeft = SelectionLeft.WorldAABB.Min.RoundToVector3I();
            Vector3D offsetPosLeft = SelectionLeft.WorldAABB.Min - (Vector3D)roundedPosLeft; // Use for everything.
            Vector3I roundedPosRight = (SelectionRight.WorldAABB.Min - offsetPosLeft).RoundToVector3I();
            Vector3D offsetPosRight = SelectionRight.WorldAABB.Min - (Vector3D)roundedPosRight; // Use for everything.

            // calculate smallest allowable size for contents of both.
            const int paddCells = 3;

            var minLeft = SelectionLeft.WorldAABB.Min + SelectionLeft.ContentBounds.Min - offsetPosLeft;
            var minRight = SelectionRight.WorldAABB.Min + SelectionRight.ContentBounds.Min - offsetPosRight;
            var min = Vector3D.Zero;
            var posOffset = Vector3D.Zero;
            var asteroidSize = Vector3I.Zero;

            switch (VoxelMergeType)
            {
                case VoxelMergeType.UnionVolumeLeftToRight:
                case VoxelMergeType.UnionVolumeRightToLeft:
                    min = Vector3D.Min(minLeft, minRight) - paddCells;
                    var max = Vector3D.Max(SelectionLeft.WorldAABB.Min + SelectionLeft.ContentBounds.Max - offsetPosLeft, SelectionRight.WorldAABB.Min + SelectionRight.ContentBounds.Max - offsetPosRight) + paddCells;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    var size = (max - min).RoundToVector3I();
                    asteroidSize = new Vector3I(size.X.RoundUpToNearest(64), size.Y.RoundUpToNearest(64), size.Z.RoundUpToNearest(64));
                    break;
                case VoxelMergeType.UnionMaterialLeftToRight:
                    min = SelectionRight.WorldAABB.Min - offsetPosRight;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    asteroidSize = SelectionRight.Size;
                    break;
                case VoxelMergeType.UnionMaterialRightToLeft:
                    min = SelectionLeft.WorldAABB.Min - offsetPosLeft;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    asteroidSize = SelectionLeft.Size;
                    break;
                case VoxelMergeType.SubtractVolumeLeftFromRight:
                    min = SelectionRight.WorldAABB.Min - offsetPosRight;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    asteroidSize = SelectionRight.Size;
                    break;
                case VoxelMergeType.SubtractVolumeRightFromLeft:
                    min = SelectionLeft.WorldAABB.Min - offsetPosLeft;
                    posOffset = new Vector3D(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
                    asteroidSize = SelectionLeft.Size;
                    break;
            }

            // Prepare new asteroid.
            var newAsteroid = new MyVoxelMap();
            newAsteroid.Create(asteroidSize, SpaceEngineersCore.Resources.GetDefaultMaterialName());
            newAsteroid.RemoveContent();
            if (string.IsNullOrEmpty(MergeFileName))
                MergeFileName = "merge";
            var filename = MainViewModel.CreateUniqueVoxelStorageName(MergeFileName);

            // merge.
            switch (VoxelMergeType)
            {
                case VoxelMergeType.UnionVolumeLeftToRight:
                    MergeAsteroidVolumeInto(ref newAsteroid, min, SelectionRight, SelectionLeft, minRight, minLeft);
                    break;
                case VoxelMergeType.UnionVolumeRightToLeft:
                    MergeAsteroidVolumeInto(ref newAsteroid, min, SelectionLeft, SelectionRight, minLeft, minRight);
                    break;
                case VoxelMergeType.UnionMaterialLeftToRight:
                    MergeAsteroidMaterialFrom(ref newAsteroid, min, SelectionRight, SelectionLeft, minRight, minLeft);
                    break;
                case VoxelMergeType.UnionMaterialRightToLeft:
                    MergeAsteroidMaterialFrom(ref newAsteroid, min, SelectionLeft, SelectionRight, minLeft, minRight);
                    break;
                case VoxelMergeType.SubtractVolumeLeftFromRight:
                    SubtractAsteroidVolumeFrom(ref newAsteroid, min, SelectionRight, SelectionLeft, minRight, minLeft);
                    break;
                case VoxelMergeType.SubtractVolumeRightFromLeft:
                    SubtractAsteroidVolumeFrom(ref newAsteroid, min, SelectionLeft, SelectionRight, minLeft, minRight);
                    break;
            }

            // Generate Entity
            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            newAsteroid.Save(tempfilename);
            SourceFile = tempfilename;

            var position = min + posOffset;
            var entity = new MyObjectBuilder_VoxelMap(position, filename)
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ASTEROID),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                StorageName = Path.GetFileNameWithoutExtension(filename),
                PositionAndOrientation = new MyPositionAndOrientation
                {
                    Position = position,
                    Forward = Vector3.Forward,
                    Up = Vector3.Up
                }
            };

            return entity;
        }

        #region MergeAsteroidVolumeInto

        private void MergeAsteroidVolumeInto(ref MyVoxelMap newAsteroid, Vector3D min, StructureVoxelModel modelPrimary, StructureVoxelModel modelSecondary, Vector3D minPrimary, Vector3D minSecondary)
        {
            var filenameSecondary = modelSecondary.SourceVoxelFilepath ?? modelSecondary.VoxelFilepath;
            var filenamePrimary = modelPrimary.SourceVoxelFilepath ?? modelPrimary.VoxelFilepath;
            Vector3I coords;

            var asteroid = new MyVoxelMap();
            asteroid.Load(filenameSecondary);

            for (coords.Z = (int)modelSecondary.ContentBounds.Min.Z; coords.Z <= modelSecondary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelSecondary.ContentBounds.Min.Y; coords.Y <= modelSecondary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelSecondary.ContentBounds.Min.X; coords.X <= modelSecondary.ContentBounds.Max.X; coords.X++)
                    {
                        byte volume;
                        string cellMaterial;
                        asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                        var newCoord = ((minSecondary - min) + ((Vector3D)coords - modelSecondary.ContentBounds.Min)).RoundToVector3I();
                        newAsteroid.SetVoxelContent(volume, ref newCoord);
                        newAsteroid.SetVoxelMaterialAndIndestructibleContent(cellMaterial, 0xff, ref newCoord);
                    }
                }
            }

            asteroid.Load(filenamePrimary);

            for (coords.Z = (int)modelPrimary.ContentBounds.Min.Z; coords.Z <= modelPrimary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelPrimary.ContentBounds.Min.Y; coords.Y <= modelPrimary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelPrimary.ContentBounds.Min.X; coords.X <= modelPrimary.ContentBounds.Max.X; coords.X++)
                    {
                        byte volume;
                        string cellMaterial;
                        asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                        if (volume > 0)
                        {
                            byte existingVolume;
                            string existingCellMaterial;

                            var newCoord = ((minPrimary - min) + ((Vector3D)coords - modelPrimary.ContentBounds.Min)).RoundToVector3I();
                            newAsteroid.GetVoxelMaterialContent(ref newCoord, out existingCellMaterial, out existingVolume);

                            if (volume > existingVolume)
                            {
                                newAsteroid.SetVoxelContent(volume, ref newCoord);
                            }
                            // Overwrites secondary material with primary.
                            newAsteroid.SetVoxelMaterialAndIndestructibleContent(cellMaterial, 0xff, ref newCoord);
                        }
                    }
                }
            }
        }

        #endregion

        #region SubtractAsteroidVolumeFrom

        private void SubtractAsteroidVolumeFrom(ref MyVoxelMap newAsteroid, Vector3D min, StructureVoxelModel modelPrimary, StructureVoxelModel modelSecondary, Vector3D minPrimary, Vector3D minSecondary)
        {
            var filenameSecondary = modelSecondary.SourceVoxelFilepath ?? modelSecondary.VoxelFilepath;
            var filenamePrimary = modelPrimary.SourceVoxelFilepath ?? modelPrimary.VoxelFilepath;
            Vector3I coords;

            var asteroid = new MyVoxelMap();
            asteroid.Load(filenamePrimary);

            for (coords.Z = (int)modelPrimary.ContentBounds.Min.Z; coords.Z <= modelPrimary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelPrimary.ContentBounds.Min.Y; coords.Y <= modelPrimary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelPrimary.ContentBounds.Min.X; coords.X <= modelPrimary.ContentBounds.Max.X; coords.X++)
                    {
                        byte volume;
                        string cellMaterial;
                        asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                        var newCoord = ((minPrimary - min) + ((Vector3D)coords - modelPrimary.ContentBounds.Min)).RoundToVector3I();
                        newAsteroid.SetVoxelContent(volume, ref newCoord);
                        newAsteroid.SetVoxelMaterialAndIndestructibleContent(cellMaterial, 0xff, ref newCoord);
                    }
                }
            }

            asteroid.Load(filenameSecondary);

            for (coords.Z = (int)modelSecondary.ContentBounds.Min.Z; coords.Z <= modelSecondary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelSecondary.ContentBounds.Min.Y; coords.Y <= modelSecondary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelSecondary.ContentBounds.Min.X; coords.X <= modelSecondary.ContentBounds.Max.X; coords.X++)
                    {
                        var newCoord = ((minSecondary - min) + ((Vector3D)coords - modelSecondary.ContentBounds.Min)).RoundToVector3I();
                        if (Vector3I.BoxContains(Vector3I.Zero, modelPrimary.Size - 1, newCoord))
                        {
                            byte volume;
                            string cellMaterial;
                            asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                            if (volume > 0)
                            {
                                byte existingVolume;
                                string existingCellMaterial;

                                newAsteroid.GetVoxelMaterialContent(ref newCoord, out existingCellMaterial, out existingVolume);

                                if (existingVolume - volume < 0)
                                    volume = 0;
                                else
                                    volume = (byte)(existingVolume - volume);

                                newAsteroid.SetVoxelContent(volume, ref newCoord);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region MergeAsteroidMaterialFrom

        private void MergeAsteroidMaterialFrom(ref MyVoxelMap newAsteroid, Vector3 min, StructureVoxelModel modelPrimary, StructureVoxelModel modelSecondary, Vector3 minPrimary, Vector3 minSecondary)
        {
            var filenameSecondary = modelSecondary.SourceVoxelFilepath ?? modelSecondary.VoxelFilepath;
            var filenamePrimary = modelPrimary.SourceVoxelFilepath ?? modelPrimary.VoxelFilepath;
            Vector3I coords;

            var asteroid = new MyVoxelMap();
            asteroid.Load(filenamePrimary);

            for (coords.Z = (int)modelPrimary.ContentBounds.Min.Z; coords.Z <= modelPrimary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelPrimary.ContentBounds.Min.Y; coords.Y <= modelPrimary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelPrimary.ContentBounds.Min.X; coords.X <= modelPrimary.ContentBounds.Max.X; coords.X++)
                    {
                        byte volume;
                        string cellMaterial;
                        asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                        var newCoord = ((minPrimary - min) + ((Vector3D)coords - modelPrimary.ContentBounds.Min)).RoundToVector3I();
                        newAsteroid.SetVoxelContent(volume, ref newCoord);
                        newAsteroid.SetVoxelMaterialAndIndestructibleContent(cellMaterial, 0xff, ref newCoord);
                    }
                }
            }

            asteroid.Load(filenameSecondary);

            for (coords.Z = (int)modelSecondary.ContentBounds.Min.Z; coords.Z <= modelSecondary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelSecondary.ContentBounds.Min.Y; coords.Y <= modelSecondary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelSecondary.ContentBounds.Min.X; coords.X <= modelSecondary.ContentBounds.Max.X; coords.X++)
                    {
                        var newCoord = ((minSecondary - min) + ((Vector3D)coords - modelSecondary.ContentBounds.Min)).RoundToVector3I();
                        if (Vector3I.BoxContains(Vector3I.Zero, modelPrimary.Size - 1, newCoord))
                        {
                            byte volume;
                            string cellMaterial;
                            asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                            if (volume > 0)
                            {
                                newAsteroid.SetVoxelMaterialAndIndestructibleContent(cellMaterial, 0xff, ref newCoord);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
