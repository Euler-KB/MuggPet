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
using MuggPet.Utils;
using Android.Graphics;

namespace MuggPet.Activity.VisualState
{
    public static class Extensions
    {
        /// <summary>
        /// Transitions between two states
        /// </summary>
        /// <param name="stateManager">The state manager</param>
        /// <param name="newState">The new visual state. It is activated immediately after call</param>
        /// <param name="restoreState">The restore visual state. It is activated after dispose has been called on the returned object. If null, the default state will be activated</param>
        public static IDisposable TransitionStates(this VisualStateManager stateManager, string newState, string restoreState = null)
        {
            return BusyState.Begin(() => stateManager.GotoState(newState), () => 
            {
                if (restoreState != null)
                    stateManager.GotoState(restoreState);
                else stateManager.GotoDefaultState();
            });
        }

        public static StateMemberDefinitionWrapper<TextView> SetText(this StateMemberDefinitionWrapper<TextView> member, string text)
        {
            return member.WithProperty(x => x.Text).Set(text);
        }

        public static StateMemberDefinitionWrapper<EditText> SetText(this StateMemberDefinitionWrapper<EditText> member, string text)
        {
            return member.WithProperty(x => x.Text).Set(text);
        }

        public static StateMemberDefinitionWrapper<TextView> SetTextColor(this StateMemberDefinitionWrapper<TextView> member, Color color)
        {
            return member.WithComplex((view, value) =>
            {
                //  state manager wants us to apply our state
                if (value == null)
                {
                    view.SetTextColor(color);
                }

                //  state manager wants us to restore the default value
                else
                {
                    view.SetTextColor((Color)value);
                }

            }, (view) => new Color(view.CurrentTextColor));
        }

        public static StateMemberDefinitionWrapper<EditText> SetTextColor(this StateMemberDefinitionWrapper<EditText> member, Color color)
        {
            return member.WithComplex((view, value) =>
            {
                //  state manager wants us to apply our state
                if (value == null)
                {
                    view.SetTextColor(color);
                }

                //  state manager wants us to restore the default value
                else
                {
                    view.SetTextColor((Color)value);
                }

            }, (view) => new Color(view.CurrentTextColor));
        }

        public static StateMemberDefinitionWrapper<T> Focus<T>(this StateMemberDefinitionWrapper<T> member) where T : View
        {
            return member.WithComplex((view, value) =>
            {
                //  state manager wants us to apply our state
                if (value == null)
                {
                    view.RequestFocus();
                }

                //  state manager wants us to (revert state)
                else
                {
                    if ((bool)value)
                        view.RequestFocus();
                    else
                        view.ClearFocus();
                }

            }, (view) => view.IsFocused);
        }

        public static StateMemberDefinitionWrapper<T> Enable<T>(this StateMemberDefinitionWrapper<T> member, bool enable) where T : View
        {
            return member.WithProperty(x => x.Enabled).Set(enable);
        }

        public static StateMemberDefinitionWrapper<T> Hide<T>(this StateMemberDefinitionWrapper<T> member) where T : View
        {
            return member.WithProperty(x => x.Visibility).Set(ViewStates.Gone);
        }

        public static StateMemberDefinitionWrapper<T> Show<T>(this StateMemberDefinitionWrapper<T> member) where T : View
        {
            return member.WithProperty(x => x.Visibility).Set(ViewStates.Visible);
        }

        public static StateMemberDefinitionWrapper<T> SetAlpha<T>(this StateMemberDefinitionWrapper<T> member, float alpha) where T : View
        {
            return member.WithProperty(x => x.Alpha).Set(alpha);
        }

        public static StateMemberDefinitionWrapper<T> SetSelected<T>(this StateMemberDefinitionWrapper<T> member, bool selected) where T : View
        {
            return member.WithProperty(x => x.Selected).Set(selected);
        }

    }
}