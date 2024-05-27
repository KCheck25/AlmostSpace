﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch);
        public void LoadContent();
        public int NextScreen();
        public void Start();
        public void Resize();
    }
}
