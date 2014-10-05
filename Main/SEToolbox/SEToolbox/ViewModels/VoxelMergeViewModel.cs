namespace SEToolbox.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using VRageMath;

    public class VoxelMergeViewModel : BaseViewModel
    {
        #region Fields

        private readonly VoxelMergeModel _dataModel;
        private bool? _closeResult;

        #endregion

        #region Constructors

        public VoxelMergeViewModel(BaseViewModel parentViewModel, VoxelMergeModel dataModel)
            : base(parentViewModel)
        {
            _dataModel = dataModel;

            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command Properties

        public ICommand ApplyCommand
        {
            get { return new DelegateCommand(ApplyExecuted, ApplyCanExecute); }
        }

        public ICommand CancelCommand
        {
            get { return new DelegateCommand(CancelExecuted, CancelCanExecute); }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get { return _closeResult; }

            set
            {
                _closeResult = value;
                RaisePropertyChanged(() => CloseResult);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the View is currently in the middle of an asynchonise operation.
        /// </summary>
        public bool IsBusy
        {
            get { return _dataModel.IsBusy; }
            set { _dataModel.IsBusy = value; }
        }

        public bool IsValidMerge
        {
            get { return _dataModel.IsValidMerge; }
            set { _dataModel.IsValidMerge = value; }
        }

        public MyObjectBuilder_VoxelMap NewEntity { get; set; }

        public StructureVoxelModel SelectionLeft
        {
            get { return (StructureVoxelModel)_dataModel.SelectionLeft; }
            set { _dataModel.SelectionLeft = value; }
        }

        public StructureVoxelModel SelectionRight
        {
            get { return (StructureVoxelModel)_dataModel.SelectionRight; }
            set { _dataModel.SelectionRight = value; }
        }

        public string SourceFile
        {
            get { return _dataModel.SourceFile; }
            set { _dataModel.SourceFile = value; }
        }

        public VoxelMergeType VoxelMergeType
        {
            get { return _dataModel.VoxelMergeType; }
            set { _dataModel.VoxelMergeType = value; }
        }

        #endregion

        #region methods

        public bool ApplyCanExecute()
        {
            return IsValidMerge;
        }

        public void ApplyExecuted()
        {
            CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            CloseResult = false;
        }

        #endregion

        public MyObjectBuilder_EntityBase BuildEntity()
        {
            // TODO: merge.

            //SelectionLeft.DataModel.
            //NewEntity =

            var modelLeft = (StructureVoxelModel)SelectionLeft;
            var modelRight = (StructureVoxelModel)SelectionRight;

            // TODO: Realign both asteroids grids.
            //Vector3  modelLeft.AABB.Min


            // TODO: calculate smallest allowable size for contents of both.
            //var min = modelLeft.AABB.Min + modelLeft.ContentBounds.Min;
            //modelRight.AABB + modelRight.ContentBounds;
            //Vector3.Min(
            //Vector3.Max(


            return null;
        }
    }
}
