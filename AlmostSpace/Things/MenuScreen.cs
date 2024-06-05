﻿using AlmostSpace.Things.UserInterface;
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
        TextBox textBox;
        Texture2D buttonTexture;
        Texture2D backgroundImage;
        SpriteFont uiFont;
        Action exitCommand;

        ContentManager Content;

        bool loadingFile;
        Button[] fileSelectButtons;

        public MenuScreen(ContentManager Content, SpriteFont uiFont, Action exitCommand)
        {
            this.Content = Content;
            this.uiFont = uiFont;
            this.exitCommand = exitCommand;
        }

        public void LoadContent()
        {
            buttonTexture = Content.Load<Texture2D>("Button1");
            backgroundImage = Content.Load<Texture2D>("menu_background");
            startNewButton = new Button("Start New", uiFont, buttonTexture, startNewGame, new Vector2(Camera.ScreenWidth / 2, 500));
            loadButton = new Button("Load", uiFont, buttonTexture, loadGame, new Vector2(Camera.ScreenWidth / 2, 600));
            exitButton = new Button("Exit", uiFont, buttonTexture, exitCommand, new Vector2(Camera.ScreenWidth / 2, 700));

            //textBox = new TextBox("Hi", uiFont, buttonTexture, new Vector2(Camera.ScreenWidth / 2, 900));
        }

        public void Update(GameTime gameTime)
        {
            if (loadingFile)
            {
                foreach (Button button in fileSelectButtons) {
                    button.Update();
                }
            } 
            else
            {
                startNewButton.Update();
                loadButton.Update();
                exitButton.Update();
            }
            
            //textBox.Update();
        }

        public void startNewGame()
        {
            MapView.startNewGame = true;
            next = 1;
        }

        public void loadGame()
        {
            loadingFile = true;
            Directory.CreateDirectory("Saves");
            string[] saves = Directory.GetFiles("Saves");
            fileSelectButtons = new Button[saves.Length];
            for (int i = 0; i < saves.Length; i++)
            {
                string buttonText = saves[i].Split("\\")[1].Split(".")[0];
                fileSelectButtons[i] = new Button(buttonText, uiFont, buttonTexture, () => loadFile(buttonText), new Vector2(Camera.ScreenWidth / 2, 50 + 100 * i));
            }
            /*
            MapView.startNewGame = false;
            next = 1;
            */
        }

        public void loadFile(String file)
        {
            MapView.startNewGame = false;
            MapView.setFilename(file);
            next = 1;
            Debug.WriteLine(file);
            loadingFile = false;
        }

        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            // Picture from https://opengameart.org/content/pixel-space
            _spriteBatch.Draw(backgroundImage, new Vector2(Camera.ScreenWidth / 2, Camera.ScreenHeight / 2), null, Color.White, 0f, new Vector2(backgroundImage.Width / 2, backgroundImage.Height / 2), Vector2.One, SpriteEffects.None, 0f);
            if (loadingFile)
            {
                foreach (Button button in fileSelectButtons)
                {
                    button.Draw(_spriteBatch);
                }
            }
            else
            {
                startNewButton.Draw(_spriteBatch);
                loadButton.Draw(_spriteBatch);
                exitButton.Draw(_spriteBatch);
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
            loadButton.Resize();
            exitButton.Resize();
        }

        public void ReadKey(char key)
        {
            //textBox.ReadKey(key);
        }
    }
}
