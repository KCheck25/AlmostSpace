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
            velocity = new Vector2(60, 0);
            position = new Vector2(900, 200);
            this.planetOrbiting = startingPlanet;
            generateTrajectory(200, 200, (int)startingPlanet.getPosition().X, (int)startingPlanet.getPosition().Y);
        }

        // Returns a vector representing the force of gravity acting
        // on the rocket in the x and y directions
        Vector2 computeForce()
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
                return new Vector2(0, 0);
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

            updateOrbit(angleToPlanet);

            return new Vector2(xForce, yForce);
        }


        public void updateOrbit(float angleToPlanet)
        {
            // no work :(
            float velocityAngle = (float)Math.Atan2(velocity.Y, velocity.X);
            float zenith = (angleToPlanet) - (MathHelper.TwoPi - velocityAngle);
            //angle = velocityAngle;
            angleDeg = (MathHelper.TwoPi - angle) * 180 / MathHelper.Pi;
            angleToPlanetDeg = (angleToPlanet) * 180 / MathHelper.Pi;
            this.zenithDisp = wrapAngle(zenith) * 180 / MathHelper.Pi + 360;
            
            float velocityMagnitude = (float)Math.Sqrt((velocity.X * velocity.X) + (velocity.Y * velocity.Y));

            float c = (2f * 6.67E-11f * planetOrbiting.getMass()) / (height * velocityMagnitude * velocityMagnitude);

            float first = -c / (2 * (1 - c));

            float second = (float)Math.Sqrt(c * c - 4 * (1 - c) * -Math.Pow(Math.Sin(zenith), 2)) / (2 * (1 - c));

            periapsis = (first - second) * height;

            apoapsis = (first + second) * height;

            dispVelocity = velocityMagnitude;

            float semiMajorAxis = (Math.Abs(apoapsis) + Math.Abs(periapsis)) / 2;

            float eccentricity = (2 * semiMajorAxis - 2 * Math.Min(Math.Abs(apoapsis), Math.Abs(periapsis))) / (2 * semiMajorAxis);

            float semiMinorAxis = semiMajorAxis * (float)Math.Sqrt(1 - eccentricity * eccentricity);
            //Debug.WriteLine(eccentricity);

            generateTrajectory(semiMajorAxis, semiMinorAxis, planetOrbiting.getPosition().X, planetOrbiting.getPosition().Y);

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

            Vector2 forces = computeForce();
            float enginePower = 100f;

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
                forces.X += enginePower * (float)Math.Cos(angle);
                forces.Y += enginePower * (float)Math.Sin(angle);
            }

            velocity.X += forces.X / mass;
            velocity.Y += forces.Y / mass;

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
        public void generateTrajectory(float a, float b, float h, float k)
        {
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

    }
}
