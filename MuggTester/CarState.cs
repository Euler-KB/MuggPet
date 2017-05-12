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

namespace MuggTester
{
    public class CarState
    {
        public bool IsActive { get; set; }

        public double EngineRev { get; set; }

        public double MaxTorque { get; set; }

        public int EngineClocks { get; set; }
    }
}