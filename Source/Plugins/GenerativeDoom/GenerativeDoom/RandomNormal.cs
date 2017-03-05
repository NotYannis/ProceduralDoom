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

        public double Generate(int mean, float standDev)
        {
            double theta = Math.PI * 2 * (1.0 - _r.NextDouble());

            double N = Math.Sqrt(-2 * Math.Log(1.0 - _r.NextDouble())) * Math.Cos(theta);

            return mean + standDev * N;
        }
    }

}

