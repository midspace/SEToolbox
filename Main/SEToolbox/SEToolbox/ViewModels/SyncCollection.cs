//namespace SEToolbox.ViewModels
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Collections.ObjectModel;
//    using System.Collections.Specialized;
//    using System.Linq;
//    using System.Text;

//    public class SyncCollection<VM, M> : ObservableCollection<VM>
//    {
//        private ObservableCollection<M> modelCollection;

//        #region ctor

//        public SyncCollection(ObservableCollection<M> collection)
//            : base()
//        {
//            modelCollection = collection;
//            modelCollection.CollectionChanged += ModelCollectionChanged;
//        }

     
//        #endregion

//        // ViewModel collection changed.
//        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
//        {
//            base.OnCollectionChanged(e);

//            base.ispop
//        }


//        // Model collection changed.
//        void ModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
//        {
//            var collection = sender as ObservableCollection<M>;

//            switch (e.Action)
//            {
//                case NotifyCollectionChangedAction.Add:
//                    apple = e.NewItems[0] as Apple;
//                    if (apple != null)
//                        AddViewModel(asset);
//                    break;
//                case NotifyCollectionChangedAction.Remove:
//                    apple = e.OldItems[0] as Apple;
//                    if (apple != null)
//                        RemoveViewModel(apple);
//                    break;
//            }
//        }


//        void OnApplesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
//        {
//            // Only add/remove items if already populated. 
//            if (!IsPopulated)
//                return;

//            Apple apple;

//            switch (e.Action)
//            {
//                case NotifyCollectionChangedAction.Add:
//                    apple = e.NewItems[0] as Apple;
//                    if (apple != null)
//                        AddViewModel(asset);
//                    break;
//                case NotifyCollectionChangedAction.Remove:
//                    apple = e.OldItems[0] as Apple;
//                    if (apple != null)
//                        RemoveViewModel(apple);
//                    break;
//            }

//        }
//    }
//}
