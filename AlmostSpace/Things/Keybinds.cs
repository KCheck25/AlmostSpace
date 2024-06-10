using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    // This class contains all controls used within the game, allowing them to be easily referenced or changed
    // It also tries to read keybinds from the bindings.txt file, which allows a user to change the controls
    // without changing the source code by editing the file from a text editor. Eventually I plan to make a
    // way to do this in the GUI, but I ran out of time :(
    internal class Keybinds
    {
        public static Keys toggleEngine = Keys.Space;
        public static Keys increaseThrottle = Keys.LeftShift;
        public static Keys decreaseThrottle = Keys.LeftControl;
        public static Keys fullThrottle = Keys.Z;
        public static Keys cutThrottle = Keys.X;
        public static Keys rotateRight = Keys.A;
        public static Keys rotateLeft = Keys.D;

        public static Keys cameraRight = Keys.Right;
        public static Keys cameraLeft = Keys.Left;
        public static Keys cameraUp = Keys.Up;
        public static Keys cameraDown = Keys.Down;

        public static Keys pause = Keys.P;
        public static Keys increaseTimeWarp = Keys.OemPeriod;
        public static Keys decreaseTimeWarp = Keys.OemComma;
        public static Keys cancelTimeWarp = Keys.OemQuestion;

        public static Keys toggleFullScreen = Keys.F11;

        // Write the current key bindings to a file
        public static void saveBindings()
        {
            String toSave = "Toggle Engine: " + toggleEngine + "\n";
            toSave += "Increase Throttle: " + increaseThrottle + "\n";
            toSave += "Decrease Throttle: " + decreaseThrottle + "\n";
            toSave += "Full Throttle: " + fullThrottle + "\n";
            toSave += "Cut Throttle: " + cutThrottle + "\n";
            toSave += "Rotate Right: " + rotateRight + "\n";
            toSave += "Rotate Left: " + rotateLeft + "\n";
            toSave += "Camera Right: " + cameraRight + "\n";
            toSave += "Camera Left: " + cameraLeft + "\n";
            toSave += "Camera Up: " + cameraUp + "\n";
            toSave += "Camera Down: " + cameraDown + "\n";
            toSave += "Pause: " + pause + "\n";
            toSave += "Increase Time Warp: " + increaseTimeWarp + "\n";
            toSave += "Decrease Time Warp: " + decreaseTimeWarp + "\n";
            toSave += "Cancel Time Warp: " + cancelTimeWarp + "\n";
            toSave += "Toggle Full Screen: " + toggleFullScreen + "\n";

            using (StreamWriter writetext = new StreamWriter("bindings.txt"))
            {
                writetext.WriteLine(toSave);
            }
        }

        // Read and set keybinds from the bindings file
        public static void readBindings()
        {
            if (!File.Exists("bindings.txt"))
            {
                Debug.WriteLine("Generating config...");
                saveBindings();
                return;
            }
            using (StreamReader readtext = new StreamReader("bindings.txt"))
            {
                while (!readtext.EndOfStream)
                {
                    string line = readtext.ReadLine();
                    string[] tokens = line.Split(": ");

                    if (tokens.Length < 2)
                    {
                        continue;
                    }

                    Enum.TryParse(tokens[1], out Keys key);
                    if (key == Keys.None)
                    {
                        continue;
                    }
                    Debug.WriteLine(key);

                    switch (tokens[0])
                    {
                        case "Toggle Engine":
                            toggleEngine = key;
                            break;
                        case "Increase Throttle":
                            increaseThrottle = key;
                            break;
                        case "Decrease Throttle":
                            decreaseThrottle = key;
                            break;
                        case "Full Throttle":
                            fullThrottle = key;
                            break;
                        case "Cut Throttle":
                            cutThrottle = key;
                            break;
                        case "Rotate Right":
                            rotateRight = key;
                            break;
                        case "Rotate Left":
                            rotateLeft = key;
                            break;
                        case "Camera Right":
                            cameraRight = key;
                            break;
                        case "Camera Left":
                            cameraLeft = key;
                            break;
                        case "Camera Up":
                            cameraUp = key;
                            break;
                        case "Camera Down":
                            cameraDown = key;
                            break;
                        case "Pause":
                            pause = key;
                            break;
                        case "Increase Time Warp":
                            increaseTimeWarp = key;
                            break;
                        case "Decrease Time Warp":
                            decreaseTimeWarp = key;
                            break;
                        case "Cancel Time Warp":
                            cancelTimeWarp = key;
                            break;
                        case "Toggle Full Screen":
                            toggleFullScreen = key;
                            break;
                    }

                }

            }
        }



    }
}
