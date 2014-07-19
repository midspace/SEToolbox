namespace SEToolbox.Models
{
    using SEToolbox.Interop;
    using System;

    [Serializable]
    public class CubeAssetModel : BaseModel
    {
        #region fields

        private string _name;

        private double _mass;

        private double _volume;

        private long _count;

        private TimeSpan _time;

        private string _textureFile;

        #endregion

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
                    this.FriendlyName = SpaceEngineersApi.GetResourceName(this.Name);
                    this.RaisePropertyChanged(() => Name);
                }
            }
        }

        public string FriendlyName { get; set; }

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

        public long Count
        {
            get
            {
                return this._count;
            }

            set
            {
                if (value != this._count)
                {
                    this._count = value;
                    this.RaisePropertyChanged(() => Count);
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
