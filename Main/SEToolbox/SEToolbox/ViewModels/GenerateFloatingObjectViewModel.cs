namespace SEToolbox.ViewModels
{
    using System.Collections.Generic;
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;

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

        #region command properties

        public ICommand CreateCommand
        {
            get
            {
                return new DelegateCommand(new Action(CreateExecuted), new Func<bool>(CreateCanExecute));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(new Action(CancelExecuted), new Func<bool>(CancelCanExecute));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get
            {
                return this._closeResult;
            }

            set
            {
                this._closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
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

        public ObservableCollection<ComonentItemModel> StockItemList
        {
            get
            {
                return this._dataModel.StockItemList;
            }
        }

        public ComonentItemModel StockItem
        {
            get
            {
                return this._dataModel.StockItem;
            }

            set
            {
                this._dataModel.StockItem = value;
            }
        }

        public bool IsValidItemToImport
        {
            get
            {
                return this._dataModel.IsValidItemToImport;
            }

            set
            {
                this._dataModel.IsValidItemToImport = value;
            }
        }

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

        public int? Units
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

        public decimal? DecimalUnits
        {
            get
            {
                return this._dataModel.DecimalUnits;
            }

            set
            {
                this._dataModel.DecimalUnits = value;
            }
        }

        public bool IsDecimal
        {
            get
            {
                return this._dataModel.IsDecimal;
            }

            set
            {
                this._dataModel.IsDecimal = value;
            }
        }

        public bool IsInt
        {
            get
            {
                return this._dataModel.IsInt;
            }

            set
            {
                this._dataModel.IsInt = value;
            }
        }

        public int Multiplier
        {
            get
            {
                return this._dataModel.Multiplier;
            }

            set
            {
                this._dataModel.Multiplier = value;
            }
        }

        public float MaxFloatingObjects
        {
            get
            {
                return this._dataModel.MaxFloatingObjects;
            }

            set
            {
                this._dataModel.MaxFloatingObjects = value;
            }
        }

        #endregion

        #region methods

        #region commands

        public bool CreateCanExecute()
        {
            return this.StockItem != null &&
                ((this.IsInt && this.Units.HasValue && this.Units.Value > 0) ||
                (this.IsDecimal && this.DecimalUnits.HasValue && this.DecimalUnits.Value > 0));
        }

        public void CreateExecuted()
        {
            this.CloseResult = true;
        }

        public bool CancelCanExecute()
        {
            return true;
        }

        public void CancelExecuted()
        {
            this.CloseResult = false;
        }

        #endregion

        #region BuildEntity

        public MyObjectBuilder_EntityBase[] BuildEntities()
        {
            var entity = new MyObjectBuilder_FloatingObject
            {
                EntityId = SpaceEngineersAPI.GenerateEntityId(),
                PersistentFlags = MyPersistentEntityFlags2.Enabled | MyPersistentEntityFlags2.InScene,
                Item = new MyObjectBuilder_InventoryItem { ItemId = 0 },
            };

            if (this.IsDecimal)
                entity.Item.AmountDecimal = this.DecimalUnits.Value;
            if (this.IsInt)
                entity.Item.AmountDecimal = this.Units.Value;

            this.IsValidItemToImport = true;
            entity.Item.PhysicalContent = (MyObjectBuilder_PhysicalObject)MyObjectBuilder_Base.CreateNewObject(this.StockItem.TypeId, this.StockItem.SubtypeId);

            switch (this.StockItem.TypeId)
            {
                case MyObjectBuilderTypeEnum.Component:
                case MyObjectBuilderTypeEnum.Ingot:
                case MyObjectBuilderTypeEnum.Ore:
                case MyObjectBuilderTypeEnum.AmmoMagazine:
                    break;

                case MyObjectBuilderTypeEnum.PhysicalGunObject:
                    // TODO: May have to create the GunEntity, to define the ownership of the GunObject.
                    // At this stage, it doesn't appear to be required.

                    //((MyObjectBuilder_PhysicalGunObject)entity.Item.PhysicalContent).GunEntity =
                    //    new MyObjectBuilder_AngleGrinder() { EntityId = 0, PersistentFlags = MyPersistentEntityFlags2.None };
                    //MyObjectBuilder_AngleGrinder MyObjectBuilderTypeEnum.AngleGrinder <SubtypeName>AngleGrinderItem</SubtypeName>
                    //MyObjectBuilder_AutomaticRifle MyObjectBuilderTypeEnum.AutomaticRifle
                    //MyObjectBuilder_Welder MyObjectBuilderTypeEnum.Welder
                    //MyObjectBuilder_HandDrill MyObjectBuilderTypeEnum.HandDrill
                    break;

                default:
                    // As yet uncatered for items which may be new.
                    this.IsValidItemToImport = false;
                    break;
            }

            // Figure out where the Character is facing, and plant the new construct 1m out in front, and 1m up from the feet, facing the Character.
            var vectorFwd = this._dataModel.CharacterPosition.Forward.ToVector3D();
            var vectorUp = this._dataModel.CharacterPosition.Up.ToVector3D();
            vectorFwd.Normalize();
            vectorUp.Normalize();
            var vector = Vector3D.Multiply(vectorFwd, 1.0f) + Vector3D.Multiply(vectorUp, 1.0f);

            entity.PositionAndOrientation = new MyPositionAndOrientation()
            {
                Position = Point3D.Add(this._dataModel.CharacterPosition.Position.ToPoint3D(), vector).ToVector3(),
                Forward = this._dataModel.CharacterPosition.Forward,
                Up = this._dataModel.CharacterPosition.Up
            };

            var entities = new List<MyObjectBuilder_EntityBase>();

            for (var i = 0; i < this.Multiplier; i++)
            {
                var newEntity = entity.Clone() as MyObjectBuilder_FloatingObject;
                newEntity.EntityId = SpaceEngineersAPI.GenerateEntityId();
                entities.Add(newEntity);
            }

            return entities.ToArray();
        }

        #endregion

        #endregion
    }
}
