using Android.Content;
using Android.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.Views;
using Android.Widget;
using MuggPet.Utils;
using System.Collections;
using System.Reflection;
using Android.Graphics.Drawables;
using MuggPet.Commands;
using MuggPet.Utils.Adapter;

namespace MuggPet.Binding
{
    /// <summary>
    /// Defines various flags employed when performing binding
    /// </summary>
    [Flags]
    public enum BindFlags
    {
        /// <summary>
        /// Nothing
        /// </summary>
        None,

        /// <summary>
        /// Useful when building dynamic views. If causes a new id to be generated for the target view
        /// </summary>
        GenerateViewID = 0x100000,

        /// <summary>
        /// Disables binding for resources
        /// </summary>
        NoResource = 0x200000,

        /// <summary>
        /// Disables command binding
        /// </summary>
        NoObjectBinding = 0x400000

    }

    /// <summary>
    /// Represents all attributes that supports binding of views to properties or vice versa
    /// </summary>
    public interface IBindingAttribute
    {
        /// <summary>
        /// The id of the item or resource
        /// </summary>
        int ID { get; }

        /// <summary>
        /// Determines whether view attaching is supported on this attribute
        /// </summary>
        bool CanAttachView(View view, MemberInfo memberInfo, Type memberType);

        /// <summary>
        /// Invoked when a property is binding its value to a view.
        /// The aim of this method is to implement a mechanism of assigning(binding) the value of the property to the view
        /// </summary>
        void OnBindPropertyToView(View view, object propertyValue, Type propertyType, MemberInfo memberInfo);

        /// <summary>
        /// Invoked when a view is binding to a property. The main purpose of this method is to return the value 
        /// from the view specified for binding
        /// </summary>
        object OnBindViewValueToProperty(View view, Type propertyType);

    }

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

    /// <summary>
    /// Manages binding of basic and common views to properties and vice versa. Custom functionalities can be implemented through sub classing
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class BindIDAttribute : Attribute, IBindingAttribute
    {
        public int ID { get; }

        /// <summary>
        /// The target property or method used in binding
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// An additional parameter used in binding
        /// </summary>
        public object Parameter { get; set; }

        /// <summary>
        /// The formatting to use in binding
        /// </summary>
        public string StringFormat { get; set; }

        public virtual bool CanAttachView(View view, MemberInfo memberInfo, Type memberType)
        {
            //  We can attach only to views
            return memberType.IsSubclassOf(typeof(View));
        }

        public BindIDAttribute(int ID)
        {
            this.ID = ID;
        }

        public void OnBindPropertyToView(View view, object propertyValue, Type propertyType, MemberInfo memberInfo)
        {
            if (propertyType.IsSubclassOf(typeof(View)))
                return;

            if (propertyType.HasInterface<ICommand>())
            {
                //  bind command to view
                ICommand cmd = (ICommand)propertyValue;
                CommandBinding.BindCommand(cmd, view, null);
            }
            else
            {
                if (view is ToggleButton)
                {
                    BindingUtils.BindProperties(view, Target, "Checked", propertyValue, StringFormat);
                }
                else if (view is CompoundButton || view is Button)
                {
                    BindingUtils.BindProperties(view, Target, "Text", propertyValue, StringFormat);
                }
                else if (view is RadioGroup)
                {
                    if (Target == null)
                        BindingUtils.BindMethod(view, "Check", propertyValue, null);
                    else
                        BindingUtils.BindAuto(view, Target, "Check", propertyValue, null);
                }
                else if (view is SeekBar || view is ProgressBar)
                {
                    BindingUtils.BindProperties(view, Target, "Progress", propertyValue, null);
                }
                else if (view is ToggleButton)
                {
                    BindingUtils.BindProperties(view, Target, "Checked", propertyValue, null);
                }
                else if (view is SearchView)
                {
                    if (Target == null)
                    {
                        ((SearchView)view).SetQuery((string)propertyValue, false);
                    }
                    else
                    {
                        BindingUtils.BindProperties(view, Target, "Query", propertyValue, StringFormat);
                    }
                }
                else if (view is ImageView)
                {
                    if (Target == null)
                    {
                        if (propertyType == typeof(int))
                            BindingUtils.BindMethod(view, "SetImageBitmap", BitmapFactory.DecodeResource(view.Context.Resources, (int)propertyValue), null);
                        else if (propertyType == typeof(Bitmap))
                            BindingUtils.BindMethod(view, "SetImageBitmap", (Bitmap)propertyValue, null);
                    }
                    else
                    {
                        BindingUtils.BindAuto(view, Target, "SetImageBitmap", propertyValue, null);
                    }
                }
                else if (view is TextView || view is EditText)
                {
                    BindingUtils.BindProperties(view, Target, "Text", propertyValue, StringFormat);
                }
            }
        }

        public object OnBindViewValueToProperty(View view, Type propertyType)
        {
            Type viewType = view.GetType();
            if (propertyType == typeof(ICommand))
                return null;

            //
            object value = null;
            if (view is EditText)
                value = BindingUtils.GetPropertyValue(view, Target, propertyType, ((EditText)view).Text, StringFormat);
            else if (view is TextView)
                value = BindingUtils.GetPropertyValue(view, Target, propertyType, ((TextView)view).Text, StringFormat);
            else if (viewType == typeof(SearchView))
                value = BindingUtils.GetPropertyValue(view, Target, propertyType, ((SearchView)view).Query, StringFormat);
            else if (viewType == typeof(CheckBox))
            {
                if (propertyType == typeof(bool))
                    return ((CheckBox)view).Checked;
            }

            //
            if (value != null)
                return Convert.ChangeType(value, propertyType);

            return null;
        }
    }

    /// <summary>
    /// Implements the comparison of objects
    /// </summary>
    public interface IAdapterObjectComparer
    {
        /// <summary>
        /// The property to use in comparison
        /// </summary>
        string Property { get; }

        /// <summary>
        /// The mode of comparison
        /// </summary>
        SortOrder Mode { get; set; }

    }

    /// <summary>
    /// Binds values to adapters of views from resources and custom sources
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class BindAdapterAttribute : Attribute, IBindingAttribute
    {
        public int ID { get; }

        public BindAdapterAttribute(int ID)
        {
            this.ID = ID;
        }

        /// <summary>
        /// The name of the adapter property
        /// </summary>
        public string Property { get; set; } = "Adapter";

        /// <summary>
        /// The layout for each item
        /// </summary>
        public int ItemLayout { get; set; } = -1;

        /// <summary>
        /// The string formatting for  items
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// The resource id for string array
        /// </summary>
        public int ItemsResourceId { get; set; } = -1;

        /// <summary>
        /// The item source for the adapter. This can be used in place of the ItemsResouceId
        /// </summary>
        public object[] ItemsSource { get; set; }

        public bool CanAttachView(View view, MemberInfo memberInfo, Type memberType)
        {
            //  we only allow view attachment with members which are views
            return memberType.IsSubclassOf(typeof(View));
        }

        /// <summary>
        /// Formats string array values
        /// </summary>
        /// <param name="items">The string array or items</param>
        /// <param name="format">The format for each item</param>
        IEnumerable<string> FormatStringItems(IEnumerable items, string format)
        {
            List<string> values = new List<string>();
            foreach (var item in items)
                values.Add(BindingUtils.FormatPrimitive(item, format).ToString());

            return values;
        }

        public void OnBindPropertyToView(View view, object propertyValue, Type propertyType, MemberInfo memberInfo)
        {
            //  get the view type 
            var viewType = view.GetType();

            //
            var propInfo = view.GetType().GetProperty(Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propInfo == null)
                return;

            //  
            if (propertyType.IsSubclassOf(typeof(View)))
            {
                if (ItemsResourceId != -1)
                {
                    propInfo.SetValue(view, HandleAdapterFunctionality(GenericAdapter<string>.Create(view.Context, FormatStringItems(view.Context.Resources.GetStringArray(ItemsResourceId), StringFormat)), memberInfo));
                }
                else if (ItemsSource != null)
                {
                    var type = ItemsSource.GetType().GetInterface(typeof(IEnumerable<>).Name).GenericTypeArguments[0];
                    OnBindEnumerableSource(propInfo, view, type, memberInfo, ItemsSource);
                }
            }
            else if (propertyType.IsSubclassOf(typeof(IAdapter)))
            {
                //  just assign without any further processes (:-
                propInfo.SetValue(view, propertyValue);
            }
            else if (propertyValue is IEnumerable)
            {
                var type = propertyType.GetInterface(typeof(IEnumerable<object>).Name).GenericTypeArguments[0];
                OnBindEnumerableSource(propInfo, view, type, memberInfo, (IEnumerable)propertyValue);
            }
        }

        void OnBindEnumerableSource(PropertyInfo propInfo, View view, Type sourceItemType, MemberInfo memberInfo, IEnumerable source)
        {
            if (BindingUtils.IsFormattablePrimitiveType(sourceItemType) || StringFormat != null)
            {
                //  get format for strings
                IEnumerable<string> values = FormatStringItems(source, StringFormat);

                //
                GenericAdapter<string> adapter = null;
                if (ItemLayout == -1)
                    adapter = GenericAdapter<string>.Create(view.Context, values);
                else
                    adapter = new GenericAdapter<string>(view.Context, ItemLayout, values, (v, str) => v.FindViewById<TextView>(Android.Resource.Id.Text1).Text = str);

                //
                propInfo.SetValue(view, HandleAdapterFunctionality(adapter, memberInfo));
            }
            else
            {
                if (ItemLayout == -1)
                {
                    //  use obj.ToString() implementation of the custom data type as the text
                    propInfo.SetValue(view, HandleAdapterFunctionality(GenericAdapter<string>.Create(view.Context, ((IEnumerable<object>)source).Select(x => x.ToString())), memberInfo));
                }
                else
                {
                    //  create a generic adapter wrapper for item
                    var adapter = typeof(GenericAdapter<>).ActivateGenericInstance(sourceItemType, view.Context, ItemLayout, source, null);
                    var mInvoke = GetType().GetMethod(nameof(BindAdapterAttribute.HandleAdapterFunctionality), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(sourceItemType);
                    propInfo.SetValue(view, mInvoke.Invoke(this, new object[] { adapter, memberInfo, sourceItemType }));
                }
            }
        }


        private GenericAdapter<T> HandleAdapterFunctionality<T>(GenericAdapter<T> adapter, MemberInfo memberInfo)
        {
            //
            var comparers = memberInfo.GetCustomAttributes().OfType<IAdapterObjectComparer>();
            if (comparers.Count() > 0)
                adapter.SortDescriptions = comparers.Select(x => new SortDescription() { Mode = x.Mode, Property = x.Property });

            return adapter;
        }

        public object OnBindViewValueToProperty(View view, Type propertyType)
        {
            //  Not supported in this context yet!
            return null;
        }
    }

    /// <summary>
    /// Enables sorting on the adapter binding with specified property and mode.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class SortDescriptionAttribute : Attribute, IAdapterObjectComparer
    {
        public string Property { get; set; }

        public SortOrder Mode { get; set; }
    }

    #region Resources

    /// <summary>
    /// Represents resource loading attribute
    /// </summary>
    public interface IResourceAttribute
    {
        /// <summary>
        /// Returns the id for the resource item
        /// </summary>
        int ID { get; }

        /// <summary>
        /// Loads the resource object with the given id
        /// </summary>
        /// <param name="context">The context for fetching resources</param>
        /// <param name="targetType">The expected type of the loaded resource</param>
        /// <returns>The load resource object of the target type</returns>
        object LoadResource(Context context, Type targetType);
    }

    /// <summary>
    /// Represents the base for all resource attributes
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public abstract class ResourceAttribute : Attribute, IResourceAttribute
    {
        protected int id;

        public int ID { get { return id; } }

        public abstract object LoadResource(Context context, Type targetType);
    }


    /// <summary>
    /// Load resources as binary data
    /// </summary>
    internal static class BinaryResourceLoader
    {
        public static byte[] Load(Context context, int id)
        {
            using (var ms = new MemoryStream())
            using (var stream = context.Resources.OpenRawResource(id))
            {
                stream.CopyTo(ms);
                ms.Flush();
                return ms.ToArray();
            }
        }
    }

    /// <summary>
    /// Binds the adorned property or field to the specified resource id by mapping the type of the property to an appropriate resource item.
    /// Thus the type of the property will determine the type of resource to load.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class BindResourceAttribute : ResourceAttribute
    {
        public BindResourceAttribute(int ID)
        {
            this.id = ID;
        }

        public override object LoadResource(Context context, Type targetType)
        {
            if (targetType == typeof(string))
                return context.Resources.GetString(ID);
            else if (targetType == typeof(string[]))
                return context.Resources.GetStringArray(ID);
            else if (targetType == typeof(int))
                return context.Resources.GetInteger(ID);
            else if (targetType == typeof(bool))
                return context.Resources.GetBoolean(ID);
            else if (targetType == typeof(Stream))
                return context.Resources.OpenRawResource(ID);
            else if (targetType == typeof(Color))
                return context.Resources.GetColor(ID, null);
            else if (targetType == typeof(int[]))
                return context.Resources.GetIntArray(ID);
            else if (targetType == typeof(System.Xml.XmlReader))
                return context.Resources.GetXml(ID);
            else if (targetType == typeof(byte[]))
                return BinaryResourceLoader.Load(context, ID);
            else if (targetType == typeof(Bitmap))
                return BitmapFactory.DecodeResource(context.Resources, ID);
            else if (targetType == typeof(Drawable))
                return context.Resources.GetDrawable(ID, null);

            return null;
        }
    }

    /// <summary>
    /// Binds a string from resource to the adorned property or field
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class BindStringAttribute : ResourceAttribute
    {
        public BindStringAttribute(int ID)
        {
            this.id = ID;
        }

        public override object LoadResource(Context context, Type targetType)
        {
            if (targetType == typeof(string))
                return context.GetString(ID);

            return null;
        }
    }

    /// <summary>
    /// Binds a string array from resource to the adorned property or field
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class BindStringArrayAttribute : ResourceAttribute, IResourceAttribute
    {
        public BindStringArrayAttribute(int ID)
        {
            this.id = ID;
        }

        public override object LoadResource(Context context, Type targetType)
        {
            if (targetType == typeof(string[]))
                return context.Resources.GetStringArray(ID);

            return null;
        }
    }

    /// <summary>
    /// Binds a color from resource to the target property or field
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class BindColorAttribute : ResourceAttribute
    {
        public BindColorAttribute(int ID)
        {
            this.id = ID;
        }

        public override object LoadResource(Context context, Type targetType)
        {
            if (targetType == typeof(Color))
                return context.Resources.GetColor(ID, null);

            return null;
        }
    }

    /// <summary>
    /// Binds raw resource content (byte[]) to adorned property or field.
    /// Supports streams as well
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class BindRawAttribute : ResourceAttribute
    {
        public BindRawAttribute(int ID)
        {
            this.id = ID;
        }

        public override object LoadResource(Context context, Type targetType)
        {
            if (targetType == typeof(byte[]))
                return BinaryResourceLoader.Load(context, ID);
            else if (targetType == typeof(Stream))
                return context.Resources.OpenRawResource(ID);

            return null;
        }
    }

    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class BindImageAttribute : ResourceAttribute
    {
        public override object LoadResource(Context context, Type targetType)
        {
            if (targetType == typeof(Bitmap))
                return BitmapFactory.DecodeResource(context.Resources, ID);

            return null;
        }
    }

    #endregion

}