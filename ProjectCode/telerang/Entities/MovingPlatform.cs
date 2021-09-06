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

        public Texture2D SpriteTexture { get; set; }

        public IShapeF Bounds { get; private set; }

        public CollisionComponent CollisionComponentSimple { get; set; }

        public TiledMapObject MapObject { get; private set; }

        // public bool IsColliding { get; private set; }

        public string Direction { get; private set; }

        public float _WindowWidth { get; set; }

        public float Speed { get; set; }

        private Vector2 _startPosition { get; set; }

        public MovingPlatform(TiledMapObject mapObject, Texture2D spriteSheetTexture, float WindowWidth)
        {
            MapObject = mapObject;
            SpriteTexture = spriteSheetTexture;
            Position = mapObject.Position;
            _startPosition = Position;
            _WindowWidth = WindowWidth;

            Bounds = new RectangleF(Position, mapObject.Size);
            Direction = MapObject.Properties.GetValueOrDefault<string, string>("direction");
            if(Direction == null || Direction.Length == 0)
            {
                Direction = "left";
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(SpriteTexture, Position, Color.White);
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red);
        }

        public void DrawPrimitives(PrimitiveDrawing primitiveDrawing, GameTime gameTime)
        {
            
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            
        }

        public void Update(GameTime gameTime)
        {
            if(Direction.Equals("left"))
            {
                Vector2 endPoint = new Vector2(Position.X + MapObject.Size.Width, Position.Y + MapObject.Size.Height);
                //Position += new Vector2(-0.25f, 0);
                Position += new Vector2(-Speed, 0);
                if (endPoint.X < 0)
                {
                    //Position = new Vector2(_startPosition.X + _WindowWidth, Position.Y);
                    Position = new Vector2(_WindowWidth + MapObject.Size.Width, Position.Y);
                }
                Bounds.Position = Position;
            }
            else if(Direction.Equals("right"))
            {
                Vector2 endPoint = new Vector2(Position.X - MapObject.Size.Width, Position.Y + MapObject.Size.Height);
                //Position += new Vector2(-0.25f, 0);
                Position += new Vector2(Speed, 0);
                if (endPoint.X > _WindowWidth)
                {
                    Position = new Vector2( (0 - MapObject.Size.Width),  Position.Y);
                }
                Bounds.Position = Position;
            }
            
        }
    }
}
