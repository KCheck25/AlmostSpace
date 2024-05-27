using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Net.Sockets;

namespace AlmostSpace.Things.UserInterface
{
    // Represents a clickable button in the UI
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

        float xPercent;
        float yPercent;

        // Creates a new button object displaying the given text in the given font on the button,
        // using the given texture for the button, and at the given coordinates
        public Button(string text, SpriteFont font, Texture2D texture, Action command, Vector2 position, Vector2 dimensions)
        {
            this.text = text;
            this.font = font;
            this.position = position;
            this.dimensions = dimensions;
            this.texture = texture;
            this.command = command;
            firstLoop = true;

            Vector2 textDimensions = font.MeasureString(text);
            Vector2 textOffsets = new Vector2((dimensions.X - textDimensions.X) / 2, (dimensions.Y - textDimensions.Y) / 2);
            textPosition = position + textOffsets;
        }

        public Button(string text, SpriteFont font, Texture2D texture, Action command, Vector2 position)
        {
            this.text = text;
            this.font = font;
            this.position = position;
            this.position.X -= texture.Width / 2;
            dimensions.X = texture.Width;
            dimensions.Y = texture.Height;
            this.texture = texture;
            this.command = command;
            firstLoop = true;

            Vector2 textDimensions = font.MeasureString(text);
            textOffsets = new Vector2((dimensions.X - textDimensions.X) / 2, (dimensions.Y - textDimensions.Y) / 2);
            textPosition = this.position + textOffsets;

            xPercent = position.X / Camera.ScreenWidth;
            yPercent = position.Y / Camera.ScreenHeight;
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
    }
}
