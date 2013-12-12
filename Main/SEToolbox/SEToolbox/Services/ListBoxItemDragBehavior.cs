namespace SEToolbox.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    /// <summary>
    /// Multi Select Item Drag.
    /// </summary>
    public class ListBoxItemDragBehavior : Behavior<ListBoxItem>
    {
        #region fields

        private bool isMouseClicked = false;
        private Point startPoint;
        private BindingBase dragMemberBinding;

        #endregion

        #region Properties

        public BindingBase DragSourceBinding
        {
            get
            {
                return this.dragMemberBinding;
            }
            set
            {
                if (this.dragMemberBinding != value)
                {
                    this.dragMemberBinding = value;
                }
            }
        }

        #endregion

        #region methods

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseLeave += new MouseEventHandler(AssociatedObject_MouseLeave);
            this.AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
        }

        #endregion

        #region events

        void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.isMouseClicked = false;
        }

        void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.isMouseClicked = true;
            this.startPoint = e.GetPosition(null);
        }

        void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.isMouseClicked)
            {
                // set the item's DataContext as the data to be transferred.
                IDragable dragObject = this.AssociatedObject.DataContext as IDragable;
                if (dragObject != null)
                {
                    DataObject data = new DataObject();

                    var parent = ItemsControl.ItemsControlFromItemContainer(this.AssociatedObject) as ListBox;
                    IList list = null;

                    if (this.DragSourceBinding == null)
                    {
                        // Pass the raw ItemSource as the drag object.
                        list = (IList)Activator.CreateInstance((typeof(List<>).MakeGenericType(dragObject.DataType)));
                        if (!this.AssociatedObject.IsSelected)
                        {
                            list.Add(this.AssociatedObject.DataContext);
                        }

                        foreach (var item in parent.SelectedItems)
                        {
                            list.Add(item);
                        }
                    }
                    else
                    {
                        // Pass the Binding object under the ItemSource as the drag object.
                        var propertyName = ((Binding)this.DragSourceBinding as Binding).Path.Path;
                        var pd = TypeDescriptor.GetProperties(this.AssociatedObject.DataContext).Find(propertyName, false);

                        list = (IList)Activator.CreateInstance((typeof(List<>).MakeGenericType(pd.PropertyType)));
                        if (!this.AssociatedObject.IsSelected)
                        {
                            list.Add(pd.GetValue(this.AssociatedObject.DataContext));
                        }

                        foreach (var item in parent.SelectedItems)
                        {
                            list.Add(pd.GetValue(item));
                        }
                    }

                    data.SetData(list.GetType(), list);

                    // Send the ListBox that initiated the drag, so we can determine if the drag and drop are different or not.
                    data.SetData(typeof(string), parent.Uid);

                    //data.SetData(dragObject.DataType, this.AssociatedObject.DataContext);
                    System.Windows.DragDrop.DoDragDrop(parent, data, DragDropEffects.Copy);
                    //System.Windows.DragDrop.DoDragDrop(this.AssociatedObject, data, DragDropEffects.Move);
                }
            }
            this.isMouseClicked = false;
        }
        #endregion
    }
}
