namespace SEToolbox.Models
{
    using System;
    using System.Windows.Media.Media3D;
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using System.Linq;

    public class StructureFloatingObjectModel : StructureBaseModel
    {
        #region Fields

        #endregion

        #region ctor

        public StructureFloatingObjectModel(MyObjectBuilder_EntityBase entityBase)
            : base(entityBase)
        {
        }

        #endregion

        #region Properties

        public MyObjectBuilder_FloatingObject FloatingObject
        {
            get
            {
                return this.EntityBase as MyObjectBuilder_FloatingObject;
            }
        }

        public MyObjectBuilder_InventoryItem Item
        {
            get
            {
                return this.FloatingObject.Item;
            }

            set
            {
                if (value != this.FloatingObject.Item)
                {
                    this.FloatingObject.Item = value;
                    this.RaisePropertyChanged(() => Item);
                }
            }
        }

        #endregion

        #region methods

        public override void UpdateFromEntityBase()
        {
            this.ClassType = ClassType.FloatingObject;
            this.Description = string.Format("{1}x {0}", this.FloatingObject.Item.Content.SubtypeName, this.FloatingObject.Item.Amount);
        }

        #endregion
    }
}
