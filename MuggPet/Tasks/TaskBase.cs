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
using System.Threading.Tasks;
using MuggPet.App.Activity;
using MuggPet.App;

namespace MuggPet.Tasks
{
    /// <summary>
    /// Represents the base for all tasks
    /// </summary>
    /// <typeparam name="T">The type of result</typeparam>
    public abstract class TaskBase<T> : ITask<T>
    {
        private IStartActivityAsync host;

        /// <summary>
        /// Gets the bound activity
        /// </summary>
        protected IStartActivityAsync Host => host;

        public TaskBase(IStartActivityAsync host)
        {
            this.host = host;
        }

        public TaskBase()
        {
            this.host = BaseApplication.CurrentActivity as IStartActivityAsync;
        }

        /// <summary>
        /// Invoked after the started activity has finished
        /// </summary>
        /// <param name="state">The state object passed prior execute</param>
        /// <param name="result">The result of the application</param>
        protected abstract Task<T> OnResult(object state, ActivityResultState result);

        /// <summary>
        /// Retreives the intent for the activity to start for results
        /// </summary>
        /// <param name="state">The state object passed prior execute</param>
        protected abstract Intent OnGetIntent(object state);

        /// <summary>
        /// Executes task with optional state argument
        /// </summary>
        /// <param name="state">An optional argument for the task</param>
        public async Task<TaskResult<T>> Execute(object state = null)
        {
            //  get intent for target activity
            var intent = OnGetIntent(state);
            var result = await host.StartActivityForResultAsync(intent);

            //  process result
            return new TaskResult<T>(await OnResult(state, result));
        }
    }
}