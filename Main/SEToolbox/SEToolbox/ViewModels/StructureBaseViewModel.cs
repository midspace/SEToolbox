namespace SEToolbox.ViewModels
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System.Collections.Generic;

    public abstract class StructureBaseViewModel<TModel> : DataBaseViewModel, /*IDragable,*/ IStructureViewBase where TModel : IStructureBase
    {
        #region fields

        private bool isSelected;
        //private TModel dataModel;

        #endregion

        #region ctor

        public StructureBaseViewModel(BaseViewModel parentViewModel, TModel dataModel)
            : base(parentViewModel, dataModel)
        {
            //this.dataModel = dataModel;
        }

        #endregion

        #region Properties

        //protected TModel DataModel
        //{
        //    get
        //    {
        //        return (TModel)base.dataModel;
        //    }

        //    set
        //    {
        //        //if (value != this.dataModel)
        //        {
        //            this.dataModel = value;
        //            this.RaisePropertyChanged(() => DataModel);
        //        }
        //    }
        //}

        public bool IsSelected
        {
            get { return this.isSelected; }

            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    this.RaisePropertyChanged(() => IsSelected);
                }
            }
        }

        public long EntityId
        {
            get
            {
                return this.DataModel.EntityId;
            }

            set
            {
                if (value != this.DataModel.EntityId)
                {
                    this.DataModel.EntityId = value;
                    this.RaisePropertyChanged(() => EntityId);
                }
            }
        }

        public MyPositionAndOrientation? PositionAndOrientation
        {
            get
            {
                return this.DataModel.PositionAndOrientation;
            }

            set
            {
                if (!EqualityComparer<MyPositionAndOrientation?>.Default.Equals(value, this.DataModel.PositionAndOrientation))
                //if (value != this.entityBase.PositionAndOrientation)
                {
                    this.DataModel.PositionAndOrientation = value;
                    this.RaisePropertyChanged(() => PositionAndOrientation);
                }
            }
        }

        public ClassType ClassType
        {
            get
            {
                return this.DataModel.ClassType;
            }

            set
            {
                if (value != this.DataModel.ClassType)
                {
                    this.DataModel.ClassType = value;
                    this.RaisePropertyChanged(() => ClassType);
                }
            }
        }

        public string Description
        {
            get
            {
                return this.DataModel.Description;
            }

            set
            {
                this.DataModel.Description = value;
            }
        }

        public double PlayerDistance
        {
            get
            {
                return this.DataModel.PlayerDistance;
            }

            set
            {
                this.DataModel.PlayerDistance = value;
            }
        }

        #endregion

        #region methods

        //public virtual void UpdateFromEntityBase()
        //{
        //    this.ClassType = ClassType.Unknown;
        //}

        //public static IStructureBase Create(MyObjectBuilder_EntityBase entityBase)
        //{
        //    if (entityBase is MyObjectBuilder_VoxelMap)
        //    {
        //        return new StructureVoxelModel(entityBase);
        //    }
        //    else if (entityBase is MyObjectBuilder_Character)
        //    {
        //        return new StructureCharacterModel(entityBase);
        //    }
        //    else if (entityBase is MyObjectBuilder_CubeGrid)
        //    {
        //        return new StructureCubeGridModel(entityBase);
        //    }
        //    else
        //    {
        //        throw new NotImplementedException(string.Format("A new object has not been catered for in the StructureBase, of type '{0}'.", entityBase.GetType()));
        //    }
        //}

        #endregion

        //#region IDragable Interface

        ////[XmlIgnore]
        //System.Type IDragable.DataType
        //{
        //    get { return typeof(BaseViewModel); }
        //}

        //void IDragable.Remove(object i)
        //{
            
        //} 

        //#endregion
    }
}
