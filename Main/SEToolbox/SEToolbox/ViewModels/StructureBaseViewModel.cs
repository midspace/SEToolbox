namespace SEToolbox.ViewModels
{
    using System.Collections.Generic;
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using SEToolbox.Models;

    public abstract class StructureBaseViewModel<TModel> : DataBaseViewModel, /*IDragable,*/ IStructureViewBase where TModel : IStructureBase
    {
        #region fields

        private bool isSelected;

        #endregion

        #region ctor

        public StructureBaseViewModel(BaseViewModel parentViewModel, TModel dataModel)
            : base(parentViewModel, dataModel)
        {
        }

        #endregion

        #region Properties

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
    }
}
