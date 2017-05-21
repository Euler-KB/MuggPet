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
    /// Implements resource caching for various sub binding operations
    /// </summary>
    public class BindingResourceCache : IBindingResourceCache
    {
        IDictionary<int, object> _resourceCache;
        IDictionary<View, IList<View>> _viewCache;

        public BindingResourceCache()
        {
            //  initialize
            _resourceCache = new Dictionary<int, object>();
            _viewCache = new Dictionary<View, IList<View>>();
        }

        public View GetView(View rootView, int subViewId)
        {
            return null;
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

        }

        public void RemoveResource(int resourceId)
        {
            _resourceCache.Remove(resourceId);
        }

        public void RemoveView(View rootView)
        {
            _viewCache.Remove(rootView);
        }
    }
}