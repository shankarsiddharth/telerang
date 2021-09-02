using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.VectorDraw;

namespace telerang.Entities
{
    public class Ninja : IGameEntity
    {
        public int DrawOrder { get; set; }

        public Vector2 Position { get; set; }

        public Texture2D SpriteTexture { get; set; }

        public IShapeF Bounds { get; private set; }

        public NinjaState State { get; private set; }

        public bool IsAlive { get; private set; }

        public CollisionComponent CollisionComponentSimple { get; set; }

        public Vector2 targetPosition;

        private Vector2 _startPosition;

        private Vector2 _spritePosition;

        public Ninja(Texture2D spriteTexture, Vector2 initialPosition)
        {
            SpriteTexture = spriteTexture;
            Position = initialPosition;
            _startPosition = Position;
            State = NinjaState.Idle;
            IsAlive = true;

            _spritePosition = new Vector2(Position.X - (SpriteTexture.Width / 2), Position.Y - (SpriteTexture.Height));
            Bounds = new RectangleF(_spritePosition, new Size2(SpriteTexture.Width, SpriteTexture.Height));
        }

        public void Initialize()
        {
            State = NinjaState.Idle;
            IsAlive = true;
            Position = new Vector2(_startPosition.X, _startPosition.Y);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (IsAlive)
            {
                switch (State)
                {
                    case NinjaState.Idle:
                        break;

                    case NinjaState.Teleporting:
                        break;

                    case NinjaState.Aiming:
                        break;
                }
            }
            else
            {
            }

            _spritePosition = new Vector2(Position.X - (SpriteTexture.Width / 2), Position.Y - (SpriteTexture.Height));
            spriteBatch.Draw(SpriteTexture, _spritePosition, Color.White);

            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red);
        }

        public void Update(GameTime gameTime)
        {
            switch (State)
            {
                case NinjaState.Idle:
                    break;

                case NinjaState.Teleporting:
                    break;

                case NinjaState.Aiming:
                    break;
            }
            _spritePosition = new Vector2(Position.X - (SpriteTexture.Width / 2), Position.Y - (SpriteTexture.Height));
            Bounds.Position = _spritePosition;
        }

        public void ChangeState(NinjaState newNinjaState)
        {
            switch (State)
            {
                case NinjaState.Idle:
                    {
                    }
                    break;

                case NinjaState.Aiming:
                    {
                    }
                    break;

                case NinjaState.Teleporting:
                    {
                    }
                    break;
            }
            State = newNinjaState;
        }

        // event handler
        public void OnBoomerangReleased(object sender, TeleRangEventArgs e)
        {
            ChangeState(NinjaState.Teleporting);
            Position = e.position;
            ChangeState(NinjaState.Aiming);
        }

        public void DrawPrimitives(PrimitiveDrawing primitiveDrawing, GameTime gameTime)
        {
            //throw new System.NotImplementedException();
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {         
            //throw new System.NotImplementedException();
        }
    }
}