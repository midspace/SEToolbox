namespace SEToolbox.Models.Asteroids
{
    using SEToolbox.Interop.Asteroids;

    // TODO: need to rewite how the fill interface is displayed to allow custom fill methods.
    // Otherwise it will have to remain generic.

    public class AsteroidFiller : BaseModel
    {
        #region fields

        private int _index;
        private GenerateVoxelDetailModel _voxelFile;
        private IMyVoxelFiller _fillMethod;

        #endregion

        public AsteroidFiller()
        {

        }

        #region properties

        public int Index
        {
            get { return _index; }

            set
            {
                if (value != _index)
                {
                    _index = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }

        public GenerateVoxelDetailModel VoxelFile
        {
            get { return _voxelFile; }

            set
            {
                if (value != _voxelFile)
                {
                    _voxelFile = value;
                    OnPropertyChanged(nameof(VoxelFile));
                }
            }
        }

        public IMyVoxelFiller FillMethod
        {
            get { return _fillMethod; }

            set
            {
                if (value != _fillMethod)
                {
                    _fillMethod = value;
                    OnPropertyChanged(nameof(FillMethod));
                }
            }
        }

        #endregion

        public void CreateFillProperties()
        {
            // VoxelCollection.Insert(VoxelCollection.IndexOf(SelectedRow) + 1, (AsteroidByteFillProperties)SelectedRow.Clone());
        }

        public void RandomizeFillProperties()
        {
            //var filler = new AsteroidByteFiller();
            //var randomModel = (AsteroidByteFillProperties)filler.CreateRandom(VoxelCollection.Count + 1, _dataModel.BaseMaterial, MaterialsCollection, VoxelFileList);
        }

        public void FillAsteroid(MyVoxelMap asteroid, IMyVoxelFillProperties fillProperties)
        {
            _fillMethod.FillAsteroid(asteroid, fillProperties);
        }
    }
}
