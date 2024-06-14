using AlmostSpace.Core.Common;
using AlmostSpace.Things.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Diagnostics;
using System.IO;

namespace AlmostSpace.Things
{
    // Controls the main menu screen of the project, coordinating the buttons and menus
    internal class MenuScreen : Screen
    {
        int next = -1;
        Button startNewButton;
        Button loadButton;
        Button exitButton;
        Button tutorialButton;
        Button aboutButton;
        Button controlsButton;
        DisplayTextBox aboutMeBlurb;
        DisplayTextBox controlsBlurb;
        Texture2D buttonTexture;
        Texture2D backgroundImage;
        Texture2D titleTexture;
        Texture2D deleteButtonTexture;
        Texture2D aboutMeBoxTexture;
        SpriteFont uiFont;
        SpriteFont titleFont;
        Action exitCommand;
        Song addingTheSun;

        ContentManager Content;

        bool loadingFile;
        bool creatingFile;
        bool aboutScreen;
        bool controlsScreen;
        Button[] fileSelectButtons;
        Button[] deleteButtons;
        TextBox selectFilename;
        Button nextButton;
        Button backButton;

        int pageOnLoadScreen = 0;

        // Creates a new MenuScreen object from the given ContentManager, font to use for buttons and UI, and command to run to quit the game
        public MenuScreen(ContentManager Content, SpriteFont uiFont, Action exitCommand)
        {
            this.Content = Content;
            this.uiFont = uiFont;
            this.exitCommand = exitCommand;
        }

        // Loads necessary inage and sound files
        public void LoadContent()
        {
            buttonTexture = Content.Load<Texture2D>("Button1");
            deleteButtonTexture = Content.Load<Texture2D>("DeleteButton");

            // Picture from https://opengameart.org/content/pixel-space
            backgroundImage = Content.Load<Texture2D>("menu_background");
            titleTexture = Content.Load<Texture2D>("title");
            startNewButton = new Button("Start New", uiFont, buttonTexture, () => creatingFile = true, new Vector2(Camera.ScreenWidth / 2 - (buttonTexture.Width + 25) / 2, 500));
            tutorialButton = new Button("Tutorial", uiFont, buttonTexture, startTutorial, new Vector2(Camera.ScreenWidth / 2 - (buttonTexture.Width + 25) / 2, 600));
            loadButton = new Button("Load", uiFont, buttonTexture, loadGame, new Vector2(Camera.ScreenWidth / 2 + (buttonTexture.Width + 25) / 2, 500));
            aboutButton = new Button("About", uiFont, buttonTexture, () => aboutScreen = true, new Vector2(Camera.ScreenWidth / 2 + (buttonTexture.Width + 25) / 2, 600));
            exitButton = new Button("Exit", uiFont, buttonTexture, exitCommand, new Vector2(Camera.ScreenWidth / 2 + (buttonTexture.Width + 25) / 2, 700));
            controlsButton = new Button("Controls", uiFont, buttonTexture, () => controlsScreen = true, new Vector2(Camera.ScreenWidth / 2 - (buttonTexture.Width + 25) / 2, 700));
            aboutMeBoxTexture = Content.Load<Texture2D>("TutorialBox");

            titleFont = Content.Load<SpriteFont>("TitleFont");

            selectFilename = new TextBox("", startNewGame, uiFont, buttonTexture, new Vector2(Camera.ScreenWidth / 2, Camera.ScreenHeight / 2));
            selectFilename.setSelected(true);

            nextButton = new Button("Next >", uiFont, buttonTexture, () => pageOnLoadScreen++, new Vector2());
            backButton = new Button("< Back", uiFont, buttonTexture, () => pageOnLoadScreen--, new Vector2());

            // Adding the Sun by Kevin MacLeod
            addingTheSun = Content.Load<Song>("adding_the_sun");

            MediaPlayer.Play(addingTheSun);

            aboutMeBlurb = new DisplayTextBox(uiFont, aboutMeBoxTexture, 1000, new Vector2(Camera.ScreenWidth / 2, 450), aboutMe());
            controlsBlurb = new DisplayTextBox(uiFont, aboutMeBoxTexture, 1000, new Vector2(Camera.ScreenWidth / 2, 450), Keybinds.getControlsString());
        }

        // Updates the currently displayed buttons and UI elements, and 
        // manages switching between sections
        public void Update(GameTime gameTime)
        {
            var kState = Keyboard.GetState();
            if (kState.IsKeyDown(Keys.Escape))
            {
                if (loadingFile)
                {
                    loadingFile = false;
                }
                else if (creatingFile)
                {
                    creatingFile = false;
                }
                else if (aboutScreen)
                {
                    aboutScreen = false;
                }
                else if (controlsScreen)
                {
                    controlsScreen = false;
                }
            }

            if (loadingFile)
            {
                nextButton.Update();
                backButton.Update();
                int maxButtons = (Camera.ScreenHeight / 100) - 1;
                int maxPage = fileSelectButtons.Length / maxButtons;
                if (fileSelectButtons.Length % maxButtons == 0)
                {
                    maxPage -= 1;
                }
                if (pageOnLoadScreen > maxPage)
                {
                    pageOnLoadScreen = maxPage;
                }
                if (pageOnLoadScreen < 0)
                {
                    pageOnLoadScreen = 0;
                }
                for (int i = maxButtons * pageOnLoadScreen; i < Math.Min(maxButtons * (pageOnLoadScreen + 1), fileSelectButtons.Length); i++)
                {
                    fileSelectButtons[i].Update();
                    deleteButtons[i].Update();
                }
            } 
            else if (creatingFile)
            {
                selectFilename.Update();
            }
            else if (aboutScreen)
            {
                return;
            }
            else if (controlsScreen)
            {
                return;
            }
            else
            {
                startNewButton.Update();
                loadButton.Update();
                exitButton.Update();
                tutorialButton.Update();
                aboutButton.Update();
                controlsButton.Update();
            }
            
        }

        // Starts a new game with the tutorial enabled
        public void startTutorial()
        {
            MapView.isTutorial = true;
            startNewGame("tutorial");
        }

        // Starts a new game with the given file name
        public void startNewGame(string fileName)
        {
            creatingFile = false;
            MapView.setFilename(fileName);
            MapView.startNewGame = true;
            next = 1;
        }

        // Opens the load menu, displaying all saved files to the user
        public void loadGame()
        {
            loadingFile = true;
            Directory.CreateDirectory("Saves");
            string[] saves = Directory.GetFiles("Saves");
            
            fileSelectButtons = new Button[saves.Length];
            deleteButtons = new Button[saves.Length];
            
            for (int i = 0; i < saves.Length; i++)
            {
                string buttonText = saves[i].Split("\\")[1].Split(".")[0];
                fileSelectButtons[i] = new Button(buttonText, uiFont, buttonTexture, () => loadFile(buttonText), new Vector2());
                deleteButtons[i] = new Button("", uiFont, deleteButtonTexture, () => deleteFile(buttonText), new Vector2());
            }
        }

        // Loads the given save file and switches to the game
        public void loadFile(string file)
        {
            MapView.startNewGame = false;
            MapView.setFilename(file);
            next = 1;
            Debug.WriteLine(file);
            loadingFile = false;
        }

        // Deletes the given save file and regenerates the list of save files
        public void deleteFile(string file)
        {
            File.Delete("Saves\\" + file + ".txt");
            loadGame();
        }

        // Draws the current buttons and UI elements to the screen
        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            Vector2 bgScale = new Vector2((float)Camera.ScreenHeight / backgroundImage.Height, (float)Camera.ScreenHeight / backgroundImage.Height);
            _spriteBatch.Draw(backgroundImage, new Vector2(Camera.ScreenWidth / 2, Camera.ScreenHeight / 2), null, Color.White, 0f, new Vector2(backgroundImage.Width / 2, backgroundImage.Height / 2), bgScale, SpriteEffects.None, 0f);
            if (loadingFile)
            {
                Vector2 textPos = new Vector2(Camera.ScreenWidth / 2 - 140, 25);
                _spriteBatch.DrawString(uiFont, "Select a Save File to Load:", textPos + new Vector2(3, 3), Color.Black);
                _spriteBatch.DrawString(uiFont, "Select a Save File to Load:", textPos, Color.White);
                int maxButtons = (Camera.ScreenHeight / 100) - 1;
                int height = 100;
                int maxPage = fileSelectButtons.Length / maxButtons;
                if (fileSelectButtons.Length % maxButtons == 0)
                {
                    maxPage -= 1;
                }
                if (pageOnLoadScreen > maxPage)
                {
                    pageOnLoadScreen = maxPage;
                }
                if (pageOnLoadScreen < 0)
                {
                    pageOnLoadScreen = 0;
                }
                if (pageOnLoadScreen != maxPage)
                {
                    nextButton.setPosition(new Vector2(Camera.ScreenWidth - 250, Camera.ScreenHeight - 150));
                    nextButton.Draw(_spriteBatch);
                }
                if (pageOnLoadScreen != 0)
                {
                    backButton.setPosition(new Vector2(250, Camera.ScreenHeight - 150));
                    backButton.Draw(_spriteBatch);
                }

                for (int i = maxButtons * pageOnLoadScreen; i < Math.Min(maxButtons * (pageOnLoadScreen + 1), fileSelectButtons.Length); i++)
                {
                    fileSelectButtons[i].setPosition(new Vector2(Camera.ScreenWidth / 2, height));
                    deleteButtons[i].setPosition(new Vector2(Camera.ScreenWidth / 2 + 200, height));
                    fileSelectButtons[i].Draw(_spriteBatch);
                    deleteButtons[i].Draw(_spriteBatch);
                    height += 100;
                }
            }
            else if (creatingFile)
            {
                Vector2 textPos = new Vector2(Camera.ScreenWidth / 2 - 210, Camera.ScreenHeight / 2 - 50);
                _spriteBatch.DrawString(uiFont, "Enter a name for your new save:", textPos + new Vector2(3, 3), Color.Black);
                _spriteBatch.DrawString(uiFont, "Enter a name for your new save:", textPos, Color.White);
                selectFilename.Draw(_spriteBatch);
                selectFilename.setSelected(true);
            }
            else if (aboutScreen)
            {
                Vector2 textPos = new Vector2(Camera.ScreenWidth / 2 - 200, 50);
                _spriteBatch.DrawString(titleFont, "About:", textPos + new Vector2(5, 5), Color.Black);
                _spriteBatch.DrawString(titleFont, "About:", textPos, Color.White);
                aboutMeBlurb.Draw(_spriteBatch);
            }
            else if (controlsScreen)
            {
                Vector2 textPos = new Vector2(Camera.ScreenWidth / 2 - 300, 50);
                _spriteBatch.DrawString(titleFont, "Controls:", textPos + new Vector2(5, 5), Color.Black);
                _spriteBatch.DrawString(titleFont, "Controls:", textPos, Color.White);
                controlsBlurb.Draw(_spriteBatch );
            }
            else
            {
                startNewButton.Draw(_spriteBatch);
                loadButton.Draw(_spriteBatch);
                exitButton.Draw(_spriteBatch);
                tutorialButton.Draw(_spriteBatch);
                aboutButton.Draw(_spriteBatch);
                controlsButton.Draw(_spriteBatch);
                _spriteBatch.Draw(titleTexture, new Vector2(Camera.ScreenWidth / 2, 50), null, Color.White, 0f, new Vector2(titleTexture.Width / 2, 0), Camera.ScreenHeight / titleTexture.Height * 0.3f, SpriteEffects.None, 0f);
            }
            //textBox.Draw(_spriteBatch);
            _spriteBatch.End();
        }

        // Changing next from -1 causes Game1 to switch the screen
        public int NextScreen()
        {
            if (next != -1)
            {
                MediaPlayer.Stop();
            }
            return next;
        }

        // Starts this screen, and starts playing the title screen music
        public void Start()
        {
            MediaPlayer.Play(addingTheSun);
            next = -1;
        }

        // Ensures elements are drawn correctly when the screen is resized
        public void Resize()
        {
            startNewButton.setPosition(new Vector2(Camera.ScreenWidth / 2 - (buttonTexture.Width + 25) / 2, Camera.ScreenHeight / 2.5f));
            loadButton.setPosition(new Vector2(Camera.ScreenWidth / 2 + (buttonTexture.Width + 25) / 2, Camera.ScreenHeight / 2.5f));
            tutorialButton.setPosition(startNewButton.getPosition() + new Vector2(0, 100));
            aboutButton.setPosition(loadButton.getPosition() + new Vector2(0, 100));
            exitButton.setPosition(aboutButton.getPosition() + new Vector2(0, 100));
            controlsButton.setPosition(tutorialButton.getPosition() + new Vector2(0, 100));
            //loadButton.Resize();
            //exitButton.Resize();
            selectFilename.Resize();
            aboutMeBlurb.Resize();
            controlsBlurb.Resize();
        }

        // Called when the user presses a key
        // Key inputs get passed to the file name textbox
        // when creating files
        public void ReadKey(char key)
        {
            //textBox.ReadKey(key);
            if (creatingFile)
            {
                selectFilename.ReadKey(key);
            }
        }

        // Returns a string with a blurb about the project
        public string aboutMe()
        {
            return "Kevin Check\n" +
                "Period 1 Advanced Programming Topics\n" +
                "Project Completed on June 9th, 2024\n\n" +
                "Project name: AlmostSpace\n" +
                "AlmostSpace is a simple 2-dimensional orbital mechanics simulator. A user starts off in a rocket landed at earth, but is free to roam " +
                "the solar system as they please with infinite fuel and realistic physics. They control their rocket's throttle, heading, " +
                "and can bend time to their will as they try to wrap their minds around the relatively unintuitive realm of orbital mechanics.\n\n" +
                "This project was created using the following pieces of software:\n" +
                " - The MonoGame framework\n" +
                " - Visual Studio 2022\n" +
                " - The C# programming language\n" +
                " - GitHub Desktop\n" +
                " - GNU Image Manipulation Program (GIMP)";
        }
    }
}
