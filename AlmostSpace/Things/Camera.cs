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
                    * Matrix.CreateTranslation(ScreenWidth / 2, ScreenHeight / 2, 0);
            }
        }

        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;
        
        // Position of center of camera
        public Vector2D focusPosition;
        float xOffset;
        float yOffset;

        float originalXOffset;
        float originalYOffset;

        float zoom;

        float cameraSpeed = 500;

        int prevScrollValue = 0;

        bool justClicked;
        Point clickPos = new Point();

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

            // Arrow keys move the camera
            if (kState.IsKeyDown(Keybinds.cameraUp))
            {
                yOffset -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }
            if (kState.IsKeyDown(Keybinds.cameraLeft))
            {
                xOffset -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }
            if (kState.IsKeyDown(Keybinds.cameraDown))
            {
                yOffset += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }
            if (kState.IsKeyDown(Keybinds.cameraRight))
            {
                xOffset += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }

            // Right click and drag also moves the camera!
            if (mouseState.RightButton.Equals(ButtonState.Pressed))
            {
                if (!justClicked)
                {
                    justClicked = true;
                    clickPos = mouseState.Position;
                    originalXOffset = xOffset;
                    originalYOffset = yOffset;
                }
                else
                {
                    xOffset = originalXOffset + -(1 / zoom) * (mouseState.Position.X - clickPos.X);
                    yOffset = originalYOffset + -(1 / zoom) * (mouseState.Position.Y - clickPos.Y);
                }
            } else
            {
                justClicked = false;
            }

            //Debug.WriteLine(mouseState.ScrollWheelValue);

            // Scroll to zoom
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

        public float getZoom()
        {
            return zoom;
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
