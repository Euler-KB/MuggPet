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
using Newtonsoft.Json;

namespace MuggPet.Security
{
    /// <summary>
    /// Contains user credential properties. All properties are kept as strings for serialization.
    /// </summary>
    public class UserCredentials
    {
        /// <summary>
        /// Gets or sets the username property of the user
        /// </summary>
        [JsonIgnore]
        public string Username
        {
            get
            {
                if (!Properties.ContainsKey(nameof(Username)))
                    return null;

                return Properties[nameof(Username)];
            }

            set
            {
                Properties[nameof(Username)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the password property of the user
        /// </summary>
        [JsonIgnore]
        public string Password
        {
            get
            {
                if (!Properties.ContainsKey(nameof(Password)))
                    return null;

                return Properties[nameof(Password)];
            }

            set
            {
                Properties[nameof(Password)] = value;
            }
        }


        /// <summary>
        /// Contains properties for the user
        /// </summary>
        public IDictionary<string, string> Properties { get; private set; }

        public UserCredentials()
        {
            Properties = new Dictionary<string, string>();
        }

        public UserCredentials(string username, string password) : this()
        {
            Username = username;
            Password = password;
        }
    }
}