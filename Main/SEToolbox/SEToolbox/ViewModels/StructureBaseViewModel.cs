namespace SEToolbox.ViewModels
{
    using System.Collections.Generic;
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;

    public abstract class StructureBaseViewModel<TModel> : DataBaseViewModel, IStructureViewBase where TModel : IStructureBase
    {
        #region fields

        private bool _isSelected;

        #endregion

        #region ctor

        protected StructureBaseViewModel(BaseViewModel parentViewModel, TModel dataModel)
            : base(parentViewModel, dataModel)
        {
        }

        #endregion

        #region Properties

        public bool IsSelected
        {
            get { return this._isSelected; }

            set
            {
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    this.RaisePropertyChanged(() => IsSelected);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this.dataModel.IsBusy;
            }

            set
            {
                this.dataModel.IsBusy = value;
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

        public string DisplayName
        {
            get
            {
                return this.DataModel.DisplayName;
            }

            set
            {
                this.DataModel.DisplayName = value;
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
