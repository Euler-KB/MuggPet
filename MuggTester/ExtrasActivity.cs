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
using MuggPet.Binding;
using Android.Support.V4.View;
using Android.Support.V4.App;
using Java.Lang;
using MuggPet.Commands;
using MuggPet.App;
using MuggPet.Tasks.Native;
using MuggPet.Utils;
using MuggPet.App.Activity;

namespace MuggTester
{
    [Activity(Label = "Extras Activity", Theme = "@style/AppTheme", MainLauncher = false)]
    public class ExtrasActivity : BaseActivity
    {
        public interface ITabPage
        {
            string Title { get; }
        }

        public abstract class TabBase : BaseFragment, ITabPage
        {
            string title;
            public TabBase(int fragmentLayout, string title) : base(fragmentLayout)
            {
                this.title = title;
            }

            public string Title => title;
        }

        public class ViewPagerAdapter : FragmentStatePagerAdapter
        {
            IList<TabBase> tabs;
            public ViewPagerAdapter(Android.Support.V4.App.FragmentManager fragmentManager, IList<TabBase> tabs) : base(fragmentManager)
            {
                this.tabs = tabs;
            }

            public override int Count => tabs.Count;

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                return tabs[position];
            }

            public override ICharSequence GetPageTitleFormatted(int position)
            {
                return new Java.Lang.String(tabs[position].Title);
            }

        }

        public class Tab1 : TabBase
        {
            private ICommand captureCommand;

            [BindID(Resource.Id.myImageView)]
            ImageView myImageView = null;

            [BindID(Resource.Id.btnCapture)]
            [BindID(Resource.Id.btnTakePicture)]
            ICommand CaptureCommand
            {
                get
                {
                    return captureCommand ?? (captureCommand = new RelayCommand(async (args) =>
                    {
                        CameraCaptureTask capture = new CameraCaptureTask();
                        var result = await capture.Execute();
                        if (result.Succeeded)
                        {
                            myImageView.SetImageBitmap(result.Result);
                        }

                    }));
                }
            }

            public Tab1() : base(Resource.Layout.tab1, "Home")
            {

            }

            protected override void OnLoaded()
            {

            }

        }

        public class Tab2 : TabBase
        {
            public Tab2() : base(Resource.Layout.tab2,"Callbacks")
            {

            }
        }

        [BindID(Resource.Id.tabLayout)]
        Android.Support.Design.Widget.TabLayout tabLayout = null;

        [BindID(Resource.Id.view_pager)]
        ViewPager viewPager = null;

        public ExtrasActivity() : base(Resource.Layout.extras, closeMethod: CloseMethod.System)
        {

        }

        protected override void OnLoaded()
        {
            //
            viewPager.Adapter = new ViewPagerAdapter(SupportFragmentManager, new TabBase[]
            {
                new Tab1(),
                new Tab2()
            });

            //
            tabLayout.SetupWithViewPager(viewPager);

        }
    }
}