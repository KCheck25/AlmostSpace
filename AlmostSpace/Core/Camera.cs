using System;
using AlmostSpace.Core.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AlmostSpace.Things
{
    // A 2D Camera object that converts coordinates of objects in space to coordinates
    // on the screen. It can be moved, rotated, and zoomed in and out.
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
                    * Matrix.CreateTranslation(screenX(), screenY(), 0)
                    * Matrix.CreateRotationZ(rotation);
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

        float rotation = 0;

        bool justClicked;
        Point clickPos = new Point();

        // Create a new camera centered on the center of the screen with normal zoom
        public Camera()
        {
            focusPosition = new Vector2D(0, 0);
            zoom = 0.00006f;
        }

        // Create a new camera based on data from a save file
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

        // Change the camera position and zoom based on user inputs
        public void update(GameTime gameTime)
        {
            var kState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            float yToMove = 0;
            float xToMove = 0;
            // Arrow keys move the camera
            if (kState.IsKeyDown(Keybinds.cameraUp))
            {
                yToMove -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }
            if (kState.IsKeyDown(Keybinds.cameraLeft))
            {
                xToMove -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }
            if (kState.IsKeyDown(Keybinds.cameraDown))
            {
                yToMove += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }
            if (kState.IsKeyDown(Keybinds.cameraRight))
            {
                xToMove += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * (1 / zoom);
            }

            xOffset += MathF.Cos(-rotation) * xToMove - MathF.Sin(-rotation) * yToMove;
            yOffset += MathF.Sin(-rotation) * xToMove + MathF.Cos(-rotation) * yToMove;

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
                    Vector2 changeVector = new Vector2(-(1 / zoom) * (mouseState.Position.X - clickPos.X), -(1 / zoom) * (mouseState.Position.Y - clickPos.Y));
                    float rotatedX = MathF.Cos(-rotation) * changeVector.X - MathF.Sin(-rotation) * changeVector.Y;
                    float rotatedY = MathF.Sin(-rotation) * changeVector.X + MathF.Cos(-rotation) * changeVector.Y;

                    xOffset = originalXOffset + rotatedX;
                    yOffset = originalYOffset + rotatedY;
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

        // Resets the camera's offsets from it's origin
        public void clearOffsets()
        {
            xOffset = 0; 
            yOffset = 0;
        }

        // Sets the rotation of the camera in radians
        public void setRotation(float rotation)
        {
            this.rotation = rotation;
        }

        // Returns the zoom level of the camera
        public float getZoom()
        {
            return zoom;
        }

        // Returns the rotation of the camera in radians
        public float getRotation()
        {
            return rotation;
        }

        // Returns the zero x position of the camera on the screen, accounting for camera rotation
        float screenX()
        {
            float X = ScreenWidth / 2;
            float Y = ScreenHeight / 2;
            return MathF.Cos(-rotation) * X - MathF.Sin(-rotation) * Y;
        }

        // Returns the zero y position of the camera on the screen, accounting for camera rotation
        float screenY()
        {
            float X = ScreenWidth / 2;
            float Y = ScreenHeight / 2;
            return MathF.Sin(-rotation) * X + MathF.Cos(-rotation) * Y;
        }

        // Returns a string containing important camera data to be written to a save file
        public string getSaveData()
        {
            string output = "Type: " + "Camera" + "\n";
            output += "Focus Position: " + focusPosition.X + "," + focusPosition.Y + "\n";
            output += "X Offset: " + xOffset + "\n";
            output += "Y Offset: " + yOffset + "\n";
            output += "Zoom: " + zoom + "\n\n";

            return output;
        }

    }
}
