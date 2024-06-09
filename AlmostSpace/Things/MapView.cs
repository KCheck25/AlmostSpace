using AlmostSpace.Things.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        Texture2D navBallTexture;
        Texture2D navBallFrameTexture;
        Texture2D progradeTexture;
        Texture2D retrogradeTexture;
        Texture2D radialInTexture;
        Texture2D radialOutTexture;

        Texture2D throttleTexture;
        Texture2D throttleFrameTexture;

        Texture2D buttonTexture;

        List<Song> songs;

        SoundEffect engineNoise;

        NavBallElement navBall;
        ThrottleElement throttle;

        private Rocket rocket;
        private List<Planet> planets;
        private List<Texture2D> planetTextures;
        private SimClock clock;

        Camera camera;

        ContentManager Content;
        GraphicsDevice GraphicsDevice;

        SpriteFont uiFont;

        Orbit objectFocused;

        Random rand;
        double songEndTime;
        float songDuration = 0;
        double timeToStartNextSong;

        int next = -1;

        public static bool startNewGame;
        static string saveFile = "savedata.txt";

        public MapView(ContentManager Content, GraphicsDevice GraphicsDevice, SpriteFont uiFont) {
            this.Content = Content;
            this.GraphicsDevice = GraphicsDevice;
            this.uiFont = uiFont;
            camera = new Camera();
            planets = new List<Planet>();
            planetTextures = new List<Texture2D>();
            songs = new List<Song>();
            rand = new Random();
            songEndTime = 0;
        }

        public void LoadContent()
        {
            rocketTexture = Content.Load<Texture2D>("Arrow");
            planetTextures.Add(Content.Load<Texture2D>("Earth"));
            planetTextures.Add(Content.Load<Texture2D>("Moon"));
            apIndicator = Content.Load<Texture2D>("APindicator");
            peIndicator = Content.Load<Texture2D>("pIndicator");
            soiTexture = Content.Load<Texture2D>("SOI");
            navBallTexture = Content.Load<Texture2D>("NavBallCenter");
            planetTextures.Add(Content.Load<Texture2D>("Sun"));
            planetTextures.Add(Content.Load<Texture2D>("Mars"));

            navBallFrameTexture = Content.Load<Texture2D>("NavBallTextures\\NavBallFrame");
            progradeTexture = Content.Load<Texture2D>("NavBallTextures\\ProgradeSymbol");
            retrogradeTexture = Content.Load<Texture2D>("NavBallTextures\\RetrogradeSymbol");
            radialInTexture = Content.Load<Texture2D>("NavBallTextures\\RadialInSymbol");
            radialOutTexture = Content.Load<Texture2D>("NavBallTextures\\RadialOutSymbol");

            throttleTexture = Content.Load<Texture2D>("ThrottleTextures\\ThrottleBar");
            throttleFrameTexture = Content.Load<Texture2D>("ThrottleTextures\\ThrottleBox");

            buttonTexture = Content.Load<Texture2D>("Button1");

            // A few jumps away by Arthur Vyncke
            songs.Add(Content.Load<Song>("a_few_jumps_away"));
            // Wonder by Nomyn
            songs.Add(Content.Load<Song>("wonder"));
            // Kevin MacLeod
            songs.Add(Content.Load<Song>("vibing_over_venus"));
            songs.Add(Content.Load<Song>("space_jazz"));

            songs.Add(Content.Load<Song>("dark_ambient_music"));

            engineNoise = Content.Load<SoundEffect>("engine_sound2");


            Keybinds.readBindings();
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
            planets.Add(new Planet("Mars", planetTextures[3], soiTexture, 5.97E24f, new Vector2D(1.8995E11, 0), new Vector2D(0, 27784.8), 6378.14E3f, planets[0], clock, GraphicsDevice));
            //moonMoon = new Planet(moonTexture, soiTexture, 7.35E21f, new Vector2(20000E3F, 0), new Vector2(0, 500f), 5.74E5f, moon, clock, GraphicsDevice);

            rocket = new Rocket("Zoomy", rocketTexture, apIndicator, peIndicator, GraphicsDevice, 50, planets[1], clock, engineNoise);
            objectFocused = rocket;

            InitUi();
        }

        public void loadGame()
        {
            Directory.CreateDirectory("Saves");

            planets = new List<Planet>();

            String line;
            List<String> data = new List<String>();
            using (StreamReader readtext = new StreamReader("Saves\\" + saveFile))
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
                    rocket = new Rocket(block, planets, clock, GraphicsDevice, rocketTexture, apIndicator, peIndicator, engineNoise);
                    objectFocused = rocket;
                }
            }

            InitUi();
        }

        public void InitUi()
        {
            navBall = new NavBallElement(new Vector2(1700, 875), 150, navBallTexture, navBallFrameTexture, progradeTexture, retrogradeTexture, radialInTexture, radialOutTexture);
            throttle = new ThrottleElement(new Vector2(50, 500), 300, throttleTexture, throttleFrameTexture);
        }

        public void Update(GameTime gameTime)
        {

            //MediaPlayer.Play(songs[rand.Next(songs.Count)]);
            if (gameTime.TotalGameTime.TotalSeconds > songEndTime)
            {
                timeToStartNextSong = gameTime.TotalGameTime.TotalSeconds + rand.Next(30) + 30;
                Debug.WriteLine("SONG ENDED. STARTING NEXT AT: " + timeToStartNextSong);
                songEndTime = int.MaxValue;
            }
            if (gameTime.TotalGameTime.TotalSeconds > timeToStartNextSong)
            {
                Song song = songs[rand.Next(songs.Count)];
                MediaPlayer.Play(song);
                songEndTime = gameTime.TotalGameTime.TotalSeconds + song.Duration.TotalSeconds;
                Debug.WriteLine("PLAYING NEW SONG. ENDS AT: " + songEndTime);
                Debug.WriteLine("SONG TIME: " + song.Duration.TotalSeconds);
                timeToStartNextSong = int.MaxValue;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) 
            {
                
                next = 0;
                Save();
            }
            clock.Update(gameTime);
            rocket.Update();

            bool mouseDown = Mouse.GetState().LeftButton == ButtonState.Pressed;

            foreach (Planet planet in planets)
            {
                planet.Update();
                if (mouseDown && planet.clicked(camera.transform, objectFocused.getPosition()))
                {
                    objectFocused = planet;
                    camera.clearOffsets();
                }
            }

            if (mouseDown && rocket.clicked(camera.transform, objectFocused.getPosition()))
            {
                objectFocused = rocket;
                camera.clearOffsets();
            }

            camera.update(gameTime);
            navBall.Update(rocket);
            throttle.Update(rocket);
            //camera.setFocusPosition(rocket.getPosition());
        }

        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            Vector2D centerPosition = objectFocused.getPosition();
            GraphicsDevice.Clear(Color.Black);

            // Draw earth
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.transform);
            foreach (Planet planet in planets)
            {
                planet.Draw(_spriteBatch, camera.transform, centerPosition);
            }
            _spriteBatch.End();


            // Draw rocket, orbit information, and orbit
            String time = clock.getDisplayTime();
            float timeWidth = uiFont.MeasureString(time).X;

            String timeWarp = "Time warp: " + clock.getTimeFactor() + "x";
            float timeWarpWidth = uiFont.MeasureString(timeWarp).X;

            _spriteBatch.Begin();
            rocket.Draw(_spriteBatch, camera.transform, centerPosition, camera.getRotation(), true);
            DrawPlanetNames(_spriteBatch);
            navBall.Draw(_spriteBatch);
            throttle.Draw(_spriteBatch);
            _spriteBatch.DrawString(uiFont, "Orbiting: " + rocket.getPlanetOrbiting().getName(), new Vector2(25, 25), Color.White);
            _spriteBatch.DrawString(uiFont, "Height: " + Math.Round(rocket.getHeight() / 10) / 100 + "km", new Vector2(25, 60), Color.White);
            _spriteBatch.DrawString(uiFont, "Velocity: " + Math.Round(rocket.getVelocityMagnitude() / 10) / 100 + "km/s", new Vector2(25, 95), Color.White);
            _spriteBatch.DrawString(uiFont, "Apoapsis: " + Math.Round(rocket.getApoapsisHeight() / 10) / 100 + "km", new Vector2(25, 130), Color.White);
            _spriteBatch.DrawString(uiFont, "Periapsis: " + Math.Round(rocket.getPeriapsisHeight() / 10) / 100 + "km", new Vector2(25, 165), Color.White);
            _spriteBatch.DrawString(uiFont, "Period: " + Math.Round(rocket.getPeriod()) + "s", new Vector2(25, 200), Color.White);
            _spriteBatch.DrawString(uiFont, "Throttle: " + rocket.getThrottle() + "%", new Vector2(25, 270), Color.White);
            _spriteBatch.DrawString(uiFont, time, new Vector2(Camera.ScreenWidth - 25 - timeWidth, 25), Color.White);
            _spriteBatch.DrawString(uiFont, "Fps: " + Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds), new Vector2(25, 380), Color.White);
            _spriteBatch.DrawString(uiFont, timeWarp, new Vector2(Camera.ScreenWidth - 25 - timeWarpWidth, 60), Color.White);
            _spriteBatch.DrawString(uiFont, "Engine " + rocket.getEngineState(), new Vector2(25, 305), Color.White);
            _spriteBatch.DrawString(uiFont, "Time " + gameTime.TotalGameTime.TotalSeconds, new Vector2(25, 340), Color.White);


            _spriteBatch.End();
        }

        public void DrawPlanetNames(SpriteBatch _spriteBatch)
        {
            float cameraZoom = camera.getZoom();
            // 1E-7
            foreach (Planet planet in planets)
            {
                // Check if its a moon
                if (planet.getPlanetOrbiting() != null && planet.getPlanetOrbiting().getPlanetOrbiting() != null)
                {
                    if (cameraZoom < 1E-6 && cameraZoom > 1E-7)
                    {
                        _spriteBatch.DrawString(uiFont, planet.getName(), (planet.getPosition() - objectFocused.getPosition()).Transform(camera.transform).getVector2(), Color.White);

                    }
                }
                else if (cameraZoom < 1E-7)
                {
                    _spriteBatch.DrawString(uiFont, planet.getName(), (planet.getPosition() - objectFocused.getPosition()).Transform(camera.transform).getVector2(), Color.White);
                }
            }
        }

        void Save()
        {
            Directory.CreateDirectory("Saves");
            string toSave = clock.getSaveData() + camera.getSaveData() + planets[0].getSaveData() + rocket.getSaveData();
            using (StreamWriter writetext = new StreamWriter("Saves\\" + saveFile))
            {
                writetext.WriteLine(toSave);
            }
            Keybinds.saveBindings();
            //Debug.WriteLine(planets[0].getSaveData());
            //Debug.WriteLine(rocket.getSaveData());
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

        public void Resize()
        {
            foreach (Planet planet in planets)
            {
                planet.updateScreenSize();
            }
            rocket.updateScreenSize();
        }

        public void ReadKey(Char key)
        {
            
        }

        public static void setFilename(string filename)
        {
            saveFile = filename + ".txt";
        }
    }
}
