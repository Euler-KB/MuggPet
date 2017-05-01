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
using System.Reflection;
using System.Linq.Expressions;

namespace MuggPet.Activity.VisualState
{
    /// <summary>
    /// Manages visual states for a given context (activity,fragment....etc)
    /// </summary>
    public class VisualStateManager
    {
        private HashSet<VisualState> visualStates = new HashSet<VisualState>();

        private IList<IEnumerable<MemberState>> _defaultMemberStates;

        private ViewGroup rootView;

        private VisualState currentState;

        /// <summary>
        /// Gets the root view for the state manager
        /// </summary>
        public ViewGroup RootView
        {
            get { return rootView; }
        }

        /// <summary>
        /// Invoked upon state changes
        /// </summary>
        public event EventHandler<string> StateChanged;


        /// <summary>
        /// Invoked upon resetting the state manager
        /// </summary>
        public event EventHandler StateReset;


        #region State Definition

        private VisualState _defaultState;

        internal void BeginStateDefinition()
        {
            //  ensure there are no states defined
            visualStates.Clear();
        }

        internal void FinalizeStateDefinition()
        {
            if (_defaultState != null)
                CurrentState = _defaultState.Name;
        }

        #endregion

        /// <summary>
        /// Initializes a new state manager with the given root view
        /// </summary>
        /// <param name="rootView">The root view is used for finding sub views when required</param>
        public VisualStateManager(ViewGroup rootView)
        {
            if (rootView == null)
                throw new ArgumentNullException("rootView", "The root view is required!");

            //
            this.rootView = rootView;
        }

        private void InternalRestoreState()
        {
            //  restore original state
            foreach (var frame in _defaultMemberStates)
            {
                foreach (var state in frame)
                    state.Apply();
            }
        }

        /// <summary>
        /// Returns the name of the current state. Returns null if no state is activated.
        /// </summary>
        public string CurrentState
        {
            get
            {
                if (currentState == null)
                    return null;

                return currentState.Name;
            }

            set
            {
                if (currentState?.Name != value)
                {
                    var newState = GetStateWithName(value);
                    if (newState != null)
                    {

                        if (_defaultMemberStates != null)
                        {
                            //  we will update member states for continuous mode
                            if (newState.ActivationMode == StateActivationMode.Continuous)
                            {
                                //  find index of state
                                int index = 0;
                                foreach (var item in visualStates)
                                {
                                    if (item == newState)
                                        break;

                                    index++;
                                }

                                //  we are certain of state index
                                _defaultMemberStates[index] = newState.MemberStates;

                                //  Note: Get the member state for the new state will Build Activation logic so we defer to 
                                //  prevent next call( Apply() ) from building logic again
                                newState.DeferContinuousUpdate();

                            }

                            InternalRestoreState();
                        }
                        else
                        {
                            //
                            _defaultMemberStates = new List<IEnumerable<MemberState>>();

                            //  capture state
                            foreach (var state in visualStates)
                                _defaultMemberStates.Add(state.MemberStates);
                        }

                        //
                        currentState = newState;
                        newState.Apply();

                        //
                        StateChanged?.Invoke(this, currentState.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the names of all registered states
        /// </summary>
        public string[] StateNames
        {
            get { return visualStates.Select(x => x.Name).ToArray(); }
        }

        /// <summary>
        /// Returns the names of all combined states
        /// </summary>
        public string [] CombinedStates
        {
            get { return visualStates.Where(x => x.HasMultipleStates).Select(x => x.Name).ToArray(); }
        }

        /// <summary>
        /// Gets the total registered state
        /// </summary>
        public int TotalStates
        {
            get { return visualStates.Count; }
        }

        /// <summary>
        /// Returns a visual state object representing the state with the given name
        /// </summary>
        /// <param name="name">The name of the state to fetch</param>
        public VisualState GetStateWithName(string name)
        {
            return visualStates.FirstOrDefault(x => x.Name == name);
        }

        VisualState MakeNewState(string name, IEnumerable<VisualState> existingStates = null)
        {
            VisualState state = null;

            if (existingStates == null)
                state = new VisualState(this, name);
            else
                state = new VisualState(this, name, existingStates);

            if (visualStates.Contains(state))
                throw new Exception($"The state name '{name}' is already registered!");

            visualStates.Add(state);

            return state;
        }

        /// <summary>
        /// Registers a new state for 
        /// </summary>
        /// <param name="name">The name of the new state</param>
        /// <param name="defaultState">True to make this the default state</param>
        public VisualState RegisterState(string name, bool defaultState = false)
        {
            var state = MakeNewState(name);

            if (defaultState)
                _defaultState = state;

            return state;
        }

        /// <summary>
        /// Registers a combined state , thus a state that has multiple states attached together with itself
        /// </summary>
        /// <param name="combinedName">The name for the combined state</param>
        /// <param name="states">Names of registered state you wish to combine</param>
        public VisualState RegisterCombined(string combinedName, params string[] states)
        {
            return MakeNewState(combinedName, visualStates.Where(x => states.Contains(x.Name)));
        }

        /// <summary>
        /// Moves the the specified state
        /// </summary>
        /// <param name="stateName">The name of the destination state</param>
        public bool GotoState(string stateName)
        {
            var state = GetStateWithName(stateName);
            if (state != null)
            {
                CurrentState = state.Name;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reverts to the default state
        /// </summary>
        public void GotoDefaultState()
        {
            if (_defaultState != null && currentState != _defaultState)
                CurrentState = _defaultState.Name;
        }

        /// <summary>
        /// Resets the state manager
        /// </summary>
        public void Reset()
        {
            InternalRestoreState();
            _defaultMemberStates.Clear();
            visualStates.Clear();
            currentState = null;
            StateReset?.Invoke(this, EventArgs.Empty);
        }
    }

    public class MemberState
    {
        public MemberInfo Member { get; set; }

        public Delegate SetDelegate { get; set; }

        public object Source { get; set; }

        public object Value { get; set; }

        public void Apply()
        {
            if (Member == null && SetDelegate != null)
            {
                SetDelegate.DynamicInvoke(Source, Value);
            }
            else
            {
                if (Member.MemberType == MemberTypes.Property)
                {
                    var propertyInfo = (PropertyInfo)Member;
                    propertyInfo.SetValue(Source, Value);
                }
            }

        }
    }

    public enum StateActivationMode
    {
        /// <summary>
        /// State activation logic happens once and cached for performance
        /// </summary>
        Single,

        /// <summary>
        /// State activation logic happens per each call. This is useful for scenarios where that logic depends on some other changing fields
        /// </summary>
        Continuous
    }

    public class VisualState
    {
        private IEnumerable<VisualState> _existingVisualStates;

        private bool deferContinuousUpdate;

        public void DeferContinuousUpdate()
        {
            deferContinuousUpdate = true;
        }

        public void CancelContinueUpdateDefer()
        {
            deferContinuousUpdate = false;
        }

        public IEnumerable<VisualState> CombinedStates
        {
            get { return _existingVisualStates; }
        }

        public string Name { get; }

        internal StateActivationMode ActivationMode { get; private set; }

        public VisualStateManager StateManager { get; }

        public ICollection<VisualStateDescription> StateDescriptions { get; } = new List<VisualStateDescription>();

        private Action _activateCallback;

        private bool _isPrepared;

        public bool HasMultipleStates
        {
            get { return _existingVisualStates?.Count() > 0; }
        }

        private VisualState() { }

        public VisualState(VisualStateManager manager, string name)
        {
            Name = name;
            StateManager = manager;
        }

        public VisualState(VisualStateManager manager, string name, IEnumerable<VisualState> existingStates)
        {
            Name = name;
            StateManager = manager;
            this._existingVisualStates = existingStates;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public IEnumerable<MemberState> MemberStates
        {
            get
            {
                Prepare();

                List<MemberState> values = new List<MemberState>();
                foreach (var state in StateDescriptions)
                {
                    if (state.Member == null)
                    {
                        values.Add(new MemberState()
                        {
                            SetDelegate = state.SetDelegate,
                            Source = state.Source,
                            Value = state.GetDelegate.DynamicInvoke(state.Source)
                        });
                    }
                    else
                    {
                        if (state.Member.MemberType == MemberTypes.Property)
                        {
                            var property = (PropertyInfo)state.Member;
                            if (property.GetMethod != null)
                            {
                                values.Add(new MemberState()
                                {
                                    Member = state.Member,
                                    Value = property.GetValue(state.Source),
                                    Source = state.Source,
                                    SetDelegate = state.SetDelegate
                                });
                            }
                        }
                    }
                }

                return values;
            }
        }

        void Prepare()
        {
            if (_activateCallback != null)
            {
                if (ActivationMode == StateActivationMode.Single)
                {
                    if (!_isPrepared)
                    {
                        //  Build state descriptions
                        _activateCallback();
                        _isPrepared = true;
                    }
                }
                else if (ActivationMode == StateActivationMode.Continuous)
                {
                    //
                    if (deferContinuousUpdate)
                        deferContinuousUpdate = false;
                    else
                    {
                        //  
                        StateDescriptions.Clear();
                        _activateCallback();
                    }
                }
            }
        }

        /// <summary>
        /// Applies the state immediately
        /// </summary>
        public void Apply()
        {
            Prepare();

            if (HasMultipleStates)
            {
                //  apply combined states before myself
                foreach (var cvs in CombinedStates)
                    cvs.Apply();
            }

            //  apply state descriptions
            foreach (var state in StateDescriptions)
            {
                if (state.Member == null)
                {
                    //  probably using delegates...
                    state.SetDelegate.DynamicInvoke(state.Source, null);
                }
                else
                {
                    if (state.Member.MemberType == MemberTypes.Property)
                    {
                        var property = (PropertyInfo)state.Member;
                        property.SetValue(state.Source, state.Parameters[0]);
                    }
                }
            }
        }

        /// <summary>
        /// Sets how state activation logic is process
        /// </summary>
        /// <param name="activationMode">Indicates how to invoke state activation logic</param>
        public VisualState SetActivationMode(StateActivationMode activationMode)
        {
            ActivationMode = activationMode;
            return this;
        }

        /// <summary>
        /// Overrides the logic handler for state activation
        /// </summary>
        /// <param name="onActivate"></param>
        /// <returns></returns>
        public VisualState OnActivate(Action<StateObjectDefinitionWrapper> onActivate)
        {
            StateObjectDefinitionWrapper wrapper = new StateObjectDefinitionWrapper(this);
            _activateCallback = () => { onActivate(wrapper); };
            return this;
        }
    }

    public class VisualStateDescription
    {
        public object Source { get; set; }

        #region Member Mode

        public MemberInfo Member { get; set; }

        public object[] Parameters { get; set; }

        #endregion

        #region Complex Mode

        public Delegate GetDelegate { get; set; }

        public Delegate SetDelegate { get; set; }

        #endregion
    }

    /// <summary>
    /// Responsible for attaching object and views to the current state
    /// </summary>
    public class StateObjectDefinitionWrapper
    {
        private VisualState visualState;

        public object SourceObject { get; set; }

        public StateObjectDefinitionWrapper(VisualState state)
        {
            visualState = state;
        }


        /// <summary>
        /// Attaches the object to the current state
        /// </summary>
        /// <param name="obj">The object to be attached</param>
        public StateMemberDefinitionWrapper<T> HasObject<T>(T obj)
        {
            SourceObject = obj;
            return new StateMemberDefinitionWrapper<T>(visualState, this);
        }

        /// <summary>
        /// Attaches a view to the current state
        /// </summary>
        /// <param name="view">The view to attach</param>
        public StateMemberDefinitionWrapper<T> HasView<T>(T view) where T : View
        {
            return HasObject(view);
        }

        /// <summary>
        /// Attaches a view with with given id to the state
        /// </summary>
        /// <param name="viewId">The id of the view to be attached</param>
        public StateMemberDefinitionWrapper<View> HasView(int viewId)
        {
            return HasObject(visualState.StateManager.RootView.FindViewById(viewId));
        }

        /// <summary>
        /// Attaches a view with with given id to the state
        /// </summary>
        /// <param name="viewId">The id of the view to be attached</param>
        public StateMemberDefinitionWrapper<T> HasView<T>(int viewId) where T : View
        {
            return HasObject(visualState.StateManager.RootView.FindViewById<T>(viewId));
        }

    }

    /// <summary>
    /// Defines properties for object or views within the current state
    /// </summary>
    public class StateMemberDefinitionWrapper<T>
    {
        private VisualState state;

        private StateObjectDefinitionWrapper objectLayout;

        public StateObjectDefinitionWrapper ObjectDefinition => objectLayout;

        public StateMemberDefinitionWrapper(VisualState state, StateObjectDefinitionWrapper objectLayout)
        {
            this.state = state;
            this.objectLayout = objectLayout;
        }

        MemberInfo GetExpressionMember<TResult>(Expression<Func<T, TResult>> expression)
        {
            MemberInfo member = null;
            switch (expression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    member = (expression.Body as MemberExpression).Member;
                    break;
            }

            //  we currently deal with properties.... might improve in future
            if (member.MemberType == MemberTypes.Property)
                return member;

            //  
            return null;
        }

        /// <summary>
        /// Associates a property of the attached object
        /// </summary>
        /// <param name="expression">An expression for selecting the property to attach. Only properties with get and set accessors are supported</param>
        public MemberSetDefinitionWrapper<T, TResult> WithProperty<TResult>(Expression<Func<T, TResult>> expression)
        {
            return new MemberSetDefinitionWrapper<T, TResult>(state, this, GetExpressionMember(expression));
        }

        /// <summary>
        /// Assigns a value to the property in the expression given.
        /// This method implicitly associates the specified property to attached object and sets the value for the property when activated
        /// </summary>
        /// <param name="expression">An expression for selecting the property to attach. Only properties with get and set accessors are supported</param>
        /// <param name="value">The value to assign the property when state is activated</param>
        /// <param name="unique">Determines whether to ensure the property is set once</param>
        public StateMemberDefinitionWrapper<T> SetProperty<TResult>(Expression<Func<T, TResult>> expression , TResult value , bool unique)
        {
            return WithProperty(expression).Set(value);
        }

        /// <summary>
        /// Applies all associated properties to objects of the same type
        /// </summary>
        /// <param name="objects">A collection of objects to apply properties associated earlier to</param>
        public StateMemberDefinitionWrapper<T> AppliesTo(params T[] objects)
        {
            var myDescriptions = state.StateDescriptions.Where(x => x.Source.Equals(objectLayout.SourceObject)).ToArray();
            foreach (var item in objects)
            {
                foreach (var desc in myDescriptions)
                {
                    state.StateDescriptions.Add(new VisualStateDescription()
                    {
                         GetDelegate = desc.GetDelegate,
                         Member = desc.Member,
                         Parameters = desc.Parameters,
                         SetDelegate = desc.SetDelegate,
                         Source = item
                    });
                }
            }

            return this;
        }

        /// <summary>
        /// Exposes state definition to callbacks
        /// </summary>
        /// <param name="onSetValue">Invoked by the state manager to either apply or revert state to an object or view property. When this method is invoked with a null the value argument (T, object value), it indicates the application of a new state else revert the state to the given value.</param>
        /// <param name="onGetValue">Invoked to get the the current state of the object or view property</param>
        public StateMemberDefinitionWrapper<T> WithComplex(Action<T, object> onSetValue, Func<T, object> onGetValue)
        {
            state.StateDescriptions.Add(new VisualStateDescription()
            {
                GetDelegate = onGetValue,
                SetDelegate = onSetValue,
                Source = objectLayout.SourceObject
            });

            return this;
        }

    }

    /// <summary>
    /// Implements means of setting or updating values for properties associated to attached object in the current state
    /// </summary>
    public class MemberSetDefinitionWrapper<TSource, TProperty>
    {
        private StateMemberDefinitionWrapper<TSource> memberDefinition;
        private MemberInfo memberInfo;
        private VisualState state;

        public MemberSetDefinitionWrapper(VisualState state, StateMemberDefinitionWrapper<TSource> memberDef, MemberInfo memberInfo)
        {
            this.state = state;
            memberDefinition = memberDef;
            this.memberInfo = memberInfo;
        }

        /// <summary>
        /// Sets the property to the given value
        /// </summary>
        /// <param name="value">The value for the property</param>
        /// /// <param name="unique">Determines whether to ensure the property is set once</param>
        /// <returns></returns>
        public StateMemberDefinitionWrapper<TSource> Set(TProperty value , bool unique = false)
        {
            if (unique)
            {
                var existing = state.StateDescriptions.FirstOrDefault(x => x.Member == memberInfo && memberDefinition.ObjectDefinition.SourceObject == x.Source);
                if (existing != null)
                    state.StateDescriptions.Remove(existing);
            }

            state.StateDescriptions.Add(new VisualStateDescription()
            {
                Member = memberInfo,
                Source = memberDefinition.ObjectDefinition.SourceObject,
                Parameters = new object[] { value },
            });

            return memberDefinition;
        }


    }
}