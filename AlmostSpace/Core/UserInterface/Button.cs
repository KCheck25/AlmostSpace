using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AlmostSpace.Things.UserInterface
{
    // Represents a clickable button in the UI. Clicking the button runs the method passed
    // to the button's constructor.
    internal class Button
    {
        string text;
        SpriteFont font;
        Texture2D texture;
        Vector2 position;
        Vector2 textPosition;
        Vector2 dimensions;
        Vector2 textOffsets;
        Action command;

        bool isPressed;
        bool firstLoop;

        bool justLoaded;

        float xPercent;
        float yPercent;

        // Creates a new button object displaying the given text in the given font on the button,
        // using the given texture for the button, and at the given coordinates
        public Button(string text, SpriteFont font, Texture2D texture, Action command, Vector2 position)
        {
            this.font = font;
            this.position = position;
            this.position.X -= texture.Width / 2;
            dimensions.X = texture.Width;
            dimensions.Y = texture.Height;
            this.texture = texture;
            this.command = command;
            firstLoop = true;

            while (font.MeasureString(text).X > dimensions.X - 40)
            {
                text = text.Substring(1, text.Length - 1);
            }
            this.text = text;

            Vector2 textDimensions = font.MeasureString(text);
            textOffsets = new Vector2((dimensions.X - textDimensions.X) / 2, (dimensions.Y - textDimensions.Y) / 2);
            textPosition = this.position + textOffsets;

            xPercent = position.X / Camera.ScreenWidth;
            yPercent = position.Y / Camera.ScreenHeight;

            justLoaded = true;
        }

        // Checks if the button is being pressed and runs the command given to the constructor if so
        public void Update()
        {
            var mState = Mouse.GetState();
            if (justLoaded)
            {
                justLoaded = (mState.LeftButton == ButtonState.Pressed);
                return;
            }
            if (mState.LeftButton == ButtonState.Pressed)
            {
                Point mousePos = mState.Position;
                if (mousePos.X < position.X + dimensions.X && mousePos.X > position.X && mousePos.Y < position.Y + dimensions.Y && mousePos.Y > position.Y)
                {
                    isPressed = true;
                    // Make sure the command is run once and not spammed
                    if (firstLoop)
                    {
                        command();
                        firstLoop = false;
                    }
                }
            }
            else if (isPressed)
            {
                isPressed = false;
                firstLoop = true;
            }
        }

        // Sets the position of this button on the screen
        public void setPosition(Vector2 position)
        {
            this.position = position;
            this.position.X -= texture.Width / 2;
            textPosition = this.position + textOffsets;

            xPercent = position.X / Camera.ScreenWidth;
            yPercent = position.Y / Camera.ScreenHeight;
        }

        // Ensures the button displays in the correct spot when the window is resized
        public void Resize()
        {
            position.X = xPercent * Camera.ScreenWidth - texture.Width / 2;
            position.Y = yPercent * Camera.ScreenHeight;

            textPosition = position + textOffsets;
        }

        // Draws the button to the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(), new Vector2(dimensions.X / texture.Width, dimensions.Y / texture.Height), SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, textPosition, Color.Black);
        }

        // Returns the position on the screen of the center of this button
        public Vector2 getPosition()
        {
            return position + new Vector2(texture.Width / 2, 0);
        }
    }
}
