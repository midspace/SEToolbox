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

        private bool? closeResult;

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
            get
            {
                return this.closeResult;
            }

            set
            {
                this.closeResult = value;
                this.RaisePropertyChanged(() => CloseResult);
            }
        }

        public ICommand OpenLinkCommand
        {
            get
            {
                return new DelegateCommand(new Action(OpenLinkExecuted), new Func<bool>(OpenLinkCanExecute));
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new DelegateCommand(new Action(CloseExecuted), new Func<bool>(CloseCanExecute));
            }
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
            get
            {
                var version = Assembly.GetExecutingAssembly()
                     .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)
                     .OfType<AssemblyFileVersionAttribute>()
                     .FirstOrDefault();
                return new Version(version.Version);
            }
        }

        public string HomepageUrl
        {
            get
            {
                return AppConstants.SupportUrl;
            }
        }

        #endregion

        #region methods

        public bool CloseCanExecute()
        {
            return true;
        }

        public void CloseExecuted()
        {
            this.CloseResult = false;
        }

        public bool OpenLinkCanExecute()
        {
            return true;
        }

        public void OpenLinkExecuted()
        {
            Process.Start(this.HomepageUrl);
        }

        #endregion
    }
}
