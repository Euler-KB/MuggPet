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
    public static class VisualStateExtensions
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
            return member.WithCallback((view, args) =>
            {
                //  state manager wants us to apply our state
                if (args.Mode == StateChangeMode.NewState)
                {
                    view.SetTextColor(color);
                }

                //  state manager wants us to restore the default value
                else
                {
                    view.SetTextColor((Color)args.OriginalState);
                }

            }, (view) => new Color(view.CurrentTextColor));
        }

        public static StateMemberDefinitionWrapper<EditText> SetTextColor(this StateMemberDefinitionWrapper<EditText> member, Color color)
        {
            return member.WithCallback((view, args) =>
            {
                //  state manager wants us to apply our state
                if (args.Mode == StateChangeMode.NewState)
                {
                    view.SetTextColor(color);
                }

                //  state manager wants us to restore the default value
                else
                {
                    view.SetTextColor((Color)args.OriginalState);
                }

            }, (view) => new Color(view.CurrentTextColor));
        }

        public static StateMemberDefinitionWrapper<T> Focus<T>(this StateMemberDefinitionWrapper<T> member) where T : View
        {
            return member.WithCallback((view, args) =>
            {
                //  state manager wants us to apply our state
                if (args.Mode == StateChangeMode.NewState)
                {
                    view.RequestFocus();
                }

                //  state manager wants us to (revert state)
                else
                {
                    if ((bool)args.OriginalState)
                        view.RequestFocus();
                    else
                        view.ClearFocus();
                }

            }, (view) => view.IsFocused);
        }

        public static StateMemberDefinitionWrapper<T> Enable<T>(this StateMemberDefinitionWrapper<T> member) where T : View
        {
            return member.WithProperty(x => x.Enabled).Set(true);
        }

        public static StateMemberDefinitionWrapper<T> Disable<T>(this StateMemberDefinitionWrapper<T> member) where T : View
        {
            return member.WithProperty(x => x.Enabled).Set(false);
        }

        public static StateMemberDefinitionWrapper<T> Hide<T>(this StateMemberDefinitionWrapper<T> member, bool animate) where T : View
        {
            if (animate)
            {
                return member.WithCallback((view, args) =>
                {
                    //
                    args.AnimationState.Cancel("hide");

                    //  animate
                    args.AnimationState.Create("hide", view.Animate().Alpha(args.Mode == StateChangeMode.NewState ? 0 : (float)args.OriginalState)
                        .SetDuration(Animation.ViewAnimationConsts.FastDuration), () =>
                        {
                            view.Visibility = ViewStates.Gone;
                            view.Alpha = Animation.ViewAnimationConsts.AlphaOpaque;
                        });
                },
                (view) => view.Visibility);
            }
            else
                return member.WithProperty(x => x.Visibility).Set(ViewStates.Gone);
        }

        public static StateMemberDefinitionWrapper<T> Show<T>(this StateMemberDefinitionWrapper<T> member, bool animate) where T : View
        {
            if (animate)
            {
                return member.WithCallback((view, args) =>
                {
                    //
                    args.AnimationState.Cancel("show");

                    //  animate
                    args.AnimationState.Create("show", view.Animate().Alpha(args.Mode == StateChangeMode.NewState ? 1F : (float)args.OriginalState)
                        .SetDuration(Animation.ViewAnimationConsts.FastDuration), () =>
                        {
                            view.Visibility = ViewStates.Visible;
                        });
                },
                (view) => view.Visibility);

            }
            else
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