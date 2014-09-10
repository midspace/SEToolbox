namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;

    public class StructureVoxelViewModel : StructureBaseViewModel<StructureVoxelModel>
    {
        #region ctor

        public StructureVoxelViewModel(BaseViewModel parentViewModel, StructureVoxelModel dataModel)
            : base(parentViewModel, dataModel)
        {
            DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command properties

        public ICommand CopyDetailCommand
        {
            get { return new DelegateCommand(CopyDetailExecuted, CopyDetailCanExecute); }
        }

        public ICommand ReseedCommand
        {
            get { return new DelegateCommand(ReseedExecuted, ReseedCanExecute); }
        }

        public ICommand ReplaceSurfaceCommand
        {
            get { return new DelegateCommand<string>(ReplaceSurfaceExecuted, ReplaceSurfaceCanExecute); }
        }

        public ICommand ReplaceAllCommand
        {
            get { return new DelegateCommand<string>(ReplaceAllExecuted, ReplaceAllCanExecute); }
        }

        public ICommand ReplaceSelectedMenuCommand
        {
            get { return new DelegateCommand<string>(new Func<string, bool>(ReplaceSelectedMenuCanExecute)); }
        }

        public ICommand ReplaceSelectedCommand
        {
            get { return new DelegateCommand<string>(ReplaceSelectedExecuted, ReplaceSelectedCanExecute); }
        }

        public ICommand SliceQuarterCommand
        {
            get { return new DelegateCommand(SliceQuarterExecuted, SliceQuarterCanExecute); }
        }

        public ICommand SliceHalfCommand
        {
            get { return new DelegateCommand(SliceHalfExecuted, SliceHalfCanExecute); }
        }

        #endregion

        #region Properties

        protected new StructureVoxelModel DataModel
        {
            get { return base.DataModel as StructureVoxelModel; }
        }

        public string Filename
        {
            get { return DataModel.Filename; }
            set { DataModel.Filename = value; }
        }

        public BindableSize3DIModel Size
        {
            get { return new BindableSize3DIModel(DataModel.Size); }
            set { DataModel.Size = value.ToVector3I(); }
        }

        public BindableSize3DIModel ContentSize
        {
            get { return new BindableSize3DIModel(DataModel.ContentSize); }
            set { DataModel.ContentSize = value.ToVector3I(); }
        }

        public BindableVector3DModel Center
        {
            get { return new BindableVector3DModel(DataModel.Center); }
            set { DataModel.Center = value.ToVector3(); }
        }

        public long VoxCells
        {
            get { return DataModel.VoxCells; }
            set { DataModel.VoxCells = value; }
        }

        public double Volume
        {
            get { return DataModel.Volume; }
        }

        public List<VoxelMaterialAssetModel> MaterialAssets
        {
            get { return DataModel.MaterialAssets; }
            set { DataModel.MaterialAssets = value; }
        }

        public VoxelMaterialAssetModel SelectedMaterialAsset
        {
            get { return DataModel.SelectedMaterialAsset; }
            set { DataModel.SelectedMaterialAsset = value; }
        }

        public List<VoxelMaterialAssetModel> GameMaterialList
        {
            get { return DataModel.GameMaterialList; }
            set { DataModel.GameMaterialList = value; }
        }

        public List<VoxelMaterialAssetModel> EditMaterialList
        {
            get { return DataModel.EditMaterialList; }
            set { DataModel.EditMaterialList = value; }
        }

        #endregion

        #region methods

        public bool CopyDetailCanExecute()
        {
            return true;
        }

        public void CopyDetailExecuted()
        {
            var ore = new StringBuilder();

            if (MaterialAssets != null)
            {
                foreach (var mat in MaterialAssets)
                {
                    ore.AppendFormat("{0}\t{1:#,##0.00} m³\t{2:P2}\r\n", mat.MaterialName, mat.Volume, mat.Percent);
                }
            }

            var detail = string.Format(Properties.Resources.VoxelDetail,
                Filename,
                Size.Width, Size.Height, Size.Depth,
                ContentSize.Width, ContentSize.Height, ContentSize.Depth,
                Center.X, Center.Y, Center.Z,
                Volume,
                VoxCells,
                PlayerDistance,
                PositionAndOrientation.Value.Position.X, PositionAndOrientation.Value.Position.Y, PositionAndOrientation.Value.Position.Z,
                ore);

            Clipboard.SetText(detail);
        }

        public bool ReseedCanExecute()
        {
            return true;
        }

        public void ReseedExecuted()
        {
            MainViewModel.IsBusy = true;
            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            var cellCount = asteroid.SumVoxelCells();

            // TODO: regenerate the materials inside of the asteroid randomly.


            var tempfilename = TempfileUtil.NewFilename();
            asteroid.Save(tempfilename);
            DataModel.SourceVoxelFilepath = tempfilename;

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;

            DataModel.MaterialAssets = null;
            DataModel.InitializeAsync();
        }

        public bool ReplaceSurfaceCanExecute(string materialName)
        {
            return true;
        }

        public void ReplaceSurfaceExecuted(string materialName)
        {
            MainViewModel.IsBusy = true;
            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            asteroid.ForceShellMaterial(materialName, 2);

            var tempfilename = TempfileUtil.NewFilename();
            asteroid.Save(tempfilename);
            DataModel.SourceVoxelFilepath = tempfilename;

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;

            DataModel.MaterialAssets = null;
            DataModel.InitializeAsync();
        }

        public bool ReplaceAllCanExecute(string materialName)
        {
            return true;
        }

        public void ReplaceAllExecuted(string materialName)
        {
            MainViewModel.IsBusy = true;
            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            asteroid.ForceBaseMaterial(materialName, materialName);
            asteroid.ForceVoxelFaceMaterial(SpaceEngineersCore.Resources.GetDefaultMaterialName());

            var tempfilename = TempfileUtil.NewFilename();
            asteroid.Save(tempfilename);
            DataModel.SourceVoxelFilepath = tempfilename;

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;

            DataModel.MaterialAssets = null;
            DataModel.InitializeAsync();
        }

        public bool ReplaceSelectedMenuCanExecute(string materialName)
        {
            return SelectedMaterialAsset != null;
        }

        public bool ReplaceSelectedCanExecute(string materialName)
        {
            return SelectedMaterialAsset != null;
        }

        public void ReplaceSelectedExecuted(string materialName)
        {
            MainViewModel.IsBusy = true;
            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            if (string.IsNullOrEmpty(materialName))
            {
                asteroid.RemoveMaterial(SelectedMaterialAsset.MaterialName, null);
                DataModel.VoxCells = asteroid.SumVoxelCells();
            }
            else
                asteroid.ReplaceMaterial(SelectedMaterialAsset.MaterialName, materialName);

            var tempfilename = TempfileUtil.NewFilename();
            asteroid.Save(tempfilename);
            DataModel.SourceVoxelFilepath = tempfilename;


            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;

            DataModel.UpdateGeneralFromEntityBase();
            DataModel.MaterialAssets = null;
            DataModel.InitializeAsync();
        }

        public bool SliceQuarterCanExecute()
        {
            return true;
        }

        public void SliceQuarterExecuted()
        {
            MainViewModel.IsBusy = true;
            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            var height = asteroid.ContentSize.Y;

            // remove the Top half.
            asteroid.RemoveMaterial((int)Math.Round(asteroid.ContentCenter.X, 0), asteroid.Size.X, (int)Math.Round(asteroid.ContentCenter.Y, 0), asteroid.Size.Y, 0, (int)Math.Round(asteroid.ContentCenter.Z, 0));

            var tempfilename = TempfileUtil.NewFilename();
            asteroid.Save(tempfilename);

            var newFilename = MainViewModel.CreateUniqueVoxelFilename(DataModel.Filename);
            var posOrient = DataModel.PositionAndOrientation.HasValue ? DataModel.PositionAndOrientation.Value : new MyPositionAndOrientation();
            posOrient.Position.y += height;

            // genreate a new Asteroid entry.
            var newEntity = new MyObjectBuilder_VoxelMap(posOrient.Position, newFilename)
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                Filename = newFilename,
                PositionAndOrientation = new MyPositionAndOrientation
                {
                    Position = posOrient.Position,
                    Forward = posOrient.Forward,
                    Up = posOrient.Up
                }
            };

            var structure = MainViewModel.AddEntity(newEntity);
            ((StructureVoxelModel)structure).SourceVoxelFilepath = tempfilename; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        public bool SliceHalfCanExecute()
        {
            return true;
        }

        public void SliceHalfExecuted()
        {
            MainViewModel.IsBusy = true;
            var sourceFile = DataModel.SourceVoxelFilepath ?? DataModel.VoxelFilepath;

            var asteroid = new MyVoxelMap();
            asteroid.Load(sourceFile, SpaceEngineersCore.Resources.GetDefaultMaterialName(), true);

            var height = asteroid.ContentSize.Y;

            // remove the Top half.
            asteroid.RemoveMaterial(null, null, (int)Math.Round(asteroid.ContentCenter.Y, 0), asteroid.Size.Y, null, null);

            var tempfilename = TempfileUtil.NewFilename();
            asteroid.Save(tempfilename);

            var newFilename = MainViewModel.CreateUniqueVoxelFilename(DataModel.Filename);
            var posOrient = DataModel.PositionAndOrientation.HasValue ? DataModel.PositionAndOrientation.Value : new MyPositionAndOrientation();
            posOrient.Position.y += height;

            // genreate a new Asteroid entry.
            var newEntity = new MyObjectBuilder_VoxelMap(posOrient.Position, newFilename)
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(),
                PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                Filename = newFilename,
                PositionAndOrientation = new MyPositionAndOrientation
                {
                    Position = posOrient.Position,
                    Forward = posOrient.Forward,
                    Up = posOrient.Up
                }
            };

            var structure = MainViewModel.AddEntity(newEntity);
            ((StructureVoxelModel)structure).SourceVoxelFilepath = tempfilename; // Set the temporary file location of the Source Voxel, as it hasn't been written yet.

            MainViewModel.IsModified = true;
            MainViewModel.IsBusy = false;
        }

        #endregion
    }
}
