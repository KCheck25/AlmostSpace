using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    internal class SimClock
    {
        // possible time factors
        float[] timeWarpLevels = { 1, 5, 10, 25, 50, 100, 500, 1000, 10000, 100000 };
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

        // Update the current game time and listen for time warp controls
        public void Update(GameTime gameTime)
        {
            this.gameTime = gameTime;

            var kState = Keyboard.GetState();

            if (pToggle && kState.IsKeyDown(Keys.P))
            {
                timeStopped = !timeStopped;
                pToggle = false;
            }
            if (kState.IsKeyUp(Keys.P))
            {
                pToggle = true;
            }

            if (kState.IsKeyDown(Keys.OemPeriod) && !periodPressed)
            {
                if (timeWarpLevel < timeWarpLevels.Length - 1)
                {
                    timeWarpLevel++;
                }
                periodPressed = true;
            }

            if (kState.IsKeyDown(Keys.OemComma) && !commaPressed)
            {
                if (timeWarpLevel > 0)
                {
                    timeWarpLevel--;
                }
                commaPressed = true;
            }

            if (kState.IsKeyUp(Keys.OemPeriod))
            {
                periodPressed = false;
            }

            if (kState.IsKeyUp(Keys.OemComma))
            {
                commaPressed = false;
            }

            timeFactor = timeWarpLevels[timeWarpLevel];

            if (!timeStopped)
            {
                totalTimeElapsed += gameTime.ElapsedGameTime.TotalSeconds * timeFactor;
            }
        }

        // gets the current time factor
        public float getTimeFactor()
        {
            return timeFactor;
        }

        // gets the total time elapsed in the game world (in seconds)
        public double getTime()
        {
            return totalTimeElapsed;
        }

        // gets the time the last game loop took
        public float getFrameTime()
        {
            return (float)gameTime.ElapsedGameTime.TotalSeconds * timeFactor;
        }

        // returns true if the game is paused
        public bool getTimeStopped()
        {
            return timeStopped;
        }

        // Returns the time elapsed as a string in years, days, hh:mm:ss
        public String getDisplayTime()
        {
            long totalSeconds = (long)totalTimeElapsed;
            long seconds = totalSeconds % 60;
            long minutes = (totalSeconds / 60) % 60;
            long hours = (totalSeconds / 3600) % 24;
            long days = (totalSeconds / 86400) % 365;
            long years = (totalSeconds / 31536000);

            String secondsString = (seconds + "").Length == 1 ? "0" + seconds : seconds + "";
            String minutesString = (minutes + "").Length == 1 ? "0" + minutes : minutes + "";
            String hoursString = (hours + "").Length == 1 ? "0" + hours : hours + "";

            return "Year " + years + ", Day " + days + ", " + hoursString + ":" + minutesString + ":" + secondsString;
        }
    }
}
