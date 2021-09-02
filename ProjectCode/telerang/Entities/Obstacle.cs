using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.VectorDraw;
using System;
using System.Collections.Generic;
using System.Text;

namespace telerang.Entities
{
    class Obstacle : IGameEntity
    {
        public int DrawOrder => throw new NotImplementedException();

        public Vector2 Position => throw new NotImplementedException();

        public Texture2D SpriteTexture => throw new NotImplementedException();

        public IShapeF Bounds => throw new NotImplementedException();

        public CollisionComponent CollisionComponentSimple { get; set; }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void DrawPrimitives(PrimitiveDrawing primitiveDrawing, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
