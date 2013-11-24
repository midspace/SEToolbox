namespace SEToolbox.Views
{
    using System.Windows;
    using System.Windows.Controls;

    public class ViewTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            //if (element != null && item != null && item is DefaultJobsTableViewModel)
            //{
            //    DefaultJobsTableViewModel taskitem = item as DefaultJobsTableViewModel;
            //    return element.FindResource("DefaultJobsViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is DefaultLastJobTableViewModel)
            //{
            //    DefaultLastJobTableViewModel taskitem = item as DefaultLastJobTableViewModel;
            //    return element.FindResource("DefaultLastJobViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is OtherJobsTableViewModel)
            //{
            //    OtherJobsTableViewModel taskitem = item as OtherJobsTableViewModel;
            //    return element.FindResource("OtherJobsViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is AllJobsTableViewModel)
            //{
            //    AllJobsTableViewModel taskitem = item as AllJobsTableViewModel;
            //    return element.FindResource("AllJobsViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is AllLastJobTableViewModel)
            //{
            //    AllLastJobTableViewModel taskitem = item as AllLastJobTableViewModel;
            //    return element.FindResource("AllLastJobViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is ProcessJobsTableViewModel)
            //{
            //    ProcessJobsTableViewModel taskitem = item as ProcessJobsTableViewModel;
            //    return element.FindResource("ProcessJobsViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is JobStatisticsViewModel)
            //{
            //    JobStatisticsViewModel taskitem = item as JobStatisticsViewModel;
            //    return element.FindResource("JobStatisticsViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is JobParametersViewModel)
            //{
            //    JobParametersViewModel taskitem = item as JobParametersViewModel;
            //    return element.FindResource("JobParametersViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is JobProgressViewModel)
            //{
            //    JobProgressViewModel taskitem = item as JobProgressViewModel;
            //    return element.FindResource("JobProgressViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is ProcessorPerformanceViewModel)
            //{
            //    ProcessorPerformanceViewModel taskitem = item as ProcessorPerformanceViewModel;
            //    return element.FindResource("ProcessorPerformanceViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is ServiceDiscoveryViewModel)
            //{
            //    ServiceDiscoveryViewModel taskitem = item as ServiceDiscoveryViewModel;
            //    return element.FindResource("ServiceDiscoveryViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is B2BConfigurationViewModel)
            //{
            //    B2BConfigurationViewModel taskitem = item as B2BConfigurationViewModel;
            //    return element.FindResource("B2BConfigurationViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is B2BPerformanceViewModel)
            //{
            //    B2BPerformanceViewModel taskitem = item as B2BPerformanceViewModel;
            //    return element.FindResource("B2BPerformanceViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is B2BOverviewViewModel)
            //{
            //    B2BOverviewViewModel taskitem = item as B2BOverviewViewModel;
            //    return element.FindResource("B2BOverviewViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is B2BMessageQueueViewModel)
            //{
            //    B2BMessageQueueViewModel taskitem = item as B2BMessageQueueViewModel;
            //    return element.FindResource("B2BMessageQueueViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is B2BMessageStoreViewModel)
            //{
            //    B2BMessageStoreViewModel taskitem = item as B2BMessageStoreViewModel;
            //    return element.FindResource("B2BMessageStoreViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is B2BMessageViewModel)
            //{
            //    B2BMessageViewModel taskitem = item as B2BMessageViewModel;
            //    return element.FindResource("B2BMessageViewTemplate") as DataTemplate;
            //}
            //else if (element != null && item != null && item is DefaultViewModel)
            //{
            //    DefaultViewModel taskitem = item as DefaultViewModel;
            //    return element.FindResource("DefaultViewTemplate") as DataTemplate;
            //}

            return null;
        }
    }
}
