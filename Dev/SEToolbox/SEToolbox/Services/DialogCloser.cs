 namespace SEToolbox.Services
{
    using System;
    using System.Windows;

    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloser),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;

            if (window != null)
            {
                try
                {
                    window.DialogResult = e.NewValue as bool?;
                }
                catch (InvalidOperationException)
                {
                    window.Close();
                }
                catch
                {
                    // Ignore non-modal error.
                }
            }
        }
        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
}
