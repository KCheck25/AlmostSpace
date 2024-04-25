using AlmostSpace.Things;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;

namespace AlmostSpace
{
    public class Game1 : Game
    {

        private SpriteFont uiFont;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Screen currentScreen;
        Screen[] screens = new Screen[2];

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            uiFont = Content.Load<SpriteFont>("OrbitInfo");

            screens[0] = new MenuScreen(Content, uiFont, Exit);
            screens[1] = new MapView(Content, GraphicsDevice, uiFont);

            currentScreen = screens[0];

            foreach (Screen screen in screens)
            {
                screen.LoadContent();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (currentScreen.NextScreen() != -1)
            {
                currentScreen = screens[currentScreen.NextScreen()];
                currentScreen.Start();
            }

            currentScreen.Update(gameTime);

            base.Update(gameTime);
        }

        // Draw things to the screen
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            currentScreen.Draw(gameTime, _spriteBatch);

            base.Draw(gameTime);
        }
    }

}
