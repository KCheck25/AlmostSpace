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
    // This class represents the main screen that the user interfaces with.
    // It controls the actual game, drawing planets, trajectories, and rockets
    // to the screen, draws the UI, and manages some of the music.
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

        Texture2D tutorialBox;

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
        public static bool isTutorial;
        static string saveFile = "savedata.txt";

        Tutorial tutorial;

        // Creates a new MapView object using the given content manager for loading in files, graphics device,
        // and font in which to render text.
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

        // Loads necessary image and sound files, and creates other important objects that will be used later
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

            tutorialBox = Content.Load<Texture2D>("TutorialBox");

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

        // Creates a new game, without using a save file
        public void newGame()
        {
            planets = new List<Planet>();

            clock = new SimClock();

            camera = new Camera();

            // Generate solar system
            planets.Add(new Planet("Sun", planetTextures[2], 1.989E30f, new Vector2D(), 6.96E8f));
            planets.Add(new Planet("Earth", planetTextures[0], soiTexture, 5.97E24f, new Vector2D(1.4995E11, 0), new Vector2D(0, 29784.8), 6378.14E3f, planets[0], clock, GraphicsDevice));
            //earth = new Planet(earthTexture, 5.97E24f, new Vector2D(0, 0), 6378.14E3f);
            planets.Add(new Planet("Moon", planetTextures[1], soiTexture, 7.35E22f, new Vector2D(384400E3, 0), new Vector2D(0, 1000), 1.74E6f, planets[1], clock, GraphicsDevice));
            planets.Add(new Planet("Mars", planetTextures[3], soiTexture, 5.97E24f, new Vector2D(1.8995E11, 0), new Vector2D(0, 27784.8), 6378.14E3f, planets[0], clock, GraphicsDevice));
            //moonMoon = new Planet(moonTexture, soiTexture, 7.35E21f, new Vector2(20000E3F, 0), new Vector2(0, 500f), 5.74E5f, moon, clock, GraphicsDevice);

            rocket = new Rocket("Zoomy", rocketTexture, apIndicator, peIndicator, GraphicsDevice, 50, planets[1], clock, engineNoise);
            objectFocused = rocket;

            InitUi();

            if (isTutorial)
            {
                tutorial = new Tutorial(rocket, camera, clock, uiFont, tutorialBox);
            }

        }

        // Loads a saved game from a save file
        public void loadGame()
        {
            Directory.CreateDirectory("Saves");

            planets = new List<Planet>();

            // Break data up into chunks separated by the word "Type:"
            string line;
            List<string> data = new List<string>();
            using (StreamReader readtext = new StreamReader("Saves\\" + saveFile))
            {
                while (!readtext.EndOfStream)
                {
                    line = readtext.ReadLine();
                    if (line.Contains("Type:"))
                    {
                        string block = "";
                        while (!line.Equals(""))
                        {
                            block += line + "\n"; 
                            line = readtext.ReadLine();
                        }
                        data.Add(block);
                    }
                }

            }

            // Create the camera and clock first based on the data, as these must
            // be passed to other objects created in the next for loop
            foreach (string block in data)
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

            // Create planets
            foreach (string block in data)
            {
                if (block.Contains("Type: Planet"))
                {
                    planets.Add(new Planet(block, planets, clock, planetTextures, soiTexture, GraphicsDevice));
                }
            }

            // Create the rocket
            foreach (string block in data)
            {
                if (block.Contains("Type: Rocket"))
                {
                    rocket = new Rocket(block, planets, clock, GraphicsDevice, rocketTexture, apIndicator, peIndicator, engineNoise);
                    objectFocused = rocket;
                }
            }

            InitUi();

            if (isTutorial)
            {
                tutorial = new Tutorial(rocket, camera,clock, uiFont, tutorialBox);
            }

        }

        // Create the navball and throttle UI elements
        public void InitUi()
        {
            navBall = new NavBallElement(new Vector2(1700, 875), 150, navBallTexture, navBallFrameTexture, progradeTexture, retrogradeTexture, radialInTexture, radialOutTexture);
            throttle = new ThrottleElement(new Vector2(50, 500), 300, throttleTexture, throttleFrameTexture);
        }

        // Updates planets, rockets, UI elements, and manages music
        public void Update(GameTime gameTime)
        {
            if (isTutorial)
            {
                tutorial.Update();
            }

            // Manage music stuff
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

            // Save and quit if the escape key is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) 
            {
                rocket.stopNoise();
                next = 0;
                isTutorial = false;
                Save();
            }
            clock.Update(gameTime);
            rocket.Update();

            // Focus the camera on objects if they are clicked
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

        // Draws planets, the rocket, and UI elements to the screen
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
            string time = clock.getDisplayTime();
            float timeWidth = uiFont.MeasureString(time).X;

            string timeWarp = "Time warp: " + clock.getTimeFactor() + "x";
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
            if (isTutorial)
            {
                tutorial.Draw(_spriteBatch);
            }


            _spriteBatch.End();
        }

        // Draws planet names to the screen, provided the user is zoomed out enough
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

        // Saves the game to a save file
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

        // Returns the next screen to go to. If the number is -1, that tells the Game1 class to not switch the screen.
        // Otherwise, the screen is switched based on the number passed.
        public int NextScreen()
        {
            return next;
        }

        // Starts this screen, creating a new game or loading from a file based on what button was clicked
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

        // Ensures everything draws in the correct location when the window is resized.
        public void Resize()
        {
            if (isTutorial)
            {
                tutorial.Resize();
            }
            foreach (Planet planet in planets)
            {
                planet.updateScreenSize();
            }
            rocket.updateScreenSize();
        }

        // Called whenever the user presses a key
        // Needed to satisfy the Screen interface, but not used here
        public void ReadKey(char key)
        {
            
        }

        // Sets the file name to save and load files from
        public static void setFilename(string filename)
        {
            saveFile = filename + ".txt";
        }
    }
}
