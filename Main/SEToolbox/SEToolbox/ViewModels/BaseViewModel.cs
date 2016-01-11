namespace SEToolbox.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    using SEToolbox.Interfaces;
    using SEToolbox.Support;

    public class BaseViewModel : INotifyPropertyChanged
    {
        #region fields

        private BaseViewModel _ownerViewModel;

        #endregion

        #region ctor

        public BaseViewModel(BaseViewModel ownerViewModel)
        {
            _ownerViewModel = ownerViewModel;
        }

        #endregion

        #region properties

        public virtual BaseViewModel OwnerViewModel
        {
            get
            {
                return _ownerViewModel;
            }

            set
            {
                if (_ownerViewModel != value)
                {
                    _ownerViewModel = value;
                    PropertyChanged.Raise(() => OwnerViewModel);
                }
            }
        }

        public IMainView MainViewModel
        {
            get
            {
                return (IMainView)_ownerViewModel;
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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