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
    /// The base interface for resource cache manager for bindings
    /// </summary>
    public interface IBindingResourceCache
    {
        /// <summary>
        /// Caches a sub view within the root view
        /// </summary>
        /// <param name="rootView">The root view which the sub view is a child of</param>
        /// <param name="subView">A view within the root view</param>
        void PutView(View rootView, View subView);

        /// <summary>
        /// Caches a sub view within the root view. Updates existing cached value is
        /// </summary>
        /// <param name="rootView">The root view which the sub view is a child of</param>
        /// <param name="subViewId">The id of a view within the root view</param>
        View PutView(View rootView, int subViewId);

        /// <summary>
        /// Gets or loads a cached view
        /// </summary>
        /// <param name="rootView">The root view to find the sub view from</param>
        /// <param name="subViewId">The id of the sub view</param>
        View GetView(View rootView , int subViewId);

        /// <summary>
        /// Removes cache for the specified root view
        /// </summary>
        /// <param name="rootView">The view to be removed form cache</param>
        void RemoveView(View rootView);

        /// <summary>
        /// Caches specified resource 
        /// </summary>
        /// <param name="resourceId">The id of the resource</param>
        /// <param name="value">The resource value</param>
        void PutResource(int resourceId, object value);

        /// <summary>
        /// Removes the resource with the specified id from the cache
        /// </summary>
        /// <param name="resourceId">The id of the resource</param>
        void RemoveResource(int resourceId);
    }
}