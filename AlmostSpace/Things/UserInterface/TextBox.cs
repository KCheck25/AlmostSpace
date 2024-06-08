using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things.UserInterface
{
    internal class TextBox
    {
        string text;
        SpriteFont font;
        Texture2D texture;
        Vector2 position;
        Vector2 textPosition;
        Vector2 dimensions;
        Vector2 textOffsets;

        bool isPressed;
        bool firstLoop;
        bool selected;

        float xPercent;
        float yPercent;

        Action<string> command;

        public TextBox(string text, Action<string> command, SpriteFont font, Texture2D texture, Vector2 position)
        {
            this.text = text;
            this.font = font;
            this.position = position;
            this.position.X -= texture.Width / 2;
            dimensions.X = texture.Width;
            dimensions.Y = texture.Height;
            this.texture = texture;
            firstLoop = true;
            Vector2 textDimensions = font.MeasureString("hello");
            textOffsets = new Vector2(20, (dimensions.Y - textDimensions.Y) / 2);
            textPosition = this.position + textOffsets;

            xPercent = position.X / Camera.ScreenWidth;
            yPercent = position.Y / Camera.ScreenHeight;

            this.command = command;
        }

        // Checks if the button is being pressed and runs the given command if so
        public void Update()
        {
            var mState = Mouse.GetState();
            if (mState.LeftButton == ButtonState.Pressed)
            {
                Point mousePos = mState.Position;
                if (mousePos.X < position.X + dimensions.X && mousePos.X > position.X && mousePos.Y < position.Y + dimensions.Y && mousePos.Y > position.Y)
                {
                    isPressed = true;
                    if (firstLoop)
                    {
                        firstLoop = false;
                        selected = true;
                    }
                } 
                else
                {
                    selected = false;
                }
            }
            else if (isPressed)
            {
                isPressed = false;
                firstLoop = true;
            }
        }

        public void ReadKey(Char key)
        {
            if (selected)
            {
                if ((int)key == 8)
                {
                    if (text.Length != 0)
                    {
                        text = text.Substring(0, text.Length - 1);
                    }
                } 
                else if ((int)key == 13)
                {
                    Debug.WriteLine(text);
                    command(text);
                    text = "";
                }
                else if ((int)key >= 32)
                {
                    text += key;
                }
            }
        }

        public void Resize()
        {
            position.X = xPercent * Camera.ScreenWidth - texture.Width / 2;
            position.Y = yPercent * Camera.ScreenHeight;

            textPosition = position + textOffsets;
        }

        public void setSelected(bool selected)
        {
            this.selected = selected;
        }

        // Draws the button to the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            String displayText = text;
            while (font.MeasureString(displayText).X > dimensions.X - 40)
            {
                displayText = displayText.Substring(1, displayText.Length - 1);
            }
            spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(), new Vector2(dimensions.X / texture.Width, dimensions.Y / texture.Height), SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, displayText, textPosition, Color.Black);
        }
        
    }
}
