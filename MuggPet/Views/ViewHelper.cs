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
using System.Threading;

namespace MuggPet.Views
{
    /// <summary>
    /// Supplies various extensions for views 
    /// </summary>
    public static class ViewHelper
    {
        //  For backward compatibility
        static int CurrentViewId = 0;

        /// <summary>
        /// Generates a new view id
        /// </summary>
        public static int NewId
        {
            get
            {
                if ((int)Build.VERSION.SdkInt < 17)
                {
                    Interlocked.Increment(ref CurrentViewId);

                    if (CurrentViewId >= int.MaxValue)
                        CurrentViewId = 0;

                    return CurrentViewId;
                }

                return View.GenerateViewId();
            }
        }

        /// <summary>
        /// Returns all the children of a view grop
        /// </summary>
        /// <param name="viewGroup">The view group to find children from</param>
        public static IList<View> GetChildViews(this ViewGroup viewGroup)
        {
            List<View> views = new List<View>();
            for (int i = 0; i < viewGroup.ChildCount; i++)
                views.Add(viewGroup.GetChildAt(i));

            return views;
        }


        /// <summary>
        /// Searches hierachy for the first instance of the type specified
        /// </summary>
        /// <param name="viewGroup">The view group to find children from</param>
        public static T FindChildViewOfType<T>(this ViewGroup viewGroup) where T : View
        {
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                var view = viewGroup.GetChildAt(i);
                if (view is ViewGroup)
                {
                    var innerView = FindChildViewOfType<T>((ViewGroup)view);
                    if (innerView != null)
                        return innerView;
                }

                if (view is T)
                    return (T)view;
            }

            return null;
        }


        #region View Measurement

        /// <summary>
        /// Measures the width and height of the specified view
        /// </summary>
        /// <param name="view">The view to measure</param>
        /// <param name="width">The width of the measured view</param>
        /// <param name="height">The height of the measured view</param>
        /// <param name="maxHeight">The maximum height for measurement</param>
        /// <param name="maxWidth">The maximum width for measurement</param>
        public static void Measure(this View view, int maxWidth, int maxHeight, out int width, out int height)
        {
            view.Measure(View.MeasureSpec.MakeMeasureSpec(maxWidth, MeasureSpecMode.AtMost),
               View.MeasureSpec.MakeMeasureSpec(maxHeight, MeasureSpecMode.AtMost));

            width = view.MeasuredWidth;
            height = view.MeasuredHeight;
        }

        /// <summary>
        /// Measures the view within the specified layout
        /// </summary>
        /// <param name="layoutResID">The view to measure</param>
        /// <param name="width">The width of the measured view</param>
        /// <param name="height">The height of the measured view</param>
        public static void Measure(this Context context, int layoutResID, int maxWidth, int maxHeight, out int width, out int height)
        {
            var view = LayoutInflater.FromContext(context).Inflate(layoutResID, null, false);
            Measure(view, maxWidth, maxHeight, out width, out height);
        }

        /// <summary>
        /// Measures the width and height of the specified view
        /// </summary>
        /// <param name="view">The view to measure</param>
        /// <param name="width">The width of the measured view</param>
        /// <param name="height">The height of the measured view</param>
        public static void MeasureInfite(this View view, out int width, out int height)
        {
            Measure(view, int.MaxValue, int.MaxValue, out width, out height);
        }

        /// <summary>
        /// Measures the view within the specified layout
        /// </summary>
        /// <param name="layoutResID">The view to measure</param>
        /// <param name="width">The width of the measured view</param>
        /// <param name="height">The height of the measured view</param>
        public static void MeasureInfite(this Context context, int layoutResID, out int width, out int height)
        {
            var view = LayoutInflater.FromContext(context).Inflate(layoutResID, null, false);
            MeasureInfite(view, out width, out height);
        }

        #endregion

    }
}