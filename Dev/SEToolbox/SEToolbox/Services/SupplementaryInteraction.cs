namespace SEToolbox.Services
{
    using System.Collections.Generic;
    using System.Windows;
    using Microsoft.Xaml.Behaviors;

    public class Behaviors : List<Behavior>
    {
    }

    public class Triggers : List<Microsoft.Xaml.Behaviors.TriggerBase>
    {
    }

    public static class SupplementaryInteraction
    {
        public static Behaviors GetBehaviors(DependencyObject obj)
        {
            return (Behaviors)obj.GetValue(BehaviorsProperty);
        }

        public static void SetBehaviors(DependencyObject obj, Behaviors value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }

        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached("Behaviors", typeof(Behaviors), typeof(SupplementaryInteraction), new UIPropertyMetadata(null, OnPropertyBehaviorsChanged));

        private static void OnPropertyBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behaviors = Interaction.GetBehaviors(d);
            foreach (var behavior in e.NewValue as Behaviors) behaviors.Add(behavior);
        }

        public static Triggers GetTriggers(DependencyObject obj)
        {
            return (Triggers)obj.GetValue(TriggersProperty);
        }

        public static void SetTriggers(DependencyObject obj, Triggers value)
        {
            obj.SetValue(TriggersProperty, value);
        }

        public static readonly DependencyProperty TriggersProperty = DependencyProperty.RegisterAttached("Triggers", typeof(Triggers), typeof(SupplementaryInteraction), new UIPropertyMetadata(null, OnPropertyTriggersChanged));

        private static void OnPropertyTriggersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var triggers = Interaction.GetTriggers(d);
            foreach (var trigger in e.NewValue as Triggers) triggers.Add(trigger);
        }
    }
}
