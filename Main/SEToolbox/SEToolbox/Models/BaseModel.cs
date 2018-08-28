namespace SEToolbox.Models
{
    using System;
    using System.ComponentModel;

    [Serializable]
    public class BaseModel : INotifyPropertyChanged
    {
        #region Methods

        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// Use the <see cref="nameof()"/> in conjunction with OnPropertyChanged.
        /// This will set the property name into a string during compile, which will be faster to execute then a runtime interpretation.
        /// </summary>
        /// <param name="propertyNames">The name of the property that changed.</param>
        protected void OnPropertyChanged(params string[] propertyNames)
        {
            if (_propertyChanged != null)
            {
                foreach (var propertyName in propertyNames)
                    _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        [NonSerialized]
        PropertyChangedEventHandler _propertyChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { if (_propertyChanged != null) _propertyChanged -= value; }
        }

        #endregion
    }
}
