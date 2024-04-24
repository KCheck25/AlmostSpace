using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlmostSpace.Things
{
    public interface Screen
    {
        public void Update(GameTime gameTime);
        public void Draw(GameTime gameTime);
        public void LoadContent();
    }
}
