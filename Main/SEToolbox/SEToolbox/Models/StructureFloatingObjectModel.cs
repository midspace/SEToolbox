namespace SEToolbox.Models
{
    using Sandbox.CommonLib.ObjectBuilders;
    using SEToolbox.Interop;
    using System;
    using System.Xml.Serialization;

    [Serializable]
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

        [XmlIgnore]
        public MyObjectBuilder_FloatingObject FloatingObject
        {
            get
            {
                return this.EntityBase as MyObjectBuilder_FloatingObject;
            }
        }

        [XmlIgnore]
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

            if (this.FloatingObject.Item.Content is MyObjectBuilder_Ore)
            {
                this.Description = string.Format("{0} x {1:#,###.000} L", this.FloatingObject.Item.Content.SubtypeName, this.FloatingObject.Item.Amount * 1000);
            }
            else if (this.FloatingObject.Item.Content is MyObjectBuilder_EntityBase)
            {
                var name = this.FloatingObject.Item.Content.GetType().Name;
                name = name.Split('_')[1];
                this.Description = string.Format("{0} x {1}", name, this.FloatingObject.Item.Amount);
            }
            else
            {
                this.Description = string.Format("{0} x {1}", this.FloatingObject.Item.Content.SubtypeName, this.FloatingObject.Item.Amount);
            }
        }

        #endregion
    }
}
