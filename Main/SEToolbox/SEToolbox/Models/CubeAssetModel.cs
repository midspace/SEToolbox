namespace SEToolbox.Models
{
    using System;

    using SEToolbox.Interop;

    [Serializable]
    public class CubeAssetModel : BaseModel
    {
        #region fields

        private string _name;

        private double _mass;

        private double _volume;

        private long _count;

        private TimeSpan _time;

        private int _pcu;

        private string _textureFile;

        #endregion

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
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string FriendlyName { get; set; }

        public double Mass
        {
            get { return _mass; }

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
            get { return _volume; }

            set
            {
                if (value != _volume)
                {
                    _volume = value;
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        public long Count
        {
            get { return _count; }

            set
            {
                if (value != _count)
                {
                    _count = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }

        public TimeSpan Time
        {
            get { return _time; }

            set
            {
                if (value != _time)
                {
                    _time = value;
                    OnPropertyChanged(nameof(Time));
                }
            }
        }

        public int PCU
        {
            get { return _pcu; }

            set
            {
                if (value != _pcu)
                {
                    _pcu = value;
                    OnPropertyChanged(nameof(PCU));
                }
            }
        }


        public string TextureFile
        {
            get { return _textureFile; }

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
