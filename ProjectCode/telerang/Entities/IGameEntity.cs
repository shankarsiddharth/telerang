using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.VectorDraw;

namespace telerang.Entities
{
    public interface IGameEntity : ICollisionActor
    {
        int DrawOrder { get; }

        Vector2 Position { get; }

        Texture2D SpriteTexture { get; }

        CollisionComponent CollisionComponentSimple { get; }

        void Update(GameTime gameTime);

        void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        void DrawPrimitives(PrimitiveDrawing primitiveDrawing, GameTime gameTime);
    }
}