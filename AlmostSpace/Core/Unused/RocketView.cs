using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    internal class RocketView
    {
        List<Planet> planets;
        Rocket rocket;
        Camera camera;

        public RocketView(List<Planet> planets, Rocket rocket)
        {
            this.rocket = rocket;
            this.planets = planets;
            camera = new Camera();
        }

        public void Update()
        {
            Planet current = rocket.getPlanetOrbiting();
            while (current != null)
            {
                current.Update();
                current = current.getPlanetOrbiting();
            }
            foreach (Planet planet in rocket.getPlanetOrbiting().getChildren())
            {
                planet.Update();
            }

            rocket.Update();
        }

        public void Draw()
        {
            //rocket.Draw();
        }
    }
}
