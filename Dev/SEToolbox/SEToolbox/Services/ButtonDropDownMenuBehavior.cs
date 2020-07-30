namespace SEToolbox.Services
{
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Xaml.Behaviors;

    /// <summary>
    /// For enabling DropDown menu on Button
    /// </summary>
    public class ButtonDropDownMenuBehavior : Behavior<Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            ContextMenuService.SetIsEnabled(this.AssociatedObject, false);
            this.AssociatedObject.Click += AssociatedObject_Click;
        }

        void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
            // Loads context menu from Button as a Drop down Menu.
            var button = sender as Button;
            button.ContextMenu.IsEnabled = true;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }
    }
}
