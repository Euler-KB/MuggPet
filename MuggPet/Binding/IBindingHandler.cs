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
        /// Gets the state of an objet in binding
        /// </summary>
        /// <param name="obj">The object to determine state</param>
        IEnumerable<BindState> GetState(object obj , BindingMode mode );

        /// <summary>
        /// Handles the binding of an object's member to the target view
        /// </summary>
        /// <param name="obj">The object whose property is bound</param>
        /// <param name="view">The destination view</param>
        /// <param name="member">The member of the object to bind</param>
        /// <param name="update">When set to true, will forcibly undergo or update binding if has already taken place</param>
        bool BindObjectToView(object obj, MemberInfo member, View view, bool update);

        /// <summary>
        /// Bind a view to an object
        /// </summary>
        /// <param name="view">The view to bind</param>
        /// <param name="update">When set to true, will forcibly undergo or update binding if has already taken place</param>
        bool BindViewContent(View view , object source, MemberInfo member, bool update);

        /// <summary>
        /// Attaches a view to a property
        /// </summary>
        /// <param name="view">The view to be attach</param>
        /// <param name="target">A reference to the object containing the member</param>
        /// <param name="member">The member to attach the view to</param>
        /// <param name="update">If true, will update view attachment else otherwise</param>
        bool AttachViewToProperty(View view, object target, MemberInfo member, bool update);

        /// <summary>
        /// Binds resource to object member 
        /// </summary>
        /// /// <param name="update">If true, will update view attachment else otherwise</param>
        bool BindResource(Context context , object target, MemberInfo member, bool update);


        /// <summary>
        /// Binds a command to a view
        /// </summary>
        bool BindCommand(object source, MemberInfo member, View destinationView , bool update);
        
        /// <summary>
        /// Destroy all bindings that applies the specified target object
        /// </summary>
        /// <param name="target">The object in context</param>
        bool Destroy(object target);

        /// <summary>
        /// Resets all binding operations
        /// </summary>
        void Reset();
    }
}