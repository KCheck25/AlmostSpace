using AlmostSpace.Things;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AlmostSpace
{
    public class Game1 : Game
    {
        Texture2D rocketTexture;
        Texture2D earthTexture;
        Texture2D orbitTexture;
        float displayAngle = 0;

        private SpriteFont uiFont;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Rocket rocket;
        private Planet earth;

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
            rocketTexture = Content.Load<Texture2D>("Arrow");
            earthTexture = Content.Load<Texture2D>("Earth");
            uiFont = Content.Load<SpriteFont>("OrbitInfo");
            orbitTexture = Content.Load<Texture2D>("OrbitPiece");

            earth = new Planet(earthTexture, 4E14f, new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2));
            rocket = new Rocket(rocketTexture, orbitTexture, 50, earth, GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            // Example keyboard stuff:
            /*
            var kState = Keyboard.GetState();

            if (kState.IsKeyDown(Keys.Up))
            {
                ballPosition.Y -= ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            */

            //ballPosition.X = 0;
            //ballPosition.Y = 0;

            rocket.Update(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            //_spriteBatch.Begin();
            //_spriteBatch.Draw(ballTexture, ballPosition, null, Color.White, 0f, new Vector2(ballTexture.Width / 2, ballTexture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
            //_spriteBatch.Draw(earthTexture, new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2), null, Color.White, 0f, new Vector2(earthTexture.Width / 2, earthTexture.Height / 2), Vector2.One, SpriteEffects.None, 0f);
            //_spriteBatch.DrawString(uiFont, "Angle: " + displayAngle, new Vector2(50, 50), Color.White);
            //_spriteBatch.End();

            earth.Draw(_spriteBatch);

            rocket.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
    }

}
