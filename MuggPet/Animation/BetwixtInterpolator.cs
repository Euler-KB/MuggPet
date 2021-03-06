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
using System.IO;
using Android.Animation;
using MuggPet.Animation.Betwixt;

namespace MuggPet.Animation
{
    /// <summary>
    /// Implements a time interpolator with support for betwixt easing functions
    /// </summary>
    public class BetwixtInterpolator : Java.Lang.Object, ITimeInterpolator
    {
        /// <summary>
        /// The default interpolator. Usually a quintic ease implementation is returned
        /// </summary>
        public static readonly BetwixtInterpolator Default = Quintic;

        /// <summary>
        /// A standard elastic ease interpolator
        /// </summary>
        public static readonly BetwixtInterpolator Elastic = new BetwixtInterpolator(ElasticEase);

        /// <summary>
        /// A standard quintic ease interpolator
        /// </summary>
        public static readonly BetwixtInterpolator Quintic = new BetwixtInterpolator(Ease.Quint.Out);

        //  Precomputed elastic ease values (bouncy) from WPF (:-
        static float[] EaseData =
        {
            0F, 0.119228053251407F, 0.256646576858661F, 0.266305891787474F,
            0.908204083338792F, 1.17314514246451F,  1.36915008602793F,  1.47833067337665F,
            1.49856139054823F,  1.44180617146787F,  1.32945302902489F,  1.18783730586953F,
            1.0426238289619F,   0.916656170745475F, 0.825339229407273F, 0.825339229407273F,
            0.776336359374668F, 0.769424799796977F, 0.798080487607718F, 0.851477973688433F,
            0.916795403148675F, 0.981770218990792F, 1.03637553250236F,  1.07422976716441F,
            1.09274024430166F,  1.09300751259535F,  1.07889688951388F,  1.05591301124609F,
            1.02992077783653F,  1.00607820402746F,  0.988051616578558F, 0.977569694383242F,
            0.9746168392551F,   0.977609622267183F, 0.984094181743692F, 0.991367036274117F,
            0.997114658355217F, 0.999855548505417F, 1F
        };


        /// <summary>
        /// Eases the given percentage with an elastic function
        /// </summary>
        /// <param name="percent">Indicates how much to ease. Usually between 0 to 1</param>
        /// <returns>The ease value</returns>
        public static float ElasticEase(float percent)
        {
            return EaseData[(int)(percent * (EaseData.Length - 1))];
        }

        EaseFunc easeFunc;

        /// <summary>
        /// Initializes a new interpolator with a callback for easing
        /// </summary>
        /// <param name="ease">Specifies the ease function to use for interpolation. Various betwixt easing functions can be used without any hassle</param>
        public BetwixtInterpolator(EaseFunc ease)
        {
            easeFunc = ease;
        }

        /// <summary>
        /// Returns the ease value for the given input
        /// </summary>
        /// <param name="input">The input value to ease. Usually betwenn 0 to 1</param>
        public float GetInterpolation(float input)
        {
            return easeFunc(input);
        }
    }


}