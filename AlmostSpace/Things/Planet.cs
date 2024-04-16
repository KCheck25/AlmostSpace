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
        double mass;
        double planetRadius;
        double soi; // sphere of influence

        Texture2D soiTexture;

        List<Planet> children = new List<Planet>();

        // Creates a new planet using the given texture, mass, and position
        public Planet(Texture2D texture, double mass, Vector2D position, double radius) : base(position)
        {
            this.texture = texture;
            this.mass = mass;
            this.planetRadius = radius;
        }

        public Planet(Texture2D texture, Texture2D soiTexture, float mass, Vector2D position, Vector2D velocity, double radius, Planet orbiting, SimClock clock, GraphicsDevice graphicsDevice) : base(orbiting, position, velocity, clock, graphicsDevice)
        {
            this.texture = texture;
            this.mass = mass;
            this.planetRadius = radius;
            this.soiTexture = soiTexture;

            base.Update(new Vector2D());

            soi = getSemiMajorAxis() * Math.Pow(mass / orbiting.getMass(), 0.4);
            Debug.WriteLine(soi);

            orbiting.addChild(this);
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
        public double getMass()
        {
            return mass;
        }

        public double getSOI()
        {
            return soi;
        }

        // Returns the radius of the planet's surface
        public double getRadius()
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
            if (!getVelocity().Equals(new Vector2D()))
            {
                spriteBatch.Draw(soiTexture, getPosition().getVector2(), null, Color.White, 0f, new Vector2(soiTexture.Width / 2, soiTexture.Height / 2), (float)(2 * soi / soiTexture.Width), SpriteEffects.None, 0f);
            }
            base.Draw(spriteBatch, transform);
            spriteBatch.Draw(texture, getPosition().getVector2(), null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), (float)(2 * planetRadius / texture.Width), SpriteEffects.None, 0f);
            
        }

    }
}
