using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    internal class MapView : Screen
    {
        Texture2D rocketTexture;
        Texture2D apIndicator;
        Texture2D peIndicator;
        Texture2D soiTexture;

        private Rocket rocket;
        private List<Planet> planets;
        private List<Texture2D> planetTextures;
        private SimClock clock;

        Camera camera;

        ContentManager Content;
        GraphicsDevice GraphicsDevice;

        SpriteFont uiFont;

        int next = -1;

        public static bool startNewGame;

        public MapView(ContentManager Content, GraphicsDevice GraphicsDevice, SpriteFont uiFont) {
            this.Content = Content;
            this.GraphicsDevice = GraphicsDevice;
            this.uiFont = uiFont;
            camera = new Camera();
            planets = new List<Planet>();
            planetTextures = new List<Texture2D>();
        }

        public void LoadContent()
        {
            rocketTexture = Content.Load<Texture2D>("Arrow");
            planetTextures.Add(Content.Load<Texture2D>("Earth"));
            planetTextures.Add(Content.Load<Texture2D>("Moon"));
            apIndicator = Content.Load<Texture2D>("APindicator");
            peIndicator = Content.Load<Texture2D>("pIndicator");
            soiTexture = Content.Load<Texture2D>("SOI");
            planetTextures.Add(Content.Load<Texture2D>("Sun"));
        }

        public void newGame()
        {
            planets = new List<Planet>();

            clock = new SimClock();

            camera = new Camera();

            planets.Add(new Planet("Sun", planetTextures[2], 1.989E30f, new Vector2D(), 6.96E8f));
            planets.Add(new Planet("Earth", planetTextures[0], soiTexture, 5.97E24f, new Vector2D(1.4995E11, 0), new Vector2D(0, 29784.8), 6378.14E3f, planets[0], clock, GraphicsDevice));
            //earth = new Planet(earthTexture, 5.97E24f, new Vector2D(0, 0), 6378.14E3f);
            planets.Add(new Planet("Moon", planetTextures[1], soiTexture, 7.35E22f, new Vector2D(384400E3, 0), new Vector2D(0, 1000), 1.74E6f, planets[1], clock, GraphicsDevice));
            //moonMoon = new Planet(moonTexture, soiTexture, 7.35E21f, new Vector2(20000E3F, 0), new Vector2(0, 500f), 5.74E5f, moon, clock, GraphicsDevice);

            rocket = new Rocket("Zoomy", rocketTexture, apIndicator, peIndicator, GraphicsDevice, 50, planets[1], clock);
        }

        public void loadGame()
        {
            planets = new List<Planet>();

            String line;
            List<String> data = new List<String>();
            using (StreamReader readtext = new StreamReader("savedata.txt"))
            {
                while (!readtext.EndOfStream)
                {
                    line = readtext.ReadLine();
                    if (line.Contains("Type:"))
                    {
                        String block = "";
                        while (!line.Equals(""))
                        {
                            block += line + "\n"; 
                            line = readtext.ReadLine();
                        }
                        data.Add(block);
                    }
                }
                
            }

            foreach (String block in data)
            {
                if (block.Contains("Type: Clock"))
                {
                    clock = new SimClock(block);
                }
                if (block.Contains("Type: Camera"))
                {
                    camera = new Camera(block);
                }
            }

            foreach (String block in data)
            {
                if (block.Contains("Type: Planet"))
                {
                    planets.Add(new Planet(block, planets, clock, planetTextures, soiTexture, GraphicsDevice));
                }
            }

            foreach (String block in data)
            {
                if (block.Contains("Type: Rocket"))
                {
                    rocket = new Rocket(block, planets, clock, GraphicsDevice, rocketTexture, apIndicator, peIndicator);
                }
            }

        }

        public void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) 
            {
                next = 0;
                Save();
            }

            clock.Update(gameTime);
            rocket.Update();
            foreach (Planet planet in planets)
            {
                planet.Update();
            }
            camera.update(gameTime);

            camera.setFocusPosition(rocket.getPosition());
        }

        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw earth
            _spriteBatch.Begin(transformMatrix: camera.transform);
            foreach (Planet planet in planets)
            {
                planet.Draw(_spriteBatch, camera.transform);
            }
            _spriteBatch.End();


            // Draw rocket, orbit information, and orbit
            String time = clock.getDisplayTime();
            float timeWidth = uiFont.MeasureString(time).X;

            String timeWarp = "Time warp: " + clock.getTimeFactor() + "x";
            float timeWarpWidth = uiFont.MeasureString(timeWarp).X;

            _spriteBatch.Begin();
            rocket.Draw(_spriteBatch, camera.transform, true);
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

        void Save()
        {
            String toSave = clock.getSaveData() + camera.getSaveData() + planets[0].getSaveData() + rocket.getSaveData();
            using (StreamWriter writetext = new StreamWriter("savedata.txt"))
            {
                writetext.WriteLine(toSave);
            }
            Debug.WriteLine(planets[0].getSaveData());
            Debug.WriteLine(rocket.getSaveData());
        }

        public int NextScreen()
        {
            return next;
        }

        public void Start()
        {

            if (startNewGame)
            {
                newGame();
            } 
            else
            {
                loadGame();
            }
            next = -1;
        }
    }
}
