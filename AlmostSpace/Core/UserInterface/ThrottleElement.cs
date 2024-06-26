﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AlmostSpace.Things.UserInterface
{
    // Represents a throttle bar element that allows the user to visually see the throttle
    // level that they currently have their engines at.
    internal class ThrottleElement
    {
        float throttle;
        Texture2D texture;
        Texture2D frame;

        Vector2 position;
        Vector2 scale;

        // Creates a new ThrottleElement object at the given position, with the given height and textures.
        public ThrottleElement(Vector2 position, float height, Texture2D texture, Texture2D frame)
        {
            this.texture = texture;
            this.frame = frame;

            this.position = position;

            scale = new Vector2(height / texture.Height, height / texture.Height);
        }

        // Updates the bar to match the current throttle level, and allows the user
        // to click and drag on the bar to change the throttle as well.
        public void Update(Rocket rocket)
        {
            throttle = rocket.getThrottle() / 100;
            var mState = Mouse.GetState();
            if (mState.LeftButton.Equals(ButtonState.Pressed))
            {
                Point mousePos = mState.Position;
                Vector2 topLeft = new Vector2(position.X - (texture.Width / 2) * scale.X, position.Y - (texture.Height / 2) * scale.Y);
                Vector2 bottomRight = new Vector2(position.X + (texture.Width / 2) * scale.X, position.Y + (texture.Height / 2) * scale.Y);

                if (mousePos.X > topLeft.X && mousePos.X < bottomRight.X && mousePos.Y > topLeft.Y && mousePos.Y < bottomRight.Y)
                {
                    rocket.setThrottle(1 - ((mousePos.Y - topLeft.Y) / (bottomRight.Y - topLeft.Y)));
                }
            }
        }

        // Draws the throttle bar to the screen.
        public void Draw(SpriteBatch spriteBatch)
        {
            position = new Vector2(100, Camera.ScreenHeight - 200);
            Vector2 framePosition = new Vector2(position.X, position.Y + (texture.Height / 2) * scale.Y);
            spriteBatch.Draw(texture, framePosition, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height), new Vector2(scale.X, throttle * scale.Y), SpriteEffects.None, 0f);
            spriteBatch.Draw(frame, position, null, Color.White, 0f, new Vector2(frame.Width / 2, frame.Height / 2), scale, SpriteEffects.None, 0f);

        }
    }
}
