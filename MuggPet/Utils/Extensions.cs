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

namespace MuggPet.Utils
{
    /// <summary>
    /// Provides global extensions for various components
    /// </summary>
    public static class Extensions
    {
        #region Dialogs

        /// <summary>
        /// Ensures dialog is dismissed when dispose is called on the returned object
        /// </summary>
        public static IDisposable Scope(this Dialog dlg)
        {
            return BusyState.Begin(null, () => dlg.Dismiss());
        }

        #endregion

        #region Reflection

        /// <summary>
        /// Determines whether the specified type implements or inherits the interface in the arguments supplied
        /// </summary>
        /// <typeparam name="T">The interface to be checked</typeparam>
        /// <param name="type">The type to check for the interface</param>
        /// <returns></returns>
        public static bool HasInterface<T>(this Type type)
        {
            return type.GetInterface(typeof(T).Name) != null || type == typeof(T);
        }

        /// <summary>
        /// Returns the return type for the specified member info
        /// </summary>
        /// <param name="memberInfo">The member info in context</param>
        public static Type GetReturnType(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).FieldType;

            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).PropertyType;

            if (memberInfo is MethodInfo)
                return ((MethodInfo)memberInfo).ReturnType;

            return null;
        }

        /// <summary>
        /// Returns the return type for the specified member info
        /// </summary>
        /// <param name="memberInfo">The member info in context</param>
        /// <param name="source">A reference to the object containing the member</param>
        /// <param name="parameters">Parameters for invoking the member if its a method</param>
        public static object GetMemberValue(this MemberInfo memberInfo, object source, params object[] parameters)
        {
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).GetValue(source);

            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).GetValue(source);

            if (memberInfo is MethodInfo)
                return ((MethodInfo)memberInfo).Invoke(source, parameters);

            return null;
        }


        /// <summary>
        /// Makes a generic type for the type argument
        /// </summary>
        /// <typeparam name="T">The new generic type argument</typeparam>
        /// <param name="genericType">The generic type definition</param>
        /// <returns>A new generic type with generic argument T</returns>
        public static Type GetGenericType<T>(this Type genericType)
        {
            return GetGenericType(genericType, typeof(T));
        }

        /// <summary>
        /// Makes a generic type for the type argument
        /// </summary>
        /// <param name="customGenericType">The new generic type argument</param>
        /// <param name="genericType">The generic type definition</param>
        /// <returns>A new generic type with generic argument T</returns>
        public static Type GetGenericType(this Type genericType, Type customGenericType)
        {
            return (genericType.IsGenericTypeDefinition ? genericType : genericType.GetGenericTypeDefinition()).MakeGenericType(customGenericType);
        }

        /// <summary>
        /// Instantiates a generic type from its base or generic type definition to a new generic type with specified arguments
        /// </summary>
        /// <typeparam name="T">The new generic type argument</typeparam>
        /// <param name="genericType">The base generic type definition.</param>
        /// <param name="arguments">The arguments for instantiating the new type</param>
        /// <returns>A new generic type object from the base generic type</returns>
        public static object ActivateGenericInstance<T>(this Type genericType, params object[] arguments)
        {
            return ActivateGenericInstance(genericType, typeof(T), arguments);
        }

        /// <summary>
        /// Instantiates a generic type from its base or generic type definition to a new generic type with specified arguments
        /// </summary>
        /// <param name="customGenericType">The new generic type argument</param>
        /// <param name="genericType">The base generic type definition.</param>
        /// <param name="arguments">The arguments for instantiating the new type</param>
        /// <returns>A new generic type object from the base generic type</returns>
        public static object ActivateGenericInstance(this Type genericType, Type customGenericType, params object[] arguments)
        {
            var type = GetGenericType(genericType, customGenericType);
            return Activator.CreateInstance(type, arguments);
        }

        #endregion

    }
}