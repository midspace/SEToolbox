namespace SEToolbox.Services
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    public class CommandAction : TargetedTriggerAction<FrameworkElement>, ICommandSource
    {
        #region DPs

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CommandAction), new PropertyMetadata((ICommand)null, OnCommandChanged));
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(CommandAction), new PropertyMetadata());
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(CommandAction), new PropertyMetadata());
        public static readonly DependencyProperty SyncOwnerIsEnabledProperty = DependencyProperty.Register("SyncOwnerIsEnabled", typeof(bool), typeof(CommandAction), new PropertyMetadata());
        public static readonly DependencyProperty EventArgsProperty = DependencyProperty.Register("EventArgs", typeof(bool), typeof(CommandAction), new PropertyMetadata());

        #endregion

        #region Properties

        [Category("Command Properties")]
        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        [Category("Command Properties")]
        public object CommandParameter
        {
            get
            {
                return (object)GetValue(CommandParameterProperty);
            }
            set
            {
                SetValue(CommandParameterProperty, value);
            }
        }

        [Category("Command Properties")]
        public IInputElement CommandTarget
        {
            get
            {
                return (IInputElement)GetValue(CommandTargetProperty);
            }
            set
            {
                SetValue(CommandTargetProperty, value);
            }
        }

        [Category("Command Properties")]
        public bool SyncOwnerIsEnabled
        {
            get
            {
                return (bool)GetValue(SyncOwnerIsEnabledProperty);
            }
            set
            {
                SetValue(SyncOwnerIsEnabledProperty, value);
            }
        }


        [Category("Command Properties")]
        public bool EventArgs
        {
            get
            {
                return (bool)GetValue(EventArgsProperty);
            }
            set
            {
                SetValue(EventArgsProperty, value);
            }
        }

        #endregion

        #region Event Declaration

        private EventHandler CanExecuteChanged;

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
            if (this.Command != null)
            {
                RoutedCommand comRouted = this.Command as RoutedCommand;
                if (comRouted != null)
                {
                    if (this.EventArgs)
                    {
                        comRouted.Execute(o, this.CommandTarget);
                    }
                    else
                    {
                        // Is RoutedCommand
                        comRouted.Execute(this.CommandParameter, this.CommandTarget);
                    }
                }
                else
                {
                    if (this.EventArgs)
                    {
                        this.Command.Execute(o);
                    }
                    else
                    {
                        // Is NOT RoutedCommand
                        this.Command.Execute(this.CommandParameter);
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
            this.CanExecuteChanged = new EventHandler(OnCanExecuteChanged);
            command.CanExecuteChanged += CanExecuteChanged;
            UpdateCanExecute();
        }

        private void UnhookCommandCanExecuteChangedEventHandler(ICommand command)
        {
            command.CanExecuteChanged -= CanExecuteChanged;
            UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            if (this.Command != null)
            {
                RoutedCommand comRouted = this.Command as RoutedCommand;
                if (comRouted != null)
                {
                    // Is RoutedCommand
                    this.IsEnabled = comRouted.CanExecute(this.CommandParameter, this.CommandTarget);
                }
                else
                {
                    // Is NOT RoutedCommand
                    this.IsEnabled = this.Command.CanExecute(this.CommandParameter);
                }
                if (this.Target != null && this.SyncOwnerIsEnabled)
                {
                    this.Target.IsEnabled = IsEnabled;
                }
            }
        }

        #endregion
    }
}
