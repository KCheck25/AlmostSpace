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
        public float zenith = 0;

        // Constructs a new Rocket object with the given texture, orbit
        // segment texture, mass, and the planet it starts around.
        public Rocket(Texture2D texture, Texture2D orbitTexture, float mass, Planet startingPlanet)
        {
            this.orbit = new List<OrbitSprite>();
            this.texture = texture;
            this.orbitTexture = orbitTexture;
            this.mass = mass;
            this.angle = 0f;
            velocity = new Vector2(30, 0);
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
            float zenith = MathHelper.Pi - angleToPlanet - velocityAngle;
            this.zenith = zenith * 180 / MathHelper.Pi;
            angle = MathHelper.Pi - angleToPlanet;
        }

        // Checks for direction and throttle keyboard inputs and updates the
        // velocity and position of the rocket based on the forces acting on it.
        // Takes the time since the last frame as a parameter to make sure
        // calculations are based on real time.
        public void Update(double frameTime)
        {
            Vector2 forces = computeForce();
            float enginePower = 100f;

            var kState = Keyboard.GetState();
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
            orbit.Add(new OrbitSprite(orbitTexture, position));
        }

        // Draws the rocket sprite and orbit approximation to the screen
        // using the given SpriteBatch object
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (OrbitSprite pixel in orbit)
            {
                pixel.Draw(spriteBatch);
            }
            spriteBatch.Begin();
            spriteBatch.Draw(texture, position, null, Color.White, angle + MathHelper.PiOver2, new Vector2(14f, 19f), Vector2.One, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        // Generates a list of OrbitSprite objects arranged in an elipse with
        // the given semi-major and semi-minor axes (a and b), and the given
        // origin of the elipse (h and k)
        //TODO allow the elipse to be rotated, and generate with the planet coordinates at a focus
        public void generateTrajectory(int a, int b, int h, int k)
        {
            int numPoints = 500;
            Vector2[] points = new Vector2[numPoints];

            float step = MathHelper.TwoPi / numPoints;
            Debug.WriteLine(step);

            int i = 0;
            for (float t = -MathHelper.Pi; t <= MathHelper.Pi; t += step)
            {
                if (i <= 499)
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
                Debug.WriteLine(points[j]);
            }

            Debug.WriteLine(orbit.Count);

        }

    }
}
