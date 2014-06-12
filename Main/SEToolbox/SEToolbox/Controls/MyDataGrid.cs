namespace SEToolbox.Controls
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using SEToolbox.Support;

    /// <summary>
    /// Provides one click editing on data cells.
    /// </summary>
    public class MyDataGrid : DataGrid
    {
        public MyDataGrid()
        {
            this.PreviewMouseLeftButtonDown += MyDataGrid_PreviewMouseLeftButtonDown;
        }

        void MyDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cell = ((DataGrid)sender).GetHitControl<DataGridCell>((MouseEventArgs)e);

            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                var dataGrid = cell.FindVisualParent<DataGrid>();
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        var row = cell.FindVisualParent<DataGridRow>();
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }
    }
}
