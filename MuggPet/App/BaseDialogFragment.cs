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
using System.Threading.Tasks;
using MuggPet.Views.VisualState;
using MuggPet.Utils;
using MuggPet.Commands;
using MuggPet.Views;

namespace MuggPet.App
{
    /// <summary>
    /// Represents the base for all dialog fragments
    /// </summary>
    public class BaseDialogFragment : Android.Support.V4.App.DialogFragment, ISupportBinding, IVisualStateManager
    {
        #region Consts

        const string DefaultToastId = "Default";

        #endregion


        //  The id of the layout of the fragment
        private int layoutID = -1;

        //  The text on the positive button
        private string positiveButtonText;

        //  The text on the negative button
        private string negativeButtonText;

        //  The title message
        private string titleText;

        //  The inflated dialog content
        private View contentView;

        //  Determines whether the content is initialized
        private bool isInitialized;

        //  The positive button
        private Button positiveButton;

        //  The negative button
        private Button negativeButton;

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
                    toastManager = new ToastManager(Activity);

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
            return ProgressDialog.Show(Activity, title, message, indeterminate, cancelable).Scope();
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
            return ProgressDialog.Show(Activity, null, message, indeterminate, cancelable).Scope();
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
        /// Gets the layout id for the dialog
        /// </summary>
        protected virtual int LayoutId
        {
            get
            {
                return layoutID;
            }
        }

        /// <summary>
        /// Initializes a new dialog fragment without any properties
        /// </summary>
        public BaseDialogFragment()
        {

        }

        /// <summary>
        /// Initializes a new dialog fragment with the specified layout and 
        /// </summary>
        /// <param name="layoutID">The id of the layout for the dialog</param>
        /// <param name="positiveBtnText">The text on the positive button. If null, the value is ignored</param>
        /// <param name="negativeBtnText">The text on the negative button. If null, the value is ignored</param>
        /// <param name="titleText">The title message</param>
        public BaseDialogFragment(int layoutID, string titleText = null, string positiveBtnText = null, string negativeBtnText = null)
        {
            this.layoutID = layoutID;
            this.positiveButtonText = positiveBtnText;
            this.negativeButtonText = negativeBtnText;
            this.titleText = titleText;
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
            if (contentView != null)
            {
                BindCommand(command, contentView.FindViewById(viewID), parameter);
            }
        }

        #endregion


        private VisualStateManager visualStateManager;

        /// <summary>
        /// Gets the visual state manager for this dialog fragment
        /// </summary>
        public VisualStateManager VisualState
        {
            get { return visualStateManager ?? (visualStateManager = new VisualStateManager(Dialog.FindViewById(Android.Resource.Id.Content) as ViewGroup)); }
        }

        private IBindingHandler bindingHandler;

        /// <summary>
        /// Gets the binding handler for this dialog fragment
        /// </summary>
        public IBindingHandler BindingHandler
        {
            get
            {
                return bindingHandler ?? (bindingHandler = new BindingHandler());
            }
        }

        /// <summary>
        /// Sets the button which will cancel the dialog when clicked
        /// </summary>
        public void SetNegativeButton(int buttonId)
        {
            SetNegativeButton(Dialog.FindViewById<Button>(buttonId));
        }

        /// <summary>
        /// Sets the button which will cancel the dialog when clicked
        /// </summary>
        public void SetNegativeButton(Button button)
        {
            if (negativeButton != null)
                negativeButton.Click -= OnNegativeButtonClicked;

            //
            this.negativeButton = button;
            negativeButton.Click += OnNegativeButtonClicked;
        }

        private void OnNegativeButtonClicked(object sender, EventArgs e)
        {
            OnNegativeButtonClicked();
        }

        /// <summary>
        /// Sets the positive button for the dialog. The dialog will be dismissed if clicked
        /// </summary>
        public void SetPositiveButton(int buttonId)
        {
            SetPositiveButton(Dialog.FindViewById<Button>(buttonId));
        }

        /// <summary>
        /// Sets the positive button for the dialog. The dialog will be dismissed if clicked
        /// </summary>
        public void SetPositiveButton(Button button)
        {
            if (positiveButton != null)
                positiveButton.Click -= OnPositiveButtonClicked;

            this.positiveButton = button;
            positiveButton.Click += OnPositiveButtonClicked;
        }

        private void OnPositiveButtonClicked(object sender, EventArgs e)
        {
            OnPositiveButtonClicked();
        }

        /// <summary>
        /// Invoked when the positive button is clicked
        /// </summary>
        protected virtual void OnPositiveButtonClicked()
        {

        }

        /// <summary>
        /// Invoked when the negative button is clicked
        /// </summary>
        protected virtual void OnNegativeButtonClicked()
        {

        }

        /// <summary>
        /// Responsible for executing view bindings and attachment
        /// </summary>
        /// <returns></returns>
        protected Task OnBind()
        {
            //  get content view
            contentView = contentView ?? View;

            //  attach views
            if (contentView != null)
            {
                this.AttachViews(contentView);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Invoked to configure and create the dialog
        /// </summary>
        protected virtual Dialog OnBuildDialog(AlertDialog.Builder build)
        {
            if (titleText != null)
                build.SetTitle(titleText);

            if (positiveButtonText != null)
                build.SetPositiveButton(positiveButtonText, (s, e) => OnPositiveButtonClicked());

            if (negativeButtonText != null)
                build.SetNegativeButton(negativeButtonText, (s, e) => OnNegativeButtonClicked());

            return build.Create();
        }

        /// <summary>
        /// Initializes the dialog
        /// </summary>
        private async Task InitializeFragment()
        {
            if (isInitialized)
                return;

            //  bind views for fragment
            await OnBind();

            //
            HandleVisualStates();

            //
            OnLoaded();

            //  set initialized
            isInitialized = true;
        }

        public override async void OnStart()
        {
            base.OnStart();

            //  Perform initialization here...
            await InitializeFragment();
        }

        private void HandleVisualStates()
        {
            //  begin state definition
            VisualState.BeginStateDefinition();

            //
            OnDefineVisualStates();

            //  finalize state definition
            VisualState.FinalizeStateDefinition();
        }

        protected virtual void OnDefineVisualStates()
        {
            //  TODO: Override and define visual states for dialog
        }

        protected virtual void OnLoaded()
        {
            //  TODO: Override and implement Loaded logic
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            if (LayoutId != -1)
            {
                contentView = LayoutInflater.FromContext(Activity).Inflate(LayoutId, null);
                builder.SetView(contentView);
            }

            return OnBuildDialog(builder);
        }


    }
}