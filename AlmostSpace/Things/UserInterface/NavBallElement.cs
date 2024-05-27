using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things.UserInterface
{
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

        public void Update(Rocket rocket)
        {
            angle = rocket.getAngle() - (float)Math.Atan2(rocket.getRelativeVelocity().Y, rocket.getRelativeVelocity().X);
            swapRiRo = rocket.getDirection() < 0;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            position = new Vector2(Camera.ScreenWidth - 200, Camera.ScreenHeight - 200);
            spriteBatch.Draw(texture, position, null, Color.White, angle, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 0f);
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
