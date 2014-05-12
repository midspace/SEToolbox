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
                return this._buildPercent;
            }

            set
            {
                if (value != this._buildPercent)
                {
                    this._buildPercent = value;
                    this.RaisePropertyChanged(() => BuildPercent);
                }
            }
        }

        #endregion
    }
}
