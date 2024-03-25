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

        private SpriteFont uiFont;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Rocket rocket;
        private Planet earth;
        private SimClock clock;

        Camera camera;

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

            camera = new Camera();

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

            earth = new Planet(earthTexture, 4E15f, new Vector2(0, 0));
            clock = new SimClock();
            rocket = new Rocket(rocketTexture, orbitTexture, 50, earth, clock);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            clock.Update(gameTime);
            rocket.Update();
            camera.update(gameTime);

            base.Update(gameTime);
        }

        // Draw things to the screen
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw earth
            _spriteBatch.Begin(transformMatrix: camera.transform);
            earth.Draw(_spriteBatch);
            _spriteBatch.End();


            // Draw rocket, orbit information, and orbit
            String time = clock.getDisplayTime();
            float timeWidth = uiFont.MeasureString(time).X;

            String timeWarp = "Time warp: " + clock.getTimeFactor() + "x";
            float timeWarpWidth = uiFont.MeasureString(timeWarp).X;

            _spriteBatch.Begin();
            rocket.Draw(_spriteBatch, camera.transform);
            _spriteBatch.DrawString(uiFont, "Height: " + Math.Round(rocket.getHeight()) + "m", new Vector2(25, 25), Color.White);
            _spriteBatch.DrawString(uiFont, "Velocity: " + Math.Round(rocket.getVelocity()) + "m/s", new Vector2(25, 60), Color.White);
            _spriteBatch.DrawString(uiFont, "Apoapsis: " + Math.Round(rocket.getApoapsisHeight()) + "m", new Vector2(25, 95), Color.White);
            _spriteBatch.DrawString(uiFont, "Periapsis: " + Math.Round(rocket.getPeriapsisHeight()) + "m", new Vector2(25, 130), Color.White);
            _spriteBatch.DrawString(uiFont, "Period: " + Math.Round(rocket.getPeriod()) + "s", new Vector2(25, 165), Color.White);
            _spriteBatch.DrawString(uiFont, "Throttle: " + rocket.getThrottle() + "%", new Vector2(25, 270), Color.White);
            _spriteBatch.DrawString(uiFont, time, new Vector2(1895 - timeWidth, 25), Color.White);
            _spriteBatch.DrawString(uiFont, "Fps: " + Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds), new Vector2(25, 345), Color.White);
            _spriteBatch.DrawString(uiFont, timeWarp, new Vector2(1895 - timeWarpWidth, 60), Color.White);
            _spriteBatch.DrawString(uiFont, "Engine " + rocket.getEngineState(), new Vector2(25, 235), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }

}
