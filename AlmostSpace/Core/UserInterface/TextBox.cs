using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace AlmostSpace.Things.UserInterface
{
    // Represents an interactable text box in the UI that can be used to get
    // input from the user.
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

        // Creates a new TextBox object with the given starting text, command to run when
        // the enter key is pressed, font, texture, and position.
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

        // Checks if the textbox is clicked and marks it as selected if so, so that the user can type.
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

        // Adds the given character to the textbox
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
                    //Debug.WriteLine(text);
                    command(text);
                    text = "";
                }
                else if ((int)key >= 32)
                {
                    text += key;
                }
            }
        }

        // Ensures the textbox renders correctly when the window is resized
        public void Resize()
        {
            position.X = xPercent * Camera.ScreenWidth - texture.Width / 2;
            position.Y = yPercent * Camera.ScreenHeight;

            textPosition = position + textOffsets;
        }

        // Allows the textbox to be selected automatically from other files
        public void setSelected(bool selected)
        {
            this.selected = selected;
        }

        // Draws the textbox to the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            // Only display as much text as will fit in the box
            string displayText = text;
            while (font.MeasureString(displayText).X > dimensions.X - 40)
            {
                displayText = displayText.Substring(1, displayText.Length - 1);
            }
            spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(), new Vector2(dimensions.X / texture.Width, dimensions.Y / texture.Height), SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, displayText, textPosition, Color.Black);
        }
        
    }
}
