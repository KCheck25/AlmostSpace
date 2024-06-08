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

        bool fullScreen = false;
        bool f11Toggle = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;
            Window.TextInput += TextInputHandler;
        }

        public void OnResize(Object sender, EventArgs e)
        {
            if (f11Toggle)
            {
                Debug.WriteLine("yooo");
                return;
            }
            // Additional code to execute when the user drags the window
            // or in the case you programmatically change the screen or windows client screen size.
            // code that might directly change the backbuffer width height calling apply changes.
            // or passing changes that must occur in other classes or even calling there OnResize methods
            // though those methods can simply be added to the Windows event caller
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();

            Camera.ScreenWidth = _graphics.PreferredBackBufferWidth;
            Camera.ScreenHeight = _graphics.PreferredBackBufferHeight;

            Debug.WriteLine("RESIZED!!!");

            currentScreen.Resize();

        }

        private void TextInputHandler(object sender, TextInputEventArgs args)
        {
            var pressedKey = args.Key;
            var character = args.Character;
            //Debug.WriteLine(character);
            currentScreen.ReadKey(character);
            // do something with the character (and optionally the key)
            // ...
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            //_graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            Camera.ScreenWidth = _graphics.PreferredBackBufferWidth;
            Camera.ScreenHeight = _graphics.PreferredBackBufferHeight;

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
            //Debug.WriteLine(GlobalConstants.ScreenWidth);
            var kState = Keyboard.GetState();
            if (kState.IsKeyDown(Keybinds.toggleFullScreen) && !f11Toggle)
            {
                Debug.WriteLine("Pressed");
                f11Toggle = true;
                if (fullScreen)
                {
                    fullScreen = false;
                    _graphics.IsFullScreen = false;
                    _graphics.PreferredBackBufferWidth = 1920;
                    _graphics.PreferredBackBufferHeight = 1080;
                }
                else
                {
                    fullScreen = true;
                    _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    _graphics.IsFullScreen = true;

                }
                _graphics.ApplyChanges();
                Camera.ScreenWidth = _graphics.PreferredBackBufferWidth;
                Camera.ScreenHeight = _graphics.PreferredBackBufferHeight;
                currentScreen.Resize();
            }
            if (!kState.IsKeyDown(Keybinds.toggleFullScreen))
            {
                f11Toggle = false;
            }

            if (currentScreen.NextScreen() != -1)
            {
                currentScreen = screens[currentScreen.NextScreen()];
                currentScreen.Start();
                currentScreen.Resize();
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
