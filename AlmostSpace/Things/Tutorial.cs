using AlmostSpace.Things.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    // This class serves as an interactive tutorial that pops up over the game
    // when the tutorial button is pressed in the main menu
    internal class Tutorial
    {
        Rocket rocket;
        SimClock clock;
        Camera camera;

        string[] dialogue;
        List<int> noTab;
        int currentDialogue;
        int prevDialogue;
        bool printedCurrent;
        bool tabPressed;
        bool waitForTask;
        bool drawBox;

        SpriteFont font;
        Texture2D texture;

        DisplayTextBox textBox;

        double timeStart;
        bool firstTime;

        string initialCameraSettings;

        // Creates a new Tutorial object from the given rocket, camera, clock, font, and texture
        public Tutorial(Rocket rocket, Camera camera, SimClock clock, SpriteFont font, Texture2D texture)
        {
            this.rocket = rocket;
            this.camera = camera;
            this.clock = clock;
            this.font = font;
            this.texture = texture;

            currentDialogue = 20;
            prevDialogue = currentDialogue;
            printedCurrent = false;
            drawBox = true;
            firstTime = true;

            genDialogue();
            noTab = new List<int>();
            noTab.Add(5);
            noTab.Add(8);
            noTab.Add(10);
            noTab.Add(11);
            noTab.Add(13);
            noTab.Add(15);
            noTab.Add(18);

            textBox = new DisplayTextBox(font, texture, 1000, new Vector2(Camera.ScreenWidth / 2, 300), dialogue[currentDialogue]);
        }

        // Checks if the user wants to move to the next section of the tutorial, or if they have
        // achieved what they need to do to unlock the next section
        public void Update()
        {
            var kState = Keyboard.GetState();

            if (kState.IsKeyDown(Keys.Tab) && !tabPressed) {
                if (!waitForTask && !noTab.Contains(currentDialogue))
                {
                    currentDialogue++;
                    if (currentDialogue == 3)
                    {
                        waitForTask = true;
                    }
                    if (currentDialogue == 21)
                    {
                        drawBox = false;
                    }
                }
                tabPressed = true;
            }
            if (!kState.IsKeyDown (Keys.Tab))
            {
                tabPressed = false;
            }

            if (waitForTask)
            {
                if (currentDialogue == 3)
                {
                    if (rocket.getThrottle() != 100)
                    {
                        waitForTask = false;
                        currentDialogue++;
                    }
                }
            }
            if (currentDialogue == 5)
            {
                if (rocket.getEngineState().Equals("On"))
                {
                    drawBox = false;
                }
                if (rocket.getHeight() > 1000)
                {
                    drawBox = true;
                    currentDialogue = 6;
                    clock.setPaused(true);
                }
            }
            if (currentDialogue == 8)
            {
                if (!clock.getTimeStopped())
                {
                    drawBox = false;
                }
                if (rocket.getApoapsisHeight() > 400000)
                {
                    drawBox = true;
                    currentDialogue = 9;
                    clock.setPaused(true);
                }
            }
            if (currentDialogue == 10)
            {
                if(!clock.getTimeStopped())
                {
                    drawBox = false;
                    if (firstTime)
                    {
                        timeStart = clock.getTime();
                        firstTime = false;
                    }
                }
                if (clock.getTime() - timeStart > 10 && !firstTime)
                {
                    drawBox = true;
                    currentDialogue = 11;
                    clock.setPaused(true);
                    firstTime = true;
                }
            }
            if (currentDialogue == 11)
            {
                if (!clock.getTimeStopped())
                {
                    drawBox = false;
                }
                if (rocket.getHeight() > 375000 && clock.getTimeFactor() == 1)
                {
                    drawBox = true;
                    currentDialogue = 13;
                    clock.setPaused(true);
                } 
                else if (rocket.getHeight() > 395000)
                {
                    drawBox = true;
                    currentDialogue = 12;
                    clock.setPaused(true);
                    clock.setTimeWarpLevel(0);
                }
            }
            if (currentDialogue == 13)
            {
                if (!clock.getTimeStopped())
                {
                    drawBox = false;
                }
                if (rocket.getPeriapsisHeight() > 300000)
                {
                    drawBox = true;
                    currentDialogue = 14;
                    clock.setPaused(true);
                }
            }
            if (currentDialogue == 15)
            {
                if (firstTime)
                {
                    initialCameraSettings = camera.getSaveData();
                    firstTime = false;
                }
                else if (camera.getSaveData() != initialCameraSettings)
                {
                    currentDialogue = 16;
                    firstTime = true;
                }
            }
            if (currentDialogue == 18)
            {
                if (rocket.getApoapsisHeight() > 800000)
                {
                    currentDialogue = 19;
                    clock.setPaused(true);
                }
            }
            if (currentDialogue == 19)
            {
                if (rocket.getPeriapsisHeight() > 800000)
                {
                    currentDialogue = 20;
                }
            }
            if (currentDialogue != prevDialogue)
            {
                textBox.setText(dialogue[currentDialogue]);
                Debug.WriteLine("NEXT");
            }
            prevDialogue = currentDialogue;
        }

        // Draws the tutorial box to the screen
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 boxPos = new Vector2(Camera.ScreenWidth / 2 - texture.Width / 2, texture.Height / 2 - texture.Height / 2 + 25);
            if (drawBox)
            {
                textBox.draw(spriteBatch);
            }
        }

        // Ensures the tutorial draws correctly when the screen is resized
        public void Resize()
        {
            textBox.Resize();
        }

        // Stores the tutorial in an array of strings that can be easily accessed.
        public void genDialogue()
        {
            dialogue = new string[22];
            dialogue[0] = "Welcome to AlmostSpace, a simplified orbital mechanics simulation ";
            dialogue[0] += "in 2D! This tutorial will walk you through launching your rocket, and getting to orbit.\n\n";
            dialogue[0] += "In order to move through this tutorial, press tab to advance to the next section.";

            dialogue[1] = "The little white triangle below this box with an arrow in it is you! "
                         + "As you can see, you are currently on our very own home planet "
                         + "earth. The planet you are currently on or orbiting can be seen in "
                         + "the top left corner of your screen, along with a bunch of other "
                         + "info we'll cover later. ";

            dialogue[2] = "Something you might notice, however, is that your height and velocity "
                         + "are currently both zero, indicating that you are sitting on the "
                         + "ground. That is super boring!! We should really fix that. But first, "
                         + "we have just a few more things to cover.";

            dialogue[3] = "The blue bar in the bottom left of your screen is your throttle "
                         + "indicator. Currently, your throttle is at 100%, but you will surely "
                         + "want to change that at times. To do this, you can left click and "
                         + "drag on the bar, or use " + Keybinds.increaseThrottle + " and " + Keybinds.decreaseThrottle + ". Try it now!";

            dialogue[4] = dialogue[3] + "\n\nYou did it! You can also use " + Keybinds.fullThrottle + " to " +
                "instantly go to full throttle, and " + Keybinds.cutThrottle + " to cut the throttle to 0%. Press tab to advance.";

            dialogue[5] = "You may wonder why your rocket is not doing anything when the "
                         + "throttle is above 0%. This is because your engine is actually off right now. "
                         + "To fix that, just hit the " + Keybinds.toggleEngine + " key. Why don't you set your throttle to full, "
                         + "start that engine, and get going! This box will disappear for a bit, "
                         + "but I'll catch you when you reach 1km in altitude.";

            dialogue[6] = "You reached 1km! Congrats! Don't worry, I've paused the game for "
                         + "you. So far, you've done an excellent job going straight up. "
                         + "Unfortunately, going straight up is not, in fact, how to reach "
                         + "orbit.";

            dialogue[7] = "There's a common misconception that orbiting objects are not "
                          + "subject to earth's gravity, but this is not true. Objects in orbit "
                          + "are constantly falling towards earth. The difference is that they "
                          + "have enough horizontal velocity that they miss the ground. Thus, "
                          + "what we need is more horizontal velocity.";

            dialogue[8] = "In order to get this horizontal velocity, you'll need to fly at a bit of "
                          + "an angle. You can press " + Keybinds.rotateLeft + " to rotate to the left, and " + Keybinds.rotateRight + " to rotate "
                          + "to the right. When you're ready, you should rotate to an angle just a "
                          + "little above parallel to the horizon, and keep going until I pop back up. Press " + Keybinds.pause + " to resume the game. "
                          + "This is also how you pause and resume the game outside of this tutorial.";

            dialogue[9] = "Yayy, you're doing great! I'm just checking in again because your "
                          + "apoapsis is getting a little high. Your apoapsis is the highest "
                          + "point in your orbit, and is indicated by the blue box with 'AP' "
                          + "written on it. If you look at the text on the left of your screen, "
                          + "you'll see that this height is now around 400km.";

            dialogue[10] = "400km is high enough for us right now, so when we resume the game "
                          + "again, I'd hit " + Keybinds.toggleEngine + " again to toggle your engine off. "
                          + "Then, just wait until your height reads around 375km and I'll tell you "
                          + "what to do next! Remember, hit " + Keybinds.pause + " to resume.";

            dialogue[11] = "Oh man, this is going to take a while. Luckily, we have the luxury "
                          + "of speeding up time! To increase your time warp, just hit the "
                          + Keybinds.increaseTimeWarp + " key. To decrease it , you can press " + Keybinds.decreaseTimeWarp + ", "
                          + "and to cancel it completely, press " + Keybinds.cancelTimeWarp + ". Increasing and "
                          + "decreasing your time warp level will cycle through multiplying "
                          + "time by increasingly large numbers. You can see how much faster "
                          + "time is running in the top right of the screen. Try resuming the "
                          + "game again and warping yourself up to 375km, eliminating that wait time!";

            dialogue[12] = "Oops! You went a bit too far. It's okay, I've stopped you and you'll still be just fine.";

            dialogue[13] = "Now that we're nice and close to our apoapsis, we need to increase our horizontal speed even more in "
                          + "order to reach orbit. Resume the game again and point the rocket such that it is parallel to the earth's surface. Then, turn "
                          + "on the engine and wait for the magic to happen!";

            dialogue[14] = "You made it to orbit! You're so awesome. There's a whole solar system out there to explore, and "
                          + "you're well on your way to being able to do so on your own. But there's one small issue: you can't "
                          + "actually see any of it. Let's fix that by going over some camera controls.";

            dialogue[15] = "First, in order to pan the camera, right click and drag anywhere on the screen. The same can also "
                          + "be accomplished using the " + Keybinds.cameraLeft + ", " + Keybinds.cameraRight + ", " + Keybinds.cameraUp
                          + ", and " + Keybinds.cameraDown + " keys. In order to re-center the camera on an object, simply left "
                          + "click it. To zoom, use the scroll wheel. Now try moving the camera around a bit.";

            dialogue[16] = dialogue[15] + "\n\nEpic camera work! Press tab when you're ready as usual.";

            dialogue[17] = "Before I leave you alone, there's a few other things I'd like to explain. First, you periapsis, marked by " +
                "the orange 'PE' indicator, represents the lowest point in your orbit. It is often most efficient to use your engines at this point.";

            dialogue[18] = "Second, if you wish to raise your orbit, the most efficient way to do so is to point prograde along your orbit, " +
                "indicated by the yellow symbol with a dot in the middle of it on your navball in the bottom right of the screen. Your current " +
                "angle is shown by the black arrow at the top of the instrument. Try pointing prograde and raising your apoapsis to 800km.";

            dialogue[19] = "Excellent! Now let's circularize your orbit. Time warp up to your new apoapsis, again marked with 'AP', and make another " +
                "prograde burn until your periapsis is 800km as well.";

            dialogue[20] = "Epic! You're now ready to begin your adventures. Farewell, my friend. I'd recommend starting a new file, but if you want to come " +
                "back to this one, It's called 'tutorial' in your save files. Hit Escape to save the game and return to the main menu.";

            dialogue[21] = "WHOOPS something broke if you're seeing this lol";
        }
    }
}
