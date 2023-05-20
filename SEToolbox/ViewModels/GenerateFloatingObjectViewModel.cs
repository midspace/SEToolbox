namespace SEToolbox.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using SEToolbox.Interop;
    using SEToolbox.Models;
    using SEToolbox.Services;
    using VRage;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using IDType = VRage.MyEntityIdentifier.ID_OBJECT_TYPE;

    public class GenerateFloatingObjectViewModel : BaseViewModel
    {
        #region Fields

        private readonly GenerateFloatingObjectModel _dataModel;
        private bool? _closeResult;
        private bool _isBusy;

        #endregion

        #region ctor

        public GenerateFloatingObjectViewModel(BaseViewModel parentViewModel, GenerateFloatingObjectModel dataModel)
            : base(parentViewModel)
        {

            _dataModel = dataModel;
            // Will bubble property change events from the Model to the ViewModel.
            _dataModel.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);
        }

        #endregion

        #region command properties

        public ICommand CreateCommand
        {
            get { return new DelegateCommand(CreateExecuted, CreateCanExecute); }
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
            get
            {
                return _closeResult;
            }

            set
            {
                _closeResult = value;
                OnPropertyChanged(nameof(CloseResult));
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

        public ObservableCollection<ComponentItemModel> StockItemList
        {
            get
            {
                return _dataModel.StockItemList;
            }
        }

        public ComponentItemModel StockItem
        {
            get
            {
                return _dataModel.StockItem;
            }

            set
            {
                _dataModel.StockItem = value;
            }
        }

        public bool IsValidItemToImport
        {
            get
            {
                return _dataModel.IsValidItemToImport;
            }

            set
            {
                _dataModel.IsValidItemToImport = value;
            }
        }

        public double? Volume
        {
            get
            {
                return _dataModel.Volume;
            }

            set
            {
                _dataModel.Volume = value;
            }
        }

        public double? Mass
        {
            get
            {
                return _dataModel.Mass;
            }

            set
            {
                _dataModel.Mass = value;
            }
        }

        public int? Units
        {
            get
            {
                return _dataModel.Units;
            }

            set
            {
                _dataModel.Units = value;
            }
        }

        public decimal? DecimalUnits
        {
            get
            {
                return _dataModel.DecimalUnits;
            }

            set
            {
                _dataModel.DecimalUnits = value;
            }
        }

        public bool IsDecimal
        {
            get
            {
                return _dataModel.IsDecimal;
            }

            set
            {
                _dataModel.IsDecimal = value;
            }
        }

        public bool IsInt
        {
            get
            {
                return _dataModel.IsInt;
            }

            set
            {
                _dataModel.IsInt = value;
            }
        }

        public bool IsUnique
        {
            get
            {
                return _dataModel.IsUnique;
            }

            set
            {
                _dataModel.IsUnique = value;
            }
        }

        public int Multiplier
        {
            get
            {
                return _dataModel.Multiplier;
            }

            set
            {
                _dataModel.Multiplier = value;
            }
        }

        public float MaxFloatingObjects
        {
            get
            {
                return _dataModel.MaxFloatingObjects;
            }

            set
            {
                _dataModel.MaxFloatingObjects = value;
            }
        }

        #endregion

        #region methods

        #region commands

        public bool CreateCanExecute()
        {
            return StockItem != null &&
                (IsUnique ||
                (IsInt && Units.HasValue && Units.Value > 0) ||
                (IsDecimal && DecimalUnits.HasValue && DecimalUnits.Value > 0));
        }

        public void CreateExecuted()
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

        #region BuildEntity

        public MyObjectBuilder_EntityBase[] BuildEntities()
        {
            var entity = new MyObjectBuilder_FloatingObject
            {
                EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ENTITY),
                PersistentFlags = MyPersistentEntityFlags2.Enabled | MyPersistentEntityFlags2.InScene,
                Item = new MyObjectBuilder_InventoryItem { ItemId = 0 },
            };

            if (IsDecimal && DecimalUnits.HasValue)
                entity.Item.Amount = DecimalUnits.Value.ToFixedPoint();
            else if (IsInt && Units.HasValue)
                entity.Item.Amount = Units.Value.ToFixedPoint();
            else if (IsUnique)
                entity.Item.Amount = GenerateFloatingObjectModel.UniqueUnits.ToFixedPoint();
            else
                entity.Item.Amount = 1;

            IsValidItemToImport = true;
            entity.Item.PhysicalContent = SpaceEngineersCore.Resources.CreateNewObject<MyObjectBuilder_PhysicalObject>(StockItem.TypeId, StockItem.SubtypeId);

            var gasContainer = entity.Item.PhysicalContent as MyObjectBuilder_GasContainerObject;
            if (gasContainer != null)
                gasContainer.GasLevel = 1f;

            //switch (StockItem.TypeId)
            //{
            //    case MyObjectBuilderTypeEnum.Component:
            //    case MyObjectBuilderTypeEnum.Ingot:
            //    case MyObjectBuilderTypeEnum.Ore:
            //    case MyObjectBuilderTypeEnum.AmmoMagazine:
            //        break;

            //    case MyObjectBuilderTypeEnum.PhysicalGunObject:
            //        // The GunEntity appears to make each 'GunObject' unique through the definition of an EntityId.
            //        // This means, you can't stack them.
            //        // Ownership does not appear to be required at this stage.

            //  ###  Only required for pre-generating the Entity id for a gun that has been handled.  ####
            //        // This is a hack approach, to find the Enum from a SubtypeName like "AngleGrinderItem".
            //        var enumName = StockItem.SubtypeId.Substring(0, StockItem.SubtypeId.Length - 4);
            //        MyObjectBuilderTypeEnum itemEnum;
            //        if (Enum.TryParse<MyObjectBuilderTypeEnum>(enumName, out itemEnum))
            //        {
            //            var gunEntity = MyObjectBuilder_Base.CreateNewObject(itemEnum) as MyObjectBuilder_EntityBase;
            //            gunEntity.EntityId = SpaceEngineersAPI.GenerateEntityId();
            //            gunEntity.PersistentFlags = MyPersistentEntityFlags2.None;
            //            ((MyObjectBuilder_PhysicalGunObject)entity.Item.PhysicalContent).GunEntity = gunEntity;
            //        }
            //        break;

            //    default:
            //        // As yet uncatered for items which may be new.
            //        IsValidItemToImport = false;
            //        break;
            //}

            // Figure out where the Character is facing, and plant the new construct 1m out in front, and 1m up from the feet, facing the Character.
            var vectorFwd = _dataModel.CharacterPosition.Forward.ToVector3D();
            var vectorUp = _dataModel.CharacterPosition.Up.ToVector3D();
            vectorFwd.Normalize();
            vectorUp.Normalize();
            var vector = Vector3D.Multiply(vectorFwd, 1.0f) + Vector3D.Multiply(vectorUp, 1.0f);

            entity.PositionAndOrientation = new MyPositionAndOrientation
            {
                Position = Point3D.Add(_dataModel.CharacterPosition.Position.ToPoint3D(), vector).ToVector3D(),
                Forward = _dataModel.CharacterPosition.Forward,
                Up = _dataModel.CharacterPosition.Up
            };

            var entities = new List<MyObjectBuilder_EntityBase>();

            for (var i = 0; i < Multiplier; i++)
            {
                var newEntity = (MyObjectBuilder_FloatingObject)entity.Clone();
                newEntity.EntityId = SpaceEngineersApi.GenerateEntityId(IDType.ENTITY);
                //if (StockItem.TypeId == SpaceEngineersConsts.PhysicalGunObject)
                //{
                //    Only required for pre-generating the Entity id for a gun that has been handled.
                //    ((MyObjectBuilder_PhysicalGunObject)entity.Item.PhysicalContent).GunEntity.EntityId = SpaceEngineersAPI.GenerateEntityId();
                //}
                entities.Add(newEntity);
            }

            return entities.ToArray();
        }

        #endregion

        #endregion
    }
}
