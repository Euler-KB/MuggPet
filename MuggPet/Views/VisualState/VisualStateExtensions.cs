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

namespace MuggPet.Views.VisualState
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
                else
                    stateManager.GotoDefaultState();
            });
        }


        /// <summary>
        /// Sets the text of the attached text view.
        /// </summary>
        /// <param name="text">The text to set</param>
        public static StateMemberDefinitionWrapper<TextView> SetText(this StateMemberDefinitionWrapper<TextView> member, string text)
        {
            return member.WithProperty(x => x.Text).Set(text);
        }

        /// <summary>
        /// Sets the text of the attached edit text view
        /// </summary>
        /// <param name="text">The text to set</param>
        public static StateMemberDefinitionWrapper<EditText> SetText(this StateMemberDefinitionWrapper<EditText> member, string text)
        {
            return member.WithProperty(x => x.Text).Set(text);
        }

        /// <summary>
        /// Sets the text color of the attached text view
        /// </summary>
        /// <param name="color">The color to set</param>
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

        /// <summary>
        /// Sets the text color of the specified edit text view
        /// </summary>
        /// <param name="color">Specifies the color for the text</param>
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

        /// <summary>
        /// Focuses on the requested view
        /// </summary>
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

        /// <summary>
        /// Enables the attached view. Thus sets Enabled property to true
        /// </summary>
        public static StateMemberDefinitionWrapper<T> Enable<T>(this StateMemberDefinitionWrapper<T> member) where T : View
        {
            return member.WithProperty(x => x.Enabled).Set(true);
        }

        /// <summary>
        /// Disables the attached view. Thus sets Enabled property to false
        /// </summary>
        public static StateMemberDefinitionWrapper<T> Disable<T>(this StateMemberDefinitionWrapper<T> member) where T : View
        {
            return member.WithProperty(x => x.Enabled).Set(false);
        }

        public static StateMemberDefinitionWrapper<T> SetWidth<T>(this StateMemberDefinitionWrapper<T> member, int width) where T : View
        {
            return member.WithCallback((view, args) =>
            {
                if (args.Mode == StateChangeMode.NewState)
                {
                    var pms = view.LayoutParameters;
                    pms.Width = width;
                    view.LayoutParameters = pms;
                }
                else if (args.Mode == StateChangeMode.Revert)
                    view.LayoutParameters = (ViewGroup.LayoutParams)args.OriginalState;

            }, (view) => view.LayoutParameters);
        }

        public static StateMemberDefinitionWrapper<T> SetHeight<T>(this StateMemberDefinitionWrapper<T> member, int height) where T : View
        {
            return member.WithCallback((view, args) =>
            {
                if (args.Mode == StateChangeMode.NewState)
                {
                    var pms = view.LayoutParameters;
                    pms.Height = height;
                    view.LayoutParameters = pms;
                }
                else if (args.Mode == StateChangeMode.Revert)
                    view.LayoutParameters = (ViewGroup.LayoutParams)args.OriginalState;

            }, (view) => view.LayoutParameters);
        }

        public static StateMemberDefinitionWrapper<T> SetSize<T>(this StateMemberDefinitionWrapper<T> member, int width, int height) where T : View
        {
            return member.WithCallback((view, args) =>
            {
                if (args.Mode == StateChangeMode.NewState)
                {
                    var pms = view.LayoutParameters;
                    pms.Height = height;
                    pms.Width = width;
                    view.LayoutParameters = pms;
                }
                else if (args.Mode == StateChangeMode.Revert)
                    view.LayoutParameters = (ViewGroup.LayoutParams)args.OriginalState;

            }, (view) => view.LayoutParameters);
        }

        public static StateMemberDefinitionWrapper<T> TranslateX<T>(this StateMemberDefinitionWrapper<T> member, float offset) where T : View
        {
            return member.WithCallback((view, args) =>
            {
                args.AnimationState.CancelAll();

                if (args.Mode == StateChangeMode.NewState)
                {
                    args.AnimationState.Create("translate", view.Animate()
                        .TranslationX(offset));
                }
                else if (args.Mode == StateChangeMode.Revert)
                {
                    args.AnimationState.Cancel("translate");
                    view.TranslationX = (float)args.OriginalState;
                }

            }, (view) => view.TranslationX);
        }


        public static StateMemberDefinitionWrapper<T> TranslateY<T>(this StateMemberDefinitionWrapper<T> member, float offset) where T : View
        {
            return member.WithCallback((view, args) =>
            {
                args.AnimationState.CancelAll();

                if (args.Mode == StateChangeMode.NewState)
                {
                    args.AnimationState.Create("translate", view.Animate().SetDuration(220)
                        .TranslationY(offset));
                }
                else if (args.Mode == StateChangeMode.Revert)
                {
                    if (view.TranslationY != (float)args.OriginalState)
                    {
                        //  ensure canceled
                        args.AnimationState.Create("translate", view.Animate().SetDuration(220)
                            .TranslationY((float)args.OriginalState));
                    }
                }

            }, (view) => view.TranslationY);
        }

        /// <summary>
        /// Hides the attached view. Thus sets the visibility to ViewState.Gone
        /// </summary>
        /// <param name="animate">Determines whether to animate hide effect</param>
        public static StateMemberDefinitionWrapper<T> Hide<T>(this StateMemberDefinitionWrapper<T> member, bool animate = false) where T : View
        {
            if (animate)
            {
                return member.WithCallback((view, args) =>
                {
                    //
                    args.AnimationState.CancelAll();

                    if (args.Mode == StateChangeMode.NewState)
                    {
                        //  animate
                        args.AnimationState.Create("hide", view.Animate().Alpha(Animation.ViewAnimationConsts.AlphaTransparent)
                            .SetDuration(Animation.ViewAnimationConsts.FastDuration), () =>
                            {
                                view.Visibility = ViewStates.Gone;
                                view.Alpha = Animation.ViewAnimationConsts.AlphaTransparent;
                            });
                    }
                    else if (args.Mode == StateChangeMode.Revert)
                    {
                        var viewState = (ViewStates)args.OriginalState;
                        if (viewState != view.Visibility)
                        {
                            args.AnimationState.Create("hide", view.Animate().Alpha(viewState == ViewStates.Visible ? 1 : 0)
                                .SetDuration(Animation.ViewAnimationConsts.FastDuration), () =>
                                {
                                    view.Visibility = viewState;
                                });
                        }

                    }

                }, (view) => view.Visibility);
            }
            else
                return member.WithProperty(x => x.Visibility).Set(ViewStates.Gone);
        }

        /// <summary>
        /// Shows the specified view. Thus sets the visibility to ViewStates.Visible
        /// </summary>
        /// <param name="animate">Determines whether to animate show effect</param>
        public static StateMemberDefinitionWrapper<T> Show<T>(this StateMemberDefinitionWrapper<T> member, bool animate = false) where T : View
        {
            if (animate)
            {
                return member.WithCallback((view, args) =>
                {
                    //
                    args.AnimationState.CancelAll();

                    if (args.Mode == StateChangeMode.NewState)
                    {
                        args.AnimationState.Create("show", view.Animate().Alpha(Animation.ViewAnimationConsts.AlphaOpaque)
                            .SetDuration(Animation.ViewAnimationConsts.FastDuration), () =>
                            {
                                view.Visibility = ViewStates.Visible;
                                view.Alpha = Animation.ViewAnimationConsts.AlphaOpaque;
                            });
                    }
                    else if (args.Mode == StateChangeMode.Revert)
                    {
                        var viewState = (ViewStates)args.OriginalState;
                        if (viewState != view.Visibility)
                        {
                            args.AnimationState.Create("show", view.Animate().Alpha(viewState == ViewStates.Visible ? 1 : 0)
                                .SetDuration(Animation.ViewAnimationConsts.FastDuration), () =>
                                {
                                    view.Visibility = viewState;
                                });
                        }
                    }
                },
                (view) => view.Visibility);

            }
            else
                return member.WithProperty(x => x.Visibility).Set(ViewStates.Visible);
        }

        /// <summary>
        /// Sets the alpha of the view
        /// </summary>
        /// <param name="alpha">The target alpha value</param>
        public static StateMemberDefinitionWrapper<T> SetAlpha<T>(this StateMemberDefinitionWrapper<T> member, float alpha) where T : View
        {
            return member.WithProperty(x => x.Alpha).Set(alpha);
        }

        /// <summary>
        /// Sets the view as selected
        /// </summary>
        /// <param name="selected">If True, sets the view as selected else otherwise</param>
        public static StateMemberDefinitionWrapper<T> SetSelected<T>(this StateMemberDefinitionWrapper<T> member, bool selected) where T : View
        {
            return member.WithProperty(x => x.Selected).Set(selected);
        }

    }
}