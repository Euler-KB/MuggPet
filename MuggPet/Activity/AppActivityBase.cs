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
using Android.Support.V7.App;
using System.Timers;
using MuggPet.App;
using System.Threading.Tasks;
using MuggPet.Utils;
using MuggPet.Binding;
using MuggPet.Views;
using System.Reflection;
using MuggPet.Activity.Attributes;
using MuggPet.Commands;
using System.Threading;
using MuggPet.Activity.VisualState;

namespace MuggPet.Activity
{
    /// <summary>
    /// Represents the base activity
    /// </summary>
    public class AppActivityBase : AppCompatActivity, IStartActivityAsync, ISupportBinding, IMenuActionDispatcher, IVisualStateManager
    {
        #region Start Activity Async 

        static int RequestId = 0x0;

        /// <summary>
        /// Generates a new request id
        /// </summary>
        static int NewActivityRequestCode
        {
            get
            {
                var requestId = RequestId++;
                if (RequestId >= int.MaxValue)
                    RequestId = 0;

                return requestId;
            }
        }

        private class ActvityRequestData
        {
            public ManualResetEvent Event { get; set; }

            public ActivityResultState State { get; set; }
        }

        /// <summary>
        /// Holds result states for various requests
        /// </summary>
        private Dictionary<int, ActvityRequestData> resultStates = new Dictionary<int, ActvityRequestData>();

        private async Task<ActivityResultState> InternalStartActivityAsync(Action<int> onStartActivity)
        {
            int requestCode = NewActivityRequestCode;
            ManualResetEvent hEvent = new ManualResetEvent(false);

            resultStates[requestCode] = new ActvityRequestData()
            {
                Event = hEvent
            };

            //  let android do the work
            onStartActivity(requestCode);

            //  wait asynchronously
            await Task.Run(() => hEvent.WaitOne());

            //  get state
            ActivityResultState state = resultStates[requestCode].State;
            resultStates.Remove(requestCode);

            return state;
        }

        public Task<ActivityResultState> StartActivityForResultAsync(Intent intent)
        {
            return InternalStartActivityAsync((requestCode) => StartActivityForResult(intent, requestCode));
        }

        public Task<ActivityResultState> StartActivityForResultAsync(Type activityType)
        {
            return InternalStartActivityAsync((requestCode) => StartActivityForResult(activityType, requestCode));
        }

        public Task<ActivityResultState> StartActivityForResultAsync(Intent intent, Bundle options)
        {
            return InternalStartActivityAsync((requestCode) => StartActivityForResult(intent, requestCode, options));
        }

        #endregion

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultStates.ContainsKey(requestCode))
            {
                //  get frame
                var frame = resultStates[requestCode];

                //  set state
                frame.State = new ActivityResultState(resultCode, data);

                //  release 
                frame.Event.Set();

                return;
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        /// <summary>
        /// The default toast key
        /// </summary>
        public const string DefaultToastId = "Default";

        //
        private string exitMessage;

        /// <summary>
        /// Sets the message to show when the back button is pressed for the first time when close method is 'Delayed'
        /// </summary>
        /// <param name="message">The message to show</param>
        protected void SetExitMessage(string message)
        {
            this.exitMessage = message;
        }

        /// <summary>
        /// Sets or returns the exit message to show when the back button is pressed for the first time when close method is 'Delayed'
        /// </summary>
        public string ExitMessage
        {
            get { return exitMessage; }
            set { SetExitMessage(value); }
        }

        public enum CloseMethod
        {
            /// <summary>
            /// Allows android to determine what to do when the back key is pressed
            /// </summary>
            System,

            /// <summary>
            /// Plays animations before the activity finishes
            /// </summary>
            Delayed,

            /// <summary>
            /// Hides the activity when back key is pressed
            /// </summary>
            HideActivity

        }

        /// <summary>
        /// Specifies the mode of animation
        /// </summary>
        public enum AnimationMode
        {
            /// <summary>
            /// Indicates an animation mode due to activity starting or resuming from suspension
            /// </summary>
            Enter,

            /// <summary>
            /// Indicates an animation mode due to activity beign destroyed
            /// </summary>
            Exit
        }

        //  The resource id of the layout to be loaded
        private int layoutID = -1;

        //  The menu resource id
        private int menuResID = -1;

        //  The delay in milliseconds to apply before closing the activity
        private int exitDelay;

        //  Determines whether the activity is exiting
        private bool _isExiting = false;

        //  Determines whether a close has been requested already
        private bool closeRequested = false;

        //  Determines whether the activity is created
        private bool _created = false;

        //  Used in checking for exit time ranges
        private System.Timers.Timer closeResetTimer;

        //  The mode of close for the activity
        private CloseMethod closeMethod;

        //  Handles sequential toast display
        private ToastManager toastManager;

        /// <summary>
        /// Manages the showing of toasts in a sequential manner
        /// </summary>
        public ToastManager ToastManager
        {
            get
            {
                if (toastManager == null)
                    toastManager = new ToastManager(this);

                return toastManager;
            }
        }

        protected void ShowToast(Toast toast, string key)
        {
            ToastManager.ShowToast(key, toast);
        }

        /// <summary>
        /// Toasts message  with specified text
        /// </summary>
        /// <param name="text">The message to toast</param>
        /// <param name="length">The duration of the toast</param>
        /// <param name="key">The key group for the toast</param>
        /// <param name="gravity">Gravity flags for adjusting the toast's position on screen. If null, no gravity is applied</param>
        protected void ShowToast(string text, ToastLength length = ToastLength.Short, string key = DefaultToastId, GravityFlags? gravity = null)
        {
            ToastManager.Show(key, text, length, gravity);
        }

        /// <summary>
        /// Toasts message  with specified message resource id
        /// </summary>
        /// <param name="textId">The text's resource id</param>
        /// <param name="length">The duration of the toast</param>
        /// <param name="key">The key group for the toast</param>
        /// <param name="gravity">Gravity flags for adjusting the toast's position on screen. If null, no gravity is applied</param>
        protected void ShowToast(int textId, ToastLength length = ToastLength.Short, string key = DefaultToastId, GravityFlags? gravity = null)
        {
            ShowToast(GetString(textId), length, key, gravity);
        }

        /// <summary>
        /// Shows a progress dialog with specified title and message
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <param name="indeterminate">True for indeterminate progress bar else otherwise</param>
        protected IDisposable ShowProgress(string title, string message, bool indeterminate = true)
        {
            return ProgressDialog.Show(this, title, message, indeterminate).Scope();
        }

        /// <summary>
        /// Displays a progress dialog with specified title and message
        /// </summary>
        /// <param name="titleMsgResId">The resource id of the message for the title of the progress dialog</param>
        /// <param name="msgResId">The resource id of the message for the content of the progress dialog</param>
        /// <param name="indeterminate">True for indeterminate progress bar else otherwise</param>
        protected IDisposable ShowProgress(int titleMsgResId, int msgResId, bool indeterminate = true)
        {
            return ShowProgress(GetString(titleMsgResId), GetString(msgResId), indeterminate);
        }

        /// <summary>
        /// Initializes a new activity with its layout and other functionality properties
        /// </summary>
        /// <param name="resLayoutID">The layout resource id for the activity. If set to -1, no content is loaded.</param>
        /// <param name="exitDelay">The delay prior exiting activity</param>
        /// <param name="closeInterval">The delay between double back presses that will prevent activity from exiting</param>
        /// <param name="menuResourceID">The menu resource id for the activity</param>
        /// <param name="closeMethod">Determines how back bressed event is handled</param>
        public AppActivityBase(int resLayoutID, int exitDelay = 320, int closeInterval = 2100, int menuResourceID = -1, CloseMethod closeMethod = CloseMethod.System)
        {
            this.layoutID = resLayoutID;
            this.exitDelay = exitDelay;
            this.menuResID = menuResourceID;

            closeResetTimer = new System.Timers.Timer(closeInterval) { AutoReset = true };
            this.closeMethod = closeMethod;
            closeResetTimer.Elapsed += (s, e) =>
            {
                if (closeRequested)
                    closeRequested = false;
            };
        }

        /// <summary>
        /// Initializes a blank activity without any properties
        /// </summary>
        public AppActivityBase()
        {

        }

        /// <summary>
        /// Determines whether the user has already pressed the back button once when close method is 'Delayed'
        /// </summary>
        protected bool IsCloseRequested
        {
            get { return closeRequested; }
        }

        IBindingHandler bindingHandler;

        /// <summary>
        /// Gets the binding handler instance for this activity
        /// </summary>
        public IBindingHandler BindingHandler
        {
            get
            {
                return bindingHandler ?? (bindingHandler = new BindingHandler());
            }
        }

        VisualStateManager stateManager;

        /// <summary>
        /// Gets the visual state manager for this activity
        /// </summary>
        public VisualStateManager VisualState
        {
            get
            {
                return stateManager ?? (stateManager = new VisualStateManager((ViewGroup)this.FindViewById(Android.Resource.Id.Content)));
            }
        }

        protected AppActivityBase(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            //
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            //  
            if (savedInstanceState != null)
                OnRestoreActivityState(savedInstanceState);

            //  let the base handle creation of activity
            base.OnCreate(savedInstanceState);

            //  set activity created
            _created = true;


            // bind views here
            if (layoutID != -1)
            {
                //  
                await OnBind();

                //
                OnHandleVisualStates();

                //  Loaded 
                OnLoaded();

                //  Setup initial animations
                OnSetupAnimations(AnimationMode.Enter);
            }

        }

        private void OnHandleVisualStates()
        {
            //  begin state definition
            VisualState.BeginStateDefinition();

            //  Define visual states
            OnDefineVisualStates();

            //  finalize state definition
            VisualState.FinalizeStateDefinition();
        }

        protected virtual void OnDefineVisualStates()
        {
           //   TODO: Override and define visual states for activity here
        }

        protected virtual Task OnBind()
        {
            //  load content view
            SetContentView(layoutID);

            //  attach views
            this.AttachViews();

            //
            return Task.FromResult(0);
        }


        /// <summary>
        /// Handles restoring activity state
        /// </summary>
        /// <param name="savedInstanceState">The bundle containing persisted data</param>
        protected virtual void OnRestoreActivityState(Bundle savedInstanceState)
        {

        }

        /// <summary>
        /// Determines whether the activity can be closed at the time of call
        /// </summary>
        /// <returns>True if can be closed else otherwise</returns>
        protected virtual bool OnCanClose()
        {
            return true;
        }

        protected virtual void OnFinalizeClose()
        {
            this.ExecuteDelayed(exitDelay, delegate
            {
                OverridePendingTransition(0, Resource.Animation.abc_slide_out_bottom);
                Finish();
            });
        }

        /// <summary>
        /// Handles the back key pressed
        /// </summary>
        /// <returns>True if handled else otherwise</returns>
        protected virtual bool HandleBackPressed()
        {
            // Determine whether we can close activity with back button ?
            if (!OnCanClose())
                return true;

            switch (closeMethod)
            {
                case CloseMethod.HideActivity:
                    MoveTaskToBack(true);
                    return true;

                case CloseMethod.Delayed:
                    {
                        if (!closeRequested)
                        {
                            //
                            closeRequested = true;
                            closeResetTimer.Start();

                            //
                            ShowToast(exitMessage ?? "Press back once again to quit application.", ToastLength.Short, "Misc");
                        }
                        else
                        {
                            //  are we already in exiting state?
                            if (_isExiting)
                                return true;

                            //  put us in the exiting state
                            _isExiting = true;
                            closeResetTimer.Stop();

                            //  play exit animations
                            OnSetupAnimations(AnimationMode.Exit);

                            //  
                            OnFinalizeClose();
                        }

                        return true;
                    }
                case CloseMethod.System:
                    //  leave for system to handle
                    break;
            }

            return false;
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            //  ensure back key is pressed for only the first time
            if (e.Action == KeyEventActions.Down && e.KeyCode == Keycode.Back && e.RepeatCount == 0)
            {
                if (HandleBackPressed())
                    return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        protected virtual void OnHomeButtonPressed()
        {
            Finish();
        }

        public bool DispatchSelected(int itemID)
        {
            //
            bool executed = false;

            foreach (var member in GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Method))
            {
                var menuAction = member.GetCustomAttribute<MenuActionAttribute>();
                if (menuAction == null || itemID != menuAction.ID)
                    continue;

                if (member.MemberType == MemberTypes.Method)
                {
                    var mInfo = ((MethodInfo)member);
                    mInfo.Invoke(this, (mInfo.GetParameters().Length > 0) ? new object[] { itemID } : null);
                    executed = true;
                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    var fieldInfo = ((FieldInfo)member);
                    var val = fieldInfo.GetValue(this);
                    if (val is ICommand)
                    {
                        var cmd = (ICommand)val;
                        if (cmd.CanExecute(itemID))
                            cmd.Execute(itemID);

                        executed = true;
                    }
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    var propertyInfo = ((PropertyInfo)member);
                    var val = propertyInfo.GetValue(this);
                    if (val is ICommand)
                    {
                        var cmd = (ICommand)val;
                        if (cmd.CanExecute(itemID))
                            cmd.Execute(itemID);

                        executed = true;
                    }
                }
            }

            return executed;
        }

        protected virtual bool OnDispatchMenuItemSelected(IMenuItem item)
        {
            return DispatchSelected(item.ItemId);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            //  Dispatch selection action
            bool dispatched = OnDispatchMenuItemSelected(item);

            //  Did we hit the home button
            if (item.ItemId == Android.Resource.Id.Home)
            {
                OnHomeButtonPressed();
                return true;
            }

            //  was message dispatched ??
            return dispatched ? true : base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Plays animation
        /// </summary>
        /// <param name="mode">The mode of animations to play</param>
        protected virtual void OnSetupAnimations(AnimationMode mode)
        {
            //  TODO: Setup animation here

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            //  load menu from resource
            if (menuResID != -1)
            {
                MenuInflater.Inflate(menuResID, menu);
            }

            return base.OnCreateOptionsMenu(menu);
        }

        protected Android.Support.V7.Widget.Toolbar CreateSupportToolbar(int layoutID, int toolbarId, ViewGroup parent = null, bool homeAsUpAndEnabled = false)
        {
            if (parent == null)
            {
                //  get the first view group from the view
                parent = ((ViewGroup)FindViewById(Android.Resource.Id.Content))
                    .FindChildViewOfType<ViewGroup>();
            }

            //
            var toolbar = ((ViewGroup)LayoutInflater.Inflate(layoutID, null, false))
                .FindViewById<Android.Support.V7.Widget.Toolbar>(toolbarId);

            //
            toolbar.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);

            //  move view to
            parent.AddView(toolbar, 0);

            //  
            SetSupportActionBar(toolbar);

            //
            SupportActionBar.SetDisplayHomeAsUpEnabled(homeAsUpAndEnabled);

            return toolbar;
        }

        protected Android.Support.V7.Widget.Toolbar AttachSupportToolbar(bool homeAsUpAndEnabled = false)
        {
            return AttachSupportToolbar(Resource.Id.support_toolbar, homeAsUpAndEnabled);
        }

        protected Android.Support.V7.Widget.Toolbar AttachSupportToolbar(int toolbarId, bool homeAsUpAndEnabled = false)
        {
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(toolbarId);
            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(homeAsUpAndEnabled);
            }

            return toolbar;
        }

        /// <summary>
        /// Called when the activity is created and has (if available) inflated the content
        /// </summary>
        protected virtual void OnLoaded()
        {
            //  TODO: Override and handle events and other stuffs when the view is setup

        }


        /// <summary>
        /// Navigate to the activity with the type specified
        /// </summary>
        public virtual void Navigate(Type activity, int delay = 0, bool popCurrent = false, Bundle bundleExtra = null, int enterAnim = 0, int exitAnim = 0)
        {
            Navigate(new Intent(this, activity), delay, popCurrent, bundleExtra, enterAnim, exitAnim);
        }

        /// <summary>
        /// Navigate to the activity with the intent specified
        /// </summary>
        public virtual async void Navigate(Intent intent, int delay = 0, bool popCurrent = false, Bundle bundleExtra = null, int enterAnim = 0, int exitAnim = 0)
        {
            if (!_created)
                base.OnCreate(null);

            //  play exit animations
            if (delay > 0)
            {
                OnSetupAnimations(AnimationMode.Exit);
                await Task.Delay(delay);
            }

            if (bundleExtra != null)
                intent.PutExtras(bundleExtra);

            //
            if (popCurrent)
            {
                if (enterAnim != 0 || exitAnim != 0)
                    OverridePendingTransition(enterAnim, exitAnim);

                Finish();
            }
            else
            {
                if (enterAnim != 0 || exitAnim != 0)
                    OverridePendingTransition(enterAnim, exitAnim);
            }

            //  start activity
            StartActivity(intent);

        }

    }
}