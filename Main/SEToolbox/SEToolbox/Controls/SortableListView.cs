namespace SEToolbox.Controls
{
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
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        GridViewColumnHeader _lastHeaderClicked;

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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // add the event handler to the GridViewColumnHeader. This strongly ties this ListView to a GridView.
            AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
            AddHandler(ListView.MouseDoubleClickEvent, new RoutedEventHandler(MouseDoubleClickedHandler));
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

                    if (headerClicked != _lastHeaderClicked)
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
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
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
