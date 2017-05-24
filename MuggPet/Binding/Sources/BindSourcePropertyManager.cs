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

namespace MuggPet.Binding.Sources
{
    public static class BindSourcePropertyManager
    {
        //  Keeps a global collection of binding properties for views
        static IDictionary<Type, HashSet<BindProperty>> _bindMap;

        static BindSourcePropertyManager()
        {
            _bindMap = new Dictionary<Type, HashSet<BindProperty>>();
        }

        static void ValidateViewType(Type type)
        {
            if (type.IsSubclassOf(typeof(View)) || type == typeof(View))
                throw new BindingException("The specified type is not a view or its descendant.");
        }

        public static BindProperty Register(string name, Type viewType, Func<View, object> getValue)
        {
            //  ensure supplied type is a view
            ValidateViewType(viewType);

            //
            HashSet<BindProperty> frameSet = null;
            if (!_bindMap.TryGetValue(viewType, out frameSet))
            {
                frameSet = new HashSet<BindProperty>();
                _bindMap[viewType] = frameSet;
            }

            //
            var property = new BindProperty(viewType, name, getValue);
            if (frameSet.Add(property))
                return property;

            return null;
        }

    }
}