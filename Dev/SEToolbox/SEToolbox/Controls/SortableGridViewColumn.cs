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
                return _sortBinding;
            }
            set
            {
                if (_sortBinding != value)
                {
                    _sortBinding = value;
                    OnDisplayMemberBindingChanged();
                }
            }
        }

        private void OnDisplayMemberBindingChanged()
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("DisplayMemberBinding"));
        }

        #endregion
    }
}
