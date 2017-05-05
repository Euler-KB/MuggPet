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
using MuggPet.Views;
using MuggPet.Commands;
using MuggPet.Utils;

namespace MuggPet.Binding
{
    /// <summary>
    /// Manages scoped bindings for an object. Any class that wishes to support binding should posses an instance of this.
    /// </summary>
    public class BindingHandler : IBindingHandler
    {

        private IDictionary<object, BindingFrame> _bindingFrames = new Dictionary<object, BindingFrame>();

        /// <summary>
        /// Gets the resource manager for the binding handler
        /// </summary>
        public IBindingResourceCache ResourceManager
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the view the specified id
        /// </summary>
        protected View FindView(View container, int id)
        {
            if (id == BindConsts.RootViewId)
                return container;

            return container.FindViewById(id);
        }

        /// <summary>
        /// Scans for view attachment attributes and their corresponding views
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="member">The info for the member to scan </param>
        /// <param name="flags">The flags for  determining how attachment is processed</param>
        public IList<KeyValuePair<Attribute, View>> ProcessViewAttachment(View containerView, object obj, MemberInfo member, BindFlags flags)
        {
            //  hold applied bindings here
            var appliedBindings = new List<KeyValuePair<Attribute, View>>();

            //
            IDictionary<int, View> _viewMap = new Dictionary<int, View>();

            //  apply all binding attribute on property or field
            foreach (var bindAttrib in member.GetCustomAttributes().OfType<IBindingAttribute>())
            {
                //
                View targetView;
                if (!_viewMap.TryGetValue(bindAttrib.ID, out targetView))
                {
                    //  get target view
                    targetView = FindView(containerView, bindAttrib.ID);
                    _viewMap[bindAttrib.ID] = targetView;
                }

                if (targetView == null)
                    continue;

                //
                if (!bindAttrib.CanAttachView(targetView, member, member.GetReturnType()))
                    continue;

                //
                if (flags.HasFlag(BindFlags.GenerateViewID))
                    targetView.Id = ViewHelper.NewId;

                //  hold binding attributes
                appliedBindings.Add(new KeyValuePair<Attribute, View>((Attribute)bindAttrib, targetView));
            }

            return appliedBindings;
        }




        /// <summary>
        /// Scans for bindable attributes and their corresponding view
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="containerView">The container view for resolving views</param>
        /// <param name="member">The member info</param>
        /// <param name="supportViewContentBinding">True to fetch bindable attributes which have view content bindings enabled</param>
        public IList<KeyValuePair<Attribute, View>> ProcessBindableAttributes(object obj, View containerView, MemberInfo member, bool supportViewContentBinding)
        {
            //
            var appliedBindings = new List<KeyValuePair<Attribute, View>>();

            //  cache for loaded views
            IDictionary<int, View> _viewMap = new Dictionary<int, View>();

            //  apply all binding attributes
            foreach (var bindAttrib in member.GetCustomAttributes().OfType<IBindingAttribute>())
            {
                if (supportViewContentBinding && !bindAttrib.CanBindViewContent)
                    continue;

                //
                View targetView;
                if (!_viewMap.TryGetValue(bindAttrib.ID, out targetView))
                {
                    //  get target view
                    targetView = FindView(containerView, bindAttrib.ID);
                    _viewMap[bindAttrib.ID] = targetView;
                }

                if (targetView == null)
                    continue;

                //
                appliedBindings.Add(new KeyValuePair<Attribute, View>((Attribute)bindAttrib, targetView));
            }

            return appliedBindings;
        }

        /// <summary>
        /// Scans resource binding attributes for the specified member
        /// </summary>
        /// <param name="source"></param>
        /// <param name="member">The member under examination</param>
        public IList<Attribute> ProcessResourceBindings(object source, MemberInfo member)
        {
            return new List<Attribute>(member.GetCustomAttributes().OfType<IResourceAttribute>().Cast<Attribute>());
        }

        /// <summary>
        /// Scans for command binding attributes and their corresponding target views
        /// </summary>
        /// <param name="containerView">The contianer view for resolving individual command views</param>
        /// <param name="member">The member under examination</param>
        public IList<KeyValuePair<Attribute, View>> ProcessCommandBindings(View containerView, MemberInfo member)
        {
            var appliedBindings = new List<KeyValuePair<Attribute, View>>();
            IDictionary<int, View> _viewMap = new Dictionary<int, View>();
            foreach (var cmdBind in member.GetCustomAttributes().OfType<ICommandBinding>())
            {
                View srcView;
                if (!_viewMap.TryGetValue(cmdBind.ID, out srcView))
                {
                    srcView = FindView(containerView, cmdBind.ID);
                    _viewMap[cmdBind.ID] = srcView;
                }

                if (srcView == null)
                    continue;

                appliedBindings.Add(new KeyValuePair<Attribute, View>((Attribute)cmdBind, srcView));
            }

            return appliedBindings;
        }


        public bool AttachViewToProperty(View view, object source, MemberInfo member, BindFlags flags, bool update)
        {
            var frame = GetBindingFrame(source);
            var existingBindings = frame.ViewAttachment.Where(x => x.Target.Equals(source) &&
                                                                    x.TargetMember.Equals(member));
            if (update && existingBindings.Count() > 0)
            {
                IDictionary<int, View> _viewMap = new Dictionary<int, View>();
                foreach (var rBinding in existingBindings)
                {
                    int viewId = ((IBindingAttribute)rBinding.Attribute).ID;
                    View srcView;
                    if (!_viewMap.TryGetValue(viewId, out srcView))
                    {
                        srcView = FindView(view, viewId);
                        _viewMap[viewId] = srcView;
                    }

                    //  update view before updating bind
                    rBinding.Source = srcView;

                    rBinding.Execute();
                }

                return true;
            }
            else if ((!update && !existingBindings.Any()) ^ update)
            {
                var bindings = ProcessViewAttachment(view, source, member, flags);
                foreach (var aBind in bindings)
                {
                    BindState bindState = new BindState()
                    {
                        Mode = BindingMode.Attach,
                        Source = aBind.Value,
                        Target = source,
                        TargetMember = member,
                        Attribute = aBind.Key
                    };

                    //
                    if (bindState.Execute())
                        frame.ViewAttachment.Add(bindState);
                }

                return bindings.Count > 0;
            }

            return false;
        }

        public bool BindObjectToView(object obj, MemberInfo member, View view, bool update)
        {
            BindingFrame frame = GetBindingFrame(obj);
            var existingBindings = frame.ObjectViewBindings.Where(x => x.Source.Equals(obj) &&
                                                                       x.SourceMember.Equals(member)).ToArray();

            if (update && existingBindings.Count() > 0)
            {
                IDictionary<int, View> _viewMap = new Dictionary<int, View>();
                foreach (var rBinding in existingBindings)
                {
                    int viewId = ((IBindingAttribute)rBinding.Attribute).ID;
                    View srcView;
                    if (!_viewMap.TryGetValue(viewId, out srcView))
                    {
                        srcView = FindView(view, viewId);
                        _viewMap[viewId] = srcView;
                    }

                    //  update view before updating bind
                    rBinding.Target = srcView;

                    rBinding.Execute();
                }

                return true;
            }
            else if ((!update && !existingBindings.Any()) ^ update)
            {
                var bindings = ProcessBindableAttributes(obj, view, member, false);
                foreach (var vBind in bindings)
                {
                    var bindState = new BindState()
                    {
                        Attribute = vBind.Key,
                        Mode = BindingMode.ObjectToView,
                        Source = obj,
                        SourceMember = member,
                        Target = vBind.Value,
                    };

                    //
                    if (bindState.Execute())
                        frame.ObjectViewBindings.Add(bindState);

                }

                return bindings.Count > 0;
            }

            return false;
        }

        public bool BindViewContent(View view, object source, MemberInfo member, bool update)
        {
            var frame = GetBindingFrame(source);
            var existingBindings = frame.ViewContentBindings.Where(x => x.Target.Equals(source) &&
                                                                        x.TargetMember.Equals(member));

            if (update && existingBindings.Count() > 0)
            {
                IDictionary<int, View> _viewMap = new Dictionary<int, View>();
                foreach (var rBinding in existingBindings)
                {
                    int viewId = ((IBindingAttribute)rBinding.Attribute).ID;
                    View srcView;
                    if (!_viewMap.TryGetValue(viewId, out srcView))
                    {
                        srcView = FindView(view, viewId);
                        _viewMap[viewId] = srcView;
                    }

                    //  update view before updating bind
                    rBinding.Source = srcView;

                    rBinding.Execute();
                }

                return true;
            }
            else if ((!update && !existingBindings.Any()) ^ update)
            {
                var bindings = ProcessBindableAttributes(source, view, member, true);
                foreach (var vBind in bindings)
                {
                    BindState state = new BindState()
                    {
                        Attribute = vBind.Key,
                        Mode = BindingMode.ViewContent,
                        Source = vBind.Value,
                        Target = source,
                        TargetMember = member
                    };

                    if (state.Execute())
                        frame.ViewContentBindings.Add(state);
                }

                return bindings.Count > 0;
            }

            return true;
        }

        /// <summary>
        /// Returns the state of a binded source object
        /// </summary>
        /// <param name="obj">This is usually an instance of the ISupportBinding interface or an item within a collection</param>
        /// <param name="mode">Indicates the mode to fetch</param>
        public IEnumerable<BindState> GetState(object obj, BindingMode mode)
        {
            BindingFrame frame;
            if (_bindingFrames.TryGetValue(obj, out frame))
            {
                switch (mode)
                {
                    case BindingMode.Attach:
                        return frame.ViewAttachment;
                    case BindingMode.ObjectToView:
                        return frame.ObjectViewBindings;
                    case BindingMode.Resource:
                        return frame.ResourceBindings;
                    case BindingMode.Command:
                        return frame.CommandBindings;
                    case BindingMode.ViewContent:
                        return frame.ViewContentBindings;
                }
            }

            return null;
        }

        private BindingFrame GetBindingFrame(object source)
        {
            BindingFrame frame = null;
            if (!_bindingFrames.TryGetValue(source, out frame))
                _bindingFrames[source] = (frame = new BindingFrame());

            return frame;
        }

        public bool BindResource(Context context, object target, MemberInfo member, bool update)
        {
            var frame = GetBindingFrame(target);
            var existingBindings = frame.ResourceBindings.Where(x => x.Target.Equals(target) &&
                                                                     x.TargetMember.Equals(member));
            if (update && existingBindings.Count() > 0)
            {
                foreach (var rBinding in existingBindings)
                {
                    //  Update context (just in case)
                    rBinding.Source = context;

                    rBinding.Execute();
                }

                return true;
            }
            else if ((!update && !existingBindings.Any()) ^ update)
            {
                var bindings = ProcessResourceBindings(target, member);
                foreach (var rBinding in bindings)
                {
                    var state = new BindState()
                    {
                        Attribute = rBinding,
                        Mode = BindingMode.Resource,
                        Source = context,
                        Target = target,
                        TargetMember = member
                    };

                    //
                    if (state.Execute())
                        frame.ResourceBindings.Add(state);
                }

                return bindings.Count > 0;
            }

            return false;
        }

        private void InternalDestroyFrame(BindingFrame frame)
        {
            //  revert all command bindings
            foreach (var item in frame.CommandBindings)
                item.Revert();

            //  clear all other binding operations
            frame.CommandBindings.Clear();
            frame.ObjectViewBindings.Clear();
            frame.ResourceBindings.Clear();
            frame.ViewAttachment.Clear();
            frame.ViewContentBindings.Clear();
        }

        public bool Destroy(object target)
        {
            BindingFrame frame;
            if (_bindingFrames.TryGetValue(target, out frame))
            {
                InternalDestroyFrame(frame);
                _bindingFrames.Remove(target);
                return true;
            }

            return false;
        }

        public bool BindCommandDirect(object source, ICommand command, View destinationView, object parameter, bool update)
        {
            var frame = GetBindingFrame(source);
            if (command != null && !frame.CommandBindings.Any(x => x.Source.Equals(command) && x.Target.Equals(destinationView)))
            {
                var state = new BindState()
                {
                    Mode = BindingMode.Command,
                    Source = command,
                    Target = destinationView,
                    Extras = parameter
                };

                if (state.Execute())
                    frame.CommandBindings.Add(state);

                return true;
            }

            return false;
        }

        public bool BindCommand(object source, MemberInfo member, View destinationView, object parameter, bool update)
        {
            if (!member.GetReturnType().HasInterface<ICommand>())
                return false;

            //
            var frame = GetBindingFrame(source);
            ICommand command = member.GetMemberValue(source) as ICommand;
            if (command != null && !frame.CommandBindings.Any(x => x.Source.Equals(command) && x.TargetMember.Equals(member)))
            {
                var bindings = ProcessCommandBindings(destinationView, member);
                foreach (var cmdBind in bindings)
                {
                    var state = new BindState()
                    {
                        Attribute = cmdBind.Key,
                        Mode = BindingMode.Command,
                        Source = command,
                        Target = cmdBind.Value,
                        TargetMember = member,
                        Extras = parameter
                    };

                    if (state.Execute())
                        frame.CommandBindings.Add(state);
                }

                return bindings.Count > 0;
            }

            return false;
        }

        public void Reset()
        {
            foreach (var frame in _bindingFrames.Values)
                InternalDestroyFrame(frame);

            _bindingFrames.Clear();
        }

        public bool UnBindCommand(object source, MemberInfo member, View containerView)
        {
            var frame = GetBindingFrame(source);
            ICommand command = member.GetMemberValue(source) as ICommand;
            var bindings = frame.CommandBindings.Where(x => x.TargetMember.Equals(member) && x.Source.Equals(command));
            foreach (var bind in bindings)
            {
                bind.Revert();
                frame.CommandBindings.Remove(bind);
            }

            return bindings.Count() > 0;
        }

        public bool UnBindCommandDirect(object source, ICommand command, View targetView)
        {
            var frame = GetBindingFrame(source);
            var bind = frame.CommandBindings.FirstOrDefault(x => x.Source.Equals(command) && x.Target.Equals(targetView) &&
                //this will ensure it was through direct binding (:-
                x.TargetMember == null
            );

            if (bind != null)
            {
                bind.Revert();
                frame.CommandBindings.Remove(bind);
            }

            return bind != null;
        }
    }

}
