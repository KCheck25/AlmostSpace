using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AlmostSpace.Things
{
    // Adapted from https://roguesharp.wordpress.com/2014/07/13/tutorial-5-creating-a-2d-camera-with-pan-and-zoom-in-monogame/
    internal class Camera
    {
        // Matrix by which to transform positions/scales of objects
        public Matrix transform
        {
            get
            {
                return Matrix.CreateTranslation(-(float)focusPosition.X - xOffset, -(float)focusPosition.Y - yOffset, 0)
                    * Matrix.CreateScale(new Vector3(zoom, zoom, 1))
                    * Matrix.CreateTranslation(1920 / 2, 1080 / 2, 0);
            }
        }
        
        // Position of center of camera
        public Vector2D focusPosition;
        float xOffset;
        float yOffset;

        float zoom;

        float cameraSpeed = 500;

        int prevScrollValue = 0;

        // Create a new camera centered on the center of the screen with normal zoom
        public Camera()
        {
            focusPosition = new Vector2D(0, 0);
            zoom = 0.00006f;
        }

        public Camera(String data)
        {
            String[] lines = data.Split("\n");
            foreach (String line in lines)
            {
                String[] components = line.Split(": ");
                if (components.Length == 2)
                {
                    switch (components[0])
                    {
                        case "Focus Position":
                            focusPosition.X = double.Parse(components[1].Split(",")[0]);
                            focusPosition.Y = double.Parse(components[1].Split(",")[1]);
                            break;
                        case "X Offset":
                            xOffset = float.Parse(components[1]);
                            break;
                        case "Y Offset":
                            yOffset = float.Parse(components[1]);
                            break;
                        case "Zoom":
                            zoom = float.Parse(components[1]);
                            break;
                    }

                }

            }
        }

        // Change the camera position / zoom based on user inputs
        public void update(GameTime gameTime)
        {
            var kState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (kState.IsKeyDown(Keys.Up))
            {
                yOffset -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }
            if (kState.IsKeyDown(Keys.Left))
            {
                xOffset -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }
            if (kState.IsKeyDown(Keys.Down))
            {
                yOffset += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }
            if (kState.IsKeyDown(Keys.Right))
            {
                xOffset += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }

            //Debug.WriteLine(mouseState.ScrollWheelValue);

            if (mouseState.ScrollWheelValue < prevScrollValue)
            {
                zoom -= zoom / 5;
            }
            if (mouseState.ScrollWheelValue > prevScrollValue)
            {
                zoom += zoom / 5;
            }

            prevScrollValue = mouseState.ScrollWheelValue;

        }

        public void setFocusPosition(Vector2D position)
        {
            this.focusPosition = position;
        }

        public void clearOffsets()
        {
            xOffset = 0; 
            yOffset = 0;
        }

        public void setRotation()
        {

        }

        public String getSaveData()
        {
            String output = "Type: " + "Camera" + "\n";
            output += "Focus Position: " + focusPosition.X + "," + focusPosition.Y + "\n";
            output += "X Offset: " + xOffset + "\n";
            output += "Y Offset: " + yOffset + "\n";
            output += "Zoom: " + zoom + "\n\n";

            return output;
        }

    }
}
