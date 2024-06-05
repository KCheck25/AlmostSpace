﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        float engineThrust = 5000f;
        float throttle = 1;
        
        // Constructs a new Rocket object with the given texture, orbit
        // segment texture, mass, and the planet it starts around.
        public Rocket(String name, Texture2D texture, Texture2D apIndicator, Texture2D peIndicator, GraphicsDevice graphicsDevice, float mass, Planet startingPlanet, SimClock clock) : base(name, "Rocket", apIndicator, peIndicator, startingPlanet, new Vector2D(50, 6500000), new Vector2D(8000, 0), clock, graphicsDevice)
        {
            this.texture = texture;
            this.mass = mass;
            this.angle = 0f;

            setPathColor(Color.Orange);

            base.Update(new Vector2D());
        }

        public Rocket(String data, List<Planet> planets, SimClock clock, GraphicsDevice graphicsDevice, Texture2D texture, Texture2D apIndicator, Texture2D peIndicator) : base(data, planets, clock, graphicsDevice, apIndicator, peIndicator)
        {
            this.texture = texture;

            String[] lines = data.Split("\n");
            foreach (String line in lines)
            {
                String[] components = line.Split(": ");
                if (components.Length == 2)
                {
                    switch (components[0])
                    {
                        case "Mass":
                            mass = float.Parse(components[1]);
                            break;
                        case "Angle":
                            angle = float.Parse(components[1]);
                            break;
                        case "Engine On":
                            engineOn = bool.Parse(components[1]);
                            break;
                        case "Engine Thrust":
                            engineThrust = float.Parse(components[1]);
                            break;
                        case "Throttle":
                            throttle = float.Parse(components[1]);
                            break;
                    }

                }

            }
            setPathColor(Color.Orange);

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

        public void setThrottle(float throttle)
        {
            this.throttle = throttle;
        }

        // Checks for direction and throttle keyboard inputs and updates the
        // velocity and position of the rocket based on the forces acting on it.
        // Takes the time since the last frame as a parameter to make sure
        // calculations are based on real time.
        public new void Update()
        {
            double planetSOI = getPlanetOrbiting().getSOI();

            // Check if rocket exits current planet / moon's sphere of influence
            if (getOrbitRadius() > planetSOI && planetSOI != 0)
            {
                setPlanetOrbiting(getPlanetOrbiting().getPlanetOrbiting());
            }


            // Check if rocket enters a sphere of influence within the current sphere of influence

            foreach (Planet planet in getPlanetOrbiting().getChildren())
            {
                if ((getPosition() - planet.getPosition()).Length() < planet.getSOI() * 0.99)
                {
                    setPlanetOrbiting(planet);
                    break;
                }
            }

            var kState = Keyboard.GetState();

            // Stop and start engine
            if (spaceToggle && kState.IsKeyDown(Keybinds.toggleEngine))
            {
                engineOn = !engineOn;
                spaceToggle = false;
            }
            if (kState.IsKeyUp(Keybinds.toggleEngine))
            {
                spaceToggle = true;
            }

            if (getClock().getTimeStopped())
            {
                return;
            }

            if (kState.IsKeyDown(Keybinds.increaseThrottle) && throttle < 10000)
            {
                throttle = throttle + 0.01f/*throttle > 0.99f ? 1 : throttle + 0.01f*/;
            }

            if (kState.IsKeyDown(Keybinds.decreaseThrottle) && throttle > 0)
            {
                throttle = throttle < 0.01f ? 0 : throttle - 0.01f;
            }

            if (kState.IsKeyDown(Keybinds.cutThrottle))
            {
                throttle = 0;
            }

            if (kState.IsKeyDown(Keybinds.fullThrottle))
            {
                throttle = 1;
            }

            if (kState.IsKeyDown(Keybinds.rotateRight))
            {
                angle -= 3 * getClock().getFrameTime();
            }

            if (kState.IsKeyDown(Keybinds.rotateLeft))
            {
                angle += 3 * getClock().getFrameTime();
            }

            if (engineOn && getClock().getTimeFactor() == 1)
            {
                base.Update(new Vector2D(Math.Cos(angle) * engineThrust * throttle / mass, Math.Sin(angle) * engineThrust * throttle / mass));
            }
            else
            {
                if (getHeight() >= 0)
                {
                    base.Update();
                }
                //orbit.generatePath(1000);
            }

            //Debug.WriteLine(getPosition().Y);

            

            if (getHeight() <= 0 && !getLanded())
            {
                setLanded(true);
            }
            if (getLanded() && getHeight() > 0)
            {
                setLanded(false);
            }
            
        }

        // Draws the rocket sprite and orbit approximation to the screen
        // using the given SpriteBatch object
        public void Draw(SpriteBatch spriteBatch, Matrix transform, Vector2D origin, bool mapView)
        {
            // Draw rocket
            if (mapView || true)
            {
                base.Draw(spriteBatch, transform, origin);
                spriteBatch.Draw(texture, Vector2.Transform((getPosition() - origin).getVector2(), transform), null, Color.White, angle + MathHelper.PiOver2, new Vector2(14f, 19f), Vector2.One, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(texture, new Vector2(Camera.ScreenWidth / 2, Camera.ScreenHeight / 2), null, Color.White, angle + MathHelper.PiOver2, new Vector2(14f, 19f), Vector2.One, SpriteEffects.None, 0f);
            }

        }

        public bool clicked(Matrix transform, Vector2D origin)
        {
            var mState = Mouse.GetState();
            if (mState.LeftButton == ButtonState.Pressed)
            {
                Vector2 mousePos = new Vector2(mState.Position.X, mState.Position.Y);
                Vector2 onScreenPos = (getPosition() - origin).Transform(transform).getVector2();
                float clickRadius = 10;
                if ((mousePos - onScreenPos).Length() < clickRadius)
                {
                    return true;
                }
            }
            return false;
        }

        public new String getSaveData()
        {
            String output = base.getSaveData();
            output += "Mass: " + mass + "\n";
            output += "Angle: " + angle + "\n";
            output += "Texture: " + texture + "\n";
            output += "Engine On: " + engineOn + "\n";
            output += "Engine Thrust: " + engineThrust + "\n";
            output += "Throttle: " + throttle + "\n";

            return output;
        }

        public float getAngle()
        {
            return angle;
        }

    }
}
