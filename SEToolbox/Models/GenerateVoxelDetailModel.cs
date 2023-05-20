namespace SEToolbox.Models
{
    using VRageMath;

    public class GenerateVoxelDetailModel : BaseModel
    {
        #region Fields

        private string _name;
        private string _sourceFilename;
        private string _voxelFilename;
        private Vector3I _size;

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
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string SourceFilename
        {
            get { return _sourceFilename; }

            set
            {
                if (value != _sourceFilename)
                {
                    _sourceFilename = value;
                    OnPropertyChanged(nameof(SourceFilename));
                }
            }
        }

        public string VoxelFilename
        {
            get { return _voxelFilename; }

            set
            {
                if (value != _voxelFilename)
                {
                    _voxelFilename = value;
                    OnPropertyChanged(nameof(VoxelFilename));
                }
            }
        }

        public Vector3I Size
        {
            get { return _size; }

            set
            {
                if (value != _size)
                {
                    _size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        public int SizeX
        {
            get { return _size.X; }
        }

        public int SizeY
        {
            get { return _size.Y; }
        }

        public int SizeZ
        {
            get { return _size.Z; }
        }

        public long FileSize { get; set; }

        #endregion

        // To allow text searching in ComboBox.
        public override string ToString()
        {
            return _name;
        }
    }
}
