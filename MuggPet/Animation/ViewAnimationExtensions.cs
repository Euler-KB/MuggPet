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
using Android.Animation;
using MuggPet.Animation.Betwixt;

namespace MuggPet.Animation
{
    /// <summary>
    /// A simplified wrapper for animation listener
    /// </summary>
    public class AnimationListener : Java.Lang.Object, Animator.IAnimatorListener
    {
        private Action<Animator> onEnd, onCancel, onRepeat, onStart;

        /// <summary>
        /// Initializes a new animation listener with specified callback handlers.
        /// Note: Only callbacks desired to be handled should be suplied
        /// </summary>
        /// <param name="startAction">The callback for animation start</param>
        /// <param name="endAction">The callback for animation end</param>
        /// <param name="cancelAction">The callback for animation cancel</param>
        /// <param name="repeatAction">The callback for animation repeat</param>
        public AnimationListener(Action<Animator> startAction = null, Action<Animator> endAction = null, Action<Animator> cancelAction = null, Action<Animator> repeatAction = null)
        {
            this.onEnd = endAction;
            this.onCancel = cancelAction;
            this.onRepeat = repeatAction;
            this.onStart = startAction;
        }

        public void OnAnimationCancel(Animator animation)
        {
            onCancel?.Invoke(animation);
        }

        public void OnAnimationEnd(Animator animation)
        {
            onEnd?.Invoke(animation);
        }

        public void OnAnimationRepeat(Animator animation)
        {
            onRepeat?.Invoke(animation);
        }

        public void OnAnimationStart(Animator animation)
        {
            onStart?.Invoke(animation);
        }
    }

    /// <summary>
    /// Specifies the direction of entry for slide in animations
    /// </summary>
    public enum SlideInDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }

    /// <summary>
    /// Specifies a scale effect animation to be applied to a view
    /// </summary>
    public enum ScaleEffect
    {
        /// <summary>
        /// Causes an outward bounce scale effect
        /// </summary>
        BounceOutwards,

        /// <summary>
        /// Causes an inward bound scale effect
        /// </summary>
        BounceInwards
    }

    /// <summary>
    /// Provides predetermined animation constants for various view animations
    /// </summary>
    public static class ViewAnimationConsts
    {
        /// <summary>
        /// The duration for super fast animation playback
        /// </summary>
        public const long SuperFastDuration = 100;

        /// <summary>
        /// The duration for fast animation playback
        /// </summary>
        public const long FastDuration = 400;
        
        /// <summary>
        /// The duration for slow animation playback
        /// </summary>
        public const long SlowDuration = 600;

        /// <summary>
        /// The alpha value for opaque
        /// </summary>
        public const float AlphaOpaque = 1;

        /// <summary>
        /// The alpha value for transparent
        /// </summary>
        public const float AlphaTransparent = 0;

        /// <summary>
        /// Indicates no delay
        /// </summary>
        public const int NoDelay = 0;

        //  Bounce inward default initial scale value
        public const float BounceInwardScale = 1.8F;

        //  Bounce outward default target scale
        public const float BounceOutwardScale = 1.8F;

        /// <summary>
        /// The standard scale value with no deviation
        /// </summary>
        public const float ScaleNormal = 1.0F;
    }

    /// <summary>
    /// Provides various extensions for view animations
    /// </summary>
    public static class ViewAnimationExtensions
    {
        /// <summary>
        /// Fades a view's alpha value to a specified target
        /// </summary>
        /// <param name="view">The view to fade</param>
        /// <param name="target">The target or destination alpha value</param>
        /// <param name="duration">The duration of the fade</param>
        /// <param name="delay">The delay before beginning fade</param>
        /// <param name="from">Indicates the starting fade value. If null, the current fade of the view is used</param>
        /// <param name="easeFunc">The ease function for the fade animation</param>
        /// <returns>Returns the orginal view. For fluid syntax usage.</returns>
        public static View AnimateFadeAlphaTo(this View view, float target = ViewAnimationConsts.AlphaOpaque, long duration = ViewAnimationConsts.FastDuration, long delay = ViewAnimationConsts.NoDelay, float? from = null, EaseFunc easeFunc = null)
        {
            var animator = view.Animate();
            animator.SetDuration(duration);
            animator.SetStartDelay(delay);

            if (from != null)
                view.Alpha = from.Value;

            animator.Alpha(target);
            animator.SetInterpolator(easeFunc == null ? BetwixtInterpolator.Default : new BetwixtInterpolator(easeFunc));
            return view;
        }

        /// <summary>
        /// Animates a view with a scale effect
        /// </summary>
        /// <param name="view">The view to animate</param>
        /// <param name="scaleEffect">The scale effect to apply</param>
        /// <param name="duration">The duration of the scale effect</param>
        /// <param name="delay">The delay</param>
        /// <returns>Returns the orginal view. For fluid syntax usage.</returns>
        public static View AnimateScaleEffect(this View view, ScaleEffect scaleEffect, long duration = ViewAnimationConsts.FastDuration, int delay = ViewAnimationConsts.NoDelay)
        {
            switch (scaleEffect)
            {
                case ScaleEffect.BounceInwards:
                    return view.AnimateScale(ViewAnimationConsts.BounceInwardScale, ViewAnimationConsts.ScaleNormal, ViewAnimationConsts.BounceInwardScale , ViewAnimationConsts.ScaleNormal, duration, delay, easeFunc: Ease.Quint.Out);
                case ScaleEffect.BounceOutwards:
                    return view.AnimateScale(ViewAnimationConsts.ScaleNormal, ViewAnimationConsts.BounceOutwardScale, ViewAnimationConsts.ScaleNormal, ViewAnimationConsts.BounceOutwardScale, duration, delay, easeFunc: Ease.Quint.Out);
            }

            return view;
        }

        /// <summary>
        /// Animates a view with a scale effect
        /// </summary>
        /// <param name="view">The view to animate</param>
        /// <param name="fromXScale">The initial value for X scale</param>
        /// <param name="toXScale">The final value for X scale</param>
        /// <param name="fromYScale">The initial value for Y scale</param>
        /// <param name="toYScale">The final value for Y scale</param>
        /// <param name="duration">The duration of the scale animation</param>
        /// <param name="delay">The delay before animation</param>
        /// <param name="isReversed">Plays animation in reverse if true</param>
        /// <param name="easeFunc">The ease function for the animation. The default is a quintic ease function</param>
        /// <returns>Returns the orginal view. For fluid syntax usage.</returns>
        public static View AnimateScale(this View view, float fromXScale, float toXScale, float fromYScale, float toYScale, long duration = ViewAnimationConsts.FastDuration, long delay = ViewAnimationConsts.NoDelay, bool isReversed = false, EaseFunc easeFunc = null)
        {
            var animator = view.Animate();
            if (isReversed)
            {
                view.ScaleX = toXScale;
                view.ScaleY = toYScale;
                animator.ScaleX(fromXScale);
                animator.ScaleY(fromYScale);
            }
            else
            {
                view.ScaleX = fromXScale;
                view.ScaleY = fromYScale;
                animator.ScaleX(toXScale);
                animator.ScaleY(toYScale);
            }

            animator.SetStartDelay(delay);
            animator.SetDuration(duration);
            animator.SetInterpolator(easeFunc == null ? BetwixtInterpolator.Default : new BetwixtInterpolator(easeFunc));

            return view;
        }

        /// <summary>
        /// Animates a view with specified sliding effect
        /// </summary>
        /// <param name="view">The view to animate</param>
        /// <param name="offset">The offset to translate. If direction is set to top or down, a Y translation animation is applied else if left or right direction is specified, an X translation animation is applied instead</param>
        /// <param name="direction">The direction of the entry</param>
        /// <param name="duration">The duration of the animation</param>
        /// <param name="delay">The delay before starting animation</param>
        /// <param name="animateAlpha">True to animate alpha with translation</param>
        /// <param name="isReversed">If true, plays animation in reverse else otherwise</param>
        /// <param name="interpolator">The interpolator for the animation</param>
        /// <returns>Returns the orginal view. For fluid syntax usage.</returns>
        public static View AnimateSlide(this View view, float offset, SlideInDirection direction = SlideInDirection.Top, long duration = ViewAnimationConsts.FastDuration, long delay = ViewAnimationConsts.NoDelay, bool animateAlpha = true, bool isReversed = false, BetwixtInterpolator interpolator = null)
        {
            ViewPropertyAnimator animator = view.Animate();
            switch (direction)
            {
                case SlideInDirection.Top:
                    {
                        if (isReversed)
                        {
                            view.TranslationY = 0;
                            animator.TranslationY(-offset);
                        }
                        else
                        {
                            view.TranslationY = -offset;
                            animator.TranslationY(0);
                        }
                    }
                    break;
                case SlideInDirection.Bottom:
                    {
                        if (isReversed)
                        {
                            view.TranslationY = 0;
                            animator.TranslationY(offset);
                        }
                        else
                        {
                            view.TranslationY = offset;
                            animator.TranslationY(0);
                        }
                    }
                    break;
                case SlideInDirection.Left:
                    {
                        if (isReversed)
                        {
                            view.TranslationX = 0;
                            animator.TranslationX(-offset);
                        }
                        else
                        {
                            view.TranslationX = -offset;
                            animator.TranslationX(0);
                        }
                    }
                    break;
                case SlideInDirection.Right:
                    {
                        if (isReversed)
                        {
                            view.TranslationX = 0;
                            animator.TranslationX(offset);
                        }
                        else
                        {
                            view.TranslationX = offset;
                            animator.TranslationX(0);
                        }

                    }
                    break;
            }

            if (animateAlpha)
            {
                if (isReversed)
                {
                    view.Alpha = 1;
                    animator.Alpha(0);
                }
                else
                {
                    view.Alpha = 0;
                    animator.Alpha(1);
                }
            }

            animator.SetStartDelay(delay);
            animator.SetDuration(duration);
            animator.SetInterpolator(interpolator ?? BetwixtInterpolator.Default);

            return view;
        }
    }

}