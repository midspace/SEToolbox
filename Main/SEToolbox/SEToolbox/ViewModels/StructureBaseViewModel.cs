﻿namespace SEToolbox.ViewModels
{
    using System.Collections.Generic;

    using Sandbox.Common.ObjectBuilders;
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
            get { return _isSelected; }

            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    RaisePropertyChanged(() => IsSelected);
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
                    RaisePropertyChanged(() => EntityId);
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
                    RaisePropertyChanged(() => PositionAndOrientation);
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
                    RaisePropertyChanged(() => ClassType);
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

        public float PositionX
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

        public float PositionY
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

        public float PositionZ
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
