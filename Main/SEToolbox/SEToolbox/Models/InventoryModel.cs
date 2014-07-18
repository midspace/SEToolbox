namespace SEToolbox.Models
{
    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;
    using System;
    using System.ComponentModel;

    [Serializable]
    public class InventoryModel : BaseModel, IDataErrorInfo
    {
        #region fields

        private readonly MyObjectBuilder_InventoryItem _item;
        private string _name;
        private decimal _amount;
        private double _mass;
        private double _massMultiplyer;
        private double _volume;
        private double _volumeMultiplyer;

        #endregion

        public InventoryModel(MyObjectBuilder_InventoryItem item)
        {
            _item = item;
        }

        #region Properties

        public string Name
        {
            get { return this._name; }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    this.FriendlyName = SpaceEngineersAPI.GetResourceName(this.Name);
                    this.RaisePropertyChanged(() => Name);
                }
            }
        }

        public MyObjectBuilderType TypeId { get; set; }

        public string SubtypeId { get; set; }

        public Decimal Amount
        {
            get { return this._amount; }
            set
            {
                if (value != this._amount)
                {
                    this._amount = value;
                    this.RaisePropertyChanged(() => Amount);
                    this.UpdateMassVolume();
                }
            }
        }

        public double Mass
        {
            get { return this._mass; }

            private set
            {
                if (value != this._mass)
                {
                    this._mass = value;
                    this.RaisePropertyChanged(() => Mass);
                }
            }
        }

        public double MassMultiplyer
        {
            get { return this._massMultiplyer; }
            set
            {
                if (value != this._massMultiplyer)
                {
                    this._massMultiplyer = value;
                    this.RaisePropertyChanged(() => MassMultiplyer);
                    this.UpdateMassVolume();
                }
            }
        }

        public double Volume
        {
            get { return this._volume; }

            private set
            {
                if (value != this._volume)
                {
                    this._volume = value;
                    this.RaisePropertyChanged(() => Volume);
                }
            }
        }

        public double VolumeMultiplyer
        {
            get { return this._volumeMultiplyer; }
            set
            {
                if (value != this._volumeMultiplyer)
                {
                    this._volumeMultiplyer = value;
                    this.RaisePropertyChanged(() => VolumeMultiplyer);
                    this.UpdateMassVolume();
                }
            }
        }

        public string TextureFile { get; set; }

        public MyCubeSize? CubeSize { get; set; }

        public bool Exists { get; set; }

        public string FriendlyName { get; set; }

        public bool IsUnique { get; set; }

        public bool IsDecimal { get; set; }

        public bool IsInteger { get; set; }

        public override string ToString()
        {
            return this.FriendlyName;
        }

        #endregion

        private void UpdateMassVolume()
        {
            this.Mass = this.MassMultiplyer * (double)this.Amount;
            this.Volume = this.VolumeMultiplyer * (double)this.Amount;
            _item.AmountDecimal = this.Amount;
        }

        #region IDataErrorInfo interfacing

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Amount":
                        if (this.IsUnique && Amount != 1)
                            return "The Amount must be 1 for Unique items";

                        if (this.IsInteger && (Amount % 1 != 0))
                            return "The Amount must not contain decimal places";

                        break;
                }

                return string.Empty;
            }
        }

        //  TODO: need to bubble volume change up to InventoryEditor for updating TotalVolume, and up to this.MainViewModel.IsModified = true;

        #endregion
    }
}
