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

namespace AlmostSpace.Things
{
    // Represents the rocket that the user controls
    internal class Rocket
    {
        // Fields for tracking the rocket's current state
        Vector2 velocity;
        Vector2 position;

        public Vector2 forceGravity;

        float angle;
        float mass;

        Texture2D texture;
        Texture2D orbitTexture;
        List<OrbitSprite> orbit;

        Planet planetOrbiting;

        public float height;
        public float periapsis;
        public float apoapsis;
        public float dispVelocity;
        public float angleToPlanetDeg;
        public float angleDeg;
        public float period;

        public float zenithDisp = 0;

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
            generateTrajectory(200, 200, (int)startingPlanet.getPosition().X, (int)startingPlanet.getPosition().Y, 0);
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
            height = distToCenter;
            //Debug.WriteLine("Distance:" + xDist);

            if (distToCenter < 10)
            {
                forceGravity.X = 0;
                forceGravity.Y = 0;
                return;
            }

            float universalGravity = 6.67E-11f;
            float totalForce = (universalGravity * massPlanet * mass) / (distToCenter * distToCenter);
            float angleToPlanet = (float)Math.Atan2(yDist, xDist);

            //if (yDist < 0 && xDist < 0 || yDist > 0 && xDist < 0)
            //{
            //    angle += MathHelper.Pi;
            //}

            float xForce = -totalForce * (float)Math.Cos(angleToPlanet);
            float yForce = totalForce * (float)Math.Sin(angleToPlanet);

            updateOrbit2();

            forceGravity.X = xForce;
            forceGravity.Y = yForce;
        }

        public void updateOrbit2()
        {
            // This stuff works!!!
            // Using vis-viva equation but solving for a: https://en.wikipedia.org/wiki/Vis-viva_equation
            float universalGravity = 6.67E-11f;
            float mu = universalGravity * planetOrbiting.getMass();
            float velocityMagnitude = (float)Math.Sqrt((velocity.X * velocity.X) + (velocity.Y * velocity.Y));
            dispVelocity = velocityMagnitude;

            float majorAxis = -1f / ((velocityMagnitude * velocityMagnitude / mu) - (2 / height));
            //apoapsis = majorAxis;
            period = (float)(2 * Math.PI * Math.Sqrt(Math.Pow(majorAxis, 3) / mu));

            // This stuff still is a bit sad :(

            // Position -> vector from planet to satellite
            // Velocity -> derivative of position

            // Math taken from: https://physics.stackexchange.com/questions/99094/using-2d-position-velocity-and-mass-to-determine-the-parametric-position-equat
            // Might be kind of cheating because its basically my project? Ill ask.
            float tanV = (position.X * velocity.Y - position.Y * velocity.X) / height;

            float e = (float)Math.Pow(1 + (height * tanV * tanV / mu) * (height * velocityMagnitude * velocityMagnitude / mu - 2), 0.5);
            //Debug.WriteLine(e);

            apoapsis = majorAxis * (1 - e);
            periapsis = majorAxis * (1 + e);

            float semiMinorAxis = majorAxis * (float)Math.Sqrt(1 - e * e);

            float radV = (position.X * velocity.X + position.Y * velocity.Y) / height;
            float angle = Math.Sign(tanV * radV) * (float)Math.Acos((majorAxis * (1 - e * e) - height) / (e * height)) - (float)Math.Atan2(position.Y, position.X);
            Debug.WriteLine(angle);

            generateTrajectory(semiMinorAxis, majorAxis, 0, 0, angle + MathHelper.PiOver2);

            /*
            Vector2 angularVector = position * velocity;
            Vector2 eccentricityVector = ((velocity * angularVector) / mu) - (position / getVectorMagnitude(position));
            Debug.WriteLine(eccentricityVector);
            float eccentricityMagnitude = getVectorMagnitude(eccentricityVector);
            //apoapsis = eccentricityMagnitude;
            apoapsis = majorAxis * (1 - eccentricityMagnitude);
            */

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
            // draws trail behind rocket
            //orbit.Add(new OrbitSprite(orbitTexture, position));
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
        //TODO allow the elipse to be rotated, and generate with the planet coordinates at a focus
        public void generateTrajectory(float a, float b, float h, float k, float theta)
        {
            //TODO: Make rotation work lol
            /*
            float sMajor = Math.Max(a, b);
            float sMinor = Math.Min(a, b);

            float cDist = (float)Math.Pow(sMajor * sMajor - sMinor * sMinor, 0.5);

            h -= cDist * (float)Math.Sin(theta);
            k -= cDist * (float)Math.Cos(theta);
            */

            float c = (float)Math.Sqrt(b * b - a * a);
            k -= c;

            int numPoints = 1000;
            Vector2[] points = new Vector2[numPoints];

            float step = MathHelper.TwoPi / numPoints;
            //Debug.WriteLine(step);

            int i = 0;
            for (float t = -MathHelper.Pi; t <= MathHelper.Pi; t += step)
            {
                if (i <= numPoints - 1)
                {
                    points[i] = new Vector2((int)(h + a * (float)Math.Cos((double)t)), (int)(k + b * (float)Math.Sin((double)t)));
                }
                //Debug.WriteLine(points[i]);
                i++;
            }


            orbit = new List<OrbitSprite>();
            for (int j = 0; j < numPoints; j++)
            {
                orbit.Add(new OrbitSprite(orbitTexture, points[j]));
                //Debug.WriteLine(points[j]);
            }

            //Debug.WriteLine(orbit.Count);

        }

        public float wrapAngle(float angle)
        {
            return angle % MathHelper.TwoPi;
        }

        float getVectorMagnitude(Vector2 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y); 
        }

    }
}
