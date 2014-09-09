namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;

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

            asteroid.ForceShellMaterial(materialName, 1);

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

        #endregion
    }
}
