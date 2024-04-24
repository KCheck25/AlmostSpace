using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    internal class MapView : Screen
    {
        Texture2D rocketTexture;
        Texture2D earthTexture;
        Texture2D moonTexture;
        Texture2D apIndicator;
        Texture2D peIndicator;
        Texture2D soiTexture;
        Texture2D sunTexture;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Rocket rocket;
        private Planet earth;
        private Planet moon;
        private Planet sun;
        private SimClock clock;

        Camera camera;

        ContentManager Content;
        GraphicsDevice GraphicsDevice;

        SpriteFont uiFont;

        public MapView(ContentManager Content, GraphicsDevice GraphicsDevice, SpriteFont uiFont) {
            this.Content = Content;
            this.GraphicsDevice = GraphicsDevice;
            this.uiFont = uiFont;
        }

        public void LoadContent()
        {
            rocketTexture = Content.Load<Texture2D>("Arrow");
            earthTexture = Content.Load<Texture2D>("Earth");
            moonTexture = Content.Load<Texture2D>("Moon");
            apIndicator = Content.Load<Texture2D>("APindicator");
            peIndicator = Content.Load<Texture2D>("pIndicator");
            soiTexture = Content.Load<Texture2D>("SOI");
            sunTexture = Content.Load<Texture2D>("Sun");

            clock = new SimClock();
            sun = new Planet(sunTexture, 1.989E30f, new Vector2D(), 6.96E8f);
            earth = new Planet(earthTexture, soiTexture, 5.97E24f, new Vector2D(1.4995E11, 0), new Vector2D(0, 29784.8), 6378.14E3f, sun, clock, GraphicsDevice);
            //earth = new Planet(earthTexture, 5.97E24f, new Vector2D(0, 0), 6378.14E3f);
            moon = new Planet(moonTexture, soiTexture, 7.35E22f, new Vector2D(384400E3, 0), new Vector2D(0, 1000), 1.74E6f, earth, clock, GraphicsDevice);
            //moonMoon = new Planet(moonTexture, soiTexture, 7.35E21f, new Vector2(20000E3F, 0), new Vector2(0, 500f), 5.74E5f, moon, clock, GraphicsDevice);

            rocket = new Rocket(rocketTexture, apIndicator, peIndicator, GraphicsDevice, 50, earth, clock);
        }

        public void Update(GameTime gameTime)
        {
            clock.Update(gameTime);
            rocket.Update();
            moon.Update();
            earth.Update();
            sun.Update();
            camera.update(gameTime);

            camera.setFocusPosition(rocket.getPosition());
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw earth
            _spriteBatch.Begin(transformMatrix: camera.transform);
            earth.Draw(_spriteBatch, camera.transform);
            moon.Draw(_spriteBatch, camera.transform);
            //moon.Draw(_spriteBatch, camera.transform);
            sun.Draw(_spriteBatch, camera.transform);
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
            _spriteBatch.DrawString(uiFont, "Periapsis: " + Math.Round(rocket.getPeriapsisHeight() / 10) / 100 + "km", new Vector2(25, 130), Color.White);
            _spriteBatch.DrawString(uiFont, "Period: " + Math.Round(rocket.getPeriod()) + "s", new Vector2(25, 165), Color.White);
            _spriteBatch.DrawString(uiFont, "Throttle: " + rocket.getThrottle() + "%", new Vector2(25, 270), Color.White);
            _spriteBatch.DrawString(uiFont, time, new Vector2(1895 - timeWidth, 25), Color.White);
            _spriteBatch.DrawString(uiFont, "Fps: " + Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds), new Vector2(25, 345), Color.White);
            _spriteBatch.DrawString(uiFont, timeWarp, new Vector2(1895 - timeWarpWidth, 60), Color.White);
            _spriteBatch.DrawString(uiFont, "Engine " + rocket.getEngineState(), new Vector2(25, 235), Color.White);

            _spriteBatch.End();
        }
    }
}
