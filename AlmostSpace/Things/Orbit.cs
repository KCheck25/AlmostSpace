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
using System.Collections;

namespace AlmostSpace.Things
{

    internal class Orbit
    {

        SimClock clock;

        Vector2D objectPosition;
        Vector2D objectVelocity;

        Planet planetOrbiting;

        Texture2D apTexture;
        Texture2D peTexture;

        double radius;           // distance from body (m)
        public double planetAngle;      // angle from body to current position (rad)
        double mu;               // gravitation parameter for current body
        double vMagnitude;       // magnitude of object's velocity (m/s)
        double semiMajorAxis;    // semi major axis of orbit (m)
        double period;           // period of orbit (s)
        double aMomentum;        // angular momentum (rad/s^2)
        double rApoapsis;        // apoapsis radius (m)
        double rPeriapsis;       // periapsis radius (m)
        Vector2D eV;             // eccentricity vector
        double e;                // eccentricity
        public double argP;             // argument of periapsis (rad)
        double semiMinorAxis;    // semi minor axis (m)

        double m0;

        double timeSinceStoppedPhysics;

        double universalGravity = 6.67E-11f;

        PositionColorD[] path;

        BasicEffect basicEffect;
        GraphicsDevice graphicsDevice;

        bool wasPhysics = true;

        bool stationaryObject;

        public Orbit(Vector2D position)
        {
            this.objectPosition = position;
            stationaryObject = true;
        }

        public Orbit(Planet planetOrbiting, Vector2D objectPosition, Vector2D objectVelocity, SimClock clock, GraphicsDevice graphicsDevice) { 
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

            stationaryObject = false;

        }

        public Orbit(Texture2D apTexture, Texture2D peTexture, Planet planetOrbiting, Vector2D objectPosition, Vector2D objectVelocity, SimClock clock, GraphicsDevice graphicsDevice)
        {
            this.planetOrbiting = planetOrbiting;
            this.objectPosition = objectPosition;
            this.objectVelocity = objectVelocity;
            this.clock = clock;
            this.graphicsDevice = graphicsDevice;
            this.apTexture = apTexture;
            this.peTexture = peTexture;

            mu = planetOrbiting.getMass() * universalGravity;

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
            (0, graphicsDevice.Viewport.Width,     // left, right
            graphicsDevice.Viewport.Height, 0,    // bottom, top
            0, 1);

            stationaryObject = false;

        }

        public void Update(Vector2D objectAcceleration)
        {
            if (stationaryObject)
            {
                return;
            }
            wasPhysics = true;

            double massPlanet = planetOrbiting.getMass();
            double xDist = objectPosition.X;
            double yDist = -objectPosition.Y;
            double distToCenter = Math.Pow(Math.Pow(xDist, 2) + Math.Pow(yDist, 2), 0.5);
            radius = distToCenter;

            Vector2D gravityAcceleration = new Vector2D();

            double totalGAccel = (universalGravity * massPlanet) / (distToCenter * distToCenter);
            planetAngle = Math.Atan2(yDist, xDist);

            double xAccel = -totalGAccel * Math.Cos(planetAngle);
            double yAccel = totalGAccel * Math.Sin(planetAngle);

            gravityAcceleration.X = xAccel;
            gravityAcceleration.Y = yAccel;

            objectVelocity += (gravityAcceleration + objectAcceleration) * clock.getFrameTime();
            objectPosition += objectVelocity * clock.getFrameTime();

            calculateParameters();
            //generatePath(1000);
        }

        // Recalculates the rocket's position in space and velocity based on its current orbital parameters
        // Allows time to be sped up without losing precision
        public void Update()
        {
            if (stationaryObject)
            {
                return;
            }
            if (wasPhysics)
            {
                transitionToNoPhysics();
                wasPhysics = false;
            }
            timeSinceStoppedPhysics += clock.getFrameTime();
            double timePassed = timeSinceStoppedPhysics;

            double tAnomaly = 0;
            if (e <= 1)
            {
                // Elliptical orbits
                double mAnomaly = Math.Sqrt(mu / Math.Pow(semiMajorAxis, 3)) * timePassed * -Math.Sign(aMomentum) + m0; // mean anomaly

                double eAnomaly = getEccentricAnomaly(mAnomaly, e, mAnomaly); // eccentric anomaly

                // don't update vectors if eccentric anomaly calculation gets stuck for some reason
                if (eAnomaly == -1)
                {
                    Debug.WriteLine("Error: Could Not Find Root!");
                    return;
                }

                tAnomaly = 2 * Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(eAnomaly / 2)); // true anomaly
            }
            else
            {
                // Hyperbolic orbits (https://control.asu.edu/Classes/MAE462/462Lecture05.pdf)

                double mAnomaly = Math.Sqrt(mu / Math.Pow(-semiMajorAxis, 3)) * timePassed * -Math.Sign(aMomentum) + m0; // hyperbolic mean anomaly

                double hAnomaly = getEccentricAnomaly(mAnomaly, e, mAnomaly); // hyperbolic anomaly

                if (hAnomaly == -1)
                {
                    Debug.WriteLine("Error: Could Not Find Root!");
                    return;
                }

                tAnomaly = 2 * Math.Atan(Math.Sqrt((e + 1) / (e - 1)) * Math.Tanh(hAnomaly / 2)); // true anomaly
            }

            double distAtAnomaly = semiMajorAxis * (1 - e * e) / (1 + e * Math.Cos(tAnomaly)); // distance from planet

            radius = distAtAnomaly;

            vMagnitude = Math.Sqrt(mu * (2 / distAtAnomaly - 1 / semiMajorAxis));

            // https://physics.stackexchange.com/questions/669946/how-to-calculate-the-direction-of-the-velocity-vector-for-a-body-that-moving-is
            double vY = -Math.Sin(tAnomaly) / Math.Sqrt(1 + e * e + 2 * e * Math.Cos(tAnomaly)) * vMagnitude * -Math.Sign(aMomentum);
            double vX = (e + Math.Cos(tAnomaly)) / Math.Sqrt(1 + e * e + 2 * e * Math.Cos(tAnomaly)) * vMagnitude * -Math.Sign(aMomentum);

            // https://matthew-brett.github.io/teaching/rotation_2d.html
            objectVelocity.X = (Math.Cos(-argP - MathHelper.PiOver2) * vX - Math.Sin(-argP - MathHelper.PiOver2) * vY);
            objectVelocity.Y = (Math.Sin(-argP - MathHelper.PiOver2) * vX + Math.Cos(-argP - MathHelper.PiOver2) * vY);

            objectPosition.X = distAtAnomaly * Math.Cos(tAnomaly + argP);
            objectPosition.Y = -distAtAnomaly * Math.Sin(tAnomaly + argP);

            planetAngle = Math.Atan2(-objectPosition.Y, objectPosition.X);
        }

        public void Draw(SpriteBatch spriteBatch, Matrix transform)
        {
            if (stationaryObject)
            {
                return;
            }
            generatePath(1000);
            if (path != null)
            {
                basicEffect.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, PositionColorD.getVertexPositionColorArr(path, transform), 0, path.Length - 1);
            }
            if (apTexture != null && peTexture != null)
            {
                if ((planetOrbiting.getSOI() == 0 && e < 1) || (rApoapsis < planetOrbiting.getSOI() && rApoapsis > 0))
                {
                    Vector2D apPos = new Vector2D(Math.Cos(argP + MathHelper.Pi) * rApoapsis, -Math.Sin(argP + MathHelper.Pi) * rApoapsis);
                    spriteBatch.Draw(apTexture, Vector2D.Transform(apPos + planetOrbiting.getPosition(), transform).getVector2(), null, Color.White, 0f, new Vector2(apTexture.Width / 2, 0), 0.5f, SpriteEffects.None, 0f);
                }
                Vector2D pePos = new Vector2D(Math.Cos(argP) * rPeriapsis, -Math.Sin(argP) * rPeriapsis);
                spriteBatch.Draw(peTexture, Vector2D.Transform(pePos + planetOrbiting.getPosition(), transform).getVector2(), null, Color.White, 0f, new Vector2(peTexture.Width / 2, 0), 0.5f, SpriteEffects.None, 0f);
            }
        }

        // Recalculates the rocket's orbital parameters from its velocity and position vectors
        void calculateParameters()
        {
            // mu is the standard gravitational parameter of the planet that's being orbited
            mu = universalGravity * planetOrbiting.getMass();
            double velocityMagnitude = objectVelocity.Length();
            vMagnitude = velocityMagnitude;

            // Using vis-viva equation but solving for the semi major axis (a): https://en.wikipedia.org/wiki/Vis-viva_equation
            semiMajorAxis = -1f / ((velocityMagnitude * velocityMagnitude / mu) - (2 / radius));

            period = (2 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / mu));


            // Equations taken from here: https://space.stackexchange.com/questions/2562/2d-orbital-path-from-state-vectors
            aMomentum = objectPosition.X * objectVelocity.Y - objectPosition.Y * objectVelocity.X; // angular momentum

            // eccentricity vector
            double eX = (objectVelocity.Y * aMomentum) / mu - objectPosition.X / radius;
            double eY = (-objectVelocity.X * aMomentum) / mu - objectPosition.Y / radius;
            eV = new Vector2D(eX, eY);
            e = eV.Length();

            // argument of periapsis - angle from planet to periapsis
            argP = -Math.Atan2(eV.Y, eV.X);

            // Apoapsis and periapsis radiuses
            rApoapsis = semiMajorAxis * (1 + e);
            rPeriapsis = semiMajorAxis * (1 - e);

            semiMinorAxis = semiMajorAxis * Math.Sqrt(1 - e * e);

            //Debug.WriteLine(mu + " " + semiMajorAxis + " " + radius + " " + e + " " + vMagnitude + " " + objectPosition);
        }

        // Generates a list of OrbitSprite objects arranged in the rocket's trajectory
        void generatePath(int numPoints)
        {
            bool connected = true;

            double rMax = planetOrbiting.getSOI();
            double tMax = MathHelper.Pi;
            double tMin = -MathHelper.Pi;

            // Limit angle for hyperbolic trajectories to a max radius
            if ((e > 1 || rApoapsis > rMax) && rMax > 0)
            {
                tMax = Math.Acos((semiMajorAxis * (1 - e * e) - rMax) / (e * rMax)) * -Math.Sign(aMomentum);
                connected = false;

                tMin = planetAngle - argP;
            } else if (e > 1)
            {
                tMax = MathHelper.PiOver2 * -Math.Sign(aMomentum);
                tMin = planetAngle - argP;
                connected = false;
            }

            path = new PositionColorD[connected ? numPoints + 1 : numPoints];

            if (tMin < -MathHelper.Pi)
            {
                tMin += MathHelper.TwoPi;
            } else if (tMin > MathHelper.Pi)
            {
                tMin -= MathHelper.TwoPi;
            }

            if (tMax < -MathHelper.Pi)
            {
                tMax += MathHelper.TwoPi;
            }
            else if (tMax > MathHelper.Pi)
            {
                tMax -= MathHelper.TwoPi;
            }

            // Swap min and max if min is greater than max
            if (tMin > tMax)
            {
                tMin += tMax;
                tMax = tMin - tMax;
                tMin -= tMax;
            }

            double step = (tMax - tMin) / numPoints;

            int i = 0;

            double currentTheta = tMin;
            while (i < numPoints)
            {
                double r = getRadiusAtAngle(semiMajorAxis, e, currentTheta);
                PositionColorD point = new PositionColorD(new Vector2D(r * Math.Cos(currentTheta + argP) + planetOrbiting.getPosition().X, -r * Math.Sin(currentTheta + argP) + planetOrbiting.getPosition().Y), Color.White);
                if (i < numPoints)
                {
                    path[i] = point;
                }
                if (i == 0 && connected)
                {
                    path[path.Length - 1] = point;
                }
                i++;
                currentTheta += step;
            }

        }

        // This method runs when switching between physics mode and non physics mode
        // It's main purpose is to compute the rocket's mean anomaly at the time of making this switch
        void transitionToNoPhysics()
        {
            timeSinceStoppedPhysics = 0;
            double currentTrueAnomaly = planetAngle - argP;
            if (e > 1)
            {
                double hAnomaly = 2 * Math.Atanh(Math.Tan(currentTrueAnomaly / 2) / Math.Sqrt((e + 1) / (e - 1)));
                m0 = e * Math.Sinh(hAnomaly) - hAnomaly;
            }
            else
            {
                m0 = (Math.Atan2(-Math.Sqrt(1 - e * e) * Math.Sin(currentTrueAnomaly), -e - Math.Cos(currentTrueAnomaly)) + MathHelper.Pi - e * Math.Sqrt(1 - e * e) * Math.Sin(currentTrueAnomaly) / (1 + e * Math.Cos(currentTrueAnomaly)));
            }
        }

        // Gets the radius of the rocket at a given angle
        double getRadiusAtAngle(double a, double e, double theta)
        {
            return a * (1 - e * e) / (1 + e * Math.Cos(theta));
        }

        // Code adapted from https://www.geeksforgeeks.org/program-for-newton-raphson-method/
        // Computes the eccentric anomaly using the newton raphson root finding algorithm
        // Takes a guess (x), the eccentricity of the orbit, and the mean anomaly
        static double getEccentricAnomaly(double x, double eccentricity, double mAnomaly)
        {

            double h = func(x, eccentricity, mAnomaly) / derivFunc(x, eccentricity);
            int i = 0;
            while (Math.Abs(h) >= 0.000001)
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

            return Math.Round(x * 1000000) / 1000000;
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

        public Vector2D getPosition()
        {
            return stationaryObject ? objectPosition : objectPosition + planetOrbiting.getPosition();
        }

        public Vector2D getRelativePosition()
        {
            return objectPosition;
        }

        public Vector2D getVelocity()
        {
            return stationaryObject ? objectVelocity : objectVelocity + planetOrbiting.getVelocity();
        }

        public Vector2D getRelativeVelocity()
        {
            return objectVelocity;
        }

        public double getSemiMajorAxis()
        {
            return semiMajorAxis;
        }

        // Returns the rockets height above the planets surface in meters
        public double getHeight()
        {
            return radius - planetOrbiting.getRadius();
        }

        public double getOrbitRadius()
        {
            return radius;
        }

        // Returns the highest point above the surface tha the rocket will reach in meters
        public double getApoapsisHeight()
        {
            return rApoapsis - planetOrbiting.getRadius() > 0 ? rApoapsis - planetOrbiting.getRadius() : double.NaN;
        }

        // Returns the lowest point above the surface tha the rocket will reach in meters
        public double getPeriapsisHeight()
        {
            return rPeriapsis - planetOrbiting.getRadius();
        }

        // Returns the magnitude of the rocket's velocity
        public double getVelocityMagnitude()
        {
            return vMagnitude;
        }

        // Returns the period of the rockets current orbit in seconds
        public double getPeriod()
        {
            return period;
        }

        // Changes the position and velocity vectors of the orbit to be relative to the given planet
        public void setPlanetOrbiting(Planet planet)
        {
            if (stationaryObject)
            {
                return;
            }
            Debug.WriteLine("BEFORE:    Current Velocity: " + getVelocity() + " Current Position: " + getPosition());
            Debug.WriteLine("RELATIVE:  Current Velocity: " + objectVelocity + " Current Position: " + objectPosition);
            //if (enter)
            //{
            //    objectPosition += planetOrbiting.getRelativePosition();
            //    objectVelocity += planetOrbiting.getRelativeVelocity();
            //} else
            //{
            //    objectPosition -= planetOrbiting.getRelativePosition();
            //    objectVelocity -= planetOrbiting.getRelativeVelocity();
            //}

            objectPosition = getPosition() - planet.getPosition();
            objectVelocity = getVelocity() - planet.getVelocity();

            planetOrbiting = planet;

            Debug.WriteLine("AFTER:     Current Velocity: " + getVelocity() + " Current Position: " + getPosition());
            Debug.WriteLine("RELATIVE:  Current Velocity: " + objectVelocity + " Current Position: " + objectPosition);

            clock.setTimeFactor(0);
            Update(new Vector2D());
            //double thing = 0;
        }

        public SimClock getClock()
        {
            return clock;
        }

        public Planet getPlanetOrbiting()
        {
            return planetOrbiting;
        }

        public void oldEquations()
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
