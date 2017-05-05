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
    /// The base interface for resource cache manager for a binding handler
    /// </summary>
    public interface IBindingResourceCache
    {
        /// <summary>
        /// Puts the resource in store
        /// </summary>
        /// <param name="key">The key for the identifying the resource</param>
        /// <param name="resource">The resource object</param>
        /// <param name="tag">The tag for the resource</param>
        bool Put(object key, object resource, object tag);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Destroy(int id);


        bool Destroy(int id, object tag);

        object GetResource(int id);

        IEnumerable<object> GetResources(object tag);

        bool HasResource(int id);

        void Clear();

    }
}