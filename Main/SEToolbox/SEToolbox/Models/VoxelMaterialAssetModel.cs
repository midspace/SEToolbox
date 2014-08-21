namespace SEToolbox.Models
{
    using System;

    [Serializable]
    public class VoxelMaterialAssetModel : BaseModel
    {
        private string _materialName;

        private double _volume;

        private double _percent;

        #region Properties

        /// <summary>
        /// 'Name' which represents the name used in the Voxel Material, and .vox file.
        /// </summary>
        public string MaterialName
        {
            get
            {
                return _materialName;
            }

            set
            {
                if (value != _materialName)
                {
                    _materialName = value;
                    RaisePropertyChanged(() => MaterialName);
                }
            }
        }

        // 'AssetName', which represent the texture.

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
                    RaisePropertyChanged(() => Volume);
                }
            }
        }

        public double Percent
        {
            get
            {
                return _percent;
            }

            set
            {
                if (value != _percent)
                {
                    _percent = value;
                    RaisePropertyChanged(() => Percent);
                }
            }
        }

        #endregion
    }
}
