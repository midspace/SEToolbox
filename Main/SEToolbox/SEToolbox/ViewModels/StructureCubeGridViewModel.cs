namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using System.Windows.Media.Media3D;

    public class StructureCubeGridViewModel : StructureBaseViewModel
    {
        #region Fields

        #endregion

        #region ctor

        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel)
            : base(parentViewModel, dataModel)
        {
        }

        #endregion

        #region Properties

        protected new StructureCubeGridModel DataModel
        {
            get
            {
                return base.DataModel as StructureCubeGridModel;
            }
        }

        public Sandbox.CommonLib.ObjectBuilders.MyCubeSize GridSize
        {
            get
            {
                return this.DataModel.GridSize;
            }

            set
            {
                if (value != this.DataModel.GridSize)
                {
                    this.DataModel.GridSize = value;
                    this.RaisePropertyChanged(() => GridSize);
                }
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
                if (value != this.DataModel.IsStatic)
                {
                    this.DataModel.IsStatic = value;
                    this.RaisePropertyChanged(() => IsStatic);
                }
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
                if (value != this.DataModel.Min)
                {
                    this.DataModel.Min = value;
                    this.RaisePropertyChanged(() => Min);
                }
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
                if (value != this.DataModel.Max)
                {
                    this.DataModel.Max = value;
                    this.RaisePropertyChanged(() => Max);
                }
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
                if (value != this.DataModel.Size)
                {
                    this.DataModel.Size = value;
                    this.RaisePropertyChanged(() => Size);
                }
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

        #endregion
    }
}
