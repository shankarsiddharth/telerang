using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.VectorDraw;
using System;
using System.Collections.Generic;
using System.Text;

namespace telerang.Entities
{
    class MovingPlatform : IGameEntity
    {
        public int DrawOrder { get; set; }

        public Vector2 Position { get; private set; }

        public Texture2D SpriteTexture => throw new NotImplementedException();

        public IShapeF Bounds { get; private set; }

        public CollisionComponent CollisionComponentSimple { get; set; }

        public TiledMapObject MapObject { get; private set; }

        // public bool IsColliding { get; private set; }

        public string Direction { get; private set; }

        public MovingPlatform(TiledMapObject mapObject)
        {
            MapObject = mapObject;
            Position = mapObject.Position;
            Bounds = new RectangleF(Position, mapObject.Size);
            Direction = MapObject.Properties.GetValueOrDefault<string, string>("direction");
            if(Direction == null || Direction.Length == 0)
            {
                Direction = "right";
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red);
        }

        public void DrawPrimitives(PrimitiveDrawing primitiveDrawing, GameTime gameTime)
        {
            
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            
        }

        public void Update(GameTime gameTime)
        {
            
        }
    }
}
