﻿namespace SEToolbox.Models
{
    using System;
    using System.ComponentModel;

    using Sandbox.Common.ObjectBuilders;
    using SEToolbox.Interop;

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
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    FriendlyName = SpaceEngineersApi.GetResourceName(Name);
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        public MyObjectBuilderType TypeId { get; set; }

        public string SubtypeId { get; set; }

        public Decimal Amount
        {
            get { return _amount; }
            set
            {
                if (value != _amount)
                {
                    _amount = value;
                    RaisePropertyChanged(() => Amount);
                    UpdateMassVolume();
                }
            }
        }

        public double Mass
        {
            get { return _mass; }

            private set
            {
                if (value != _mass)
                {
                    _mass = value;
                    RaisePropertyChanged(() => Mass);
                }
            }
        }

        public double MassMultiplyer
        {
            get { return _massMultiplyer; }
            set
            {
                if (value != _massMultiplyer)
                {
                    _massMultiplyer = value;
                    RaisePropertyChanged(() => MassMultiplyer);
                    UpdateMassVolume();
                }
            }
        }

        public double Volume
        {
            get { return _volume; }

            private set
            {
                if (value != _volume)
                {
                    _volume = value;
                    RaisePropertyChanged(() => Volume);
                }
            }
        }

        public double VolumeMultiplyer
        {
            get { return _volumeMultiplyer; }
            set
            {
                if (value != _volumeMultiplyer)
                {
                    _volumeMultiplyer = value;
                    RaisePropertyChanged(() => VolumeMultiplyer);
                    UpdateMassVolume();
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
            return FriendlyName;
        }

        #endregion

        private void UpdateMassVolume()
        {
            Mass = MassMultiplyer * (double)Amount;
            Volume = VolumeMultiplyer * (double)Amount;
            _item.AmountDecimal = Amount;
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
                        if (IsUnique && Amount != 1)
                            return "The Amount must be 1 for Unique items";

                        if (IsInteger && (Amount % 1 != 0))
                            return "The Amount must not contain decimal places";

                        break;
                }

                return string.Empty;
            }
        }

        //  TODO: need to bubble volume change up to InventoryEditor for updating TotalVolume, and up to MainViewModel.IsModified = true;

        #endregion
    }
}
