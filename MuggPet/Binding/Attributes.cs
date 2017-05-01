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
        /// Disables object binding
        /// </summary>
        NoCommand = 0x400000,

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
        /// Determines whether support view content binding
        /// </summary>
        bool CanBindViewContent { get; }

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

    /// <summary>
    /// Manages binding of common views to properties and vice versa.
    /// Any other unsupported operation can be implemented through subclassing
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class BindIDAttribute : Attribute, IBindingAttribute
    {
        /// <summary>
        /// Gets the id of the target view
        /// </summary>
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
        /// The formatting for items in binding
        /// </summary>
        public string StringFormat { get; set; }

        public bool CanBindViewContent => true;

        public virtual bool CanAttachView(View view, MemberInfo memberInfo, Type memberType)
        {
            //  We can attach only to views
            return memberType.IsSubclassOf(typeof(View));
        }

        /// <summary>
        /// Initializes a new binding operation for the adorned property or field
        /// </summary>
        /// <param name="ID">The id of the target view.</param>
        public BindIDAttribute(int ID)
        {
            this.ID = ID;
        }

        public void OnBindPropertyToView(View view, object propertyValue, Type propertyType, MemberInfo memberInfo)
        {
            if (propertyType.IsSubclassOf(typeof(View)))
                return;

            if (view is TextView || view is EditText)
            {
                BindingUtils.BindProperties(view, Target, "Text", propertyValue, StringFormat);
            }
            else if (view is ToggleButton)
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
        }

        public object OnBindViewValueToProperty(View view, Type propertyType)
        {
            Type viewType = view.GetType();
            if (propertyType.HasInterface<ICommand>())
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
    /// Inidcates the mode of color application
    /// </summary>
    public enum ColorSetMode
    {
        /// <summary>
        /// Applies the color to the background of the view
        /// </summary>
        Background,

        /// <summary>
        /// Applies the color to the foreground(text color...etc) of the view
        /// </summary>
        Foreground,

        /// <summary>
        /// Applies the color to both the foreground and background of the target view
        /// </summary>
        Both
    }

    /// <summary>
    /// Represents the base interface for all command bindings
    /// </summary>
    public interface ICommandBinding
    {
        /// <summary>
        /// The id of the target view (usually button)
        /// </summary>
        int ID { get; }

        /// <summary>
        /// Indicates the command executes asynchronously
        /// </summary>
        bool IsAsync { get; set; }

        /// <summary>
        /// Invoked to perform binding
        /// </summary>
        /// <param name="targetView">The view to bind to</param>
        void OnBind(ICommand command, View targetView);

        /// <summary>
        /// Invoked to revert binding
        /// </summary>
        void OnUnBind(ICommand command, View targetView);
    }

    /// <summary>
    /// Binds a command to view.
    /// Current implementation supports only views deriving from CompoundButton.
    /// Any other functionalities can be implemented through subclassing
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class BindCommand : Attribute, ICommandBinding
    {
        /// <summary>
        /// The id of the target view
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Indicates the command executes on a background thread
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// Gets the bound command. 
        /// </summary>
        protected ICommand Command { get; set; }

        public BindCommand(int id)
        {
            ID = id;
        }

        public void OnBind(ICommand command, View targetView)
        {
            if (!Equals(command, Command))
            {
                if (IsSupportedView(targetView))
                {
                    Command = command;
                    OnBindView(targetView);
                }
            }
        }

        /// <summary>
        /// Determines whether the supplied view is binding to the command
        /// </summary>
        /// <param name="view">The view to determine</param>
        protected virtual bool IsSupportedView(View view)
        {
            return view is Button || view is CompoundButton;
        }

        protected virtual void OnBindView(View view)
        {
            var btn = view as CompoundButton;
            if (btn != null)
            {
                btn.Click += OnBindAction;
            }
        }

        protected virtual void OnUnBindView(View view)
        {
            var btn = view as CompoundButton;
            if (btn != null)
            {
                btn.Click -= OnBindAction;
            }
        }

        private void OnBindAction(object sender, EventArgs e)
        {
            if (Command != null)
            {
                OnExecuteCommand(Command);
            }
        }

        protected virtual void OnExecuteCommand(ICommand command)
        {
            if (command.CanExecute(null))
                command.Execute(null);
        }

        public void OnUnBind(ICommand command, View targetView)
        {
            if (IsSupportedView(targetView) && object.Equals(Command, command))
            {
                OnUnBindView(targetView);
                Command = null;
            }
        }
    }

    /// <summary>
    /// Useful for binding color properties to views
    /// </summary>
    public class BindColor : Attribute, IBindingAttribute
    {
        public int ID { get; private set; }

        /// <summary>
        /// Determines the property to apply the color
        /// </summary>
        public ColorSetMode Mode { get; set; } = ColorSetMode.Foreground;

        public bool CanBindViewContent => false;

        public BindColor(int id)
        {
            ID = id;
        }

        public bool CanAttachView(View view, MemberInfo memberInfo, Type memberType)
        {
            return false;
        }

        public void OnBindPropertyToView(View view, object propertyValue, Type propertyType, MemberInfo memberInfo)
        {
            if (propertyType == typeof(Color))
            {
                var color = (Color)propertyValue;
                if (Mode == ColorSetMode.Both || Mode == ColorSetMode.Foreground)
                {
                    OnUpdateForeground(color, view);
                }

                if (Mode == ColorSetMode.Both || Mode == ColorSetMode.Background)
                {
                    OnUpdateBackground(color, view);
                }
            }
        }

        protected virtual void OnUpdateForeground(Color foreground, View view)
        {
            if (view is TextView)
                ((TextView)view).SetTextColor(foreground);
            else if (view is EditText)
                ((EditText)view).SetTextColor(foreground);
            else if (view is Button || view is CompoundButton)
                ((CompoundButton)view).SetTextColor(foreground);
            else
                view.Foreground = new ColorDrawable(foreground);
        }

        protected virtual void OnUpdateBackground(Color background, View view)
        {
            view.SetBackgroundColor(background);
        }

        public object OnBindViewValueToProperty(View view, Type propertyType)
        {
            throw new NotSupportedException();
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
        /// The resource id for string array. If set, will bind string array values with the given id from the resource to the adapter's source
        /// </summary>
        public int ItemsResourceId { get; set; } = -1;

        /// <summary>
        /// The item source for the adapter. This can be used in place of the ItemsResouceId property which fetches it's values from the resource
        /// </summary>
        public object[] ItemsSource { get; set; }

        //  Doesn't support view content bindings
        public bool CanBindViewContent => false;


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

        protected virtual void OnBindEnumerableSource(PropertyInfo propInfo, View view, Type sourceItemType, MemberInfo memberInfo, IEnumerable source)
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
                    //  Create a generic adapter wrapper for item
                    var adapter = typeof(GenericAdapter<>).ActivateGenericInstance(sourceItemType, view.Context, ItemLayout, source, null);
                    var mInvoke = GetType().GetMethod(nameof(BindAdapterAttribute.HandleAdapterFunctionality), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(sourceItemType);
                    propInfo.SetValue(view, mInvoke.Invoke(this, new object[] { adapter, memberInfo, sourceItemType }));
                }
            }
        }

        /// <summary>
        /// Applies avialable functionalities to the adapter given the member info 
        /// </summary>
        /// <param name="adapter">The adapter to receive the functionality update</param>
        /// <param name="memberInfo">The member to fetch functionalities from</param>
        protected virtual GenericAdapter<T> HandleAdapterFunctionality<T>(GenericAdapter<T> adapter, MemberInfo memberInfo)
        {
            var comparers = memberInfo.GetCustomAttributes().OfType<IAdapterObjectComparer>();
            if (comparers.Count() > 0)
                adapter.SortDescriptions = comparers.Select(x => new SortDescription() { Mode = x.Mode, Property = x.Property });

            return adapter;
        }

        public object OnBindViewValueToProperty(View view, Type propertyType)
        {
            //  Not supported in this context yet!
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Enables sorting on the adapter binding with specified property and mode.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class SortDescriptionAttribute : Attribute, IAdapterObjectComparer
    {
        /// <summary>
        /// Indicates the sort property
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// The order for sorting
        /// </summary>
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
    /// This can also be applied to 
    /// Thus the type of the property will determine the type of resource to load.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class BindResourceAttribute : ResourceAttribute, IBindingAttribute
    {
        public BindResourceAttribute(int ID)
        {
            this.id = ID;
        }

        //  Can't bind view content to this attribute
        public bool CanBindViewContent => false;    

        /// <summary>
        /// The property to apply the resource to when binding to views. 
        /// Required for property to view bindings
        /// </summary>
        public string Property { get; set; }

        public bool CanAttachView(View view, MemberInfo memberInfo, Type memberType)
        {
            //  Can't attach views with this attribute
            return false;
        }

        public override object LoadResource(Context context, Type targetType)
        {
            if (targetType == typeof(string))
                return context.Resources.GetString(ID);
            else if (targetType.HasInterface<IEnumerable<string>>())
                return context.Resources.GetStringArray(ID);
            else if (targetType == typeof(int))
                return context.Resources.GetInteger(ID);
            else if (targetType == typeof(bool))
                return context.Resources.GetBoolean(ID);
            else if (targetType == typeof(Stream))
                return context.Resources.OpenRawResource(ID);
            else if (targetType == typeof(Color))
                return context.Resources.GetColor(ID, null);
            else if (targetType.HasInterface<IEnumerable<int>>())
                return context.Resources.GetIntArray(ID);
            else if (targetType == typeof(System.Xml.XmlReader))
                return context.Resources.GetXml(ID);
            else if (targetType.HasInterface<IEnumerable<byte>>())
                return BinaryResourceLoader.Load(context, ID);
            else if (targetType == typeof(Bitmap))
                return BitmapFactory.DecodeResource(context.Resources, ID);
            else if (targetType == typeof(Drawable))
                return context.Resources.GetDrawable(ID, null);

            return null;
        }

        public void OnBindPropertyToView(View view, object propertyValue, Type propertyType, MemberInfo memberInfo)
        {
            //
            if (Property == null)
                return;

            //
            var resource = LoadResource(view.Context, propertyType);
            if(resource != null)
            {
                //  apply resource to view

            }
        }

        public object OnBindViewValueToProperty(View view, Type propertyType)
        {
            throw new NotSupportedException();
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
            if (targetType.HasInterface<IEnumerable<string>>())
                return context.Resources.GetStringArray(ID);

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
            if (targetType.HasInterface<IEnumerable<byte>>())
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