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
        public float MaxTime { get; set; }
        private Vector2 _startPosition;
        private Texture2D _texture2D;
        private Texture2D _cursor;
        private Ninja _ninja;
        private float _maximumDistance;
        private float _timer;
        private Vector2 _cursorPosition;

        public Boomerang(Texture2D spriteSheet,Texture2D cursor, Vector2 position, Ninja ninja, float maximumDistance)
        {
            Position = position;
            _startPosition = position;
            MaxTime = 500f;
            _texture2D = spriteSheet;
            _cursor = cursor;
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
                    {
                        if (mouseState.LeftButton == ButtonState.Pressed) {
                            _ninja.ChangeState(NinjaState.Aiming);
                        }
                    }
                    break;
                case NinjaState.Aiming:
                    {
                        if (mousePosition.Y <= ninjaPosition.Y)
                        {
                            mousePosition.Y = ninjaPosition.Y;
                        }

                        //float maximumAllowedDistance = (ninjaPosition.Y + _maximumDistance);

                        //if (mousePosition.Y >= maximumAllowedDistance)
                        //{
                        //    mousePosition.Y = maximumAllowedDistance;
                        //}

                        Vector2 difference = Vector2.Subtract(mousePosition, ninjaPosition);

                        if (difference.Length() >= _maximumDistance) {
                            difference.Normalize();
                            mousePosition = Vector2.Add(ninjaPosition, difference * _maximumDistance);
                        }

                        _cursorPosition = mousePosition;

                        if (mouseState.LeftButton == ButtonState.Released)
                        {
                            _ninja.targetPosition = mousePosition;
                            _startPosition = _ninja.Position;
                            _timer=0f;
                            _ninja.ChangeState(NinjaState.Teleporting); 
                            //_ninja.Position = mousePosition;
                            //TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                            //teleRangEventArgs.position = Position;
                            //BoomerangReleased?.Invoke(this, teleRangEventArgs);
                        }
                    }
                    break;
                case NinjaState.Teleporting:
                    {
                        if (_timer <= MaxTime)
                        {
                            Position = Vector2.LerpPrecise(_startPosition, _ninja.targetPosition, _timer / MaxTime);
                            if (mouseState.LeftButton == ButtonState.Pressed
                                &&_timer> 2f*(float)gameTime.ElapsedGameTime.TotalMilliseconds)
                            {
                                _ninja.ChangeState(NinjaState.Teleported);
                            }
                        }
                        else if (_timer >= 2 * MaxTime)
                        {
                            _ninja.Position = _startPosition;
                            Position = _startPosition;
                            _timer = 0f;
                            _ninja.ChangeState(NinjaState.Idle);
                        }
                        else {
                            Position = Vector2.LerpPrecise(_startPosition, _ninja.targetPosition, (2f - _timer / MaxTime));
                            if (mouseState.LeftButton == ButtonState.Pressed)
                            {
                                _ninja.ChangeState(NinjaState.Teleported);
                            }
                        }
                        _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                    break;
                case NinjaState.Teleported:
                    {
                        _ninja.Position = Position;
                        if (mouseState.LeftButton == ButtonState.Released)
                        {
                            _ninja.ChangeState(NinjaState.Idle);
                        }
                    }
                    break;
            }
            

            
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_cursor, Mouse.GetState().Position.ToVector2(), Color.White);
            if (_ninja.State == NinjaState.Aiming) {
                spriteBatch.Draw(_cursor, _cursorPosition, Color.White);
            }
            if (_ninja.State == NinjaState.Teleporting) {
                spriteBatch.Draw(_texture2D, Position, Color.White);
            }
        }
    }
}