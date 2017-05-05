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
using Android.Preferences;
using MuggPet.App.Activity;

namespace MuggPet.Preferences
{
    /// <summary>
    /// Represents the base preference activity
    /// </summary>
    public abstract class BasePrefrenceActivity : BaseActivity , ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public class InitializationInfo
        {
            //  The default enter animation
            public int? DefaultEnterAnimation;

            //  The default exit animation
            public int? DefaultExitAnimation;

            //  The default layout for the preference activity
            public int LayoutID;

            //  The id of the view for placing preference fragment
            public int ContentID;

            //  The id of the toolbar
            public int? ToolbarResId;

        }

        protected InitializationInfo initializationInfo;
        public InitializationInfo InitInfo
        {
            get { return initializationInfo; }
        }

        //
        protected class DefaultPreferenceFragment : PreferenceFragment
        {
            private int preferenceResID;
            public DefaultPreferenceFragment(int resID)
            {
                this.preferenceResID = resID;
            }

            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                AddPreferencesFromResource(preferenceResID);
            }
        }

        public virtual void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            //  TODO: Override and implement key changed logic
            
        }

        //  The preference screen
        private int preferenceXml;

        public BasePrefrenceActivity(int preferenceXml) : this(preferenceXml, new InitializationInfo() { LayoutID = Resource.Layout.BasePreferenceLayout, ContentID = Resource.Id.preference_content_main, ToolbarResId = Resource.Id.support_toolbar })
        {

        }

        public BasePrefrenceActivity(int preferenceXml, int activityLayout, int contentId) : this(preferenceXml, new InitializationInfo() { LayoutID = activityLayout, ContentID = contentId })
        {

        }

        public BasePrefrenceActivity(int preferenceXml, InitializationInfo initializationInfo) : base(initializationInfo.LayoutID, closeMethod: CloseMethod.System)
        {
            this.preferenceXml = preferenceXml;
            this.initializationInfo = initializationInfo;
        }

        protected override void OnResume()
        {
            var prefMgr = PreferenceManager.GetDefaultSharedPreferences(this);
            prefMgr.RegisterOnSharedPreferenceChangeListener(this);
            base.OnResume();
        }

        protected override void OnStop()
        {
            PreferenceManager.GetDefaultSharedPreferences(this).UnregisterOnSharedPreferenceChangeListener(this);
            base.OnStop();
        }

        protected override void OnLoaded()
        {
            if (initializationInfo.DefaultEnterAnimation != null && initializationInfo.DefaultExitAnimation != null)
            {
                //  override transition animation
                OverridePendingTransition(initializationInfo.DefaultEnterAnimation.Value, initializationInfo.DefaultExitAnimation.Value);
            }

            //  get support toolbar
            if (initializationInfo.ToolbarResId != null)
            {
                AttachSupportToolbar(initializationInfo.ToolbarResId.Value, true);
            }

            //
            FragmentManager.BeginTransaction().Replace(initializationInfo.ContentID,
                new DefaultPreferenceFragment(preferenceXml))
                .Commit();
        }

    }

}