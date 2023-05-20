namespace SEToolbox.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using DialogResult = System.Windows.Forms.DialogResult;

    using SEToolbox.Interfaces;

    /// <summary>
    /// Class responsible for abstracting ViewModels from Views.
    /// </summary>
    class DialogService : IDialogService
    {
        private readonly HashSet<FrameworkElement> _views;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogService"/> class.
        /// </summary>
        public DialogService()
        {
            _views = new HashSet<FrameworkElement>();
        }

        #region IDialogService Members

        /// <summary>
        /// Gets the registered views.
        /// </summary>
        public ReadOnlyCollection<FrameworkElement> Views
        {
            get { return new ReadOnlyCollection<FrameworkElement>(_views.ToList()); }
        }

        /// <summary>
        /// Registers a View.
        /// </summary>
        /// <param name="view">The registered View.</param>
        public void Register(FrameworkElement view)
        {
            // Get owner window
            var owner = GetOwner(view);
            if (owner == null)
            {
                // Perform a late register when the View hasn't been loaded yet.
                // This will happen if e.g. the View is contained in a Frame.
                view.Loaded += LateRegister;
                return;
            }

            // Register for owner window closing, since we then should unregister View reference,
            // preventing memory leaks
            owner.Closed += OwnerClosed;

            _views.Add(view);
        }

        /// <summary>
        /// Unregisters a View.
        /// </summary>
        /// <param name="view">The unregistered View.</param>
        public void Unregister(FrameworkElement view)
        {
            _views.Remove(view);
        }

        /// <summary>
        /// Shows a dialog.
        /// </summary>
        /// <param name="ownerViewModel">
        /// A ViewModel that represents the owner window of the dialog.
        /// </param>
        /// <param name="viewModel">The ViewModel of the new dialog.</param>
        /// <typeparam name="T">The type of the dialog to show.</typeparam>
        /// <returns>
        /// A nullable value of type bool that signifies how a window was closed by the user.
        /// </returns>
        public bool? ShowDialog<T>(object ownerViewModel, object viewModel) where T : Window
        {
            return ShowDialog(ownerViewModel, viewModel, typeof(T), null);
        }

        public bool? ShowDialog<T>(object ownerViewModel, object viewModel, Action action) where T : Window
        {
            return ShowDialog(ownerViewModel, viewModel, typeof(T), action);
        }

        /// <summary>
        /// Shows a non-modal dialog.
        /// </summary>
        /// <param name="ownerViewModel">
        /// A ViewModel that represents the owner window of the dialog.
        /// </param>
        /// <param name="viewModel">The ViewModel of the new dialog.</param>
        /// <typeparam name="T">The type of the dialog to show.</typeparam>
        public void Show<T>(object ownerViewModel, object viewModel) where T : Window
        {
            Show(ownerViewModel, viewModel, typeof(T));
        }

        /// <summary>
        /// Shows a message box.
        /// </summary>
        /// <param name="ownerViewModel">
        /// A ViewModel that represents the owner window of the message box.
        /// </param>
        /// <param name="messageBoxText">A string that specifies the text to display.</param>
        /// <param name="caption">A string that specifies the title bar caption to display.</param>
        /// <param name="button">
        /// A MessageBoxButton value that specifies which button or buttons to display.
        /// </param>
        /// <param name="icon">A MessageBoxImage value that specifies the icon to display.</param>
        /// <returns>
        /// A MessageBoxResult value that specifies which message box button is clicked by the user.
        /// </returns>
        public MessageBoxResult ShowMessageBox(
            object ownerViewModel,
            string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon)
        {
            return MessageBox.Show(FindOwnerWindow(ownerViewModel), messageBoxText, caption, button, icon);
        }

        /// <summary>
        /// Shows the OpenFileDialog.
        /// </summary>
        /// <param name="ownerViewModel">
        /// A ViewModel that represents the owner window of the dialog.
        /// </param>
        /// <param name="openFileDialog">The interface of a open file dialog.</param>
        /// <returns>DialogResult.OK if successful; otherwise DialogResult.Cancel.</returns>
        public DialogResult ShowOpenFileDialog(object ownerViewModel, IOpenFileDialog openFileDialog)
        {
            // Create OpenFileDialog with specified ViewModel
            var dialog = new OpenFileDialog(openFileDialog);

            // Show dialog
            return dialog.ShowDialog(new WindowWrapper(FindOwnerWindow(ownerViewModel)));
        }

        /// <summary>
        /// Shows the SaveFileDialog.
        /// </summary>
        /// <param name="ownerViewModel">
        /// A ViewModel that represents the owner window of the dialog.
        /// </param>
        /// <param name="saveFileDialog">The interface of a save file dialog.</param>
        /// <returns>DialogResult.OK if successful; otherwise DialogResult.Cancel.</returns>
        public DialogResult ShowSaveFileDialog(object ownerViewModel, ISaveFileDialog saveFileDialog)
        {
            // Create SaveFileDialog with specified ViewModel
            var dialog = new SaveFileDialog(saveFileDialog);

            // Show dialog
            return dialog.ShowDialog(new WindowWrapper(FindOwnerWindow(ownerViewModel)));
        }

        /// <summary>
        /// Shows the FolderBrowserDialog.
        /// </summary>
        /// <param name="ownerViewModel">
        /// A ViewModel that represents the owner window of the dialog.
        /// </param>
        /// <param name="folderBrowserDialog">The interface of a folder browser dialog.</param>
        /// <returns>The DialogResult.OK if successful; otherwise DialogResult.Cancel.</returns>
        public DialogResult ShowFolderBrowserDialog(object ownerViewModel, IFolderBrowserDialog folderBrowserDialog)
        {
            // Create FolderBrowserDialog with specified ViewModel
            var dialog = new FolderBrowserDialog(folderBrowserDialog);

            // Show dialog
            return dialog.ShowDialog(new WindowWrapper(FindOwnerWindow(ownerViewModel)));
        }

        /// <summary>
        /// Shows the ColorDialog.
        /// </summary>
        /// <param name="ownerViewModel">
        /// A ViewModel that represents the owner window of the dialog.
        /// </param>
        /// <param name="colorDialog">The interface of a color dialog.</param>
        /// <returns>The DialogResult.OK if successful; otherwise DialogResult.Cancel.</returns>
        public DialogResult ShowColorDialog(object ownerViewModel, IColorDialog colorDialog)
        {
            // Create ColorDialog with specified ViewModel
            var dialog = new ColorDialog(colorDialog);

            // Show dialog
            return dialog.ShowDialog(new WindowWrapper(FindOwnerWindow(ownerViewModel)));
        }

        #endregion

        #region Attached properties

        /// <summary>
        /// Attached property describing whether a FrameworkElement is acting as a View in MVVM.
        /// </summary>
        public static readonly DependencyProperty IsRegisteredViewProperty =
            DependencyProperty.RegisterAttached(
            "IsRegisteredView",
            typeof(bool),
            typeof(DialogService),
            new UIPropertyMetadata(IsRegisteredViewPropertyChanged));


        /// <summary>
        /// Gets value describing whether FrameworkElement is acting as View in MVVM.
        /// </summary>
        public static bool GetIsRegisteredView(FrameworkElement target)
        {
            return (bool)target.GetValue(IsRegisteredViewProperty);
        }

        /// <summary>
        /// Sets value describing whether FrameworkElement is acting as View in MVVM.
        /// </summary>
        public static void SetIsRegisteredView(FrameworkElement target, bool value)
        {
            target.SetValue(IsRegisteredViewProperty, value);
        }

        /// <summary>
        /// Is responsible for handling IsRegisteredViewProperty changes, i.e. whether
        /// FrameworkElement is acting as View in MVVM or not.
        /// </summary>
        private static void IsRegisteredViewPropertyChanged(DependencyObject target,
            DependencyPropertyChangedEventArgs e)
        {
            // The Visual Studio Designer or Blend will run this code when setting the attached
            // property, however at that point there is no IDialogService registered
            // in the ServiceLocator which will cause the Resolve method to throw a ArgumentException.
            if (DesignerProperties.GetIsInDesignMode(target)) return;

            var view = target as FrameworkElement;
            if (view != null)
            {
                // Cast values
                var newValue = (bool)e.NewValue;
                var oldValue = (bool)e.OldValue;

                if (newValue)
                {
                    ServiceLocator.Resolve<IDialogService>().Register(view);
                }
                else
                {
                    ServiceLocator.Resolve<IDialogService>().Unregister(view);
                }
            }
        }

        #endregion

        /// <summary>
        /// Shows a dialog.
        /// </summary>
        /// <param name="ownerViewModel">
        /// A ViewModel that represents the owner window of the dialog.
        /// </param>
        /// <param name="viewModel">The ViewModel of the new dialog.</param>
        /// <param name="dialogType">The type of the dialog.</param>
        /// <returns>
        /// A nullable value of type bool that signifies how a window was closed by the user.
        /// </returns>
        private bool? ShowDialog(object ownerViewModel, object viewModel, Type dialogType, Action action)
        {
            // Create dialog and set properties
            var dialog = (Window)Activator.CreateInstance(dialogType);
            dialog.Owner = FindOwnerWindow(ownerViewModel);
            dialog.DataContext = viewModel;

            if (action != null)
                dialog.Loaded += (sender, e) => { action.Invoke(); };

            // Show dialog
            var retValue = dialog.ShowDialog();
            dialog.Close();

            System.Windows.Forms.Application.DoEvents();
            return retValue;
        }

        /// <summary>
        /// Shows a non-modal dialog.
        /// </summary>
        /// <param name="ownerViewModel">
        /// A ViewModel that represents the owner window of the dialog.
        /// </param>
        /// <param name="viewModel">The ViewModel of the new dialog.</param>
        /// <param name="dialogType">The type of the dialog.</param>
        /// <returns>
        /// A nullable value of type bool that signifies how a window was closed by the user.
        /// </returns>
        private void Show(object ownerViewModel, object viewModel, Type dialogType)
        {
            // Create dialog and set properties
            var dialog = (Window)Activator.CreateInstance(dialogType);
            dialog.Owner = FindOwnerWindow(ownerViewModel);
            dialog.DataContext = viewModel;

            // Show dialog
            dialog.Show();
        }

        /// <summary>
        /// Finds window corresponding to specified ViewModel.
        /// </summary>
        private Window FindOwnerWindow(object viewModel)
        {
            var view = _views.SingleOrDefault(v => ReferenceEquals(v.DataContext, viewModel));
            if (view == null)
            {
                throw new ArgumentException("Viewmodel is not referenced by any registered View.");
            }

            // Get owner window
            var owner = view as Window;
            if (owner == null)
            {
                owner = Window.GetWindow(view);
            }

            // Make sure owner window was found
            if (owner == null)
            {
                throw new InvalidOperationException("View is not contained within a Window.");
            }

            return owner;
        }


        /// <summary>
        /// Callback for late View register. It wasn't possible to do a instant register since the
        /// View wasn't at that point part of the logical nor visual tree.
        /// </summary>
        private void LateRegister(object sender, RoutedEventArgs e)
        {
            var view = sender as FrameworkElement;
            if (view != null)
            {
                // Unregister loaded event
                view.Loaded -= LateRegister;

                // Register the view
                Register(view);
            }
        }

        /// <summary>
        /// Handles owner window closed, View service should then unregister all Views acting
        /// within the closed window.
        /// </summary>
        private void OwnerClosed(object sender, EventArgs e)
        {
            var owner = sender as Window;
            if (owner != null)
            {
                // Find Views acting within closed window
                var windowViews =
                    from view in _views
                    where Window.GetWindow(view) == owner
                    select view;

                // Unregister Views in window
                foreach (var view in windowViews.ToArray())
                {
                    Unregister(view);
                }
            }
        }

        /// <summary>
        /// Gets the owning Window of a view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>The owning Window if found; otherwise null.</returns>
        private Window GetOwner(FrameworkElement view)
        {
            return view as Window ?? Window.GetWindow(view);
        }
    }
}
