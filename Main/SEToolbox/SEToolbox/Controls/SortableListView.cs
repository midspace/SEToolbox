namespace SEToolbox.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using SEToolbox.Support;

    public class SortableListView : ListView
    {
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        GridViewColumnHeader _lastHeaderClicked = null;

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
            this.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;

            // May be triggered by clicking on vertical scrollbar.
            if (headerClicked != null)
            {
                var listView = VisualTreeEnumeration.FindVisualParent<ListView>(headerClicked);
                ListSortDirection direction;

                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
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

                    string header;
                    if (headerClicked.Column.DisplayMemberBinding is Binding)
                    {
                        Binding binding = headerClicked.Column.DisplayMemberBinding as Binding;
                        header = binding.Path.Path;
                    }
                    else
                    {
                        header = headerClicked.Column.Header as string;
                    }

                    this.Sort(listView, header, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        if (this.ColumnHeaderArrowUpTemplate != null)
                            headerClicked.Column.HeaderTemplate = this.ColumnHeaderArrowUpTemplate;
                    }
                    else
                    {
                        if (this.ColumnHeaderArrowDownTemplate != null)
                            headerClicked.Column.HeaderTemplate = this.ColumnHeaderArrowDownTemplate;
                    }

                    //if (direction == ListSortDirection.Ascending)
                    //{
                    //    headerClicked.Column.HeaderTemplate = headerClicked.FindResource("HeaderTemplateArrowUp") as DataTemplate;
                    //}
                    //else
                    //{
                    //    headerClicked.Column.HeaderTemplate = headerClicked.FindResource("HeaderTemplateArrowDown") as DataTemplate;
                    //}

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

        private void Sort(ListView lv, string sortBy, ListSortDirection direction)
        {
            if (lv.ItemsSource != null)
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(lv.ItemsSource);
                //ICollectionView dataView = lv.Items as ICollectionView;

                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }
    }
}
