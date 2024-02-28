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
    // Simple sprite to serve as one unit of an orbit representation
    internal class OrbitSprite
    {
        Vector2 position;
        Texture2D texture;

        // Constructs a new OrbitSprite object with the given texture and position
        public OrbitSprite(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
        }

        // Draws this OrbitSprite to the screen using the given SpriteBatch object
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, position, Color.White);
            spriteBatch.End();
        }
    }
}
