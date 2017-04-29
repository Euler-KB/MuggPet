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
using Betwixt;
using Android.Animation;

namespace MuggPet.Animation
{
    /// <summary>
    /// A simplified wrapper for animation listener
    /// </summary>
    public class AnimationListener : Java.Lang.Object, Animator.IAnimatorListener
    {
        private Action<Animator> onEnd, onCancel, onRepeat, onStart;
        public AnimationListener(Action<Animator> endAction = null, Action<Animator> cancelAction = null, Action<Animator> repeatAction = null, Action<Animator> startAction = null)
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
        BounceOutwards,
        BounceInwards
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
        public static View AnimateFadeAlphaTo(this View view, float target = 1, long duration = 400, long delay = 0, float? from = null, EaseFunc easeFunc = null)
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
        public static View AnimateScaleEffect(this View view, ScaleEffect scaleEffect, int duration = 450, int delay = 20)
        {
            switch (scaleEffect)
            {
                case ScaleEffect.BounceInwards:
                    return view.AnimateScale(1.8F, 1F, 1.8F, 1F, duration, delay, easeFunc: Ease.Quint.Out);
                case ScaleEffect.BounceOutwards:
                    return view.AnimateScale(1F, 1.8F, 1F, 1.8F, duration, delay, easeFunc: Ease.Quint.Out);
            }

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="fromXScale"></param>
        /// <param name="toXScale"></param>
        /// <param name="fromYScale"></param>
        /// <param name="toYScale"></param>
        /// <param name="duration"></param>
        /// <param name="delay"></param>
        /// <param name="isReversed"></param>
        /// <param name="easeFunc"></param>
        /// <returns>Returns the orginal view. For fluid syntax usage.</returns>
        public static View AnimateScale(this View view, float fromXScale, float toXScale, float fromYScale, float toYScale, long duration = 400, long delay = 0, bool isReversed = false, EaseFunc easeFunc = null)
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
        /// <param name="offset">The</param>
        /// <param name="direction"></param>
        /// <param name="duration"></param>
        /// <param name="delay"></param>
        /// <param name="animateAlpha"></param>
        /// <param name="isReversed"></param>
        /// <param name="interpolator"></param>
        /// <returns>Returns the orginal view. For fluid syntax usage.</returns>
        public static View AnimateSlide(this View view, float offset, SlideInDirection direction = SlideInDirection.Top, long duration = 400, long delay = 0, bool animateAlpha = true, bool isReversed = false, BetwixtInterpolator interpolator = null)
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