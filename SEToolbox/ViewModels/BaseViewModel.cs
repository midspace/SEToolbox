namespace SEToolbox.ViewModels
{
    using System.ComponentModel;

    using SEToolbox.Interfaces;

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
                    OnPropertyChanged(nameof(OwnerViewModel));
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
        /// Use the <see cref="nameof()"/> in conjunction with OnPropertyChanged.
        /// This will set the property name into a string during compile, which will be faster to execute then a runtime interpretation.
        /// </summary>
        /// <param name="propertyNames">The name of the property that changed.</param>
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged != null)
            {
                foreach (var propertyName in propertyNames)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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