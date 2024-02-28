using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    internal class Planet
    {
        Texture2D texture;
        float mass;
        Vector2 position;
        public Planet(Texture2D texture, float mass, Vector2 position)
        {
            this.texture = texture;
            this.mass = mass;
            this.position = position;
        }

        public float getMass()
        {
            return mass;
        }

        public Vector2 getPosition()
        {
            return position;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

    }
}
