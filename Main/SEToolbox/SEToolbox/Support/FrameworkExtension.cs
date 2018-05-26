namespace SEToolbox.Support
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Threading;
    using Res = SEToolbox.Properties.Resources;

    public static class FrameworkExtension
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
            var ctrlType = root.GetType();
            var attr = (ContentPropertyAttribute)ctrlType.GetCustomAttributes(typeof(ContentPropertyAttribute), true).FirstOrDefault();

            if (attr != null)
            {
                var prop = ctrlType.GetProperty(attr.Name);

                if (prop.PropertyType.GetInterfaces().Contains<Type>(typeof(IEnumerable)))
                {
                    foreach (var child in ((IEnumerable)prop.GetValue(root, null)).OfType<FrameworkElement>())
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
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
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
        public static T FindVisualParent<T>(this DependencyObject child)
            where T : DependencyObject
        {
            // get parent item
            var parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            var parent = parentObject as T;
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

        /// <summary>
        /// Get the UIElement that is in the container at the point specified
        /// </summary>
        /// <param name="container"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        internal static UIElement GetUIElement(this ItemsControl container, Point position)
        {
            var elementAtPosition = container.InputHitTest(position) as UIElement;
            //move up the UI tree until you find the actual UIElement that is the Item of the container
            if (elementAtPosition != null)
            {
                while (elementAtPosition != null)
                {
                    var testUiElement = container.ItemContainerGenerator.ItemFromContainer(elementAtPosition);
                    if (testUiElement != DependencyProperty.UnsetValue) //if found the UIElement
                    {
                        return elementAtPosition;
                    }
                    else
                    {
                        elementAtPosition = System.Windows.Media.VisualTreeHelper.GetParent(elementAtPosition) as UIElement;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Determines if the relative position is above the UIElement in the coordinate
        /// </summary>
        /// <param name="i"></param>
        /// <param name="relativePosition"></param>
        /// <returns></returns>
        internal static bool IsPositionAboveElement(this UIElement i, Point relativePosition)
        {
            if (relativePosition != null)
                if (relativePosition.Y < ((FrameworkElement)i).ActualHeight / 2) //if above
                    return true;
            return false;
        }

        /// <summary>
        /// Moves the keyboard focus away from this element and to another element in a provided traversal direction.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="direction"></param>
        /// <param name="wrap"></param>
        internal static void MoveFocus(this FrameworkElement control, FocusNavigationDirection direction = FocusNavigationDirection.Next, bool wrap = true)
        {
            control.Dispatcher.Invoke(DispatcherPriority.Input, (Action)(() =>
            {
                var request = new TraversalRequest(direction) { Wrapped = wrap };
                control.MoveFocus(request);
            }));
        }

        internal static void FocusedElementMoveFocus()
        {
            (Keyboard.FocusedElement as FrameworkElement).MoveFocus();
        }

        public static decimal ToDecimal(this float value)
        {
            return Convert.ToDecimal(value.ToString("G9", null));
        }

        public static double ToDouble(this float value)
        {
            return Convert.ToDouble(value.ToString("G9", null));
        }

        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static byte RoundUpToNearest(this byte value, int scale)
        {
            return (byte)(Math.Min(0xff, Math.Ceiling((double)value / scale) * scale));
        }

        #region GetHitControl

        /// <summary>
        /// Used to determine what T was clicked on during a DoubleClick event, or a Context menu open
        ///     If a MouseDoubleClick, pass in the MouseButtonEventArgs.
        ///     If a ContextMenu Opened, pass in Null.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static T GetHitControl<T>(this UIElement parentControl, MouseEventArgs e)
        {
            Point hit;

            if (e == null)
                hit = Mouse.GetPosition(parentControl);
            else
                hit = e.GetPosition(parentControl);
            object obj = parentControl.InputHitTest(hit);

            if ((obj != null) && (obj is FrameworkElement))
            {
                var control = obj;
                while (control != null)
                {
                    if (control.GetType().GetProperty("TemplatedParent").GetValue(control, null) != null)
                        control = (FrameworkElement)obj.GetType().GetProperty("TemplatedParent").GetValue(control, null);
                    else if (control == parentControl)
                        break;
                    else if (control is FrameworkElement)
                        control = ((FrameworkElement)control).Parent;
                    else
                        break;

                    if (control is T)
                    {
                        return (T)control;
                    }
                }
            }

            return default(T);
        }

        #endregion

        /// <summary>
        /// Adds an element with the provided key and value to the System.Collections.Generic.IDictionary&gt;TKey,TValue&lt;.
        /// If the provide key already exists, then the existing key is updated with the newly supplied value.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="System.ArgumentNullException">key is null</exception>
        /// <exception cref="System.NotSupportedException">The System.Collections.Generic.IDictionary&gt;TKey,TValue&lt; is read-only.</exception>
        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        /// <summary>
        /// Concatenates the Message portion of each exception and inner exception together into a string, in much the same manner as .ToString() except without the stack.
        /// </summary>
        public static string AllMessages(this Exception exception)
        {
            Exception ex = exception;

            StringBuilder text = new StringBuilder();
            text.Append(ex.Message);
            while (ex.InnerException != null)
            {
                text.AppendLine();
                text.Append(" ---> ");
                text.AppendLine(ex.InnerException.Message);

                if (ex.InnerException is InvalidOperationException)
                {
                    text.AppendLine(Res.ErrorStackLabel);
                    text.AppendLine(ex.InnerException.StackTrace);
                }

                ex = ex.InnerException;
            }

            return text.ToString();
        }
    }
}