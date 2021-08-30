using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace telerang.Entities
{
    public class Ninja : IGameEntity
    {
        public NinjaState State { get; private set; }

        public Vector2 Position { get; set; }

        public bool IsAlive { get; private set; }

        public int DrawOrder { get; set; }

        public Vector2 targetPosition;

        private Vector2 _startPosition;
        private Texture2D _texture2D;

        public Ninja(Texture2D spriteSheet, Vector2 position)
        {
            Position = position;
            _startPosition = Position;

            _texture2D = spriteSheet;

            State = NinjaState.Aiming;

            IsAlive = true;
        }

        public void Initialize()
        {
            State = NinjaState.Aiming;
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

            spriteBatch.Draw(_texture2D, Position, Color.White);
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
                    { }
                    break;
                case NinjaState.Aiming:
                    { }
                    break;
                case NinjaState.Teleporting:
                    { }
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
    }   
}