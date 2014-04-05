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

        #endregion

        #region methods

        public void Load(ObservableCollection<IStructureViewBase> selections, Vector3 playerPosition)
        {
            this.Selections = new ObservableCollection<GroupMoveItemModel>();
            this._playerPosition = playerPosition;

            foreach(var selection in selections)
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
                selection.PositionX = selection.Item.DataModel.PositionX + this.GlobalOffsetPositionX;
                selection.PositionY = selection.Item.DataModel.PositionY + this.GlobalOffsetPositionY;
                selection.PositionZ = selection.Item.DataModel.PositionZ + this.GlobalOffsetPositionZ;
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
