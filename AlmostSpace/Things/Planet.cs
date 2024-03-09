using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    // Represents a planet in space
    internal class Planet
    {
        Texture2D texture;
        float mass;
        Vector2 position;

        // Creates a new planet using the given texture, mass, and position
        public Planet(Texture2D texture, float mass, Vector2 position)
        {
            this.texture = texture;
            this.mass = mass;
            this.position = position;
        }

        // Returns the mass of this planet
        public float getMass()
        {
            return mass;
        }

        // Returns the position of this planet's center
        public Vector2 getPosition()
        {
            return position;
        }

        // Draws this planet to the screen using the given SpriteBatch object
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
        }

    }
}
