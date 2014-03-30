namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;

    public class StructureVoxelViewModel : StructureBaseViewModel<StructureVoxelModel>
    {
        #region ctor

        public StructureVoxelViewModel(BaseViewModel parentViewModel, StructureVoxelModel dataModel)
            : base(parentViewModel, dataModel)
        {
            this.DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command properties

        public ICommand CopyDetailCommand
        {
            get
            {
                return new DelegateCommand(new Action(CopyDetailExecuted), new Func<bool>(CopyDetailCanExecute));
            }
        }

        #endregion

        #region Properties

        protected new StructureVoxelModel DataModel
        {
            get
            {
                return base.DataModel as StructureVoxelModel;
            }
        }

        public string Filename
        {
            get
            {
                return this.DataModel.Filename;
            }

            set
            {
                this.DataModel.Filename = value;
            }
        }

        public BindableSize3DIModel Size
        {
            get
            {
                return new BindableSize3DIModel(this.DataModel.Size);
            }

            set
            {
                this.DataModel.Size = value.ToVector3I();
            }
        }

        public BindableSize3DIModel ContentSize
        {
            get
            {
                return new BindableSize3DIModel(this.DataModel.ContentSize);
            }

            set
            {
                this.DataModel.ContentSize = value.ToVector3I();
            }
        }

        public long VoxCells
        {
            get
            {
                return this.DataModel.VoxCells;
            }

            set
            {
                this.DataModel.VoxCells = value;
            }
        }

        public double Volume
        {
            get
            {
                return this.DataModel.Volume;
            }
        }

        public double PositionX
        {
            get
            {
                return this.DataModel.PositionX;
            }

            set
            {
                this.DataModel.PositionX = value;
                this.MainViewModel.IsModified = true;
                this.MainViewModel.CalcDistances();
            }
        }

        public double PositionY
        {
            get
            {
                return this.DataModel.PositionY;
            }

            set
            {
                this.DataModel.PositionY = value;
                this.MainViewModel.IsModified = true;
                this.MainViewModel.CalcDistances();
            }
        }

        public double PositionZ
        {
            get
            {
                return this.DataModel.PositionZ;
            }

            set
            {
                this.DataModel.PositionZ = value;
                this.MainViewModel.IsModified = true;
                this.MainViewModel.CalcDistances();
            }
        }

        public List<VoxelMaterialAssetModel> MaterialAssets
        {
            get
            {
                return this.DataModel.MaterialAssets;
            }

            set
            {
                this.DataModel.MaterialAssets = value;
            }
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

            if (this.MaterialAssets != null)
            {
                foreach (var mat in this.MaterialAssets)
                {
                    ore.AppendFormat("{0}\t{1:#,##0.00} m³\t{2:P2}\r\n", mat.OreName, mat.Volume, mat.Percent);
                }
            }

            var detail = string.Format(Properties.Resources.VoxelDetail,
                Filename,
                Size.Width, Size.Height, Size.Depth,
                ContentSize.Width, ContentSize.Height, ContentSize.Depth,
                Volume,
                VoxCells,
                PlayerDistance,
                PositionAndOrientation.Value.Position.X, PositionAndOrientation.Value.Position.Y, PositionAndOrientation.Value.Position.Z,
                ore.ToString());

            Clipboard.SetText(detail);
        }

        #endregion
    }
}
