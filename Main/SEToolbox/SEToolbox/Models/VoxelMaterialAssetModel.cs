namespace SEToolbox.Models
{
    using System;

    [Serializable]
    public class VoxelMaterialAssetModel : BaseModel
    {
        private string _oreName;

        private double _volume;

        private double _percent;

        #region Properties

        public string OreName
        {
            get
            {
                return this._oreName;
            }

            set
            {
                if (value != this._oreName)
                {
                    this._oreName = value;
                    this.RaisePropertyChanged(() => OreName);
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

        public double Percent
        {
            get
            {
                return this._percent;
            }

            set
            {
                if (value != this._percent)
                {
                    this._percent = value;
                    this.RaisePropertyChanged(() => Percent);
                }
            }
        }

        #endregion
    }
}
