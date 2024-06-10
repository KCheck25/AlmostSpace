using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Input;
using AlmostSpace.Core.Common;

namespace AlmostSpace.Things
{
    // Represents a planet in space
    internal class Planet : Orbit
    {
        Texture2D texture;
        double mass;
        double planetRadius;
        double soi; // sphere of influence

        Texture2D soiTexture;

        List<Planet> children = new List<Planet>();

        // Creates a new planet using the given texture, mass, and position
        public Planet(String name, Texture2D texture, double mass, Vector2D position, double radius) : base(name, "Planet", position)
        {
            this.texture = texture;
            this.mass = mass;
            this.planetRadius = radius;
        }

        // Creates a new planet object using the given name, textures, mass, position, velocity, radius, the body it orbits,
        // the clock it should use, and the graphics device it should use
        public Planet(String name, Texture2D texture, Texture2D soiTexture, float mass, Vector2D position, Vector2D velocity, double radius, Planet orbiting, SimClock clock, GraphicsDevice graphicsDevice) : base(name, "Planet", orbiting, position, velocity, clock, graphicsDevice)
        {
            this.texture = texture;
            this.mass = mass;
            this.planetRadius = radius;
            this.soiTexture = soiTexture;

            base.Update(new Vector2D());

            soi = getSemiMajorAxis() * Math.Pow(mass / orbiting.getMass(), 0.4);
            Debug.WriteLine(soi);

            orbiting.addChild(this);
        }

        // Specifies other planets that are orbiting this planet
        public void addChild(Planet child)
        {
            children.Add(child);
        }

        // Returns a list of other planets orbiting this planet
        public List<Planet> getChildren()
        {
            return children;
        }

        // Returns the mass of this planet
        public double getMass()
        {
            return mass;
        }

        // Returns the radius of the sphere of influence of this planet
        public double getSOI()
        {
            return soi;
        }

        // Returns the radius of the planet's surface
        public double getRadius()
        {
            return planetRadius;
        }

        // Checks whether this planet has been clicked, and returns true if so
        public bool clicked(Matrix transform, Vector2D origin)
        {
            var mState = Mouse.GetState();
            if (mState.LeftButton == ButtonState.Pressed)
            {
                Vector2 mousePos = new Vector2(mState.Position.X, mState.Position.Y);
                Vector2 onScreenPos = (getPosition() - origin).Transform(transform).getVector2();
                Vector2 atRadiusPos = (getPosition() + new Vector2D(planetRadius, 0) - origin).Transform(transform).getVector2();
                float clickRadius = (atRadiusPos - onScreenPos).Length();
                if ((mousePos - onScreenPos).Length() < clickRadius)
                {
                    return true;
                }
            }
            return false;
        }

        // Draws this planet to the screen using the given SpriteBatch object
        public new void Draw(SpriteBatch spriteBatch, Matrix transform, Vector2D origin)
        {
            if (!getVelocity().Equals(new Vector2D()))
            {
                spriteBatch.Draw(soiTexture, (getPosition() - origin).getVector2(), null, Color.White, 0f, new Vector2(soiTexture.Width / 2, soiTexture.Height / 2), (float)(2 * soi / soiTexture.Width), SpriteEffects.None, 0f);
            }
            base.Draw(spriteBatch, transform, origin);
            spriteBatch.Draw(texture, (getPosition() - origin).getVector2(), null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), (float)(2 * planetRadius / texture.Width), SpriteEffects.None, 0f);
        }

        // Draws this planet to the screen using the given SpriteBatch object
        public void DrawSurface(SpriteBatch spriteBatch, Matrix transform, Vector2D origin)
        {
            spriteBatch.Draw(texture, new Vector2(0, -(float)(getPosition() - origin).Transform(transform).Length()), null, Color.White, 0f, new Vector2(texture.Width / 2, 0), Vector2.One, SpriteEffects.None, 0f);
        }

        // Returns a string representing this planet's save data to be written to a save file
        public new string getSaveData()
        {
            String output = base.getSaveData();
            output += "Texture: " + texture.Name + "\n";
            output += "Mass: " + mass + "\n";
            output += "Planet Radius: " + planetRadius + "\n\n";
            foreach (Planet planet in children)
            {
                output += planet.getSaveData();
            }
            return output;
        }

        // Creates a new planet from the given save file data
        public Planet(string data, List<Planet> planets, SimClock clock, List<Texture2D> textures, Texture2D soiTexture, GraphicsDevice graphicsDevice) : base(data, planets, clock, graphicsDevice)
        {
            this.soiTexture = soiTexture;
            string[] lines = data.Split("\n");
            foreach (string line in lines)
            {
                string[] components = line.Split(": ");
                if (components.Length == 2)
                {
                    switch (components[0])
                    {
                        case "Mass":
                            mass = double.Parse(components[1]);
                            break;
                        case "Planet Radius":
                            planetRadius = double.Parse(components[1]);
                            break;
                        case "Orbiting Planet":
                            Debug.Write(components[1]);
                            foreach (Planet planet in planets)
                            {
                                if (planet.getName().Equals(components[1]))
                                {
                                    planet.addChild(this);
                                }
                            }
                            break;
                        case "Texture":
                            foreach (Texture2D texture in textures)
                            {
                                if (texture.Name.Equals(components[1]))
                                {
                                    this.texture = texture;
                                }
                            }
                            break;
                    }

                }

            }
            if (getPlanetOrbiting() != null)
            {
                soi = getSemiMajorAxis() * Math.Pow(mass / getPlanetOrbiting().getMass(), 0.4);
            }
        }

    }
}
