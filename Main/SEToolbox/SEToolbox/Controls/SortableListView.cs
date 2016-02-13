namespace SEToolbox.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    using SEToolbox.Support;

    public class SortableListView : ListView
    {
        #region fields

        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private GridViewColumn _lastColumnClicked;
        private List<string> _lastSortList;

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
                if (dataView.SortDescriptions.Count == 0 && _lastSortList != null && _lastSortList.Count > 0)
                {
                    foreach (var sortBy in _lastSortList)
                    {
                        var sd = new SortDescription(sortBy, _lastDirection);
                        dataView.SortDescriptions.Add(sd);
                    }
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
                    var list = new List<string> { DefaultSortColumn };
                    Sort(this, list, ListSortDirection.Ascending);

                    if (ColumnHeaderArrowUpTemplate != null)
                        selectedColumn.HeaderTemplate = ColumnHeaderArrowUpTemplate;

                    _lastColumnClicked = selectedColumn;
                    _lastDirection = ListSortDirection.Ascending;
                    _lastSortList = list;
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
                    ListSortDirection direction;

                    if (headerClicked.Column != _lastColumnClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

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

                    if (header.Count == 0)
                        return;

                    Sort(listView, header, direction);

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

                    // Remove arrow from previously sorted header 
                    if (_lastColumnClicked != null && _lastColumnClicked != headerClicked.Column)
                    {
                        _lastColumnClicked.HeaderTemplate = null;
                    }

                    _lastColumnClicked = headerClicked.Column;
                    _lastDirection = direction;
                    _lastSortList = header;
                }
            }
        }

        private static void Sort(ItemsControl lv, List<string> sortList, ListSortDirection direction)
        {
            if (lv.ItemsSource != null)
            {
                var dataView = CollectionViewSource.GetDefaultView(lv.ItemsSource);
                //ICollectionView dataView = lv.Items as ICollectionView;

                dataView.SortDescriptions.Clear();
                foreach (var sortBy in sortList)
                {
                    var sd = new SortDescription(sortBy, direction);
                    dataView.SortDescriptions.Add(sd);
                }
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
    }
}
