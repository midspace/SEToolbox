namespace SEToolbox.Services
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using Microsoft.Xaml.Behaviors;

    public class CommandAction : TargetedTriggerAction<FrameworkElement>, ICommandSource
    {
        #region DPs

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandAction), new PropertyMetadata(null, OnCommandChanged));
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(CommandAction), new PropertyMetadata());
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(CommandAction), new PropertyMetadata());
        public static readonly DependencyProperty SyncOwnerIsEnabledProperty = DependencyProperty.Register("SyncOwnerIsEnabled", typeof(bool), typeof(CommandAction), new PropertyMetadata());
        public static readonly DependencyProperty EventArgsProperty = DependencyProperty.Register("EventArgs", typeof(bool), typeof(CommandAction), new PropertyMetadata());

        #endregion

        #region Properties

        [Category("Command Properties")]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        [Category("Command Properties")]
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        [Category("Command Properties")]
        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        [Category("Command Properties")]
        public bool SyncOwnerIsEnabled
        {
            get { return (bool)GetValue(SyncOwnerIsEnabledProperty); }
            set { SetValue(SyncOwnerIsEnabledProperty, value); }
        }

        [Category("Command Properties")]
        public bool EventArgs
        {
            get { return (bool)GetValue(EventArgsProperty); }
            set { SetValue(EventArgsProperty, value); }
        }

        #endregion

        #region Event Declaration

        private EventHandler _canExecuteChanged;

        #endregion

        #region Event Handlers

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateCanExecute();
        }

        #region DP Event Handlers

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandAction)d).OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        #endregion

        #endregion

        #region Overrides

        [DebuggerStepThrough]
        protected override void Invoke(object o)
        {
            if (Command != null)
            {
                var comRouted = Command as RoutedCommand;
                if (comRouted != null)
                {
                    if (EventArgs)
                    {
                        comRouted.Execute(o, CommandTarget);
                    }
                    else
                    {
                        // Is RoutedCommand
                        comRouted.Execute(CommandParameter, CommandTarget);
                    }
                }
                else
                {
                    if (EventArgs)
                    {
                        Command.Execute(o);
                    }
                    else
                    {
                        // Is NOT RoutedCommand
                        Command.Execute(CommandParameter);
                    }
                }
            }
        }

        #endregion

        #region Helper functions

        private void OnCommandChanged(ICommand comOld, ICommand comNew)
        {
            if (comOld != null)
            {
                UnhookCommandCanExecuteChangedEventHandler(comOld);
            }
            if (comNew != null)
            {
                HookupCommandCanExecuteChangedEventHandler(comNew);
            }
        }

        private void HookupCommandCanExecuteChangedEventHandler(ICommand command)
        {
            _canExecuteChanged = new EventHandler(OnCanExecuteChanged);
            command.CanExecuteChanged += _canExecuteChanged;
            UpdateCanExecute();
        }

        private void UnhookCommandCanExecuteChangedEventHandler(ICommand command)
        {
            command.CanExecuteChanged -= _canExecuteChanged;
            UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            if (Command != null)
            {
                RoutedCommand comRouted = Command as RoutedCommand;
                if (comRouted != null)
                {
                    // Is RoutedCommand
                    IsEnabled = comRouted.CanExecute(CommandParameter, CommandTarget);
                }
                else
                {
                    // Is NOT RoutedCommand
                    IsEnabled = Command.CanExecute(CommandParameter);
                }
                if (Target != null && SyncOwnerIsEnabled)
                {
                    Target.IsEnabled = IsEnabled;
                }
            }
        }

        #endregion
    }
}
