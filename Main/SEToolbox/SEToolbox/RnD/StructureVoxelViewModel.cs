//namespace SEToolbox.ViewModels
//{
//    using Sandbox.CommonLib.ObjectBuilders;
//    using SEToolbox.Interop;
//    using SEToolbox.Models;
//    using System.Collections.Generic;

//    public class StructureVoxelViewModel : StructureBaseViewModel<StructureVoxelModel>
//    {
//        #region ctor

//        public StructureVoxelViewModel(BaseViewModel parentViewModel, StructureVoxelModel dataModel)
//            : base(parentViewModel, dataModel)
//        {
//        }

//        #endregion

//        #region Properties

//        protected new StructureVoxelModel DataModel
//        {
//            get
//            {
//                return base.DataModel as StructureVoxelModel;
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

//        public string Filename
//        {
//            get
//            {
//                return this.DataModel.Filename;
//            }

//            set
//            {
//                if (value != this.DataModel.Filename)
//                {
//                    this.DataModel.Filename = value;
//                    this.RaisePropertyChanged(() => Filename);
//                }
//            }
//        }

//        #endregion

//        #region methods

//        #endregion
//    }
//}
