using AlmostSpace.Things.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    internal class MenuScreen : Screen
    {
        int next = -1;
        Button startNewButton;
        Button loadButton;
        Button exitButton;
        Button tutorialButton;
        TextBox textBox;
        Texture2D buttonTexture;
        Texture2D backgroundImage;
        Texture2D deleteButtonTexture;
        SpriteFont uiFont;
        Action exitCommand;

        ContentManager Content;

        bool loadingFile;
        bool creatingFile;
        Button[] fileSelectButtons;
        Button[] deleteButtons;
        TextBox selectFilename;
        Button nextButton;
        Button backButton;

        int pageOnLoadScreen = 0;

        public MenuScreen(ContentManager Content, SpriteFont uiFont, Action exitCommand)
        {
            this.Content = Content;
            this.uiFont = uiFont;
            this.exitCommand = exitCommand;
        }

        public void LoadContent()
        {
            buttonTexture = Content.Load<Texture2D>("Button1");
            deleteButtonTexture = Content.Load<Texture2D>("DeleteButton");
            backgroundImage = Content.Load<Texture2D>("menu_background");
            startNewButton = new Button("Start New", uiFont, buttonTexture, () => creatingFile = true, new Vector2(Camera.ScreenWidth / 2, 500));
            tutorialButton = new Button("Tutorial", uiFont, buttonTexture, () => creatingFile = true, new Vector2(Camera.ScreenWidth / 2, 700));
            loadButton = new Button("Load", uiFont, buttonTexture, loadGame, new Vector2(Camera.ScreenWidth / 2, 600));
            exitButton = new Button("Exit", uiFont, buttonTexture, exitCommand, new Vector2(Camera.ScreenWidth / 2, 800));

            selectFilename = new TextBox("", startNewGame, uiFont, buttonTexture, new Vector2(Camera.ScreenWidth / 2, Camera.ScreenHeight / 2));
            selectFilename.setSelected(true);

            nextButton = new Button("Next >", uiFont, buttonTexture, () => pageOnLoadScreen++, new Vector2());
            backButton = new Button("< Back", uiFont, buttonTexture, () => pageOnLoadScreen--, new Vector2());
            //textBox = new TextBox("Hi", uiFont, buttonTexture, new Vector2(Camera.ScreenWidth / 2, 900));
        }

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
            }

            if (loadingFile)
            {
                nextButton.Update();
                backButton.Update();
                int maxButtons = (Camera.ScreenHeight / 100) - 1;
                int maxPage = fileSelectButtons.Length / maxButtons;
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
            else
            {
                startNewButton.Update();
                loadButton.Update();
                exitButton.Update();
                tutorialButton.Update();
            }
            
            //textBox.Update();
        }

        public void startNewGame(string fileName)
        {
            creatingFile = false;
            MapView.setFilename(fileName);
            MapView.startNewGame = true;
            next = 1;
        }

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
            /*
            MapView.startNewGame = false;
            next = 1;
            */
        }

        public void loadFile(string file)
        {
            MapView.startNewGame = false;
            MapView.setFilename(file);
            next = 1;
            Debug.WriteLine(file);
            loadingFile = false;
        }

        public void deleteFile(string file)
        {
            File.Delete("Saves\\" + file + ".txt");
            loadGame();
        }

        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            // Picture from https://opengameart.org/content/pixel-space
            Vector2 bgScale = new Vector2((float)Camera.ScreenHeight / backgroundImage.Height, (float)Camera.ScreenHeight / backgroundImage.Height);
            _spriteBatch.Draw(backgroundImage, new Vector2(Camera.ScreenWidth / 2, Camera.ScreenHeight / 2), null, Color.White, 0f, new Vector2(backgroundImage.Width / 2, backgroundImage.Height / 2), bgScale, SpriteEffects.None, 0f);
            if (loadingFile)
            {
                _spriteBatch.DrawString(uiFont, "Select a Save File to Load:", new Vector2(Camera.ScreenWidth / 2 - 140, 25), Color.White);
                int maxButtons = (Camera.ScreenHeight / 100) - 1;
                int height = 100;
                int maxPage = fileSelectButtons.Length / maxButtons;
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
                _spriteBatch.DrawString(uiFont, "Enter a name for your new save:", new Vector2(Camera.ScreenWidth / 2 - 200, Camera.ScreenHeight / 2 - 50), Color.White);
                selectFilename.Draw(_spriteBatch);
                selectFilename.setSelected(true);
            }
            else
            {
                startNewButton.Draw(_spriteBatch);
                loadButton.Draw(_spriteBatch);
                exitButton.Draw(_spriteBatch);
                tutorialButton.Draw(_spriteBatch);
            }
            //textBox.Draw(_spriteBatch);
            _spriteBatch.End();
        }

        public int NextScreen()
        {
            return next;
        }

        public void Start()
        {
            next = -1;
        }

        public void Resize()
        {
            startNewButton.Resize();
            loadButton.setPosition(startNewButton.getPosition() + new Vector2(0, 100));
            tutorialButton.setPosition(loadButton.getPosition() + new Vector2(0, 100));
            exitButton.setPosition(tutorialButton.getPosition() + new Vector2(0, 100));
            //loadButton.Resize();
            //exitButton.Resize();
            selectFilename.Resize();
        }

        public void ReadKey(char key)
        {
            //textBox.ReadKey(key);
            if (creatingFile)
            {
                selectFilename.ReadKey(key);
            }
        }
    }
}
