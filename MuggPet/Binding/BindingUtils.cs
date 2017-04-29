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
    internal static class BindingUtils
    {
        /// <summary>
        /// Determines whether the type is formattable and single line element
        /// </summary>
        static internal bool IsFormattablePrimitiveType(Type vType)
        {
            return vType.IsPrimitive || vType == typeof(DateTime) || vType == typeof(DateTimeOffset) ||
                    vType == typeof(TimeSpan) || vType == typeof(string) || vType == typeof(decimal);
        }

        /// <summary>
        /// Formats a primitive value with specified formatting arguments
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <param name="format">The formatting to apply</param>
        /// <returns>Returns the formatted value</returns>
        static internal object FormatPrimitive(object value, string format)
        {
            return string.Format(format ?? "{0}", value);
        }

        static internal object UnformatPrimitive(string value, string format)
        {
            string realFormat = format ?? "{0}";
            int startIndex = realFormat.IndexOf("{0");
            if (startIndex == -1)
                throw new BindingException("Cannot convert formatted value!");

            //
            int lBrace = realFormat.IndexOf('}', startIndex);
            if (lBrace == -1)
                throw new BindingException("Invalid format expression! Cannot convert formatted value!");

            //
            if (lBrace == realFormat.Length - 1)
                return value.Substring(startIndex);

            //
            int destIndex = value.LastIndexOf(format.Substring(lBrace + 1));
            if (destIndex == -1)
                throw new BindingException("Invalid format expression! Cannot convert formatted value!");

            //
            return value.Substring(startIndex, destIndex - startIndex);
        }

        static internal object GetPropertyValue(object source, string propertyName, Type propertyType, object defaultValue, string format)
        {
            var propInfo = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propInfo == null)
                return defaultValue;

            //
            var finalValue = propInfo.GetValue(source);
            if (format != null && propInfo.PropertyType == typeof(string))
            {
                finalValue = UnformatPrimitive((string)finalValue, format);
            }

            //
            return Convert.ChangeType(finalValue, propertyType);
        }

        static internal void BindProperties(object source, string propertyName, string defaultProperty, object value, string format)
        {
            var propInfo = source.GetType().GetProperty(propertyName ?? defaultProperty, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propInfo == null)
                throw new BindingException($"The requested property name '{propertyName ?? defaultProperty}' was not found upon binding");

            //
            object finalValue = value;
            if (format != null && propInfo.PropertyType == typeof(string))
            {
                finalValue = FormatPrimitive(value, format);
            }

            //  do we have compatible types yet??
            if (propInfo.PropertyType != finalValue.GetType())
            {
                //  try converting to destination type (:--
                finalValue = Convert.ChangeType(finalValue, propInfo.PropertyType);
            }

            //  set value finally
            propInfo.SetValue(source, finalValue);
        }

        static internal void BindMethod(object source, string methodName, object value, string format)
        {
            var methodInfo = source.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
                throw new BindingException($"The requested method '{methodName}' was not found prior to binding");

            //
            object parameterValue = value;
            var pms = methodInfo.GetParameters();
            if (pms.Length == 0)
                throw new BindingException("The specified method has no arguments!");

            //
            var pType = pms[0].GetType();
            if (format != null && pType == typeof(string) && format != null)
            {
                var vType = value.GetType();
                parameterValue = FormatPrimitive(value, format);
            }

            //  do we have compatible types yet??
            if (pType != parameterValue.GetType())
            {
                parameterValue = Convert.ChangeType(parameterValue, pType);
            }

            //  invoke value finally
            methodInfo.Invoke(source, new[] { parameterValue });
        }

        static internal void BindAuto(object source, string memberName, string defaultMember, object value, string format)
        {
            var methods = source.GetType().GetMember(memberName ?? defaultMember, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methods != null)
            {
                if (methods.Length > 1)
                    throw new BindingException("Ambiguous target specified! This may be due to the existence of a property and a method of equal names");

                //
                switch (methods[0].MemberType)
                {
                    case MemberTypes.Method:
                        BindProperties(source, memberName, defaultMember, value, format);
                        break;
                    case MemberTypes.Property:
                        BindMethod(source, memberName, value, format);
                        break;
                }
            }

        }

    }

}