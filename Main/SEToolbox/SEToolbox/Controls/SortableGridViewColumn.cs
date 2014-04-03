namespace SEToolbox.Controls
{
    using System.Windows.Data;

    public class SortableGridViewColumn : System.Windows.Controls.GridViewColumn
    {
        #region fields

        private BindingBase _sortBinding;

        #endregion

        #region SortBinding

        public BindingBase SortBinding
        {
            get
            {
                return this._sortBinding;
            }
            set
            {
                if (this._sortBinding != value)
                {
                    this._sortBinding = value;
                    this.OnDisplayMemberBindingChanged();
                }
            }
        }

        private void OnDisplayMemberBindingChanged()
        {
            this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("DisplayMemberBinding"));
        }

        #endregion
    }
}
