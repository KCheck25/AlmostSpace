using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.ComponentModel;

namespace AlmostSpace.Things
{
    // Represents the rocket that the user controls
    internal class Rocket
    {
        // Fields for tracking the rocket's current state
        // Always updated
        Vector2 velocity;
        Vector2 position;

        // Eccentricity vector
        Vector2 eV;

        // Only updated when physics
        Vector2 forceGravity;
        float planetAngle;

        float angle;
        float mass;

        Texture2D texture;
        Texture2D orbitTexture;
        Texture2D apIndicator;
        Texture2D peIndicator;
        List<OrbitSprite> orbit;

        Planet planetOrbiting;

        // Always updated
        float rPeriapsis;
        float rApoapsis;
        float vMagnitude;
        float period;
        float argP;
        float radius;
        float semiMajorAxis;
        float semiMinorAxis;
        float e;
        float mu;
        float aMomentum;

        double timeSinceStoppedPhysics;

        float universalGravity = 6.67E-11f;

        // only updated when switching from physics to no physics
        float m0;

        bool physicsJustStopped = true;
        bool spaceToggle = true;
        bool engineOn = false;

        public double totalTimeElapsed;

        public float timeFactor;

        float engineThrust = 5000f;
        float throttle = 1;

        SimClock clock;

        // Constructs a new Rocket object with the given texture, orbit
        // segment texture, mass, and the planet it starts around.
        public Rocket(Texture2D texture, Texture2D orbitTexture, Texture2D apIndicator, Texture2D peIndicator, float mass, Planet startingPlanet, SimClock clock)
        {
            this.orbit = new List<OrbitSprite>();
            this.texture = texture;
            this.orbitTexture = orbitTexture;
            this.mass = mass;
            this.angle = 0f;
            velocity = new Vector2(8000f, 0f);
            position = new Vector2(50, 6500000);
            this.planetOrbiting = startingPlanet;
            this.clock = clock;
            this.apIndicator = apIndicator;
            this.peIndicator = peIndicator;
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
        public float getVelocity()
        {
            return vMagnitude;
        }

        // Returns the period of the rockets current orbit in seconds
        public float getPeriod()
        {
            return period;
        }

        // Returns the rocket's current throttle as a percentage
        public float getThrottle()
        {
            return (float)Math.Round(throttle * 100);
        }

        // Returns a string of whether the engine is on or off
        public String getEngineState()
        {
            return engineOn ? "On" : "Off";
        }

        // Returns a vector representing the force of gravity acting
        // on the rocket in the x and y directions
        void updatePhysics()
        {
            Vector2 planetPosition = planetOrbiting.getPosition();
            float massPlanet = planetOrbiting.getMass();
            float xDist = (position.X - planetPosition.X);
            float yDist = -(position.Y - planetPosition.Y);
            float distToCenter = (float)Math.Pow(Math.Pow(xDist, 2) + Math.Pow(yDist, 2), 0.5);
            radius = distToCenter;

            if (distToCenter < 10)
            {
                forceGravity.X = 0;
                forceGravity.Y = 0;
                return;
            }

            float totalForce = (universalGravity * massPlanet * mass) / (distToCenter * distToCenter);
            planetAngle = (float)Math.Atan2(yDist, xDist);

            float xForce = -totalForce * (float)Math.Cos(planetAngle);
            float yForce = totalForce * (float)Math.Sin(planetAngle);

            forceGravity.X = xForce;
            forceGravity.Y = yForce;

            updateOrbit();
        }

        // Recalculates the rocket's orbital parameters from its velocity and position vectors
        public void updateOrbit()
        {
            // mu is the standard gravitational parameter of the planet that's being orbited
            mu = universalGravity * planetOrbiting.getMass();
            float velocityMagnitude = getMagnitude(velocity);
            vMagnitude = velocityMagnitude;

            // Using vis-viva equation but solving for the semi major axis (a): https://en.wikipedia.org/wiki/Vis-viva_equation
            semiMajorAxis = -1f / ((velocityMagnitude * velocityMagnitude / mu) - (2 / radius));

            period = (float)(2 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / mu));

            
            // Equations taken from here: https://space.stackexchange.com/questions/2562/2d-orbital-path-from-state-vectors
            aMomentum = position.X * velocity.Y - position.Y * velocity.X; // angular momentum

            // eccentricity vector
            float eX = (velocity.Y * aMomentum) / mu - position.X / radius;
            float eY = (-velocity.X * aMomentum) / mu - position.Y / radius;
            eV = new Vector2(eX, eY);
            e = getMagnitude(eV);

            // argument of periapsis - angle from planet to periapsis
            argP = -(float)Math.Atan2(eV.Y, eV.X);

            // Apoapsis and periapsis radiuses
            rApoapsis = semiMajorAxis * (1 + e);
            rPeriapsis = semiMajorAxis * (1 - e);

            semiMinorAxis = semiMajorAxis * (float)Math.Sqrt(1 - e * e);

            // Draw orbit to screen
            //generateTrajectory(semiMinorAxis, semiMajorAxis, 0, 0, -argP + MathHelper.PiOver2);
            genTrajectory2(semiMajorAxis, e, argP);
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

        // Recalculates the rocket's position in space and velocity based on its current orbital parameters
        // Allows time to be sped up without losing precision
        public void updateNoPhysics()
        {

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
            velocity.X = (float)(Math.Cos(-argP - MathHelper.PiOver2) * vX - Math.Sin(-argP - MathHelper.PiOver2) * vY);
            velocity.Y = (float)(Math.Sin(-argP - MathHelper.PiOver2) * vX + Math.Cos(-argP - MathHelper.PiOver2) * vY);

            position.X = distAtAnomaly * (float)Math.Cos(tAnomaly + argP);
            position.Y = -distAtAnomaly * (float)Math.Sin(tAnomaly + argP);

        }

        public void oldEquations()
        {
            // Math taken from: https://physics.stackexchange.com/questions/99094/using-2d-position-velocity-and-mass-to-determine-the-parametric-position-equat
            float semiMajorAxis = 0;
            float mu = 0;
            float velocityMagnitude = 0;

            float tanV = (position.X * velocity.Y - position.Y * velocity.X) / radius;
            float e = (float)Math.Pow(1 + (radius * tanV * tanV / mu) * (radius * velocityMagnitude * velocityMagnitude / mu - 2), 0.5);
            float radV = (position.X * velocity.X + position.Y * velocity.Y) / radius;
            float angle = Math.Sign(tanV * radV) * (float)Math.Acos((semiMajorAxis * (1 - e * e) - radius) / (e * radius)) - (float)Math.Atan2(position.Y, position.X);

        }

        // Checks for direction and throttle keyboard inputs and updates the
        // velocity and position of the rocket based on the forces acting on it.
        // Takes the time since the last frame as a parameter to make sure
        // calculations are based on real time.
        public void Update()
        {
            var kState = Keyboard.GetState();

            // Stop and start engine
            if (spaceToggle && kState.IsKeyDown(Keys.Space))
            {
                engineOn = !engineOn;
                spaceToggle = false;
            }
            if (kState.IsKeyUp(Keys.Space))
            {
                spaceToggle = true;
            }

            if (clock.getTimeStopped())
            {
                return;
            }

            if (kState.IsKeyDown(Keys.LeftShift) && throttle < 1)
            {
                throttle = throttle > 0.99f ? 1 : throttle + 0.01f;
            }

            if (kState.IsKeyDown(Keys.LeftControl) && throttle > 0)
            {
                throttle = throttle < 0.01f ? 0 : throttle - 0.01f;
            }

            if (kState.IsKeyDown(Keys.X))
            {
                throttle = 0;
            }

            if (kState.IsKeyDown(Keys.Z))
            {
                throttle = 1;
            }

            if (kState.IsKeyDown(Keys.A))
            {
                angle -= 3 * clock.getFrameTime();
            }

            if (kState.IsKeyDown(Keys.D))
            {
                angle += 3 * clock.getFrameTime();
            }

            if (engineOn && clock.getTimeFactor() == 1)
            {

                updatePhysics();

                forceGravity.X += engineThrust * throttle * (float)Math.Cos(angle);
                forceGravity.Y += engineThrust * throttle * (float)Math.Sin(angle);
                physicsJustStopped = true;


                velocity.X += forceGravity.X / mass * clock.getFrameTime();
                velocity.Y += forceGravity.Y / mass * clock.getFrameTime();

                position.X += velocity.X * clock.getFrameTime();
                position.Y += velocity.Y * clock.getFrameTime();

            }
            else
            {

                if (physicsJustStopped)
                {
                    updatePhysics();
                    transitionToNoPhysics();
                    physicsJustStopped = false;
                }
                timeSinceStoppedPhysics += clock.getFrameTime();
                if (timeSinceStoppedPhysics > 100000)
                {
                    physicsJustStopped = true;
                }

                updateNoPhysics();

            }

        }

        // Draws the rocket sprite and orbit approximation to the screen
        // using the given SpriteBatch object
        public void Draw(SpriteBatch spriteBatch, Matrix transform)
        {
            foreach (OrbitSprite pixel in orbit)
            {
                pixel.Draw(spriteBatch, transform);
            }
            
            if (e < 1)
            {
                Vector2 apPos = new Vector2((float)Math.Cos(argP + MathHelper.Pi) * rApoapsis, -(float)Math.Sin(argP + MathHelper.Pi) * rApoapsis);
                spriteBatch.Draw(apIndicator, Vector2.Transform(apPos, transform), null, Color.White, 0f, new Vector2(apIndicator.Width / 2, 0), 0.5f, SpriteEffects.None, 0f);
            }
            Vector2 pePos = new Vector2((float)Math.Cos(argP) * rPeriapsis, -(float)Math.Sin(argP) * rPeriapsis);
            spriteBatch.Draw(peIndicator, Vector2.Transform(pePos, transform), null, Color.White, 0f, new Vector2(peIndicator.Width / 2, 0), 0.5f, SpriteEffects.None, 0f);

            spriteBatch.Draw(texture, Vector2.Transform(position, transform), null, Color.White, angle + MathHelper.PiOver2, new Vector2(14f, 19f), Vector2.One, SpriteEffects.None, 0f);
        }

        // Generates a list of OrbitSprite objects arranged in the rocket's trajectory
        // using the given semi-major axis (a), eccentricity (e), and argument of periapsis (argP)
        public void genTrajectory2(float a, float e, float argP)
        {
            int numPoints = 1000;
            Vector2[] points = new Vector2[numPoints];

            float rMax = planetOrbiting.getRadius() * 10;
            float tMax = MathHelper.Pi;

            // Limit angle for hyperbolic trajectories to a max radius
            if (e >= 1)
            {
                tMax = (float)Math.Acos((a * (1 - e * e) - rMax) / (e * rMax));
            }

            float step = (tMax * 2) / numPoints;

            int i = 0;

            for (float t = -tMax; t <= tMax; t += step)
            {
                float r = getRadiusAtAngle(a, e, t);
                if (i < numPoints)
                {
                    points[i] = new Vector2(r * (float)Math.Cos(t + argP), -r * (float)Math.Sin(t + argP));
                }
                i++;
            }

            orbit = new List<OrbitSprite>();
            for (int j = 0; j < numPoints; j++)
            {
                orbit.Add(new OrbitSprite(orbitTexture, points[j]));
            }
        }

        // Gets the radius of the rocket at a given angle
        public float getRadiusAtAngle(float a, float e, float theta)
        {
            return a * (1 - e * e) / (1 + e * (float)Math.Cos(theta));
        }

        // Returns the magnitude of the given vector
        float getMagnitude(Vector2 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y); 
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

        // Converts the given angle in radians to degrees
        static float degrees(float angle)
        {
            return angle * 180 / MathHelper.Pi;
        }

    }
}
