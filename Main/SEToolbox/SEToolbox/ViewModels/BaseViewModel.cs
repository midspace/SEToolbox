namespace SEToolbox.ViewModels
{
    using SEToolbox.Support;
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    public class BaseViewModel : INotifyPropertyChanged
    {
        #region fields

        private BaseViewModel ownerViewModel;
        //protected object dataModel;

        #endregion

        #region ctor

        public BaseViewModel(BaseViewModel ownerViewModel)
            : base()
        {
            this.ownerViewModel = ownerViewModel;
        }

        #endregion

        #region properties

        public BaseViewModel OwnerViewModel
        {
            get
            {
                return this.ownerViewModel;
            }

            set
            {
                if (this.ownerViewModel != value)
                {
                    this.ownerViewModel = value;
                    PropertyChanged.Raise(() => OwnerViewModel);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        //[Obsolete("Use RaisePropertyChanged(() => PropertyName) instead.")]
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RaisePropertyChanged(params Expression<Func<object>>[] expression)
        {
            PropertyChanged.Raise(expression);
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}