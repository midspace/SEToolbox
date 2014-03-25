namespace SEToolbox.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
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
            this.AddHandler(ListView.MouseDoubleClickEvent, new RoutedEventHandler(MouseDoubleClickedHandler));
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

                    Sort(listView, header, direction);

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

        private static void Sort(ItemsControl lv, string sortBy, ListSortDirection direction)
        {
            if (lv.ItemsSource != null)
            {
                var dataView = CollectionViewSource.GetDefaultView(lv.ItemsSource);
                //ICollectionView dataView = lv.Items as ICollectionView;

                dataView.SortDescriptions.Clear();
                var sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        public static readonly RoutedEvent MouseDoubleClickItemEvent = EventManager.RegisterRoutedEvent("MouseDoubleClickItem", RoutingStrategy.Direct, typeof(MouseButtonEventHandler), typeof(SortableListView));

        // Events
        public event MouseButtonEventHandler MouseDoubleClickItem;

        private void MouseDoubleClickedHandler(object sender, RoutedEventArgs e)
        {
            var item = GetHitControl((ListView)sender, (MouseEventArgs)e);
            if (item != null)
            {
                if (this.MouseDoubleClickItem != null)
                {
                    var args = e as MouseButtonEventArgs;
                    this.MouseDoubleClickItem(sender, args);
                }
            }
        }

        #region GetHitControl

        /// <summary>
        /// Used to determine what ListViewItem was clicked on during a DoubleClick event, or a Context menu open
        ///     If a MouseDoubleClick, pass in the MouseButtonEventArgs.
        ///     If a ContextMenu Opened, pass in Null.
        /// </summary>
        /// <param name="listControl"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ListViewItem GetHitControl(UIElement listControl, MouseEventArgs e)
        {
            Point hit;

            if (e == null)
                hit = Mouse.GetPosition(listControl);
            else
                hit = e.GetPosition(listControl);
            object obj = listControl.InputHitTest(hit);

            if ((obj != null) && (obj is FrameworkElement))
            {
                object control = obj;
                while (control != null)
                {
                    if (control.GetType().GetProperty("TemplatedParent").GetValue(control, null) != null)
                        control = (FrameworkElement)obj.GetType().GetProperty("TemplatedParent").GetValue(control, null);
                    else if (control == listControl)
                        break;
                    else if (control is FrameworkElement)
                        control = ((FrameworkElement)control).Parent;
                    else
                        break;

                    if (control is ListViewItem)
                    {
                        return control as ListViewItem;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
