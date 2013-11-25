namespace SEToolbox.ViewModels
{
    using SEToolbox.Models;
    using SEToolbox.Services;
    using System;
    using System.Reflection;
    using System.Windows.Input;
    using System.Linq;

    public class AboutViewModel : BaseViewModel
    {
        #region Fields

        private SelectWorldModel dataModel;
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

        #endregion
    }
}
