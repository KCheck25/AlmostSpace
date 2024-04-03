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
using System.Runtime.ExceptionServices;
using System.ComponentModel;
using System.Transactions;
using System.Net.Sockets;

namespace AlmostSpace.Things
{
    // Represents the rocket that the user controls
    internal class Rocket : Orbit
    {

        float angle;
        float mass;

        Texture2D texture;

        bool spaceToggle = true;
        bool engineOn = false;

        public float timeFactor;

        float engineThrust = 5000f;
        float throttle = 1;

        // Constructs a new Rocket object with the given texture, orbit
        // segment texture, mass, and the planet it starts around.
        public Rocket(Texture2D texture, Texture2D apIndicator, Texture2D peIndicator, GraphicsDevice graphicsDevice, float mass, Planet startingPlanet, SimClock clock) : base(apIndicator, peIndicator, startingPlanet, new Vector2(50, 6500000), new Vector2(1000f, 0f), clock, graphicsDevice)
        {
            this.texture = texture;
            this.mass = mass;
            this.angle = 0f;

            base.Update(new Vector2());
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

        // Checks for direction and throttle keyboard inputs and updates the
        // velocity and position of the rocket based on the forces acting on it.
        // Takes the time since the last frame as a parameter to make sure
        // calculations are based on real time.
        public new void Update()
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

            if (getClock().getTimeStopped())
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
                angle -= 3 * getClock().getFrameTime();
            }

            if (kState.IsKeyDown(Keys.D))
            {
                angle += 3 * getClock().getFrameTime();
            }

            if (engineOn && getClock().getTimeFactor() == 1)
            {
                base.Update(new Vector2((float)Math.Cos(angle) * engineThrust * throttle / mass, (float)Math.Sin(angle) * engineThrust * throttle / mass));
            }
            else
            {
                base.Update();
                //orbit.generatePath(1000);
            }

            // Check if rocket exits current planet / moon's sphere of influence
            if (getHeight() > getPlanetOrbiting().getSOI() && getPlanetOrbiting().getSOI() != 0)
            {
                setPlanetOrbiting(getPlanetOrbiting().getPlanetOrbiting());
            }

            // Check if rocket enters a sphere of influence within the current sphere of influence
            foreach (Planet planet in getPlanetOrbiting().getChildren())
            {
                if (Orbit.getMagnitude(planet.getPosition() - getPosition()) < planet.getSOI())
                {
                    setPlanetOrbiting(planet);
                    break;
                }
            }
            
        }

        // Draws the rocket sprite and orbit approximation to the screen
        // using the given SpriteBatch object
        public new void Draw(SpriteBatch spriteBatch, Matrix transform)
        {
            // Draw rocket
            base.Draw(spriteBatch, transform);
            spriteBatch.Draw(texture, Vector2.Transform(getPosition(), transform), null, Color.White, angle + MathHelper.PiOver2, new Vector2(14f, 19f), Vector2.One, SpriteEffects.None, 0f);
        }

    }
}
