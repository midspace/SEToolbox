namespace SEToolbox.Models
{
    using System.Collections.ObjectModel;

    using SEToolbox.Interfaces;
    using VRageMath;

    public class GroupMoveModel : BaseModel
    {
        #region Fields

        private ObservableCollection<GroupMoveItemModel> _selections;
        private Vector3 _playerPosition;

        private float _globalOffsetPositionX;
        private float _globalOffsetPositionY;
        private float _globalOffsetPositionZ;
        private bool _isGlobalOffsetPosition;

        private float _singlePositionX;
        private float _singlePositionY;
        private float _singlePositionZ;
        private bool _isSinglePosition;

        private bool _isBusy;

        #endregion

        #region ctor

        public GroupMoveModel()
        {
            GlobalOffsetPositionX = 0f;
            GlobalOffsetPositionY = 0f;
            GlobalOffsetPositionZ = 0f;
        }

        #endregion

        #region Properties

        public ObservableCollection<GroupMoveItemModel> Selections
        {
            get
            {
                return _selections;
            }

            set
            {
                if (value != _selections)
                {
                    _selections = value;
                    OnPropertyChanged(nameof(Selections));
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
                return _isBusy;
            }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                    if (_isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        public float GlobalOffsetPositionX
        {
            get
            {
                return _globalOffsetPositionX;
            }

            set
            {
                if (value != _globalOffsetPositionX)
                {
                    _globalOffsetPositionX = value;
                    OnPropertyChanged(nameof(GlobalOffsetPositionX));
                }
            }
        }

        public float GlobalOffsetPositionY
        {
            get
            {
                return _globalOffsetPositionY;
            }

            set
            {
                if (value != _globalOffsetPositionY)
                {
                    _globalOffsetPositionY = value;
                    OnPropertyChanged(nameof(GlobalOffsetPositionY));
                }
            }
        }

        public float GlobalOffsetPositionZ
        {
            get
            {
                return _globalOffsetPositionZ;
            }

            set
            {
                if (value != _globalOffsetPositionZ)
                {
                    _globalOffsetPositionZ = value;
                    OnPropertyChanged(nameof(GlobalOffsetPositionZ));
                }
            }
        }

        public bool IsGlobalOffsetPosition
        {
            get
            {
                return _isGlobalOffsetPosition;
            }

            set
            {
                if (value != _isGlobalOffsetPosition)
                {
                    _isGlobalOffsetPosition = value;
                    OnPropertyChanged(nameof(IsGlobalOffsetPosition));
                }
            }
        }

        public float SinglePositionX
        {
            get
            {
                return _singlePositionX;
            }

            set
            {
                if (value != _singlePositionX)
                {
                    _singlePositionX = value;
                    OnPropertyChanged(nameof(SinglePositionX));
                }
            }
        }

        public float SinglePositionY
        {
            get
            {
                return _singlePositionY;
            }

            set
            {
                if (value != _singlePositionY)
                {
                    _singlePositionY = value;
                    OnPropertyChanged(nameof(SinglePositionY));
                }
            }
        }

        public float SinglePositionZ
        {
            get
            {
                return _singlePositionZ;
            }

            set
            {
                if (value != _singlePositionZ)
                {
                    _singlePositionZ = value;
                    OnPropertyChanged(nameof(SinglePositionZ));
                }
            }
        }

        public bool IsSinglePosition
        {
            get
            {
                return _isSinglePosition;
            }

            set
            {
                if (value != _isSinglePosition)
                {
                    _isSinglePosition = value;
                    OnPropertyChanged(nameof(IsSinglePosition));
                }
            }
        }

        #endregion

        #region methods

        public void Load(ObservableCollection<IStructureViewBase> selections, Vector3D playerPosition)
        {
            Selections = new ObservableCollection<GroupMoveItemModel>();
            _playerPosition = playerPosition;
            IsGlobalOffsetPosition = true;

            foreach (var selection in selections)
            {
                Selections.Add(new GroupMoveItemModel
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
            foreach (var selection in Selections)
            {
                if (IsGlobalOffsetPosition)
                {
                    // Apply a Global Offset to all objects.
                    selection.PositionX = selection.Item.DataModel.PositionX + GlobalOffsetPositionX;
                    selection.PositionY = selection.Item.DataModel.PositionY + GlobalOffsetPositionY;
                    selection.PositionZ = selection.Item.DataModel.PositionZ + GlobalOffsetPositionZ;
                }

                if (IsSinglePosition)
                {
                    // Apply a Single Position to all objects.
                    selection.PositionX = SinglePositionX;
                    selection.PositionY = SinglePositionY;
                    selection.PositionZ = SinglePositionZ;
                }

                selection.PlayerDistance = (_playerPosition - new Vector3D(selection.PositionX, selection.PositionY, selection.PositionZ)).Length();
            }
        }

        public void ApplyNewPositions()
        {
            foreach (var selection in Selections)
            {
                selection.Item.DataModel.PositionX = selection.PositionX;
                selection.Item.DataModel.PositionY = selection.PositionY;
                selection.Item.DataModel.PositionZ = selection.PositionZ;
            }
        }

        #endregion
    }
}
