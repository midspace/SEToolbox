namespace SEToolbox.Models
{
    using SEToolbox.Interfaces;
    using System.Collections.ObjectModel;
    using VRageMath;

    public class GroupMoveModel : BaseModel
    {
        #region Fields

        private ObservableCollection<GroupMoveItemModel> _selections;
        private Vector3 _playerPosition;

        private double _globalOffsetPositionX;
        private double _globalOffsetPositionY;
        private double _globalOffsetPositionZ;
        private bool _isGlobalOffsetPosition;

        private double _singlePositionX;
        private double _singlePositionY;
        private double _singlePositionZ;
        private bool _isSinglePosition;

        private bool _isBusy;

        #endregion

        #region ctor

        public GroupMoveModel()
        {
            this.GlobalOffsetPositionX = 0f;
            this.GlobalOffsetPositionY = 0f;
            this.GlobalOffsetPositionZ = 0f;
        }

        #endregion

        #region Properties

        public ObservableCollection<GroupMoveItemModel> Selections
        {
            get
            {
                return this._selections;
            }

            set
            {
                if (value != this._selections)
                {
                    this._selections = value;
                    this.RaisePropertyChanged(() => Selections);
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
                return this._isBusy;
            }

            set
            {
                if (value != this._isBusy)
                {
                    this._isBusy = value;
                    this.RaisePropertyChanged(() => IsBusy);
                    if (this._isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        public double GlobalOffsetPositionX
        {
            get
            {
                return this._globalOffsetPositionX;
            }

            set
            {
                if (value != this._globalOffsetPositionX)
                {
                    this._globalOffsetPositionX = value;
                    this.RaisePropertyChanged(() => GlobalOffsetPositionX);
                }
            }
        }

        public double GlobalOffsetPositionY
        {
            get
            {
                return this._globalOffsetPositionY;
            }

            set
            {
                if (value != this._globalOffsetPositionY)
                {
                    this._globalOffsetPositionY = value;
                    this.RaisePropertyChanged(() => GlobalOffsetPositionY);
                }
            }
        }

        public double GlobalOffsetPositionZ
        {
            get
            {
                return this._globalOffsetPositionZ;
            }

            set
            {
                if (value != this._globalOffsetPositionZ)
                {
                    this._globalOffsetPositionZ = value;
                    this.RaisePropertyChanged(() => GlobalOffsetPositionZ);
                }
            }
        }

        public bool IsGlobalOffsetPosition
        {
            get
            {
                return this._isGlobalOffsetPosition;
            }

            set
            {
                if (value != this._isGlobalOffsetPosition)
                {
                    this._isGlobalOffsetPosition = value;
                    this.RaisePropertyChanged(() => IsGlobalOffsetPosition);
                }
            }
        }

        public double SinglePositionX
        {
            get
            {
                return this._singlePositionX;
            }

            set
            {
                if (value != this._singlePositionX)
                {
                    this._singlePositionX = value;
                    this.RaisePropertyChanged(() => SinglePositionX);
                }
            }
        }

        public double SinglePositionY
        {
            get
            {
                return this._singlePositionY;
            }

            set
            {
                if (value != this._singlePositionY)
                {
                    this._singlePositionY = value;
                    this.RaisePropertyChanged(() => SinglePositionY);
                }
            }
        }

        public double SinglePositionZ
        {
            get
            {
                return this._singlePositionZ;
            }

            set
            {
                if (value != this._singlePositionZ)
                {
                    this._singlePositionZ = value;
                    this.RaisePropertyChanged(() => SinglePositionZ);
                }
            }
        }

        public bool IsSinglePosition
        {
            get
            {
                return this._isSinglePosition;
            }

            set
            {
                if (value != this._isSinglePosition)
                {
                    this._isSinglePosition = value;
                    this.RaisePropertyChanged(() => IsSinglePosition);
                }
            }
        }

        #endregion

        #region methods

        public void Load(ObservableCollection<IStructureViewBase> selections, Vector3 playerPosition)
        {
            this.Selections = new ObservableCollection<GroupMoveItemModel>();
            this._playerPosition = playerPosition;
            this.IsGlobalOffsetPosition = true;

            foreach (var selection in selections)
            {
                this.Selections.Add(new GroupMoveItemModel()
                {
                    Item = selection,
                    PositionX = selection.DataModel.PositionX,
                    PositionY = selection.DataModel.PositionY,
                    PositionZ = selection.DataModel.PositionZ,
                    PlayerDistance = selection.DataModel.PlayerDistance
                });
            }
        }

        #endregion

        #region helpers

        public void CalcOffsetDistances()
        {
            foreach (var selection in this.Selections)
            {
                if (this.IsGlobalOffsetPosition)
                {
                    // Apply a Global Offset to all objects.
                    selection.PositionX = selection.Item.DataModel.PositionX + this.GlobalOffsetPositionX;
                    selection.PositionY = selection.Item.DataModel.PositionY + this.GlobalOffsetPositionY;
                    selection.PositionZ = selection.Item.DataModel.PositionZ + this.GlobalOffsetPositionZ;
                }

                if (this.IsSinglePosition)
                {
                    // Apply a Single Position to all objects.
                    selection.PositionX = this.SinglePositionX;
                    selection.PositionY = this.SinglePositionY;
                    selection.PositionZ = this.SinglePositionZ;
                }

                selection.PlayerDistance = (this._playerPosition - new Vector3((float)selection.PositionX, (float)selection.PositionY, (float)selection.PositionZ)).Length();
            }
        }

        public void ApplyNewPositions()
        {
            foreach (var selection in this.Selections)
            {
                selection.Item.DataModel.PositionX = selection.PositionX;
                selection.Item.DataModel.PositionY = selection.PositionY;
                selection.Item.DataModel.PositionZ = selection.PositionZ;
            }
        }

        #endregion
    }
}
