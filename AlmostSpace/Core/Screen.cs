using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlmostSpace.Things
{
    // An interface representing a specific screen in the program
    public interface Screen
    {
        public void Update(GameTime gameTime);
        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch);
        public void LoadContent();
        public int NextScreen();
        public void Start();
        public void Resize();
        public void ReadKey(char key);
    }
}
