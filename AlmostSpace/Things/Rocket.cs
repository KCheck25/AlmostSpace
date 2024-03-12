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
        Vector2 velocity;
        Vector2 position;

        Vector2 forceGravity;

        float angle;
        float mass;

        Texture2D texture;
        Texture2D orbitTexture;
        List<OrbitSprite> orbit;

        Planet planetOrbiting;

        float radius;

        float rPeriapsis;
        float rApoapsis;
        float vMagnitude;
        float period;

        bool spaceToggle = true;
        bool timeStopped = false;

        // Constructs a new Rocket object with the given texture, orbit
        // segment texture, mass, and the planet it starts around.
        public Rocket(Texture2D texture, Texture2D orbitTexture, float mass, Planet startingPlanet)
        {
            this.orbit = new List<OrbitSprite>();
            this.texture = texture;
            this.orbitTexture = orbitTexture;
            this.mass = mass;
            this.angle = 0f;
            velocity = new Vector2(40f, 0);
            position = new Vector2(0, 200);
            this.planetOrbiting = startingPlanet;
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
        void computeGravity()
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

            float universalGravity = 6.67E-11f;
            float totalForce = (universalGravity * massPlanet * mass) / (distToCenter * distToCenter);
            float angleToPlanet = (float)Math.Atan2(yDist, xDist);

            float xForce = -totalForce * (float)Math.Cos(angleToPlanet);
            float yForce = totalForce * (float)Math.Sin(angleToPlanet);

            updateOrbit();

            forceGravity.X = xForce;
            forceGravity.Y = yForce;
        }

        // Recalculates the rocket's orbital parameters from its velocity and position vectors
        public void updateOrbit()
        {
            // This stuff works!!!
            // Using vis-viva equation but solving for a: https://en.wikipedia.org/wiki/Vis-viva_equation
            float universalGravity = 6.67E-11f;
            float mu = universalGravity * planetOrbiting.getMass();
            float velocityMagnitude = getMagnitude(velocity);
            vMagnitude = velocityMagnitude;

            float semiMajorAxis = -1f / ((velocityMagnitude * velocityMagnitude / mu) - (2 / radius));
            period = (float)(2 * Math.PI * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / mu));

            // Math taken from: https://physics.stackexchange.com/questions/99094/using-2d-position-velocity-and-mass-to-determine-the-parametric-position-equat
            // Might be kind of cheating because its basically my project? Ill ask.
            float tanV = (position.X * velocity.Y - position.Y * velocity.X) / radius;

            float e = (float)Math.Pow(1 + (radius * tanV * tanV / mu) * (radius * velocityMagnitude * velocityMagnitude / mu - 2), 0.5);

            rApoapsis = semiMajorAxis * (1 + e);
            rPeriapsis = semiMajorAxis * (1 - e);

            float semiMinorAxis = semiMajorAxis * (float)Math.Sqrt(1 - e * e);

            float radV = (position.X * velocity.X + position.Y * velocity.Y) / radius;
            float angle = Math.Sign(tanV * radV) * (float)Math.Acos((semiMajorAxis * (1 - e * e) - radius) / (e * radius)) - (float)Math.Atan2(position.Y, position.X);

            generateTrajectory(semiMinorAxis, semiMajorAxis, 0, 0, MathHelper.PiOver2 - angle);
        }

        // Checks for direction and throttle keyboard inputs and updates the
        // velocity and position of the rocket based on the forces acting on it.
        // Takes the time since the last frame as a parameter to make sure
        // calculations are based on real time.
        public void Update(double frameTime)
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

            computeGravity();

            float enginePower = 500f;

            if (kState.IsKeyDown(Keys.Left))
            {
                angle -= 10 * (float)frameTime;
            }

            if (kState.IsKeyDown(Keys.Right))
            {
                angle += 10 * (float)frameTime;
            }

            if (kState.IsKeyDown(Keys.Up))
            {
                forceGravity.X += enginePower * (float)Math.Cos(angle);
                forceGravity.Y += enginePower * (float)Math.Sin(angle);
            }

            velocity.X += forceGravity.X / mass * (float)frameTime;
            velocity.Y += forceGravity.Y / mass * (float)frameTime;

            position.X += velocity.X * (float)frameTime;
            position.Y += velocity.Y * (float)frameTime;
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

    }
}
