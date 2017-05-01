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
        /// This binding mode indicates 
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
    /// Describes a the state of a binding
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
        /// The binding attribute
        /// </summary>
        public Attribute Attribute { get; set; }

        public void Apply()
        {
            switch (Mode)
            {
                case BindingMode.Attach:
                    InternalAttachView();
                    break;
                case BindingMode.ObjectToView:
                    InternalBindObjectToView();
                    break;
                case BindingMode.ViewContent:
                    InternalBindViewContent();
                    break;
                case BindingMode.Resource:
                    InternalBindResource();
                    break;
                case BindingMode.Command:
                    InternalBindCommand();
                    break;
            }
        }

        //  Note: Not all modes support reverting
        public void Revert()
        {
            switch (Mode)
            {
                case BindingMode.Command:
                    InternalUnBindCommand();
                    break;
            }
        }

        public static void SetMemberValue(MemberInfo member, object target, object value)
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

        protected virtual void InternalBindCommand()
        {
            ICommandBinding bindCommand = (ICommandBinding)Attribute;
            bindCommand.OnBind((ICommand)Source, (View)Target);
        }

        protected virtual void InternalUnBindCommand()
        {
            ICommandBinding bindCommand = (ICommandBinding)Attribute;
            bindCommand.OnUnBind((ICommand)Source, (View)Target);
        }

        protected virtual void InternalAttachView()
        {
            SetMemberValue(TargetMember, Target, Source);
        }

        protected virtual void InternalBindViewContent()
        {
            var bindAttrib = ((IBindingAttribute)Attribute);
            SetMemberValue(TargetMember, Target, bindAttrib.OnBindViewValueToProperty((View)Source, TargetMember.GetReturnType()));
        }

        protected virtual void InternalBindObjectToView()
        {
            var bindAttrib = ((IBindingAttribute)Attribute);
            if (SourceMember.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo)SourceMember;
                var fieldValue = field.GetValue(Source);
                bindAttrib.OnBindPropertyToView((View)Target, fieldValue, field.FieldType, SourceMember);
            }
            else if (SourceMember.MemberType == MemberTypes.Property)
            {
                var property = (PropertyInfo)SourceMember;
                var propertyValue = property.GetValue(Source);
                bindAttrib.OnBindPropertyToView((View)Target, propertyValue, property.PropertyType, SourceMember);
            }
        }

        protected virtual void InternalBindResource()
        {
            var resourceBind = ((IResourceAttribute)Attribute);
            Context context = (Context)Source;
            SetMemberValue(TargetMember, Target, resourceBind.LoadResource(context, TargetMember.GetReturnType()));
        }
    }
}