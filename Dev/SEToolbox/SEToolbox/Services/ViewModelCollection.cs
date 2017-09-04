namespace SEToolbox.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;

    // http://social.msdn.microsoft.com/Forums/vstudio/en-US/10713000-8069-4277-bab2-249f2f0af131/mvvm-question-syncing-a-collection-between-the-model-and-the-viewmodel?forum=wpf

    /// <summary>
    /// A collection of ViewModel objects that wraps a collection of Model objects,
    /// with each ViewModel object wrapping its' corresponding Model object.
    /// The two collections are synchronized.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    public class ViewModelCollection<TViewModel, TModel>
      : IList<TViewModel>, INotifyCollectionChanged
       where TViewModel : IModelWrapper
    {
        #region Private Members

        /// <summary>
        /// Contains all VM objects
        /// </summary>
        readonly List<TViewModel> _list;

        /// <summary>
        /// Reference to the collection in the model, which is wrapped by this collection
        /// </summary>
        readonly ObservableCollection<TModel> _model;

        /// <summary>
        /// A method to create a VM object from a Model object (possibly VM constructor)
        /// </summary>
        readonly Func<TModel, TViewModel> _createViewModel;

        #endregion //Private Members

        /// <summary>
        /// Creates a collection of VM objects
        /// 
        /// Note that the IModelWrapper constraint on TViewModel, combined with the 
        /// ViewModelFactory member, we get bidirectionality: we can both create a VM 
        /// from model, and also get the model from a VM.
        /// </summary>
        /// <param name="modelCollection">Reference to the collection in the model</param>
        /// <param name="viewModelCreator">A method to create a VM object from a Model object (possibly VM constructor)</param>
        public ViewModelCollection(ObservableCollection<TModel> modelCollection, Func<TModel, TViewModel> viewModelCreator)
        {
            if (modelCollection == null) throw new ArgumentNullException("modelCollection");
            if (viewModelCreator == null) throw new ArgumentNullException("viewModelCreator");

            _model = modelCollection;
            _model.CollectionChanged += new NotifyCollectionChangedEventHandler(model_CollectionChanged);

            _createViewModel = viewModelCreator;

            //inits the list to reflect initial state of model collection
            _list = new List<TViewModel>(from m in _model select viewModelCreator(m));
        }

        /// <summary>
        /// Listens to changes in the model collection and changes the VM list accordingly.
        /// This is the only place where changes to the list of VM objects (_list) occur, to
        /// guarantee that it represents the model collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void model_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    //create a VM object to wrap each new model object
                    foreach (var item in e.NewItems)
                    {
                        var vmItem = _createViewModel((TModel)item);

                        //the operation could have been either Insert or Add
                        if (e.NewStartingIndex != _list.Count)
                        {
                            _list.Insert(_model.IndexOf((TModel)item), vmItem);
                        }
                        else
                        {
                            _list.Add(vmItem);
                        }

                        //notify the change
                        OnCollectionChanged(e.Action, vmItem, e.NewStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    //remove instances of VM objects that wrap a removed model object
                    foreach (var item in e.OldItems)
                    {
                        //find VM objects that wrap the relevant model object and remove them
                        IEnumerable<TViewModel> query;
                        while ((query = from vm in _list where vm.GetModel() == item select vm).Count() > 0)
                        {
                            var vmItem = query.First();
                            var index = _list.IndexOf(vmItem);
                            _list.Remove(vmItem);
                            //notify the change
                            OnCollectionChanged(e.Action, vmItem, index);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _list.Clear();
                    //notify the change
                    OnCollectionChanged(e.Action, null, e.NewStartingIndex);
                    break;
                
                //default:
                //    break;
            }

        }

        #region IList<T> Implementation

        public int IndexOf(TViewModel item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, TViewModel item)
        {
            _model.Insert(index, (TModel)item.GetModel());
        }

        public void RemoveAt(int index)
        {
            Remove(_list[index]);
        }

        public TViewModel this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
            }
        }

        public void Add(TViewModel item)
        {
            //note that _list is not changed directly
            _model.Add((TModel)item.GetModel());
        }

        public void Clear()
        {
            _model.Clear();
        }

        public bool Contains(TViewModel item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(TViewModel[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TViewModel item)
        {
            //note that _list is not changed directly
            return _model.Remove((TModel)item.GetModel());
        }

        public IEnumerator<TViewModel> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (_list as System.Collections.IEnumerable).GetEnumerator();
        }

        #endregion //IList<T> Implementation

        #region INotifyCollectionChanged Implementation

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                /* Note that there exist several ctors for NotifyCollectionChangedEventArgs
                 * and it may be required to call one of the other, more complex ctors
                 * for the change to take effect on all UI elements */
                var e = new NotifyCollectionChangedEventArgs(action, item, index);
                handler(this, e);
            }
        }

        #endregion //INotifyCollectionChanged Implementation
    }

    public interface IModelWrapper
    {
        object GetModel();
    }
}
