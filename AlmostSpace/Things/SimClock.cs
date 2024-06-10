using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AlmostSpace.Things
{
    // An object that serves as a clock for the game, controlling, stopping, and speeding up time.
    internal class SimClock
    {
        // possible time factors
        float[] timeWarpLevels = { 1, 5, 10, 25, 50, 100, 500, 1000, 10000, 100000, 1000000 };
        int timeWarpLevel = 0;

        float timeFactor;

        double totalTimeElapsed;

        bool pToggle = true;
        bool timeStopped = false;

        bool periodPressed = false;
        bool commaPressed = false;

        GameTime gameTime;

        // Constructs a new SimClock object
        public SimClock() { 

        }

        // Constructs a new SimClock object from the given save data
        public SimClock(string data)
        {
            string[] lines = data.Split("\n");
            foreach(string line in lines)
            {
                string[] components = line.Split(": ");
                if (components.Length == 2)
                {
                    switch (components[0])
                    {
                        case "Total Time":
                            totalTimeElapsed = double.Parse(components[1]);
                            break;
                        case "Time Warp Level":
                            timeWarpLevel = int.Parse(components[1]);
                            break;
                        case "Time Factor":
                            timeFactor = float.Parse(components[1]);
                            break;
                        case "Time Stopped":
                            timeStopped = bool.Parse(components[1]);
                            break;
                    }

                }
                
            }
        }

        // Update the current game time and listen for time warp controls
        public void Update(GameTime gameTime)
        {
            this.gameTime = gameTime;

            var kState = Keyboard.GetState();

            if (pToggle && kState.IsKeyDown(Keybinds.pause))
            {
                timeStopped = !timeStopped;
                pToggle = false;
            }
            if (kState.IsKeyUp(Keybinds.pause))
            {
                pToggle = true;
            }

            if (kState.IsKeyDown(Keybinds.increaseTimeWarp) && !periodPressed)
            {
                if (timeWarpLevel < timeWarpLevels.Length - 1)
                {
                    timeWarpLevel++;
                }
                periodPressed = true;
            }

            if (kState.IsKeyDown(Keybinds.decreaseTimeWarp) && !commaPressed)
            {
                if (timeWarpLevel > 0)
                {
                    timeWarpLevel--;
                }
                commaPressed = true;
            }

            if (kState.IsKeyDown(Keybinds.cancelTimeWarp))
            {
                timeWarpLevel = 0;
            }

            if (kState.IsKeyUp(Keybinds.increaseTimeWarp))
            {
                periodPressed = false;
            }

            if (kState.IsKeyUp(Keybinds.decreaseTimeWarp))
            {
                commaPressed = false;
            }

            timeFactor = timeWarpLevels[timeWarpLevel];

            if (!timeStopped)
            {
                totalTimeElapsed += gameTime.ElapsedGameTime.TotalSeconds * timeFactor;
            }
        }

        // Pauses or starts the game
        public void setPaused(bool paused)
        {
            this.timeStopped = paused;
        }

        // gets the current time factor
        public float getTimeFactor()
        {
            return timeFactor;
        }

        // Sets the time warp level
        public void setTimeWarpLevel(int timeFactor)
        {
            this.timeWarpLevel = timeFactor;
        }

        // gets the total time elapsed in the game world (in seconds)
        public double getTime()
        {
            return totalTimeElapsed;
        }

        // gets the time the last game loop took
        public float getFrameTime()
        {
            if (timeStopped)
            {
                return 0;
            }
            if (gameTime == null)
            {
                return 1f / 60f;
            }
            return (float)gameTime.ElapsedGameTime.TotalSeconds * timeFactor;
        }

        // returns true if the game is paused
        public bool getTimeStopped()
        {
            return timeStopped;
        }

        // Returns the time elapsed as a string in years, days, hh:mm:ss
        public string getDisplayTime()
        {
            long totalSeconds = (long)totalTimeElapsed;
            long seconds = totalSeconds % 60;
            long minutes = (totalSeconds / 60) % 60;
            long hours = (totalSeconds / 3600) % 24;
            long days = (totalSeconds / 86400) % 365;
            long years = (totalSeconds / 31536000);

            string secondsString = (seconds + "").Length == 1 ? "0" + seconds : seconds + "";
            string minutesString = (minutes + "").Length == 1 ? "0" + minutes : minutes + "";
            string hoursString = (hours + "").Length == 1 ? "0" + hours : hours + "";

            return "Year " + years + ", Day " + days + ", " + hoursString + ":" + minutesString + ":" + secondsString;
        }

        // Returns the clock's data to be written to a save file
        public string getSaveData()
        {
            string output = "Type: " + "Clock" + "\n";
            output += "Total Time: " + totalTimeElapsed + "\n";
            output += "Time Warp Level: " + timeWarpLevel + "\n";
            output += "Time Factor: " + timeFactor + "\n";
            output += "Time Stopped: " + timeStopped + "\n\n";

            return output;
        }
    }
}
