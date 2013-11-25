//namespace SEToolbox.ViewModels
//{
//    using Sandbox.CommonLib.ObjectBuilders;
//    using SEToolbox.Interop;
//    using SEToolbox.Models;
//    using SEToolbox.Services;
//    using System;
//    using System.Collections.Generic;
//    using System.Windows.Input;
//    using System.Windows.Media.Media3D;

//    public class StructureCubeGridViewModel : StructureBaseViewModel<StructureCubeGridModel>
//    {
    
//        #region ctor

//        public StructureCubeGridViewModel(BaseViewModel parentViewModel, StructureCubeGridModel dataModel)
//            : base(parentViewModel, dataModel)
//        {
//        }

//        #endregion

//        #region Properties

//        public ICommand DeleteCubesCommand
//        {
//            get
//            {
//                return new DelegateCommand(new Action(DeleteCubesExecuted), new Func<bool>(DeleteCubesCanExecute));
//            }
//        }


//        protected new StructureCubeGridModel DataModel
//        {
//            get
//            {
//                return base.DataModel as StructureCubeGridModel;
//            }
//        }

//        public long EntityId
//        {
//            get
//            {
//                return this.DataModel.EntityBase.EntityId;
//            }

//            set
//            {
//                if (value != this.DataModel.EntityBase.EntityId)
//                {
//                    this.DataModel.EntityBase.EntityId = value;
//                    this.RaisePropertyChanged(() => EntityId);
//                }
//            }
//        }

//        public MyPositionAndOrientation? PositionAndOrientation
//        {
//            get
//            {
//                return this.DataModel.EntityBase.PositionAndOrientation;
//            }

//            set
//            {
//                if (!EqualityComparer<MyPositionAndOrientation?>.Default.Equals(value, this.DataModel.EntityBase.PositionAndOrientation))
//                //if (value != this.entityBase.PositionAndOrientation)
//                {
//                    this.DataModel.EntityBase.PositionAndOrientation = value;
//                    this.RaisePropertyChanged(() => PositionAndOrientation);
//                }
//            }
//        }

//        public ClassType ClassType
//        {
//            get
//            {
//                return this.DataModel.ClassType;
//            }

//            set
//            {
//                if (value != this.DataModel.ClassType)
//                {
//                    this.DataModel.ClassType = value;
//                    this.RaisePropertyChanged(() => ClassType);
//                }
//            }
//        }

//        public Sandbox.CommonLib.ObjectBuilders.MyCubeSize GridSize
//        {
//            get
//            {
//                return this.DataModel.GridSize;
//            }

//            set
//            {
//                if (value != this.DataModel.GridSize)
//                {
//                    this.DataModel.GridSize = value;
//                    this.RaisePropertyChanged(() => GridSize);
//                }
//            }
//        }

//        public bool IsStatic
//        {
//            get
//            {
//                return this.DataModel.IsStatic;
//            }

//            set
//            {
//                if (value != this.DataModel.IsStatic)
//                {
//                    this.DataModel.IsStatic = value;
//                    this.RaisePropertyChanged(() => IsStatic);
//                }
//            }
//        }

//        public Point3D Min
//        {
//            get
//            {
//                return this.DataModel.Min;
//            }

//            set
//            {
//                if (value != this.DataModel.Min)
//                {
//                    this.DataModel.Min = value;
//                    this.RaisePropertyChanged(() => Min);
//                }
//            }
//        }

//        public Point3D Max
//        {
//            get
//            {
//                return this.DataModel.Max;
//            }

//            set
//            {
//                if (value != this.DataModel.Max)
//                {
//                    this.DataModel.Max = value;
//                    this.RaisePropertyChanged(() => Max);
//                }
//            }
//        }

//        public Vector3D Size
//        {
//            get
//            {
//                return this.DataModel.Size;
//            }

//            set
//            {
//                if (value != this.DataModel.Size)
//                {
//                    this.DataModel.Size = value;
//                    this.RaisePropertyChanged(() => Size);
//                }
//            }
//        }

//        //public List<Cube> Cubes
//        //{
//        //    get
//        //    {
//        //        return this.cubes;
//        //    }

//        //    set
//        //    {
//        //        this.cubes = value;

//        //        this.RaisePropertyChanged(() => Cubes);
//        //    }
//        //}

//        #endregion

//        #region methods

//        public bool DeleteCubesCanExecute()
//        {
//            return true;
//        }

//        public void DeleteCubesExecuted()
//        {
//            //this.OwnerViewModel.
            
//        }

//        #endregion
//    }
//}
