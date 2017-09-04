namespace SEToolbox.Models
{
    using SEToolbox.Interfaces;
    using SEToolbox.Support;

    public class MergeVoxelModel : BaseModel
    {
        #region Fields

        private IStructureBase _selectionLeft;
        private IStructureBase _selectionRight;
        private string _sourceFile;
        private bool _isValidMerge;
        private VoxelMergeType _voxelMergeType;
        private bool _isBusy;
        private string _mergeFileName;
        private bool _removeOriginalAsteroids;

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

        public IStructureBase SelectionLeft
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

        public IStructureBase SelectionRight
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

        public string MergeFileName
        {
            get { return _mergeFileName; }

            set
            {
                if (value != _mergeFileName)
                {
                    _mergeFileName = value;
                    RaisePropertyChanged(() => MergeFileName);
                }
            }
        }

        public bool RemoveOriginalAsteroids
        {
            get { return _removeOriginalAsteroids; }

            set
            {
                if (value != _removeOriginalAsteroids)
                {
                    _removeOriginalAsteroids = value;
                    RaisePropertyChanged(() => RemoveOriginalAsteroids);
                }
            }
        }

        #endregion

        #region methods

        public void Load(IStructureBase selection1, IStructureBase selection2)
        {
            SelectionLeft = selection1;
            SelectionRight = selection2;

            var modelLeft = (StructureVoxelModel)SelectionLeft;
            var modelRight = (StructureVoxelModel)SelectionRight;

            IsValidMerge = modelLeft.WorldAABB.Intersects(modelRight.WorldAABB);
        }

        #endregion
    }
}