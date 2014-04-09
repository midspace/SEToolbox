namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    public class GenerateFloatingObjectModel : BaseModel
    {
        #region Fields

        private double? _volume;
        private double? _mass;
        private double? _units;

        #endregion

        #region ctor

        public GenerateFloatingObjectModel()
        {
        }

        #endregion

        #region Properties

        //public MyObjectBuilder_InventoryItem Item
        //{
        //    get
        //    {
        //        return this.FloatingObject.Item;
        //    }

        //    set
        //    {
        //        if (value != this.FloatingObject.Item)
        //        {
        //            this.FloatingObject.Item = value;
        //            this.RaisePropertyChanged(() => Item);
        //        }
        //    }
        //}

        public double? Volume
        {
            get
            {
                return this._volume;
            }

            set
            {
                if (value != this._volume)
                {
                    this._volume = value;
                    this.RaisePropertyChanged(() => Volume);
                }
            }
        }

        public double? Mass
        {
            get
            {
                return this._mass;
            }

            set
            {
                if (value != this._mass)
                {
                    this._mass = value;
                    this.RaisePropertyChanged(() => Mass);
                }
            }
        }

        public double? Units
        {
            get
            {
                return this._units;
            }

            set
            {
                if (value != this._units)
                {
                    this._units = value;
                    this.RaisePropertyChanged(() => Units);
                }
            }
        }

        #endregion

        #region methods

        public void Load(MyPositionAndOrientation characterPosition)
        {
            // TODO:
        }

        public void Unload()
        {
            // TODO:
        }

        #endregion
    }
}
