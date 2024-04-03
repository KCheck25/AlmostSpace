using AlmostSpace.Things;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace AlmostSpace
{
    public class Game1 : Game
    {
        Texture2D rocketTexture;
        Texture2D earthTexture;
        Texture2D moonTexture;
        Texture2D apIndicator;
        Texture2D peIndicator;
        Texture2D soiTexture;

        private SpriteFont uiFont;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Rocket rocket;
        private Planet earth;
        private Planet moon;
        private Planet moonMoon;
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
            moonTexture = Content.Load<Texture2D>("Moon");
            apIndicator = Content.Load<Texture2D>("APindicator");
            peIndicator = Content.Load<Texture2D>("pIndicator");
            soiTexture = Content.Load<Texture2D>("SOI");

            clock = new SimClock();
            earth = new Planet(earthTexture, 5.97E24f, new Vector2(0, 0), 6378.14E3f);
            moon = new Planet(moonTexture, soiTexture, 7.35E22f, new Vector2(384400E3F, 0), new Vector2(0, 1000f), 1.74E6f, earth, clock, GraphicsDevice);
            moonMoon = new Planet(moonTexture, soiTexture, 7.35E21f, new Vector2(20000E3F, 0), new Vector2(0, 500f), 5.74E5f, moon, clock, GraphicsDevice);

            rocket = new Rocket(rocketTexture, apIndicator, peIndicator, GraphicsDevice, 50, moonMoon, clock);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            clock.Update(gameTime);
            rocket.Update();
            moon.Update();
            moonMoon.Update();
            camera.update(gameTime);

            camera.setFocusPosition(rocket.getPosition());

            base.Update(gameTime);
        }

        // Draw things to the screen
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw earth
            _spriteBatch.Begin(transformMatrix: camera.transform);
            earth.Draw(_spriteBatch, camera.transform);
            moon.Draw(_spriteBatch, camera.transform);
            moonMoon.Draw(_spriteBatch, camera.transform);
            _spriteBatch.End();


            // Draw rocket, orbit information, and orbit
            String time = clock.getDisplayTime();
            float timeWidth = uiFont.MeasureString(time).X;

            String timeWarp = "Time warp: " + clock.getTimeFactor() + "x";
            float timeWarpWidth = uiFont.MeasureString(timeWarp).X;

            _spriteBatch.Begin();
            rocket.Draw(_spriteBatch, camera.transform);
            _spriteBatch.DrawString(uiFont, "Height: " + Math.Round(rocket.getHeight() / 10) / 100 + "km", new Vector2(25, 25), Color.White);
            _spriteBatch.DrawString(uiFont, "Velocity: " + Math.Round(rocket.getVelocityMagnitude() / 10) / 100 + "km/s", new Vector2(25, 60), Color.White);
            _spriteBatch.DrawString(uiFont, "Apoapsis: " + Math.Round(rocket.getApoapsisHeight() / 10) / 100 + "km", new Vector2(25, 95), Color.White);
            _spriteBatch.DrawString(uiFont, "Periapsis: " + Math.Round(rocket.getPeriapsisHeight()/ 10) / 100 + "km", new Vector2(25, 130), Color.White);
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
