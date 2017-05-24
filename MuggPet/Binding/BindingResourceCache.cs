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

namespace MuggPet.Binding
{
    /// <summary>
    /// Implements resource caching for binding operations
    /// </summary>
    public class BindingResourceCache : IBindingResourceCache
    {
        //  
        private IDictionary<int, object> _resourceCache;
        private IDictionary<View, HashSet<View>> _viewCache;

        private HashSet<View> GetViewSet(View container)
        {
            HashSet<View> set;
            if (!_viewCache.TryGetValue(container, out set))
                _viewCache[container] = (set = new HashSet<View>());

            return set;
        }

        /// <summary>
        /// Initializes a new empty binding resource cache
        /// </summary>
        public BindingResourceCache()
        {
            _resourceCache = new Dictionary<int, object>();
            _viewCache = new Dictionary<View, HashSet<View>>();
        }

        public View GetView(View rootView, int subViewId)
        {
            //  get set
            var set = GetViewSet(rootView);

            //  get existing view
            var view = set.FirstOrDefault(x => x.Id == subViewId);
            if (view != null)
                return view;

            //  find sub view
            view = BindingUtils.FindView(rootView, subViewId);
            if (view != null)
                set.Add(view);

            //  
            return view;
        }

        public void PutResource(int resourceId, object value)
        {
            _resourceCache[resourceId] = value;
        }

        public View PutView(View rootView, int subViewId)
        {
            View subView = rootView.FindViewById(subViewId);
            PutView(rootView, subView);
            return subView;
        }

        public void PutView(View rootView, View subView)
        {
            var set = GetViewSet(rootView);
            if (!set.Any(x => x.Id == subView.Id))
                set.Add(subView);
        }

        public void RemoveResource(int resourceId)
        {
            _resourceCache.Remove(resourceId);
        }

        public void RemoveView(View rootView)
        {
            _viewCache.Remove(rootView);
        }

        public void Reset(ResetOptionFlags flags = 0)
        {
            if (flags.HasFlag(ResetOptionFlags.View))
                _viewCache.Clear();

            if (flags.HasFlag(ResetOptionFlags.Resources))
                _resourceCache.Clear();
        }

        public object GetResource(int resourceId)
        {
            object resource;
            if(_resourceCache.TryGetValue(resourceId,out resource))
                return resource;

            return null;
        }
    }
}