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
using MuggPet.Commands;
using System.Threading;
using MuggPet.Views.VisualState;
using MuggPet.App.Activity.Attributes;
using Android.Content.PM;

namespace MuggPet.App.Activity
{
    /// <summary>
    /// Represents the base activity
    /// </summary>
    public class BaseActivity : AppCompatActivity, IStartActivityAsync, ISupportBinding, IMenuActionDispatcher, IVisualStateManager, IRequestPermissionAsync
    {
        #region Consts

        //  The default interval between double back presses that will close the activity
        const int DefaultCloseInterval = 2100;

        //  Represents an invalid resource identifier
        const int InvalidResourceId = -1;

        //  The default exit animation duration
        const int DefaultExitAnimDuration = 320;

        //  The default toast key
        const string DefaultToastId = "Default";

        const string DefaultExitMessage = "Press back once again to quit!";

        #endregion


        #region Activity Async 

        static int RequestId = 0x0;

        /// <summary>
        /// Generates a new request code
        /// </summary>
        static int NewRequestCode
        {
            get
            {
                var requestId = Interlocked.Increment(ref RequestId);
                if (RequestId >= int.MaxValue)
                    RequestId = 0;

                return requestId;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (resultStates.ContainsKey(requestCode))
            {
                var perm = resultStates[requestCode] as PermissionRequestData;

                //
                perm.State = new PermissionGrantResultState()
                {
                    GrantResults = grantResults,
                    Permissions = permissions
                };

                //  release
                perm.Event.Set();
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private abstract class RequestDataBase
        {
            public ManualResetEvent Event { get; set; }
        }

        private class ActivityRequestData : RequestDataBase
        {
            public ActivityResultState State { get; set; }
        }

        private class PermissionRequestData : RequestDataBase
        {
            public PermissionGrantResultState State { get; set; }
        }

        /// <summary>
        /// Holds result states for various requests
        /// </summary>
        private Dictionary<int, RequestDataBase> resultStates = new Dictionary<int, RequestDataBase>();

        private async Task<T> InternalBeginAsyncOperation<T>(Action<int> onBegin) where T : RequestDataBase
        {
            int requestCode = NewRequestCode;
            ManualResetEvent hEvent = new ManualResetEvent(false);

            //
            var request = Activator.CreateInstance<T>();
            request.Event = hEvent;
            resultStates.Add(requestCode, request);

            //  
            onBegin(requestCode);

            //  wait asynchronously
            await Task.Run(() => hEvent.WaitOne());

            //  
            T state = (T)resultStates[requestCode];

            resultStates.Remove(requestCode);

            return state;
        }

        /// <summary>
        /// Requests for permission asynchronously
        /// </summary>
        /// <param name="permissions">A collection of permissions to request</param>
        public async Task<PermissionGrantResultState> RequestPermissionAsync(string[] permissions)
        {
            return (await InternalBeginAsyncOperation<PermissionRequestData>((requestCode) => RequestPermissions(permissions, requestCode))).State;
        }

        /// <summary>
        /// Starts an activity for result asynchronously
        /// </summary>
        /// <param name="intent">The intent to start the activity</param>
        public async Task<ActivityResultState> StartActivityForResultAsync(Intent intent)
        {
            return (await InternalBeginAsyncOperation<ActivityRequestData>((requestCode) => StartActivityForResult(intent, requestCode))).State;
        }

        /// <summary>
        /// Starts an activity for result asynchronously
        /// </summary>
        /// <param name="activityType">The type of the activity to start</param>
        public async Task<ActivityResultState> StartActivityForResultAsync(Type activityType)
        {
            return (await InternalBeginAsyncOperation<ActivityRequestData>((requestCode) => StartActivityForResult(activityType, requestCode))).State;
        }

        /// <summary>
        /// Starts an activity for result asynchronously
        /// </summary>
        /// <param name="intent">The intent to start the activity</param>
        /// <param name="options">Extra options for the intent</param>
        public async Task<ActivityResultState> StartActivityForResultAsync(Intent intent, Bundle options)
        {
            return (await InternalBeginAsyncOperation<ActivityRequestData>((requestCode) => StartActivityForResult(intent, requestCode, options))).State;
        }

        #endregion

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultStates.ContainsKey(requestCode))
            {
                //  get frame
                var frame = resultStates[requestCode] as ActivityRequestData;

                //  set state
                frame.State = new ActivityResultState(resultCode, data);

                //  release 
                frame.Event.Set();

                return;
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }



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
        private int layoutID = InvalidResourceId;

        //  The menu resource id
        private int menuResID = InvalidResourceId;

        //  The delay in milliseconds to apply before closing the activity
        private int exitAnimDuration = DefaultExitAnimDuration;

        //  Determines whether the activity is exiting
        private bool _isExiting = false;

        //  Determines whether a close has been requested already
        private bool closeRequested = false;

        //  Determines whether the activity is created
        private bool _created = false;

        //  Used in checking for exit time ranges
        private System.Timers.Timer closeResetTimer;

        //  The mode of close for the activity
        private CloseMethod closeMethod = CloseMethod.System;

        #region Toast & Dialogs

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

        /// <summary>
        /// Shows the toast with give key group
        /// </summary>
        /// <param name="toast">The toast to show</param>
        /// <param name="key">The key group for the toast</param>
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
        /// <param name="cancelable">True to enable Back key dismiss the dialog</param>
        protected IDisposable ShowProgress(string title, string message, bool indeterminate = true, bool cancelable = false)
        {
            return ProgressDialog.Show(this, title, message, indeterminate, cancelable).Scope();
        }

        /// <summary>
        /// Shows a progress dialog with no title and message
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <param name="indeterminate">True for indeterminate progress bar else otherwise</param>
        /// <param name="cancelable">True to enable Back key dismiss the dialog</param>
        protected IDisposable ShowProgress(string message, bool indeterminate = true, bool cancelable = false)
        {
            return ProgressDialog.Show(this, null, message, indeterminate, cancelable).Scope();
        }

        /// <summary>
        /// Displays a progress dialog with specified title and message
        /// </summary>
        /// <param name="titleMsgResId">The resource id of the message for the title of the progress dialog</param>
        /// <param name="msgResId">The resource id of the message for the content of the progress dialog</param>
        /// <param name="indeterminate">True for indeterminate progress bar else otherwise</param>
        /// <param name="cancelable">True to enable Back key dismiss the dialog</param>
        protected IDisposable ShowProgress(int titleMsgResId, int msgResId, bool indeterminate = true, bool cancelable = false)
        {
            return ShowProgress(GetString(titleMsgResId), GetString(msgResId), indeterminate, cancelable);
        }

        #endregion


        /// <summary>
        /// Initializes a new activity with its layout and other functionality properties
        /// </summary>
        /// <param name="resLayoutID">The layout resource id for the activity. If set to -1, no content is loaded.</param>
        /// <param name="exitAnimDuration">The delay prior exiting activity</param>
        /// <param name="closeInterval">The delay between double back presses that will prevent activity from exiting</param>
        /// <param name="menu">The menu resource id for the activity. If set to -1, no menu resouce is loaded</param>
        /// <param name="closeMethod">Determines how back pressed event is handled</param>
        public BaseActivity(int resLayoutID, int exitAnimDuration = DefaultExitAnimDuration, int closeInterval = DefaultCloseInterval, int menu = InvalidResourceId, CloseMethod closeMethod = CloseMethod.System)
        {
            this.layoutID = resLayoutID;
            this.exitAnimDuration = exitAnimDuration;
            this.menuResID = menu;
            this.closeMethod = closeMethod;

            //
            InitializeActivity(closeInterval);
        }

        /// <summary>
        /// Initializes a blank activity without any properties
        /// </summary>
        public BaseActivity()
        {
            InitializeActivity(DefaultCloseInterval);
        }

        private void InitializeActivity(int closeInterval)
        {
            closeResetTimer = new System.Timers.Timer(closeInterval) { AutoReset = true };
            closeResetTimer.Elapsed += (s, e) =>
            {
                if (closeRequested)
                    closeRequested = false;
            };
        }

        /// <summary>
        /// Determines whether the user has already pressed the back button once when close method is 'Delayed'
        /// </summary>
        protected bool IsCloseRequested
        {
            get { return closeRequested; }
        }

        private IBindingHandler bindingHandler;

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

        protected BaseActivity(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        /// <summary>
        /// Gets the layout to inflate the activity
        /// </summary>
        protected virtual int LayoutId
        {
            get
            {
                return layoutID;
            }
        }

        /// <summary>
        /// Gets the menu resource for the activity
        /// </summary>
        protected virtual int MenuId
        {
            get
            {
                return menuResID;
            }
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            //
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            //  let the base handle creation of activity
            base.OnCreate(savedInstanceState);

            //  set activity created
            _created = true;

            // bind views here
            if (LayoutId != -1)
            {
                //  
                await OnBind();
            }

            //  Define visual states
            OnHandleVisualStates();

            //  Handle loaded logic
            OnLoaded();

            //  Setup initial animations
            OnSetupAnimations(AnimationMode.Enter);
        }

        private void OnHandleVisualStates()
        {
            //  begin state definition
            VisualState.BeginStateDefinition();

            try
            {
                //  Define visual states
                OnDefineVisualStates();
            }
            finally
            {
                //  finalize state definition
                VisualState.FinalizeStateDefinition();
            }
        }

        protected virtual void OnDefineVisualStates()
        {
            //   TODO: Override and define visual states for activity here

        }

        protected virtual Task OnBind()
        {
            //  load content view
            SetContentView(LayoutId);

            //  attach views
            this.AttachViews();

            //
            return Task.FromResult(0);
        }

        /// <summary>
        /// Determines whether the activity can be closed at the time of call
        /// </summary>
        /// <returns>True if can be closed else otherwise</returns>
        protected virtual bool OnCanClose()
        {
            //  are we already in exiting state?
            if (_isExiting)
                return false;

            return true;
        }

        protected virtual void OnFinalizeClose()
        {
            this.ExecuteDelayed(exitAnimDuration, delegate
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
                            ShowToast(exitMessage ?? DefaultExitMessage , ToastLength.Short, "Misc");
                        }
                        else
                        {
                            //  put us in the exiting state
                            _isExiting = true;
                            closeResetTimer.Stop();

                            //  play exit animations
                            OnSetupAnimations(AnimationMode.Exit);

                            //  sets up the delay to
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

        /// <summary>
        /// Invoked when the home button is pressed. 
        /// TODO: Override and implement custom logic
        /// </summary>
        protected virtual void OnHomeButtonPressed()
        {
            //  just finish activity
            Finish();
        }

        #region Command Binding

        /// <summary>
        /// Binds a command directly to the specified view
        /// </summary>
        /// <param name="command">The command to bind</param>
        /// <param name="targetView">The target view</param>
        /// <param name="parameter">An optional parameter for the command</param>
        public void BindCommand(ICommand command, View targetView, object parameter = null)
        {
            BindingHandler.BindCommandDirect(this, command, targetView, parameter, true);
        }

        /// <summary>
        /// Binds a command directly to the specified view
        /// </summary>
        /// <param name="command">The command to bind</param>
        /// <param name="viewID">The target view id</param>
        /// <param name="parameter">An optional parameter for the command</param>
        public void BindCommand(ICommand command, int viewID, object parameter = null)
        {
            BindCommand(command, FindViewById(viewID), parameter);
        }

        #endregion


        /// <summary>
        /// Dispatches the selected menu item id
        /// </summary>
        /// <param name="itemID">The id of the selected menu item</param>
        public bool DispatchSelected(int itemID, bool useContextMenu)
        {
            bool executed = false;
            foreach (var member in GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Method))
            {
                foreach (var menuAction in member.GetCustomAttributes<MenuActionAttribute>())
                {
                    if (menuAction == null || itemID != menuAction.ID || (useContextMenu && !menuAction.UseContextMenu))
                        continue;

                    if (member.MemberType == MemberTypes.Method)
                    {
                        var mInfo = ((MethodInfo)member);
                        var properties = mInfo.GetParameters();
                        mInfo.Invoke(this, (properties.Length > 0) ? new object[] { itemID } : null);
                        executed = true;
                    }
                    else if (member.MemberType == MemberTypes.Field || member.MemberType == MemberTypes.Property)
                    {
                        var val = member.GetMemberValue(this);
                        if (val is ICommand)
                        {
                            var cmd = (ICommand)val;
                            if (cmd.CanExecute(itemID))
                                cmd.Execute(itemID);

                            executed = true;
                        }
                    }
                }
            }

            return executed;
        }

        protected virtual bool OnDispatchMenuItemSelected(IMenuItem item)
        {
            return DispatchSelected(item.ItemId, false);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            //  Dispatch selection action
            bool dispatched = OnDispatchMenuItemSelected(item);

            //  Did we hit the home button
            if (item.ItemId.Equals(Android.Resource.Id.Home))
            {
                OnHomeButtonPressed();
                return true;
            }

            //  was message dispatched ??
            return dispatched ? true : base.OnOptionsItemSelected(item);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            //  Dispatch selected item
            if (DispatchSelected(item.ItemId, true))
                return true;

            return base.OnContextItemSelected(item);
        }

        /// <summary>
        /// Setups enter and exit animations for the activity
        /// </summary>
        protected virtual void OnSetupAnimations(AnimationMode mode)
        {
            //  TODO: Setup animation here

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            //  load menu from resource
            if (MenuId != -1)
            {
                MenuInflater.Inflate(MenuId, menu);
                return true;
            }

            return base.OnCreateOptionsMenu(menu);
        }

        /// <summary>
        /// Sets the support toolbar for this activity
        /// </summary>
        /// <param name="homeAsUpAndEnabled">Shows or disables home button</param>
        protected Android.Support.V7.Widget.Toolbar AttachSupportToolbar(bool homeAsUpAndEnabled = false)
        {
            return AttachSupportToolbar(Resource.Id.support_toolbar, homeAsUpAndEnabled);
        }

        /// <summary>
        /// Sets the support toolbar for this activity
        /// </summary>
        /// <param name="homeAsUpAndEnabled">Shows or disables home button</param>
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
        /// Navigates to the activity with the type specified
        /// </summary>
        public virtual void Navigate(Type activity, int delay = 0, bool popCurrent = false, Bundle bundleExtra = null, int enterAnim = 0, int exitAnim = 0)
        {
            Navigate(new Intent(this, activity), delay, popCurrent, bundleExtra, enterAnim, exitAnim);
        }

        /// <summary>
        /// Navigates to the activity with the intent specified
        /// </summary>
        /// <param name="bundleExtra">Additional extras bundle for the intent</param>
        /// <param name="delay">The override delay</param>
        /// <param name="enterAnim">An optional enter transition animation for the new activity. This parameter is ignored if set to 0</param>
        /// <param name="exitAnim">An optional exit transition animation for the current activity. This parameter is ignored if set to 0</param>
        /// <param name="intent">The intent for the target activity</param>
        /// <param name="popCurrent">If set to true, the current activity will be finished or closed before navigating to the new activity.</param>
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

            //
            if (bundleExtra != null)
                intent.PutExtras(bundleExtra);

            //  Override transition
            if (enterAnim != 0 || exitAnim != 0)
                OverridePendingTransition(enterAnim, exitAnim);

            //
            if (popCurrent)
            {
                Finish();
            }

            //  Start activity
            StartActivity(intent);
        }

    }
}