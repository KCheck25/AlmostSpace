﻿using Microsoft.Xna.Framework.Graphics;
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
    internal class Rocket
    {
        Vector2 velocity;
        Vector2 position;
        float angle;
        float mass;

        Texture2D texture;
        Texture2D orbitTexture;
        List<OrbitSprite> orbit;

        Planet planetOrbiting;

        public Rocket(Texture2D texture, Texture2D orbitTexture, float mass, Planet startingPlanet, GraphicsDevice graphicsDevice)
        {
            this.orbit = new List<OrbitSprite>();
            this.texture = texture;
            this.orbitTexture = orbitTexture;
            this.mass = mass;
            this.angle = 0f;
            velocity = new Vector2(30, 0);
            position = new Vector2(900, 200);
            this.planetOrbiting = startingPlanet;
            calculateOrbit(200, 300, (int)startingPlanet.getPosition().X, (int)startingPlanet.getPosition().Y);
        }

        Vector2 computeForce()
        {
            Vector2 planetPosition = planetOrbiting.getPosition();
            float massPlanet = planetOrbiting.getMass();
            float xDist = (position.X - planetPosition.X);
            float yDist = -(position.Y - planetPosition.Y);
            float distToCenter = (float)Math.Pow(Math.Pow(xDist, 2) + Math.Pow(yDist, 2), 0.5);
            //Debug.WriteLine("Distance:" + xDist);

            if (distToCenter < 10)
            {
                return new Vector2(0, 0);
            }

            float universalGravity = 6.67E-11f;
            float totalForce = (universalGravity * massPlanet * mass) / (distToCenter * distToCenter);
            float angleToPlanet = (float)System.Math.Atan2(yDist, xDist);

            //if (yDist < 0 && xDist < 0 || yDist > 0 && xDist < 0)
            //{
            //    angle += MathHelper.Pi;
            //}

            float xForce = -totalForce * (float)System.Math.Cos(angleToPlanet);
            float yForce = totalForce * (float)System.Math.Sin(angleToPlanet);

            return new Vector2(xForce, yForce);
        }

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
            //orbit.Add(new OrbitSprite(orbitTexture, position));
        }

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

        public void calculateOrbit(int a, int b, int h, int k)
        {
            int numPoints = 100;
            Vector2[] points = new Vector2[numPoints];

            float step = MathHelper.TwoPi / numPoints;
            Debug.WriteLine(step);

            int i = 0;
            for (float t = -MathHelper.Pi; t < MathHelper.Pi; t += step)
            {
                points[i] = new Vector2((int)(h + a * (float)Math.Cos((double)t)), (int)(k + b * (float)Math.Sin((double)t)));
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
