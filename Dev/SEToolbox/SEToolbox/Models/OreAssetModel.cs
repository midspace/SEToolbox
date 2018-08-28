namespace SEToolbox.Models
{
    using System;

    using SEToolbox.Interop;

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
                return _name;
            }

            set
            {
                if (value != _name)
                {
                    _name = value;
                    FriendlyName = SpaceEngineersApi.GetResourceName(Name);
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string FriendlyName { get; set; }

        public decimal Amount
        {
            get
            {
                return _amount;
            }

            set
            {
                if (value != _amount)
                {
                    _amount = value;
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        public double Mass
        {
            get
            {
                return _mass;
            }

            set
            {
                if (value != _mass)
                {
                    _mass = value;
                    OnPropertyChanged(nameof(Mass));
                }
            }
        }

        public double Volume
        {
            get
            {
                return _volume;
            }

            set
            {
                if (value != _volume)
                {
                    _volume = value;
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        public TimeSpan Time
        {
            get
            {
                return _time;
            }

            set
            {
                if (value != _time)
                {
                    _time = value;
                    OnPropertyChanged(nameof(Time));
                }
            }
        }

        public string TextureFile
        {
            get
            {
                return _textureFile;
            }

            set
            {
                if (value != _textureFile)
                {
                    _textureFile = value;
                    OnPropertyChanged(nameof(TextureFile));
                }
            }
        }

        #endregion
    }
}
