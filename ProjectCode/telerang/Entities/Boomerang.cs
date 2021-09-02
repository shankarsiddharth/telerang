﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.VectorDraw;

using System;
using System.Collections.Generic;
using telerang.Entities;
using telerang.Shapes;

namespace telerang
{
    internal class Boomerang : IGameEntity
    {
        public event EventHandler<TeleRangEventArgs> BoomerangReleased;

        public Vector2 Position { get; set; }
        public float MaxTime { get; set; }
        public int TileWidth;
        public int TileHeight;
        public float Speed;

        private Vector2 _startPosition;
        private Texture2D _texture2D;
        private Texture2D _cursor;
        private Ninja _ninja;
        private float _maximumDistance;
        private float _timer;
        private Vector2 _cursorPosition;
        private TiledMap _tiledMap;
        private TiledMapTileLayer _tiledMapPlatformLayer;
        private TiledMapTile? _tile = null;


        private Vector2[] _pathToTravel;
        private int _currentPathIndex = 0;
        private float _distanceTreshold = 0.01f;

        public Boomerang(Texture2D spriteSheet, Texture2D cursor, Vector2 position, Ninja ninja, float maximumDistance, TiledMap tiledMap)
        {
            Position = position;
            _startPosition = position;
            _texture2D = spriteSheet;
            _cursor = cursor;
            _ninja = ninja;
            _maximumDistance = maximumDistance;
            _tiledMap = tiledMap;

            _tiledMapPlatformLayer = _tiledMap.GetLayer<TiledMapTileLayer>("Platforms");
        }

        public int DrawOrder { get; set; }

        public void Update(GameTime gameTime)
        {
            double deltaTime = gameTime.ElapsedGameTime.TotalMilliseconds;
            MouseState mouseState = Mouse.GetState();
            Vector2 ninjaPosition = _ninja.Position;
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);

            switch (_ninja.State)
            {
                case NinjaState.Idle:
                    {
                        if (mouseState.LeftButton == ButtonState.Pressed)
                        {
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

                        if (difference.Length() >= _maximumDistance)
                        {
                            difference.Normalize();
                            mousePosition = Vector2.Add(ninjaPosition, difference * _maximumDistance);
                        }

                        _cursorPosition = mousePosition;

                        if (mouseState.LeftButton == ButtonState.Released)
                        {
                            _ninja.targetPosition = mousePosition;
                            _startPosition = _ninja.Position;
                            _timer = 0f;

                            float distance = Vector2.Distance(_ninja.Position, _cursorPosition);
                            Vector2 midpoint = (_ninja.Position + _cursorPosition) / 2.0f;
                            _pathToTravel = CreateCircle(midpoint, distance / 2.0f);
                            _currentPathIndex = 0;
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
                        /*if (_timer <= MaxTime)
                        {
                            Position = Vector2.LerpPrecise(_startPosition, _ninja.targetPosition, _timer / MaxTime);
                            if (mouseState.LeftButton == ButtonState.Pressed
                                && _timer > 2f * (float)gameTime.ElapsedGameTime.TotalMilliseconds)
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
                        else
                        {
                            Position = Vector2.LerpPrecise(_startPosition, _ninja.targetPosition, (2f - _timer / MaxTime));
                            if (mouseState.LeftButton == ButtonState.Pressed)
                            {
                                _ninja.ChangeState(NinjaState.Teleported);
                            }
                        }
                        _timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;*/

                    }
                    {
                        Vector2 currentPosition = _pathToTravel[_currentPathIndex];
                        Vector2 nextPositionInPath = _pathToTravel[(_currentPathIndex + 1) % _pathToTravel.Length];
                        Vector2 differceInPosition = ( nextPositionInPath - currentPosition);
                        differceInPosition.Normalize();
                        Position = differceInPosition * (float)deltaTime * Speed;
                        if (Vector2.DistanceSquared(Position, nextPositionInPath) < _distanceTreshold)
                        {
                            _currentPathIndex += 1;
                        }
                    }
                    break;

                case NinjaState.Teleported:
                    {
                        _ninja.Position = Position;
                        if (mouseState.LeftButton == ButtonState.Released)
                        {
                            ushort x = (ushort)(Position.X / TileWidth);
                            ushort y = (ushort)(Position.Y / TileHeight);
                            if (IsAbyss(x, y))
                            {
                                Console.WriteLine("Dead");
                                _ninja.ChangeState(NinjaState.Idle);
                            }
                            else
                            {
                                Console.WriteLine("Still Alive");
                                _ninja.ChangeState(NinjaState.Idle);
                            }
                        }
                    }
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (_ninja.State)
            {
                case NinjaState.Idle:
                    {
                        spriteBatch.Draw(_cursor, Mouse.GetState().Position.ToVector2(), Color.White);
                    }
                    break;

                case NinjaState.Aiming:
                    {
                        spriteBatch.Draw(_cursor, _cursorPosition, Color.White);
                    }
                    break;

                case NinjaState.Teleporting:
                    {
                        spriteBatch.Draw(_cursor, Mouse.GetState().Position.ToVector2(), Color.White);
                        spriteBatch.Draw(_texture2D, Position, Color.White);
                        Primitives2D.DrawPoints(spriteBatch, _pathToTravel, Color.Black, 1.0f);
                    }
                    break;

                case NinjaState.Teleported:
                    {
                        spriteBatch.Draw(_cursor, Mouse.GetState().Position.ToVector2(), Color.White);
                    }
                    break;
            }
        }

        public void DrawPrimitives(PrimitiveDrawing primitiveDrawing, GameTime gameTime)
        {
            switch (_ninja.State)
            {
                case NinjaState.Idle:
                    {
                    }
                    break;

                case NinjaState.Aiming:
                    {
                        float distance = Vector2.Distance(_ninja.Position, _cursorPosition);   
                        Vector2 midpoint = (_ninja.Position + _cursorPosition ) / 2.0f;
                        primitiveDrawing.DrawCircle(midpoint, distance/2.0f, Color.Orange);                        
                    }
                    break;

                case NinjaState.Teleporting:
                    {
                    }
                    break;

                case NinjaState.Teleported:
                    {
                    }
                    break;
            }
        }

        private bool IsAbyss(ushort x, ushort y)
        {
            _tiledMapPlatformLayer.TryGetTile(x, y, out _tile);
            if (_tile.HasValue)
            {
                TiledMapTile tile = (TiledMapTile)(_tile);
                if (tile.IsBlank)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsObject(ushort x, ushort y)
        {
            //TiledMapObjectLayer _tiledMapObjectLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("Objects"); ;
            //TiledMapObject? _tile = null;
            //_tiledMapObjectLayer.Objects[0].Size

            _tiledMapPlatformLayer.TryGetTile(x, y, out _tile);
            if (_tile.HasValue)
            {
                TiledMapTile tile = (TiledMapTile)(_tile);
                if (tile.IsBlank)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static Vector2[] CreateEllipse(float rx, float ry, int sides)
        {
            var vertices = new Vector2[sides];

            var t = 0.0;
            var dt = 2.0 * Math.PI / sides;
            for (var i = 0; i < sides; i++, t += dt)
            {
                var x = (float)(rx * Math.Cos(t));
                var y = (float)(ry * Math.Sin(t));
                vertices[i] = new Vector2(x, y);
            }
            return vertices;
        }

        private float GetInverseProportionOfY(float y) 
        {
            float PROPORTION_CONSTANT = 5.0f;
            return y / PROPORTION_CONSTANT;
        }


        public static Vector2[] CreateCircle(Vector2 center, float radius)
        {

            //Primitives2D.Cre

            const int CircleSegments = 32;
            const double increment = Math.PI * 2.0 / CircleSegments;
            double theta = 0.0;

            Vector2[] VectorList = new Vector2[CircleSegments * 2];

            for (int i = 0; i < CircleSegments; i++)
            {
                Vector2 v1 = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                Vector2 v2 = center + radius * new Vector2((float)Math.Cos(theta + increment), (float)Math.Sin(theta + increment));

                VectorList[(i * 2)] = (v1);
                VectorList[((i *2) + 1)] = (v2);

                theta += increment;
            }

            return VectorList;
        }
    }
}