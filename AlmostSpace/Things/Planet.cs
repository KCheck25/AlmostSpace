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
    internal class Planet : Orbit
    {
        Texture2D texture;
        float mass;
        float planetRadius;
        float soi; // sphere of influence

        Texture2D soiTexture;

        List<Planet> children = new List<Planet>();

        // Creates a new planet using the given texture, mass, and position
        public Planet(Texture2D texture, float mass, Vector2 position, float radius) : base(position)
        {
            this.texture = texture;
            this.mass = mass;
            this.planetRadius = radius;
        }

        public Planet(Texture2D texture, Texture2D soiTexture, float mass, Vector2 position, Vector2 velocity, float radius, Planet orbiting, SimClock clock, GraphicsDevice graphicsDevice) : base(orbiting, position, velocity, clock, graphicsDevice)
        {
            this.texture = texture;
            this.mass = mass;
            this.planetRadius = radius;
            this.soiTexture = soiTexture;

            base.Update(new Vector2());

            soi = getSemiMajorAxis() * (float)Math.Pow(mass / orbiting.getMass(), 0.4);
            Debug.WriteLine(soi);
        }

        public void addChild(Planet child)
        {
            children.Add(child);
        }

        public List<Planet> getChildren()
        {
            return children;
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

        // Returns the radius of the planet's surface
        public float getRadius()
        {
            return planetRadius;
        }

        public new void Update()
        {
            base.Update();
        }

        // Draws this planet to the screen using the given SpriteBatch object
        public new void Draw(SpriteBatch spriteBatch, Matrix transform)
        {
            if (getVelocity() != new Vector2())
            {
                spriteBatch.Draw(soiTexture, getPosition(), null, Color.White, 0f, new Vector2(soiTexture.Width / 2, soiTexture.Height / 2), 2 * soi / soiTexture.Width, SpriteEffects.None, 0f);
            }
            base.Draw(spriteBatch, transform);
            spriteBatch.Draw(texture, getPosition(), null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), 2 * planetRadius / texture.Width, SpriteEffects.None, 0f);
            
        }

    }
}
