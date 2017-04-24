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

namespace MuggPet.Utils
{
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
        /// Determines whether the specified type has the interface in the generic argument implemented
        /// </summary>
        /// <typeparam name="T">The interface to be checked</typeparam>
        /// <param name="type">The type to check for the interface</param>
        /// <returns></returns>
        public static bool HasInterface<T>(this Type type) 
        {
            return type.GetInterface(typeof(T).Name) != null || type == typeof(T);
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