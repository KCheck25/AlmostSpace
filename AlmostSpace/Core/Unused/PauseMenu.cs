using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things.UserInterface
{
    internal class PauseMenu
    {
        Texture2D buttonTexture;
        Texture2D backgroundTexture;
        SpriteFont uiFont;
        public PauseMenu(Texture2D buttonTexture, Texture2D backgroundTexture, SpriteFont uiFont)
        {
            this.buttonTexture = buttonTexture;
            this.backgroundTexture = backgroundTexture;
            this.uiFont = uiFont;

        }

        public void Update()
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
