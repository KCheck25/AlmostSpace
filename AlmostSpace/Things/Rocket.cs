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

        float timeStoppedPhysics;

        float universalGravity = 6.67E-11f;

        // only updated when switching from physics to no physics
        float m0;

        bool physicsJustStopped = true;
        bool spaceToggle = true;
        bool timeStopped = false;

        bool periodPressed = false;
        bool commaPressed = false;

        public float totalTimeElapsed;

        float[] timeWarpLevels = { 1, 5, 10, 25, 50, 100, 500, 1000 };
        int timeWarpLevel = 0;

        public float timeFactor;

        // Constructs a new Rocket object with the given texture, orbit
        // segment texture, mass, and the planet it starts around.
        public Rocket(Texture2D texture, Texture2D orbitTexture, float mass, Planet startingPlanet)
        {
            this.orbit = new List<OrbitSprite>();
            this.texture = texture;
            this.orbitTexture = orbitTexture;
            this.mass = mass;
            this.angle = 0f;
            velocity = new Vector2(40f, 0f);
            position = new Vector2(0, 200);
            this.planetOrbiting = startingPlanet;
            timeFactor = timeWarpLevels[timeWarpLevel];
        }

        // Returns the rockets height above the planets surface in meters
        public float getHeight()
        {
            return radius - planetOrbiting.getRadius();
        }

        // Returns the highest point above the surface tha the rocket will reach in meters
        public float getApoapsisHeight()
        {
            return rApoapsis - planetOrbiting.getRadius();
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
            // This stuff works!!!
            // Using vis-viva equation but solving for a: https://en.wikipedia.org/wiki/Vis-viva_equation
            mu = universalGravity * planetOrbiting.getMass();
            float velocityMagnitude = getMagnitude(velocity);
            vMagnitude = velocityMagnitude;

            semiMajorAxis = -1f / ((velocityMagnitude * velocityMagnitude / mu) - (2 / radius));
            period = (float)(2 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / mu));

            // More equations https://space.stackexchange.com/questions/2562/2d-orbital-path-from-state-vectors

            float h = position.X * velocity.Y - position.Y * velocity.X;


            float eX = (velocity.Y * h) / mu - position.X / radius;
            float eY = (-velocity.X * h) / mu - position.Y / radius;
            eV = new Vector2(eX, eY);


            e = getMagnitude(eV);
            argP = -(float)Math.Atan2(eV.Y, eV.X);

            rApoapsis = semiMajorAxis * (1 + e);
            rPeriapsis = semiMajorAxis * (1 - e);

            semiMinorAxis = semiMajorAxis * (float)Math.Sqrt(1 - e * e);

            /*

            float currentTrueAnomaly = planetAngle - argP;

            float m0 = (float)(Math.Atan2(-Math.Sqrt(1 - e * e) * Math.Sin(currentTrueAnomaly), -e - Math.Cos(currentTrueAnomaly)) + MathHelper.Pi - e * Math.Sqrt(1 - e * e) * Math.Sin(currentTrueAnomaly) / (1 + e * Math.Cos(currentTrueAnomaly)));

            // mean anomaly at a time since periapsis
            float mAnomaly = (float)Math.Sqrt(mu / Math.Pow(semiMajorAxis, 3)) * 5 + m0;

            float eAnomaly = (float)getEccentricAnomaly(0, e, mAnomaly);
            float tAnomaly = 2 * (float)Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(eAnomaly / 2));
            float distAtAnomaly = semiMajorAxis * (1 - e * e) / (1 + e * (float)Math.Cos(tAnomaly)); // if argP is omitted, this becomes distance at a given time since periapsis

            //Debug.WriteLine("Height in 5 seconds: " + (distAtAnomaly - 150));
            //Debug.WriteLine("argP: " + degrees(tAnomaly) + " ta: " + degrees(currentTrueAnomaly));
            */
            generateTrajectory(semiMinorAxis, semiMajorAxis, 0, 0, -argP + MathHelper.PiOver2);
        }

        public void transitionToNoPhysics()
        {
            timeStoppedPhysics = totalTimeElapsed;
            float currentTrueAnomaly = planetAngle - argP;
            m0 = (float)(Math.Atan2(-Math.Sqrt(1 - e * e) * Math.Sin(currentTrueAnomaly), -e - Math.Cos(currentTrueAnomaly)) + MathHelper.Pi - e * Math.Sqrt(1 - e * e) * Math.Sin(currentTrueAnomaly) / (1 + e * Math.Cos(currentTrueAnomaly)));
        }

        public void updateNoPhysics(GameTime gameTime)
        {
            float timePassed = totalTimeElapsed - timeStoppedPhysics;

            float mAnomaly = (float)Math.Sqrt(mu / Math.Pow(semiMajorAxis, 3)) * timePassed + m0;

            float eAnomaly = (float)getEccentricAnomaly(0, e, mAnomaly);
            if (eAnomaly == -1)
            {
                Debug.WriteLine("Error: Could Not Find Root!");
                return;
            }

            float tAnomaly = 2 * (float)Math.Atan(Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(eAnomaly / 2));

            float distAtAnomaly = semiMajorAxis * (1 - e * e) / (1 + e * (float)Math.Cos(tAnomaly)); // if argP is omitted, this becomes distance at a given time since periapsis

            radius = distAtAnomaly;

            vMagnitude = (float)Math.Sqrt(mu * (2 / distAtAnomaly - 1 / semiMajorAxis));

            // https://physics.stackexchange.com/questions/669946/how-to-calculate-the-direction-of-the-velocity-vector-for-a-body-that-moving-is
            float vY = -(float)Math.Sin(tAnomaly) / (float)Math.Sqrt(1 + e * e + 2 * e * Math.Cos(tAnomaly)) * vMagnitude;
            float vX = (float)(e + Math.Cos(tAnomaly)) / (float)Math.Sqrt(1 + e * e + 2 * e * Math.Cos(tAnomaly)) * vMagnitude;


            // https://matthew-brett.github.io/teaching/rotation_2d.html
            velocity.X = (float)(Math.Cos(-argP - MathHelper.PiOver2) * vX - Math.Sin(-argP - MathHelper.PiOver2) * vY);
            velocity.Y = (float)(Math.Sin(-argP - MathHelper.PiOver2) * vX + Math.Cos(-argP - MathHelper.PiOver2) * vY);

            position.X = distAtAnomaly * (float)Math.Cos(tAnomaly + argP);
            position.Y = -distAtAnomaly * (float)Math.Sin(tAnomaly + argP);
        }

        public void oldEquations()
        {
            // Math taken from: https://physics.stackexchange.com/questions/99094/using-2d-position-velocity-and-mass-to-determine-the-parametric-position-equat
            // Might be kind of cheating because its basically my project? Ill ask.
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
        public void Update(GameTime gameTime)
        {
            var kState = Keyboard.GetState();

            if (spaceToggle && kState.IsKeyDown(Keys.Space))
            {
                timeStopped = !timeStopped;
                spaceToggle = false;
            }
            if (kState.IsKeyUp(Keys.Space))
            {
                spaceToggle = true;
            }

            if (timeStopped)
            {
                return;
            }


            float enginePower = 500f;

            if (kState.IsKeyDown(Keys.Left))
            {
                angle -= 3 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kState.IsKeyDown(Keys.Right))
            {
                angle += 3 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kState.IsKeyDown(Keys.Up))
            {
                totalTimeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

                updatePhysics();

                forceGravity.X += enginePower * (float)Math.Cos(angle);
                forceGravity.Y += enginePower * (float)Math.Sin(angle);
                physicsJustStopped = true;


                velocity.X += forceGravity.X / mass * (float)gameTime.ElapsedGameTime.TotalSeconds;
                velocity.Y += forceGravity.Y / mass * (float)gameTime.ElapsedGameTime.TotalSeconds;

                position.X += velocity.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
                position.Y += velocity.Y * (float)gameTime.ElapsedGameTime.TotalSeconds;

            }
            else
            {
                totalTimeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds * timeFactor;

                if (physicsJustStopped)
                {
                    updatePhysics();
                    transitionToNoPhysics();
                    physicsJustStopped = false;
                }

                updateNoPhysics(gameTime);

            }

            if (kState.IsKeyDown(Keys.OemPeriod) && !periodPressed)
            {
                if (timeWarpLevel < timeWarpLevels.Length - 1)
                {
                    timeWarpLevel++;
                }
                periodPressed = true;
            }

            if (kState.IsKeyDown(Keys.OemComma) && !commaPressed)
            {
                if (timeWarpLevel > 0)
                {
                    timeWarpLevel--;
                }
                commaPressed = true;
            }

            if (kState.IsKeyUp(Keys.OemPeriod))
            {
                periodPressed = false;
            }

            if (kState.IsKeyUp(Keys.OemComma))
            {
                commaPressed = false;
            }

            timeFactor = timeWarpLevels[timeWarpLevel];

        }

        // Draws the rocket sprite and orbit approximation to the screen
        // using the given SpriteBatch object
        public void Draw(SpriteBatch spriteBatch, Matrix transform)
        {
            foreach (OrbitSprite pixel in orbit)
            {
                pixel.Draw(spriteBatch, transform);
            }
            spriteBatch.Draw(texture, Vector2.Transform(position, transform), null, Color.White, angle + MathHelper.PiOver2, new Vector2(14f, 19f), Vector2.One, SpriteEffects.None, 0f);
        }

        // Generates a list of OrbitSprite objects arranged in an elipse with
        // the given semi-major and semi-minor axes (a and b), and the given
        // origin of the elipse (h and k)
        // Adapted from this helpful stack overflow dude's code: https://stackoverflow.com/questions/21511281/draw-an-ellipse-in-xna
        public void generateTrajectory(float a, float b, float h, float k, float theta)
        {
            
            float sMajor = Math.Max(a, b);
            float sMinor = Math.Min(a, b);

            float cDist = (float)Math.Pow(sMajor * sMajor - sMinor * sMinor, 0.5);

            h -= cDist * (float)Math.Sin(theta);
            k += cDist * (float)Math.Cos(theta);

            Matrix rotation = Matrix.CreateRotationZ(theta);

            int numPoints = 1000;
            Vector2[] points = new Vector2[numPoints];

            float step = MathHelper.TwoPi / numPoints;
            //Debug.WriteLine(step);

            int i = 0;
            for (float t = -MathHelper.Pi; t <= MathHelper.Pi; t += step)
            {
                if (i <= numPoints - 1)
                {
                    points[i] = Vector2.Transform(new Vector2((int)(a * (float)Math.Cos((double)t)), (int)(b * (float)Math.Sin((double)t))), rotation);
                }
                //Debug.WriteLine(points[i]);
                i++;
            }

            for (int j = 0; j < points.Length; j++)
            {
                points[j].X += h;
                points[j].Y += k;
            }


            orbit = new List<OrbitSprite>();
            for (int j = 0; j < numPoints; j++)
            {
                orbit.Add(new OrbitSprite(orbitTexture, points[j]));
                //Debug.WriteLine(points[j]);
            }

            //Debug.WriteLine(orbit.Count);

        }

        float getMagnitude(Vector2 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y); 
        }

        static double getEccentricAnomaly(double x, double eccentricity, double mAnomaly)
        {

            double h = func(x, eccentricity, mAnomaly) / derivFunc(x, eccentricity);
            int i = 0;
            while (Math.Abs(h) >= 0.0001)
            {
                h = func(x, eccentricity, mAnomaly) / derivFunc(x, eccentricity);

                // x(i+1) = x(i) - f(x) / f'(x) 
                x = x - h;

                i++;
                if (i > 100000)
                {
                    return -1;
                }
            }

            return Math.Round(x * 1000) / 1000;
        }

        static double func(double x, double eccentricity, double meanAnomaly)
        {
            return x - eccentricity * Math.Sin(x) - meanAnomaly;
        }

        static double derivFunc(double x, double eccentricity)
        {
            return 1 - eccentricity * Math.Cos(x);
        }

        static float degrees(float angle)
        {
            return angle * 180 / MathHelper.Pi;
        }

    }
}
