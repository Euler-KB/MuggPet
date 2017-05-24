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
using MuggPet.Commands;
using MuggPet.Utils;
using MuggPet.Binding.Logging;

namespace MuggPet.Binding
{
    /// <summary>
    /// Represents the mode of a binding operation
    /// </summary>
    public enum BindingMode
    {
        /// <summary>
        /// This mode indicates a view attachment to a property
        /// </summary>
        Attach,

        /// <summary>
        /// A binding of a view's property to an object
        /// </summary>
        ViewContent,

        /// <summary>
        /// Represents a binding of an object to a view
        /// </summary>
        ObjectToView,

        /// <summary>
        /// Indicates a resouce binding
        /// </summary>
        Resource,

        /// <summary>
        /// Indicates a command binding
        /// </summary>
        Command
    }

    /// <summary>
    /// Serves as a link between between source and target object bindings.
    /// </summary>
    public class BindState
    {
        /// <summary>
        /// The binding mode for this state
        /// </summary>
        public BindingMode Mode { get; set; }

        /// <summary>
        /// The source object in the binding
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        /// The member of the source involved in binding
        /// </summary>
        public MemberInfo SourceMember { get; set; }

        /// <summary>
        /// The destination object in binding
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// The member of the target involved in binding
        /// </summary>
        public MemberInfo TargetMember { get; set; }

        /// <summary>
        /// The applied binding attribute. Can be null for command bindings for direct command bindings.
        /// </summary>
        public Attribute Attribute { get; set; }

        /// <summary>
        /// Contains additional/optional data for binding
        /// </summary>
        public object Extras { get; set; }

        /// <summary>
        /// Executes the binding state
        /// </summary>
        public bool Execute(IBindingResourceCache resourceCache = null)
        {
            switch (Mode)
            {
                case BindingMode.Attach:
                    return InternalAttachView();
                case BindingMode.ObjectToView:
                    return InternalBindObjectToView();
                case BindingMode.ViewContent:
                    return InternalBindViewContent();
                case BindingMode.Resource:
                    return InternalBindResource(resourceCache);
                case BindingMode.Command:
                    return InternalBindCommand();
            }

            return false;
        }

        /// <summary>
        /// Reverts executed state. It is to be noted that not all operations are revertible
        /// </summary>
        public bool Revert()
        {
            switch (Mode)
            {
                case BindingMode.Command:
                    InternalUnBindCommand();
                    break;
            }

            return false;
        }

        static void SetMemberValue(MemberInfo member, object target, object value)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo)member;
                field.SetValue(target, value);
            }
            else if (member.MemberType == MemberTypes.Property)
            {
                var property = (PropertyInfo)member;
                property.SetValue(target, value);
            }
        }

        protected virtual bool InternalBindCommand()
        {
            //  check 
            if (Source == null || Target == null || Attribute == null)
            {
                BindingTrace.TraceFail(BindingMode.Command, "Invalid parameters. 'Source', 'Attribute'and 'Target' are required!");
                return false;
            }

            ICommandBinding bindCommand = (ICommandBinding)Attribute;
            if(bindCommand == null)
            {

            }

            return bindCommand.OnBind((ICommand)Source, (View)Target, Extras);
        }

        protected virtual void InternalUnBindCommand()
        {
            ICommandBinding bindCommand = Attribute as ICommandBinding;
            if(bindCommand == null)
            {

                return;
            }

            bindCommand.OnUnBind();
        }

        protected virtual bool InternalAttachView()
        {
            if (TargetMember == null || Target == null)
            {
                return false;
            }

            SetMemberValue(TargetMember, Target, Source);

            return true;
        }

        protected virtual bool InternalBindViewContent()
        {
            if (TargetMember == null || Target == null || Source == null)
            {
                return false;
            }

            var bindAttrib = ((IBindingAttribute)Attribute);
            SetMemberValue(TargetMember, Target, bindAttrib.OnBindViewContentToProperty((View)Source, TargetMember.GetReturnType()));
            return true;
        }

        protected virtual bool InternalBindObjectToView()
        {
            if (Target == null || SourceMember == null || Source == null)
            {
                BindingTrace.TraceFail(BindingMode.ObjectToView, "Invalid parameters. Parameters 'Target', 'SourceMember' and 'Source' are required!");
                return false;
            }

            var bindAttrib = ((IBindingAttribute)Attribute);
            bindAttrib.OnBindPropertyToView((View)Target, SourceMember.GetMemberValue(Source), SourceMember.GetReturnType(), SourceMember);
            return true;
        }

        protected virtual bool InternalBindResource(IBindingResourceCache resourceCache = null)
        {
            //
            var resourceBind = ((IResourceAttribute)Attribute);
            Context context = (Context)Source;

            //
            if (resourceBind == null || context == null || TargetMember == null || Target == null)
            {
                BindingTrace.TraceFail(BindingMode.Resource, "Invalid parameters. Parameters 'Attribute', 'TargetMember', 'Target' and 'Source' are required!");
                return false;
            }

            //  Load resource
            object resource = null;
            if (resourceCache != null)
                resource = resourceCache.GetResource(resourceBind.ID);

            //  failed loading from cache??
            if (resource == null)
            {
                //  load resource
                resourceBind.LoadResource(context, TargetMember.GetReturnType());
            }

            if (resource == null)
            {
                // TRACE ***
                BindingTrace.TraceFail(BindingMode.Resource, $"Failed loading resource with id '{resourceBind.ID}'. Member: {TargetMember.Name}, Target: {Target.ToString()} ");
            }
            else
            {
                SetMemberValue(TargetMember, Target, resource);
            }

            return resource != null;
        }
    }
}