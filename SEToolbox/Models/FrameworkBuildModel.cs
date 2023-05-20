namespace SEToolbox.Models
{
    public class FrameworkBuildModel : BaseModel
    {
        public const int UniqueUnits = 1;

        #region Fields

        private double? _buildPercent;

        #endregion

        #region Properties

        public double? BuildPercent
        {
            get
            {
                return _buildPercent;
            }

            set
            {
                if (value != _buildPercent)
                {
                    _buildPercent = value;
                    OnPropertyChanged(nameof(BuildPercent));
                }
            }
        }

        #endregion
    }
}
