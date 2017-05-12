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
using Android.Util;
using Android.Support.V4.View;
using Android.Gestures;
using MuggPet.App.Activity;

namespace MuggTester
{
    [Activity(Label = "Gesture-Playground", Theme = "@style/Theme.AppCompat.Light.DarkActionBar" , MainLauncher = false)]
    public class GesturePlayground : BaseActivity , GestureDetector.IOnGestureListener
    {
        GestureDetectorCompat gestureDetector;

        public GesturePlayground() : base(Resource.Layout.Main)
        {

        }

        public bool OnDown(MotionEvent e)
        {
            return true;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            Log.Debug("Fling", $"VelX: {velocityX} , VelY: {velocityY}");
            return true;
        }

        public void OnLongPress(MotionEvent e)
        {

        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return false;
        }

        public void OnShowPress(MotionEvent e)
        {
            
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return true;
        }

        protected override void OnLoaded()
        {
            gestureDetector = new GestureDetectorCompat(this,this);
            //var view = new GestureOverlayView(this);
            //view.EventsInterceptionEnabled = false;
            //var lParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            
            //AddContentView(view, lParams);
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            gestureDetector.OnTouchEvent(ev);
            return base.DispatchTouchEvent(ev);
        }
    }
}