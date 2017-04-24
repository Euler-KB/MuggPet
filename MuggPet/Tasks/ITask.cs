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
        Task<TaskResult<T>> Execute(object state);
    }
}