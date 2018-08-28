namespace SEToolbox.Models
{
    using System;

    [Serializable]
    public class VoxelMaterialAssetModel : BaseModel
    {
        private string _materialName;

        private string _displayName;

        private double _volume;

        private double _percent;

        private string _textureFile;

        #region Properties

        /// <summary>
        /// 'Name' which represents the name used in the Voxel Material, and .vox file.
        /// </summary>
        public string MaterialName
        {
            get { return _materialName; }

            set
            {
                if (value != _materialName)
                {
                    _materialName = value;
                    OnPropertyChanged(nameof(MaterialName));
                }
            }
        }

        public string DisplayName
        {
            get { return _displayName; }

            set
            {
                if (value != _displayName)
                {
                    _displayName = value;
                    OnPropertyChanged(nameof(DisplayName));
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

        public double Percent
        {
            get { return _percent; }

            set
            {
                if (value != _percent)
                {
                    _percent = value;
                    OnPropertyChanged(nameof(Percent));
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
