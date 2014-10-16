namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using VRageMath;
    using SEToolbox.Interop.Asteroids;

    public class VoxelMergeViewModel : BaseViewModel
    {
        #region Fields

        private readonly VoxelMergeModel _dataModel;
        private bool? _closeResult;

        #endregion

        #region Constructors

        public VoxelMergeViewModel(BaseViewModel parentViewModel, VoxelMergeModel dataModel)
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
            var modelLeft = (StructureVoxelModel)SelectionLeft;
            var modelRight = (StructureVoxelModel)SelectionRight;
            var filenameLeft = modelLeft.SourceVoxelFilepath ?? modelLeft.VoxelFilepath;
            var filenameRight = modelRight.SourceVoxelFilepath ?? modelRight.VoxelFilepath;

            // Realign both asteroids to a common grid, so voxels can be lined up.
            Vector3I roundedPosLeft = modelLeft.AABB.Min.RoundToVector3I();
            Vector3 offsetPosLeft = modelLeft.AABB.Min - roundedPosLeft; // Use for everything.
            Vector3I roundedPosRight = (modelRight.AABB.Min - offsetPosLeft).RoundToVector3I();
            Vector3 offsetPosRight = modelRight.AABB.Min - roundedPosRight; // Use for everything.

            // calculate smallest allowable size for contents of both.
            const int paddCells = 3;
            var minLeft = modelLeft.AABB.Min + modelLeft.ContentBounds.Min - offsetPosLeft;
            var minRight = modelRight.AABB.Min + modelRight.ContentBounds.Min - offsetPosRight;
            var min = Vector3.Min(minLeft, minRight) - paddCells;
            var max = Vector3.Max(modelLeft.AABB.Min + modelLeft.ContentBounds.Max - offsetPosLeft, modelRight.AABB.Min + modelRight.ContentBounds.Max - offsetPosRight) + paddCells;
            var posOffset = new Vector3(minLeft.X < minRight.X ? offsetPosLeft.X : offsetPosRight.X, minLeft.Y < minRight.Y ? offsetPosLeft.Y : offsetPosRight.Y, minLeft.Z < minRight.Z ? offsetPosLeft.Z : offsetPosRight.Z);
            var size = (max - min).RoundToVector3I();
            var asteroidSize = new Vector3I(size.X.RoundUpToNearest(64), size.Y.RoundUpToNearest(64), size.Z.RoundUpToNearest(64));

            // Prepare new asteroid.
            var newAsteroid = new MyVoxelMap();
            newAsteroid.Init(Vector3.Zero, asteroidSize, SpaceEngineersCore.Resources.GetDefaultMaterialName());
            newAsteroid.RemoveContent();
            if (string.IsNullOrEmpty(MergeFileName))
                MergeFileName = "merge";
            var filename = MainViewModel.CreateUniqueVoxelStorageName(MergeFileName);

            // merge.
            switch (VoxelMergeType)
            {
                case Support.VoxelMergeType.UnionLeftToRight:
                    MergeAsteroidInto(ref newAsteroid, min, modelRight, modelLeft, minRight, minLeft);
                    break;
                case Support.VoxelMergeType.UnionRightToLeft:
                    MergeAsteroidInto(ref newAsteroid, min, modelLeft, modelRight, minLeft, minRight);
                    break;
                case Support.VoxelMergeType.SubtractLeftFromRight:
                    SubtractAsteroidFrom(ref newAsteroid, min, modelRight, modelLeft, minRight, minLeft);
                    break;
                case Support.VoxelMergeType.SubtractRightFromLeft:
                    SubtractAsteroidFrom(ref newAsteroid, min, modelLeft, modelRight, minLeft, minRight);
                    break;
            }

            // Generate Entity
            var tempfilename = TempfileUtil.NewFilename(MyVoxelMap.V2FileExtension);
            newAsteroid.Save(tempfilename);
            this.SourceFile = tempfilename;

            var position = min + posOffset;
            var entity = new MyObjectBuilder_VoxelMap(position, filename)
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                Filename = filename,
                PositionAndOrientation = new MyPositionAndOrientation
                {
                    Position = position,
                    Forward = Vector3.Forward,
                    Up = Vector3.Up
                }
            };

            return entity;
        }

        #region MergeAsteroidInto

        private void MergeAsteroidInto(ref MyVoxelMap newAsteroid, Vector3 min, StructureVoxelModel modelPrimary, StructureVoxelModel modelSecondary, Vector3 minPrimary, Vector3 minSecondary)
        {
            var filenameSecondary = modelSecondary.SourceVoxelFilepath ?? modelSecondary.VoxelFilepath;
            var filenamePrimary = modelPrimary.SourceVoxelFilepath ?? modelPrimary.VoxelFilepath;
            Vector3I coords;

            var asteroid = new MyVoxelMap();
            asteroid.Load(filenameSecondary, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            for (coords.Z = (int)modelSecondary.ContentBounds.Min.Z; coords.Z <= modelSecondary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelSecondary.ContentBounds.Min.Y; coords.Y <= modelSecondary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelSecondary.ContentBounds.Min.X; coords.X <= modelSecondary.ContentBounds.Max.X; coords.X++)
                    {
                        byte volume = 0xff;
                        string cellMaterial;
                        asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                        var newCoord = ((minSecondary - min) + (coords - modelSecondary.ContentBounds.Min)).RoundToVector3I();
                        newAsteroid.SetVoxelContent(volume, ref newCoord);
                        newAsteroid.SetVoxelMaterialAndIndestructibleContent(cellMaterial, 0xff, ref newCoord);
                    }
                }
            }

            asteroid.Load(filenamePrimary, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            for (coords.Z = (int)modelPrimary.ContentBounds.Min.Z; coords.Z <= modelPrimary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelPrimary.ContentBounds.Min.Y; coords.Y <= modelPrimary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelPrimary.ContentBounds.Min.X; coords.X <= modelPrimary.ContentBounds.Max.X; coords.X++)
                    {
                        byte volume = 0xff;
                        string cellMaterial;
                        asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                        if (volume > 0)
                        {
                            byte existingVolume = 0xff;
                            string existingCellMaterial;

                            var newCoord = ((minPrimary - min) + (coords - modelPrimary.ContentBounds.Min)).RoundToVector3I();
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

        #region SubtractAsteroidFrom

        private void SubtractAsteroidFrom(ref MyVoxelMap newAsteroid, Vector3 min, StructureVoxelModel modelPrimary, StructureVoxelModel modelSecondary, Vector3 minPrimary, Vector3 minSecondary)
        {
            var filenameSecondary = modelSecondary.SourceVoxelFilepath ?? modelSecondary.VoxelFilepath;
            var filenamePrimary = modelPrimary.SourceVoxelFilepath ?? modelPrimary.VoxelFilepath;
            Vector3I coords;

            var asteroid = new MyVoxelMap();
            asteroid.Load(filenamePrimary, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            for (coords.Z = (int)modelPrimary.ContentBounds.Min.Z; coords.Z <= modelPrimary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelPrimary.ContentBounds.Min.Y; coords.Y <= modelPrimary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelPrimary.ContentBounds.Min.X; coords.X <= modelPrimary.ContentBounds.Max.X; coords.X++)
                    {
                        byte volume = 0xff;
                        string cellMaterial;
                        asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                        var newCoord = ((minPrimary - min) + (coords - modelPrimary.ContentBounds.Min)).RoundToVector3I();
                        newAsteroid.SetVoxelContent(volume, ref newCoord);
                        newAsteroid.SetVoxelMaterialAndIndestructibleContent(cellMaterial, 0xff, ref newCoord);
                    }
                }
            }

            asteroid.Load(filenameSecondary, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            for (coords.Z = (int)modelSecondary.ContentBounds.Min.Z; coords.Z <= modelSecondary.ContentBounds.Max.Z; coords.Z++)
            {
                for (coords.Y = (int)modelSecondary.ContentBounds.Min.Y; coords.Y <= modelSecondary.ContentBounds.Max.Y; coords.Y++)
                {
                    for (coords.X = (int)modelSecondary.ContentBounds.Min.X; coords.X <= modelSecondary.ContentBounds.Max.X; coords.X++)
                    {
                        byte volume = 0xff;
                        string cellMaterial;
                        asteroid.GetVoxelMaterialContent(ref coords, out cellMaterial, out volume);

                        if (volume > 0)
                        {
                            byte existingVolume = 0xff;
                            string existingCellMaterial;

                            var newCoord = ((minSecondary - min) + (coords - modelSecondary.ContentBounds.Min)).RoundToVector3I();
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

        #endregion


    }
}
