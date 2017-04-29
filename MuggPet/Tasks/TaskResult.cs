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

namespace MuggPet.Tasks
{
    /// <summary>
    /// Represents a task result
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    public class TaskResult<T>
    {
        bool _success;
        T _result;
        string _errorMessage;

        /// <summary>
        /// Determines whether the task succeeded
        /// </summary>
        public bool Succeeded
        {
            get
            {
                return _success;
            }
        }

        /// <summary>
        /// Initializes a new task result
        /// </summary>
        /// <param name="result">The result from the task. If null, will be marked as unsuccessful</param>
        public TaskResult(T result)
        {
            _success = result != null;
            _result = result;
        }

        public TaskResult(bool success, T result)
        {
            _success = success;
            _result = result;
        }

        /// <summary>
        /// Initializes a new task 
        /// </summary>
        /// <param name="errorMessage"></param>
        public TaskResult(string errorMessage)
        {
            _success = false;
            _errorMessage = errorMessage;
        }

        /// <summary>
        /// The output generated from the task
        /// </summary>
        public T Result
        {
            get { return _result; }
        }

        /// <summary>
        /// Gets associated error messages
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }
    }
}