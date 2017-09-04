namespace SEToolbox.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    using SEToolbox.Support;

    public class SortableListView : ListView
    {
        private const int MaxSortableColumns = 1;

        #region fields

        private readonly List<SortColumn> _sortList = new List<SortColumn>();

        #endregion

        #region ColumnHeaderArrowUpTemplate

        public static readonly DependencyProperty ColumnHeaderArrowUpTemplateProperty = DependencyProperty.Register("ColumnHeaderArrowUpTemplate", typeof(DataTemplate), typeof(SortableListView));
        public DataTemplate ColumnHeaderArrowUpTemplate
        {
            get { return (DataTemplate)GetValue(ColumnHeaderArrowUpTemplateProperty); }
            set { SetValue(ColumnHeaderArrowUpTemplateProperty, value); }
        }

        #endregion

        #region ColumnHeaderArrowDownTemplate

        public static readonly DependencyProperty ColumnHeaderArrowDownTemplateProperty = DependencyProperty.Register("ColumnHeaderArrowDownTemplate", typeof(DataTemplate), typeof(SortableListView));
        public DataTemplate ColumnHeaderArrowDownTemplate
        {
            get { return (DataTemplate)GetValue(ColumnHeaderArrowDownTemplateProperty); }
            set { SetValue(ColumnHeaderArrowDownTemplateProperty, value); }
        }

        #endregion

        public static readonly DependencyProperty DefaultSortColumnProperty = DependencyProperty.Register("DefaultSortColumn", typeof(string), typeof(SortableListView));
        public string DefaultSortColumn
        {
            get { return (string)GetValue(DefaultSortColumnProperty); }
            set { SetValue(DefaultSortColumnProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // add the event handler to the GridViewColumnHeader. This strongly ties this ListView to a GridView.
            AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
            AddHandler(ListView.MouseDoubleClickEvent, new RoutedEventHandler(MouseDoubleClickedHandler));
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (this.ItemsSource != null)
            {
                var dataView = CollectionViewSource.GetDefaultView(this.ItemsSource);

                if (dataView.SortDescriptions.Count == 0 && _sortList != null && _sortList.Count > 0)
                {
                    for (int i = _sortList.Count - 1; i >=0 ; i--)
                        dataView.SortDescriptions.Add(new SortDescription(_sortList[i].SortPath, _sortList[i].SortDirection));
                }

                dataView.Refresh();
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (DefaultSortColumn != null)
            {
                var grdiView = this.View as GridView;

                if (grdiView == null)
                    return;

                GridViewColumn selectedColumn = null;

                foreach (var column in grdiView.Columns)
                {
                    if (column is SortableGridViewColumn && ((SortableGridViewColumn)column).SortBinding is Binding)
                    {
                        var binding = (Binding)((SortableGridViewColumn)column).SortBinding;
                        if (binding.Path.Path == DefaultSortColumn)
                        {
                            selectedColumn = column;
                            break;
                        }
                    }
                    else if (column is SortableGridViewColumn && ((SortableGridViewColumn)column).SortBinding is MultiBinding)
                    {
                        //var multiBinding = (MultiBinding)((SortableGridViewColumn)column).SortBinding;
                        //header.AddRange(multiBinding.Bindings.OfType<Binding>().Select(binding => binding.Path.Path));
                        // we're going to ignore MultBinding as an option for Default column sorting, as it's a bit more complex, unless we concatenate the field names.
                        // There isn't an immediate need for it in SEToolbox.
                    }
                    else if (column.DisplayMemberBinding is Binding)
                    {
                        var binding = (Binding)column.DisplayMemberBinding;
                        if (binding.Path.Path == DefaultSortColumn)
                        {
                            selectedColumn = column;
                            break;
                        }
                    }
                    else
                    {
                        if (column.Header.ToString() == DefaultSortColumn)
                        {
                            selectedColumn = column;
                            break;
                        }
                    }
                }

                if (selectedColumn != null)
                {
                    _sortList.Clear();
                    _sortList.Add(new SortColumn(DefaultSortColumn, ListSortDirection.Ascending, selectedColumn));
                    Sort(this, _sortList);

                    if (ColumnHeaderArrowUpTemplate != null)
                        selectedColumn.HeaderTemplate = ColumnHeaderArrowUpTemplate;
                }
            }
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            // May be triggered by clicking on vertical scrollbar.
            if (headerClicked != null)
            {
                var listView = headerClicked.FindVisualParent<ListView>();

                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    var header = new List<string>();
                    if (headerClicked.Column is SortableGridViewColumn && ((SortableGridViewColumn)headerClicked.Column).SortBinding is Binding)
                    {
                        var binding = (Binding)((SortableGridViewColumn)headerClicked.Column).SortBinding;
                        header.Add(binding.Path.Path);
                    }
                    else if (headerClicked.Column is SortableGridViewColumn && ((SortableGridViewColumn)headerClicked.Column).SortBinding is MultiBinding)
                    {
                        var multiBinding = (MultiBinding)((SortableGridViewColumn)headerClicked.Column).SortBinding;
                        header.AddRange(multiBinding.Bindings.OfType<Binding>().Select(binding => binding.Path.Path));
                    }
                    else if (headerClicked.Column.DisplayMemberBinding is Binding)
                    {
                        var binding = headerClicked.Column.DisplayMemberBinding as Binding;
                        header.Add(binding.Path.Path);
                    }
                    else
                    {
                        header.Add(headerClicked.Column.Header as string);
                    }

                    // multi binding columns may not have anything to sort by.
                    if (header.Count == 0)
                        return;

                    // Get the previous direction a column was sorted by.
                    var oldItem = _sortList.FirstOrDefault(i => i.Column != null && i.Column.Equals(headerClicked.Column));
                    ListSortDirection direction;
                    if (oldItem == null)
                        direction = ListSortDirection.Ascending;
                    else
                        direction = oldItem.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

                 
                    for (int i = 0; i < _sortList.Count; i++)
                    {
                        if (_sortList[i].Column == null || headerClicked.Column.Equals(_sortList[i].Column))
                        {
                            // Remove arrow from previously sorted header 
                            if (_sortList[i].Column != null)
                                _sortList[i].Column.HeaderTemplate = null;
                            _sortList.RemoveAt(i);
                            i--;
                        }
                    }

                    int countUnique = 1;
                    GridViewColumn lastColumn = null;
                    for (int i = _sortList.Count - 1; i >= 0; i--)
                    {
                        if (!_sortList[i].Column.Equals(lastColumn))
                        {
                            lastColumn = _sortList[i].Column;
                            countUnique++;
                        }

                        // restrict to 1 sortable columns.
                        if (countUnique > MaxSortableColumns)
                        {
                            // Remove arrow from previously sorted header 
                            _sortList[i].Column.HeaderTemplate = null;
                            _sortList.RemoveAt(i);
                        }
                    }

                    foreach (var colPath in header)
                        _sortList.Add(new SortColumn(colPath, direction, headerClicked.Column));

                    Sort(listView, _sortList);

                    if (direction == ListSortDirection.Ascending)
                    {
                        if (ColumnHeaderArrowUpTemplate != null)
                            headerClicked.Column.HeaderTemplate = ColumnHeaderArrowUpTemplate;
                    }
                    else
                    {
                        if (ColumnHeaderArrowDownTemplate != null)
                            headerClicked.Column.HeaderTemplate = ColumnHeaderArrowDownTemplate;
                    }
                }
            }
        }

        private static void Sort(ItemsControl lv, List<SortColumn> sortList)
        {
            if (lv.ItemsSource != null)
            {
                var dataView = CollectionViewSource.GetDefaultView(lv.ItemsSource);
                //ICollectionView dataView = lv.Items as ICollectionView;

                dataView.SortDescriptions.Clear();
                for (int i = sortList.Count - 1; i >= 0; i--)
                    dataView.SortDescriptions.Add(new SortDescription(sortList[i].SortPath, sortList[i].SortDirection));

                dataView.Refresh();
            }
        }

        public static readonly RoutedEvent MouseDoubleClickItemEvent = EventManager.RegisterRoutedEvent("MouseDoubleClickItem", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), typeof(SortableListView));

        // Events
        public event MouseButtonEventHandler MouseDoubleClickItem;

        private void MouseDoubleClickedHandler(object sender, RoutedEventArgs e)
        {
            var item = ((ListView)sender).GetHitControl<ListViewItem>((MouseEventArgs)e);
            if (item != null)
            {
                if (MouseDoubleClickItem != null)
                {
                    var args = e as MouseButtonEventArgs;
                    MouseDoubleClickItem(sender, args);
                }
            }
        }

        protected class SortColumn
        {
            public string SortPath { get; set; }
            public ListSortDirection SortDirection { get; set; }
            public GridViewColumn Column { get; set; }

            public SortColumn(  string sortPath, ListSortDirection sortDirection, GridViewColumn column )
            {
                SortPath = sortPath;
                SortDirection = sortDirection;
                Column = column;
            }
        }
    }
}
