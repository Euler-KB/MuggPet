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
using System.Reflection;

namespace MuggPet.Binding
{
    /// <summary>
    /// The core manager for binding reflection and related work
    /// </summary>
    public static class BindingManager
    {
        //  The default binding flags for fetching properties and fields
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        //  The default member types supported through binding
        private static MemberTypes[] DefaultMemberTypes = new MemberTypes[] { MemberTypes.Property, MemberTypes.Field };

        static IEnumerable<MemberInfo> GetMembers(Type type) => type.GetMembers(DefaultBindingFlags).Where(x => DefaultMemberTypes.Contains(x.MemberType));

        /// <summary>
        /// Initiates a binding on the source object to the target view
        /// </summary>
        /// <param name="handler">The binding handler in scope</param>
        /// <param name="source">An instance of an object whose values are to be bound to the destination view</param>
        /// <param name="targetView">The destination view </param>
        /// <param name="flags">Addition flags for adjusting binding operation behaviour</param>
        public static void BindObjectToView(IBindingHandler handler, object source, View targetView, BindFlags flags = BindFlags.None)
        {
            foreach (var member in GetMembers(source.GetType()))
            {
                //  bind resources first
                if (!flags.HasFlag(BindFlags.NoResource))
                {
                    handler.BindResource(targetView.Context, source, member, false);
                }

                //  bind bindable attributes
                handler.BindObjectToView(source, member, targetView, false);

                //  bind commands
                if (!flags.HasFlag(BindFlags.NoCommand))
                {
                    //  pass the object to bind as the parameter for the command
                    handler.BindCommand(source, member, targetView, source, false);
                }
            }
        }

        /// <summary>
        /// Initiates a view attachment binding with the root view to the specified target object
        /// </summary>
        /// <param name="handler">The binding handler in scope</param>
        /// <param name="rootView">The root view for finding sub view for attachment</param>
        /// <param name="target">An instance of an object which receives the attachment of views</param>
        /// <param name="flags">Addition flags for adjusting binding operation behaviour</param>
        public static void AttachViews(IBindingHandler handler, View rootView, object target, BindFlags flags = BindFlags.None)
        {
            if (rootView == null)
                throw new ArgumentNullException("rootView", "The root view is used for resolving child views upon binding");

            if (target == null)
                throw new ArgumentNullException("target", "The target object is required for updating attachments u");

            foreach (var member in GetMembers(target.GetType()))
            {
                //  bind resources first
                if (!flags.HasFlag(BindFlags.NoResource))
                {
                    handler.BindResource(rootView.Context, target, member, false);
                }

                //  attach view
                handler.AttachViewToProperty(rootView, target, member, flags, false);

                //  bind commands
                if (!flags.HasFlag(BindFlags.NoCommand))
                {
                    //  pass the object to bind as the parameter for the command
                    handler.BindCommand(target, member, rootView, target, false);
                }
            }
        }

        public static void BindViewContent(IBindingHandler handler, View view, object obj)
        {
            foreach (var member in GetMembers(obj.GetType()))
            {
                handler.BindViewContent(view, obj, member, false);
            }
        }

        public static void BindResources(IBindingHandler handler, object target, Context context)
        {
            foreach (var member in GetMembers(target.GetType()))
            {
                handler.BindResource(context, target, member, false);
            }
        }

        public static void BindCommands(IBindingHandler handler, object source, View view)
        {
            foreach (var member in GetMembers(source.GetType()))
            {
                handler.BindCommand(source, member, view, source, false);
            }
        }

    }
}