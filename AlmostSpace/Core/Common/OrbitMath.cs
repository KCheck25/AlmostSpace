using Microsoft.Xna.Framework;
using System;

namespace AlmostSpace.Core.Common
{
    // Contains many useful functions for doing math involving orbital mechanics
    public static class OrbitMath
    {
        public static readonly double UNIVERSAL_G = 6.6743E-11;

        // Calculates the true anomaly of an object given its eccentricity, semi major axis, angular momentum, elapsed time, and starting mean anomaly
        public static double getTrueAnomaly(double mu, double e, double semiMajorAxis, double time, double aMomentum, double m0)
        {
            if (e <= 1)
            {
                // Elliptical orbits
                double mAnomaly = Math.Sqrt(mu / Math.Pow(semiMajorAxis, 3)) * time * -Math.Sign(aMomentum) + m0; // mean anomaly
                double eAnomaly = getEccentricAnomaly(mAnomaly, e, mAnomaly); // eccentric anomaly
                return eAnomaly == -1 ? -10000 : 2 * Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(eAnomaly / 2)); // true anomaly
            }
            else
            {
                // Hyperbolic orbits (https://control.asu.edu/Classes/MAE462/462Lecture05.pdf)
                double mAnomaly = Math.Sqrt(mu / Math.Pow(-semiMajorAxis, 3)) * time * -Math.Sign(aMomentum) + m0; // hyperbolic mean anomaly
                double hAnomaly = getEccentricAnomaly(mAnomaly, e, mAnomaly); // hyperbolic anomaly
                return hAnomaly == -1 ? -10000 : 2 * Math.Atan(Math.Sqrt((e + 1) / (e - 1)) * Math.Tanh(hAnomaly / 2)); // true anomaly
            }
        }

        // Code adapted from https://www.geeksforgeeks.org/program-for-newton-raphson-method/
        // Computes the eccentric anomaly using the newton raphson root finding algorithm
        // Takes a guess (x), the eccentricity of the orbit, and the mean anomaly
        public static double getEccentricAnomaly(double x, double eccentricity, double mAnomaly)
        {

            double h = func(x, eccentricity, mAnomaly) / derivFunc(x, eccentricity);
            int i = 0;
            while (Math.Abs(h) >= 0.00000001)
            {
                h = func(x, eccentricity, mAnomaly) / derivFunc(x, eccentricity);

                // x(i+1) = x(i) - f(x) / f'(x) 
                x = x - h;

                i++;

                // Exit if it can't find a root
                if (i > 1000000)
                {
                    return -1;
                }
            }

            return Math.Round(x * 100000000) / 100000000;
        }

        // Mean and eccentric anomaly function
        static double func(double x, double eccentricity, double meanAnomaly)
        {
            if (eccentricity < 1)
            {
                return x - eccentricity * Math.Sin(x) - meanAnomaly;
            }
            else
            {
                return eccentricity * Math.Sinh(x) - x - meanAnomaly;
            }
        }

        // First derivative of mean and eccentric anomaly function
        static double derivFunc(double x, double eccentricity)
        {
            if (eccentricity < 1)
            {
                return 1 - eccentricity * Math.Cos(x);
            }
            else
            {
                return eccentricity * Math.Cosh(x) - 1;
            }
        }

        // stolen from MathHelper and converted to doubles
        public static double WrapAngle(double angle)
        {
            if (angle > -Math.PI && angle <= Math.PI)
            {
                return angle;
            }

            angle %= Math.PI * 2f;
            if (angle <= -Math.PI)
            {
                return angle + Math.PI * 2f;
            }

            if (angle > Math.PI)
            {
                return angle - Math.PI * 2f;
            }

            return angle;
        }

        // Calculates the mean anomaly of an object along its orbit given its true anomaly and eccentricity
        public static double getMeanAnomaly(double e, double trueAnomaly)
        {
            if (e > 1)
            {
                double hAnomaly = 2 * Math.Atanh(Math.Tan(trueAnomaly / 2) / Math.Sqrt((e + 1) / (e - 1)));
                return e * Math.Sinh(hAnomaly) - hAnomaly;
            }
            else
            {
                return Math.Atan2(-Math.Sqrt(1 - e * e) * Math.Sin(trueAnomaly), -e - Math.Cos(trueAnomaly)) + MathHelper.Pi - e * Math.Sqrt(1 - e * e) * Math.Sin(trueAnomaly) / (1 + e * Math.Cos(trueAnomaly));
            }
        }

        // Using vis-viva equation but solving for the semi major axis (a): https://en.wikipedia.org/wiki/Vis-viva_equation
        public static double getSemiMajorAxis(double velocityMagnitude, double mu, double radius)
        {
            return -1 / (velocityMagnitude * velocityMagnitude / mu - 2 / radius);
        }

        // Calculates the period of an object's orbit based on its semi major axis
        public static double calcPeriod(double semiMajorAxis, double mu)
        {
            return 2 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / mu);
        }

        // Calculates the eccentricity vector of an object based on its position, velocity, and angular momentum
        public static Vector2D calcEccentricity(Vector2D position, Vector2D velocity, double angularMomentum, double mu)
        {
            double radius = position.Length();
            double eX = velocity.Y * angularMomentum / mu - position.X / radius;
            double eY = -velocity.X * angularMomentum / mu - position.Y / radius;
            return new Vector2D(eX, eY);
        }

        // This just exists cuz I wanted to store them somewhere lol
        public static void oldEquations()
        {
            /*
            // Math taken from: https://physics.stackexchange.com/questions/99094/using-2d-position-velocity-and-mass-to-determine-the-parametric-position-equat
            float semiMajorAxis = 0;
            float mu = 0;
            float velocityMagnitude = 0;

            float tanV = (position.X * velocity.Y - position.Y * velocity.X) / radius;
            float e = (float)Math.Pow(1 + (radius * tanV * tanV / mu) * (radius * velocityMagnitude * velocityMagnitude / mu - 2), 0.5);
            float radV = (position.X * velocity.X + position.Y * velocity.Y) / radius;
            float angle = Math.Sign(tanV * radV) * (float)Math.Acos((semiMajorAxis * (1 - e * e) - radius) / (e * radius)) - (float)Math.Atan2(position.Y, position.X);
            */

        }

    }
}
