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

namespace MuggPet.Tasks
{
    /// <summary>
    /// Represents the interface for a task
    /// </summary>
    public interface ITask<T>
    {
        /// <summary>
        /// Executes the current task
        /// </summary>
        /// <param name="state">An optional state for the task</param>
        Task<TaskResult<T>> Execute(object state);
    }
}