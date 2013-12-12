namespace SEToolbox.Models
{
    using SEToolbox.Support;
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    [Serializable]
    public class BaseModel : INotifyPropertyChanged
    {
        #region Methods

        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        //[Obsolete("Use RaisePropertyChanged(() => PropertyName) instead.")]
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.propertyChanged != null)
            {
                this.propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void RaisePropertyChanged(params Expression<Func<object>>[] expression)
        {
            propertyChanged.Raise(expression);
        }

        #endregion

        #region INotifyPropertyChanged Members

        [NonSerialized]
        PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        #endregion
    }
}
