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
using Android.Provider;
using Android.Graphics;
using MuggPet.App;
using MuggPet.App.Activity;

namespace MuggPet.Tasks.Native
{
    /// <summary>
    /// Represents a task for capturing images from the camera
    /// </summary>
    public class CameraCaptureTask : TaskBase<Bitmap>
    {
        //  The output file
        private Java.IO.File _file;

        /// <summary>
        /// The output file name
        /// </summary>
        public string FileName
        {
            get { return _file.Path; }
            set
            {
                _file = new Java.IO.File(value);
            }
        }

        /// <summary>
        /// Initializes a new camera capture task with the current activity and a temporary generated file as the output
        /// </summary>
        public CameraCaptureTask() : this(BaseApplication.CurrentActivity as IStartActivityAsync, Utils.FileUtils.InternalTempFile)
        {

        }

        /// <summary>
        /// Initializes a new camera capture task with
        /// </summary>
        /// <param name="host">The host activity</param>
        /// <param name="path">The output file destination path for the captured image</param>
        public CameraCaptureTask(IStartActivityAsync host, string path) : base(host)
        {
            FileName = path;
        }

        /// <summary>
        /// Initializes a new camera capture task with a temporary storage file
        /// </summary>
        /// <param name="host">The host activity</param>
        /// <param name="file">The output file</param>
        public CameraCaptureTask(IStartActivityAsync host, Java.IO.File file) : base(host)
        {
            _file = file;
        }

        protected override Intent OnGetIntent(object state)
        {
            var intent = new Intent(MediaStore.ActionImageCapture);
            intent.PutExtra(MediaStore.ExtraOutput, _file);
            return intent;
        }

        protected override async Task<Bitmap> OnResult(object state, ActivityResultState result)
        {
            Bitmap bitmap = null;
            if (result.ResultCode == Result.Ok)
            {
                if (result.Data != null && result.Data.HasExtra("data"))
                {
                    bitmap = (Bitmap)result.Data.GetParcelableExtra("data");
                }
                else
                {
                    bitmap = await BitmapFactory.DecodeFileAsync(FileName);
                }
            }

            return bitmap;

        }

    }
}