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

        #region Properties

        public IStructureViewBase Item
        {
            get
            {
                return _item;
            }

            set
            {
                if (value != _item)
                {
                    _item = value;
                    RaisePropertyChanged(() => Item);
                }
            }
        }

        public float PositionX
        {
            get
            {
                return _newPositionX;
            }

            set
            {
                if (value != _newPositionX)
                {
                    _newPositionX = value;
                    RaisePropertyChanged(() => PositionX);
                }
            }
        }

        public float PositionY
        {
            get
            {
                return _newtPositionY;
            }

            set
            {
                if (value != _newtPositionY)
                {
                    _newtPositionY = value;
                    RaisePropertyChanged(() => PositionY);
                }
            }
        }

        public float PositionZ
        {
            get
            {
                return _newPositionZ;
            }

            set
            {
                if (value != _newPositionZ)
                {
                    _newPositionZ = value;
                    RaisePropertyChanged(() => PositionZ);
                }
            }
        }

        public double PlayerDistance
        {
            get
            {
                return _playerDistance;
            }

            set
            {
                if (value != _playerDistance)
                {
                    _playerDistance = value;
                    RaisePropertyChanged(() => PlayerDistance);
                }
            }
        }

        #endregion
    }
}
