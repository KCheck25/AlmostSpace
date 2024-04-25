using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    internal class MenuScreen : Screen
    {
        int next = -1;
        Button startNewButton;
        Button loadButton;
        Button exitButton;
        Texture2D buttonTexture;
        Texture2D backgroundImage;
        SpriteFont uiFont;
        Action exitCommand;

        ContentManager Content;

        public MenuScreen(ContentManager Content, SpriteFont uiFont, Action exitCommand)
        {
            this.Content = Content;
            this.uiFont = uiFont;
            this.exitCommand = exitCommand;
        }

        public void LoadContent()
        {
            buttonTexture = Content.Load<Texture2D>("Button1");
            backgroundImage = Content.Load<Texture2D>("menu_background");
            startNewButton = new Button("Start New", uiFont, buttonTexture, startNewGame, new Vector2(1920 / 2 - buttonTexture.Width / 2, 500));
            loadButton = new Button("Load", uiFont, buttonTexture, loadGame, new Vector2(1920 / 2 - buttonTexture.Width / 2, 600));
            exitButton = new Button("Exit", uiFont, buttonTexture, exitCommand, new Vector2(1920 / 2 - buttonTexture.Width / 2, 700));
        }

        public void Update(GameTime gameTime)
        {
            startNewButton.Update();
            loadButton.Update();
            exitButton.Update();
        }

        public void startNewGame()
        {
            MapView.startNewGame = true;
            next = 1;
        }

        public void loadGame()
        {
            MapView.startNewGame = false;
            next = 1;
        }

        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            _spriteBatch.Begin();
            // Picture from https://opengameart.org/content/pixel-space
            _spriteBatch.Draw(backgroundImage, new Vector2(), null, Color.White, 0f, new Vector2(), Vector2.One, SpriteEffects.None, 0f);
            startNewButton.Draw(_spriteBatch);
            loadButton.Draw(_spriteBatch);
            exitButton.Draw(_spriteBatch);
            _spriteBatch.End();
        }

        public int NextScreen()
        {
            return next;
        }

        public void Start()
        {
            next = -1;
        }
    }
}
