//namespace SEToolbox.ViewModels
//{
//    using Sandbox.CommonLib.ObjectBuilders;
//    using SEToolbox.Interop;
//    using SEToolbox.Models;
//    using System.Collections.Generic;

//    public abstract class StructureBaseViewModel<TModel> : BaseViewModel
//    {
//        #region fields

//        private TModel dataModel;

//        #endregion

//        #region ctor

//        public StructureBaseViewModel(BaseViewModel parentViewModel, TModel dataModel)
//            : base(parentViewModel)
//        {
//            this.dataModel = dataModel;
//        }

//        #endregion

//        #region Properties

//        protected TModel DataModel
//        {
//            get
//            {
//                return this.dataModel;
//            }
//        }

//        //public long EntityId
//        //{
//        //    get
//        //    {
//        //        return this.dataModel.EntityId;
//        //    }

//        //    set
//        //    {
//        //        if (value != this.dataModel.EntityId)
//        //        {
//        //            this.dataModel.EntityId = value;
//        //            this.RaisePropertyChanged(() => EntityId);
//        //        }
//        //    }
//        //}

//        //public MyPositionAndOrientation? PositionAndOrientation
//        //{
//        //    get
//        //    {
//        //        return this.dataModel.PositionAndOrientation;
//        //    }

//        //    set
//        //    {
//        //        if (!EqualityComparer<MyPositionAndOrientation?>.Default.Equals(value, this.dataModel.PositionAndOrientation))
//        //        //if (value != this.entityBase.PositionAndOrientation)
//        //        {
//        //            this.dataModel.PositionAndOrientation = value;
//        //            this.RaisePropertyChanged(() => PositionAndOrientation);
//        //        }
//        //    }
//        //}

//        //public ClassType ClassType
//        //{
//        //    get
//        //    {
//        //        return this.dataModel.ClassType;
//        //    }

//        //    set
//        //    {
//        //        if (value != this.dataModel.ClassType)
//        //        {
//        //            this.dataModel.ClassType = value;
//        //            this.RaisePropertyChanged(() => ClassType);
//        //        }
//        //    }
//        //}

//        #endregion

//        #region methods

//        //public virtual void UpdateFromEntityBase()
//        //{
//        //    this.ClassType = ClassType.Unknown;
//        //}

//        //public static IStructureBase Create(MyObjectBuilder_EntityBase entityBase)
//        //{
//        //    if (entityBase is MyObjectBuilder_VoxelMap)
//        //    {
//        //        return new StructureVoxelModel(entityBase);
//        //    }
//        //    else if (entityBase is MyObjectBuilder_Character)
//        //    {
//        //        return new StructureCharacterModel(entityBase);
//        //    }
//        //    else if (entityBase is MyObjectBuilder_CubeGrid)
//        //    {
//        //        return new StructureCubeGridModel(entityBase);
//        //    }
//        //    else
//        //    {
//        //        throw new NotImplementedException(string.Format("A new object has not been catered for in the StructureBase, of type '{0}'.", entityBase.GetType()));
//        //    }
//        //}

//        #endregion
//    }
//}
