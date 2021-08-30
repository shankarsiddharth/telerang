using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using telerang.Entities;

namespace telerang
{
    internal class Boomerang : IGameEntity
    {
        public event EventHandler<TeleRangEventArgs> BoomerangReleased;

        public Vector2 Position { get; set; }

        private Vector2 _startPosition;
        private Texture2D _texture2D;

        private Ninja _ninja;
        private float _maximumDistance;

        public Boomerang(Texture2D spriteSheet, Vector2 position, Ninja ninja, float maximumDistance)
        {
            Position = position;
            _startPosition = position;

            _texture2D = spriteSheet;
            _ninja = ninja;
            _maximumDistance = maximumDistance;
        }

        public int DrawOrder { get; set; }

        public void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            Vector2 ninjaPosition = _ninja.Position;
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

            switch (_ninja.State)
            {
                case NinjaState.Idle:
                    { }
                    break;
                case NinjaState.Aiming:
                    {
                        if (mousePosition.Y <= ninjaPosition.Y)
                        {
                            mousePosition.Y = ninjaPosition.Y;
                        }

                        float maximumAllowedDistance = (ninjaPosition.Y + _maximumDistance);
                        if (mousePosition.Y >= maximumAllowedDistance)
                        {
                            mousePosition.Y = maximumAllowedDistance;
                        }

                        Position = mousePosition;

                        if (mouseState.LeftButton == ButtonState.Pressed)
                        {
                            _ninja.ChangeState(NinjaState.Teleporting);
                            _ninja.Position = mousePosition;
                            //TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                            //teleRangEventArgs.position = Position;
                            //BoomerangReleased?.Invoke(this, teleRangEventArgs);
                        }
                    }
                    break;
                case NinjaState.Teleporting:
                    {
                        _ninja.ChangeState(NinjaState.Teleported);
                    }
                    break;
                case NinjaState.Teleported:
                    {
                        _ninja.ChangeState(NinjaState.Aiming);
                    }
                    break;
            }
            

            
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_texture2D, Position, Color.White);
        }
    }
}