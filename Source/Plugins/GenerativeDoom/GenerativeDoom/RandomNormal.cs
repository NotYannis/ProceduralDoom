using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerativeDoom
{
    class RandomNormal
    {
        static Random _r = new Random();
        static Random _r2 = new Random();

        /// <summary>
        /// Create a normaly distributed number
        /// </summary>
        /// <param name="mean">The mean of the distribution</param>
        /// <param name="standDev">The standard deviation of the distribution</param>
        /// <returns>Return the number</returns>
        public double Generate(int mean, float standDev)
        {
            double theta = Math.PI * 2 * (1.0 - _r.NextDouble());

            double N = Math.Sqrt(-2 * Math.Log(1.0 - _r.NextDouble())) * Math.Cos(theta);

            return mean + standDev * N;
        }
    }

}

