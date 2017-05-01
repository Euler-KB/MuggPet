using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MuggPet.Activity;
using Android.Speech.Tts;

namespace MuggPet.Tasks.Native
{
    /// <summary>
    /// Ensures language packs for operating system are installed and ready to use text-to-speech features
    /// </summary>
    public class SpeechEngineInitializer : TaskBase<bool>
    {
        public const int TtsCheckDataPass = 1;

        /// <summary>
        /// Determines whether to request the user to install language packs if not
        /// </summary>
        public bool RequestInstall { get; set; }



        public SpeechEngineInitializer(IStartActivityAsync host) : base(host)
        {

        }

        protected override Intent OnGetIntent(object state)
        {
            return new Intent(TextToSpeech.Engine.ActionCheckTtsData);
        }

        protected override Task<bool> OnResult(object state, ActivityResultState result)
        {
            if ((int)result.ResultCode != TtsCheckDataPass && RequestInstall)
            {
                Intent installIntent = new Intent(TextToSpeech.Engine.ActionInstallTtsData);
                (HostActivity as Android.App.Activity)?.StartActivity(installIntent);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}