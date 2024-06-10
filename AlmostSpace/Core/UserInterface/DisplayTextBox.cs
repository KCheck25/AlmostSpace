using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things.UserInterface
{
    // Represents a non interactable text box to display information to the user
    internal class DisplayTextBox
    {
        SpriteFont font;
        Texture2D texture;
        int width;
        int originalWidth;
        string originalText;
        float heightScale;
        float widthScale;
        Vector2 position;
        string text;

        // Creates a new DisplayTextBox object using the given font, background texture, width in pixels, position
        // on the screen, and text to display
        public DisplayTextBox(SpriteFont font, Texture2D texture, int width, Vector2 position, String text)
        {
            this.font = font;
            this.texture = texture;
            this.width = width;
            this.originalWidth = width;
            this.position = position - new Vector2(texture.Width / 2, texture.Height / 2);
            originalText = text;

            setText(text);
            widthScale = (float)width / texture.Width;
        }

        // Formats the text correctly so that it will fit in the given width
        public void setText(string text)
        {
            originalText = text;
            int textWidth = width - 50;
            Vector2 size = font.MeasureString(text);
            // loop through each word and add newlines where necessary
            if (size.X > textWidth)
            {
                string[] words = text.Split(" ");
                string newText = "";
                int wordsIndex = 0;
                while (wordsIndex < words.Length - 1)
                {
                    string line = "";
                    while (font.MeasureString(line + words[wordsIndex] + " ").X < textWidth && wordsIndex < words.Length - 1)
                    {
                        line += words[wordsIndex] + " ";
                        wordsIndex++;
                    }
                    newText += line;
                    if (wordsIndex != words.Length - 1)
                    {
                        newText += "\n";
                    }
                }
                if (font.MeasureString(newText + words[wordsIndex]).X > textWidth)
                {
                    this.text = newText + "\n" + words[wordsIndex];
                } 
                else
                {
                    this.text = newText + words[wordsIndex];
                }
            }
            else
            {
                this.text = text;
            }
            heightScale = font.MeasureString(this.text).Y / texture.Height + 50.0f / texture.Height;
        }

        // Ensures the textbox displays correctly when the window is resized
        public void Resize()
        {
            if (width > Camera.ScreenWidth - 100)
            {
                width = Camera.ScreenWidth - 100;
            } else
            {
                width = originalWidth;
            }
            position.X = Camera.ScreenWidth / 2 - width / 2;
            widthScale = (float)width / texture.Width;
            setText(originalText);
        }

        // Draws the textbox to the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(), new Vector2(widthScale, heightScale), SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, text, position + new Vector2(25, 25), Color.White);
        }
    }
}
