﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Iron_Max
{
    class GameObject
    {
        public Texture2D sprite;
        public Vector2 position;
        public float rotation;
        public Vector2 center;
        public Vector2 velocity;
        public bool alive;

        public Rectangle rectangle
        {
            get
            {
                int left = (int)position.X;
                int width = sprite.Width;
                int top = (int)position.Y;
                int height = sprite.Height;
                return new Rectangle(left, top, width, height);
            }
        }

        public GameObject(Texture2D loadedTexture)
        {
            rotation = 0.0f;
            position = Vector2.Zero;
            sprite = loadedTexture;
            center = new Vector2(sprite.Width / 2, sprite.Height / 2);
            velocity = Vector2.Zero;
            alive = false;
        }

    }
}
