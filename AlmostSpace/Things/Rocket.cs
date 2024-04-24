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

        bool soiChange = false;

        public float timeFactor;

        float engineThrust = 5000f;
        float throttle = 1;

        Planet justLeft;
        
        // Constructs a new Rocket object with the given texture, orbit
        // segment texture, mass, and the planet it starts around.
        public Rocket(Texture2D texture, Texture2D apIndicator, Texture2D peIndicator, GraphicsDevice graphicsDevice, float mass, Planet startingPlanet, SimClock clock) : base(apIndicator, peIndicator, startingPlanet, new Vector2D(50, 6500000), new Vector2D(11000, 0), clock, graphicsDevice)
        {
            this.texture = texture;
            this.mass = mass;
            this.angle = 0f;

            base.Update(new Vector2D());
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
                base.Update(new Vector2D(Math.Cos(angle) * engineThrust * throttle / mass, Math.Sin(angle) * engineThrust * throttle / mass));
            }
            else
            {
                base.Update();
                //orbit.generatePath(1000);
            }

            double planetSOI = getPlanetOrbiting().getSOI();

            if (soiChange)
            {
                if (Math.Abs((getPosition() - justLeft.getPosition()).Length() - justLeft.getSOI()) > justLeft.getSOI() * 0.05)
                {
                    soiChange = false;

                } 
            }

            // Check if rocket exits current planet / moon's sphere of influence
            if (getOrbitRadius() > planetSOI && planetSOI != 0 && !soiChange)
            {
                justLeft = getPlanetOrbiting();
                setPlanetOrbiting(getPlanetOrbiting().getPlanetOrbiting());
                soiChange = true;
            }

            // Check if rocket enters a sphere of influence within the current sphere of influence
            foreach (Planet planet in getPlanetOrbiting().getChildren())
            {
                if ((getPosition() - planet.getPosition()).Length() < planet.getSOI() && !soiChange)
                {
                    justLeft = planet;
                    setPlanetOrbiting(planet);
                    soiChange = true;
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
            spriteBatch.Draw(texture, Vector2D.Transform(getPosition(), transform).getVector2(), null, Color.White, angle + MathHelper.PiOver2, new Vector2(14f, 19f), Vector2.One, SpriteEffects.None, 0f);
        }

    }
}
