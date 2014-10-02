namespace SEToolbox.Models
{
    using System.Collections.ObjectModel;

    using SEToolbox.Interfaces;
    using SEToolbox.Support;

    public class VoxelMergeModel : BaseModel
    {
        #region Fields

        private IStructureViewBase _selectionLeft;
        private IStructureViewBase _selectionRight;
        private string _sourceFile;
        private bool _isValidMerge;
        private VoxelMergeType _voxelMergeType;
        private bool _isBusy;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }

            set
            {
                if (value != _isBusy)
                {
                    _isBusy = value;
                    RaisePropertyChanged(() => IsBusy);
                    if (_isBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
        }

        public IStructureViewBase SelectionLeft
        {
            get { return _selectionLeft; }

            set
            {
                if (value != _selectionLeft)
                {
                    _selectionLeft = value;
                    RaisePropertyChanged(() => SelectionLeft);
                }
            }
        }

        public IStructureViewBase SelectionRight
        {
            get { return _selectionRight; }

            set
            {
                if (value != _selectionRight)
                {
                    _selectionRight = value;
                    RaisePropertyChanged(() => SelectionRight);
                }
            }
        }

        /// <summary>
        /// Indicates if the Entity created at the end of processing is valid.
        /// </summary>
        public bool IsValidMerge
        {
            get { return _isValidMerge; }

            set
            {
                if (value != _isValidMerge)
                {
                    _isValidMerge = value;
                    RaisePropertyChanged(() => IsValidMerge);
                }
            }
        }

        public string SourceFile
        {
            get { return _sourceFile; }

            set
            {
                if (value != _sourceFile)
                {
                    _sourceFile = value;
                    RaisePropertyChanged(() => SourceFile);
                }
            }
        }

        public VoxelMergeType VoxelMergeType
        {
            get { return _voxelMergeType; }

            set
            {
                if (value != _voxelMergeType)
                {
                    _voxelMergeType = value;
                    RaisePropertyChanged(() => VoxelMergeType);
                }
            }
        }

        #endregion

        #region methods

        public void Load(ObservableCollection<IStructureViewBase> selections)
        {
            SelectionLeft = selections[0];
            SelectionRight = selections[1];

            // TODO: validate that asteroids overlap.

            //var voxelLeft = (StructureVoxelViewModel) SelectionLeft;
            IsValidMerge = true;
        }

        #endregion
    }
}
