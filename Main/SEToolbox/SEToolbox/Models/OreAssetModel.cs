namespace SEToolbox.Models
{
    using SEToolbox.Interop;
    using System;

    [Serializable]
    public class OreAssetModel : BaseModel
    {
        private string _name;

        private decimal _amount;

        private double _mass;

        private double _volume;

        private TimeSpan _time;

        private string _textureFile;

        #region Properties

        public string Name
        {
            get
            {
                return this._name;
            }

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

        public string FriendlyName { get; set; }

        public decimal Amount
        {
            get
            {
                return this._amount;
            }

            set
            {
                if (value != this._amount)
                {
                    this._amount = value;
                    this.RaisePropertyChanged(() => Amount);
                }
            }
        }

        public double Mass
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

        public double Volume
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

        public TimeSpan Time
        {
            get
            {
                return this._time;
            }

            set
            {
                if (value != this._time)
                {
                    this._time = value;
                    this.RaisePropertyChanged(() => Time);
                }
            }
        }

        public string TextureFile
        {
            get
            {
                return this._textureFile;
            }

            set
            {
                if (value != this._textureFile)
                {
                    this._textureFile = value;
                    this.RaisePropertyChanged(() => TextureFile);
                }
            }
        }

        #endregion
    }
}
