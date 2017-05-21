using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MuggPet.Views;
using MuggPet.Utils;
using MuggPet.Commands;

namespace MuggPet.Binding
{
    /// <summary>
    /// Manages the binding of views and mapping of objects to views
    /// </summary>
    public static class BindingExtensions
    {
        public static void AttachViews(this ISupportBinding bindInterface , View rootView = null)
        {
            if (rootView == null)
            {
                if (bindInterface is Activity)
                    rootView = ((Activity)bindInterface).FindViewById(Android.Resource.Id.Content);
                else if (bindInterface is Fragment)
                    rootView = ((Fragment)bindInterface).View;
                else if (bindInterface is Android.Support.V4.App.Fragment)
                    rootView = ((Android.Support.V4.App.Fragment)bindInterface).View;
            }

            BindingManager.AttachViews(bindInterface.BindingHandler, rootView, bindInterface);
        }

        public static void BindViewContent(this ISupportBinding bindInterface, View sourceView, object destinationObject)
        {
            BindingManager.BindViewContent(bindInterface.BindingHandler, sourceView, destinationObject);
        }

        public static void BindObjectToView(this ISupportBinding bindInterface, object sourceObject, View destinationView)
        {
            BindingManager.BindObjectToView(bindInterface.BindingHandler, sourceObject, destinationView);
        }

        public static void BindResources(this ISupportBinding bindInterface, Context context, object destinationObject)
        {
            BindingManager.BindResources(bindInterface.BindingHandler, destinationObject, context);
        }

        public static void BindCommands(this ISupportBinding bindInterface, object sourceObject, View destinationView)
        {
            BindingManager.BindCommands(bindInterface.BindingHandler, sourceObject, destinationView);
        }
    }
}