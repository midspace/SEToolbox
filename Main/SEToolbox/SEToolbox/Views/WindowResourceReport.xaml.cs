﻿namespace SEToolbox.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for WindowResourceReport.xaml
    /// </summary>
    public partial class WindowResourceReport : Window
    {
        public WindowResourceReport()
        {
            InitializeComponent();
        }

        // TODO: remove from code behind, into behavior?
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.ContextMenu.IsEnabled = true;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }
    }
}
