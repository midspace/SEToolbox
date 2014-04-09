namespace SEToolbox.ViewModels
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Interop.Asteroids;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;
    using VRageMath;

    public class GenerateFloatingObjectViewModel : BaseViewModel
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly GenerateFloatingObjectModel _dataModel;

        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region ctor

        public GenerateFloatingObjectViewModel(BaseViewModel parentViewModel, GenerateFloatingObjectModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>())
        {
        }

        public GenerateFloatingObjectViewModel(BaseViewModel parentViewModel, GenerateFloatingObjectModel dataModel, IDialogService dialogService)
            : base(parentViewModel)
        {
            Contract.Requires(dialogService != null);

            this._dialogService = dialogService;
            this._dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            this._dataModel.PropertyChanged += (sender, e) => this.OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region Properties


        //public MyObjectBuilder_InventoryItem Item
        //{
        //    get
        //    {
        //        return this._dataModel.Item;
        //    }

        //    set
        //    {
        //        this._dataModel.Item = value;
        //    }
        //}

        //public string SubTypeName
        //{
        //    get
        //    {
        //        return this._dataModel.Item.Content.SubtypeName;
        //    }
        //}

        public double? Volume
        {
            get
            {
                return this._dataModel.Volume;
            }

            set
            {
                this._dataModel.Volume = value;
            }
        }

        public double? Mass
        {
            get
            {
                return this._dataModel.Mass;
            }

            set
            {
                this._dataModel.Mass = value;
            }
        }

        public double? Units
        {
            get
            {
                return this._dataModel.Units;
            }

            set
            {
                this._dataModel.Units = value;
            }
        }

        #endregion
    }
}
