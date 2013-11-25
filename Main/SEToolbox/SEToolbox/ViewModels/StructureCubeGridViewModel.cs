namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

    public class StructureCubeGridViewModel : StructureBaseViewModel<StructureCubeGridModel>
    {
        #region ctor

        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel)
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

        protected new StructureCubeGridModel DataModel
        {
            get
            {
                return base.DataModel as StructureCubeGridModel;
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

        public Sandbox.CommonLib.ObjectBuilders.MyCubeSize GridSize
        {
            get
            {
                return this.DataModel.GridSize;
            }

            set
            {
                this.DataModel.GridSize = value;
            }
        }

        public bool IsStatic
        {
            get
            {
                return this.DataModel.IsStatic;
            }

            set
            {
                this.DataModel.IsStatic = value;
            }
        }

        public Point3D Min
        {
            get
            {
                return this.DataModel.Min;
            }

            set
            {
                this.DataModel.Min = value;
            }
        }

        public Point3D Max
        {
            get
            {
                return this.DataModel.Max;
            }

            set
            {
                this.DataModel.Max = value;
            }
        }

        public Vector3D Size
        {
            get
            {
                return this.DataModel.Size;
            }

            set
            {
                this.DataModel.Size = value;
            }
        }

        public bool IsPiloted
        {
            get
            {
                return this.DataModel.IsPiloted;
            }

            set
            {
                this.DataModel.IsPiloted = value;
            }
        }

        //public List<Cube> Cubes
        //{
        //    get
        //    {
        //        return this.cubes;
        //    }

        //    set
        //    {
        //        this.cubes = value;

        //        this.RaisePropertyChanged(() => Cubes);
        //    }
        //}

        #endregion

        #region methods

        public bool DeleteObjectCanExecute()
        {
            return !this.IsPiloted;
        }

        public void DeleteObjectExecuted()
        {
            ((ExplorerViewModel)this.OwnerViewModel).DeleteModel(this);
        }

        #endregion
    }
}
