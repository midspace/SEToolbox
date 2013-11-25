namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.ComponentModel;
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

        #region Properties

        public ICommand DeleteObjectCommand
        {
            get
            {
                return new DelegateCommand(new Action(DeleteObjectExecuted), new Func<bool>(DeleteObjectCanExecute));
            }
        }

        protected new StructureVoxelModel DataModel
        {
            get
            {
                return base.DataModel as StructureVoxelModel;
            }
        }

        //public long EntityId
        //{
        //    get
        //    {
        //        return this.DataModel.EntityBase.EntityId;
        //    }

        //    set
        //    {
        //        if (value != this.DataModel.EntityBase.EntityId)
        //        {
        //            this.DataModel.EntityBase.EntityId = value;
        //            this.RaisePropertyChanged(() => EntityId);
        //        }
        //    }
        //}

        //public MyPositionAndOrientation? PositionAndOrientation
        //{
        //    get
        //    {
        //        return this.DataModel.EntityBase.PositionAndOrientation;
        //    }

        //    set
        //    {
        //        if (!EqualityComparer<MyPositionAndOrientation?>.Default.Equals(value, this.DataModel.EntityBase.PositionAndOrientation))
        //        //if (value != this.entityBase.PositionAndOrientation)
        //        {
        //            this.DataModel.EntityBase.PositionAndOrientation = value;
        //            this.RaisePropertyChanged(() => PositionAndOrientation);
        //        }
        //    }
        //}

        //public ClassType ClassType
        //{
        //    get
        //    {
        //        return this.DataModel.ClassType;
        //    }

        //    set
        //    {
        //        if (value != this.DataModel.ClassType)
        //        {
        //            this.DataModel.ClassType = value;
        //            this.RaisePropertyChanged(() => ClassType);
        //        }
        //    }
        //}

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

        #endregion

        #region methods

        public bool DeleteObjectCanExecute()
        {
            return true;
        }

        public void DeleteObjectExecuted()
        {
            ((ExplorerViewModel)this.OwnerViewModel).DeleteModel(this);
        }

        #endregion
    }
}
