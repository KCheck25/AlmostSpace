using Microsoft.VisualBasic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

namespace AlmostSpace.Things
{

    internal class Orbit
    {

        SimClock clock;

        Vector2D objectPosition;
        Vector2D objectVelocity;

        Planet planetOrbiting;

        Texture2D apTexture;
        Texture2D peTexture;

        String name;
        String type;

        double radius;           // distance from body (m)
        public double planetAngle;      // angle from body to current position (rad)
        double mu;               // gravitation parameter for current body
        double vMagnitude;       // magnitude of object's velocity (m/s)
        double semiMajorAxis;    // semi major axis of orbit (m)
        double period;           // period of orbit (s)
        double aMomentum;        // angular momentum (rad/s^2)
        double rApoapsis;        // apoapsis radius (m)
        double rPeriapsis;       // periapsis radius (m)
        Vector2D eV;             // eccentricity vector
        double e;                // eccentricity
        public double argP;             // argument of periapsis (rad)
        double semiMinorAxis;    // semi minor axis (m)
        double mAnomaly;

        double m0;

        double timeSinceStoppedPhysics;

        PositionColorD[] path;

        BasicEffect basicEffect;
        GraphicsDevice graphicsDevice;

        bool wasPhysics = true;

        bool stationaryObject;
        bool landed;

        public Orbit(String name, String type, Vector2D position)
        {
            this.objectPosition = position;
            stationaryObject = true;
            this.name = name;
            this.type = type;
        }

        public Orbit(String name, String type, Planet planetOrbiting, Vector2D objectPosition, Vector2D objectVelocity, SimClock clock, GraphicsDevice graphicsDevice) { 
            this.planetOrbiting = planetOrbiting;
            this.objectPosition = objectPosition;
            this.objectVelocity = objectVelocity;
            this.clock = clock;
            this.graphicsDevice = graphicsDevice;
            this.name = name;
            this.type = type;

            mu = planetOrbiting.getMass() * OrbitMath.UNIVERSAL_G;

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
            (0, graphicsDevice.Viewport.Width,     // left, right
            graphicsDevice.Viewport.Height, 0,    // bottom, top
            0, 1);

            stationaryObject = false;

        }

        public Orbit(String name, String type, Texture2D apTexture, Texture2D peTexture, Planet planetOrbiting, Vector2D objectPosition, Vector2D objectVelocity, SimClock clock, GraphicsDevice graphicsDevice)
        {
            this.planetOrbiting = planetOrbiting;
            this.objectPosition = objectPosition;
            this.objectVelocity = objectVelocity;
            this.clock = clock;
            this.graphicsDevice = graphicsDevice;
            this.apTexture = apTexture;
            this.peTexture = peTexture;
            this.name = name;
            this.type = type;

            mu = planetOrbiting.getMass() * OrbitMath.UNIVERSAL_G;

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
            (0, graphicsDevice.Viewport.Width,     // left, right
            graphicsDevice.Viewport.Height, 0,    // bottom, top
            0, 1);

            stationaryObject = false;

        }

        public Orbit(String data, List<Planet> planets, SimClock clock, GraphicsDevice graphicsDevice)
        {
            this.clock = clock;
            this.graphicsDevice = graphicsDevice;

            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter
            (0, graphicsDevice.Viewport.Width,     // left, right
            graphicsDevice.Viewport.Height, 0,    // bottom, top
            0, 1);

            String[] lines = data.Split("\n");
            foreach (String line in lines)
            {
                String[] components = line.Split(": ");
                if (components.Length == 2)
                {
                    switch (components[0])
                    {
                        case "Type":
                            type = components[1];
                            break;
                        case "ID":
                            name = components[1];
                            break;
                        case "Position":
                            objectPosition.X = double.Parse(components[1].Split(",")[0]);
                            objectPosition.Y = double.Parse(components[1].Split(",")[1]);
                            break;
                        case "Stationary":
                            stationaryObject = bool.Parse(components[1]);
                            break;
                        case "Velocity":
                            objectVelocity.X = double.Parse(components[1].Split(",")[0]);
                            objectVelocity.Y = double.Parse(components[1].Split(",")[1]);
                            break;
                        case "Initial Mean Anomaly":
                            m0 = double.Parse(components[1]);
                            break;
                        case "Time Since Physics Last Stopped":
                            timeSinceStoppedPhysics = double.Parse(components[1]);
                            break;
                        case "Was Physics":
                            wasPhysics = bool.Parse(components[1]);
                            break;
                        case "Orbiting Planet":
                            Debug.Write(components[1]);
                            foreach (Planet planet in planets)
                            {
                                if (planet.getName().Equals(components[1]))
                                {
                                    planetOrbiting = planet;
                                }
                            }
                            break;
                        case "Landed":
                            landed = bool.Parse(components[1]);
                            break;
                    }

                }

            }

            radius = objectPosition.Length();
            planetAngle = Math.Atan2(-objectPosition.Y, objectPosition.X);
            calculateParameters();

        }

        public Orbit(String data, List<Planet> planets, SimClock clock, GraphicsDevice graphicsDevice, Texture2D apTexture, Texture2D peTexture) : this(data, planets, clock, graphicsDevice)
        {
            this.apTexture = apTexture;
            this.peTexture = peTexture;
        }

        // Setters and getters:

        // Changes the position and velocity vectors of the orbit to be relative to the given planet
        public void setPlanetOrbiting(Planet planet)
        {
            if (stationaryObject)
            {
                return;
            }

            //clock.setTimeFactor(0);

            Debug.WriteLine("BEFORE:    Current Velocity: " + getVelocity() + " Current Position: " + getPosition());
            Debug.WriteLine("RELATIVE:  Current Velocity: " + objectVelocity + " Current Position: " + objectPosition);

            objectPosition = getPosition() - planet.getPosition();
            objectVelocity = getVelocity() - planet.getVelocity();

            objectPosition = new Vector2D(148054327227.83652, 23494216235.473633);
            objectVelocity = new Vector2D(-4846.474948306446, 28740.23504994603);

            planetOrbiting = planet;

            Debug.WriteLine("AFTER:     Current Velocity: " + getVelocity() + " Current Position: " + getPosition());
            Debug.WriteLine("RELATIVE:  Current Velocity: " + objectVelocity + " Current Position: " + objectPosition);

            radius = objectPosition.Length();
            Debug.WriteLine(radius);
            planetAngle = Math.Atan2(-objectPosition.Y, objectPosition.X);
            calculateParameters();
            transitionToNoPhysics();
        }

        public void setLanded(bool landed)
        {
            this.landed = landed;
            objectVelocity = new Vector2D();
        }

        // Gets the radius of the rocket at a given angle
        double getRadiusAtAngle(double a, double e, double theta)
        {
            return a * (1 - e * e) / (1 + e * Math.Cos(theta));
        }

        public Vector2D getPosition()
        {
            return stationaryObject ? objectPosition : objectPosition + planetOrbiting.getPosition();
        }

        public Vector2D getRelativePosition()
        {
            return objectPosition;
        }

        public Vector2D getVelocity()
        {
            return stationaryObject ? objectVelocity : objectVelocity + planetOrbiting.getVelocity();
        }

        public Vector2D getRelativeVelocity()
        {
            return objectVelocity;
        }

        public double getSemiMajorAxis()
        {
            return semiMajorAxis;
        }

        // Returns the rockets height above the planets surface in meters
        public double getHeight()
        {
            return radius - planetOrbiting.getRadius();
        }

        public double getOrbitRadius()
        {
            return radius;
        }

        // Returns the highest point above the surface tha the rocket will reach in meters
        public double getApoapsisHeight()
        {
            return rApoapsis - planetOrbiting.getRadius() > 0 ? rApoapsis - planetOrbiting.getRadius() : double.NaN;
        }

        // Returns the lowest point above the surface tha the rocket will reach in meters
        public double getPeriapsisHeight()
        {
            return rPeriapsis - planetOrbiting.getRadius();
        }

        // Returns the magnitude of the rocket's velocity
        public double getVelocityMagnitude()
        {
            return vMagnitude;
        }

        // Returns the period of the rockets current orbit in seconds
        public double getPeriod()
        {
            return period;
        }

        public SimClock getClock()
        {
            return clock;
        }

        public Planet getPlanetOrbiting()
        {
            return planetOrbiting;
        }

        public String getName()
        {
            return name;
        }

        public String getSaveData()
        {
            String output = "Type: " + type + "\n";
            output += "ID: " + name + "\n";
            output += "Position: " + objectPosition.X + "," + objectPosition.Y + "\n";
            output += "Stationary: " + stationaryObject + "\n";
            if (stationaryObject)
            {
                return output;
            }
            output += "Orbiting Planet: " + (planetOrbiting != null ? planetOrbiting.getName() : "none") + "\n";
            output += "Velocity: " + objectVelocity.X + "," + objectVelocity.Y + "\n";
            output += "Initial Mean Anomaly: " + m0 + "\n";
            output += "Time Since Physics Last Stopped: " + timeSinceStoppedPhysics + "\n";
            output += "Was Physics: " + wasPhysics + "\n";
            output += "Landed: " + landed + "\n";

            return output;
        }

        public bool getLanded()
        {
            return landed;
        }

        // Takes into account engine thrust
        // cannot run under time warp - will be inaccurate
        public void Update(Vector2D objectAcceleration)
        {
            if (stationaryObject)
            {
                return;
            }
            wasPhysics = true;

            double massPlanet = planetOrbiting.getMass();
            double xDist = objectPosition.X;
            double yDist = -objectPosition.Y;
            double distToCenter = Math.Pow(Math.Pow(xDist, 2) + Math.Pow(yDist, 2), 0.5);
            radius = distToCenter;

            Vector2D gravityAcceleration = new Vector2D();

            double totalGAccel = (OrbitMath.UNIVERSAL_G * massPlanet) / (distToCenter * distToCenter);
            planetAngle = Math.Atan2(yDist, xDist);

            double xAccel = -totalGAccel * Math.Cos(planetAngle);
            double yAccel = totalGAccel * Math.Sin(planetAngle);

            gravityAcceleration.X = xAccel;
            gravityAcceleration.Y = yAccel;

            objectVelocity += (gravityAcceleration + objectAcceleration) * clock.getFrameTime();
            objectPosition += objectVelocity * clock.getFrameTime();

            calculateParameters();
        }

        // Recalculates the rocket's position in space and velocity based on its current orbital parameters
        // Allows time to be sped up without losing precision
        public void Update()
        {
            if (stationaryObject)
            {
                return;
            }
            if (landed)
            {
                return;
            }
            if (wasPhysics)
            {
                transitionToNoPhysics();
                wasPhysics = false;
            }
            timeSinceStoppedPhysics += clock.getFrameTime();
            double timePassed = timeSinceStoppedPhysics;

            double tAnomaly = OrbitMath.getTrueAnomaly(mu, e, semiMajorAxis, timePassed, aMomentum, m0);
            if (tAnomaly == -10000)
            {
                return;
            }

            double distAtAnomaly = semiMajorAxis * (1 - e * e) / (1 + e * Math.Cos(tAnomaly)); // distance from planet

            radius = distAtAnomaly;

            vMagnitude = Math.Sqrt(mu * (2 / distAtAnomaly - 1 / semiMajorAxis));

            // https://physics.stackexchange.com/questions/669946/how-to-calculate-the-direction-of-the-velocity-vector-for-a-body-that-moving-is
            double vY = -Math.Sin(tAnomaly) / Math.Sqrt(1 + e * e + 2 * e * Math.Cos(tAnomaly)) * vMagnitude * -Math.Sign(aMomentum);
            double vX = (e + Math.Cos(tAnomaly)) / Math.Sqrt(1 + e * e + 2 * e * Math.Cos(tAnomaly)) * vMagnitude * -Math.Sign(aMomentum);

            // https://matthew-brett.github.io/teaching/rotation_2d.html
            objectVelocity.X = (Math.Cos(-argP - MathHelper.PiOver2) * vX - Math.Sin(-argP - MathHelper.PiOver2) * vY);
            objectVelocity.Y = (Math.Sin(-argP - MathHelper.PiOver2) * vX + Math.Cos(-argP - MathHelper.PiOver2) * vY);

            objectPosition.X = distAtAnomaly * Math.Cos(tAnomaly + argP);
            objectPosition.Y = -distAtAnomaly * Math.Sin(tAnomaly + argP);

            planetAngle = Math.Atan2(-objectPosition.Y, objectPosition.X);
        }

        // This method runs when switching between physics mode and non physics mode
        // It's main purpose is to compute the rocket's mean anomaly at the time of making this switch
        void transitionToNoPhysics()
        {
            timeSinceStoppedPhysics = 0;
            double currentTrueAnomaly = planetAngle - argP;
            m0 = OrbitMath.getMeanAnomaly(e, currentTrueAnomaly);
            if (name.Equals("Zoomy"))
            {
                Debug.WriteLine(e);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Matrix transform, Vector2D origin)
        {
            if (stationaryObject || landed)
            {
                return;
            }
            generatePath(1000, origin);
            if (path != null)
            {
                basicEffect.CurrentTechnique.Passes[0].Apply();
                graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, PositionColorD.getVertexPositionColorArr(path, transform), 0, path.Length - 1);
            }
            if (apTexture != null && peTexture != null)
            {
                bool escapeTrajectory = (e > 1) ? true : rApoapsis > planetOrbiting.getSOI();
                // Only draw apoapsis if the rocket is not on an escape trajectory
                if (!escapeTrajectory)
                {
                    Vector2D apPos = new Vector2D(Math.Cos(argP + MathHelper.Pi) * rApoapsis, -Math.Sin(argP + MathHelper.Pi) * rApoapsis);
                    spriteBatch.Draw(apTexture, Vector2D.Transform(apPos + planetOrbiting.getPosition() - origin, transform).getVector2(), null, Color.White, 0f, new Vector2(apTexture.Width / 2, 0), 0.5f, SpriteEffects.None, 0f);
                }

                // Only draw periapsis if it is above ground and the rocket is in a circular orbit
                // or on an escape trajectory but before reaching the periapsis
                double currentAngle = planetAngle - argP;
                bool clockwise = aMomentum > 0;
                bool beforePlanet = (OrbitMath.WrapAngle(currentAngle) > 0 && clockwise) || (OrbitMath.WrapAngle(currentAngle) < 0 && !clockwise);
                if ((getPeriapsisHeight() > 0 && !escapeTrajectory) || (getPeriapsisHeight() > 0 && escapeTrajectory && beforePlanet))
                {
                    Vector2D pePos = new Vector2D(Math.Cos(argP) * rPeriapsis, -Math.Sin(argP) * rPeriapsis);
                    spriteBatch.Draw(peTexture, Vector2D.Transform(pePos + planetOrbiting.getPosition() - origin, transform).getVector2(), null, Color.White, 0f, new Vector2(peTexture.Width / 2, 0), 0.5f, SpriteEffects.None, 0f);
                }
            }
        }

        // Recalculates the rocket's orbital parameters from its velocity and position vectors
        public void calculateParameters()
        {
            if (stationaryObject)
            {
                return;
            }
            // mu is the standard gravitational parameter of the planet that's being orbited
            mu = OrbitMath.UNIVERSAL_G * planetOrbiting.getMass();
            double velocityMagnitude = objectVelocity.Length();
            vMagnitude = velocityMagnitude;

            semiMajorAxis = OrbitMath.getSemiMajorAxis(velocityMagnitude, mu, radius);

            period = OrbitMath.calcPeriod(semiMajorAxis, mu);

            // Equations taken from here: https://space.stackexchange.com/questions/2562/2d-orbital-path-from-state-vectors
            aMomentum = objectPosition.X * objectVelocity.Y - objectPosition.Y * objectVelocity.X; // angular momentum

            // eccentricity vector
            eV = OrbitMath.calcEccentricity(objectPosition, objectVelocity, aMomentum, mu);
            e = eV.Length();

            // argument of periapsis - angle from planet to periapsis
            argP = -Math.Atan2(eV.Y, eV.X);

            // Apoapsis and periapsis radiuses
            rApoapsis = semiMajorAxis * (1 + e);
            rPeriapsis = semiMajorAxis * (1 - e);

            semiMinorAxis = semiMajorAxis * Math.Sqrt(1 - e * e);

            Debug.WriteLine("Name: " + name);
            Debug.WriteLine("mu: " + mu);
            Debug.WriteLine("vMagnitude: " + vMagnitude); 
            Debug.WriteLine("semiMajorAxis: " + semiMajorAxis); 
            Debug.WriteLine("period: " + period);
            Debug.WriteLine("aMomentum: " + aMomentum);
            Debug.WriteLine("eV: " + eV);
            Debug.WriteLine("e: " + e);
            Debug.WriteLine("argP: " + argP);
            Debug.WriteLine("rApoapsis: " + rApoapsis);
            Debug.WriteLine("rPeriapsis: " + rPeriapsis); 
            Debug.WriteLine("semiMinorAxis: " + semiMinorAxis);

            //Debug.WriteLine(mu + " " + semiMajorAxis + " " + radius + " " + e + " " + vMagnitude + " " + objectPosition);
        }

        // Generates a list of OrbitSprite objects arranged in the rocket's trajectory
        void generatePath(int numPoints, Vector2D origin)
        {

            double rMax = planetOrbiting.getSOI();
            double endAngle = MathHelper.Pi;
            double startAngle = -MathHelper.Pi;

            bool hyperbolic = e > 1;
            bool escapeTrajectory = hyperbolic ? true : rApoapsis > rMax;
            if (rMax <= 0)
            {
                escapeTrajectory = false;
            }
            bool hitsPlanet = getPeriapsisHeight() < 0;
            bool clockwise = aMomentum > 0;
            bool fullOrbit = false;

            // Figure out what portion of trajectory should be rendered

            if (escapeTrajectory)
            {
                startAngle = planetAngle - argP;

                if (hitsPlanet)
                {
                    bool beforePlanet = (OrbitMath.WrapAngle(startAngle) > 0 && clockwise) || (OrbitMath.WrapAngle(startAngle) < 0 && !clockwise);
                    if (!beforePlanet)
                    {
                        endAngle = Math.Acos((semiMajorAxis * (1 - e * e) - rMax) / (e * rMax));
                        endAngle *= clockwise ? -1 : 1;
                    }
                    else
                    {
                        endAngle = Math.Acos((semiMajorAxis * (1 - e * e) - planetOrbiting.getRadius()) / (e * planetOrbiting.getRadius()));
                        endAngle *= clockwise ? 1 : -1;
                    }
                }
                else
                {
                    endAngle = Math.Acos((semiMajorAxis * (1 - e * e) - rMax) / (e * rMax));
                    endAngle *= clockwise ? -1 : 1;
                }
            }
            else if (hitsPlanet)
            {
                startAngle = Math.Acos((semiMajorAxis * (1 - e * e) - planetOrbiting.getRadius()) / (e * planetOrbiting.getRadius()));
                startAngle *= clockwise ? -1 : 1;
                endAngle = MathHelper.TwoPi -startAngle;
            } 
            else
            {
                fullOrbit = true;
            }

            path = new PositionColorD[fullOrbit ? numPoints + 1 : numPoints];

            // Wrap angles and make sure they are rendered in the right direction

            if (!fullOrbit)
            {
                startAngle = OrbitMath.WrapAngle(startAngle);
                endAngle = OrbitMath.WrapAngle(endAngle);

                if (clockwise)
                {
                    if (endAngle > startAngle)
                    {
                        endAngle -= MathHelper.TwoPi;
                    }
                }
                else
                {
                    if (endAngle < startAngle)
                    {
                        endAngle += MathHelper.TwoPi;
                    }
                }
            }

            // Calculate points
            double step = (endAngle - startAngle) / numPoints;
            int i = 0;
            double currentTheta = startAngle;
            while (i < numPoints)
            {
                double r = getRadiusAtAngle(semiMajorAxis, e, currentTheta);
                PositionColorD point = new PositionColorD(new Vector2D(r * Math.Cos(currentTheta + argP) + planetOrbiting.getPosition().X, -r * Math.Sin(currentTheta + argP) + planetOrbiting.getPosition().Y) - origin, Color.White);
                if (i < numPoints)
                {
                    path[i] = point;
                }
                if (i == 0 && fullOrbit)
                {
                    path[path.Length - 1] = point;
                }
                i++;
                currentTheta += step;
            }

        }
    }
}
