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

        public Boomerang(Texture2D spriteSheet, Vector2 position, Ninja ninja)
        {
            Position = position;
            _startPosition = position;

            _texture2D = spriteSheet;
            _ninja = ninja;
        }

        public int DrawOrder { get; set; }

        public void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            Position = new Vector2(mouseState.X, mouseState.Y);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                TeleRangEventArgs teleRangEventArgs = new TeleRangEventArgs();
                teleRangEventArgs.position = Position;
                BoomerangReleased?.Invoke(this, teleRangEventArgs);
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(_texture2D, Position, Color.White);
        }
    }
}