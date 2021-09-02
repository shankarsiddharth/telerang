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

        public Ninja()
        {            
            _startPosition = Position;
            State = NinjaState.Idle;
            IsAlive = true; 
            Bounds = new RectangleF(Position, new Size2(32f, 32f));
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

            spriteBatch.Draw(SpriteTexture, Position, Color.White);
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
            throw new System.NotImplementedException();
        }
    }
}