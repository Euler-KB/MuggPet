using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MuggPet.Binding;
using System.Collections.Specialized;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace MuggPet.Utils.Adapter
{
    /// <summary>
    /// Indicates a sorting order
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// Orders items by ascending mode
        /// </summary>
        Ascending,

        /// <summary>
        /// Order items in descending mode
        /// </summary>
        Descending
    }

    public class SortDescription
    {
        private MemberInfo _memberInfo;
        internal MemberInfo MemberInfo => _memberInfo;

        internal void UpdateMemberInfo(Type source)
        {
            _memberInfo = source.GetMember(Property).FirstOrDefault(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property);
        }

        internal object GetPropertyValue(object source)
        {
            switch (_memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)_memberInfo).GetValue(source);
                case MemberTypes.Property:
                    return ((PropertyInfo)_memberInfo).GetValue(source);
            }

            return null;
        }

        /// <summary>
        /// The property or field of each item to use in sorting. If null, the entire object is used.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// The mode of sorting items
        /// </summary>
        public SortOrder Mode { get; set; }
    }

    /// <summary>
    /// A generic adapter with filtering, sorting, an out of the box model binding to views and support for INotifyCollectionChanged data sources
    /// </summary>
    /// <typeparam name="T">The entity model type</typeparam>
    public class GenericAdapter<T> : BaseAdapter<T>, ISupportBinding
    {
        //  An object bind map
        private IDictionary<object, View> _objectBindMap = new Dictionary<object, View>();

        //  Holds the sort description for the adapater
        private IEnumerable<SortDescription> _sortDescriptions = null;

        /// <summary>
        /// Compares two object with a given sort description
        /// </summary>
        protected virtual int InternalCompare(SortDescription desc, object left, object right)
        {
            int cmpValue = 0;
            IComparable cmpLeft = null;
            IComparable cmpRight = null;

            if (desc.Property == null)
            {
                cmpLeft = (IComparable)left;
                cmpRight = (IComparable)right;
            }
            else
            {
                cmpLeft = (IComparable)desc.GetPropertyValue(left);
                cmpRight = (IComparable)desc.GetPropertyValue(right);
            }

            //
            if (cmpLeft != null && cmpRight != null)
                cmpValue = cmpLeft.CompareTo(cmpRight);
            else if (cmpLeft != null)
                cmpValue = cmpLeft.CompareTo(cmpRight);
            else if (cmpRight != null)
                cmpValue = cmpRight.CompareTo(cmpLeft);

            //
            return desc.Mode == SortOrder.Ascending ? -cmpValue : cmpValue;
        }

        /// <summary>
        /// Gets or sets the sort description. Setting this property always affects the sort comparer in use and automatically enable sorting if disabled
        /// </summary>
        public IEnumerable<SortDescription> SortDescriptions
        {
            get { return _sortDescriptions; }
            set
            {
                if (_sortDescriptions != value)
                {
                    _sortDescriptions = value;
                    if (_sortDescriptions == null || _sortDescriptions.Count() == 0)
                    {
                        //  nullify comparer to use deafult object comparer
                        SortComparer = null;
                    }
                    else
                    {
                        if (!enableSorting)
                            enableSorting = true;

                        //
                        foreach (var desc in _sortDescriptions)
                            desc.UpdateMemberInfo(typeof(T));

                        OnApplySortDescriptions();
                    }
                }
            }
        }

        protected virtual void OnApplySortDescriptions()
        {
            //  set comparer
            _sortComparer = Comparer<T>.Create(new Comparison<T>((T left, T right) =>
                                _sortDescriptions.Sum(x => InternalCompare(x, left, right))));

            //  sort afterwards
            InternalSort();
        }

        private class ChangesListener : Android.Database.DataSetObserver
        {
            private GenericAdapter<T> adapter;
            public ChangesListener(GenericAdapter<T> adapter)
            {
                this.adapter = adapter;
            }

            public override void OnChanged()
            {
                adapter._dataSetChanged?.Invoke(adapter, EventArgs.Empty);
            }

            public override void OnInvalidated()
            {
                adapter._dataSetInvalidated?.Invoke(adapter, EventArgs.Empty);
            }
        }

        //  The observer
        ChangesListener _listener;

        /// <summary>
        /// Initializes data set changes listener
        /// </summary>
        private void InitializeObserver()
        {
            if (_listener == null)
            {
                _listener = new ChangesListener(this);
                RegisterDataSetObserver(_listener);
            }
        }

        /// <summary>
        /// Unregisters data set changes listener
        /// </summary>
        private void DestroyObserver()
        {
            if (_listener != null)
                UnregisterDataSetObserver(_listener);
        }

        private event EventHandler _dataSetInvalidated;

        /// <summary>
        /// Invoked when data set invalidation occurs
        /// </summary>
        public event EventHandler Invalidated
        {
            add
            {
                _dataSetInvalidated += value;

                if (_listener == null)
                    InitializeObserver();
            }

            remove
            {
                _dataSetInvalidated -= value;
                if (_dataSetChanged == null && _dataSetInvalidated == null)
                    DestroyObserver();
            }
        }

        private event EventHandler _dataSetChanged;

        /// <summary>
        /// Invoked when data set changes occur
        /// </summary>
        public event EventHandler Changed
        {
            add
            {
                _dataSetChanged += value;

                if (_listener == null)
                    InitializeObserver();
            }

            remove
            {
                _dataSetChanged -= value;
                if (_dataSetChanged == null && _dataSetInvalidated == null)
                    DestroyObserver();
            }
        }

        private Context context;

        public Context Context => context;

        private Func<int, View, ViewGroup, View> onGetView;

        private IEnumerable<T> source;

        //
        private List<T> items;

        //  Enable sorting
        private bool enableSorting;

        /// <summary>
        /// Enables live sorting on the adapter
        /// </summary>
        public bool EnableSorting
        {
            get { return enableSorting; }
            set
            {
                if (enableSorting != value)
                {
                    if (!value && _sortDescriptions != null)
                    {
                        throw new Exception("Cannot disable sorting when sort descriptions are active. Please nullify sort descriptions before attempting to disable sorting");
                    }

                    enableSorting = value;
                    if (enableSorting)
                    {
                        InternalSort();
                    }
                    else
                    {
                        //  reset items
                        ResetItems();

                        //  apply filtering if enabled
                        InternalFilter();
                    }
                }
            }
        }

        //  The internal filtering value
        private bool enableFiltering;

        /// <summary>
        /// Enables live filtering on the adapter
        /// </summary>
        public bool EnableFiltering
        {
            get { return enableFiltering; }
            set
            {
                if (enableFiltering != value)
                {
                    enableFiltering = value;
                    if (enableFiltering)
                    {
                        InternalFilter();
                    }
                    else
                    {
                        //  reset items
                        ResetItems();

                        //  apply sorting if enabled
                        InternalSort();
                    }
                }
            }
        }

        //
        public delegate bool FilterDelegate(T item);

        //  The internal filter delegate
        private FilterDelegate _filter;

        /// <summary>
        /// Called to filter each item within the adapter
        /// </summary>
        public event FilterDelegate Filter
        {
            add
            {
                //  auto enable filtering
                if (_filter == null && !EnableFiltering)
                    enableFiltering = true;

                //
                _filter += value;
                InternalFilter();
            }

            remove
            {
                _filter -= value;
                InternalFilter();
            }
        }

        //  The sorting comparer
        private Comparer<T> _sortComparer = Comparer<T>.Default;

        /// <summary>
        /// Enables live sorting on the adapter
        /// </summary>
        public Comparer<T> SortComparer
        {
            get { return _sortComparer; }
            set
            {
                if (_sortComparer != value)
                {
                    //
                    if (_sortDescriptions != null)
                    {
                        throw new Exception("Cannot set comaparer whiles sort descriptions are active. Ensure sort descriptions are nullified before attempting to use this operation!");
                    }

                    //  auto enable sorting
                    if (_sortComparer == null && !EnableSorting && _sortComparer != null)
                        enableSorting = true;

                    //
                    _sortComparer = value ?? Comparer<T>.Default;
                    InternalSort();
                }
            }
        }

        /// <summary>
        /// Refreshes the view by updating current source
        /// </summary>
        public void Refresh()
        {
            //  reset items
            ResetItems();

            //  filter first
            InternalFilter();

            //  sort afterwards to reduce overall time since filtered collection will contain less data
            InternalSort();

            //  reflect changes
            NotifyDataSetChanged();
        }

        /// <summary>
        /// Resets the internal collection to match the given data source
        /// </summary>
        protected virtual void ResetItems()
        {
            items = (source == null) ? new List<T>() : new List<T>(source);
        }

        /// <summary>
        /// Filters an item, passing if no filter handler is registered
        /// </summary>
        /// <param name="obj">The object to be filtered</param>
        /// <returns>True if passed filter else otherwise</returns>
        protected virtual bool InternalFilter(T obj)
        {
            return _filter == null ? true : _filter(obj);
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (T item in e.NewItems)
                        {
                            if (!EnableFiltering || (EnableFiltering && InternalFilter(item)))
                            {
                                items.Add(item);
                                OnItemAdded(item);
                            }
                        }

                        InternalSort();
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        //  we prevent moving indices when sorting is enabled
                        if (!EnableSorting)
                        {
                            var temp = items[e.NewStartingIndex];
                            items[e.NewStartingIndex] = items[e.OldStartingIndex];
                            items[e.OldStartingIndex] = temp;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var item in e.OldItems)
                        {
                            items.Remove((T)item);
                            OnItemRemoved((T)item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            //
                            OnItemRemoved(items[i]);

                            T item = (T)e.NewItems[i];
                            if (EnableFiltering && !InternalFilter(item))
                                items.RemoveAt(i);
                            else
                                items[i] = item;
                        }

                        InternalSort();
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        OnItemsReset();
                        items.Clear();
                    }
                    break;
            }

            //  reflect changes
            NotifyDataSetChanged();
        }

        /// <summary>
        /// Initializes a new adapter with your items source and a custom callback to load the view for an item.
        /// Views loaded are automatically binded to corresponding items
        /// </summary>
        /// <param name="context">The context object. Used in inflating views from resource</param>
        /// <param name="items">A collection of items within the adapter. If your items implement INotifyPropertyChanged, changes on the items are automatically synchronized withiin the adapter</param>
        /// <param name="getViewFunc">A callback to fetch the view for each item. Items are then automatically binded to loaded views</param>
        public GenericAdapter(Context context, IEnumerable<T> items, Func<int, View, ViewGroup, View> getViewFunc)
        {
            this.onGetView = getViewFunc;
            this.context = context;
            Source = items;
        }

        /// <summary>
        /// Initializes a new adapter with a custom callback to load the view for an item. 
        /// The adapter initially contains no items. You can use the Source property to assign one
        /// </summary>
        /// <param name="context">The context object. Used in inflating views from resource</param>
        /// <param name="getViewFunc">A callback to fetch the view for each item. Items are then automatically binded to loaded views</param>
        public GenericAdapter(Context context, Func<int, View, ViewGroup, View> getViewFunc) : this(context, null, getViewFunc)
        {

        }

        /// <summary>
        /// Initializes a new empty instance with the specified item layout and a custom callback for binding items to views. 
        /// The adapter initially contains no items. You can use the Source property to assign one
        /// </summary>
        /// <param name="context">The context object. Used in inflating views from resource</param>
        /// <param name="itemLayoutResID">The layout for each item within the view</param>
        /// <param name="onBind">A callback for specifying binding each item to items view. If null, will automatically bind the item to the view else you'll have to implement you own way of mapping items to views.</param>
        public GenericAdapter(Context context, int itemLayoutResID, Action<View, T> onBind = null) : this(context, itemLayoutResID, null, onBind)
        {

        }

        /// <summary>
        /// Initializes a new empty instance with custom callback for determining each item's layout and a custom callback for binding items to views. 
        /// </summary>
        /// <param name="context">The context object. Used in inflating views from resource</param>
        /// <param name="itemLayoutResID">The layout for each item within the view</param>
        /// /// <param name="items">A collection of items within the adapter. If your items implement INotifyPropertyChanged, changes on the items are automatically synchronized withiin the adapter</param>
        /// <param name="onBind">A callback for specifying binding each item to items view. If null, will automatically bind the item to the view else you'll have to implement you own way of mapping items to views.</param>
        public GenericAdapter(Context context, int itemLayoutResID, IEnumerable<T> items, Action<View, T> onBind = null) : this(context, (x) => itemLayoutResID, items, onBind)
        {

        }


        /// <summary>
        /// Initializes a new empty instance with custom callback for determining each item's layout and a custom callback for binding items to views. 
        /// The adapter initially contains no items. You can use the Source property to assign one
        /// </summary>
        /// <param name="context">The context object. Used in inflating views from resource</param>
        /// <param name="onGetItemLayout">A custom callback for determining the layout for an item</param>
        /// <param name="onBind">A callback for specifying binding each item to items view. If null, will automatically bind the item to the view else you'll have to implement you own way of mapping items to views.</param>
        public GenericAdapter(Context context, Func<T, int> onGetItemLayout, Action<View, T> onBind = null) : this(context, onGetItemLayout, null, onBind)
        {

        }

        /// <summary>
        /// Initializes a new adapter with specified items
        /// </summary>
        /// <param name="context">The context object for inflating views</param>
        /// <param name="items">A collection of items within the adapter. If your items implement INotifyPropertyChanged, changes on the items are automatically synchronized withiin the adapter</param>
        public static GenericAdapter<string> Create(Context context, IEnumerable<string> items)
        {
            return new GenericAdapter<string>(context, Android.Resource.Layout.SimpleListItem1, items, (v, str) => v.FindViewById<TextView>(Android.Resource.Id.Text1).Text = str);
        }

        /// <summary>
        /// Initializes a new adapter with items(string-array) from resouce
        /// </summary>
        /// <param name="context">The context object for fetching resouces and inflating views</param>
        /// <param name="itemsResId">The id of the string array resource</param>
        public static GenericAdapter<string> Create(Context context, int itemsResId)
        {
            return Create(context, context.Resources.GetStringArray(itemsResId));
        }

        /// <summary>
        /// Initializes a new instance with custom callback for determining each item's layout, your items data source and a custom callback for binding items to views. 
        /// </summary>
        /// <param name="context">The context object. Used in inflating views from resource</param>
        /// <param name="onGetItemLayout">A custom callback for determining the layout for an item</param>
        /// <param name="items">A collection of items within the adapter. If your items implement INotifyPropertyChanged, changes on the items are automatically synchronized withiin the adapter</param>
        /// <param name="onBind">A callback for specifying binding each item to items view. If null, will automatically bind the item to the view else you'll have to implement you own way of mapping items to views.</param>
        public GenericAdapter(Context context, Func<T, int> onGetItemLayout, IEnumerable<T> items, Action<View, T> onBind = null)
        {
            this.context = context;
            Source = items;
            this.onGetView = (int pos, View cView, ViewGroup parent) =>
            {
                var item = this.items[pos];
                if (cView == null)
                    cView = LayoutInflater.FromContext(Context).Inflate(onGetItemLayout(item), parent, false);

                if (onBind != null)
                {
                    //  This is provided for types that do not support direct binding with the binding framework
                    onBind(cView, item);
                }
                else
                {
                    OnBind(item, cView);
                }

                return cView;
            };
        }

        /// <summary>
        /// Notifies the adapter of an explicit binding between the specified item and the view. If there's an existing binding, it is updated
        /// </summary>
        /// <param name="item">The item within with adapter that is beign bound</param>
        /// <param name="view">The target view</param>
        public void OnBind(T item, View view)
        {
            //  Destroy existing bindings for this item
            BindingHandler.Destroy(item);

            //  Bind object to view
            BindingExtensions.BindObjectToView(this, item, view);

            //  Update or add reference to view binding
            _objectBindMap[item] = view;
        }

        protected virtual void OnItemAdded(T item)
        {
            var notif = item as INotifyPropertyChanged;
            if (notif != null)
                notif.PropertyChanged += OnItemPropertyChanged;
        }

        protected virtual void OnItemRemoved(T item)
        {
            BindingHandler.Destroy(item);
            _objectBindMap.Remove(item);

            //
            var notif = item as INotifyPropertyChanged;
            if (notif != null)
                notif.PropertyChanged -= OnItemPropertyChanged;
        }


        protected virtual void OnItemsReset()
        {
            BindingHandler.Reset();
            _objectBindMap.Clear();

            //
            if (source == null)
                return;

            foreach (var item in source)
            {
                if (item is INotifyPropertyChanged)
                    ((INotifyPropertyChanged)item).PropertyChanged -= OnItemPropertyChanged;
            }
        }


        /// <summary>
        /// Sets or gets the underlying data source for the adapter
        /// </summary>
        public IEnumerable<T> Source
        {
            get { return source; }

            set
            {
                if (source != value)
                {
                    if (source is INotifyCollectionChanged)
                        ((INotifyCollectionChanged)source).CollectionChanged -= OnSourceCollectionChanged;

                    //  force reset
                    OnItemsReset();

                    //  set new source
                    source = value;

                    //
                    if (source != null)
                    {
                        if (source is INotifyCollectionChanged)
                            ((INotifyCollectionChanged)source).CollectionChanged += OnSourceCollectionChanged;

                        //  listen for changes in item properties
                        foreach (var item in source)
                            OnItemAdded(item);
                    }

                    //
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Invoked when a property of an item within the collection has changed
        /// </summary>
        protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var type = sender.GetType();
            var member = type.GetMember(e.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property);
            if (member != null)
            {
                if (member.GetCustomAttributes().OfType<IBindingAttribute>().Any())
                {
                    View bindedView;
                    if (_objectBindMap.TryGetValue(sender, out bindedView))
                    {
                        BindingHandler.BindObjectToView(sender, member, bindedView, true);
                    }
                }
            }
        }

        public override T this[int position] => items[position];

        public override int Count => items.Count;

        private IBindingHandler bindingHandler;
        public IBindingHandler BindingHandler
        {
            get
            {
                return bindingHandler ?? (bindingHandler = new BindingHandler());
            }
        }

        public override long GetItemId(int position) => position;

        public override View GetView(int position, View convertView, ViewGroup parent) => onGetView(position, convertView, parent);

        /// <summary>
        /// Checks whether sorting is enabled on the adapter then sorts the internal collection
        /// </summary>
        protected virtual void InternalSort()
        {
            if (EnableSorting)
            {
                items.Sort(_sortComparer);
            }
        }

        /// <summary>
        /// Checks whether filtering is enabled on the adapter then filters the internal collection
        /// </summary>
        protected virtual void InternalFilter()
        {
            if (EnableFiltering)
            {
                //  we filter when we have a valid filtering subscriber
                if (_filter != null)
                {
                    items.RemoveAll(x => !InternalFilter(x));
                }
            }
        }
    }


}