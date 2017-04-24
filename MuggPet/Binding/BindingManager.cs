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
    /// Represents a binding exception
    /// </summary>
    [Serializable]
    public class BindingException : Exception
    {
        public BindingException() { }

        public BindingException(string message) : base(message) { }
    }

    /// <summary>
    /// Manages the binding of views and mapping of objects to views
    /// </summary>
    public static class BindingManager
    {
        //  The default binding flags for fetching properties and fields
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        //  The default member types supported through binding
        private static MemberTypes[] DefaultMemberTypes = new MemberTypes[] { MemberTypes.Property, MemberTypes.Field };

        /// <summary>
        /// Binds the values from the properties or fields defined in the specified object to the target view
        /// </summary>
        /// <param name="obj">The object containing the properties</param>
        /// <param name="activity">The target activity to receive the values from the object</param>
        public static void BindObjectToView(this object obj, Android.App.Activity activity)
        {
            BindObjectToView(obj, activity.FindViewById(Android.Resource.Id.Content));
        }

        /// <summary>
        /// Binds the values from the properties or fields defined in the specified object to the target view
        /// </summary>
        /// <param name="obj">The object containing the properties</param>
        /// <param name="view">The view to receive the values from the object</param>
        public static void BindObjectToView(this object obj, View view)
        {
            var type = obj.GetType();
            foreach (var member in type.GetMembers(DefaultBindingFlags).Where(x => DefaultMemberTypes.Contains(x.MemberType)))
            {
                InternalBindObject(obj, view, member);
            }
        }

        static void InternalBindObject(object obj, View view, MemberInfo member)
        {
            //  apply all binding attributes
            foreach (var bindAttrib in member.GetCustomAttributes().OfType<IBindingAttribute>())
            {
                //  get target view
                var targetView = view.FindViewById(bindAttrib.ID);
                if (targetView == null)
                    continue;

                if (member.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)member;
                    var fieldValue = field.GetValue(obj);
                    bindAttrib.OnBindPropertyToView(targetView, fieldValue, field.FieldType, member);
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    var property = (PropertyInfo)member;
                    var propertyValue = property.GetValue(obj);
                    bindAttrib.OnBindPropertyToView(targetView, propertyValue, property.PropertyType, property);
                }
            }
        }

        /// <summary>
        /// Attaches views from the source view to the target object
        /// </summary>
        /// <param name="view">The view to find sub views from</param>
        /// <param name="obj">The object containing</param>
        /// <param name="flags">Additional flags which determines how views are binded</param>
        public static void AttachViews(this View view, object obj, BindFlags flags = BindFlags.None)
        {
            var type = obj.GetType();
            foreach (var member in type.GetMembers(DefaultBindingFlags).Where(x => DefaultMemberTypes.Contains(x.MemberType)))
            {
                InternalAttachView(view, obj, member, flags);

                //  load resource on the fly
                if (!flags.HasFlag(BindFlags.NoResource))
                {
                    InternalLoadResource(obj, view.Context, member);
                }

                if (!flags.HasFlag(BindFlags.NoObjectBinding))
                {
                    InternalBindObject(obj, view, member);
                }
            }
        }

        static void InternalAttachView(View view, object obj, MemberInfo member, BindFlags flags)
        {
            //  apply all binding attribute on property or field
            foreach (var bindAttrib in member.GetCustomAttributes().OfType<IBindingAttribute>())
            {
                View targetView = view.FindViewById(bindAttrib.ID);
                if (targetView == null)
                    continue;

                //  get member type
                Type memberType = null;
                if (member.MemberType == MemberTypes.Field)
                    memberType = ((FieldInfo)member).FieldType;
                if (member.MemberType == MemberTypes.Property)
                    memberType = ((PropertyInfo)member).PropertyType;

                //
                if (!bindAttrib.CanAttachView(view, member, memberType))
                    continue;

                if (flags.HasFlag(BindFlags.GenerateViewID))
                    targetView.Id = ViewHelper.NewId;

                if (member.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)member;
                    field.SetValue(obj, targetView);
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    var property = (PropertyInfo)member;
                    property.SetValue(obj, targetView);
                }
            }
        }

        #region Extensions

        /// <summary>
        /// Binds resources from the given context to the destination object
        /// </summary>
        /// <param name="target">The destination object</param>
        /// <param name="context">The context to resolve resources from</param>
        public static void BindResources(this object target, Context context)
        {
            var type = target.GetType();
            foreach (var member in type.GetMembers(DefaultBindingFlags).Where(x => DefaultMemberTypes.Contains(x.MemberType)))
            {
                //  load resource internally
                InternalLoadResource(target, context, member);
            }
        }

        static void InternalLoadResource(object target, Context context, MemberInfo member)
        {
            foreach (var resourceAttrib in member.GetCustomAttributes().OfType<IResourceAttribute>())
            {
                if (member.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)member;
                    field.SetValue(target, resourceAttrib.LoadResource(context, field.FieldType));
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    var property = (PropertyInfo)member;
                    property.SetValue(target, resourceAttrib.LoadResource(context, property.PropertyType));
                }
            }
        }

        /// <summary>
        /// Binds or maps the values within all sub views from the source activity to the target object
        /// </summary>
        /// <param name="activity">The target activity</param>
        /// <param name="obj">The object to receive the values from the activity</param>
        public static void BindValues(this Android.App.Activity activity, object obj)
        {
            BindValues(activity.FindViewById(Android.Resource.Id.Content), obj);
        }

        /// <summary>
        /// Returns the values within the various views to the destination object
        /// </summary>
        /// <param name="view">The view with the values</param>
        /// <param name="obj">The target object where values from the view is placed</param>
        public static void BindValues(this View view, object obj)
        {
            var type = obj.GetType();
            foreach (var member in type.GetMembers(DefaultBindingFlags))
            {
                InternalBindValue(view, obj, member);
            }
        }

        static void InternalBindValue(View view, object obj, MemberInfo member)
        {
            foreach (var bindAttrib in member.GetCustomAttributes().OfType<IBindingAttribute>())
            {
                var targetView = view.FindViewById(bindAttrib.ID);
                if (targetView == null)
                    continue;

                if (member.MemberType == MemberTypes.Field)
                {
                    var field = (FieldInfo)member;
                    field.SetValue(obj, bindAttrib.OnBindViewValueToProperty(view, field.FieldType));
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    var property = (PropertyInfo)member;
                    property.SetValue(obj, bindAttrib.OnBindViewValueToProperty(view, property.PropertyType));
                }
            }
        }

        /// <summary>
        /// Binds the views from the activity's layout to fields and properties with additional flags
        /// </summary>
        /// <param name="activity">The containing activity</param>
        /// <param name="flags">Optional flags which determine how binding is performed</param>
        public static void AttachViewsEx(this Android.App.Activity activity, BindFlags flags = BindFlags.None)
        {
            //  attach views
            AttachViews(activity.FindViewById(Android.Resource.Id.Content), activity, flags);
        }

        /// <summary>
        /// Attaches the views within the activity's hierarchy to the properties and fields within. 
        /// The activity's content is inflated with the layout supplied before attaching views.
        /// </summary>
        /// <param name="activity">The activity to use</param>
        /// <param name="layoutID">The resource id of the layout</param>
        public static void AttachViews(this Android.App.Activity activity, int layoutID)
        {
            //  inflate content first
            activity.SetContentView(layoutID);

            //  attach views
            AttachViewsEx(activity);
        }

        /// <summary>
        /// Attaches the views within the fragment's hierarchy to the properties and fields within. 
        /// </summary>
        /// <param name="activity">The activity to use</param>
        /// <param name="layoutID">The resource id of the layout</param>
        public static void AttachViews(this Fragment fragment)
        {
            //  attach views
            AttachViews(fragment.View, fragment);
        }

        /// <summary>
        /// Attaches the views within the fragment's hierarchy to the properties and fields within. 
        /// </summary>
        /// <param name="activity">The activity to use</param>
        /// <param name="layoutID">The resource id of the layout</param>
        public static void AttachViews(this Android.Support.V4.App.Fragment fragment)
        {
            //  attach views
            AttachViews(fragment.View, fragment);
        }

        #endregion

    }
}