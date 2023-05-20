namespace SEToolbox.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Input;

    using SEToolbox.Services;
    using SEToolbox.Support;

    public class AboutViewModel : BaseViewModel
    {
        #region Fields

        private bool? _closeResult;

        #endregion

        #region Constructors

        public AboutViewModel(BaseViewModel parentViewModel)
            : base(parentViewModel)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the DialogResult of the View.  If True or False is passed, this initiates the Close().
        /// </summary>
        public bool? CloseResult
        {
            get { return _closeResult; }

            set
            {
                _closeResult = value;
                OnPropertyChanged(nameof(CloseResult));
            }
        }

        public ICommand OpenLinkCommand
        {
            get { return new DelegateCommand(OpenLinkExecuted, OpenLinkCanExecute); }
        }

        public ICommand CloseCommand
        {
            get { return new DelegateCommand(CloseExecuted, CloseCanExecute); }
        }

        public string Company
        {
            get
            {
                var company = Assembly.GetExecutingAssembly()
                     .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)
                     .OfType<AssemblyCompanyAttribute>()
                     .FirstOrDefault();
                return company.Company;
            }
        }

        public Version Version
        {
            get { return GlobalSettings.GetAppVersion(); }
        }

        public string HomepageUrl
        {
            get { return SEToolbox.Properties.Resources.GlobalHomepageUrl; }
        }

        #endregion

        #region methods

        public bool CloseCanExecute()
        {
            return true;
        }

        public void CloseExecuted()
        {
            CloseResult = false;
        }

        public bool OpenLinkCanExecute()
        {
            return true;
        }

        public void OpenLinkExecuted()
        {
            Process.Start(HomepageUrl);
        }

        #endregion
    }
}
