namespace SEToolbox.Models
{
    using SEToolbox.Interfaces;

    public class GroupMoveItemModel : BaseModel
    {
        #region Fields

        private IStructureViewBase _item;

        private float _newPositionX;

        private float _newtPositionY;

        private float _newPositionZ;

        private double _playerDistance;

        #endregion

        #region ctor

        public GroupMoveItemModel()
        {
        }

        #endregion

        #region Properties

        public IStructureViewBase Item
        {
            get
            {
                return this._item;
            }

            set
            {
                if (value != this._item)
                {
                    this._item = value;
                    this.RaisePropertyChanged(() => Item);
                }
            }
        }

        public float PositionX
        {
            get
            {
                return this._newPositionX;
            }

            set
            {
                if (value != this._newPositionX)
                {
                    this._newPositionX = value;
                    this.RaisePropertyChanged(() => PositionX);
                }
            }
        }

        public float PositionY
        {
            get
            {
                return this._newtPositionY;
            }

            set
            {
                if (value != this._newtPositionY)
                {
                    this._newtPositionY = value;
                    this.RaisePropertyChanged(() => PositionY);
                }
            }
        }

        public float PositionZ
        {
            get
            {
                return this._newPositionZ;
            }

            set
            {
                if (value != this._newPositionZ)
                {
                    this._newPositionZ = value;
                    this.RaisePropertyChanged(() => PositionZ);
                }
            }
        }

        public double PlayerDistance
        {
            get
            {
                return this._playerDistance;
            }

            set
            {
                if (value != this._playerDistance)
                {
                    this._playerDistance = value;
                    this.RaisePropertyChanged(() => PlayerDistance);
                }
            }
        }

        #endregion
    }
}
