namespace SEToolbox.ViewModels
{
    using System.Collections.Generic;

    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using VRage;

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
            get { return _isSelected; }

            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
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
                return DataModel.IsBusy;
            }

            set
            {
                DataModel.IsBusy = value;
            }
        }

        public long EntityId
        {
            get
            {
                return DataModel.EntityId;
            }

            set
            {
                if (value != DataModel.EntityId)
                {
                    DataModel.EntityId = value;
                    OnPropertyChanged(nameof(EntityId));
                }
            }
        }

        public MyPositionAndOrientation? PositionAndOrientation
        {
            get
            {
                return DataModel.PositionAndOrientation;
            }

            set
            {
                if (!EqualityComparer<MyPositionAndOrientation?>.Default.Equals(value, DataModel.PositionAndOrientation))
                //if (value != entityBase.PositionAndOrientation)
                {
                    DataModel.PositionAndOrientation = value;
                    OnPropertyChanged(nameof(PositionAndOrientation));
                }
            }
        }

        public ClassType ClassType
        {
            get
            {
                return DataModel.ClassType;
            }

            set
            {
                if (value != DataModel.ClassType)
                {
                    DataModel.ClassType = value;
                    OnPropertyChanged(nameof(ClassType));
                }
            }
        }

        public string DisplayName
        {
            get
            {
                return DataModel.DisplayName;
            }

            set
            {
                DataModel.DisplayName = value;
                MainViewModel.IsModified = true;
            }
        }

        public string Description
        {
            get
            {
                return DataModel.Description;
            }

            set
            {
                DataModel.Description = value;
            }
        }

        public double PlayerDistance
        {
            get
            {
                return DataModel.PlayerDistance;
            }

            set
            {
                DataModel.PlayerDistance = value;
            }
        }

        public double Mass
        {
            get
            {
                return DataModel.Mass;
            }

            set
            {
                DataModel.Mass = value;
            }
        }

        public int BlockCount
        {
            get
            {
                return DataModel.BlockCount;
            }

            set
            {
                DataModel.BlockCount = value;
            }
        }

        public virtual double LinearVelocity
        {
            get
            {
                return DataModel.LinearVelocity;
            }

            set
            {
                DataModel.LinearVelocity = value;
            }
        }

        public double PositionX
        {
            get
            {
                return DataModel.PositionX;
            }

            set
            {
                DataModel.PositionX = value;
                MainViewModel.IsModified = true;
                MainViewModel.CalcDistances();
            }
        }

        public double PositionY
        {
            get
            {
                return DataModel.PositionY;
            }

            set
            {
                DataModel.PositionY = value;
                MainViewModel.IsModified = true;
                MainViewModel.CalcDistances();
            }
        }

        public double PositionZ
        {
            get
            {
                return DataModel.PositionZ;
            }

            set
            {
                DataModel.PositionZ = value;
                MainViewModel.IsModified = true;
                MainViewModel.CalcDistances();
            }
        }

        #endregion
    }
}
