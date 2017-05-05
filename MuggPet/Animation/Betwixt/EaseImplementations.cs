using System;
using MuggPet.Animation.Betwixt.Annotations;

namespace MuggPet.Animation.Betwixt
{
    // The implementations only specify an In or an Out (depending on which is easier to write)
    // And the rest of the set is created via Generic Ease Creation

    /// <summary>
    /// Implementation of Quadratic Ease
    /// </summary>
    internal static class QuadraticImpl
    {
        public static float In(float percent)
        {
            return (float)System.Math.Pow(percent, 2);
        }
    }

    /// <summary>
    /// Implementation of Cubic Ease
    /// </summary>
    internal static class CubicImpl
    {
        public static float In(float percent)
        {
            return (float)System.Math.Pow(percent, 3);
        }
    }

    /// <summary>
    /// Implementation of Quartic Ease
    /// </summary>
    internal static class QuarticImpl
    {
        public static float In(float percent)
        {
            return (float)System.Math.Pow(percent, 4);
        }
    }

    /// <summary>
    /// Implementation of Quintic Ease
    /// </summary>
    internal static class QuinticImpl
    {
        public static float In(float percent)
        {
            return (float)System.Math.Pow(percent, 5);
        }
    }

    /// <summary>
    /// Implementation of Sine Ease
    /// </summary>
    internal static class SineImpl
    {
        public static float Out(float percent)
        {
            return (float)System.Math.Sin(percent * (System.Math.PI / 2));
        }
    }

    /// <summary>
    /// Implementation of Exponential Ease
    /// </summary>
    internal static class ExponentialImpl
    {
        public static float Out(float percent)
        {
            return (float)System.Math.Pow(2, 10 * (percent - 1));
        }
    }

    /// <summary>
    /// Implementation of Circlic Ease
    /// </summary>
    internal static class CircImpl
    {
        public static float Out(float percent)
        {
            return (float)System.Math.Sqrt(1 - System.Math.Pow(percent - 1, 2));
        }
    }

    /// <summary>
    /// Implementation of "Back" Ease
    /// </summary>
    internal static class BackImpl
    {
        public static float In(float percent)
        {
            const float s = 1.70158f;
            return (float)System.Math.Pow(percent, 2) * ((s + 1) * percent - s);
        }
    }

    /// <summary>
    /// Implementation of "Elastic" Ease
    /// </summary>
    [UsedImplicitly]
    internal static class ElasticImpl
    {
        public static float Out(float percent)
        {
            return (float)(1 + System.Math.Pow(2, -10 * percent) * System.Math.Sin((percent - 0.075) * (2 * System.Math.PI) / 0.3));
        }
    }

    /// <summary>
    /// Implementation of "Bounce" Ease
    /// </summary>
    [UsedImplicitly]
    internal static class BounceImpl
    {
        public static float Out(float percent)
        {
            const float s = 7.5625f;
            const float p = 2.75f;

            if (percent < (1 / p))
            {
                return (float)(s * System.Math.Pow(percent, 2));
            }

            if (percent < (2 / p))
            {
                percent -= (1.5f / p);
                return (float)(s * System.Math.Pow(percent, 2) + .75);
            }

            if (percent < (2.5f / p))
            {
                percent -= (2.25f / p);
                return (float)(s * System.Math.Pow(percent, 2) + .9375);
            }

            percent -= (2.625f / p);
            return (float)(s * System.Math.Pow(percent, 2) + .984375);
        }
    }
}
