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
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        //  The default member types supported through binding
        private static MemberTypes[] DefaultMemberTypes = new MemberTypes[] { MemberTypes.Property, MemberTypes.Field };

        static IEnumerable<MemberInfo> GetMembers(Type type) => type.GetMembers(DefaultBindingFlags).Where(x => DefaultMemberTypes.Contains(x.MemberType));

        public static void BindObjectToView(IBindingHandler handler, object obj, View view, BindFlags flags = BindFlags.None)
        {
            foreach (var member in GetMembers(obj.GetType()))
            {
                //  bind bindable attributes
                handler.BindObjectToView(obj, member, view, false);

                //  bind commands
                if (flags.HasFlag(BindFlags.NoCommand))
                {
                    handler.BindCommand(obj, member, view, false);
                }
            }
        }

        public static void AttachViews(IBindingHandler handler, View rootView, object obj, BindFlags flags = BindFlags.None)
        {
            foreach (var member in GetMembers(obj.GetType()))
            {
                //  attach view
                handler.AttachViewToProperty(rootView, obj, member, false);

                //  bind resources
                if (!flags.HasFlag(BindFlags.NoResource))
                {
                    handler.BindResource(rootView.Context, obj, member, false);
                }

                //  bind commands
                if (!flags.HasFlag(BindFlags.NoCommand))
                {
                    handler.BindCommand(obj, member, rootView, false);
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
                handler.BindResource(context, target, member, false);
        }

        public static void BindCommands(IBindingHandler handler, object source, View view)
        {
            foreach (var member in GetMembers(source.GetType()))
                handler.BindCommand(source, member, view, false);
        }

        //  Invokes a complete binding operation and provides better performance for multibinding operations
        public static void Bind(IBindingHandler handler, object source, object target)
        {
            foreach (var member in GetMembers(target.GetType()))
            {

            }
        }
    }
}