namespace SEToolbox.Services
{
    using SEToolbox.Interfaces;
    using SEToolbox.Models;
    using SEToolbox.ViewModels;
    using SEToolbox.Views;

    public static class DialogExtensions
    {
        public static bool? ShowErrorDialog(this IDialogService dialogService, BaseViewModel parentViewModel, string errorTitle, string errorInformation, bool canContinue)
        {
            var model = new ErrorDialogModel();
            model.Load(errorTitle, errorInformation, canContinue);
            var loadVm = new ErrorDialogViewModel(parentViewModel, model);
            return dialogService.ShowDialog<WindowErrorDialog>(parentViewModel, loadVm);
        }
    }
}
