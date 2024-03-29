using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AlmostSpace.Things
{
    // Represents a planet in space
    internal class Planet
    {
        Texture2D texture;
        float mass;
        Vector2 position;
        float radius;
        float soi; // sphere of influence

        Planet orbiting;
        GraphicsDevice graphicsDevice;
        Orbit orbit;
        SimClock clock;
        Texture2D soiTexture;

        // Creates a new planet using the given texture, mass, and position
        public Planet(Texture2D texture, float mass, Vector2 position, float radius)
        {
            this.texture = texture;
            this.mass = mass;
            this.position = position;
            this.radius = radius;
        }

        public Planet(Texture2D texture, Texture2D soiTexture, float mass, Vector2 position, float radius, Planet orbiting, SimClock clock, GraphicsDevice graphicsDevice)
        {
            this.texture = texture;
            this.mass = mass;
            this.position = position;
            this.radius = radius;
            this.orbiting = orbiting;
            this.graphicsDevice = graphicsDevice;
            this.soiTexture = soiTexture;
            this.clock = clock;
            
            orbit = new Orbit(orbiting, position, new Vector2(0f, 1000f), clock, graphicsDevice);
            orbit.Update(new Vector2());

            soi = orbit.getSemiMajorAxis() * (float)Math.Pow(mass / orbiting.getMass(), 0.4);
            Debug.WriteLine(soi);
        }

        // Returns the mass of this planet
        public float getMass()
        {
            return mass;
        }

        public float getSOI()
        {
            return soi;
        }

        // Returns the position of this planet's center
        public Vector2 getPosition()
        {
            return position;
        }

        public Vector2 getVelocity()
        {
            return orbit == null ? new Vector2() : orbit.getVelocity();
        }

        // Returns the radius of the planet's surface
        public float getRadius()
        {
            return radius;
        }

        public void update()
        {
            if (orbit != null && !clock.getTimeStopped())
            {
                orbit.Update();
                position = orbit.getPosition();
                //Debug.WriteLine(position);
            }
        }

        // Draws this planet to the screen using the given SpriteBatch object
        public void Draw(SpriteBatch spriteBatch, Matrix transform)
        {
            if (orbit != null)
            {
                spriteBatch.Draw(soiTexture, position, null, Color.White, 0f, new Vector2(soiTexture.Width / 2, soiTexture.Height / 2), 2 * soi / soiTexture.Width, SpriteEffects.None, 0f);
                orbit.Draw(spriteBatch, transform);
            }
            spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), 2 * radius / texture.Width, SpriteEffects.None, 0f);
            
        }

    }
}
