namespace SEToolbox.Support
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Markup;
    using System.Windows.Media;

    public static class VisualTreeEnumeration
    {
        /// <summary>
        /// Finds all physical elements that are children of the specified element.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<FrameworkElement> Descendents(this FrameworkElement root)
        {
            return Descendents(root, Int32.MaxValue);
        }

        public static IEnumerable<FrameworkElement> Descendents(this FrameworkElement root, int depth)
        {
            Type ctrlType = root.GetType();
            ContentPropertyAttribute attr = (ContentPropertyAttribute)ctrlType.GetCustomAttributes(typeof(ContentPropertyAttribute), true).FirstOrDefault();

            if (attr != null)
            {
                PropertyInfo prop = ctrlType.GetProperty(attr.Name);

                if (prop.PropertyType.GetInterfaces().Contains<Type>(typeof(IEnumerable)))
                {
                    foreach (FrameworkElement child in ((IEnumerable)prop.GetValue(root, null)).OfType<FrameworkElement>())
                    {
                        yield return child;

                        foreach (var descendent in Descendents(child))
                        {
                            yield return (FrameworkElement)descendent;
                        }
                    }
                }
                else
                {
                    var child = prop.GetValue(root, null);

                    if (child is FrameworkElement)
                    {
                        yield return (FrameworkElement)child;

                        foreach (var descendent in Descendents((FrameworkElement)child))
                        {
                            yield return (FrameworkElement)descendent;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find all elements that are children of the specified element, including Templated controls.
        /// </summary>
        /// <remarks>
        /// This requires that the Visual element has been rendered in the visual tree. If it has not, then this won't find it.
        /// Ie., bound collections that go off the edge of the scroll area, or tabcontrols that have been opened.
        /// </remarks>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> VisualDescendents(this DependencyObject root)
        {
            return VisualDescendents(root, Int32.MaxValue);
        }

        public static IEnumerable<DependencyObject> VisualDescendents(this DependencyObject root, int depth)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                if (depth > 0)
                {
                    foreach (var descendent in VisualDescendents(child, --depth))
                        yield return descendent;
                }
            }
        }

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null reference is being returned.</returns>
        public static T FindVisualParent<T>(DependencyObject child)
          where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                // use recursion to proceed with next level
                return FindVisualParent<T>(parentObject);
            }
        }

        //public ItemsControl GetSelectedTreeViewItemParent<T>(TreeViewItem item)
        //{
        //    DependencyObject parent = VisualTreeHelper.GetParent(item);

        //    if (parent == null)
        //        return null;

        //    while (!(parent is TreeViewItem || parent is TreeView))
        //    {
        //        if (parent == null)
        //            return null;

        //        parent = VisualTreeHelper.GetParent(parent);
        //    }

        //    return parent as ItemsControl;
        //}
    }
}
