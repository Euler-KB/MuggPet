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

namespace MuggPet.App
{
    /// <summary>
    /// Defines the current scope of execution of the life cycle handler
    /// </summary>
    public enum LifecycleScope
    {
        /// <summary>
        /// Inidcates application is now staring
        /// </summary>
        ApplicationStartup = 0x1000,

        /// <summary>
        /// Indicates an activity is beign created. Usually invoked from Activity.OnCreate()
        /// </summary>
        ActivityCreated = 0x2000,

        /// <summary>
        /// Indicates an activity is beign destroyed. Usually invoked from Activity.OnDestroy()
        /// </summary>
        ActivityDestroyed = 0x4000,


        /// <summary>
        /// Indicates activity is no longer the foreground. Usually invoked from Activity.OnStop()
        /// </summary>
        ActivityMinimized = 0x8000,

        /// <summary>
        /// Indicates activity is beign resumed. Usually invoked from Activity.OnStart()
        /// </summary>
        ActivityResumed = 0x0100
    }

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    public class LifecycleAttribute : Attribute
    {
        /// <summary>
        /// Indicates the type of event the handler listens to
        /// </summary>
        public LifecycleScope Scope { get; }

        /// <summary>
        /// The type of the handler
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The static method name to receive callback
        /// </summary>
        public string Method { get; }

        public LifecycleAttribute(Type type, string method, LifecycleScope scope)
        {
            Scope = scope;
            Type = type;
            Method = method;
        }
    }

    /// <summary>
    /// Handles the execution of application life cycle events
    /// </summary>
    public static class LifeCycleManager
    {
        static Assembly[] lifeCycleAssemblies;

        static bool initialized;

        /// <summary>
        /// Tells the state of the life cycle manager
        /// </summary>
        public static bool Initialized
        {
            get { return initialized; }
        }

        static void CheckInitialized()
        {
            if (!initialized)
                throw new Exception("Life cycle manager not initialized yet! Please ensure to call Initialize first before attempting to use any other operation!");
        }

        /// <summary>
        /// Initializes life cycle manager for the supported assemblies
        /// </summary>
        /// <param name="assemblies">A collection of assemblies which have life cycle handlers</param>
        public static void Initialize(params Assembly[] assemblies)
        {
            if (!initialized)
            {
                lifeCycleAssemblies = assemblies;
                initialized = true;

                //  execute application start lifecycle event
                ExecuteLifecycle(LifecycleScope.ApplicationStartup);
            }
        }

        /// <summary>
        /// Invokes all life cycle handlers from the entry assembly
        /// </summary>
        /// <param name="scope">The scopes flag</param>
        /// <param name="parameters">Additional parameters for invocation</param>
        public static void ExecuteLifecycle(LifecycleScope scope, params object[] parameters)
        {
            CheckInitialized();

            foreach (var assembly in lifeCycleAssemblies)
            {
                ExecuteLifecycle(assembly, scope, parameters);
            }
        }

        /// <summary>
        /// Invokes life cycle handlers from the specified assembly
        /// </summary>
        /// <param name="assembly">The assembly to discover handlers from</param>
        /// <param name="scope">The scopes flag</param>
        /// <param name="parameters">Additional parameters for invocation</param>
        static void ExecuteLifecycle(Assembly assembly, LifecycleScope scope, params object[] parameters)
        {
            foreach (var attrib in assembly.GetCustomAttributes<LifecycleAttribute>().Where(x => x.Scope.HasFlag(scope)))
            {
                var method = attrib.Type.GetMethod(attrib.Method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    try
                    {
                        if (method.GetParameters().Length > 0 && parameters.Length > 0)
                            method.Invoke(null, parameters);
                        else
                        {
                            method.Invoke(null, null);
                        }
                    }
                    catch (Exception)
                    {
                        //  We'll leave the exception here
                    }
                }
            }
        }
    }
}