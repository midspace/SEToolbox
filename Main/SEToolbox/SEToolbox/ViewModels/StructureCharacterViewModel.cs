namespace SEToolbox.ViewModels
{
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;

    public class StructureCharacterViewModel : StructureBaseViewModel<StructureCharacterModel>
    {
        private InventoryEditorViewModel _inventory;
        private readonly IDialogService _dialogService;
        private readonly Func<IColorDialog> _colorDialogFactory;

        #region ctor

        public StructureCharacterViewModel(BaseViewModel parentViewModel, StructureCharacterModel dataModel)
            : this(parentViewModel, dataModel, ServiceLocator.Resolve<IDialogService>(), ServiceLocator.Resolve<IColorDialog>)
        {
        }

        public StructureCharacterViewModel(BaseViewModel parentViewModel, StructureCharacterModel dataModel, IDialogService dialogService, Func<IColorDialog> colorDialogFactory)
            : base(parentViewModel, dataModel)
        {
            Contract.Requires(dialogService != null);
            Contract.Requires(colorDialogFactory != null);

            this._dialogService = dialogService;
            this._colorDialogFactory = colorDialogFactory;

            this.Inventory = new InventoryEditorViewModel(this, dataModel.Inventory);

            this.DataModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                // Will bubble property change events from the Model to the ViewModel.
                this.OnPropertyChanged(e.PropertyName);
            };
        }

        #endregion

        #region command Properties

        public ICommand ResetVelocityCommand
        {
            get
            {
                return new DelegateCommand(new Action(ResetVelocityExecuted), new Func<bool>(ResetVelocityCanExecute));
            }
        }

        public ICommand ReverseVelocityCommand
        {
            get
            {
                return new DelegateCommand(new Action(ReverseVelocityExecuted), new Func<bool>(ReverseVelocityCanExecute));
            }
        }

        public ICommand ChangeColorCommand
        {
            get
            {
                return new DelegateCommand(new Action(ChangeColorExecuted), new Func<bool>(ChangeColorCanExecute));
            }
        }
        #endregion

        #region Properties

        protected new StructureCharacterModel DataModel
        {
            get
            {
                return base.DataModel as StructureCharacterModel;
            }
        }

        public bool IsPilot
        {
            get
            {
                return this.DataModel.IsPilot;
            }

            set
            {
                this.DataModel.IsPilot = value;
            }
        }

        public bool IsPlayer
        {
            get
            {
                return this.DataModel.IsPlayer;
            }

            set
            {
                this.DataModel.IsPlayer = value;
            }
        }

        public System.Windows.Media.Brush Color
        {
            get
            {
                return new System.Windows.Media.SolidColorBrush(this.DataModel.Color.ToSandboxMediaColor());
            }

            set
            {
                this.DataModel.Color = ((System.Windows.Media.SolidColorBrush)value).Color.ToSandboxHsvColor();
                this.MainViewModel.IsModified = true;
            }
        }

        public bool Light
        {
            get
            {
                return this.DataModel.Light;
            }

            set
            {
                this.DataModel.Light = value;
                this.MainViewModel.IsModified = true;
            }
        }

        public bool JetPack
        {
            get
            {
                return this.DataModel.JetPack;
            }

            set
            {
                this.DataModel.JetPack = value;
                this.MainViewModel.IsModified = true;
            }
        }

        public bool Dampeners
        {
            get
            {
                return this.DataModel.Dampeners;
            }

            set
            {
                this.DataModel.Dampeners = value;
                this.MainViewModel.IsModified = true;
            }
        }

        public List<string> CharacterModels
        {
            get
            {
                return this.DataModel.CharacterModels;
            }

            set
            {
                this.DataModel.CharacterModels = value;
            }
        }

        public double LinearVelocity
        {
            get
            {
                return this.DataModel.LinearVelocity;
            }
        }

        public float BatteryCapacity
        {
            get
            {
                return this.DataModel.BatteryCapacity * 100000;
            }

            set
            {
                this.DataModel.BatteryCapacity = value / 100000;
            }
        }

        public float? Health
        {
            get
            {
                return this.DataModel.Health;
            }

            set
            {
                this.DataModel.Health = value;
            }
        }

        public InventoryEditorViewModel Inventory
        {
            get
            {
                return this._inventory;
            }

            set
            {
                if (value != this._inventory)
                {
                    this._inventory = value;
                    this.RaisePropertyChanged(() => Inventory);
                }
            }
        }

        #endregion

        #region methods

        public bool ResetVelocityCanExecute()
        {
            return this.DataModel.LinearVelocity != 0f;
        }

        public void ResetVelocityExecuted()
        {
            this.DataModel.ResetVelocity();
            this.MainViewModel.IsModified = true;
        }

        public bool ReverseVelocityCanExecute()
        {
            return this.DataModel.LinearVelocity != 0f;
        }

        public void ReverseVelocityExecuted()
        {
            this.DataModel.ReverseVelocity();
            this.MainViewModel.IsModified = true;
        }

        public bool ChangeColorCanExecute()
        {
            return true;
        }

        public void ChangeColorExecuted()
        {
            var colorDialog = _colorDialogFactory();
            colorDialog.FullOpen = true;
            colorDialog.BrushColor = this.Color as System.Windows.Media.SolidColorBrush;
            colorDialog.CustomColors = this.MainViewModel.CreativeModeColors;

            var result = _dialogService.ShowColorDialog(this.OwnerViewModel, colorDialog);

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.Color = colorDialog.BrushColor;
                this.MainViewModel.IsModified = true;
            }

            this.MainViewModel.CreativeModeColors = colorDialog.CustomColors;
        }

        #endregion
    }
}
