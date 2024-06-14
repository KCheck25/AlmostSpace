using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AlmostSpace.Things.UserInterface
{
    // Represents and displays a "navball" which is an instrument stolen from Kerbal Space program
    // It essentially tells the user what direction they are facing relative to their current orbit.
    internal class NavBallElement
    {
        float angle;
        Texture2D texture;
        Texture2D prograde;
        Texture2D retrograde;
        Texture2D radialIn;
        Texture2D radialOut;
        Texture2D frame;

        Vector2 position;
        Vector2 scale;
        float radius;
        bool swapRiRo;

        // Creates a new NavBallElement object at the given position, with the given size and textures.
        public NavBallElement(Vector2 position, float radius, Texture2D texture, Texture2D frame, Texture2D prograde, Texture2D retrograde, Texture2D radialIn, Texture2D radialOut)
        {
            this.texture = texture;
            this.frame = frame;
            this.prograde = prograde;
            this.retrograde = retrograde;
            this.radialIn = radialIn;
            this.radialOut = radialOut;

            this.radius = radius;
            this.position = position;
            scale = new Vector2(radius / (texture.Width / 2), radius / (texture.Height / 2));
        }

        // Update the instrument
        public void Update(Rocket rocket)
        {
            angle = rocket.getAngle() - (float)Math.Atan2(rocket.getRelativeVelocity().Y, rocket.getRelativeVelocity().X);
            swapRiRo = rocket.getDirection() < 0;
        }

        // Draw the navball to the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            position = new Vector2(Camera.ScreenWidth - 200, Camera.ScreenHeight - 200);
            spriteBatch.Draw(texture, position, null, Color.White, angle, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 0f);
            // Force the indicators to always render upright instead of being fixed to the ball
            spriteBatch.Draw(prograde, position + new Vector2(MathF.Cos(angle - MathHelper.PiOver2) * 0.7f * radius, MathF.Sin(angle - MathHelper.PiOver2) * 0.7f * radius), null, Color.White, 0f, new Vector2(prograde.Width / 2, prograde.Height / 2), scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(retrograde, position + new Vector2(MathF.Cos(angle + MathHelper.PiOver2) * 0.7f * radius, MathF.Sin(angle + MathHelper.PiOver2) * 0.7f * radius), null, Color.White, 0f, new Vector2(retrograde.Width / 2, retrograde.Height / 2), scale, SpriteEffects.None, 0f);
            if (!swapRiRo)
            {
                spriteBatch.Draw(radialOut, position + new Vector2(MathF.Cos(angle) * 0.7f * radius, MathF.Sin(angle) * 0.7f * radius), null, Color.White, 0f, new Vector2(radialIn.Width / 2, radialIn.Height / 2), scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(radialIn, position + new Vector2(MathF.Cos(angle + MathHelper.Pi) * 0.7f * radius, MathF.Sin(angle + MathHelper.Pi) * 0.7f * radius), null, Color.White, 0f, new Vector2(radialOut.Width / 2, radialOut.Height / 2), scale, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(radialIn, position + new Vector2(MathF.Cos(angle) * 0.7f * radius, MathF.Sin(angle) * 0.7f * radius), null, Color.White, 0f, new Vector2(radialIn.Width / 2, radialIn.Height / 2), scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(radialOut, position + new Vector2(MathF.Cos(angle + MathHelper.Pi) * 0.7f * radius, MathF.Sin(angle + MathHelper.Pi) * 0.7f * radius), null, Color.White, 0f, new Vector2(radialOut.Width / 2, radialOut.Height / 2), scale, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(frame, position, null, Color.White, 0f, new Vector2(frame.Width / 2, frame.Height / 2), scale, SpriteEffects.None, 0f);

        }
    }
}
