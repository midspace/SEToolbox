namespace SEToolbox.Models
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Threading;
    using System.Xml.Serialization;

    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Common.ObjectBuilders.Voxels;
    using SEToolbox.Interfaces;
    using SEToolbox.Interop;
    using VRageMath;

    [Serializable]
    public class StructureBaseModel : BaseModel, IStructureBase
    {
        #region fields

        // Fields are marked as NonSerialized, as they aren't required during the drag-drop operation.

        [NonSerialized]
        private MyObjectBuilder_EntityBase _entityBase;

        [NonSerialized]
        private ClassType _classType;

        [NonSerialized]
        private string _name;

        [NonSerialized]
        private string _description;

        [NonSerialized]
        private Vector3 _center;

        [NonSerialized]
        private BoundingBox _worldAabb;

        [NonSerialized]
        private double _playerDistance;

        [NonSerialized]
        private double _mass;

        [NonSerialized]
        private int _blockCount;

        [NonSerialized]
        private bool _isBusy;

        private string _serializedEntity;

        [NonSerialized]
        internal Dispatcher _dispatcher;

        #endregion

        #region ctor

        public StructureBaseModel()
        {
        }

        public StructureBaseModel(MyObjectBuilder_EntityBase entityBase)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            EntityBase = entityBase;
        }

        #endregion

        #region Properties

        [XmlIgnore]
        public virtual MyObjectBuilder_EntityBase EntityBase
        {
            get { return _entityBase; }
            set
            {
                if (value != _entityBase)
                {
                    _entityBase = value;
                    UpdateGeneralFromEntityBase();
                    RaisePropertyChanged(() => EntityBase);
                }
            }
        }

        [XmlIgnore]
        public long EntityId
        {
            get { return _entityBase.EntityId; }

            set
            {
                if (value != _entityBase.EntityId)
                {
                    _entityBase.EntityId = value;
                    RaisePropertyChanged(() => EntityId);
                }
            }
        }

        [XmlIgnore]
        public MyPositionAndOrientation? PositionAndOrientation
        {
            get { return _entityBase.PositionAndOrientation; }

            set
            {
                if (!EqualityComparer<MyPositionAndOrientation?>.Default.Equals(value, _entityBase.PositionAndOrientation))
                //if (value != entityBase.PositionAndOrientation)
                {
                    _entityBase.PositionAndOrientation = value;
                    RaisePropertyChanged(() => PositionAndOrientation);
                }
            }
        }

        [XmlIgnore]
        public float PositionX
        {
            get { return _entityBase.PositionAndOrientation.Value.Position.X; }

            set
            {
                if (value != _entityBase.PositionAndOrientation.Value.Position.X)
                {
                    var pos = _entityBase.PositionAndOrientation.Value;
                    pos.Position.X = value;
                    _entityBase.PositionAndOrientation = pos;
                    RaisePropertyChanged(() => PositionX);
                }
            }
        }

        [XmlIgnore]
        public float PositionY
        {
            get { return _entityBase.PositionAndOrientation.Value.Position.Y; }

            set
            {
                if (value != _entityBase.PositionAndOrientation.Value.Position.Y)
                {
                    var pos = _entityBase.PositionAndOrientation.Value;
                    pos.Position.Y = value;
                    _entityBase.PositionAndOrientation = pos;
                    RaisePropertyChanged(() => PositionY);
                }
            }
        }

        [XmlIgnore]
        public float PositionZ
        {
            get { return _entityBase.PositionAndOrientation.Value.Position.Z; }

            set
            {
                if (value != _entityBase.PositionAndOrientation.Value.Position.Z)
                {
                    var pos = _entityBase.PositionAndOrientation.Value;
                    pos.Position.Z = value;
                    _entityBase.PositionAndOrientation = pos;
                    RaisePropertyChanged(() => PositionZ);
                }
            }
        }

        [XmlIgnore]
        public ClassType ClassType
        {
            get { return _classType; }

            set
            {
                if (value != _classType)
                {
                    _classType = value;
                    RaisePropertyChanged(() => ClassType);
                }
            }
        }

        [XmlIgnore]
        public virtual string DisplayName
        {
            get { return _name; }

            set
            {
                if (value != _name)
                {
                    _name = value;
                    RaisePropertyChanged(() => DisplayName);
                }
            }
        }

        [XmlIgnore]
        public string Description
        {
            get { return _description; }

            set
            {
                if (value != _description)
                {
                    _description = value;
                    RaisePropertyChanged(() => Description);
                }
            }
        }

        [XmlIgnore]
        public double PlayerDistance
        {
            get { return _playerDistance; }

            set
            {
                if (value != _playerDistance)
                {
                    _playerDistance = value;
                    RaisePropertyChanged(() => PlayerDistance);
                }
            }
        }

        [XmlIgnore]
        public double Mass
        {
            get { return _mass; }

            set
            {
                if (value != _mass)
                {
                    _mass = value;
                    RaisePropertyChanged(() => Mass);
                }
            }
        }

        [XmlIgnore]
        public virtual int BlockCount
        {
            get { return _blockCount; }

            set
            {
                if (value != _blockCount)
                {
                    _blockCount = value;
                    RaisePropertyChanged(() => BlockCount);
                }
            }
        }

        /// <summary>
        /// Center of the object in space.
        /// </summary>
        [XmlIgnore]
        public Vector3 Center
        {
            get { return _center; }

            set
            {
                if (value != _center)
                {
                    _center = value;
                    RaisePropertyChanged(() => Center);
                }
            }
        }

        /// <summary>
        /// Bounding box.
        /// </summary>
        [XmlIgnore]
        public BoundingBox WorldAABB
        {
            get { return _worldAabb; }

            set
            {
                if (value != _worldAabb)
                {
                    _worldAabb = value;
                    RaisePropertyChanged(() => WorldAABB);
                }
            }
        }

        public string SerializedEntity
        {
            get
            {
                return _serializedEntity;
            }

            set
            {
                if (value != _serializedEntity)
                {
                    _serializedEntity = value;
                    RaisePropertyChanged(() => SerializedEntity);
                }
            }
        }

        [XmlIgnore]
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

        #endregion

        #region methods

        public virtual void UpdateGeneralFromEntityBase()
        {
            ClassType = ClassType.Unknown;
        }

        public static IStructureBase Create(MyObjectBuilder_EntityBase entityBase, string savefilePath)
        {
            if (entityBase is MyObjectBuilder_VoxelMap)
            {
                return new StructureVoxelModel(entityBase, savefilePath);
            }

            if (entityBase is MyObjectBuilder_Character)
            {
                return new StructureCharacterModel(entityBase);
            }

            if (entityBase is MyObjectBuilder_CubeGrid)
            {
                return new StructureCubeGridModel(entityBase);
            }

            if (entityBase is MyObjectBuilder_FloatingObject)
            {
                return new StructureFloatingObjectModel(entityBase);
            }

            if (entityBase is MyObjectBuilder_Meteor)
            {
                return new StructureMeteorModel(entityBase);
            }

            return new StructureUnknownModel(entityBase);
            //throw new NotImplementedException(string.Format("A new object has not been catered for in the StructureBase, of type '{0}'.", entityBase.GetType()));
        }

        public virtual void InitializeAsync()
        {
            // to be overridden.
        }

        public virtual void CancelAsync()
        {
            // to be overridden.
        }

        public virtual void RecalcPosition(Vector3 playerPosition)
        {
            PlayerDistance = (playerPosition - PositionAndOrientation.Value.Position.ToVector3()).Length();
        }

        #endregion
    }
}
