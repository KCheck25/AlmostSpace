using Microsoft.VisualBasic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AlmostSpace.Things
{

    internal class Orbit
    {

        SimClock clock;

        Vector2 objectPosition;
        Vector2 objectVelocity;

        Planet planetOrbiting;

        float radius;           // distance from body (m)
        float planetAngle;      // angle from body to current position (rad)
        float mu;               // gravitation parameter for current body
        float vMagnitude;       // magnitude of object's velocity (m/s)
        float semiMajorAxis;    // semi major axis of orbit (m)
        float period;           // period of orbit (s)
        float aMomentum;        // angular momentum (rad/s^2)
        float rApoapsis;        // apoapsis radius (m)
        float rPeriapsis;       // periapsis radius (m)
        Vector2 eV;             // eccentricity vector
        float e;                // eccentricity
        float argP;             // argument of periapsis (rad)
        float semiMinorAxis;    // semi minor axis (m)

        float m0;

        double timeSinceStoppedPhysics;

        float universalGravity = 6.67E-11f;

        VertexPositionColor[] path;

        BasicEffect basicEffect;
        GraphicsDevice graphicsDevice;

        Boolean wasPhysics = true;

        public Orbit(Planet planetOrbiting, Vector2 objectPosition, Vector2 objectVelocity, SimClock clock, GraphicsDevice graphicsDevice) { 
            this.planetOrbiting = planetOrbiting;
            this.objectPosition = objectPosition;
            this.objectVelocity = objectVelocity;
            this.clock = clock;
            this.graphicsDevice = graphicsDevice;

            mu = planetOrbiting.getMass() * universalGravity;

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
            (0, graphicsDevice.Viewport.Width,     // left, right
            graphicsDevice.Viewport.Height, 0,    // bottom, top
            0, 1);

        }

        public void update(Vector2 objectAcceleration)
        {
            wasPhysics = true;

            float massPlanet = planetOrbiting.getMass();
            float xDist = objectPosition.X;
            float yDist = -objectPosition.Y;
            float distToCenter = (float)Math.Pow(Math.Pow(xDist, 2) + Math.Pow(yDist, 2), 0.5);
            radius = distToCenter;

            Vector2 gravityAcceleration = new Vector2();

            float totalGAccel = (universalGravity * massPlanet) / (distToCenter * distToCenter);
            planetAngle = (float)Math.Atan2(yDist, xDist);

            float xAccel = -totalGAccel * (float)Math.Cos(planetAngle);
            float yAccel = totalGAccel * (float)Math.Sin(planetAngle);

            gravityAcceleration.X = xAccel;
            gravityAcceleration.Y = yAccel;

            objectVelocity += gravityAcceleration + objectAcceleration * clock.getFrameTime();
            objectPosition += objectVelocity * clock.getFrameTime();

            calculateParameters();
            generatePath(1000);
        }

        public void update()
        {
            if (wasPhysics)
            {
                transitionToNoPhysics();
                wasPhysics = false;
            }
            timeSinceStoppedPhysics += clock.getFrameTime();
            float timePassed = (float)timeSinceStoppedPhysics;

            float tAnomaly = 0;
            if (e <= 1)
            {
                // Elliptical orbits
                float mAnomaly = (float)Math.Sqrt(mu / Math.Pow(semiMajorAxis, 3)) * timePassed * -Math.Sign(aMomentum) + m0; // mean anomaly

                float eAnomaly = (float)getEccentricAnomaly(0, e, mAnomaly); // eccentric anomaly

                // don't update vectors if eccentric anomaly calculation gets stuck for some reason
                if (eAnomaly == -1)
                {
                    Debug.WriteLine("Error: Could Not Find Root!");
                    return;
                }

                tAnomaly = 2 * (float)Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(eAnomaly / 2)); // true anomaly
            }
            else
            {
                // Hyperbolic orbits (https://control.asu.edu/Classes/MAE462/462Lecture05.pdf)

                float mAnomaly = (float)Math.Sqrt(mu / Math.Pow(-semiMajorAxis, 3)) * timePassed * -Math.Sign(aMomentum) + m0; // hyperbolic mean anomaly

                float hAnomaly = (float)getEccentricAnomaly(mAnomaly, e, mAnomaly); // hyperbolic anomaly

                if (hAnomaly == -1)
                {
                    Debug.WriteLine("Error: Could Not Find Root!");
                    return;
                }

                tAnomaly = 2 * (float)Math.Atan(Math.Sqrt((e + 1) / (e - 1)) * Math.Tanh(hAnomaly / 2)); // true anomaly
            }

            float distAtAnomaly = semiMajorAxis * (1 - e * e) / (1 + e * (float)Math.Cos(tAnomaly)); // distance from planet

            radius = distAtAnomaly;

            vMagnitude = (float)Math.Sqrt(mu * (2 / distAtAnomaly - 1 / semiMajorAxis));

            // https://physics.stackexchange.com/questions/669946/how-to-calculate-the-direction-of-the-velocity-vector-for-a-body-that-moving-is
            float vY = -(float)Math.Sin(tAnomaly) / (float)Math.Sqrt(1 + e * e + 2 * e * Math.Cos(tAnomaly)) * vMagnitude * -Math.Sign(aMomentum);
            float vX = (float)(e + Math.Cos(tAnomaly)) / (float)Math.Sqrt(1 + e * e + 2 * e * Math.Cos(tAnomaly)) * vMagnitude * -Math.Sign(aMomentum);

            // https://matthew-brett.github.io/teaching/rotation_2d.html
            objectVelocity.X = (float)(Math.Cos(-argP - MathHelper.PiOver2) * vX - Math.Sin(-argP - MathHelper.PiOver2) * vY);
            objectVelocity.Y = (float)(Math.Sin(-argP - MathHelper.PiOver2) * vX + Math.Cos(-argP - MathHelper.PiOver2) * vY);

            objectPosition.X = distAtAnomaly * (float)Math.Cos(tAnomaly + argP);
            objectPosition.Y = -distAtAnomaly * (float)Math.Sin(tAnomaly + argP);
        }

        public void Draw(Matrix transform)
        {
            if (path != null)
            {
                basicEffect.View = transform;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, path, 0, path.Length - 1);
            }
        }

        // Recalculates the rocket's orbital parameters from its velocity and position vectors
        public void calculateParameters()
        {
            // mu is the standard gravitational parameter of the planet that's being orbited
            mu = universalGravity * planetOrbiting.getMass();
            float velocityMagnitude = getMagnitude(objectVelocity);
            vMagnitude = velocityMagnitude;

            // Using vis-viva equation but solving for the semi major axis (a): https://en.wikipedia.org/wiki/Vis-viva_equation
            semiMajorAxis = -1f / ((velocityMagnitude * velocityMagnitude / mu) - (2 / radius));

            period = (float)(2 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / mu));


            // Equations taken from here: https://space.stackexchange.com/questions/2562/2d-orbital-path-from-state-vectors
            aMomentum = objectPosition.X * objectVelocity.Y - objectPosition.Y * objectVelocity.X; // angular momentum

            // eccentricity vector
            float eX = (objectVelocity.Y * aMomentum) / mu - objectPosition.X / radius;
            float eY = (-objectVelocity.X * aMomentum) / mu - objectPosition.Y / radius;
            eV = new Vector2(eX, eY);
            e = getMagnitude(eV);

            // argument of periapsis - angle from planet to periapsis
            argP = -(float)Math.Atan2(eV.Y, eV.X);

            // Apoapsis and periapsis radiuses
            rApoapsis = semiMajorAxis * (1 + e);
            rPeriapsis = semiMajorAxis * (1 - e);

            semiMinorAxis = semiMajorAxis * (float)Math.Sqrt(1 - e * e);

            //Debug.WriteLine(mu + " " + semiMajorAxis + " " + radius + " " + e + " " + vMagnitude + " " + objectPosition);
        }

        // Generates a list of OrbitSprite objects arranged in the rocket's trajectory
        public void generatePath(int numPoints)
        {
            path = new VertexPositionColor[e > 1 ? numPoints : numPoints + 1];

            float rMax = planetOrbiting.getRadius() * 10;
            float tMax = MathHelper.Pi;

            // Limit angle for hyperbolic trajectories to a max radius
            if (e >= 1)
            {
                tMax = (float)Math.Acos((semiMajorAxis * (1 - e * e) - rMax) / (e * rMax));
            }

            float step = (tMax * 2) / numPoints;

            int i = 0;

            for (float t = -tMax; t <= tMax; t += step)
            {
                float r = getRadiusAtAngle(semiMajorAxis, e, t);
                VertexPositionColor point = new VertexPositionColor(new Vector3(r * (float)Math.Cos(t + argP) + planetOrbiting.getPosition().X, -r * (float)Math.Sin(t + argP) + planetOrbiting.getPosition().Y, 0), Color.White);
                if (i < numPoints)
                {
                    path[i] = point;
                }
                if (i == 0 && e < 1)
                {
                    path[path.Length - 1] = point;
                }
                i++;
            }

        }

        // This method runs when switching between physics mode and non physics mode
        // It's main purpose is to compute the rocket's mean anomaly at the time of making this switch
        public void transitionToNoPhysics()
        {
            timeSinceStoppedPhysics = 0;
            float currentTrueAnomaly = planetAngle - argP;
            if (e > 1)
            {
                float hAnomaly = 2 * (float)Math.Atanh(Math.Tan(currentTrueAnomaly / 2) / Math.Sqrt((e + 1) / (e - 1)));
                m0 = e * (float)Math.Sinh(hAnomaly) - hAnomaly;
            }
            else
            {
                m0 = (float)(Math.Atan2(-Math.Sqrt(1 - e * e) * Math.Sin(currentTrueAnomaly), -e - Math.Cos(currentTrueAnomaly)) + MathHelper.Pi - e * Math.Sqrt(1 - e * e) * Math.Sin(currentTrueAnomaly) / (1 + e * Math.Cos(currentTrueAnomaly)));
            }
        }

        // Gets the radius of the rocket at a given angle
        public float getRadiusAtAngle(float a, float e, float theta)
        {
            return a * (1 - e * e) / (1 + e * (float)Math.Cos(theta));
        }

        // Code adapted from https://www.geeksforgeeks.org/program-for-newton-raphson-method/
        // Computes the eccentric anomaly using the newton raphson root finding algorithm
        // Takes a guess (x), the eccentricity of the orbit, and the mean anomaly
        static double getEccentricAnomaly(double x, double eccentricity, double mAnomaly)
        {

            double h = func(x, eccentricity, mAnomaly) / derivFunc(x, eccentricity);
            int i = 0;
            while (Math.Abs(h) >= 0.00001)
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

            return Math.Round(x * 100000) / 100000;
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

        // Returns the magnitude of the given vector
        float getMagnitude(Vector2 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        public Vector2 getPosition()
        {
            return objectPosition;
        }

        public Vector2 getVelocity()
        {
            return objectVelocity;
        }

        public float getSemiMajorAxis()
        {
            return semiMajorAxis;
        }

    }
}
