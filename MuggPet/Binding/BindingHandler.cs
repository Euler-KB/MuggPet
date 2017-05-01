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

        private IList<KeyValuePair<Attribute, View>> ProcessViewAttachment(View view, object obj, MemberInfo member, BindFlags flags)
        {
            //  hold applied bindings here
            var appliedBindings = new List<KeyValuePair<Attribute, View>>();

            //  apply all binding attribute on property or field
            foreach (var bindAttrib in member.GetCustomAttributes().OfType<IBindingAttribute>())
            {
                //
                View targetView = view.FindViewById(bindAttrib.ID);
                if (targetView == null)
                    continue;

                //  get member type
                Type memberType = null;
                if (member.MemberType == MemberTypes.Field)
                    memberType = ((FieldInfo)member).FieldType;
                if (member.MemberType == MemberTypes.Property)
                    memberType = ((PropertyInfo)member).PropertyType;

                //
                if (!bindAttrib.CanAttachView(view, member, memberType))
                    continue;

                if (flags.HasFlag(BindFlags.GenerateViewID))
                    targetView.Id = ViewHelper.NewId;

                //  hold binding attributes
                appliedBindings.Add(new KeyValuePair<Attribute, View>((Attribute)bindAttrib, targetView));
            }

            return appliedBindings;
        }

        public IList<KeyValuePair<Attribute, View>> ProcessBindableAttributes(object obj, View view, MemberInfo member)
        {
            //
            var appliedBindings = new List<KeyValuePair<Attribute, View>>();

            //  apply all binding attributes
            foreach (var bindAttrib in member.GetCustomAttributes().OfType<IBindingAttribute>())
            {
                //  get target view
                var targetView = view.FindViewById(bindAttrib.ID);
                if (targetView == null)
                    continue;

                //
                appliedBindings.Add(new KeyValuePair<Attribute, View>((Attribute)bindAttrib, targetView));
            }

            return appliedBindings;
        }

        public IList<Attribute> ProcessResourceBindings(object source, MemberInfo member)
        {
            return new List<Attribute>(member.GetCustomAttributes().OfType<IResourceAttribute>().Cast<Attribute>());
        }

        public IList<KeyValuePair<Attribute, View>> ProcessCommandBindings(View containerView, MemberInfo member)
        {
            var appliedBindings = new List<KeyValuePair<Attribute, View>>();
            foreach (var cmdBind in member.GetCustomAttributes().OfType<ICommandBinding>())
            {
                var targetView = containerView.FindViewById(cmdBind.ID);
                if (containerView != null && targetView is Button)
                    appliedBindings.Add(new KeyValuePair<Attribute, View>((Attribute)cmdBind, targetView));
            }

            return appliedBindings;
        }


        public bool AttachViewToProperty(View view, object source, MemberInfo member, bool update)
        {
            var frame = GetBindingFrame(source);
            var existingBindings = frame.ViewAttachment.Where(x => x.Target.Equals(source) &&
                                                                    x.TargetMember.Equals(member));
            if (update && existingBindings.Count() > 0)
            {
                foreach (var rBinding in existingBindings)
                {
                    //  update view before updating bind
                    rBinding.Source = view.FindViewById(((IBindingAttribute)rBinding.Attribute).ID);

                    rBinding.Apply();
                }

                return true;
            }
            else if ((!update && !existingBindings.Any()) ^ update)
            {
                var bindings = ProcessViewAttachment(view, source, member, BindFlags.None);
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
                    frame.ViewAttachment.Add(bindState);

                    //  apply bind state
                    bindState.Apply();
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
                foreach (var rBinding in existingBindings)
                {
                    //  update view before updating bind
                    rBinding.Target = view.FindViewById(((IBindingAttribute)rBinding.Attribute).ID);

                    rBinding.Apply();
                }

                return true;
            }
            else if ((!update && !existingBindings.Any()) ^ update)
            {
                var bindings = ProcessBindableAttributes(obj, view, member);
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
                    frame.ObjectViewBindings.Add(bindState);

                    //  apply binding
                    bindState.Apply();
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
                foreach (var rBinding in existingBindings)
                {
                    //  update view before updating bind
                    rBinding.Source = view.FindViewById(((IBindingAttribute)rBinding.Attribute).ID);

                    rBinding.Apply();
                }

                return true;
            }
            else if ((!update && !existingBindings.Any()) ^ update)
            {
                var bindings = ProcessBindableAttributes(source, view, member);
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

                    frame.ViewContentBindings.Add(state);
                }

                return bindings.Count > 0;
            }

            return true;
        }

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

                    rBinding.Apply();
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
                    frame.ResourceBindings.Add(state);

                    //  apply state
                    state.Apply();
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

        public bool BindCommandDirect(object source, ICommand command, View destinationView, bool update)
        {
            var frame = GetBindingFrame(source);
            if (command != null && !frame.CommandBindings.Any(x => x.Source.Equals(command) && x.Target.Equals(destinationView)))
            {
                var state = new BindState()
                {
                    Mode = BindingMode.Command,
                    Source = command,
                    Target = destinationView,
                };

                frame.CommandBindings.Add(state);

                state.Apply();

                return true;
            }

            return false;
        }

        public bool BindCommand(object source, MemberInfo member, View destinationView, bool update)
        {
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
                        TargetMember = member
                    };

                    //
                    frame.CommandBindings.Add(state);

                    //  apply state
                    state.Apply();
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
