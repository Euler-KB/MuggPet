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
using MuggPet.Commands;
using System.Reflection;

namespace MuggPet.Binding
{
    /// <summary>
    /// Represents a binding handler interface.
    /// </summary>
    public interface IBindingHandler
    {
        /// <summary>
        /// Manages access commonly loaded resources 
        /// </summary>
        IBindingResourceCache ResourceManager { get; }

        /// <summary>
        /// Gets the state of an objet in binding
        /// </summary>
        /// <param name="frameObject">The object to determine state</param>
        /// <param name="mode">The binding mode to fetch</param>
        IEnumerable<BindState> GetState(object frameObject, BindingMode mode);

        /// <summary>
        /// Handles the binding of an object's member to the target view
        /// </summary>
        /// <param name="source">The object whose property is bound</param>
        /// <param name="view">The destination view</param>
        /// <param name="member">The member of the object to bind</param>
        /// <param name="update">When set to true, will forcibly undergo or update binding if has already taken place</param>
        bool BindObjectToView(object source, MemberInfo member, View view, bool update);

        /// <summary>
        /// Bind a view to an object
        /// </summary>
        /// <param name="source">The object whose property is bound</param>
        /// <param name="member">The member of the object to bind</param>
        /// <param name="view">The view to bind</param>
        /// <param name="update">When set to true, will forcibly undergo or update binding if has already taken place</param>
        bool BindViewContent(View view, object source, MemberInfo member, bool update);

        /// <summary>
        /// Attaches a view to a property
        /// </summary>
        /// <param name="view">The view to be attach</param>
        /// <param name="target">A reference to the object containing the member</param>
        /// <param name="member">The member to attach the view to</param>
        /// <param name="update">If true, will update view attachment else otherwise</param>
        /// <param name="flags">Logical combination of flags which modifies the behaviour of view attachment</param>
        bool AttachViewToProperty(View view, object target, MemberInfo member, BindFlags flags, bool update);

        /// <summary>
        /// Binds resource to object member 
        /// </summary>
        /// <param name="context">The context used in loading resources</param>
        /// <param name="update">If true, will update resource binding</param>
        /// <param name="member">A member of the target object</param>
        /// <param name="target">An instance of an object that receives binded resources</param>
        bool BindResource(Context context, object target, MemberInfo member, bool update);

        #region Commands

        /// <summary>
        /// Binds a command to a view. The member is reflected upon for command bindings interfaces
        /// </summary>
        bool BindCommand(object source, MemberInfo member, View containerView, object parameter, bool update);

        /// <summary>
        /// Unbinds command 
        /// </summary>
        bool UnBindCommand(object source, MemberInfo member, View containerView);

        /// <summary>
        /// Binds a command directly to the specified view. No reflection is involved
        /// </summary>
        bool BindCommandDirect(object source, ICommand command, View targetView, object parameter, bool update);

        /// <summary>
        /// Unbinds a command that was binded directly to the specified view
        /// </summary>
        bool UnBindCommandDirect(object source, ICommand command, View targetView);

        #endregion

        /// <summary>
        /// Destroy all bindings that applies the specified source object
        /// </summary>
        /// <param name="source">The object in context</param>
        bool Destroy(object source);

        /// <summary>
        /// Resets all binding operations
        /// </summary>
        void Reset();
    }
}