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

        Texture2D apTexture;
        Texture2D peTexture;

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

        bool wasPhysics = true;

        bool stationaryObject;

        public Orbit(Vector2 position)
        {
            this.objectPosition = position;
            stationaryObject = true;
        }

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

            stationaryObject = false;

        }

        public Orbit(Texture2D apTexture, Texture2D peTexture, Planet planetOrbiting, Vector2 objectPosition, Vector2 objectVelocity, SimClock clock, GraphicsDevice graphicsDevice)
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

        public void Update(Vector2 objectAcceleration)
        {
            if (stationaryObject)
            {
                return;
            }
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
            float timePassed = (float)timeSinceStoppedPhysics;

            float tAnomaly = 0;
            if (e <= 1)
            {
                // Elliptical orbits
                float mAnomaly = (float)Math.Sqrt(mu / Math.Pow(semiMajorAxis, 3)) * timePassed * -Math.Sign(aMomentum) + m0; // mean anomaly

                float eAnomaly = (float)getEccentricAnomaly(mAnomaly, e, mAnomaly); // eccentric anomaly

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

        public void Draw(SpriteBatch spriteBatch, Matrix transform)
        {
            if (stationaryObject)
            {
                return;
            }
            generatePath(1000);
            if (path != null)
            {
                basicEffect.View = transform;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, path, 0, path.Length - 1);
            }
            if (apTexture != null && peTexture != null)
            {
                if (e < 1)
                {
                    Vector2 apPos = new Vector2((float)Math.Cos(argP + MathHelper.Pi) * rApoapsis, -(float)Math.Sin(argP + MathHelper.Pi) * rApoapsis);
                    spriteBatch.Draw(apTexture, Vector2.Transform(apPos + planetOrbiting.getPosition(), transform), null, Color.White, 0f, new Vector2(apTexture.Width / 2, 0), 0.5f, SpriteEffects.None, 0f);
                }
                Vector2 pePos = new Vector2((float)Math.Cos(argP) * rPeriapsis, -(float)Math.Sin(argP) * rPeriapsis);
                spriteBatch.Draw(peTexture, Vector2.Transform(pePos + planetOrbiting.getPosition(), transform), null, Color.White, 0f, new Vector2(peTexture.Width / 2, 0), 0.5f, SpriteEffects.None, 0f);
            }
        }

        // Recalculates the rocket's orbital parameters from its velocity and position vectors
        void calculateParameters()
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
        void generatePath(int numPoints)
        {
            path = new VertexPositionColor[e > 1 ? numPoints : numPoints + 1];

            float rMax = planetOrbiting.getSOI();
            float tMax = MathHelper.Pi;

            // Limit angle for hyperbolic trajectories to a max radius
            if (e > 1 && rMax > 0)
            {
                tMax = (float)Math.Acos((semiMajorAxis * (1 - e * e) - rMax) / (e * rMax));
            }

            float step = (tMax * 2) / numPoints;

            int i = 0;

            for (float t = -tMax; t <= tMax; t += step)
            {
                float r = getRadiusAtAngle(semiMajorAxis, e, t);
                VertexPositionColor point = new VertexPositionColor(new Vector3(r * (float)Math.Cos(t + argP) + planetOrbiting.getPosition().X, -r * (float)Math.Sin(t + argP) + planetOrbiting.getPosition().Y, 0), Color.White);
                if (r > planetOrbiting.getSOI() && planetOrbiting.getSOI() != 0)
                {
                    point.Color = Color.Black;
                }
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
        void transitionToNoPhysics()
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
        float getRadiusAtAngle(float a, float e, float theta)
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
        public static float getMagnitude(Vector2 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        public Vector2 getPosition()
        {
            return stationaryObject ? objectPosition : objectPosition + planetOrbiting.getPosition();
        }

        public Vector2 getRelativePosition()
        {
            return objectPosition;
        }

        public Vector2 getVelocity()
        {
            return stationaryObject ? objectVelocity : objectVelocity + planetOrbiting.getVelocity();
        }

        public Vector2 getRelativeVelocity()
        {
            return objectVelocity;
        }

        public float getSemiMajorAxis()
        {
            return semiMajorAxis;
        }

        // Returns the rockets height above the planets surface in meters
        public float getHeight()
        {
            return radius - planetOrbiting.getRadius();
        }

        // Returns the highest point above the surface tha the rocket will reach in meters
        public float getApoapsisHeight()
        {
            return rApoapsis - planetOrbiting.getRadius() > 0 ? rApoapsis - planetOrbiting.getRadius() : float.NaN;
        }

        // Returns the lowest point above the surface tha the rocket will reach in meters
        public float getPeriapsisHeight()
        {
            return rPeriapsis - planetOrbiting.getRadius();
        }

        // Returns the magnitude of the rocket's velocity
        public float getVelocityMagnitude()
        {
            return vMagnitude;
        }

        // Returns the period of the rockets current orbit in seconds
        public float getPeriod()
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
            Debug.WriteLine(objectPosition + " " + planetOrbiting.getPosition());
            objectPosition = objectPosition + planetOrbiting.getPosition() - planet.getPosition();
            objectVelocity = objectVelocity + planetOrbiting.getVelocity() - planet.getVelocity();
            planetOrbiting = planet;
            clock.setTimeFactor(1);
            Update(new Vector2());

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
